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
    /// <summary>U4c: damage isolation, both auto sources, schema, native-purchase checkpoints.</summary>
    public static class AutoStrengthVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U4c: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create(); return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int strength = 0, int count = 1) => new[] { Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1), Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, 10), Rank(UpgradeNodeRules.DoubleStrikePower, 1), Rank(UpgradeNodeRules.DoubleStrikeChanceII, 1), Rank(UpgradeNodeRules.TripleStrikeChance, 1), Rank(UpgradeNodeRules.AutoActionCount, count), Rank(UpgradeNodeRules.AutoActionChance, 1), Rank(UpgradeNodeRules.AutoStrength, strength) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile p, bool manual = false)
        { var s = new PrototypeOrderSession(p, manual ? () => 0 : () => throw new Exception("auto used crit RNG"), manual ? () => .22 : () => throw new Exception("auto used manual proc RNG"), () => 0); Check(s.ConfigureUpgrades(Catalog), "configure"); return s; }
        private static PrototypeOrderSession Session(int strength, int count = 1, bool manual = false) => Session(Decode(Signed(9, "50000000", Ranks(strength, count))), manual);
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
                SetDish(dish, 10000); SetPool(location, dish); var locked = Session(0, 0); Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.AutoStrength, out _), "count prerequisite"); var poor = Session(Decode(Signed(9, "499.99", Ranks()))); poor.TryQuoteUpgrade(UpgradeNodeRules.AutoStrength, out var costly); Check(!poor.TryPurchaseUpgrade(costly) && poor.Rewards.Wallet == 499.99m, "insufficient wallet");
                var buyer = Session(0); Check(buyer.AutoDamagePerAction == 1, "legacy base damage"); decimal price = 500, spent = 0;
                for (int n = 0; n < 15; n++)
                {
                    Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoStrength, out var q) && q.Price == price, "known power-of-two price " + n); buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false; buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "day purchase"); buyer.EndDay(); Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += price; price *= 2;
                    var order = Begin(buyer, location); buyer.TryCookAutoStart(order); int damage = n + 2; Check(order.RemainingClicks == 10000 - 2 * damage && buyer.LastAutoActionCount == 2 && buyer.LastHitDamage == 2 * damage && !buyer.LastHitWasCritical && buyer.LastStrikeCount == 0 && buyer.AutoStrengthRank == n + 1 && buyer.AutoDamagePerAction == damage, "start uses real damage " + n); buyer.TryCookAuto(order, 0); buyer.TryCookAuto(order, .999999); Check(order.RemainingClicks == 10000 - 2 * damage, "no early periodic tick"); buyer.TryCookAuto(order, 1); Check(order.RemainingClicks == 10000 - 4 * damage && buyer.ClickDamage == 6 && buyer.AutoActionsPerTick == 2 && buyer.AutoActionChance == .02 && buyer.DoubleStrikeChance == .22 && buyer.TripleStrikeChance == .01, "periodic damage / independent stats"); buyer.EndDay();
                }
                Check(spent == 16383500m && buyer.Rewards.Wallet == 50000000m - spent && buyer.AutoStrengthRank == 15 && buyer.AutoDamagePerAction == 16 && !buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoStrength, out _), "cap/total"); foreach (var id in new[] { "node.auto_strength.02", "node.auto_strength_multiplier.02", "node.auto_frequency.02" }) Check(UpgradeBatchVerification.AvailabilityMatches(buyer,id), "later tier availability contract");
                var tree = UpgradeTreeDefinition.Load(); Check(tree.PrerequisitesMet(tree.Find("node.auto_strength.02"), buyer.Profile.GetUpgradeRank), "next tier sees first node's own purchased max");
                SetDish(dish, 1000); var mixed = Session(2, 1, true); var active = Begin(mixed, location); mixed.TryCookAutoStart(active); Check(active.RemainingClicks == 994, "strength 3 x two actions"); mixed.TryCookClick(active); Check(active.RemainingClicks == 967 && mixed.LastHitDamage == 27 && mixed.LastStrikeCount == 3 && !mixed.LastActionWasAuto, "manual damage unchanged"); mixed.TryCookAuto(active, 0); mixed.TryCookAuto(active, 1); Check(active.RemainingClicks == 961 && mixed.LastHitDamage == 6 && mixed.LastActionWasAuto, "periodic same strength"); mixed.ReturnToBoard(); mixed.TrySelect(0); Check(!mixed.TryCookAutoStart(mixed.ActiveOrder) && mixed.ActiveOrder.RemainingClicks == 1000, "strength does not reopen start visit"); mixed.EndDay();
                foreach (int hp in new[] { 1, 3, 4, 6, 7 })
                {
                    SetDish(dish, hp); var s = Session(2); var order = Begin(s, location); int changes = 0; s.Profile.Changed += () => changes++; decimal before = s.Rewards.Wallet; bool done = s.TryCookAutoStart(order); if (hp == 7) { Check(!done && order.RemainingClicks == 1 && changes == 0, "partial start"); s.TryCookAuto(order, 0); done = s.TryCookAuto(order, 1); }
                    int actions = hp <= 3 || hp == 7 ? 1 : 2; Check(done && order.RemainingClicks == 0 && s.LastAutoActionCount == actions && s.LastHitDamage == actions * 3 && s.CompletedDishes == 1 && changes == 1 && !s.LastHitWasCritical && s.Rewards.Wallet == before + s.LastResult.Payout && s.Profile.CaptureSnapshot().DishXpById[1] == 50 + s.LastResult.XpGained, "overshoot bounded / one receipt " + hp); decimal paid = s.Rewards.Wallet; Check(!s.TryCookAutoStart(order) && !s.TryCookAuto(order, 100) && !s.TryCookClick(order) && s.Rewards.Wallet == paid && changes == 1, "stale result"); s.TrySelect(0); Check(s.ActiveOrder.RemainingClicks == hp && !s.ActiveOrder.AutoStartChecked, "no damage carry"); s.EndDay();
                }
                SetDish(dish, 177); var maximum = Session(15, 10); active = Begin(maximum, location); maximum.TryCookAutoStart(active); Check(active.RemainingClicks == 1 && maximum.LastAutoActionCount == 11 && maximum.LastHitDamage == 176, "max count/strength batch"); maximum.TryCookAuto(active, 0); Check(maximum.TryCookAuto(active, 1) && maximum.LastAutoActionCount == 1 && maximum.LastHitDamage == 16 && maximum.CompletedDishes == 1, "max batch stops after first lethal action"); maximum.EndDay();
                return "U4c model PASS: 15 prices/cap/guards, same start/periodic damage, independent manual/chance/count/timer, overshoot/one receipt/no carry and max batch.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U4cStore/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 8; version++)
            {
                int length = version == 8 ? 11 : version == 7 ? 10 : version == 6 ? 9 : version == 5 ? 7 : version + 1; string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks().Take(length).ToArray(); string old = Signed(version, "2500.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old); var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot(); Check(p.GetUpgradeRank(UpgradeNodeRules.AutoStrength) == 0 && Session(p).AutoDamagePerAction == 1 && File.ReadAllText(path) == old, "legacy damage/no rewrite"); Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "schema 9/backup"); var after = new PlayerProfileStore(path).Load().CaptureSnapshot(); Check(after.Wallet == 2500.25m && after.DishXpById[1] == 50 && before.UpgradeRanksById.Count == after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value), "exact old state"); File.WriteAllText(path, "broken"); var recovery = new PlayerProfileStore(path); Check(recovery.Load() != null && recovery.Status == ProfileLoadStatus.Recovered, "old backup"); Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(UpgradeNodeRules.AutoStrength, 0) }).ToArray())) == null, "old schema rejects even rank 0");
            }
            foreach (int rank in new[] { 1, 15 }) { string path = Path.Combine(folder, "r" + rank + ".json"); Check(new PlayerProfileStore(path).Save(Decode(Signed(9, "1000.25", Ranks(rank)))), "write strength"); var s = Session(new PlayerProfileStore(path).Load()); Check(s.AutoStrengthRank == rank && s.AutoDamagePerAction == 1 + rank && s.AutoActionCountRank == 1 && s.AutoActionChanceRank == 1 && s.Rewards.Wallet == 1000.25m && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto, "independent roundtrip/idle"); }
            foreach (var ranks in new[] { Ranks(-1), Ranks(16), Ranks(1, 0), Ranks(1).Concat(new[] { Rank(UpgradeNodeRules.AutoStrength, 1) }).ToArray(), Ranks().Concat(new[] { Rank("node.auto_strength.02", 1) }).ToArray(), Ranks().Concat(new[] { Rank("auto_strength", 1) }).ToArray() }) Check(Decode(Signed(9, "1000", ranks)) == null, "invalid cap/prerequisite/duplicate/tier/alias"); string future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()), futurePath = Path.Combine(folder, "future.json"); File.WriteAllText(futurePath, future); var blocked = new PlayerProfileStore(futurePath); Check(blocked.Load() == null && blocked.Status == ProfileLoadStatus.Unsupported && !blocked.Save(Decode(Signed(9, "1000", Ranks()))) && File.ReadAllText(futurePath) == future, "future protected"); return "U4c storage PASS: schemas 1–8 → 9, exact legacy state/backups, rank bounds/prerequisites, idle roundtrip and version 10 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish; private static LocationData playLocation;
        public static string PreparePlay()
        { Check(!EditorApplication.isPlaying, "prepare stopped"); string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U4c-" + Guid.NewGuid().ToString("N"), "profile.json")); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(8, "2500.25", Ranks().Take(11).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path; }
        public static string ShowNativePurchase()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4c-"), "isolated profile"); var c = Controller; c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoStrength); Field<UpgradeTreeView>(c, "upgradeTreeView").FocusNode(UpgradeNodeRules.AutoStrength); var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); Check(detail.text.Contains("0 / 15") && detail.text.Contains("1 HP") && detail.text.Contains("2 HP") && detail.text.Contains(ClickCookUiCopy.Money(500)) && Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "ready for native purchase"); return "U4c ready: buy via native mouse, then verify rank 1.";
        }
        public static string CheckNativePurchase(int rank)
        {
            var c = Controller; decimal wallet = rank == 1 ? 2000.25m : 1000.25m; var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(c.OrderSession.AutoStrengthRank == rank && c.OrderSession.AutoDamagePerAction == rank + 1 && c.OrderSession.Rewards.Wallet == wallet && disk.GetUpgradeRank(UpgradeNodeRules.AutoStrength) == rank && disk.Rewards.Wallet == wallet && c.OrderSession.AutoActionCountRank == 1 && c.OrderSession.AutoActionChanceRank == 1, "native single debit/effect/autosave " + rank); return "U4c native purchase rank " + rank + " PASS.";
        }
        private static void BeginFixture(int hp)
        {
            var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.CloseUpgrades(); var s = Session(c.OrderSession.Profile); typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation(); playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation); SetDish(playDish, hp); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); c.SelectOrder(0);
        }
        private static void EndFixture()
        { var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.ReturnToProfileMenu(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); }
        public static string StartCooking()
        {
            CheckNativePurchase(2); var c = Controller; c.GetComponent<ProfileMenu>().Resume(); var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); Check(detail.text.Contains("2 / 15") && detail.text.Contains("3 HP") && detail.text.Contains("4 HP") && detail.text.Contains(ClickCookUiCopy.Money(2000)) && !Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "refreshed rank/price/funds preview"); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionChance); Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains("2 × 3 HP"), "chance detail has current strength"); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionCount); Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains("3 HP"), "count detail has current strength");
            var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases(); Check(stats.text.Contains("Авто: 2 дій × 3 HP") && stats.preferredHeight <= stats.rectTransform.rect.height + 1 && stats.preferredWidth <= stats.rectTransform.rect.width + 1, "actual stats fit"); BeginFixture(1000); Check(c.OrderSession.ActiveOrder.RemainingClicks == 994 && c.OrderSession.LastAutoActionCount == 2 && c.OrderSession.LastHitDamage == 6 && !c.OrderSession.LastHitWasCritical, "controller immediate strengthened start"); return "U4c start batch PASS; observe a real periodic tick.";
        }
        public static string CheckTickAndComplete()
        {
            var c = Controller; Check(c.OrderSession.ActiveOrder.RemainingClicks < 994 && c.OrderSession.LastHitDamage == 6 && c.OrderSession.LastAutoActionCount == 2, "controller strengthened periodic tick"); EndFixture(); decimal before = c.OrderSession.Rewards.Wallet; BeginFixture(4); var s = c.OrderSession; Check(s.ActiveOrder == null && s.CompletedDishes == 1 && s.LastAutoActionCount == 2 && s.LastHitDamage == 6 && !s.LastHitWasCritical && Field<GameObject>(c, "dishResultScreen").activeSelf && s.Rewards.Wallet == before + s.LastResult.Payout, "strengthened overshoot/result/one receipt"); decimal paid = s.Rewards.Wallet; c.ClickCookingDish(); Check(s.CompletedDishes == 1 && s.Rewards.Wallet == paid, "stale result guards"); var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(disk.GetUpgradeRank(UpgradeNodeRules.AutoStrength) == 2 && disk.Rewards.Wallet == paid, "completion autosave"); SessionState.SetString("ClickCook.U4cWallet", paid.ToString(System.Globalization.CultureInfo.InvariantCulture)); SessionState.SetInt("ClickCook.U4cXp", s.Profile.CaptureSnapshot().DishXpById[1]); EndFixture(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoStrength); Field<UpgradeTreeView>(c, "upgradeTreeView").FocusNode(UpgradeNodeRules.AutoStrength); return "U4c real periodic/start/result/autosave PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4c-"), "isolated restart"); var c = Controller; var s = c.OrderSession; Check(s.Profile.CaptureSnapshot().UpgradeRanksById.Count == 12 && s.AutoStrengthRank == 2 && s.AutoDamagePerAction == 3 && s.AutoActionChanceRank == 1 && s.AutoActionCountRank == 1 && s.DoubleStrikeChanceRank == 10 && s.DoubleStrikeChanceIIRank == 1 && s.TripleStrikeChanceRank == 1 && s.ClickStrengthRank == 3 && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto && s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture) == SessionState.GetString("ClickCook.U4cWallet", "") && s.Profile.CaptureSnapshot().DishXpById[1] == SessionState.GetInt("ClickCook.U4cXp", -1), "all ranks/exact wallet/XP/idle"); c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoStrength); return "U4c restart/Continue PASS.";
        }
        public static string Cleanup()
        { Check(!EditorApplication.isPlaying, "cleanup stopped"); SessionState.EraseString("ClickCook.U4cWallet"); SessionState.EraseInt("ClickCook.U4cXp"); return PlayerProfileStoreVerification.FinishPlayVerification(); }
    }
}
