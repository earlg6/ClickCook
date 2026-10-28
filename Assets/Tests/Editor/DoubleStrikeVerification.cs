using System;
using System.Collections.Generic;
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
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    /// <summary>U3 transaction/RNG/schema checks plus isolated live purchase/feedback/Continue.</summary>
    public static class DoubleStrikeVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U3: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create(); return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int chance = 0, int power = 0, int critical = 1) => new[] { Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, critical), Rank(UpgradeNodeRules.CriticalMultiplier, critical), Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, chance), Rank(UpgradeNodeRules.DoubleStrikePower, power) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile p, Func<double> crit = null, Func<double> proc = null)
        { var s = new PrototypeOrderSession(p, crit, proc); Check(s.ConfigureUpgrades(Catalog), "configure"); return s; }
        private static PrototypeOrderSession Session(int chance, int power, Func<double> crit, Func<double> proc, int critical = 1) => Session(Decode(Signed(5, "10000000", Ranks(chance, power, critical))), crit, proc);
        private static void SetDish(DishData dish, int hp)
        {
            var d = new SerializedObject(dish); d.FindProperty("dishId").intValue = 1; d.FindProperty("clickRequirement").intValue = hp; d.FindProperty("saleValue").intValue = 5; d.ApplyModifiedPropertiesWithoutUndo();
        }
        private static void SetPool(LocationData location, DishData dish)
        { var d = new SerializedObject(location); var pool = d.FindProperty("documentedAvailableDishes"); pool.arraySize = 1; pool.GetArrayElementAtIndex(0).objectReferenceValue = dish; d.ApplyModifiedPropertiesWithoutUndo(); }
        private static PrototypeOrder Begin(PrototypeOrderSession s, LocationData location) { s.StartDay(location, 2); Check(s.TrySelect(0), "select"); return s.ActiveOrder; }

        public static string RunModelChecks()
        {
            var random = UnityEngine.Random.state; var dish = ScriptableObject.CreateInstance<DishData>(); var location = ScriptableObject.CreateInstance<LocationData>();
            try
            {
                SetDish(dish, 1000); SetPool(location, dish);
                var locked = Session(Decode(Signed(5, "100000", Rank(UpgradeNodeRules.Strength, 1)))); Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.DoubleStrikeChance, out _), "strength prerequisite");
                var buyer = Session(0, 0, () => 0, () => 0); Check(!buyer.TryQuoteUpgrade(UpgradeNodeRules.DoubleStrikePower, out _) && buyer.DoubleStrikePower == .5m, "chance required / free baseline power");
                decimal[][] prices = { new decimal[] { 350, 595, 1012, 1720, 2924, 4970, 8449, 14362, 24416, 41506 }, new decimal[] { 400, 680, 1156, 1966, 3341, 5680, 9656, 16414, 27904, 47436 } };
                string[] ids = { UpgradeNodeRules.DoubleStrikeChance, UpgradeNodeRules.DoubleStrikePower }; decimal spent = 0;
                for (int node = 0; node < 2; node++) for (int n = 0; n < 10; n++)
                {
                    Check(buyer.TryQuoteUpgrade(ids[node], out var q) && q.Price == prices[node][n], "known quote " + ids[node] + n);
                    buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false;
                    buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "active-day purchase"); buyer.EndDay();
                    Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += prices[node][n];
                }
                Check(buyer.DoubleStrikeChance == .2d && buyer.DoubleStrikePower == 1m && buyer.Rewards.Wallet == 10000000m - spent && !buyer.TryQuoteUpgrade(ids[0], out _) && !buyer.TryQuoteUpgrade(ids[1], out _), "caps and exact total debit");
                Check(UpgradeBatchVerification.AvailabilityMatches(buyer,"node.double_strike_chance.03") && UpgradeBatchVerification.AvailabilityMatches(buyer,"node.double_strike_power.02"), "later tiers unavailable");
                var poor = Session(Decode(Signed(5, "349.99", Ranks()))); poor.TryQuoteUpgrade(ids[0], out var cost); Check(!poor.TryPurchaseUpgrade(cost) && poor.Rewards.Wallet == 349.99m, "insufficient wallet");
                foreach (var pair in new[] { new[] { .999, .999 }, new[] { 0d, .999 }, new[] { .999, 0d }, new[] { 0d, 0d } })
                {
                    var rolls = new Queue<double>(pair); int proc = 0; var s = Session(1, 1, () => rolls.Dequeue(), () => { proc++; return 0; }); var order = Begin(s, location);
                    s.TryCookClick(order); int expected = (pair[0] == 0 ? 13 : 6) + (pair[1] == 0 ? 7 : 4);
                    Check(order.RemainingClicks == 1000 - expected && s.LastHitDamage == expected && s.LastExtraStrikeDamage == (pair[1] == 0 ? 7 : 4) && s.LastStrikeCount == 2 && s.LastHitWasCritical == pair.Any(x => x == 0) && rolls.Count == 0 && proc == 1, "independent crits, single proc and one rounding"); s.EndDay();
                }
                foreach (var boundary in new[] { new { rank = 1, roll = .019999999d, extra = true }, new { rank = 1, roll = .02d, extra = false }, new { rank = 10, roll = .199999999d, extra = true }, new { rank = 10, roll = .2d, extra = false }, new { rank = 1, roll = .999999999d, extra = false } })
                {
                    var s = Session(boundary.rank, 0, () => throw new Exception("zero crit RNG"), () => boundary.roll, 0); var order = Begin(s, location); s.TryCookClick(order);
                    Check(s.LastStrikeCount == (boundary.extra ? 2 : 1) && order.RemainingClicks == (boundary.extra ? 991 : 994), "proc threshold/baseline strength"); s.EndDay();
                }
                var zero = Session(0, 0, () => throw new Exception("zero crit"), () => throw new Exception("zero proc"), 0); var zeroOrder = Begin(zero, location); zero.TryCookClick(zeroOrder); Check(zero.LastHitDamage == 6 && zero.LastStrikeCount == 1, "zero chance consumes no RNG"); zero.EndDay();
                foreach (int hp in new[] { 13, 14, 20 })
                {
                    SetDish(dish, hp); int crits = 0, procs = 0, changes = 0; var s = Session(1, 1, () => { crits++; return 0; }, () => { procs++; return 0; }); var order = Begin(s, location); s.Profile.Changed += () => changes++;
                    decimal before = s.Rewards.Wallet; s.TryBeginHold(order, 0); Check(s.TryCookClick(order), "bounded completion");
                    Check(crits == (hp == 13 ? 1 : 2) && procs == (hp == 13 ? 0 : 1) && changes == 1 && s.CompletedDishes == 1 && order.RemainingClicks == 0 && !s.IsHolding && s.ActiveOrder == null && s.Rewards.Wallet == before + s.LastResult.Payout && s.Profile.CaptureSnapshot().DishXpById[1] == 50 + s.LastResult.XpGained, "lethal primary suppression/extra single receipt");
                    decimal paid = s.Rewards.Wallet; Check(!s.TryCookClick(order) && !s.TryTakeHoldRepeat(1) && s.Rewards.Wallet == paid && changes == 1, "stale result/repeat no extra reward");
                    s.TrySelect(0); Check(s.ActiveOrder.RemainingClicks == hp && s.LastStrikeCount == 0, "replacement no carry/feedback"); s.EndDay();
                }
                SetDish(dish, 1000); int calls = 0; var hold = Session(1, 1, () => 0, () => { calls++; return 0; }); var active = Begin(hold, location);
                Check(!hold.TryCookClick(hold.GetOrderAtSlot(1)) && calls == 0, "foreign order consumes no RNG"); hold.IsPaused = true; Check(!hold.TryCookClick(active) && calls == 0, "paused input"); hold.IsPaused = false;
                hold.TryBeginHold(active, 0); hold.TryCookClick(active); Check(hold.TryTakeHoldRepeat(1d / 4.5), "frequency repeat"); hold.TryCookClick(active); Check(active.RemainingClicks == 960 && calls == 2, "one double transaction per Hold trigger");
                hold.ReturnToBoard(); Check(!hold.TryCookClick(active) && !hold.TryTakeHoldRepeat(2) && calls == 2, "Back/stale Hold no RNG"); hold.EndDay();
                return "U3 model PASS: prices/caps/guards, independent crit combinations/rounding, proc boundaries, no chain/carry, lethal suppression, reward once and Hold.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U3Store/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 4; version++)
            {
                string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks().Take(version + 1).ToArray(); string old = Signed(version, "1000.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old);
                var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot();
                Check(p != null && File.ReadAllText(path) == old && p.GetUpgradeRank(UpgradeNodeRules.DoubleStrikeChance) == 0 && p.GetUpgradeRank(UpgradeNodeRules.DoubleStrikePower) == 0, "old schema no free ranks/rewrite");
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "current schema / old backup");
                var after = new PlayerProfileStore(path).Load().CaptureSnapshot(); Check(after.Wallet == 1000.25m && after.DishXpById[1] == 50 && before.UpgradeRanksById.Count == after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value), "exact old state");
                File.WriteAllText(path, "broken"); var recovered = new PlayerProfileStore(path); Check(recovered.Load() != null && recovered.Status == ProfileLoadStatus.Recovered, "old backup recovery");
                foreach (var id in new[] { UpgradeNodeRules.DoubleStrikeChance, UpgradeNodeRules.DoubleStrikePower }) Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(id, 0) }).ToArray())) == null, "old schema rejects new ID even 0");
            }
            foreach (int rank in new[] { 1, 10 })
            {
                var p = Decode(Signed(5, "250.25", Ranks(rank, rank))); string path = Path.Combine(folder, "rank" + rank + ".json"); Check(new PlayerProfileStore(path).Save(p), "write owned");
                var s = Session(new PlayerProfileStore(path).Load()); Check(s.DoubleStrikeChanceRank == rank && s.DoubleStrikePowerRank == rank && s.Rewards.Wallet == 250.25m && s.HoldFrequencyRank == 1 && s.CriticalMultiplierRank == 1 && !s.IsDayActive && s.LastStrikeCount == 0, "roundtrip all ranks idle");
            }
            foreach (var ranks in new[] { Ranks(-1), Ranks(11), Ranks(1, -1), Ranks(1, 11), Ranks(0, 1), new[] { Rank(UpgradeNodeRules.Strength, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, 1) }, Ranks(1).Concat(new[] { Rank(UpgradeNodeRules.DoubleStrikeChance, 1) }).ToArray(), Ranks(1, 1).Concat(new[] { Rank(UpgradeNodeRules.DoubleStrikePower, 1) }).ToArray(), Ranks().Concat(new[] { Rank("node.double_strike_chance.02", 1) }).ToArray(), Ranks().Concat(new[] { Rank("double_strike_power", 1) }).ToArray() }) Check(Decode(Signed(5, "1000", ranks)) == null, "invalid cap/duplicate/prerequisite/tier/alias");
            string futurePath = Path.Combine(folder, "future.json"), future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()); File.WriteAllText(futurePath, future); var futureStore = new PlayerProfileStore(futurePath);
            Check(futureStore.Load() == null && futureStore.Status == ProfileLoadStatus.Unsupported && !futureStore.Save(Decode(Signed(5, "1000", Ranks()))) && File.ReadAllText(futurePath) == future, "future protection");
            return "U3 storage PASS: schemas 1–4 → 5, exact old state/backups, both node caps/prerequisites/duplicates, roundtrip and version 6 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static CounterDishPointer Pointer => UnityEngine.Object.FindObjectsByType<CounterDishPointer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        private static DishData playDish; private static LocationData playLocation;
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying, "prepare stopped"); string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U3-" + Guid.NewGuid().ToString("N"), "profile.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(4, "1000.25", Ranks().Take(5).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path;
        }
        private static void Buy(string id, decimal expectedWallet)
        {
            var c = Controller; c.SelectUpgradeNode(id); var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var button = Field<UnityEngine.UI.Button>(c, "buyUpgradeButton");
            Check(button.interactable && detail.text.Contains("0 / 10"), "buyable detail " + id); button.onClick.Invoke(); button.onClick.Invoke();
            var p = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(p.GetUpgradeRank(id) == 1 && p.Rewards.Wallet == expectedWallet && c.OrderSession.Rewards.Wallet == expectedWallet, "one debit/autosave " + id);
        }
        public static string BuyChance()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U3-"), "isolated profile"); var c = Controller; c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.DoubleStrikePower); Check(!Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "power locked before chance");
            Buy(UpgradeNodeRules.DoubleStrikeChance, 650.25m); return "U3 chance purchase/autosave PASS; advance frame before power.";
        }
        public static string BuyPower() { Buy(UpgradeNodeRules.DoubleStrikePower, 250.25m); return "U3 power purchase/autosave PASS; advance frame for refresh."; }
        public static string StartCooking()
        {
            var c = Controller; var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases();
            Check(detail.text.Contains("1 / 10") && detail.text.Contains(ClickCookUiCopy.Money(680m)) && detail.text.Contains($"×{.55m:0.##}") && stats.text.Contains("Подвійний: 2%") && stats.text.Contains($"Сила повтору: ×{.55m:0.##}") && stats.preferredHeight <= stats.rectTransform.rect.height + 1, "preview and actual statistics fit");
            c.CloseUpgrades(); var s = Session(c.OrderSession.Profile, () => 0, () => 0); typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation();
            playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation); SetDish(playDish, 1000); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); c.SelectOrder(0); return "U3 fixture ready; advance rendered frame.";
        }
        public static string CheckInputAndFinish()
        {
            var c = Controller; var s = c.OrderSession; var pointer = Pointer; Check(pointer.isActiveAndEnabled, "actual pointer"); var e = new PointerEventData(EventSystem.current) { pointerId = -1 };
            pointer.OnPointerDown(e); pointer.OnPointerUp(e); pointer.OnPointerClick(e); Check(s.ActiveOrder.RemainingClicks == 980 && s.LastStrikeCount == 2 && !s.IsHolding, "one pointer trigger/no release hit");
            var hp = Field<UnityEngine.UI.Text>(c, "cookingHpText"); Canvas.ForceUpdateCanvases(); Check(hp.text.Contains("2 рухи: −20 HP") && hp.text.Contains("крит") && hp.preferredHeight <= hp.rectTransform.rect.height + 1 && hp.preferredWidth <= hp.rectTransform.rect.width + 1, "combined critical feedback fits");
            pointer.OnSubmit(new BaseEventData(EventSystem.current)); Check(s.ActiveOrder.RemainingClicks == 960, "Submit shares bounded transaction");
            var order = s.ActiveOrder; typeof(PrototypeOrder).GetProperty("RemainingClicks").SetValue(order, 14); decimal before = s.Rewards.Wallet; pointer.OnPointerDown(e); pointer.OnPointerUp(e); pointer.OnPointerClick(e); pointer.OnSubmit(new BaseEventData(EventSystem.current));
            Check(order.IsCompleted && s.CompletedDishes == 1 && !s.IsHolding && s.Rewards.Wallet == before + s.LastResult.Payout, "lethal extra/result stale events reward once");
            var saved = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(saved.Rewards.Wallet == s.Rewards.Wallet && saved.GetUpgradeRank(UpgradeNodeRules.DoubleStrikeChance) == 1 && saved.GetUpgradeRank(UpgradeNodeRules.DoubleStrikePower) == 1, "completion autosave retains nodes");
            SessionState.SetString("ClickCook.U3ExpectedWallet", s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture)); SessionState.SetInt("ClickCook.U3ExpectedXp", s.Profile.CaptureSnapshot().DishXpById[1]);
            c.ContinueFromDishResult(); c.BackToBoard(); c.FinishDay(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.DoubleStrikePower); return "U3 live pointer/Submit/feedback/completion/autosave PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U3-"), "isolated restart"); var c = Controller; var s = c.OrderSession;
            Check(s.DoubleStrikeChanceRank == 1 && s.DoubleStrikePowerRank == 1 && s.ClickStrengthRank == 3 && s.CriticalChanceRank == 1 && s.CriticalMultiplierRank == 1 && s.HoldFrequencyRank == 1 && s.HasHold && !s.IsDayActive && !s.IsHolding && s.LastStrikeCount == 0 && s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture) == SessionState.GetString("ClickCook.U3ExpectedWallet", "") && s.Profile.CaptureSnapshot().DishXpById[1] == SessionState.GetInt("ClickCook.U3ExpectedXp", -1), "all ranks/wallet/XP/idle restored");
            c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.DoubleStrikeChance); SessionState.EraseString("ClickCook.U3ExpectedWallet"); SessionState.EraseInt("ClickCook.U3ExpectedXp"); return "U3 restart/Continue PASS.";
        }
    }
}
