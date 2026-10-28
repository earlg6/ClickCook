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
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    /// <summary>Scoped U2c checks. Every written profile is under project Temp.</summary>
    public static class HoldFrequencyVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U2c: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, string wallet, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet + "\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create();
            return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int frequency = 0) => new[] { Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1), Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, frequency) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile profile)
        { var session = new PrototypeOrderSession(profile, () => 0); Check(session.ConfigureUpgrades(Catalog), "catalog"); return session; }
        private static void SetPool(LocationData location, DishData dish)
        {
            var data = new SerializedObject(dish); data.FindProperty("clickRequirement").intValue = 1000; data.ApplyModifiedPropertiesWithoutUndo();
            data = new SerializedObject(location); var pool = data.FindProperty("documentedAvailableDishes"); pool.arraySize = 1; pool.GetArrayElementAtIndex(0).objectReferenceValue = dish; data.ApplyModifiedPropertiesWithoutUndo();
        }

        public static string RunModelChecks()
        {
            var random = UnityEngine.Random.state; var dish = ScriptableObject.CreateInstance<DishData>(); var location = ScriptableObject.CreateInstance<LocationData>();
            try
            {
                SetPool(location, dish);
                var locked = Session(Decode(Signed(4, "100000", Rank(UpgradeNodeRules.Strength, 3))));
                Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.HoldFrequency, out _) && locked.HoldFrequency == 0, "Hold required");
                var poor = Session(Decode(Signed(4, "299.99", Ranks()))); poor.TryQuoteUpgrade(UpgradeNodeRules.HoldFrequency, out var costly);
                Check(!poor.TryPurchaseUpgrade(costly) && poor.Rewards.Wallet == 299.99m && poor.HoldFrequencyRank == 0, "insufficient exact wallet");
                var s = Session(Decode(Signed(4, "100000", Ranks())));
                decimal[] prices = { 300, 480, 768, 1229, 1967, 3146, 5034, 8054, 12885, 20616 };
                for (int rank = 0; rank <= 10; rank++)
                {
                    Check(s.HoldFrequency == 4m + .5m * rank, "effective rate " + rank);
                    s.StartDay(location, 2); s.TrySelect(0); var order = s.ActiveOrder;
                    double interval = 1d / (double)(4m + .5m * rank);
                    Check(s.TryBeginHold(order, 0) && !s.TryTakeHoldRepeat(interval - 1e-9), "not early " + rank);
                    Check(s.TryTakeHoldRepeat(interval) && !s.TryTakeHoldRepeat(interval), "exact deadline once " + rank);
                    int hp = order.RemainingClicks; s.TryCookClick(order);
                    Check(order.RemainingClicks == hp - 13 && s.LastHitWasCritical, "frequency preserves critical damage " + rank);
                    Check(s.TryTakeHoldRepeat(100) && !s.TryTakeHoldRepeat(100) && !s.TryTakeHoldRepeat(100 + interval - 1e-9), "no backlog " + rank);
                    s.IsPaused = true; s.IsPaused = false; Check(!s.TryTakeHoldRepeat(200), "pause cancels " + rank); s.EndDay();
                    if (rank == 10) break;
                    Check(s.TryQuoteUpgrade(UpgradeNodeRules.HoldFrequency, out var quote) && quote.Price == prices[rank], "price " + rank);
                    s.IsPaused = true; Check(!s.TryPurchaseUpgrade(quote), "pause blocks purchase"); s.IsPaused = false;
                    s.StartDay(location, 1); Check(!s.TryPurchaseUpgrade(quote), "day blocks purchase"); s.EndDay();
                    Check(!locked.TryPurchaseUpgrade(quote) && s.TryPurchaseUpgrade(quote) && !s.TryPurchaseUpgrade(quote), "owner/purchase/replay " + rank);
                }
                Check(s.Rewards.Wallet == 45521m && !s.TryQuoteUpgrade(UpgradeNodeRules.HoldFrequency, out _), "ten rank cap and exact total");
                var tree = UpgradeTreeDefinition.Load();
                Check(tree.PrerequisitesMet(tree.Find("node.hold_frequency.02"), s.Profile.GetUpgradeRank) && UpgradeBatchVerification.AvailabilityMatches(s,"node.hold_frequency.02"), "tier 2 prerequisite met but effect unavailable");
                Check(s.ClickStrengthRank == 3 && s.CriticalChanceRank == 1 && s.CriticalMultiplierRank == 1 && s.HasHold, "independent ranks");
                return "U2c model PASS: all ten prices/rates/deadlines, prerequisite/cap/debit, no backlog, critical repeat and purchase guards.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/U2cStore/" + Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for (int version = 1; version <= 3; version++)
            {
                string[] ranks = version == 1 ? new[] { Rank("click_strength", 3) } : Ranks().Take(version == 2 ? 3 : 4).ToArray();
                string old = Signed(version, "1000.25", ranks), path = Path.Combine(folder, "v" + version + ".json"); File.WriteAllText(path, old);
                var store = new PlayerProfileStore(path); var p = store.Load(); var before = p.CaptureSnapshot();
                Check(File.ReadAllText(path) == old && p.GetUpgradeRank(UpgradeNodeRules.HoldFrequency) == 0 && before.Wallet == 1000.25m && before.DishXpById[1] == 50, "old load unchanged " + version);
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version == PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath) == old, "write current / old backup " + version);
                var after = new PlayerProfileStore(path).Load().CaptureSnapshot();
                Check(before.Wallet == after.Wallet && before.UpgradeRanksById.All(x => after.UpgradeRanksById[x.Key] == x.Value) && after.DishXpById[1] == 50, "retain exact state " + version);
                File.WriteAllText(path, "broken"); var recovery = new PlayerProfileStore(path); Check(recovery.Load() != null && recovery.Status == ProfileLoadStatus.Recovered, "old backup recovery");
                Check(Decode(Signed(version, "1000", ranks.Concat(new[] { Rank(UpgradeNodeRules.HoldFrequency, 0) }).ToArray())) == null, "old schema rejects frequency even rank 0");
            }
            foreach (int rank in new[] { 1, 10 })
            {
                var p = Decode(Signed(4, "700.25", Ranks(rank))); string path = Path.Combine(folder, "rank" + rank + ".json");
                Check(new PlayerProfileStore(path).Save(p), "save owned frequency"); var s = Session(new PlayerProfileStore(path).Load());
                Check(s.HoldFrequencyRank == rank && s.HoldFrequency == 4m + .5m * rank && s.Rewards.Wallet == 700.25m && !s.IsHolding && !s.IsDayActive, "roundtrip frequency and idle state");
            }
            foreach (var ranks in new[] { Ranks(-1), Ranks(11), Ranks(1).Concat(new[] { Rank(UpgradeNodeRules.HoldFrequency, 1) }).ToArray(), Ranks(1).Where(x => !x.Contains(UpgradeNodeRules.Hold + "\"")).ToArray(), Ranks().Concat(new[] { Rank("node.hold_frequency.02", 1) }).ToArray(), new[] { Rank(UpgradeNodeRules.Strength, 3), Rank("hold_frequency", 1) } })
                Check(Decode(Signed(4, "1000", ranks)) == null, "invalid bounds/duplicates/prerequisite/tier/alias");
            string futurePath = Path.Combine(folder, "future.json"), future = Signed(PlayerProfileStore.CurrentVersion + 1, "1000", Ranks()); File.WriteAllText(futurePath, future);
            var futureStore = new PlayerProfileStore(futurePath); Check(futureStore.Load() == null && futureStore.Status == ProfileLoadStatus.Unsupported && !futureStore.Save(Decode(Signed(4, "1000", Ranks()))) && File.ReadAllText(futurePath) == future, "future protected");
            return "U2c storage PASS: schema 1/2/3 → 4, exact state/backups, frequency bounds/prerequisites, idle roundtrip and version 5 protection.";
        }

        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static CounterDishPointer Pointer => UnityEngine.Object.FindObjectsByType<CounterDishPointer>(FindObjectsInactive.Include, FindObjectsSortMode.None).Single();
        private static DishData playDish; private static LocationData playLocation; private static int releasedHp;
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying, "prepare stopped"); string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/U2c-" + Guid.NewGuid().ToString("N"), "profile.json"));
            Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path, Signed(3, "1000.25", Ranks().Take(4).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path;
        }
        public static string BuyInPlay()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U2c-"), "isolated save"); var c = Controller; c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.HoldFrequency);
            var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var buy = Field<UnityEngine.UI.Button>(c, "buyUpgradeButton");
            Check(detail.text.Contains("0 / 10") && detail.text.Contains($"{4.5m:0.#} рухи/с") && detail.text.Contains(ClickCookUiCopy.Money(300m)) && buy.interactable, "current/next/price UI");
            buy.onClick.Invoke(); buy.onClick.Invoke(); Check(c.OrderSession.Rewards.Wallet == 700.25m && c.OrderSession.HoldFrequencyRank == 1, "single button debit");
            var saved = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(saved.GetUpgradeRank(UpgradeNodeRules.HoldFrequency) == 1 && saved.Rewards.Wallet == 700.25m, "immediate autosave"); return "U2c live purchase/autosave PASS; advance a frame for refreshed details.";
        }
        public static string StartCooking()
        {
            var c = Controller; var detail = Field<UnityEngine.UI.Text>(c, "upgradeDetailsText"); var view = Field<UpgradeTreeView>(c, "upgradeTreeView");
            var stats = view.transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases();
            Check(detail.text.Contains("1 / 10") && detail.text.Contains(ClickCookUiCopy.Money(480m)) && stats.text.Contains($"{4.5m:0.#} рухи/с") && stats.preferredHeight <= stats.rectTransform.rect.height + 1, "refreshed preview and actual statistics");
            c.CloseUpgrades(); var s = Session(c.OrderSession.Profile); typeof(ClickCookPrototypeController).GetField("session", Private).SetValue(c, s); c.CookSelectedLocation();
            playDish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation = UnityEngine.Object.Instantiate(s.SelectedLocation); SetPool(playLocation, playDish); s.StartDay(playLocation, 2); c.SelectOrder(0);
            return "U2c cooking fixture ready; advance a rendered frame.";
        }
        public static string BeginHold()
        {
            var c = Controller; var s = c.OrderSession; int hp = s.ActiveOrder.RemainingClicks;
            Check(Pointer.isActiveAndEnabled, "actual pointer active"); Pointer.OnPointerDown(new PointerEventData(EventSystem.current) { pointerId = -1 });
            Check(s.IsHolding && s.ActiveOrder.RemainingClicks == hp - 13 && Math.Abs(s.HoldRepeatInterval - 1d / 4.5d) < 1e-12, "initial critical hit/dynamic interval");
            return "U2c Hold active at 4.5/s; observe a real LateUpdate repeat.";
        }
        public static string ReleaseHold()
        {
            var s = Controller.OrderSession; Check(s.ActiveOrder.RemainingClicks < 987, "LateUpdate repeated"); int hp = s.ActiveOrder.RemainingClicks;
            var e = new PointerEventData(EventSystem.current) { pointerId = -1 }; Pointer.OnPointerUp(e); Pointer.OnPointerClick(e); releasedHp = hp;
            Check(!s.IsHolding && s.ActiveOrder.RemainingClicks == hp, "release adds no hit");
            Pointer.OnDeselect(new BaseEventData(EventSystem.current)); var hint = Field<UnityEngine.UI.Text>(Pointer, "inputHint"); Canvas.ForceUpdateCanvases();
            Check(hint.text.Contains($"{4.5m:0.#} рухи/с") && hint.preferredHeight <= hint.rectTransform.rect.height + 1, "counter hint rate and fit");
            return "U2c dynamic hint/release PASS; observe no later hit.";
        }
        public static string FinishPlay()
        {
            Check(Controller.OrderSession.ActiveOrder.RemainingClicks == releasedHp, "no post-release repeat"); var c = Controller; c.BackToBoard(); c.FinishDay();
            UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.HoldFrequency); return "U2c live controller repeat/release PASS.";
        }
        public static string VerifyRestart()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U2c-"), "isolated restart"); var c = Controller; var s = c.OrderSession;
            Check(s.HoldFrequencyRank == 1 && s.HoldFrequency == 4.5m && s.Rewards.Wallet == 700.25m && s.ClickStrengthRank == 3 && s.CriticalChanceRank == 1 && s.CriticalMultiplierRank == 1 && s.HasHold && !s.IsHolding && !s.IsDayActive && s.Profile.CaptureSnapshot().DishXpById[1] == 50, "Continue state/ranks/XP/idle");
            c.GetComponent<ProfileMenu>().Continue(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.HoldFrequency); return "U2c restart/Continue PASS: all state retained, Hold idle.";
        }
    }
}
