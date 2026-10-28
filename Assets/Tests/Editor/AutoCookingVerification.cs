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
    /// <summary>U4a only: periodic auto scheduling, bounded non-critical batch, schema and controller.</summary>
    public static class AutoCookingVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U4a: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create(); return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int auto = 0) => new[] { Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1), Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, 10), Rank(UpgradeNodeRules.DoubleStrikePower, 1), Rank(UpgradeNodeRules.DoubleStrikeChanceII, 1), Rank(UpgradeNodeRules.TripleStrikeChance, 1), Rank(UpgradeNodeRules.AutoActionCount, auto) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile profile, Func<double> crit = null, Func<double> proc = null)
        { var s = new PrototypeOrderSession(profile, crit ?? (() => throw new Exception("auto consumed crit RNG")), proc ?? (() => throw new Exception("auto consumed proc RNG"))); Check(s.ConfigureUpgrades(Catalog), "configure"); return s; }
        private static PrototypeOrderSession Session(int rank) => Session(Decode(Signed(7, "10000000", Ranks(rank))));
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
                SetDish(dish, 1000); SetPool(location, dish); var locked = Session(Decode(Signed(7, "1000", Rank(UpgradeNodeRules.Strength, 1)))); Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.AutoActionCount, out _), "strength prerequisite");
                var poor = Session(Decode(Signed(7, "499.99", Ranks()))); poor.TryQuoteUpgrade(UpgradeNodeRules.AutoActionCount, out var costly); Check(!poor.TryPurchaseUpgrade(costly) && poor.Rewards.Wallet == 499.99m, "insufficient exact wallet");
                var buyer = Session(0); var unowned = Begin(buyer, location); buyer.TryCookAuto(unowned, 0); buyer.TryCookAuto(unowned, 100); Check(unowned.RemainingClicks == 1000 && !buyer.HasAuto, "unowned auto disabled"); buyer.EndDay();
                decimal[] prices = { 500, 900, 1620, 2916, 5249, 9448, 17007, 30612, 55100, 99180 }; decimal spent = 0;
                for (int n = 0; n < 10; n++)
                {
                    Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoActionCount, out var q) && q.Price == prices[n], "known price " + n); buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false; buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "day purchase"); buyer.EndDay();
                    Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += q.Price; var order = Begin(buyer, location);
                    Check(buyer.AutoActionsPerTick == n + 2 && !buyer.TryCookAuto(order, 0) && !buyer.TryCookAuto(order, .999999) && order.RemainingClicks == 1000, "fresh full interval"); buyer.TryCookAuto(order, 1);
                    Check(order.RemainingClicks == 1000 - (n + 2) && buyer.LastActionWasAuto && buyer.LastAutoActionCount == n + 2 && buyer.LastHitDamage == n + 2 && !buyer.LastHitWasCritical && buyer.LastStrikeCount == 0, "bounded plain batch " + n); buyer.EndDay();
                }
                Check(buyer.AutoActionCountRank == 10 && buyer.AutoActionsPerTick == 11 && buyer.Rewards.Wallet == 10000000m - spent && !buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoActionCount, out _), "cap/total");
                foreach (var id in new[] { "node.auto_action_count.02", "node.auto_action_chance.02", "node.auto_strength.02", "node.auto_frequency.02", "node.auto_strength_multiplier.01" }) Check(UpgradeBatchVerification.AvailabilityMatches(buyer,id), "later tier availability contract");
                var s = Session(1); var active = Begin(s, location); s.TryCookAuto(active, 0); s.TryCookAuto(active, 1); s.TryCookAuto(active, 100); s.TryCookAuto(active, 100); s.TryCookAuto(active, 100.999999); Check(active.RemainingClicks == 996, "no burst/backlog"); s.TryCookAuto(active, 101); Check(active.RemainingClicks == 994, "next fresh deadline");
                s.IsPaused = true; s.IsPaused = false; s.TryCookAuto(active, 200); s.TryCookAuto(active, 200.999999); Check(active.RemainingClicks == 994, "same-frame pause/resume resets"); s.TryCookAuto(active, 201); Check(active.RemainingClicks == 992, "resume full interval");
                s.SuspendAuto(); s.TryCookAuto(active, 300); Check(active.RemainingClicks == 992, "focus/disable suspension resets"); s.TryCookAuto(active, double.NaN); s.TryCookAuto(active, 400); Check(active.RemainingClicks == 992, "invalid clock suspends");
                s.ReturnToBoard(); Check(!s.TryCookAuto(active, 500), "Back stale order"); s.TrySelect(0); active = s.ActiveOrder; s.TryCookAuto(active, 600); Check(active.RemainingClicks == 1000, "reselect no immediate tick"); s.TryCookAuto(active, 601); Check(active.RemainingClicks == 998, "reselect tick"); s.EndDay(); Check(!s.TryCookAuto(active, 1000), "end day");
                int crit = 0, proc = 0; var mixed = Session(Decode(Signed(7, "1000", Ranks(1))), () => { crit++; return 0; }, () => { proc++; return .22; }); active = Begin(mixed, location); mixed.TryCookAuto(active, 0); mixed.TryCookClick(active); Check(active.RemainingClicks == 973, "manual triple remains"); mixed.TryCookAuto(active, 1); Check(active.RemainingClicks == 971 && crit == 3 && proc == 1 && mixed.LastActionWasAuto, "auto neither rolls nor inherits manual damage"); mixed.TryCookClick(active); Check(active.RemainingClicks == 944 && !mixed.LastActionWasAuto && mixed.LastStrikeCount == 3 && crit == 6 && proc == 2, "next manual feedback"); mixed.EndDay();
                foreach (int hp in new[] { 1, 2, 11, 12 })
                {
                    SetDish(dish, hp); var a = Session(10); var order = Begin(a, location); a.TryBeginHold(order, 0); int changes = 0; a.Profile.Changed += () => changes++; decimal before = a.Rewards.Wallet; a.TryCookAuto(order, 0); bool done = a.TryCookAuto(order, 1); if (hp == 12) { Check(!done && order.RemainingClicks == 1, "partial batch"); done = a.TryCookAuto(order, 2); }
                    Check(done && a.LastAutoActionCount == (hp == 12 ? 1 : hp) && a.LastHitDamage == a.LastAutoActionCount && !a.LastHitWasCritical && !a.IsHolding && a.ActiveOrder == null && changes == 1 && a.CompletedDishes == 1 && a.Rewards.Wallet == before + a.LastResult.Payout && a.Profile.CaptureSnapshot().DishXpById[1] == 50 + a.LastResult.XpGained, "auto completion once/bounded actions " + hp);
                    decimal paid = a.Rewards.Wallet; Check(!a.TryCookAuto(order, 200) && !a.TryCookClick(order) && a.Rewards.Wallet == paid && changes == 1, "result stale events"); a.TrySelect(0); Check(a.ActiveOrder.RemainingClicks == hp, "no carried batch"); a.EndDay();
                }
                SetDish(dish, 13); var manual = Session(Decode(Signed(7, "1000", Ranks(1))), () => 0, () => throw new Exception("lethal primary proc")); active = Begin(manual, location); manual.TryCookAuto(active, 0); Check(manual.TryCookClick(active) && !manual.TryCookAuto(active, 1) && manual.CompletedDishes == 1, "manual completes before due auto"); manual.EndDay();
                SetDish(dish, 1); var cap = Session(1); cap.StartDay(location, 2); for (int i = 0; i < 10; i++) { cap.TrySelect(0); var order = cap.ActiveOrder; cap.TryCookAuto(order, i * 2); Check(cap.TryCookAuto(order, i * 2 + 1), "auto day completion"); } Check(cap.CompletedDishes == 10 && !cap.TrySelect(0), "day cap"); cap.EndDay();
                return "U4a model PASS: prices/activation/cap, exact ticks, no backlog, reset guards, plain auto without combat RNG, mixed manual and single bounded completion/day cap.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U4aStore/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 6; version++)
            {
                string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks().Take(version == 6 ? 9 : version == 5 ? 7 : version + 1).ToArray(); string old = Signed(version, "1000.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old); var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot(); Check(!Session(p).HasAuto && File.ReadAllText(path) == old, "old load no free auto/rewrite");
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "schema 7/backup"); var after = new PlayerProfileStore(path).Load().CaptureSnapshot(); Check(after.Wallet == 1000.25m && after.DishXpById[1] == 50 && before.UpgradeRanksById.Count == after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value), "exact old state");
                File.WriteAllText(path, "broken"); var recovered = new PlayerProfileStore(path); Check(recovered.Load() != null && recovered.Status == ProfileLoadStatus.Recovered, "old backup recovery"); Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(UpgradeNodeRules.AutoActionCount, 0) }).ToArray())) == null, "old schema rejects even rank 0");
            }
            foreach (int rank in new[] { 1, 10 }) { string path = Path.Combine(folder, "r" + rank + ".json"); Check(new PlayerProfileStore(path).Save(Decode(Signed(7, "500.25", Ranks(rank)))), "save owned"); var s = Session(new PlayerProfileStore(path).Load()); Check(s.AutoActionCountRank == rank && s.AutoActionsPerTick == 1 + rank && s.Rewards.Wallet == 500.25m && s.TripleStrikeChanceRank == 1 && !s.IsDayActive && !s.LastActionWasAuto && !s.IsHolding, "roundtrip ranks/idle"); }
            foreach (var ranks in new[] { Ranks(-1), Ranks(11), new[] { Rank(UpgradeNodeRules.Strength, 1), Rank(UpgradeNodeRules.AutoActionCount, 1) }, Ranks(1).Concat(new[] { Rank(UpgradeNodeRules.AutoActionCount, 1) }).ToArray(), Ranks().Concat(new[] { Rank("node.auto_action_count.02", 1) }).ToArray(), Ranks().Concat(new[] { Rank("auto_action_count", 1) }).ToArray() }) Check(Decode(Signed(7, "1000", ranks)) == null, "invalid cap/prerequisite/duplicate/tier/alias");
            string future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()), fp = Path.Combine(folder, "future.json"); File.WriteAllText(fp, future); var blocked = new PlayerProfileStore(fp); Check(blocked.Load() == null && blocked.Status == ProfileLoadStatus.Unsupported && !blocked.Save(Decode(Signed(7, "1000", Ranks()))) && File.ReadAllText(fp) == future, "future protected"); return "U4a storage PASS: schemas 1–6 → 7, exact legacy state/backups, cap/prerequisite/whitelist, idle roundtrip and version 8 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish; private static LocationData playLocation; private static int pausedHp;
        public static string PreparePlay()
        { Check(!EditorApplication.isPlaying, "prepare stopped"); string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U4a-" + Guid.NewGuid().ToString("N"), "profile.json")); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(6, "1000.25", Ranks().Take(9).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path; }
        public static string BuyInPlay()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4a-"), "isolated save"); var c = Controller; c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionCount); var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var buy = Field<UnityEngine.UI.Button>(c, "buyUpgradeButton"); Check(buy.interactable && detail.text.Contains("0 / 10") && detail.text.Contains(ClickCookUiCopy.Money(500)) && detail.text.Contains("Дій за цикл: 2"), "activation preview"); buy.onClick.Invoke(); buy.onClick.Invoke(); var saved = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(saved.Rewards.Wallet == 500.25m && saved.GetUpgradeRank(UpgradeNodeRules.AutoActionCount) == 1, "single button debit/autosave"); return "U4a purchase/autosave PASS; advance frame.";
        }
        public static string StartCooking()
        {
            var c = Controller; var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases(); Check(detail.text.Contains("1 / 10") && detail.text.Contains(ClickCookUiCopy.Money(900)) && stats.text.Contains("Авто: 2 дій × 1 HP · 1/с") && stats.preferredHeight <= stats.rectTransform.rect.height + 1 && stats.preferredWidth <= stats.rectTransform.rect.width + 1, "next price/statistics fit");
            c.CloseUpgrades(); var s = Session(c.OrderSession.Profile); typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation(); playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation); SetDish(playDish, 1000); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); c.SelectOrder(0); c.SendMessage("OnApplicationFocus", true); return "U4a fixture ready; observe real LateUpdate tick.";
        }
        public static string CheckTickAndPause()
        {
            var c = Controller; var s = c.OrderSession; Check(s.ActiveOrder.RemainingClicks < 1000 && s.LastActionWasAuto && s.LastAutoActionCount == 2 && s.LastHitDamage == 2 && !s.LastHitWasCritical, "live plain auto tick"); var hp = Field<UnityEngine.UI.Text>(c, "cookingHpText"); Canvas.ForceUpdateCanvases(); Check(hp.text.Contains("Помічник: −2 HP") && hp.preferredHeight <= hp.rectTransform.rect.height + 1, "auto feedback fits"); c.GetComponent<ProfileMenu>().Pause(); pausedHp = s.ActiveOrder.RemainingClicks; return "U4a live tick/feedback PASS; paused, observe unchanged HP.";
        }
        private static void Advance() => typeof(ClickCookPrototypeController).GetMethod("AdvanceAutoCooking", Private).Invoke(Controller, null);
        public static string ResumeAndArmCompletion()
        {
            var c = Controller; var s = c.OrderSession; Check(s.ActiveOrder.RemainingClicks == pausedHp, "no paused tick"); c.GetComponent<ProfileMenu>().Resume();
            foreach (string message in new[] { "OnApplicationFocus", "OnApplicationPause" }) { c.SendMessage(message, message == "OnApplicationPause"); Advance(); Check(s.ActiveOrder.RemainingClicks == pausedHp, "suspended controller " + message); c.SendMessage(message, message != "OnApplicationPause"); c.GetComponent<ProfileMenu>().Resume(); Advance(); Check(Field<double>(s, "nextAutoTime") >= Time.unscaledTimeAsDouble + .99, "fresh full interval " + message); }
            c.enabled = false; c.enabled = true; Advance(); Check(Field<double>(s, "nextAutoTime") >= Time.unscaledTimeAsDouble + .99, "disable/enable fresh interval");
            typeof(PrototypeOrder).GetProperty("RemainingClicks").SetValue(s.ActiveOrder, 1); s.SuspendAuto(); Advance(); return "U4a pause/focus/system pause/disable reset PASS; await real auto completion.";
        }
        public static string CheckCompletion()
        {
            var c = Controller; var s = c.OrderSession; Check(s.CompletedDishes == 1 && s.ActiveOrder == null && s.LastActionWasAuto && s.LastAutoActionCount == 1 && !s.IsHolding, "actual auto result transition"); decimal paid = s.Rewards.Wallet; c.ClickCookingDish(); Advance(); Check(s.Rewards.Wallet == paid && s.CompletedDishes == 1, "stale result guards"); var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(disk.Rewards.Wallet == paid && disk.GetUpgradeRank(UpgradeNodeRules.AutoActionCount) == 1, "completion autosave");
            SessionState.SetString("ClickCook.U4aWallet", paid.ToString(System.Globalization.CultureInfo.InvariantCulture)); SessionState.SetInt("ClickCook.U4aXp", s.Profile.CaptureSnapshot().DishXpById[1]); c.GetComponent<ProfileMenu>().Resume(); c.ContinueFromDishResult(); c.BackToBoard(); c.FinishDay(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionCount); return "U4a live auto completion/one receipt/autosave PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4a-"), "isolated restart"); var c = Controller; var s = c.OrderSession; Check(s.Profile.CaptureSnapshot().UpgradeRanksById.Count == 10 && s.AutoActionCountRank == 1 && s.DoubleStrikeChanceRank == 10 && s.DoubleStrikeChanceIIRank == 1 && s.TripleStrikeChanceRank == 1 && s.ClickStrengthRank == 3 && s.HoldFrequencyRank == 1 && s.HasHold && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto && s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture) == SessionState.GetString("ClickCook.U4aWallet", "") && s.Profile.CaptureSnapshot().DishXpById[1] == SessionState.GetInt("ClickCook.U4aXp", -1), "ranks/exact wallet/XP/idle"); c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoActionCount); return "U4a restart/Continue PASS.";
        }
        public static string Cleanup()
        { Check(!EditorApplication.isPlaying, "cleanup stopped"); SessionState.EraseString("ClickCook.U4aWallet"); SessionState.EraseInt("ClickCook.U4aXp"); return PlayerProfileStoreVerification.FinishPlayVerification(); }
    }
}
