using System;
using System.Linq;
using System.Reflection;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    /// <summary>Editor-only presentation checks over the real controller/session and rendered Canvas.</summary>
    public static class UiA2Verification
    {
        private static int checks;
        private static T Field<T>(ClickCookPrototypeController c,string key)=>(T)typeof(ClickCookPrototypeController)
            .GetField(key,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(c);
        private static ClickCookPrototypeController Controller=>UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static void Check(bool ok,string detail){checks++;if(!ok)throw new Exception("UI-A2: "+detail);}
        public static string ShowBoard(int level)
        {
            var c=Controller;if(!EditorApplication.isPlaying||!c.enabled)throw new Exception("Ready Play Mode required.");
            if(Field<GameObject>(c,"cookingPrototypeScreen").activeSelf)c.BackToBoard();
            if(Field<GameObject>(c,"dishResultScreen").activeSelf)c.ContinueFromDishResult();
            if(c.OrderSession.IsDayActive)c.FinishDay();c.CloseUpgrades();
            for(int i=0;i<11 && Field<int>(c,"currentLocationIndex")!=level;i++)c.ShowNextLocation();
            c.CookSelectedLocation();return "Board fixture location "+level;
        }
        public static string ShowCooking(){Controller.SelectOrder(0);return "Cooking fixture; selected actual order 0";}
        public static string ShowResult()
        {
            var c=Controller;var active=c.OrderSession.ActiveOrder;
            if(active==null)throw new Exception("Select an order first.");
            int clicks=(active.Dish.ClickRequirement+c.OrderSession.ClickDamage-1)/c.OrderSession.ClickDamage;
            for(int i=0;i<clicks;i++)c.ClickCookingDish();return "Result fixture via "+clicks+" controller clicks (not native mouse)";
        }
        public static string RunPresentationChecks()
        {
            checks=0;var c=Controller;
            for(int level=0;level<11;level++)
            {
                ShowBoard(level);var s=c.OrderSession;
                Check(Field<UnityEngine.UI.Text>(c,"workDayLocationText").text==ClickCookUiCopy.Location(s.SelectedLocation),"location copy");
                Check(Field<UnityEngine.UI.Text>(c,"dayCompletedText").text=="Приготовано: 0 / 10","daily count separate");
                Check(Field<UnityEngine.UI.Text>(c,"orderBoardStatusText").text.Contains(s.Orders.Count+" / "+s.BoardCapacity),"capacity/count");
                Check(!Field<UnityEngine.UI.Text>(c,"workDayDishCandidatesText").gameObject.activeSelf,"candidate diagnostics hidden, data retained");
                var buttons=Field<UnityEngine.UI.Button[]>(c,"orderButtons");
                for(int i=0;i<buttons.Length;i++)
                {
                    Check(buttons[i].gameObject.activeSelf==(i<s.BoardCapacity),"slot visibility");
                    var order=s.GetOrderAtSlot(i);
                    if(order!=null)Check(Field<UnityEngine.UI.Text[]>(c,"orderLabels")[i].text.Contains(ClickCookUiCopy.Dish(order.Dish)),"card name");
                }
                c.FinishDay();Check(!s.IsDayActive&&s.Orders.Count==0,"Finish clears day");
            }
            ShowBoard(0);var first=c.OrderSession.GetOrderAtSlot(0);ShowCooking();
            c.ClickCookingDish();c.BackToBoard();
            Check(c.OrderSession.GetOrderAtSlot(0)==first&&c.OrderSession.CompletedDishes==0,"Back identity/count");
            for(int n=1;n<=10;n++)
            {
                ShowCooking();var order=c.OrderSession.ActiveOrder;
                Check(Field<UnityEngine.UI.Text>(c,"cookingOrderText").text.Contains(ClickCookUiCopy.Dish(order.Dish)),"cooking name");
                ShowResult();var result=c.OrderSession.LastResult;
                Check(Field<UnityEngine.UI.Text>(c,"resultRarityText").text==ClickCookUiCopy.Rarity(result.Rarity),"named rarity");
                Check(Field<UnityEngine.UI.Text>(c,"dishResultText").text.Contains(ClickCookUiCopy.Money(result.Payout)),"receipt payout");
                int xp=c.OrderSession.Rewards.GetLevelXp(result.Dish);
                float expected=result.Level==18?1f:(float)xp/PrototypeDishRewards.RequiredXp(result.Level);
                Check(Mathf.Abs(Field<UnityEngine.UI.Image>(c,"resultMasteryFill").fillAmount-expected)<.0001f,"real mastery fill, not HP");
                Check(Field<UnityEngine.UI.Button>(c,"continueFromResultButton").GetComponentInChildren<UnityEngine.UI.Text>().text==
                    (n==10?"Завершити день":"Продовжити"),"tenth CTA");
                Check(EventSystem.current.currentSelectedGameObject==Field<UnityEngine.UI.Button>(c,"continueFromResultButton").gameObject,"result focus");
                decimal wallet=c.OrderSession.Rewards.Wallet;c.ContinueFromDishResult();
                Check(c.OrderSession.Rewards.Wallet==wallet,"no repeated payout");
            }
            Check(!c.OrderSession.IsDayActive,"tenth Continue end");
            Check(Field<UnityEngine.UI.Text>(c,"resultMasteryText").text==string.Empty&&Field<UnityEngine.UI.Image>(c,"resultMasteryFill").fillAmount==0,"result presentation cleared");
            return "PASS "+checks+" UI-A2 presentation/lifecycle assertions: 11 boards, Back and 10 receipts/Continue.";
        }
        public static string LayoutChecks(int width,int height,string stage)
        {
            checks=0;var c=Controller;Canvas.ForceUpdateCanvases();var canvas=c.GetComponentInChildren<Canvas>(true);
            Check(canvas.pixelRect.width==width&&canvas.pixelRect.height==height,"actual Canvas size");
            string field=stage=="board"?"workDayScreen":stage=="cooking"?"cookingPrototypeScreen":"dishResultScreen";
            var screen=Field<GameObject>(c,field);Check(screen.activeInHierarchy,"visible stage");
            foreach(var t in screen.GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                Check(t.preferredHeight<=t.rectTransform.rect.height+2,"clipped text: "+t.name);
                Check(!t.raycastTarget,"decorative text input: "+t.name);
                Check(t.text.All(ch=>char.IsWhiteSpace(ch)||t.font.HasCharacter(ch)),"font glyphs: "+t.name);
            }
            bool pointer=Screen.width==width&&Screen.height==height;
            var buttons=screen.GetComponentsInChildren<UnityEngine.UI.Button>();
            foreach(var b in buttons)
            {
                var rt=b.GetComponent<RectTransform>();var corners=new Vector3[4];rt.GetWorldCorners(corners);
                Check(corners.All(p=>p.x>=0&&p.x<=width&&p.y>=0&&p.y<=height),"button outside Canvas: "+b.name);
                Check(b.navigation.mode==UnityEngine.UI.Navigation.Mode.Explicit,"navigation: "+b.name);
                if(pointer&&b.interactable)
                {
                    var point=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center));
                    var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},hits);
                    Check(hits.Count>0&&hits[0].gameObject==b.gameObject,"topmost raycast: "+b.name);
                }
            }
            if(stage=="board")
            {
                var cards=Field<UnityEngine.UI.Button[]>(c,"orderButtons").Where(b=>b.gameObject.activeInHierarchy).ToArray();
                for(int i=0;i<cards.Length;i++)for(int j=i+1;j<cards.Length;j++)
                {
                    var a=cards[i].GetComponent<RectTransform>();var b=cards[j].GetComponent<RectTransform>();
                    Check(!new Rect(a.anchoredPosition-a.sizeDelta/2,a.sizeDelta).Overlaps(new Rect(b.anchoredPosition-b.sizeDelta/2,b.sizeDelta)),"cards overlap");
                }
            }
            var target=buttons.FirstOrDefault(b=>b.interactable);
            Check(target!=null,"available control");EventSystem.current.SetSelectedGameObject(target.gameObject);
            Check(target.GetComponent<UnityEngine.UI.Outline>().enabled,"focus cue");
            var next=target.navigation.selectOnRight;
            ExecuteEvents.Execute(target.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Right},ExecuteEvents.moveHandler);
            Check(next==null||EventSystem.current.currentSelectedGameObject==next.gameObject,"EventSystem right focus");
            return "PASS "+checks+" "+stage+" checks at "+width+"x"+height+"; pointer raycast "+(pointer?"PASS":"NOT VERIFIED (Screen mismatch)")+". Native input separate.";
        }
        public static string RunEdgeChecks()
        {
            checks=0;var c=Controller;ShowBoard(0);
            var empty=ScriptableObject.CreateInstance<ClickCook.Data.LocationData>();
            try
            {
                c.OrderSession.StartDay(empty,2);
                typeof(ClickCookPrototypeController).GetMethod("RefreshOrderBoard",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
                typeof(ClickCookPrototypeController).GetMethod("FocusBoard",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
                Check(c.OrderSession.Orders.Count==0,"no fake empty-pool orders");
                Check(Field<UnityEngine.UI.Text>(c,"orderBoardStatusText").text.Contains("Немає доступних замовлень"),"empty message");
                Check(Field<UnityEngine.UI.Button[]>(c,"orderButtons").All(b=>!b.interactable),"empty slots disabled");
                Check(EventSystem.current.currentSelectedGameObject==Field<UnityEngine.UI.Button>(c,"finishDayButton").gameObject,"empty board focus escape");
                var text=Field<UnityEngine.UI.Text>(c,"orderBoardStatusText");
                Check(text.preferredHeight<=text.rectTransform.rect.height+2,"empty copy fits");
                c.FinishDay();Check(!c.OrderSession.IsDayActive,"empty day Finish");
            }
            finally { UnityEngine.Object.DestroyImmediate(empty); }
            ShowBoard(0);var dish=c.OrderSession.GetOrderAtSlot(0).Dish;
            var xp=(System.Collections.Generic.Dictionary<int,int>)typeof(PrototypeDishRewards)
                .GetField("dishXp",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(c.OrderSession.Rewards);
            xp[dish.DishId]=1744; // Play Mode-only mastery-cap fixture; never touches DishData.
            ShowCooking();ShowResult();
            Check(c.OrderSession.LastResult.Level==18&&c.OrderSession.LastResult.XpGained==0,"cap receipt");
            Check(Field<UnityEngine.UI.Text>(c,"resultMasteryText").text=="Майстерність: максимум","cap copy");
            Check(Field<UnityEngine.UI.Image>(c,"resultMasteryFill").fillAmount==1f,"cap fill");
            Check(c.OrderSession.LastResult.Payout>0,"cap still pays");
            c.ContinueFromDishResult();c.FinishDay();
            return "PASS "+checks+" empty-board/mastery-cap checks using transient Play Mode-only fixtures.";
        }
    }
}
