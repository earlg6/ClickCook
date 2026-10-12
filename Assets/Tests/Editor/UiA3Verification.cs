using System;
using System.Reflection;
using ClickCook.Runtime;
using UnityEngine;

namespace ClickCook.Verification
{
    public static class UiA3Verification
    {
        private static T Field<T>(ClickCookPrototypeController c,string key)=>(T)typeof(ClickCookPrototypeController).GetField(key,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(c);
        public static string RunChecks()
        {
            int checks=0;
            void Check(bool ok,string detail){checks++;if(!ok)throw new Exception("UI-A3: "+detail);}
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            UiA2Verification.ShowBoard(0);var s=c.OrderSession;var first=s.GetOrderAtSlot(0);
            Check(Field<UnityEngine.UI.Button>(c,"exitGameButton").gameObject.activeInHierarchy==false,"Exit only on menu");
            c.ExitGame();Check(UnityEditor.EditorApplication.isPlaying,"hidden Exit callback ignored");
            var buttons=Field<UnityEngine.UI.Button[]>(c,"orderButtons");
            Check(buttons[0].GetComponent<RectTransform>().anchoredPosition.x==-210 && buttons[1].GetComponent<RectTransform>().anchoredPosition.x==210,"two centered cards");
            c.SelectOrder(0);var text=Field<UnityEngine.UI.Text>(c,"cookingHpText");var fill=Field<UnityEngine.UI.Image>(c,"cookingHpFill");
            int maximum=first.Dish.ClickRequirement;
            Check(fill.fillAmount==1f&&text.text.Contains("HP: "+maximum+" / "+maximum),"full initial HP");
            Check(text.text.Contains("Залишилось кліків: "+((maximum+s.ClickDamage-1)/s.ClickDamage)),"click count uses current strength");
            c.ClickCookingDish();int remaining=maximum-s.ClickDamage;
            Check(remaining>0 && text.text.Contains("HP: "+remaining+" / "+maximum),"HP decreases on non-final click");
            Check(Mathf.Abs(fill.fillAmount-(float)remaining/maximum)<.0001f,"real HP fraction");
            c.BackToBoard();Check(s.GetOrderAtSlot(0)==first&&s.CompletedDishes==0,"Back preserves exact order");
            Check(fill.fillAmount==0&&text.text==string.Empty,"Back clears stale cooking display");
            c.SelectOrder(0);Check(fill.fillAmount==1f,"reselection resets HP");
            while(s.ActiveOrder!=null)c.ClickCookingDish();
            Check(s.CompletedDishes==1&&Field<GameObject>(c,"dishResultScreen").activeSelf,"one completion");
            var wallet=s.Rewards.Wallet;c.ContinueFromDishResult();Check(s.Rewards.Wallet==wallet,"Continue no duplicate reward");
            var fills=Field<UnityEngine.UI.Image[]>(c,"orderMasteryFills");
            for(int i=0;i<s.BoardCapacity;i++)
            {
                var dish=s.GetOrderAtSlot(i).Dish;int level=s.Rewards.GetLevel(dish);
                float expected=level==18?1f:(float)s.Rewards.GetLevelXp(dish)/PrototypeDishRewards.RequiredXp(level);
                Check(Mathf.Abs(fills[i].fillAmount-expected)<.0001f,"card bar is mastery, not cooking HP");
            }
            c.FinishDay();Check(Field<UnityEngine.UI.Button>(c,"exitGameButton").gameObject.activeInHierarchy,"Exit visible on menu");
            return "PASS "+checks+" UI-A3 HP/click count, reset, central cards, mastery, Exit guard assertions. Native Exit tested separately.";
        }
    }
}
