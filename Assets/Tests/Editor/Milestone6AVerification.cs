using System;
using System.Linq;
using System.Reflection;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.Runtime.Locations;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    /// <summary>Repeatable Editor-only checks invoked through Unity MCP eval. No production cheats.</summary>
    public static class Milestone6AVerification
    {
        private static int checks;
        private static void Check(bool value, string message)
        {
            checks++;
            if (!value) throw new Exception("6A verification: " + message);
        }
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static PrototypeOrderSession Session()
        {
            var session = new PrototypeOrderSession();
            Check(session.ConfigureUpgrades(Catalog), "configure catalog");
            return session;
        }
        private static void FundFixture(PrototypeOrderSession session, decimal amount)
        {
            typeof(PrototypeDishRewards).GetField("<Wallet>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(session.Rewards, amount);
        }

        public static string RunModelChecks()
        {
            checks = 0;
            var random = UnityEngine.Random.state;
            var fixture = ScriptableObject.CreateInstance<LocationData>();
            try
            {
                Check(Catalog.IsValid && Catalog.Upgrades.Count == 16, "16 valid unique definitions");
                int[] costs = {20,44,97,213,469,1031,2268,4989,10976,24146,53120,116864,257101,565621,1244365};
                int[] damage = {1,2,4,6,10,17,29,51,88,154,270,472,826,1444,2527,4422};
                var definition = Catalog.Find(UpgradeCatalog.ClickStrengthId);
                for (int i = 0; i < 15; i++) Check(definition.PriceForNextRank(i) == costs[i], "price rank " + (i+1));
                for (int i = 0; i <= 15; i++) Check(UpgradeCatalog.ClickDamageAtRank(i) == damage[i], "damage rank " + i);
                var s = Session();
                Check(s.ClickStrengthRank == 0 && s.ClickDamage == 1 && s.Rewards.Wallet == 0, "new session");
                Check(!s.ConfigureUpgrades(Catalog), "no reconfigure");
                foreach (var entry in Catalog.Upgrades.Where(x => x.Id != UpgradeCatalog.ClickStrengthId))
                    Check(!s.TryQuoteUpgrade(entry.Id, out _), "unfinished node blocked " + entry.Id);
                Check(!s.TryQuoteUpgrade("unknown", out _) && !s.TryQuoteUpgrade(null, out _), "invalid IDs");
                Check(!s.TryPurchaseUpgrade(null), "null quote");
                s.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var first);
                FundFixture(s,19.99m);
                Check(!s.TryPurchaseUpgrade(first) && s.Rewards.Wallet == 19.99m && s.ClickStrengthRank == 0, "insufficient exact cents");
                FundFixture(s,20m);
                Check(s.TryPurchaseUpgrade(first) && s.Rewards.Wallet == 0 && s.ClickStrengthRank == 1, "exact price debit");
                FundFixture(s,10000000m);
                Check(!s.TryPurchaseUpgrade(first) && s.Rewards.Wallet == 10000000m, "stale replay");
                var other = Session();
                other.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var foreign);
                Check(!s.TryPurchaseUpgrade(foreign), "cross-session quote");
                s.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var beforeDay);
                s.StartDay(fixture,2);
                Check(!s.TryPurchaseUpgrade(beforeDay) && !s.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out _), "no mid-day purchase");
                Check(s.Orders.Count == 0, "empty pool");
                s.EndDay();
                Check(s.TryPurchaseUpgrade(beforeDay), "unchanged quote valid between days");
                Check(s.ClickStrengthRank == 2, "rank survives day cleanup");

                var dishes = AssetDatabase.FindAssets("t:DishData", new[]{"Assets/Data/Dishes"})
                    .Select(g => AssetDatabase.LoadAssetAtPath<DishData>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
                Check(dishes.Length == 32, "all dish fixtures");
                for(int rank=0;rank<=15;rank++)
                {
                    var session=Session();
                    FundFixture(session,10000000m);
                    for(int n=0;n<rank;n++)
                    {
                        session.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId,out var q);
                        decimal balance=session.Rewards.Wallet;
                        Check(session.TryPurchaseUpgrade(q) && session.Rewards.Wallet==balance-costs[n], "rank purchase debit");
                        Check(!session.TryPurchaseUpgrade(q), "duplicate quote");
                    }
                    Check(session.ClickDamage==damage[rank], "applied strength");
                    if(rank==15) Check(!session.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId,out _), "maximum rank");
                    foreach(var dish in dishes)
                    {
                        var so=new SerializedObject(fixture);
                        var pool=so.FindProperty("documentedAvailableDishes");pool.arraySize=1;
                        pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;so.ApplyModifiedPropertiesWithoutUndo();
                        session.StartDay(fixture,2);
                        var firstOrder=session.GetOrderAtSlot(0);
                        var untouched=session.GetOrderAtSlot(1);
                        Check(firstOrder.Dish==dish && untouched.Dish==dish && firstOrder.Identity!=untouched.Identity,"independent duplicates");
                        Check(session.TrySelect(0) && !session.TrySelect(1),"one active");
                        int required=(dish.ClickRequirement+damage[rank]-1)/damage[rank];
                        if(required>1)
                        {
                            Check(!session.TryCookClick(firstOrder),"partial");
                            session.ReturnToBoard();
                            Check(session.TrySelect(0) && firstOrder.RemainingClicks==dish.ClickRequirement,"Back resets HP");
                        }
                        decimal wallet=session.Rewards.Wallet;
                        for(int click=1;click<=required;click++)
                        {
                            bool completed=session.TryCookClick(firstOrder);
                            if(completed!=(click==required)) throw new Exception("Early/late completion: "+dish.name+" rank "+rank);
                        }
                        Check(firstOrder.IsCompleted && firstOrder.RemainingClicks==0 && session.CompletedDishes==1,"exact HP completion");
                        Check(session.Rewards.Wallet==wallet+session.LastResult.Payout,"single exact payout");
                        Check(session.GetOrderAtSlot(1)==untouched && session.GetOrderAtSlot(0).Identity!=firstOrder.Identity,"replacement only freed slot");
                        var result=session.LastResult;
                        wallet=session.Rewards.Wallet;
                        Check(!session.TryCookClick(firstOrder) && session.Rewards.Wallet==wallet && session.LastResult==result,"repeat completion blocked");
                        session.EndDay();
                        Check(session.ClickStrengthRank==rank && session.Rewards.Wallet==wallet,"cross-day retention");
                    }
                }
                var fresh=Session();
                Check(fresh.ClickStrengthRank==0 && fresh.ClickDamage==1 && fresh.Rewards.Wallet==0,"fresh run reset");
                return "PASS: "+checks+" model assertions; all 32 dishes x 16 strength ranks (512 exact-cooking cases).";
            }
            finally
            {
                UnityEngine.Random.state=random;
                UnityEngine.Object.DestroyImmediate(fixture);
            }
        }

        private static T Field<T>(ClickCookPrototypeController c,string name) =>
            (T)typeof(ClickCookPrototypeController).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(c);
        private static void Press(ClickCookPrototypeController c,string field) => Field<UnityEngine.UI.Button>(c,field).onClick.Invoke();
        private static void CompleteUiDish(ClickCookPrototypeController c)
        {
            var order=c.OrderSession.ActiveOrder;
            int clicks=(order.Dish.ClickRequirement+c.OrderSession.ClickDamage-1)/c.OrderSession.ClickDamage;
            for(int n=0;n<clicks;n++) Press(c,"cookingDishButton");
            Check(Field<GameObject>(c,"dishResultScreen").activeSelf,"result visible");
            decimal wallet=c.OrderSession.Rewards.Wallet;
            Press(c,"cookingDishButton");
            Check(c.OrderSession.Rewards.Wallet==wallet,"stale dish callback");
        }

        public static string RunPlayChecks()
        {
            if(!EditorApplication.isPlaying) throw new Exception("Enter fresh Play Mode first.");
            checks=0;
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            Check(c!=null && c.enabled,"controller ready");
            Check(c.OrderSession.Rewards.Wallet==0 && c.OrderSession.ClickStrengthRank==0,"fresh Play Mode");
            Press(c,"openUpgradesButton");
            Check(Field<GameObject>(c,"upgradeScreen").activeSelf && !Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"zero-wallet UI");
            Press(c,"buyUpgradeButton");
            Check(c.OrderSession.ClickStrengthRank==0,"unaffordable callback");
            Press(c,"cookButton");
            Press(c,"nextButton");
            Check(!c.OrderSession.IsDayActive && Field<GameObject>(c,"upgradeScreen").activeSelf,"hidden navigation guards");
            Press(c,"closeUpgradesButton");
            string initial=Field<UnityEngine.UI.Text>(c,"locationNameText").text;
            Press(c,"previousButton");
            Check(Field<UnityEngine.UI.Text>(c,"locationLevelText").text.Contains("10"),"left wrap");
            Press(c,"nextButton");
            Check(Field<UnityEngine.UI.Text>(c,"locationNameText").text==initial,"right wrap");
            for(int pass=0;pass<2;pass++)
            {
                for(int location=0;location<11;location++)
                {
                    Press(c,"cookButton");
                    var s=c.OrderSession;
                    Check(s.IsDayActive && s.SelectedLocation.LevelRequirement==location,"start location "+location);
                    Check(s.BoardCapacity<=6 && s.Orders.All(o=>s.SelectedLocation.DocumentedAvailableDishes.Contains(o.Dish)),"pool/capacity");
                    Press(c,"openUpgradesButton");
                    Check(!Field<GameObject>(c,"upgradeScreen").activeSelf,"upgrade blocked during day");
                    if(s.Orders.Count>0)
                    {
                        Field<UnityEngine.UI.Button[]>(c,"orderButtons")[0].onClick.Invoke();
                        var active=s.ActiveOrder;
                        Press(c,"backToBoardButton");
                        Check(s.ActiveOrder==null && s.GetOrderAtSlot(0)==active,"Back identity");
                        Field<UnityEngine.UI.Button[]>(c,"orderButtons")[0].onClick.Invoke();
                        CompleteUiDish(c);
                        var replacement=s.GetOrderAtSlot(0);
                        decimal wallet=s.Rewards.Wallet;
                        Press(c,"continueFromResultButton");Press(c,"continueFromResultButton");
                        Check(s.GetOrderAtSlot(0)==replacement && s.CompletedDishes==1 && s.Rewards.Wallet==wallet,"Continue idempotent");
                    }
                    Press(c,"finishDayButton");
                    Check(!s.IsDayActive && s.Orders.Count==0,"early Finish");
                    Press(c,"nextButton");
                }
                if(pass==0)
                {
                    Press(c,"openUpgradesButton");
                    decimal wallet=c.OrderSession.Rewards.Wallet;
                    Press(c,"buyUpgradeButton");Press(c,"buyUpgradeButton");
                    Check(c.OrderSession.ClickStrengthRank==1 && c.OrderSession.Rewards.Wallet==wallet-20,"same-frame double callback buys once");
                    Press(c,"closeUpgradesButton");Press(c,"openUpgradesButton");
                    Check(Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text.Contains("44.00"),"next price UI");
                    Press(c,"buyUpgradeButton");
                    Check(c.OrderSession.ClickStrengthRank==2 && c.OrderSession.ClickDamage==4,"second explicit purchase");
                    Press(c,"closeUpgradesButton");
                }
            }
            Press(c,"cookButton");
            for(int n=1;n<=10;n++)
            {
                Field<UnityEngine.UI.Button[]>(c,"orderButtons")[0].onClick.Invoke();
                CompleteUiDish(c);
                Check(c.OrderSession.CompletedDishes==n,"completed counter");
                decimal wallet=c.OrderSession.Rewards.Wallet;
                Press(c,"continueFromResultButton");Press(c,"continueFromResultButton");
                Check(c.OrderSession.Rewards.Wallet==wallet,"Continue does not pay");
            }
            Check(!c.OrderSession.IsDayActive && c.OrderSession.Orders.Count==0 && c.OrderSession.ClickStrengthRank==2,"tenth result end/upgrade retention");
            Press(c,"openUpgradesButton");
            Canvas.ForceUpdateCanvases();
            Check(Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text.Contains("2 / 15"),"current rank UI");
            foreach(var label in Field<GameObject>(c,"upgradeScreen").GetComponentsInChildren<UnityEngine.UI.Text>())
                Check(label.preferredHeight<=label.rectTransform.rect.height+2,"label height: "+label.name);
            return "PASS: "+checks+" Play Mode assertions; all 11 locations before/after upgrades, 10-dish day, UI callbacks and layout. Run interaction checks after the Game View updates.";
        }

        // Separate invocation lets the Canvas/GraphicRaycaster observe the changed screen next frame.
        public static string RunInteractionChecks()
        {
            if(!EditorApplication.isPlaying) throw new Exception("Play Mode required.");
            checks=0;
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            Check(Field<GameObject>(c,"upgradeScreen").activeSelf,"upgrade screen visible");
            var button=Field<UnityEngine.UI.Button>(c,"closeUpgradesButton");
            var screen=RectTransformUtility.WorldToScreenPoint(null,button.GetComponent<RectTransform>().TransformPoint(button.GetComponent<RectTransform>().rect.center));
            var pointer=new PointerEventData(EventSystem.current){position=screen,button=PointerEventData.InputButton.Left};
            var hits=new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer,hits);
            Check(hits.Count>0 && hits[0].gameObject==button.gameObject,"Back topmost raycast");
            ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
            Check(Field<GameObject>(c,"locationSelectionScreen").activeSelf,"EventSystem Back");
            return "PASS: "+checks+" EventSystem Back raycast/pointer assertions. Physical mouse NOT tested.";
        }
    }
}
