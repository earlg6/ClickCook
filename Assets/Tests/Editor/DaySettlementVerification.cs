using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    public static class DaySettlementVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static T Field<T>(object o, string key) => (T)o.GetType().GetField(key, Private).GetValue(o);
        private static void Check(bool condition, string message) { if (!condition) throw new Exception("Day settlement: " + message); }
        private static DishData dish;
        private static LocationData location;
        private static void Fixtures()
        {
            dish = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset").Find(1));
            var ds = new SerializedObject(dish); ds.FindProperty("clickRequirement").intValue = 1; ds.ApplyModifiedPropertiesWithoutUndo();
            location = ScriptableObject.CreateInstance<LocationData>();
            var ls = new SerializedObject(location); var pool = ls.FindProperty("documentedAvailableDishes"); pool.arraySize = 1;
            pool.GetArrayElementAtIndex(0).objectReferenceValue = dish; ls.ApplyModifiedPropertiesWithoutUndo();
            dish.hideFlags = location.hideFlags = HideFlags.DontSave;
        }
        private static PlayerProfile Funded() => (PlayerProfile)typeof(PlayerProfile).GetMethod("Restore", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null,
            new object[] { 123.25m, new Dictionary<int,int>(), new Dictionary<string,int> { { UpgradeNodeRules.Strength, 3 },
                { UpgradeNodeRules.DishXpFlat, 1 }, { UpgradeNodeRules.DishXpMultiplier, 1 }, { UpgradeNodeRules.DishValue, 2 } }, new Dictionary<int,int> { { 1, 37 } } });
        public static string RunModelChecks()
        {
            var random = UnityEngine.Random.state; Fixtures();
            string root = Path.GetFullPath("Temp/ClickCookSaveTests/DaySettlement-" + Guid.NewGuid().ToString("N"));
            try
            {
                for (int completed = 1; completed <= 10; completed++)
                {
                    var profile = Funded(); var s = new PrototypeOrderSession(profile);
                    s.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset"));
                    var store = new PlayerProfileStore(Path.Combine(root, completed + ".json")); int changes = 0;
                    profile.Changed += () => { changes++; Check(store.Save(profile), "autosave"); };
                    s.StartDay(location, 2); decimal receiptSum = 0, xpSum = .37m;
                    for (int i = 1; i <= completed; i++)
                    {
                        Check(s.TrySelect(0), "select"); var order = s.ActiveOrder;
                        Check(s.TryCookClick(order), "complete"); receiptSum += s.LastResult.Payout; xpSum += s.LastResult.XpGained;
                        Check(!s.TryCookClick(order), "stale receipt");
                        Check(profile.Rewards.Wallet == (i < 10 ? 123.25m : 123.25m + receiptSum) && s.PendingDayEarnings == (i < 10 ? receiptSum : 0), "exact pending/settled wallet");
                        var disk = store.Load();
                        Check(disk.Rewards.Wallet == profile.Rewards.Wallet && disk.Rewards.GetExactTotalXp(dish) == xpSum && !disk.HasActiveDay,
                            "receipt save retains XP but never unsettled cash");
                    }
                    decimal expectedWallet = completed < 10 ? 123.25m : 123.25m + receiptSum;
                    Check(s.TryFinishDay(out var summary) && summary.Earnings == (completed < 10 ? 0 : receiptSum) &&
                        summary.ForfeitedEarnings == (completed < 10 ? receiptSum : 0) && summary.DishXp == xpSum - .37m, "early/full report exact");
                    Check(!s.TryFinishDay(out _) && profile.Rewards.Wallet == expectedWallet && changes == completed &&
                        s.PendingDayEarnings == 0 && !profile.HasActiveDay && profile.Rewards.GetExactTotalXp(dish) == xpSum, "finish/replay no extra save/pay, XP retained");
                    s.StartDay(location, 1); s.TrySelect(0); s.TryCookClick(s.ActiveOrder); s.EndDay();
                    Check(profile.Rewards.Wallet == expectedWallet && s.PendingDayEarnings == 0, "abandoned next day no carry or debit of old wallet");
                }
                // Exit/restart/scene destruction uses EndDay, not TryFinishDay.
                var abandoned = new PrototypeOrderSession(Funded()); abandoned.StartDay(location, 1); abandoned.TrySelect(0); abandoned.TryCookClick(abandoned.ActiveOrder);
                decimal xpBeforeExit = abandoned.Rewards.GetExactTotalXp(dish); abandoned.EndDay(); abandoned.EndDay();
                Check(abandoned.Rewards.Wallet == 123.25m && abandoned.PendingDayEarnings == 0 && abandoned.Rewards.GetExactTotalXp(dish) == xpBeforeExit,
                    "menu/scene cleanup voids cash, keeps original money and XP");
                return "Day settlement model PASS: all early counts1–9, full tenth, fractional XP/value effects, exact saved wallet, interruption/replay/no carry and original-balance protection.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }
        public static string PreparePlay() => DaySummaryVerification.PreparePlay();
        public static string VerifyEarlyPlay()
        {
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();
            c.CookSelectedLocation(); Fixtures(); var s=c.OrderSession; s.StartDay(location,2);
            for(int i=0;i<3;i++) { c.SelectOrder(0); c.ClickCookingDish();
                var result=Field<UnityEngine.UI.Text>(c,"dishResultText");Canvas.ForceUpdateCanvases();
                Check(result.preferredHeight<=result.rectTransform.rect.height+1 && result.text.Contains("Виплата — після 10"),"pending result copy/fit");c.ContinueFromDishResult(); }
            decimal pending=s.PendingDayEarnings, xp=s.Rewards.GetExactTotalXp(dish);Check(pending>0 && s.Rewards.Wallet==0,"three receipts unpaid");
            c.FinishDay();Check(c.DaySummary.Earnings==0 && c.DaySummary.ForfeitedEarnings==pending && s.Rewards.Wallet==0 && s.PendingDayEarnings==0,"early UI voids all order money");
            var text=Field<UnityEngine.UI.Text>(c,"daySummaryText");Canvas.ForceUpdateCanvases();
            Check(text.text.Contains("Анульовано")&&text.preferredHeight<=text.rectTransform.rect.height+1,"forfeiture summary copy/fit");
            var loaded=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(loaded.Rewards.Wallet==0 && loaded.Rewards.GetExactTotalXp(dish)==xp,"early autosave zero-money/exact XP");
            return "Early Play PASS: three receipts, zero wallet/save money, exact retained Dish XP, forfeiture summary and result fit.";
        }
        public static string VerifyFullPlay()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();c.GetComponent<ProfileMenu>().Resume();c.NextDay();c.CookSelectedLocation();
            var s=c.OrderSession;s.StartDay(location,2);decimal total=0;
            for(int i=1;i<=10;i++){c.SelectOrder(0);c.ClickCookingDish();total+=s.LastResult.Payout;
                Check(s.Rewards.Wallet==(i<10?0:total),"full controller settlement at tenth");c.ContinueFromDishResult();}
            Check(c.DaySummary.Earnings==total && c.DaySummary.ForfeitedEarnings==0 && s.PendingDayEarnings==0,"full report");
            c.ContinueFromDishResult();c.ClickCookingDish();c.FinishDay();Check(s.Rewards.Wallet==total,"full stale callbacks");
            SessionState.SetString("ClickCook.DaySettlementWallet",total.ToString(System.Globalization.CultureInfo.InvariantCulture));
            return "Full Play PASS: payout once at tenth receipt, no early-day carry, full summary and stale callbacks.";
        }
        public static string VerifyRestart()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();
            decimal expected=decimal.Parse(SessionState.GetString("ClickCook.DaySettlementWallet",""),System.Globalization.CultureInfo.InvariantCulture);
            Check(c.OrderSession.Rewards.Wallet==expected && c.OrderSession.PendingDayEarnings==0 && !c.OrderSession.IsDayActive,"restart exact settled balance/no pending cash");
            return "Restart PASS: exact full-day balance retained, no pending income/day restored.";
        }
    }
}
