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
    /// <summary>U4b: one first-entry roll per order, shared batch, schema and immediate result UI.</summary>
    public static class AutoStartVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U4b: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create(); return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int chance = 0, int count = 0) => new[] { Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1), Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, 10), Rank(UpgradeNodeRules.DoubleStrikePower, 1), Rank(UpgradeNodeRules.DoubleStrikeChanceII, 1), Rank(UpgradeNodeRules.TripleStrikeChance, 1), Rank(UpgradeNodeRules.AutoActionCount, count), Rank(UpgradeNodeRules.AutoActionChance, chance) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile p, Func<double> roll = null)
        { var s = new PrototypeOrderSession(p, () => throw new Exception("auto used crit RNG"), () => throw new Exception("auto used manual proc RNG"), roll ?? (() => throw new Exception("unexpected start RNG"))); Check(s.ConfigureUpgrades(Catalog), "configure"); return s; }
        private static PrototypeOrderSession Session(int chance, int count = 0, Func<double> roll = null) => Session(Decode(Signed(8, "10000000", Ranks(chance, count))), roll);
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
                SetDish(dish, 1000); SetPool(location, dish); var locked = Session(Decode(Signed(8, "1000", Rank(UpgradeNodeRules.Strength, 1)))); Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.AutoActionChance, out _), "strength prerequisite");
                var poor = Session(Decode(Signed(8, "249.99", Ranks()))); poor.TryQuoteUpgrade(UpgradeNodeRules.AutoActionChance, out var costly); Check(!poor.TryPurchaseUpgrade(costly) && poor.Rewards.Wallet == 249.99m, "insufficient wallet");
                var buyer = Session(0); decimal[] prices = { 250, 450, 810, 1458, 2625, 4724, 8504, 15306, 27550, 49590 }; decimal spent = 0;
                for (int n = 0; n < 10; n++)
                {
                    Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoActionChance, out var q) && q.Price == prices[n], "known price " + n); buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false; buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "day purchase"); buyer.EndDay(); Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += q.Price;
                }
                Check(buyer.AutoActionChance == .2 && buyer.AutoActionChanceRank == 10 && !buyer.HasAuto && buyer.AutoActionCountRank == 0 && buyer.Rewards.Wallet == 10000000m - spent && !buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoActionChance, out _) && !buyer.TryQuoteUpgrade("node.auto_action_chance.02", out _), "cap/debit/independent periodic unlock");
                var zero = Session(0); var active = Begin(zero, location); Check(!zero.TryCookAutoStart(active) && active.AutoStartChecked && !zero.LastActionWasAuto && active.RemainingClicks == 1000, "zero chance consumes visit without RNG"); zero.ReturnToBoard(); zero.TrySelect(0); Check(!zero.TryCookAutoStart(zero.ActiveOrder) && zero.ActiveOrder.AutoStartChecked, "zero chance Back"); zero.EndDay();
                foreach (var b in new[] { new { rank = 1, u = 0d, hit = true }, new { rank = 1, u = .019999999d, hit = true }, new { rank = 1, u = .02d, hit = false }, new { rank = 10, u = .199999999d, hit = true }, new { rank = 10, u = .2d, hit = false }, new { rank = 1, u = .999999999d, hit = false } })
                {
                    int rolls = 0; var s = Session(b.rank, 0, () => { rolls++; return b.u; }); var order = Begin(s, location); Check(!s.TryCookAutoStart(s.GetOrderAtSlot(1)) && rolls == 0 && !order.AutoStartChecked && !s.GetOrderAtSlot(1).AutoStartChecked, "foreign order does not consume visit"); s.IsPaused = true; Check(!s.TryCookAutoStart(order) && !order.AutoStartChecked && rolls == 0, "paused entry does not consume visit"); s.IsPaused = false;
                    Check(!s.TryCookAutoStart(order) && order.AutoStartChecked && order.RemainingClicks == (b.hit ? 999 : 1000) && rolls == 1 && s.LastActionWasAuto == b.hit, "strict threshold / standalone one-action batch"); Check(!s.TryCookAutoStart(order) && rolls == 1, "same entry no repeat");
                    s.IsPaused = true; s.IsPaused = false; s.SuspendAuto(); s.ReturnToBoard(); s.TrySelect(0); Check(!s.TryCookAutoStart(s.ActiveOrder) && s.ActiveOrder.RemainingClicks == 1000 && rolls == 1, "Back after hit/miss/reset cannot reroll");
                    s.ReturnToBoard(); s.TrySelect(1); s.TryCookAutoStart(s.ActiveOrder); Check(rolls == 2, "same dish different order has own visit"); s.EndDay();
                }
                int startRolls = 0; var periodic = Session(1, 1, () => { startRolls++; return 0; }); active = Begin(periodic, location); periodic.TryCookAutoStart(active); Check(active.RemainingClicks == 998 && periodic.LastAutoActionCount == 2 && startRolls == 1, "current upgraded batch"); periodic.TryCookAuto(active, 10); periodic.TryCookAuto(active, 10.999999); Check(active.RemainingClicks == 998, "start does not replace fresh periodic interval"); periodic.TryCookAuto(active, 11); Check(active.RemainingClicks == 996 && startRolls == 1, "periodic no start reroll"); periodic.EndDay();
                foreach (var example in new[] { new { count = 0, hp = 1 }, new { count = 1, hp = 1 }, new { count = 1, hp = 2 }, new { count = 10, hp = 11 } })
                {
                    SetDish(dish, example.hp); int rolls = 0, changes = 0; var s = Session(1, example.count, () => { rolls++; return 0; }); var order = Begin(s, location); s.Profile.Changed += () => changes++; decimal before = s.Rewards.Wallet; Check(s.TryCookAutoStart(order) && order.IsCompleted && order.AutoStartChecked && s.LastAutoActionCount == example.hp && s.LastHitDamage == example.hp && !s.LastHitWasCritical && s.LastStrikeCount == 0 && changes == 1 && rolls == 1 && s.CompletedDishes == 1 && s.ActiveOrder == null && s.Rewards.Wallet == before + s.LastResult.Payout && s.Profile.CaptureSnapshot().DishXpById[1] == 50 + s.LastResult.XpGained, "immediate bounded completion/one receipt");
                    decimal paid = s.Rewards.Wallet; Check(!s.TryCookAutoStart(order) && !s.TryCookAuto(order, 100) && !s.TryCookClick(order) && s.Rewards.Wallet == paid && changes == 1 && rolls == 1, "stale completion no reward/RNG"); s.TrySelect(0); Check(!s.ActiveOrder.AutoStartChecked && s.ActiveOrder.RemainingClicks == example.hp, "replacement fresh identity/no carry"); s.TryCookAutoStart(s.ActiveOrder); Check(rolls == 2 && s.CompletedDishes == 2, "replacement may start once"); s.EndDay();
                }
                return "U4b model PASS: prices/cap/guards, strict start thresholds, once per order after hit/miss/zero/Back, standalone/upgraded batch, periodic separation and immediate bounded receipt.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U4bStore/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 7; version++)
            {
                int length = version == 7 ? 10 : version == 6 ? 9 : version == 5 ? 7 : version + 1; string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks(0, 1).Take(length).ToArray(); string old = Signed(version, "1000.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old); var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot(); Check(p.GetUpgradeRank(UpgradeNodeRules.AutoActionChance) == 0 && File.ReadAllText(path) == old, "old load no gift/rewrite");
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "schema 8/old backup"); var after = new PlayerProfileStore(path).Load().CaptureSnapshot(); Check(after.Wallet == 1000.25m && after.DishXpById[1] == 50 && before.UpgradeRanksById.Count == after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value), "exact old state"); File.WriteAllText(path, "broken"); var recovery = new PlayerProfileStore(path); Check(recovery.Load() != null && recovery.Status == ProfileLoadStatus.Recovered, "old backup"); Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(UpgradeNodeRules.AutoActionChance, 0) }).ToArray())) == null, "old schema rejects even rank 0");
            }
            foreach (int rank in new[] { 1, 10 }) { string path = Path.Combine(folder, "r" + rank + ".json"); Check(new PlayerProfileStore(path).Save(Decode(Signed(8, "750.25", Ranks(rank, 1)))), "write chance"); var s = Session(new PlayerProfileStore(path).Load()); Check(s.AutoActionChanceRank == rank && s.AutoActionCountRank == 1 && s.DoubleStrikeChanceIIRank == 1 && s.Rewards.Wallet == 750.25m && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto, "roundtrip independent/idle"); }
            foreach (var ranks in new[] { Ranks(-1), Ranks(11), new[] { Rank(UpgradeNodeRules.Strength, 1), Rank(UpgradeNodeRules.AutoActionChance, 1) }, Ranks(1).Concat(new[] { Rank(UpgradeNodeRules.AutoActionChance, 1) }).ToArray(), Ranks().Concat(new[] { Rank("node.auto_action_chance.02", 1) }).ToArray(), Ranks().Concat(new[] { Rank("auto_action_chance", 1) }).ToArray() }) Check(Decode(Signed(8, "1000", ranks)) == null, "invalid cap/prerequisite/duplicate/tier/alias");
            string future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()), pathFuture = Path.Combine(folder, "future.json"); File.WriteAllText(pathFuture, future); var blocked = new PlayerProfileStore(pathFuture); Check(blocked.Load() == null && blocked.Status == ProfileLoadStatus.Unsupported && !blocked.Save(Decode(Signed(8, "1000", Ranks()))) && File.ReadAllText(pathFuture) == future, "future protected"); return "U4b storage PASS: schemas 1–7 → 8, exact legacy state/backups, node whitelist/bounds, independent roundtrip and version 9 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish; private static LocationData playLocation; private static int startRolls;
        public static string PreparePlay()
        { Check(!EditorApplication.isPlaying, "prepare stopped"); string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U4b-" + Guid.NewGuid().ToString("N"), "profile.json")); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(7, "1000.25", Ranks().Take(10).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path; }
        public static string BuyInPlay()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4b-"), "isolated profile"); var c = Controller; c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionChance); var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var buy = Field<UnityEngine.UI.Button>(c, "buyUpgradeButton"); Check(buy.interactable && detail.text.Contains("0 / 10") && detail.text.Contains(ClickCookUiCopy.Money(250)) && detail.text.Contains("першому вході"), "first-entry preview"); buy.onClick.Invoke(); buy.onClick.Invoke(); var p = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(p.GetUpgradeRank(UpgradeNodeRules.AutoActionChance) == 1 && p.GetUpgradeRank(UpgradeNodeRules.AutoActionCount) == 0 && p.Rewards.Wallet == 750.25m, "single chance debit/autosave"); return "U4b purchase/autosave PASS; advance frame.";
        }
        private static void BeginFixture(int hp)
        {
            var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.CloseUpgrades(); var s = Session(c.OrderSession.Profile, () => { startRolls++; return 0; }); typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation(); playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation); SetDish(playDish, hp); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); startRolls = 0; c.SelectOrder(0);
        }
        private static void EndFixture()
        { var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.ReturnToProfileMenu(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); playDish = null; playLocation = null; }
        public static string CheckStandaloneAndBack()
        {
            var c = Controller; var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); Canvas.ForceUpdateCanvases(); Check(detail.text.Contains("1 / 10") && detail.text.Contains(ClickCookUiCopy.Money(450)) && stats.text.Contains("Цикл: вимкнено · старт: 2%") && stats.preferredHeight <= stats.rectTransform.rect.height + 1 && stats.preferredWidth <= stats.rectTransform.rect.width + 1, "standalone preview/statistics fit");
            BeginFixture(50); var s = c.OrderSession; var first = s.ActiveOrder; Check(first.RemainingClicks == 49 && first.AutoStartChecked && startRolls == 1 && !s.HasAuto && Field<UnityEngine.UI.Text>(c, "cookingHpText").text.Contains("Помічник: −1 HP"), "controller immediate standalone batch"); c.BackToBoard(); c.SelectOrder(0); Check(s.ActiveOrder == first && first.RemainingClicks == 50 && startRolls == 1, "controller Back no reroll"); c.BackToBoard(); c.SelectOrder(1); Check(s.ActiveOrder.RemainingClicks == 49 && startRolls == 2, "other order independent visit"); return "U4b standalone/controller Back/other order PASS; observe no periodic ticks.";
        }
        public static string BuyCountAndStart()
        {
            var c = Controller; Check(c.OrderSession.ActiveOrder.RemainingClicks == 49 && !c.OrderSession.HasAuto && startRolls == 2, "standalone no periodic tick"); EndFixture(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionCount); Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").onClick.Invoke(); Check(c.OrderSession.AutoActionCountRank == 1 && c.OrderSession.Rewards.Wallet == 250.25m, "existing count combines independently"); BeginFixture(1000); Check(c.OrderSession.ActiveOrder.RemainingClicks == 998 && startRolls == 1 && c.OrderSession.LastAutoActionCount == 2, "start uses upgraded batch"); return "U4b current batch PASS; observe later periodic tick.";
        }
        public static string CheckPeriodicAndImmediateCompletion()
        {
            var c = Controller; Check(c.OrderSession.ActiveOrder.RemainingClicks < 998 && startRolls == 1, "periodic independent from first-entry roll"); EndFixture(); decimal before = c.OrderSession.Rewards.Wallet; BeginFixture(1); var s = c.OrderSession;
            Check(s.ActiveOrder == null && s.CompletedDishes == 1 && s.LastAutoActionCount == 1 && startRolls == 1 && Field<GameObject>(c, "dishResultScreen").activeSelf && !Field<GameObject>(c, "cookingPrototypeScreen").activeSelf && s.Rewards.Wallet == before + s.LastResult.Payout, "lethal start directly shows one result"); decimal paid = s.Rewards.Wallet; c.ClickCookingDish(); Check(s.CompletedDishes == 1 && s.Rewards.Wallet == paid, "stale result input"); var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(disk.GetUpgradeRank(UpgradeNodeRules.AutoActionChance) == 1 && disk.GetUpgradeRank(UpgradeNodeRules.AutoActionCount) == 1 && disk.Rewards.Wallet == paid, "immediate completion autosave"); SessionState.SetString("ClickCook.U4bWallet", paid.ToString(System.Globalization.CultureInfo.InvariantCulture)); SessionState.SetInt("ClickCook.U4bXp", s.Profile.CaptureSnapshot().DishXpById[1]); EndFixture(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionChance);
            var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases(); Check(stats.text.Contains("Авто: 2 дій × 1 HP · 1/с · старт: 2%") && stats.preferredHeight <= stats.rectTransform.rect.height + 1 && stats.preferredWidth <= stats.rectTransform.rect.width + 1, "combined statistics fit"); return "U4b periodic/immediate result/one receipt/autosave PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4b-"), "isolated restart"); var c = Controller; var s = c.OrderSession; Check(s.Profile.CaptureSnapshot().UpgradeRanksById.Count == 11 && s.AutoActionChanceRank == 1 && s.AutoActionCountRank == 1 && s.DoubleStrikeChanceRank == 10 && s.DoubleStrikeChanceIIRank == 1 && s.TripleStrikeChanceRank == 1 && s.ClickStrengthRank == 3 && s.HasHold && s.HoldFrequencyRank == 1 && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto && s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture) == SessionState.GetString("ClickCook.U4bWallet", "") && s.Profile.CaptureSnapshot().DishXpById[1] == SessionState.GetInt("ClickCook.U4bXp", -1), "all ranks/exact wallet/XP/idle"); c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionChance); return "U4b restart/Continue PASS.";
        }
        public static string Cleanup()
        { Check(!EditorApplication.isPlaying, "cleanup stopped"); SessionState.EraseString("ClickCook.U4bWallet"); SessionState.EraseInt("ClickCook.U4bXp"); return PlayerProfileStoreVerification.FinishPlayVerification(); }
    }
}
