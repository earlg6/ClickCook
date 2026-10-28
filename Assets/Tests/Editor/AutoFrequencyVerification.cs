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
    /// <summary>U4e: purchased frequency deadlines, lifecycle reset and persistence boundaries.</summary>
    public static class AutoFrequencyVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U4e: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create(); return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int frequency = 0, int count = 1, int strength = 2, int multiplier = 2) => new[] {
            Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1),
            Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, 10),
            Rank(UpgradeNodeRules.DoubleStrikePower, 1), Rank(UpgradeNodeRules.DoubleStrikeChanceII, 1), Rank(UpgradeNodeRules.TripleStrikeChance, 1),
            Rank(UpgradeNodeRules.AutoActionCount, count), Rank(UpgradeNodeRules.AutoActionChance, 1), Rank(UpgradeNodeRules.AutoStrength, strength),
            Rank(UpgradeNodeRules.AutoStrengthMultiplier, multiplier), Rank(UpgradeNodeRules.AutoFrequency, frequency) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile p, bool manual = false)
        {
            var s = new PrototypeOrderSession(p, manual ? () => 0 : () => throw new Exception("auto used crit RNG"),
                manual ? () => .22 : () => throw new Exception("auto used manual proc RNG"), () => 0);
            Check(s.ConfigureUpgrades(Catalog), "configure"); return s;
        }
        private static PrototypeOrderSession Session(int frequency, bool manual = false) => Session(Decode(Signed(11, "10000000.25", Ranks(frequency))), manual);
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
                var locked = Session(Decode(Signed(11, "1000", Ranks(0, 0, 0, 0))));
                Check(!locked.HasAuto && locked.AutoFrequency == 0 && !locked.TryQuoteUpgrade(UpgradeNodeRules.AutoFrequency, out _), "count prerequisite/disabled actual rate");
                var inactive = Begin(locked, location); locked.TryCookAuto(inactive, 0); locked.TryCookAuto(inactive, 100); Check(inactive.RemainingClicks == 10000, "frequency does not unlock auto"); locked.EndDay();
                var poor = Session(Decode(Signed(11, "599.99", Ranks()))); Check(poor.TryQuoteUpgrade(UpgradeNodeRules.AutoFrequency, out var costly) && !poor.TryPurchaseUpgrade(costly) && poor.Rewards.Wallet == 599.99m, "insufficient funds");
                decimal[] prices = { 600, 1080, 1944, 3500, 6299, 11338, 20408, 36734, 66120, 119016, 214229, 385611, 694099, 1249378, 2248881 };
                var buyer = Session(0); decimal spent = 0; Check(buyer.AutoFrequency == 1 && buyer.AutoInterval == 1, "rank zero compatibility");
                for (int n = 0; n < prices.Length; n++)
                {
                    Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoFrequency, out var q) && q.Price == prices[n], "known price " + n);
                    buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false; buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "day purchase"); buyer.EndDay();
                    Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += prices[n];
                    int rank = n + 1; double interval = 20d / (20 + 3 * rank);
                    Check(buyer.AutoFrequency == (20 + 3 * rank) / 20m && Math.Abs(buyer.AutoInterval - interval) < 1e-12 && buyer.AutoFrequencyRank == rank, "independent rate/interval oracle");
                    var order = Begin(buyer, location); buyer.TryCookAutoStart(order); Check(order.RemainingClicks == 9992, "unchanged start batch");
                    buyer.TryCookAuto(order, 0); buyer.TryCookAuto(order, interval - 1e-6); Check(order.RemainingClicks == 9992, "full first interval");
                    buyer.TryCookAuto(order, buyer.AutoInterval); Check(order.RemainingClicks == 9984 && buyer.LastHitDamage == 8 && buyer.LastAutoActionCount == 2 && !buyer.LastHitWasCritical && buyer.LastStrikeCount == 0, "exact deadline one plain batch");
                    buyer.TryCookAuto(order, 100); buyer.TryCookAuto(order, 100); buyer.TryCookAuto(order, 100 + interval - 1e-6); Check(order.RemainingClicks == 9976, "delayed frame no catchup");
                    buyer.TryCookAuto(order, 100 + buyer.AutoInterval); Check(order.RemainingClicks == 9968 && buyer.AutoDamagePerAction == 4 && buyer.AutoStrengthRank == 2 && buyer.AutoStrengthMultiplierRank == 2 && buyer.AutoActionsPerTick == 2 && buyer.AutoActionChance == .02 && buyer.ClickDamage == 6 && buyer.HoldFrequency == 4.5m, "reschedule exact interval/independent effects"); buyer.EndDay();
                }
                Check(spent == 5059237 && buyer.Rewards.Wallet == 10000000.25m - spent && buyer.AutoFrequency == 3.25m && !buyer.TryQuoteUpgrade(UpgradeNodeRules.AutoFrequency, out _), "cap/total");
                foreach (var id in new[] { "node.auto_frequency.02", "node.auto_frequency.03", "node.auto_frequency.04" }) Check(UpgradeBatchVerification.AvailabilityMatches(buyer,id), "later tier availability contract");
                var tree = UpgradeTreeDefinition.Load(); Check(tree.PrerequisitesMet(tree.Find("node.auto_frequency.02"), buyer.Profile.GetUpgradeRank), "second tier sees first purchased max");
                var s = Session(15); var active = Begin(s, location); double dt = s.AutoInterval; s.TryCookAuto(active, 0); s.TryCookAuto(active, dt);
                s.IsPaused = true; s.TryCookAuto(active, 50); s.IsPaused = false; s.TryCookAuto(active, 100); s.TryCookAuto(active, 100 + dt - 1e-6); Check(active.RemainingClicks == 9992, "pause fresh full interval");
                s.TryCookAuto(active, 100 + dt); Check(active.RemainingClicks == 9984, "resume exact tick");
                s.IsPaused = true; s.IsPaused = false; s.TryCookAuto(active, 200); s.TryCookAuto(active, 200 + dt - 1e-6); Check(active.RemainingClicks == 9984, "same-frame pause reset");
                s.SuspendAuto(); s.TryCookAuto(active, 300); s.TryCookAuto(active, 300 + dt - 1e-6); Check(active.RemainingClicks == 9984, "focus/disable full reset");
                foreach (double bad in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity }) { s.TryCookAuto(active, bad); s.TryCookAuto(active, 400); Check(active.RemainingClicks == 9984, "invalid clock reset"); }
                s.ReturnToBoard(); Check(!s.TryCookAuto(active, 500), "stale Back order"); s.TrySelect(0); active = s.ActiveOrder; s.TryCookAuto(active, 600); Check(active.RemainingClicks == 10000, "reselect no immediate tick"); s.TryCookAuto(active, 600 + dt); Check(active.RemainingClicks == 9992, "reselect frequency"); s.EndDay(); Check(!s.TryCookAuto(active, 1000), "end day");
                SetDish(dish, 1000); var mixed = Session(2, true); active = Begin(mixed, location); mixed.TryCookAutoStart(active); mixed.TryCookClick(active); Check(active.RemainingClicks == 965 && mixed.LastHitDamage == 27, "manual unchanged"); mixed.TryCookAuto(active, 0); mixed.TryCookAuto(active, mixed.AutoInterval); Check(active.RemainingClicks == 957 && mixed.LastHitDamage == 8, "mixed periodic"); mixed.ReturnToBoard(); mixed.TrySelect(0); Check(!mixed.TryCookAutoStart(mixed.ActiveOrder), "frequency no start reroll"); mixed.EndDay();
                foreach (int hp in new[] { 1, 4, 5, 8, 9 })
                {
                    SetDish(dish, hp); var a = Session(15); var order = Begin(a, location); int changes = 0; a.Profile.Changed += () => changes++; decimal before = a.Rewards.Wallet;
                    a.TryCookAuto(order, 0); bool done = a.TryCookAuto(order, a.AutoInterval); if (hp == 9) { Check(!done && order.RemainingClicks == 1 && changes == 0, "partial periodic"); done = a.TryCookAuto(order, 2 * a.AutoInterval); }
                    int actions = hp <= 4 || hp == 9 ? 1 : 2;
                    Check(done && order.RemainingClicks == 0 && a.LastHitDamage == actions * 4 && a.LastAutoActionCount == actions && a.CompletedDishes == 1 && changes == 1 && a.Rewards.Wallet == before + a.LastResult.Payout && a.Profile.CaptureSnapshot().DishXpById[1] == 50 + a.LastResult.XpGained, "periodic single bounded completion");
                    decimal paid = a.Rewards.Wallet; Check(!a.TryCookAuto(order, 100) && !a.TryCookAutoStart(order) && !a.TryCookClick(order) && a.Rewards.Wallet == paid && changes == 1, "stale reward guards"); a.TrySelect(0); Check(a.ActiveOrder.RemainingClicks == hp, "no carry"); a.EndDay();
                }
                return "U4e model PASS: all prices/rates/deadlines, cap/guards, no backlog, pause/reselect/suspend/invalid-clock reset, unchanged start/manual/Hold and single periodic reward.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U4eStore/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 10; version++)
            {
                int length = version >= 6 ? version + 3 : version == 5 ? 7 : version + 1;
                string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks().Take(length).ToArray();
                string old = Signed(version, "2500.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old); var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot();
                Check(p.GetUpgradeRank(UpgradeNodeRules.AutoFrequency) == 0 && Session(p).AutoInterval == 1 && File.ReadAllText(path) == old, "old interval/no rewrite");
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "schema/backup");
                var after = new PlayerProfileStore(path).Load().CaptureSnapshot(); Check(after.Wallet == 2500.25m && after.DishXpById[1] == 50 && before.UpgradeRanksById.Count == after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value), "exact old state");
                File.WriteAllText(path, "broken"); var recovery = new PlayerProfileStore(path); Check(recovery.Load() != null && recovery.Status == ProfileLoadStatus.Recovered, "old backup recovery");
                Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(UpgradeNodeRules.AutoFrequency, 0) }).ToArray())) == null, "old schema rejects new ID even zero");
            }
            foreach (int rank in new[] { 1, 15 })
            {
                string path = Path.Combine(folder, "r" + rank + ".json"); Check(new PlayerProfileStore(path).Save(Decode(Signed(11, "1000.25", Ranks(rank)))), "write frequency"); var s = Session(new PlayerProfileStore(path).Load());
                Check(s.AutoFrequencyRank == rank && s.AutoFrequency == (rank == 1 ? 1.15m : 3.25m) && s.AutoDamagePerAction == 4 && s.AutoActionCountRank == 1 && s.AutoStrengthRank == 2 && s.AutoStrengthMultiplierRank == 2 && s.Rewards.Wallet == 1000.25m && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto, "independent idle roundtrip");
            }
            foreach (var ranks in new[] { Ranks(-1), Ranks(16), Ranks(1, 0, 0, 0), Ranks(1).Concat(new[] { Rank(UpgradeNodeRules.AutoFrequency, 1) }).ToArray(), Ranks().Concat(new[] { Rank("node.auto_frequency.02", 1) }).ToArray(), Ranks().Concat(new[] { Rank("auto_frequency", 1) }).ToArray() }) Check(Decode(Signed(11, "1000", ranks)) == null, "invalid bounds/prerequisite/duplicate/tier/alias");
            string future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()), futurePath = Path.Combine(folder, "future.json"); File.WriteAllText(futurePath, future); var blocked = new PlayerProfileStore(futurePath);
            Check(blocked.Load() == null && blocked.Status == ProfileLoadStatus.Unsupported && !blocked.Save(Decode(Signed(11, "1000", Ranks()))) && File.ReadAllText(futurePath) == future, "future protected");
            return "U4e storage PASS: schemas 1–10 to 11, exact old state/backups, bounds/prerequisites/whitelist, independent idle roundtrip and version 12 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish; private static LocationData playLocation;
        private static int pausedHp; private static double probeAt; private static int probeHp; private static double probeDeadline;
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying, "prepare stopped"); string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U4e-" + Guid.NewGuid().ToString("N"), "profile.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(10, "2000.25", Ranks().Take(13).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path;
        }
        public static string ShowNativePurchase()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4e-"), "isolated profile"); var c = Controller; c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoFrequency); Field<UpgradeTreeView>(c, "upgradeTreeView").FocusNode(UpgradeNodeRules.AutoFrequency);
            Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains("0 / 15") && Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "ready native purchase"); return "U4e ready for native purchase, wallet 2000.25.";
        }
        public static string CheckNativePurchase(int rank)
        {
            var s = Controller.OrderSession; decimal wallet = rank == 1 ? 1400.25m : 320.25m; var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();
            Check(s.AutoFrequencyRank == rank && s.AutoFrequency == (rank == 1 ? 1.15m : 1.3m) && s.Rewards.Wallet == wallet && disk.GetUpgradeRank(UpgradeNodeRules.AutoFrequency) == rank && disk.Rewards.Wallet == wallet && s.AutoDamagePerAction == 4, "single debit/rate/autosave " + rank); return "U4e native purchase rank " + rank + " PASS.";
        }
        private static void BeginFixture(int hp)
        {
            var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.CloseUpgrades(); var s = Session(c.OrderSession.Profile); typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation();
            playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation); SetDish(playDish, hp); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); c.SelectOrder(0);
        }
        private static void EndFixture()
        { var c = Controller; c.GetComponent<ProfileMenu>().Resume(); c.ReturnToProfileMenu(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); }
        public static string StartCooking()
        {
            CheckNativePurchase(2); var c = Controller; c.GetComponent<ProfileMenu>().Resume(); string detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text;
            Check(detail.Contains("2 / 15") && detail.Contains(1.3m.ToString("0.##")) && detail.Contains(1.45m.ToString("0.##")) && detail.Contains(ClickCookUiCopy.Money(1944)) && !Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "rank/rate/price/funds preview");
            c.SelectUpgradeNode(UpgradeNodeRules.AutoActionCount); Check(Field<UnityEngine.UI.Text>(c, "upgradeDetailsText").text.Contains(1.3m.ToString("0.##") + "/с"), "count preview current rate");
            var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases();
            Check(stats.text.Contains("Авто: 2 дій × 4 HP") && stats.text.Contains(1.3m.ToString("0.##") + "/с") && stats.preferredHeight <= stats.rectTransform.rect.height + 1 && stats.preferredWidth <= stats.rectTransform.rect.width + 1, "actual statistics fit");
            BeginFixture(1000); Check(c.OrderSession.ActiveOrder.RemainingClicks == 992 && c.OrderSession.AutoInterval > .769 && c.OrderSession.AutoInterval < .770, "controller start/interval"); return "U4e start/UI PASS; bind periodic timer.";
        }
        public static string ArmTickProbe()
        {
            var s = Controller.OrderSession; Check(s.ActiveOrder != null, "active probe");
            probeAt = Time.unscaledTimeAsDouble; probeHp = s.ActiveOrder.RemainingClicks; s.SuspendAuto(); s.TryCookAuto(s.ActiveOrder, probeAt); probeDeadline = Field<double>(s, "nextAutoTime");
            Check(Math.Abs(probeDeadline - probeAt - 10d / 13) < 1e-9, "live rank-2 exact interval");
            return "U4e live timer armed at " + probeAt + ", deadline " + probeDeadline + ", HP " + probeHp;
        }
        public static string CheckTickAndPause()
        {
            var c = Controller; var s = c.OrderSession; Check(s.ActiveOrder.RemainingClicks < probeHp && s.LastHitDamage == 8 && s.LastAutoActionCount == 2, "real periodic tick");
            c.GetComponent<ProfileMenu>().Pause(); pausedHp = s.ActiveOrder.RemainingClicks; return "U4e periodic PASS, paused HP " + pausedHp;
        }
        public static string CheckPauseAndResume()
        {
            var c = Controller; Check(c.OrderSession.IsPaused && c.OrderSession.ActiveOrder.RemainingClicks == pausedHp, "paused HP unchanged"); c.GetComponent<ProfileMenu>().Resume(); return "U4e pause PASS; Resume waits full interval.";
        }
        public static string CompletePeriodic()
        {
            var c = Controller; Check(c.OrderSession.ActiveOrder.RemainingClicks < pausedHp, "real tick after Resume"); EndFixture();
            BeginFixture(13); Check(c.OrderSession.ActiveOrder.RemainingClicks == 5, "start leaves periodic work"); return "U4e lethal fixture waits for real periodic completion.";
        }
        public static string CheckResult()
        {
            var c = Controller; var s = c.OrderSession; Check(s.ActiveOrder == null && s.CompletedDishes == 1 && s.LastAutoActionCount == 2 && s.LastHitDamage == 8 && Field<GameObject>(c, "dishResultScreen").activeSelf, "periodic lethal/result"); decimal paid = s.Rewards.Wallet; c.ClickCookingDish(); Check(s.CompletedDishes == 1 && s.Rewards.Wallet == paid, "stale result");
            var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(disk.GetUpgradeRank(UpgradeNodeRules.AutoFrequency) == 2 && disk.Rewards.Wallet == paid, "result autosave"); SessionState.SetString("ClickCook.U4eWallet", paid.ToString(System.Globalization.CultureInfo.InvariantCulture)); SessionState.SetInt("ClickCook.U4eXp", s.Profile.CaptureSnapshot().DishXpById[1]);
            EndFixture(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoFrequency); Field<UpgradeTreeView>(c, "upgradeTreeView").FocusNode(UpgradeNodeRules.AutoFrequency); return "U4e controller periodic result/autosave PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U4e-"), "isolated restart"); var c = Controller; var s = c.OrderSession;
            Check(s.Profile.CaptureSnapshot().UpgradeRanksById.Count == 14 && s.AutoFrequencyRank == 2 && s.AutoFrequency == 1.3m && s.AutoDamagePerAction == 4 && s.AutoStrengthRank == 2 && s.AutoStrengthMultiplierRank == 2 && s.AutoActionCountRank == 1 && s.AutoActionChanceRank == 1 && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto && s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture) == SessionState.GetString("ClickCook.U4eWallet", "") && s.Profile.CaptureSnapshot().DishXpById[1] == SessionState.GetInt("ClickCook.U4eXp", -1), "all ranks/exact wallet/XP/idle");
            c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.AutoFrequency); return "U4e restart/Continue PASS.";
        }
        public static string Cleanup()
        { Check(!EditorApplication.isPlaying, "cleanup stopped"); SessionState.EraseString("ClickCook.U4eWallet"); SessionState.EraseInt("ClickCook.U4eXp"); return PlayerProfileStoreVerification.FinishPlayVerification(); }
    }
}
