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
    /// <summary>U2b only: scheduler, input lifecycle, purchase and schema boundary. Live tests require a Temp profile.</summary>
    public static class HoldUpgradeVerification
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value,string message) { if(!value)throw new Exception("U2b: "+message); }
        private static T Field<T>(object target,string name) => (T)target.GetType().GetField(name,Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id,int rank) => "{\"id\":\""+id+"\",\"rank\":"+rank+"}";
        private static string Signed(int version,string wallet,params string[] ranks)
        {
            string payload="{\"wallet\":\""+wallet+"\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":["+string.Join(",",ranks)+"]}";
            using var sha=SHA256.Create();return JsonUtility.ToJson(new Envelope {version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))});
        }
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static PlayerProfile Profile(int strength,int hold=0,bool critical=false) => Decode(Signed(3,"1000",Rank(UpgradeNodeRules.Strength,strength),Rank(UpgradeNodeRules.Hold,hold),Rank(UpgradeNodeRules.CriticalChance,critical?1:0),Rank(UpgradeNodeRules.CriticalMultiplier,critical?1:0)));
        private static PrototypeOrderSession Session(PlayerProfile profile,Func<double> roll=null)
        { var session=new PrototypeOrderSession(profile,roll);Check(session.ConfigureUpgrades(Catalog),"configure");return session; }
        private static void SetHp(DishData dish,int hp)
        { var data=new SerializedObject(dish);data.FindProperty("clickRequirement").intValue=hp;data.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetPool(LocationData location,DishData dish)
        { var data=new SerializedObject(location);var pool=data.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;data.ApplyModifiedPropertiesWithoutUndo(); }

        public static string RunModelChecks()
        {
            var random=UnityEngine.Random.state;var dish=ScriptableObject.CreateInstance<DishData>();var location=ScriptableObject.CreateInstance<LocationData>();
            try
            {
                var data=new SerializedObject(dish);data.FindProperty("dishId").intValue=1;data.FindProperty("saleValue").intValue=5;data.ApplyModifiedPropertiesWithoutUndo();SetHp(dish,1000);SetPool(location,dish);
                var locked=Session(Profile(1));Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.Hold,out _),"strength prerequisite");
                var buyer=Session(Profile(2));Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.Hold,out var quote) && quote.Price==250m,"single exact quote");
                Check(buyer.TryPurchaseUpgrade(quote) && !buyer.TryPurchaseUpgrade(quote) && buyer.HasHold && buyer.Rewards.Wallet==750m,"single debit/replay");
                Check(!buyer.TryQuoteUpgrade(UpgradeNodeRules.Hold,out _) && buyer.ClickStrengthRank==2,"one rank only, independent strength");
                Check(!buyer.TryQuoteUpgrade("node.hold_frequency.02",out _),"later frequency tier remains unavailable");
                var poor=Session(Profile(2));typeof(PrototypeDishRewards).GetField("<Wallet>k__BackingField",Private).SetValue(poor.Rewards,249.99m);
                poor.TryQuoteUpgrade(UpgradeNodeRules.Hold,out var expensive);Check(!poor.TryPurchaseUpgrade(expensive) && !poor.HasHold && poor.Rewards.Wallet==249.99m,"insufficient exact wallet");
                poor.IsPaused=true;Check(!poor.TryPurchaseUpgrade(expensive),"pause purchase guard");poor.IsPaused=false;
                buyer.StartDay(location,2);buyer.TrySelect(0);var order=buyer.ActiveOrder;
                Check(!buyer.TryBeginHold(buyer.GetOrderAtSlot(1),0),"bind only active order");
                Check(buyer.TryBeginHold(order,10) && !buyer.TryBeginHold(order,10),"one hold source");buyer.TryCookClick(order);int hp=order.RemainingClicks;
                Check(hp==996 && !buyer.TryTakeHoldRepeat(10.249999),"initial hit and minimum delay");
                Check(buyer.TryTakeHoldRepeat(10.25),"exact deadline");buyer.TryCookClick(order);Check(order.RemainingClicks==hp-4 && !buyer.TryTakeHoldRepeat(10.25),"one repeat per deadline");
                Check(buyer.TryTakeHoldRepeat(30) && !buyer.TryTakeHoldRepeat(30) && !buyer.TryTakeHoldRepeat(30.249) && buyer.TryTakeHoldRepeat(30.25),"delayed frame skips backlog");
                buyer.IsPaused=true;buyer.IsPaused=false;Check(!buyer.IsHolding && !buyer.TryTakeHoldRepeat(100),"same-frame pause/resume cancels");
                buyer.TryBeginHold(order,101);buyer.ReturnToBoard();buyer.TrySelect(0);Check(!buyer.IsHolding && !buyer.TryTakeHoldRepeat(200),"Back/reselect same order cancels");
                buyer.TryBeginHold(buyer.ActiveOrder,201);buyer.EndDay();Check(!buyer.IsHolding,"end day cancels");
                var absent=Session(Profile(3));absent.StartDay(location,1);absent.TrySelect(0);Check(!absent.TryBeginHold(absent.ActiveOrder,0),"unowned hold disabled");absent.EndDay();
                int rolls=0;SetHp(dish,30);var crit=Session(Profile(3,1,true),()=>{rolls++;return 0;});crit.StartDay(location,2);crit.TrySelect(0);order=crit.ActiveOrder;
                Check(crit.TryBeginHold(order,0),"critical hold begin");crit.TryCookClick(order);
                Check(crit.LastHitWasCritical && order.RemainingClicks==17 && rolls==1,"initial critical same pipeline");
                crit.TryTakeHoldRepeat(.25);crit.TryCookClick(order);Check(order.RemainingClicks==4 && rolls==2,"repeat critical same pipeline");
                crit.TryTakeHoldRepeat(.5);Check(crit.TryCookClick(order) && !crit.IsHolding && order.RemainingClicks==0 && rolls==3,"critical completion cancels");
                var paid=crit.Rewards.Wallet;Check(!crit.TryTakeHoldRepeat(10) && !crit.TryCookClick(order) && crit.Rewards.Wallet==paid && crit.CompletedDishes==1 && rolls==3,"single reward/no carry/stale hit");crit.EndDay();
                return "U2b model PASS: price/prerequisite/cap, exact deadlines, no backlog, pause/Back/end/order binding, critical repeats and one completion.";
            }
            finally { UnityEngine.Random.state=random;UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            var folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/U2bStore/"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
            foreach(int version in new[]{1,2})
            {
                string path=Path.Combine(folder,"v"+version+".json");
                string old=version==1?Signed(1,"1000.25",Rank("click_strength",3)):Signed(2,"1000.25",Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.CriticalChance,1),Rank(UpgradeNodeRules.CriticalMultiplier,1));
                File.WriteAllText(path,old);var store=new PlayerProfileStore(path);var profile=store.Load();
                Check(profile!=null && profile.Rewards.Wallet==1000.25m && profile.GetUpgradeRank(UpgradeNodeRules.Strength)==3 && profile.CaptureSnapshot().DishXpById[1]==50 && !Session(profile).HasHold && File.ReadAllText(path)==old,"load v"+version+" without rewrite or free Hold");
                var session=Session(profile);session.TryQuoteUpgrade(UpgradeNodeRules.Hold,out var quote);session.TryPurchaseUpgrade(quote);Check(store.Save(profile),"save current schema");
                Check(JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version==PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath)==old,"current version/old backup");
                var reloaded=new PlayerProfileStore(path).Load();Check(reloaded.GetUpgradeRank(UpgradeNodeRules.Hold)==1 && reloaded.Rewards.Wallet==750.25m && reloaded.GetUpgradeRank(UpgradeNodeRules.CriticalChance)==(version==2?1:0),"all ranks retained with Hold");
                File.WriteAllText(path,"broken");var backup=new PlayerProfileStore(path);Check(backup.Load().Rewards.Wallet==1000.25m && backup.Status==ProfileLoadStatus.Recovered,"old backup readable");
                string future=Signed(PlayerProfileStore.CurrentVersion+1,"1000",Rank(UpgradeNodeRules.Strength,3));File.WriteAllText(path,future);Check(new PlayerProfileStore(path).Load()==null && !store.Save(profile) && File.ReadAllText(path)==future,"future version protection");
            }
            foreach(var entries in new[]{new[]{Rank(UpgradeNodeRules.Hold,1)},new[]{Rank(UpgradeNodeRules.Strength,1),Rank(UpgradeNodeRules.Hold,1)},new[]{Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.Hold,2)},new[]{Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.Hold,-1)},new[]{Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.Hold,1),Rank(UpgradeNodeRules.Hold,1)},new[]{Rank(UpgradeNodeRules.Strength,3),Rank("node.hold_frequency.01",1)}})
                Check(Decode(Signed(3,"1000",entries))==null,"invalid signed Hold state");
            Check(Decode(Signed(2,"1000",Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.Hold,1)))==null,"Hold never accepted in schema 2");
            return "U2b storage PASS: schema 1/2 → 3, exact wallet/XP/working ranks, old backups, Hold bounds/whitelist and future protection.";
        }

        private static DishData playDish;
        private static LocationData playLocation;
        private static int releasedHp;
        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static CounterDishPointer Pointer => UnityEngine.Object.FindObjectsByType<CounterDishPointer>(FindObjectsInactive.Include,FindObjectsSortMode.None).Single();
        private static PointerEventData Event(int id=-1,PointerEventData.InputButton button=PointerEventData.InputButton.Left) => new(EventSystem.current){pointerId=id,button=button};
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying,"prepare outside Play");string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/ClickCookSaveTests/U2b-"+Guid.NewGuid().ToString("N"),"profile.json"));Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path,Signed(2,"1000",Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.CriticalChance,1),Rank(UpgradeNodeRules.CriticalMultiplier,1)));SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;
        }
        private static void SetupCooking()
        {
            var c=Controller;c.CloseUpgrades();var old=c.OrderSession;old.EndDay();var session=Session(old.Profile,()=>0);typeof(ClickCookPrototypeController).GetField("session",Private).SetValue(c,session);
            c.CookSelectedLocation();playDish=UnityEngine.Object.Instantiate(session.GetOrderAtSlot(0).Dish);playLocation=UnityEngine.Object.Instantiate(session.SelectedLocation);SetHp(playDish,1000);SetPool(playLocation,playDish);session.StartDay(playLocation,2);c.SelectOrder(0);
        }
        private static void FinishCooking()
        { var c=Controller;c.BackToBoard();c.FinishDay();UnityEngine.Object.DestroyImmediate(playDish);UnityEngine.Object.DestroyImmediate(playLocation);playDish=null;playLocation=null;c.OpenUpgrades(); }
        public static string SetupBeforePurchase()
        {
            Check(PlayerProfileRuntime.Store.SavePath.Contains("U2b-"),"isolated profile");var c=Controller;c.GetComponent<ProfileMenu>().Continue();c.OpenUpgrades();
            c.SelectUpgradeNode(UpgradeNodeRules.Hold);var buy=Field<UnityEngine.UI.Button>(c,"buyUpgradeButton");var detail=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");
            Check(buy.interactable && detail.text.Contains("0 / 1") && detail.text.Contains("250,00") && detail.text.Contains("4 рухи/с"),"Hold purchase preview");SetupCooking();return "U2b: unowned pointer fixture ready; advance a rendered frame.";
        }
        public static string CheckUnownedAndBuy()
        {
            var c=Controller;var pointer=Pointer;var order=c.OrderSession.ActiveOrder;int hp=order.RemainingClicks;
            Check(pointer.isActiveAndEnabled && pointer.IsInteractable(),"active collider pointer");
            typeof(CounterDishPointer).GetField("suppressReleaseClick",Private).SetValue(pointer,true);
            pointer.OnPointerDown(Event());Check(order.RemainingClicks==hp && !c.OrderSession.IsHolding,"fresh unowned down clears old run's consumed gesture without attacking");
            pointer.OnPointerUp(Event());pointer.OnPointerClick(Event());Check(order.RemainingClicks==hp-13,"unowned release click still one critical hit");pointer.OnSubmit(new BaseEventData(EventSystem.current));Check(order.RemainingClicks==hp-26,"Submit stays discrete");
            FinishCooking();c.SelectUpgradeNode(UpgradeNodeRules.Hold);var buy=Field<UnityEngine.UI.Button>(c,"buyUpgradeButton");buy.onClick.Invoke();buy.onClick.Invoke();
            Check(c.OrderSession.HasHold && c.OrderSession.Rewards.Wallet==750m,"one live Hold purchase");var loaded=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(loaded.GetUpgradeRank(UpgradeNodeRules.Hold)==1 && loaded.Rewards.Wallet==750m,"immediate Hold autosave");
            return "U2b UI PASS: unowned click/Submit unchanged, one Hold debit and autosave. Wait next frame.";
        }
        public static string SetupPurchasedCooking()
        {
            var c=Controller;var view=Field<UpgradeTreeView>(c,"upgradeTreeView");var detail=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");
            Check(!Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable && detail.text.Contains("1 / 1") && detail.text.Contains("Максимальний ранг"),"Hold max UI");
            var stats=view.transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>();Canvas.ForceUpdateCanvases();Check(stats.text.Contains("Утримання: 4 рухи/с") && stats.preferredHeight<=stats.rectTransform.rect.height+1,"actual Hold statistics fit");
            c.SelectUpgradeNode("node.hold_frequency.02");Check(!Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"later frequency tier not sold");SetupCooking();return "U2b purchased pointer fixture ready; advance a rendered frame.";
        }
        public static string BeginTimedHold()
        {
            var c=Controller;int hp=c.OrderSession.ActiveOrder.RemainingClicks;Pointer.OnPointerDown(Event());
            Check(c.OrderSession.IsHolding && c.OrderSession.ActiveOrder.RemainingClicks==hp-13,"initial down hit");Pointer.OnPointerDown(Event());Check(c.OrderSession.ActiveOrder.RemainingClicks==hp-13,"duplicate down suppressed");
            return "U2b initial down PASS; wait for ActiveOrder.RemainingClicks to change through live LateUpdate.";
        }
        public static string ReleaseTimedHold()
        {
            var c=Controller;var pointer=Pointer;pointer.OnPointerUp(Event(55));Check(c.OrderSession.IsHolding,"foreign up ignored");pointer.OnPointerUp(Event(-1,PointerEventData.InputButton.Right));Check(c.OrderSession.IsHolding,"right up ignored");
            pointer.OnPointerUp(Event());releasedHp=c.OrderSession.ActiveOrder.RemainingClicks;pointer.OnPointerClick(Event());
            Check(!c.OrderSession.IsHolding && c.OrderSession.ActiveOrder.RemainingClicks==releasedHp,"release/click adds no hit");return "U2b release PASS; wait beyond one interval before cancellation checks.";
        }
        public static string RunLifecycleChecks()
        {
            var c=Controller;var pointer=Pointer;Check(c.OrderSession.ActiveOrder.RemainingClicks==releasedHp,"no post-release repeat");
            void Press() { pointer.OnPointerDown(Event());Check(c.OrderSession.IsHolding,"fixture hold begins"); }
            void Stopped(string reason) { Check(!c.OrderSession.IsHolding && !c.OrderSession.TryTakeHoldRepeat(Time.unscaledTimeAsDouble+100),reason); }
            Press();pointer.OnPointerExit(Event());Stopped("pointer exit");pointer.OnPointerUp(Event());pointer.OnPointerClick(Event());
            Press();var menu=c.GetComponent<ProfileMenu>();menu.Pause();menu.Resume();Stopped("same-frame pause/resume");
            Press();pointer.SendMessage("OnApplicationFocus",false);pointer.SendMessage("OnApplicationFocus",true);Stopped("focus loss/return");
            Press();c.SendMessage("OnApplicationPause",true);c.SendMessage("OnApplicationPause",false);Stopped("system pause");
            Press();pointer.enabled=false;pointer.enabled=true;Stopped("pointer disable");
            Press();c.enabled=false;c.enabled=true;Stopped("controller disable");
            Press();EventSystem.current.SetSelectedGameObject(Field<UnityEngine.UI.Button>(c,"backToBoardButton").gameObject);Stopped("keyboard focus leaves dish");
            Press();c.BackToBoard();c.SelectOrder(0);Stopped("Back/reselect");int hp=c.OrderSession.ActiveOrder.RemainingClicks;pointer.OnPointerUp(Event());pointer.OnPointerClick(Event());Check(c.OrderSession.ActiveOrder.RemainingClicks==hp,"stale release does not hit reselected order");
            var order=c.OrderSession.ActiveOrder;typeof(PrototypeOrder).GetProperty("RemainingClicks").SetValue(order,13);pointer.OnPointerDown(Event());
            Check(order.IsCompleted && c.OrderSession.CompletedDishes==1 && !c.OrderSession.IsHolding,"initial-hit completion cancels");var paid=c.OrderSession.Rewards.Wallet;
            pointer.OnPointerUp(Event());pointer.OnPointerClick(Event());pointer.OnSubmit(new BaseEventData(EventSystem.current));Check(c.OrderSession.Rewards.Wallet==paid && c.OrderSession.CompletedDishes==1,"result blocks repeat and duplicate reward");
            c.ContinueFromDishResult();c.SelectOrder(0);Stopped("new replacement order has no carried Hold");
            c.BackToBoard();c.FinishDay();Stopped("end day");UnityEngine.Object.DestroyImmediate(playDish);UnityEngine.Object.DestroyImmediate(playLocation);playDish=null;playLocation=null;
            c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.Hold);Field<UpgradeTreeView>(c,"upgradeTreeView").RevealSelection();
            return "U2b lifecycle PASS: pointer exit/up, pause/resume, focus/system pause, disable, keyboard focus, Back/reselect, result/new order/end; no extra click or receipt.";
        }
        public static string VerifyRestart()
        {
            var c=Controller;Check(c.OrderSession.HasHold && c.OrderSession.ClickStrengthRank==3 && c.OrderSession.CriticalChanceRank==1 && c.OrderSession.CriticalMultiplierRank==1 && !c.OrderSession.IsHolding && !c.OrderSession.IsDayActive,"restart retains working ranks, no transient Hold/day");
            c.GetComponent<ProfileMenu>().Continue();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.Hold);Check(!Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"owned Hold max after Continue");return "U2b restart PASS: four node ranks restored, Hold idle, Continue max state.";
        }
    }
}
