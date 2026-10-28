using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    public static class DaySummaryVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static void Check(bool value, string message) { if (!value) throw new Exception("8A: " + message); }
        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Private).GetValue(owner);
        private static void Set(object owner, string name, object value) => owner.GetType().GetField(name, Private).SetValue(owner, value);
        private static DishData Dish(int hp)
        {
            var d = ScriptableObject.CreateInstance<DishData>();
            var so = new SerializedObject(d);
            so.FindProperty("dishId").intValue = 1;
            so.FindProperty("clickRequirement").intValue = hp;
            so.FindProperty("saleValue").intValue = 5;
            so.ApplyModifiedPropertiesWithoutUndo(); return d;
        }
        private static void Pool(LocationData location, DishData dish)
        {
            var so = new SerializedObject(location); var pool = so.FindProperty("documentedAvailableDishes");
            pool.arraySize = 1; pool.GetArrayElementAtIndex(0).objectReferenceValue = dish;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
        public static string RunModelChecks()
        {
            var random = UnityEngine.Random.state;
            var dish = Dish(1); var partial = Dish(3); var location = ScriptableObject.CreateInstance<LocationData>();
            try
            {
                Pool(location, dish);
                var profile = (PlayerProfile)typeof(PlayerProfile).GetMethod("Restore", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,
                    new object[] { 123.25m, new Dictionary<int, int>(), new Dictionary<string, int> {
                        { UpgradeNodeRules.Strength, 1 }, { UpgradeNodeRules.DishXpFlat, 1 }, { UpgradeNodeRules.DishXpMultiplier, 1 } }, null });
                var s = new PrototypeOrderSession(profile);
                s.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset"));
                int changes = 0; profile.Changed += () => changes++;
                Check(!s.TryFinishDay(out _), "idle finish rejected");
                s.StartDay(location, 2); s.IsPaused = true;
                Check(!s.TryFinishDay(out _) && s.IsDayActive, "paused finish rejected"); s.IsPaused = false;
                Check(s.TryFinishDay(out var empty) && empty.Receipts.Count == 0 && empty.Earnings == 0 && empty.DishXp == 0 &&
                    profile.Rewards.Wallet == 123.25m && changes == 0 && !profile.HasActiveDay, "empty day releases owner without rewards");
                s.StartDay(location, 2);
                decimal earnings = 0, xp = 0;
                for (int i = 0; i < 10; i++)
                {
                    Check(s.TrySelect(0), "select duplicate dish"); var order = s.ActiveOrder;
                    Check(!s.TryFinishDay(out _), "cannot finish active cooking");
                    Check(s.TryCookClick(order), "complete receipt");
                    earnings += s.LastResult.Payout; xp += s.LastResult.XpGained;
                    Check(profile.Rewards.Wallet == (i < 9 ? 123.25m : 123.25m + earnings) &&
                        s.PendingDayEarnings == (i < 9 ? earnings : 0), "money pending until tenth receipt");
                    Check(!s.TryCookClick(order), "stale completion rejected");
                }
                decimal wallet = profile.Rewards.Wallet, totalXp = profile.Rewards.GetExactTotalXp(dish);
                Check(xp % 1m != 0 && changes == 10, "fractional receipts and one notification per completion");
                Check(s.TryFinishDay(out var full) && full.LimitReached && full.Location == location && full.Receipts.Count == 10 &&
                    full.Earnings == earnings && full.DishXp == xp && wallet == 123.25m + earnings && totalXp == xp,
                    "ten duplicate receipts summed exactly, no starting wallet in earnings");
                Check(!s.TryFinishDay(out _) && profile.Rewards.Wallet == wallet && changes == 10 && !s.IsDayActive && s.Orders.Count == 0,
                    "finish/replay does not pay or notify");
                bool immutable = false;
                try { ((IList<PrototypeDishResult>)full.Receipts).Clear(); } catch (NotSupportedException) { immutable = true; }
                Check(immutable, "receipt list read only");
                Pool(location, partial); s.StartDay(location, 1); Check(s.TrySelect(0), "partial select");
                Check(!s.TryCookClick(s.ActiveOrder), "partial work"); s.ReturnToBoard();
                Check(s.TryFinishDay(out var abandoned) && abandoned.Receipts.Count == 0 && abandoned.Earnings == 0 && abandoned.DishXp == 0 &&
                    profile.Rewards.Wallet == wallet && profile.Rewards.GetExactTotalXp(dish) == totalXp, "partial/Back never counted");
                Check(full.Receipts.Count == 10 && full.Earnings == earnings && full.DishXp == xp && empty.Receipts.Count == 0,
                    "reports detached across new days/cleanup");
                Pool(location, dish); s.StartDay(location, 1); s.TrySelect(0); s.TryCookClick(s.ActiveOrder); var receipt = s.LastResult;
                Check(s.TryFinishDay(out var early) && !early.LimitReached && early.Receipts.Count == 1 && early.Earnings == 0 &&
                    early.ForfeitedEarnings == receipt.Payout && early.DishXp == receipt.XpGained && profile.Rewards.Wallet == wallet && s.PendingDayEarnings == 0,
                    "early day voids money, preserves prior balance and Dish XP");
                return "8A model PASS: empty/early/ten receipts, exact earnings/fraction XP, partial/paused/active/stale guards, immutable reports and owner cleanup.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(partial); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying, "prepare outside Play");
            string path = System.IO.Path.GetFullPath("Temp/ClickCookSaveTests/DaySummary-" + Guid.NewGuid().ToString("N") + "/profile.json");
            Check(new PlayerProfileStore(path).Save(new PlayerProfile()), "temporary profile");
            SessionState.SetString("ClickCook.SaveVerificationPath", path);
            return path;
        }

        public static string VerifyPlay()
        {
            Check(EditorApplication.isPlaying, "Play required");
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var menu = c.GetComponent<ProfileMenu>(); menu.Continue(); menu.Resume();
            c.CookSelectedLocation(); c.FinishDay();
            Check(c.DaySummary != null && c.DaySummary.Receipts.Count == 0, "empty controller summary");
            c.FinishDay(); c.ContinueFromDishResult(); Check(c.DaySummary.Receipts.Count == 0, "stale day/result callbacks");
            c.OpenUpgrades(); Check(Field<GameObject>(c, "upgradeScreen").activeSelf && !Field<GameObject>(c, "daySummaryScreen").activeSelf, "summary to upgrades");
            c.NextDay(); Check(c.DaySummary != null, "hidden summary next blocked");
            c.CloseUpgrades(); Check(Field<GameObject>(c, "daySummaryScreen").activeSelf, "upgrade back to same summary");
            c.NextDay(); c.NextDay(); Check(c.DaySummary == null && !c.OrderSession.IsDayActive && Field<GameObject>(c, "locationSelectionScreen").activeSelf, "next once to location selection");
            c.CookSelectedLocation(); var s = c.OrderSession;
            var dish = UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish);
            var location = UnityEngine.Object.Instantiate(s.SelectedLocation);
            try
            {
                var so = new SerializedObject(dish); so.FindProperty("clickRequirement").intValue = 1; so.ApplyModifiedPropertiesWithoutUndo();
                Pool(location, dish); s.StartDay(location, 2);
                decimal before = s.Rewards.Wallet, earnings = 0, xp = 0;
                for (int i = 0; i < 10; i++)
                {
                    c.SelectOrder(0); c.ClickCookingDish(); earnings += s.LastResult.Payout; xp += s.LastResult.XpGained;
                    c.ContinueFromDishResult();
                }
                var report = c.DaySummary;
                Check(report != null && report.LimitReached && report.Receipts.Count == 10 && report.Earnings == earnings && report.DishXp == xp && s.Rewards.Wallet == before + earnings,
                    "tenth-result ends in exact summary");
                c.FinishDay(); c.ClickCookingDish(); c.ContinueFromDishResult();
                Check(s.Rewards.Wallet == before + earnings && c.DaySummary == report && !s.Profile.HasActiveDay, "stale callbacks no payment");
                Canvas.ForceUpdateCanvases(); var text = Field<UnityEngine.UI.Text>(c, "daySummaryText");
                Check(text.preferredHeight <= text.rectTransform.rect.height + 1 && text.text.Contains(ClickCookUiCopy.Money(earnings)), "ten-row report fits and shows earnings");
                Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == Field<UnityEngine.UI.Button>(c, "nextDayButton").gameObject, "summary initial focus");
                var saved = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();
                Check(saved != null && saved.Rewards.Wallet == s.Rewards.Wallet && saved.Rewards.GetExactTotalXp(dish) == s.Rewards.GetExactTotalXp(dish) && !saved.HasActiveDay,
                    "receipt autosave contains exact progress with idle reload");
                return "8A Play PASS: empty and ten-row reports, tenth-result flow, replay guards, upgrades return, Next Day, focus/text fit and isolated save reload. Summary left visible for inspection.";
            }
            finally
            {
                // Dish references remain in the visible report until the inspection is finished.
                dish.hideFlags = location.hideFlags = HideFlags.DontSave;
            }
        }

        public static string FinishPlay()
        {
            Check(!EditorApplication.isPlaying, "stop Play before cleanup");
            SessionState.EraseString("ClickCook.SaveVerificationPath");
            return "8A temporary profile override removed.";
        }
    }
}
