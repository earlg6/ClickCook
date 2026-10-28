using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    /// <summary>U4d: independent price/rounding oracles, auto boundaries and profile migration.</summary>
    public static class AutoMultiplierVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U4d: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create();
            return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int multiplier = 0, int strength = 2, int count = 1) => new[] {
            Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1),
            Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, 10),
            Rank(UpgradeNodeRules.DoubleStrikePower, 1), Rank(UpgradeNodeRules.DoubleStrikeChanceII, 1), Rank(UpgradeNodeRules.TripleStrikeChance, 1),
            Rank(UpgradeNodeRules.AutoActionCount, count), Rank(UpgradeNodeRules.AutoActionChance, 1), Rank(UpgradeNodeRules.AutoStrength, strength),
            Rank(UpgradeNodeRules.AutoStrengthMultiplier, multiplier) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile p, bool manual = false)
        {
            var s = new PrototypeOrderSession(p, manual ? () => 0 : () => throw new Exception("auto used crit RNG"),
                manual ? () => .22 : () => throw new Exception("auto used manual proc RNG"), () => 0);
            Check(s.ConfigureUpgrades(Catalog), "configure"); return s;
        }
        private static PrototypeOrderSession Session(int multiplier, int strength = 2, int count = 1, bool manual = false) => Session(Decode(Signed(10, "1000000.25", Ranks(multiplier, strength, count))), manual);
        private static void SetDish(DishData dish, int hp)
        { var d = new SerializedObject(dish); d.FindProperty("dishId").intValue = 1; d.FindProperty("clickRequirement").intValue = hp; d.FindProperty("saleValue").intValue = 5; d.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetPool(LocationData location, DishData dish)
        { var d = new SerializedObject(location); var pool = d.FindProperty("documentedAvailableDishes"); pool.arraySize = 1; pool.GetArrayElementAtIndex(0).objectReferenceValue = dish; d.ApplyModifiedPropertiesWithoutUndo(); }
        private static PrototypeOrder Begin(PrototypeOrderSession s, LocationData location) { s.StartDay(location, 2); Check(s.TrySelect(0), "select"); return s.ActiveOrder; }

        public static string RunModelChecks()
        {
            var random = UnityEngine.Random.state; var dish = ScriptableObject.CreateInstance<DishData>(); var location = ScriptableObject.CreateInstance<LocationData>();
            try
            {
                SetDish(dish, 10000); SetPool(location, dish);
                var locked = Session(0, 0); Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.AutoStrengthMultiplier, out _), "strength prerequisite");
                var poor = Session(Decode(Signed(10, "999.99", Ranks()))); Check(poor.TryQuoteUpgrade(UpgradeNodeRules.AutoStrengthMultiplier, out var costly) && !poor.TryPurchaseUpgrade(costly) && poor.Rewards.Wallet == 999.99m, "insufficient funds");
                // Explicit prices guard decimal exponentiation/ceil, independently of catalog logic.
                decimal[] prices = { 1000, 1800, 3240, 5832, 10498, 18896, 34013, 61223, 110200, 198360 };
                int[] damages = { 4, 4, 4, 5, 5, 5, 6, 6, 6, 6 };
                var buyer = Session(0); decimal spent = 0;
                Check(buyer.AutoDamagePerAction == 3 && buyer.AutoStrengthMultiplier == 1m, "rank zero identity");
                for (int n = 0; n < prices.Length; n++)
                {
                    Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoStrengthMultiplier, out var q) && q.Price == prices[n], "known price " + n);
                    buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false;
                    buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "day purchase"); buyer.EndDay();
                    Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += prices[n];
                    var order = Begin(buyer, location); buyer.TryCookAutoStart(order);
                    Check(order.RemainingClicks == 10000 - 2 * damages[n] && buyer.LastHitDamage == 2 * damages[n] && buyer.LastAutoActionCount == 2 && !buyer.LastHitWasCritical && buyer.LastStrikeCount == 0, "start damage " + n);
                    buyer.TryCookAuto(order, 0); buyer.TryCookAuto(order, .999999); Check(order.RemainingClicks == 10000 - 2 * damages[n], "full unchanged interval");
                    buyer.TryCookAuto(order, 1); Check(order.RemainingClicks == 10000 - 4 * damages[n] && buyer.AutoDamagePerAction == damages[n] && buyer.AutoStrengthRank == 2 && buyer.AutoStrengthMultiplierRank == n + 1 && buyer.AutoActionChance == .02 && buyer.AutoActionsPerTick == 2 && buyer.ClickDamage == 6, "periodic / independent stats"); buyer.EndDay();
                }
                Check(spent == 445062 && buyer.Rewards.Wallet == 1000000.25m - spent && !buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoStrengthMultiplier, out _), "cap/total");
                foreach (var id in new[] { "node.auto_strength_multiplier.02", "node.auto_strength.02", "node.auto_frequency.02" }) Check(UpgradeBatchVerification.AvailabilityMatches(buyer,id), "later tier availability contract");
                var tree = UpgradeTreeDefinition.Load(); Check(tree.PrerequisitesMet(tree.Find("node.auto_strength_multiplier.02"), buyer.Profile.GetUpgradeRank), "second tier sees purchased cap");
                // Integer rational oracle: ceil(base * (10 + rank) / 10), including exact boundaries.
                for (int strength = 1; strength <= 15; strength++) for (int rank = 0; rank <= 10; rank++)
                { var s = Session(rank, strength); Check(s.AutoDamagePerAction == ((strength + 1) * (10 + rank) + 9) / 10, "rounding oracle"); }
                SetDish(dish, 1000); var mixed = Session(1, manual: true); var active = Begin(mixed, location); mixed.TryCookAutoStart(active);
                Check(active.RemainingClicks == 992, "fractional per-action ceil"); mixed.TryCookClick(active);
                Check(active.RemainingClicks == 965 && mixed.LastHitDamage == 27 && mixed.LastStrikeCount == 3 && !mixed.LastActionWasAuto, "manual triple/critical unchanged");
                mixed.TryCookAuto(active, 0); mixed.TryCookAuto(active, 1); Check(active.RemainingClicks == 957 && mixed.LastHitDamage == 8, "mixed periodic");
                mixed.ReturnToBoard(); mixed.TrySelect(0); Check(!mixed.TryCookAutoStart(mixed.ActiveOrder) && mixed.ActiveOrder.RemainingClicks == 1000, "Back cannot reroll"); mixed.EndDay();
                foreach (int hp in new[] { 1, 4, 5, 8, 9 })
                {
                    SetDish(dish, hp); var s = Session(1); var order = Begin(s, location); int changes = 0; s.Profile.Changed += () => changes++; decimal before = s.Rewards.Wallet;
                    bool done = s.TryCookAutoStart(order); if (hp == 9) { Check(!done && order.RemainingClicks == 1 && changes == 0, "partial start"); s.TryCookAuto(order, 0); done = s.TryCookAuto(order, 1); }
                    int actions = hp <= 4 || hp == 9 ? 1 : 2;
                    Check(done && order.RemainingClicks == 0 && s.LastAutoActionCount == actions && s.LastHitDamage == actions * 4 && s.CompletedDishes == 1 && changes == 1 && s.Rewards.Wallet == before + s.LastResult.Payout && s.Profile.CaptureSnapshot().DishXpById[1] == 50 + s.LastResult.XpGained, "bounded completion " + hp);
                    decimal paid = s.Rewards.Wallet; Check(!s.TryCookAutoStart(order) && !s.TryCookAuto(order, 100) && !s.TryCookClick(order) && s.Rewards.Wallet == paid && changes == 1, "stale result");
                    s.TrySelect(0); Check(s.ActiveOrder.RemainingClicks == hp && !s.ActiveOrder.AutoStartChecked, "no carry"); s.EndDay();
                }
                SetDish(dish, 353); var maximum = Session(10, 15, 10); active = Begin(maximum, location); maximum.TryCookAutoStart(active);
                Check(active.RemainingClicks == 1 && maximum.LastHitDamage == 352 && maximum.LastAutoActionCount == 11, "max batch"); maximum.TryCookAuto(active, 0);
                Check(maximum.TryCookAuto(active, 1) && maximum.LastAutoActionCount == 1 && maximum.LastHitDamage == 32 && maximum.CompletedDishes == 1, "max lethal batch stops"); maximum.EndDay();
                return "U4d model PASS: prices/cap/guards, rational rounding oracle, start/periodic/manual isolation, bounded completion/no carry, maximum 352 HP batch.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U4dStore/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 9; version++)
            {
                int length = version == 9 ? 12 : version == 8 ? 11 : version == 7 ? 10 : version == 6 ? 9 : version == 5 ? 7 : version + 1;
                string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks().Take(length).ToArray();
                string old = Signed(version, "2500.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old);
                var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot();
                Check(p.GetUpgradeRank(UpgradeNodeRules.AutoStrengthMultiplier) == 0 && Session(p).AutoStrengthMultiplier == 1m && File.ReadAllText(path) == old, "old factor/no rewrite");
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "schema/backup");
                var after = new PlayerProfileStore(path).Load().CaptureSnapshot();
                Check(after.Wallet == 2500.25m && after.DishXpById[1] == 50 && before.UpgradeRanksById.Count == after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value), "exact old state");
                File.WriteAllText(path, "broken"); var recovery = new PlayerProfileStore(path); Check(recovery.Load() != null && recovery.Status == ProfileLoadStatus.Recovered, "old backup recovery");
                Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(UpgradeNodeRules.AutoStrengthMultiplier, 0) }).ToArray())) == null, "old schema rejects new ID even zero");
            }
            foreach (int rank in new[] { 1, 10 })
            {
                string path = Path.Combine(folder, "r" + rank + ".json"); Check(new PlayerProfileStore(path).Save(Decode(Signed(10, "1000.25", Ranks(rank)))), "write multiplier");
                var s = Session(new PlayerProfileStore(path).Load()); Check(s.AutoStrengthMultiplierRank == rank && s.AutoDamagePerAction == (rank == 1 ? 4 : 6) && s.AutoStrengthRank == 2 && s.AutoActionCountRank == 1 && s.AutoActionChanceRank == 1 && s.Rewards.Wallet == 1000.25m && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto, "independent idle roundtrip");
            }
            foreach (var ranks in new[] { Ranks(-1), Ranks(11), Ranks(1, 0), Ranks(1).Concat(new[] { Rank(UpgradeNodeRules.AutoStrengthMultiplier, 1) }).ToArray(), Ranks().Concat(new[] { Rank("node.auto_strength_multiplier.02", 1) }).ToArray(), Ranks().Concat(new[] { Rank("auto_strength_multiplier", 1) }).ToArray() })
                Check(Decode(Signed(10, "1000", ranks)) == null, "invalid bounds/prerequisite/duplicate/tier/alias");
            string future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()), futurePath = Path.Combine(folder, "future.json"); File.WriteAllText(futurePath, future); var blocked = new PlayerProfileStore(futurePath);
            Check(blocked.Load() == null && blocked.Status == ProfileLoadStatus.Unsupported && !blocked.Save(Decode(Signed(10, "1000", Ranks()))) && File.ReadAllText(futurePath) == future, "future protected");
            return "U4d storage PASS: schemas 1–9 to 10, exact legacy state/backups, bounds/prerequisites/whitelist, independent idle roundtrip, version 11 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish; private static LocationData playLocation;
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying, "prepare stopped");
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U4d-" + Guid.NewGuid().ToString("N"), "profile.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(9, "4000.25", Ranks().Take(12).ToArray()));
            SessionState.SetString("ClickCook.SaveVerificationPath", path); return path;
        }
        public static string ShowNativePurchase()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4d-"), "isolated profile"); var c = Controller;
            c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoStrengthMultiplier);
            Field<UpgradeTreeView>(c, "upgradeTreeView").FocusNode(UpgradeNodeRules.AutoStrengthMultiplier);
            Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains("0 / 10") && Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "ready native purchase");
            return "U4d ready for native mouse purchase, rank 0, wallet 4000.25.";
        }
        public static string CheckNativePurchase(int rank)
        {
            var s = Controller.OrderSession; decimal wallet = rank == 1 ? 3000.25m : 1200.25m;
            var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();
            Check(s.AutoStrengthMultiplierRank == rank && s.AutoDamagePerAction == 4 && s.Rewards.Wallet == wallet && disk.GetUpgradeRank(UpgradeNodeRules.AutoStrengthMultiplier) == rank && disk.Rewards.Wallet == wallet && s.AutoStrengthRank == 2, "single debit/effect/autosave " + rank);
            return "U4d native purchase rank " + rank + " PASS.";
        }
        private static void BeginFixture(int hp)
        {
            var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.CloseUpgrades(); var s = Session(c.OrderSession.Profile);
            typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation();
            playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation);
            SetDish(playDish, hp); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); c.SelectOrder(0);
        }
        private static void EndFixture()
        { var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.ReturnToProfileMenu(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); }
        public static string StartCooking()
        {
            CheckNativePurchase(2); var c = Controller; c.GetComponent<ProfileMenu>().Resume();
            string detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text;
            Check(detail.Contains("2 / 10") && detail.Contains(1.2m.ToString("0.0")) && detail.Contains(1.3m.ToString("0.0")) && detail.Contains("4 HP") && detail.Contains(ClickCookUiCopy.Money(3240)) && !Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "rank/plateau/price/funds preview");
            c.SelectUpgradeNode(UpgradeNodeRules.AutoStrength); Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains("5 HP"), "strength next preview uses factor");
            c.SelectUpgradeNode(UpgradeNodeRules.AutoActionChance); Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains("2 × 4 HP"), "chance detail");
            c.SelectUpgradeNode(UpgradeNodeRules.AutoActionCount); Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains("4 HP"), "count detail");
            var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases();
            Check(stats.text.Contains("Авто: 2 дій × 4 HP") && stats.preferredHeight <= stats.rectTransform.rect.height + 1 && stats.preferredWidth <= stats.rectTransform.rect.width + 1, "actual stats fit");
            BeginFixture(1000); Check(c.OrderSession.ActiveOrder.RemainingClicks == 992 && c.OrderSession.LastHitDamage == 8, "controller immediate multiplied start");
            return "U4d start/UI PASS; observe a real periodic tick.";
        }
        public static string CheckTickAndComplete()
        {
            var c = Controller; Check(c.OrderSession.ActiveOrder.RemainingClicks < 992 && c.OrderSession.LastHitDamage == 8 && c.OrderSession.LastAutoActionCount == 2, "real controller tick");
            EndFixture(); decimal before = c.OrderSession.Rewards.Wallet; BeginFixture(5); var s = c.OrderSession;
            Check(s.ActiveOrder == null && s.CompletedDishes == 1 && s.LastAutoActionCount == 2 && s.LastHitDamage == 8 && Field<GameObject>(c, "dishResultScreen").activeSelf && s.Rewards.Wallet == before + s.LastResult.Payout, "overshoot/result/one receipt");
            decimal paid = s.Rewards.Wallet; c.ClickCookingDish(); Check(s.CompletedDishes == 1 && s.Rewards.Wallet == paid, "stale result");
            var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(disk.GetUpgradeRank(UpgradeNodeRules.AutoStrengthMultiplier) == 2 && disk.Rewards.Wallet == paid, "result autosave");
            SessionState.SetString("ClickCook.U4dWallet", paid.ToString(System.Globalization.CultureInfo.InvariantCulture)); SessionState.SetInt("ClickCook.U4dXp", s.Profile.CaptureSnapshot().DishXpById[1]);
            EndFixture(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoStrengthMultiplier); Field<UpgradeTreeView>(c, "upgradeTreeView").FocusNode(UpgradeNodeRules.AutoStrengthMultiplier);
            return "U4d controller tick/start/result/autosave PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4d-"), "isolated restart"); var c = Controller; var s = c.OrderSession;
            Check(s.Profile.CaptureSnapshot().UpgradeRanksById.Count == 13 && s.AutoStrengthMultiplierRank == 2 && s.AutoStrengthRank == 2 && s.AutoDamagePerAction == 4 && s.AutoActionChanceRank == 1 && s.AutoActionCountRank == 1 && s.DoubleStrikeChanceRank == 10 && s.DoubleStrikeChanceIIRank == 1 && s.TripleStrikeChanceRank == 1 && s.ClickStrengthRank == 3 && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto && s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture) == SessionState.GetString("ClickCook.U4dWallet", "") && s.Profile.CaptureSnapshot().DishXpById[1] == SessionState.GetInt("ClickCook.U4dXp", -1), "all ranks/exact wallet/XP/idle");
            c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoStrengthMultiplier);
            return "U4d restart/Continue PASS.";
        }
        public static string Cleanup()
        { Check(!EditorApplication.isPlaying, "cleanup stopped"); SessionState.EraseString("ClickCook.U4dWallet"); SessionState.EraseInt("ClickCook.U4dXp"); return PlayerProfileStoreVerification.FinishPlayVerification(); }
    }
}
