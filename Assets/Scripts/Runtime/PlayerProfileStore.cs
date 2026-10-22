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
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        [Serializable] private sealed class Payload { public string wallet; public DishEntry[] dishes; public RankEntry[] upgrades; }
        [Serializable] private sealed class DishEntry { public int id; public int xp; }
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
                dishes = snapshot.DishXpById.OrderBy(p => p.Key).Select(p => new DishEntry { id = p.Key, xp = p.Value }).ToArray(),
                upgrades = snapshot.UpgradeRanksById.OrderBy(p => p.Key, StringComparer.Ordinal).Select(p => new RankEntry { id = p.Key, rank = p.Value }).ToArray()
            });
            return JsonUtility.ToJson(new Envelope { version = 1, payload = payload, checksum = Hash(payload) }, true);
        }
        private static PlayerProfile Decode(string json, out ProfileLoadStatus status)
        {
            status = ProfileLoadStatus.Invalid;
            try
            {
                if (json.Length > 1024 * 1024) return null;
                var envelope = JsonUtility.FromJson<Envelope>(json);
                if (envelope == null) return null;
                if (envelope.version > 1) { status = ProfileLoadStatus.Unsupported; return null; }
                if (envelope.version != 1 || envelope.payload == null || envelope.checksum != Hash(envelope.payload)) return null;
                var payload = JsonUtility.FromJson<Payload>(envelope.payload);
                if (payload == null || payload.dishes == null || payload.upgrades == null ||
                    !decimal.TryParse(payload.wallet, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal wallet) ||
                    wallet < 0 || wallet > decimal.MaxValue / 2 || decimal.Round(wallet, 2) != wallet ||
                    wallet.ToString(CultureInfo.InvariantCulture) != payload.wallet) return null;
                int cap = Enumerable.Range(1, PrototypeDishRewards.MaximumDishLevel - 1).Sum(PrototypeDishRewards.RequiredXp);
                var xp = new Dictionary<int, int>();
                foreach (var dish in payload.dishes)
                {
                    if (dish == null || dish.id < 1 || dish.id > 32 || dish.xp < 0 || dish.xp > cap || !xp.TryAdd(dish.id, dish.xp)) return null;
                }
                var ranks = new Dictionary<string, int>(StringComparer.Ordinal);
                foreach (var rank in payload.upgrades)
                {
                    if (rank == null || rank.id != UpgradeCatalog.ClickStrengthId || rank.rank < 0 || rank.rank > 15 || !ranks.TryAdd(rank.id, rank.rank)) return null;
                }
                status = ProfileLoadStatus.Loaded;
                return PlayerProfile.Restore(wallet, xp, ranks);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is FormatException || ex is OverflowException) { return null; }
        }
    }
}
