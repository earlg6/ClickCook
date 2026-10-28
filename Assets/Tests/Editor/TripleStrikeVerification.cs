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
    /// <summary>U3b node-specific prices, exclusive proc distribution and schema boundary.</summary>
    public static class TripleStrikeVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U3b: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create(); return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int tier = 0, int triple = 0, int first = 10) => new[] { Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1), Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1), Rank(UpgradeNodeRules.DoubleStrikeChance, first), Rank(UpgradeNodeRules.DoubleStrikePower, 1), Rank(UpgradeNodeRules.DoubleStrikeChanceII, tier), Rank(UpgradeNodeRules.TripleStrikeChance, triple) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile p, Func<double> crit = null, Func<double> proc = null)
        { var s = new PrototypeOrderSession(p, crit, proc); Check(s.ConfigureUpgrades(Catalog), "configure"); return s; }
        private static PrototypeOrderSession Session(int tier, int triple, Func<double> crit = null, Func<double> proc = null) => Session(Decode(Signed(6, "10000000", Ranks(tier, triple))), crit, proc);
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
                SetDish(dish, 1000); SetPool(location, dish); var locked = Session(Decode(Signed(6, "10000000", Ranks(0, 0, 9))));
                Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.DoubleStrikeChanceII, out _) && !locked.TryQuoteUpgrade(UpgradeNodeRules.TripleStrikeChance, out _), "purchased prerequisites");
                var buyer = Session(0, 0); Check(!buyer.TryQuoteUpgrade(UpgradeNodeRules.TripleStrikeChance, out _), "triple locked until II 1");
                decimal[][] prices = { new decimal[] { 50000, 80000, 128000, 204800, 327680 }, new decimal[] { 10000, 18000, 32400, 58320, 104976, 188957, 340123, 612221, 1101997, 1983593 } };
                string[] ids = { UpgradeNodeRules.DoubleStrikeChanceII, UpgradeNodeRules.TripleStrikeChance }; decimal spent = 0;
                for (int node = 0; node < 2; node++) for (int n = 0; n < prices[node].Length; n++)
                {
                    Check(buyer.TryQuoteUpgrade(ids[node], out var q) && q.Price == prices[node][n], "own price sequence " + ids[node] + n);
                    buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false; buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "day purchase"); buyer.EndDay();
                    Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += q.Price;
                }
                Check(buyer.Rewards.Wallet == 10000000m - spent && buyer.DoubleStrikeChanceRank == 10 && buyer.DoubleStrikeChanceIIRank == 5 && buyer.TripleStrikeChanceRank == 10 && buyer.DoubleStrikeChance == .3 && buyer.TripleStrikeChance == .1 && buyer.AdditionalStrikeChance == .4 && !buyer.TryQuoteUpgrade(ids[0], out _) && !buyer.TryQuoteUpgrade(ids[1], out _), "independent caps, contributions, exact debit");
                foreach (var node in UpgradeTreeDefinition.Load().nodes.Where(x => x.IsWorking)) Check(Catalog.FindForNode(node.id).MaximumRank == UpgradeNodeRules.MaximumRank(node.id), "matching node/catalog cap " + node.id);
                Check(Catalog.FindForNode(UpgradeNodeRules.DoubleStrikeChance).PriceForNextRank(0) == 350 && Catalog.FindForNode(UpgradeNodeRules.DoubleStrikeChanceII).PriceForNextRank(0) == 50000, "family identity does not share tier price");
                var poor = Session(Decode(Signed(6, "49999.99", Ranks()))); poor.TryQuoteUpgrade(ids[0], out var expensive); Check(!poor.TryPurchaseUpgrade(expensive) && poor.Rewards.Wallet == 49999.99m, "insufficient exact wallet");
                foreach (var id in new[] { "node.double_strike_chance.03", "node.triple_strike_chance.02", "node.double_strike_power.02" }) Check(UpgradeBatchVerification.AvailabilityMatches(buyer,id), "later tier availability contract");
                foreach (var b in new[] { new { u = 0d, count = 2 }, new { u = .219999999d, count = 2 }, new { u = .22d, count = 3 }, new { u = .229999999d, count = 3 }, new { u = .23d, count = 1 }, new { u = .999999999d, count = 1 } })
                {
                    int proc = 0, crit = 0; var s = Session(1, 1, () => { crit++; return .999; }, () => { proc++; return b.u; }); var order = Begin(s, location); s.TryCookClick(order);
                    Check(s.LastStrikeCount == b.count && proc == 1 && crit == b.count && order.RemainingClicks == 1000 - 6 - (b.count - 1) * 4 && s.LastExtraStrikeDamage == (b.count - 1) * 4, "exclusive threshold " + b.u); s.EndDay();
                }
                foreach (var b in new[] { new { u = .299999999d, count = 2 }, new { u = .3d, count = 3 }, new { u = .399999999d, count = 3 }, new { u = .4d, count = 1 } })
                { var s = Session(5, 10, () => .999, () => b.u); var order = Begin(s, location); s.TryCookClick(order); Check(s.LastStrikeCount == b.count, "cap distribution boundary"); s.EndDay(); }
                for (int mask = 0; mask < 8; mask++)
                {
                    var crits = new Queue<double>(Enumerable.Range(0, 3).Select(i => (mask & (1 << i)) != 0 ? 0d : .999d)); int procs = 0; var s = Session(1, 1, () => crits.Dequeue(), () => { procs++; return .22; }); var order = Begin(s, location); s.TryCookClick(order);
                    int damage = ((mask & 1) != 0 ? 13 : 6) + ((mask & 2) != 0 ? 7 : 4) + ((mask & 4) != 0 ? 7 : 4);
                    Check(s.LastStrikeCount == 3 && s.LastHitDamage == damage && order.RemainingClicks == 1000 - damage && s.LastHitWasCritical == (mask != 0) && crits.Count == 0 && procs == 1, "three independent crits / single rounding " + mask); s.EndDay();
                }
                foreach (int hp in new[] { 13, 14, 21, 27 })
                {
                    SetDish(dish, hp); int crit = 0, proc = 0, changes = 0; var s = Session(1, 1, () => { crit++; return 0; }, () => { proc++; return .22; }); var order = Begin(s, location); s.Profile.Changed += () => changes++; decimal before = s.Rewards.Wallet; s.TryBeginHold(order, 0);
                    Check(s.TryCookClick(order) && s.LastStrikeCount == (hp == 13 ? 1 : hp == 14 ? 2 : 3) && crit == s.LastStrikeCount && proc == (hp == 13 ? 0 : 1) && changes == 1 && order.RemainingClicks == 0 && s.CompletedDishes == 1 && !s.IsHolding && s.ActiveOrder == null && s.Rewards.Wallet == before + s.LastResult.Payout && s.Profile.CaptureSnapshot().DishXpById[1] == 50 + s.LastResult.XpGained, "lethal suppression/reward once " + hp);
                    decimal paid = s.Rewards.Wallet; Check(!s.TryCookClick(order) && !s.TryTakeHoldRepeat(10) && s.Rewards.Wallet == paid && changes == 1, "stale result"); s.TrySelect(0); Check(s.ActiveOrder.RemainingClicks == hp && s.LastStrikeCount == 0, "replacement no carry"); s.EndDay();
                }
                SetDish(dish, 1000); int calls = 0; var hold = Session(1, 1, () => 0, () => { calls++; return .22; }); var active = Begin(hold, location);
                Check(!hold.TryCookClick(hold.GetOrderAtSlot(1)) && calls == 0, "foreign order"); hold.IsPaused = true; Check(!hold.TryCookClick(active) && calls == 0, "paused input"); hold.IsPaused = false;
                hold.TryBeginHold(active, 0); hold.TryCookClick(active); Check(hold.TryTakeHoldRepeat(1d / 4.5), "Hold repeat"); hold.TryCookClick(active); Check(active.RemainingClicks == 946 && calls == 2, "one triple transaction per Hold trigger"); hold.ReturnToBoard(); Check(!hold.TryCookClick(active) && !hold.TryTakeHoldRepeat(2) && calls == 2, "Back no stale proc"); hold.EndDay();
                return "U3b model PASS: node prices/caps, exclusive distribution boundaries, eight crit combinations, no chain/carry, lethal suppression, one reward, Hold and guards.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U3bStore/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 5; version++)
            {
                string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks().Take(version == 5 ? 7 : version + 1).ToArray(); string old = Signed(version, "100000.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old);
                var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot(); Check(File.ReadAllText(path) == old && p.GetUpgradeRank(UpgradeNodeRules.DoubleStrikeChanceII) == 0 && p.GetUpgradeRank(UpgradeNodeRules.TripleStrikeChance) == 0, "old load no gifts/rewrite");
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "schema 6/old backup"); var after = new PlayerProfileStore(path).Load().CaptureSnapshot(); Check(after.Wallet == 100000.25m && after.DishXpById[1] == 50 && before.UpgradeRanksById.Count == after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value), "exact legacy state");
                File.WriteAllText(path, "broken"); var recovery = new PlayerProfileStore(path); Check(recovery.Load() != null && recovery.Status == ProfileLoadStatus.Recovered, "old backup recovery");
                foreach (var id in new[] { UpgradeNodeRules.DoubleStrikeChanceII, UpgradeNodeRules.TripleStrikeChance }) Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(id, 0) }).ToArray())) == null, "old schema rejects even rank 0");
            }
            foreach (var rank in new[] { new { ii = 1, triple = 1 }, new { ii = 5, triple = 10 } })
            { var p = Decode(Signed(6, "40000.25", Ranks(rank.ii, rank.triple))); string path = Path.Combine(folder, "r" + rank.ii + ".json"); Check(new PlayerProfileStore(path).Save(p), "save owned"); var s = Session(new PlayerProfileStore(path).Load()); Check(s.DoubleStrikeChanceIIRank == rank.ii && s.TripleStrikeChanceRank == rank.triple && s.DoubleStrikeChanceRank == 10 && s.HoldFrequencyRank == 1 && s.Rewards.Wallet == 40000.25m && !s.IsHolding && !s.IsDayActive && s.LastStrikeCount == 0, "owned roundtrip/independent ranks/idle"); }
            foreach (var ranks in new[] { Ranks(-1), Ranks(6), Ranks(1, -1), Ranks(1, 11), Ranks(0, 1), Ranks(1, 0, 9), Ranks(1, 1).Concat(new[] { Rank(UpgradeNodeRules.DoubleStrikeChanceII, 1) }).ToArray(), Ranks(1, 1).Concat(new[] { Rank(UpgradeNodeRules.TripleStrikeChance, 1) }).ToArray(), Ranks().Concat(new[] { Rank("node.triple_strike_chance.02", 1) }).ToArray(), Ranks().Concat(new[] { Rank("triple_strike_chance", 1) }).ToArray() }) Check(Decode(Signed(6, "1000", ranks)) == null, "invalid bounds/prerequisites/duplicates/tier/alias");
            string future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()), futurePath = Path.Combine(folder, "future.json"); File.WriteAllText(futurePath, future); var blocked = new PlayerProfileStore(futurePath); Check(blocked.Load() == null && blocked.Status == ProfileLoadStatus.Unsupported && !blocked.Save(Decode(Signed(6, "1000", Ranks()))) && File.ReadAllText(futurePath) == future, "future protected");
            return "U3b storage PASS: schema 1–5 → 6, exact old state/backups, independent node bounds/prerequisites, roundtrip and version 7 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static CounterDishPointer Pointer => UnityEngine.Object.FindObjectsByType<CounterDishPointer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        private static DishData playDish; private static LocationData playLocation;
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying, "prepare stopped"); string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U3b-" + Guid.NewGuid().ToString("N"), "profile.json")); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(5, "100000.25", Ranks().Take(7).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path;
        }
        private static void Buy(string id, decimal price, decimal wallet, int cap)
        {
            var c = Controller; c.SelectUpgradeNode(id); var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var buy = Field<UnityEngine.UI.Button>(c, "buyUpgradeButton"); Check(buy.interactable && detail.text.Contains("0 / " + cap) && detail.text.Contains(ClickCookUiCopy.Money(price)), "own live price/cap"); buy.onClick.Invoke(); buy.onClick.Invoke(); var saved = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(saved.Rewards.Wallet == wallet && saved.GetUpgradeRank(id) == 1 && c.OrderSession.Rewards.Wallet == wallet, "one debit/autosave");
        }
        public static string BuyTierII()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U3b-"), "isolated save"); var c = Controller; c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.TripleStrikeChance); Check(!Field<UnityEngine.UI.Button>(c, "buyUpgradeButton").interactable, "triple locked"); Buy(UpgradeNodeRules.DoubleStrikeChanceII, 50000, 50000.25m, 5); return "U3b tier II purchase/autosave PASS; advance frame.";
        }
        public static string BuyTriple() { Buy(UpgradeNodeRules.TripleStrikeChance, 10000, 40000.25m, 10); return "U3b triple purchase/autosave PASS; advance frame."; }
        public static string StartCooking()
        {
            var c = Controller; var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var stats = Field<UpgradeTreeView>(c, "upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases(); Check(detail.text.Contains("1 / 10") && detail.text.Contains(ClickCookUiCopy.Money(18000)) && stats.text.Contains("Подвійний: 22% · потрійний: 1%") && stats.preferredHeight <= stats.rectTransform.rect.height + 1 && stats.preferredWidth <= stats.rectTransform.rect.width + 1, "triple preview and aggregate statistics fit");
            c.SelectUpgradeNode(UpgradeNodeRules.DoubleStrikeChanceII); detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); Check(detail.text.Contains("1 / 5") && detail.text.Contains(ClickCookUiCopy.Money(80000)) && detail.text.Contains("+2 п.п."), "tier II own next price/cap/contribution");
            c.CloseUpgrades(); var s = Session(c.OrderSession.Profile, () => 0, () => .22); typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation(); playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation); SetDish(playDish, 1000); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); c.SelectOrder(0); return "U3b cooking fixture ready; advance rendered frame.";
        }
        public static string CheckInputAndFinish()
        {
            var c = Controller; var s = c.OrderSession; Check(Pointer.isActiveAndEnabled, "actual pointer"); var e = new PointerEventData(EventSystem.current) { pointerId = -1 }; Pointer.OnPointerDown(e); Pointer.OnPointerUp(e); Pointer.OnPointerClick(e); Check(s.ActiveOrder.RemainingClicks == 973 && s.LastStrikeCount == 3 && s.LastExtraStrikeDamage == 14 && !s.IsHolding, "one triple pointer transaction");
            var hp = Field<UnityEngine.UI.Text>(c, "cookingHpText"); Canvas.ForceUpdateCanvases(); Check(hp.text.Contains("3 рухи: −27 HP") && hp.text.Contains("крит") && hp.preferredHeight <= hp.rectTransform.rect.height + 1 && hp.preferredWidth <= hp.rectTransform.rect.width + 1, "triple critical feedback fits"); Pointer.OnSubmit(new BaseEventData(EventSystem.current)); Check(s.ActiveOrder.RemainingClicks == 946, "Submit triple");
            var order = s.ActiveOrder; typeof(PrototypeOrder).GetProperty("RemainingClicks").SetValue(order, 21); decimal before = s.Rewards.Wallet; Pointer.OnPointerDown(e); Pointer.OnPointerUp(e); Pointer.OnPointerClick(e); Pointer.OnSubmit(new BaseEventData(EventSystem.current)); Check(order.IsCompleted && s.LastStrikeCount == 3 && s.CompletedDishes == 1 && !s.IsHolding && s.Rewards.Wallet == before + s.LastResult.Payout, "third strike completes / stale result events do not pay");
            var saved = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(saved.GetUpgradeRank(UpgradeNodeRules.DoubleStrikeChanceII) == 1 && saved.GetUpgradeRank(UpgradeNodeRules.TripleStrikeChance) == 1 && saved.Rewards.Wallet == s.Rewards.Wallet, "completion autosave"); SessionState.SetString("ClickCook.U3bWallet", s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture)); SessionState.SetInt("ClickCook.U3bXp", s.Profile.CaptureSnapshot().DishXpById[1]);
            c.ContinueFromDishResult(); c.BackToBoard(); c.FinishDay(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.TripleStrikeChance); return "U3b pointer/Submit/feedback/completion/autosave PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U3b-"), "isolated restart"); var c = Controller; var s = c.OrderSession; Check(s.DoubleStrikeChanceRank == 10 && s.DoubleStrikeChanceIIRank == 1 && s.TripleStrikeChanceRank == 1 && s.DoubleStrikePowerRank == 1 && s.ClickStrengthRank == 3 && s.CriticalChanceRank == 1 && s.CriticalMultiplierRank == 1 && s.HasHold && s.HoldFrequencyRank == 1 && !s.IsHolding && !s.IsDayActive && s.LastStrikeCount == 0 && s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture) == SessionState.GetString("ClickCook.U3bWallet", "") && s.Profile.CaptureSnapshot().DishXpById[1] == SessionState.GetInt("ClickCook.U3bXp", -1), "all nine ranks/wallet/XP/idle");
            c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.TripleStrikeChance); SessionState.EraseString("ClickCook.U3bWallet"); SessionState.EraseInt("ClickCook.U3bXp"); return "U3b restart/Continue PASS.";
        }
    }
}
