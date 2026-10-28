using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ClickCook.Data;
using UnityEngine;

namespace ClickCook.Runtime
{
    public enum ProfileLoadStatus { Missing, Loaded, Recovered, Invalid, Unsupported, Unavailable }

    /// <summary>Versioned local profile. Temporary day/order/result state is deliberately excluded.</summary>
    public sealed class PlayerProfileStore
    {
        public const int CurrentVersion = 17;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        [Serializable] private sealed class Payload { public string wallet; public DishEntry[] dishes; public RankEntry[] upgrades; public int[] recipes; }
        [Serializable] private sealed class DishEntry { public int id; public int xp; public int hundredths; }
        [Serializable] private sealed class RankEntry { public string id; public int rank; }
        public string SavePath { get; }
        public string BackupPath => SavePath + ".bak";
        public ProfileLoadStatus Status { get; private set; }
        public string LastError { get; private set; }
        private bool protectFiles;
        public PlayerProfileStore(string path) => SavePath = Path.GetFullPath(path);

        public PlayerProfile Load()
        {
            LastError = null;
            protectFiles = false;
            try
            {
                if (File.Exists(SavePath))
                {
                    var profile = Decode(File.ReadAllText(SavePath), out var status);
                    if (profile != null) { Status = ProfileLoadStatus.Loaded; return profile; }
                    // A newer application may own both files. Never roll it back or overwrite it silently.
                    if (status == ProfileLoadStatus.Unsupported) { Status = status; protectFiles = true; return null; }
                }
                if (File.Exists(BackupPath))
                {
                    var profile = Decode(File.ReadAllText(BackupPath), out var status);
                    if (profile != null) { Status = ProfileLoadStatus.Recovered; return profile; }
                    if (status == ProfileLoadStatus.Unsupported) { Status = status; protectFiles = true; return null; }
                }
                Status = File.Exists(SavePath) || File.Exists(BackupPath) ? ProfileLoadStatus.Invalid : ProfileLoadStatus.Missing;
                protectFiles = Status != ProfileLoadStatus.Missing;
            }
            catch (Exception ex) when (IsStorageError(ex))
            {
                LastError = ex.Message; Status = ProfileLoadStatus.Unavailable; protectFiles = true;
            }
            return null;
        }

        public bool Save(PlayerProfile profile, bool newGame = false)
        {
            LastError = null;
            if (protectFiles && !newGame) { LastError = "Збереження захищено. Потрібне явне підтвердження нової гри."; return false; }
            string temp = SavePath + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
                string json = Encode(profile.CaptureSnapshot());
                WriteFlushed(temp, json);
                bool validPrimary = false;
                if (File.Exists(SavePath))
                {
                    validPrimary = Decode(File.ReadAllText(SavePath), out var primaryStatus) != null;
                    if (!newGame && primaryStatus == ProfileLoadStatus.Unsupported)
                    {
                        protectFiles = true; LastError = "Файл належить новішій версії гри."; return false;
                    }
                }
                if (newGame)
                {
                    // Replace the backup first: a successful reset can never resurrect pre-reset progress.
                    string backupTemp = BackupPath + ".tmp";
                    WriteFlushed(backupTemp, json);
                    Install(backupTemp, BackupPath, null);
                }
                Install(temp, SavePath, !newGame && validPrimary ? BackupPath : null);
                if (!File.Exists(BackupPath))
                {
                    WriteFlushed(BackupPath + ".tmp", json);
                    Install(BackupPath + ".tmp", BackupPath, null);
                }
                protectFiles = false; Status = ProfileLoadStatus.Loaded;
                return true;
            }
            catch (Exception ex) when (IsStorageError(ex)) { LastError = ex.Message; return false; }
            // Stale .tmp files are never accepted as committed saves, and are overwritten on retry.
        }

        private static bool IsStorageError(Exception ex) => ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException;
        private static void Install(string temp, string target, string backup)
        {
            if (File.Exists(target)) File.Replace(temp, target, backup);
            else File.Move(temp, target);
        }
        private static void WriteFlushed(string path, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            using var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            stream.Write(bytes, 0, bytes.Length);
            stream.Flush(true);
        }
        private static string Hash(string text)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
        private static string Encode(PlayerProfileSnapshot snapshot)
        {
            string payload = JsonUtility.ToJson(new Payload {
                wallet = snapshot.Wallet.ToString(CultureInfo.InvariantCulture),
                recipes = snapshot.OwnedRecipeIds.OrderBy(id => id).ToArray(),
                dishes = snapshot.DishXpById.OrderBy(p => p.Key).Select(p => new DishEntry { id = p.Key, xp = p.Value, hundredths = snapshot.DishXpHundredthsById.TryGetValue(p.Key, out int value) ? value : 0 }).ToArray(),
                upgrades = snapshot.UpgradeRanksById.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new RankEntry { id = p.Key, rank = p.Value }).ToArray()
            });
            return JsonUtility.ToJson(new Envelope { version = CurrentVersion, payload = payload, checksum = Hash(payload) }, true);
        }
        private static PlayerProfile Decode(string json, out ProfileLoadStatus status)
        {
            status = ProfileLoadStatus.Invalid;
            try
            {
                if (json.Length > 1024 * 1024) return null;
                var envelope = JsonUtility.FromJson<Envelope>(json);
                if (envelope == null) return null;
                if (envelope.version > CurrentVersion) { status = ProfileLoadStatus.Unsupported; return null; }
                if (envelope.version < 1 || envelope.payload == null || envelope.checksum != Hash(envelope.payload)) return null;
                var payload = JsonUtility.FromJson<Payload>(envelope.payload);
                if (payload == null || payload.dishes == null || payload.upgrades == null ||
                    !decimal.TryParse(payload.wallet, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal wallet) ||
                    wallet < 0 || wallet > decimal.MaxValue / 2 || decimal.Round(wallet, 2) != wallet ||
                    wallet.ToString(CultureInfo.InvariantCulture) != payload.wallet) return null;
                int cap = Enumerable.Range(1, PrototypeDishRewards.MaximumDishLevel - 1).Sum(PrototypeDishRewards.RequiredXp);
                var xp = new Dictionary<int, int>();
                var hundredths = new Dictionary<int, int>();
                foreach (var dish in payload.dishes)
                {
                    if (dish == null || dish.id < 1 || dish.id > 32 || dish.xp < 0 || dish.xp > cap || !xp.TryAdd(dish.id, dish.xp) ||
                        dish.hundredths < 0 || dish.hundredths > 99 || (dish.xp == cap && dish.hundredths != 0) ||
                        (envelope.version < 13 && dish.hundredths != 0)) return null;
                    if (dish.hundredths != 0) hundredths.Add(dish.id, dish.hundredths);
                }
                var ranks = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var rank in payload.upgrades)
                {
                    if (rank == null) return null;
                    string id = rank.id;
                    if (envelope.version == 1)
                    {
                        if (id != UpgradeCatalog.ClickStrengthId) return null;
                        id = UpgradeNodeRules.Strength;
                    }
                    if (envelope.version == 2 && id == UpgradeNodeRules.Hold) return null;
                    if (envelope.version < 4 && id == UpgradeNodeRules.HoldFrequency) return null;
                    if (envelope.version < 5 && (id == UpgradeNodeRules.DoubleStrikeChance || id == UpgradeNodeRules.DoubleStrikePower)) return null;
                    if (envelope.version < 6 && (id == UpgradeNodeRules.DoubleStrikeChanceII || id == UpgradeNodeRules.TripleStrikeChance)) return null;
                    if (envelope.version < 7 && id == UpgradeNodeRules.AutoActionCount) return null;
                    if (envelope.version < 8 && id == UpgradeNodeRules.AutoActionChance) return null;
                    if (envelope.version < 9 && id == UpgradeNodeRules.AutoStrength) return null;
                    if (envelope.version < 10 && id == UpgradeNodeRules.AutoStrengthMultiplier) return null;
                    if (envelope.version < 11 && id == UpgradeNodeRules.AutoFrequency) return null;
                    if (envelope.version < 12 && id == UpgradeNodeRules.DishXpFlat) return null;
                    if (envelope.version < 13 && id == UpgradeNodeRules.DishXpMultiplier) return null;
                    if (envelope.version < 14 && id == UpgradeNodeRules.DishValue) return null;
                    if (envelope.version < UpgradeNodeRules.IntroducedVersion(id)) return null;
                    // Canonical implemented nodes only. Each schema retains its own content boundary.
                    if (id == null || id != UpgradeNodeRules.CanonicalId(id) || UpgradeNodeRules.MaximumRank(id) == 0 ||
                        rank.rank < 0 || rank.rank > UpgradeNodeRules.MaximumRank(id) || !ranks.TryAdd(id, rank.rank)) return null;
                }
                int Rank(string id) => ranks.TryGetValue(id, out int value) ? value : 0;
                foreach (var pair in ranks)
                    if (pair.Value > 0 && !UpgradeNodeRules.PrerequisitesMet(pair.Key, Rank)) return null;
                var recipes = new HashSet<int> { 1 };
                if (envelope.version >= 17)
                {
                    if (payload.recipes == null || payload.recipes.Length == 0 || payload.recipes.Length > 32) return null;
                    recipes.Clear();
                    foreach (int id in payload.recipes)
                        if (id < 1 || id > 32 || !recipes.Add(id)) return null;
                    if (!recipes.Contains(1)) return null;
                }
                else
                {
                    if (payload.recipes != null && payload.recipes.Length != 0) return null;
                    foreach (var pair in xp)
                        if (pair.Value > 0 || hundredths.ContainsKey(pair.Key)) recipes.Add(pair.Key);
                }
                var profile = PlayerProfile.Restore(wallet, xp, ranks, hundredths);
                if (xp.Any(pair => (pair.Value > 0 || hundredths.ContainsKey(pair.Key)) && !recipes.Contains(pair.Key))) return null;
                profile.RestoreRecipes(recipes);
                status = ProfileLoadStatus.Loaded;
                return profile;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is OverflowException) { return null; }
        }
    }
}
