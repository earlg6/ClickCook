using System;
using System.Collections.Generic;
using System.Globalization;
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
    public static class CriticalUpgradeVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value,string message) { if(!value) throw new Exception("U2a: "+message); }
        private static T Field<T>(object target,string name) => (T)target.GetType().GetField(name,Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Signed(int version,string payload)
        {
            using var sha=SHA256.Create();
            return JsonUtility.ToJson(new Envelope {version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))});
        }
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic)
            .Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static string Rank(string id,int rank) => "{\"id\":\""+id+"\",\"rank\":"+rank+"}";
        private static string Payload(string wallet,params string[] ranks) => "{\"wallet\":\""+wallet+"\",\"dishes\":[{\"id\":1,\"xp\":50}],\"upgrades\":["+string.Join(",",ranks)+"]}";
        private static PlayerProfile Profile(int strength,int chance=0,int multiplier=0) => Decode(Signed(2,Payload("10000000",Rank(UpgradeNodeRules.Strength,strength),Rank(UpgradeNodeRules.CriticalChance,chance),Rank(UpgradeNodeRules.CriticalMultiplier,multiplier))));
        private static PrototypeOrderSession Session(PlayerProfile profile,Func<double> roll=null)
        { var s=new PrototypeOrderSession(profile,roll); Check(s.ConfigureUpgrades(Catalog),"configure"); return s; }
        private static void Buy(PrototypeOrderSession s,string id) { Check(s.TryQuoteUpgrade(id,out var quote) && s.TryPurchaseUpgrade(quote),"purchase "+id); }

        public static string RunModelChecks()
        {
            var random=UnityEngine.Random.state;
            var dish=ScriptableObject.CreateInstance<DishData>(); var location=ScriptableObject.CreateInstance<LocationData>();
            try
            {
                var data=new SerializedObject(dish); data.FindProperty("dishId").intValue=1; data.FindProperty("clickRequirement").intValue=30;
                data.FindProperty("saleValue").intValue=5; data.ApplyModifiedPropertiesWithoutUndo();
                data=new SerializedObject(location);var pool=data.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;data.ApplyModifiedPropertiesWithoutUndo();
                var locked=Session(Profile(2)); Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.CriticalChance,out _),"chance prerequisite");
                var s=Session(Profile(3)); Check(!s.TryQuoteUpgrade(UpgradeNodeRules.CriticalMultiplier,out _),"multiplier prerequisite");
                Check(s.TryQuoteUpgrade(UpgradeNodeRules.CriticalChance,out var first) && first.Price==150m && first.NodeId==UpgradeNodeRules.CriticalChance,"first chance quote");
                Check(s.TryPurchaseUpgrade(first) && !s.TryPurchaseUpgrade(first),"node-specific replay protection");
                Check(s.ClickStrengthRank==3 && s.CriticalChanceRank==1 && s.CriticalMultiplierRank==0 && s.CriticalMultiplier==2m,"independent ranks and base multiplier");
                Check(s.TryQuoteUpgrade(UpgradeNodeRules.CriticalChance,out var next) && next.Price==285m,"chance second price");
                Buy(s,UpgradeNodeRules.CriticalMultiplier); Check(s.CriticalMultiplier==2.1m,"multiplier effect");
                s.TryQuoteUpgrade(UpgradeNodeRules.CriticalMultiplier,out var multi);
                Check(multi.Price==380m && !locked.TryPurchaseUpgrade(multi),"multiplier price and foreign quote");
                s.IsPaused=true; Check(!s.TryPurchaseUpgrade(multi),"paused purchase");s.IsPaused=false;
                s.StartDay(location,2); Check(!s.TryPurchaseUpgrade(multi),"active-day purchase"); s.EndDay();
                foreach(var id in new[]{"node.hold_frequency.02","node.critical_chance.02","node.critical_multiplier.02",UpgradeTreeDefinition.RootId,"critical_chance"})
                    Check(!s.TryQuoteUpgrade(id,out _),"unfinished or ambiguous ID "+id);
                var maximum=Session(Profile(3,15,15)); Check(!maximum.TryQuoteUpgrade(UpgradeNodeRules.CriticalChance,out _) && !maximum.TryQuoteUpgrade(UpgradeNodeRules.CriticalMultiplier,out _) && maximum.CriticalChance==.3d && maximum.CriticalMultiplier==3.5m,"caps");

                int rolls=0; double value=0;
                var zero=Session(Profile(3),()=>{rolls++;return 0;});zero.StartDay(location,2);zero.TrySelect(0);zero.TryCookClick(zero.ActiveOrder);
                Check(rolls==0 && !zero.LastHitWasCritical && zero.LastHitDamage==6,"zero chance preserves RNG/base damage");zero.EndDay();
                var combat=Session(Profile(3,1,1),()=>{rolls++;return value;});combat.StartDay(location,2);combat.TrySelect(0);
                foreach(var sample in new[]{(0d,true),(.019999999d,true),(.02d,false),(.999999d,false)})
                {
                    value=sample.Item1; var order=combat.ActiveOrder; combat.TryCookClick(order);
                    Check(combat.LastHitWasCritical==sample.Item2 && combat.LastHitDamage==(sample.Item2?13:6),"threshold and ceil "+value);
                    combat.ReturnToBoard();combat.TrySelect(0);
                }
                int before=rolls; combat.IsPaused=true;Check(!combat.TryCookClick(combat.ActiveOrder) && rolls==before,"paused hit consumes no RNG");combat.IsPaused=false;
                Check(!combat.TryCookClick(combat.GetOrderAtSlot(1)) && rolls==before,"foreign order consumes no RNG");
                value=0;var completing=combat.ActiveOrder;var untouched=combat.GetOrderAtSlot(1);var balance=combat.Rewards.Wallet;
                Check(!combat.TryCookClick(completing) && completing.RemainingClicks==17,"first critical damage");
                Check(!combat.TryCookClick(completing) && completing.RemainingClicks==4,"second critical damage");
                Check(combat.TryCookClick(completing) && completing.RemainingClicks==0 && combat.CompletedDishes==1,"bounded critical completion");
                var paid=combat.Rewards.Wallet;before=rolls;
                Check(paid>balance && !combat.TryCookClick(completing) && combat.Rewards.Wallet==paid && rolls==before && untouched.RemainingClicks==0,"reward once/no carry/no stale RNG");combat.EndDay();
                Check(combat.LastHitDamage==0 && !combat.LastHitWasCritical,"end clears feedback");
                foreach(var pair in new[]{(3,0),(3,1),(4,3),(15,15)})
                {
                    var test=Session(Profile(pair.Item1,1,pair.Item2),()=>0);test.StartDay(location,1);test.TrySelect(0);test.TryCookClick(test.ActiveOrder);
                    Check(test.LastHitDamage==(int)decimal.Ceiling(UpgradeCatalog.ClickDamageAtRank(pair.Item1)*(2m+.1m*pair.Item2)),"round once at damage extremes");test.EndDay();
                }
                return "U2a model PASS: node quotes/prerequisites/prices/caps, exact critical threshold/rounding, zero/rejected RNG, bounded damage and one receipt.";
            }
            finally { UnityEngine.Random.state=random;UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/U2aStore/"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
            string path=Path.Combine(folder,"profile.json");
            string legacy=Signed(1,Payload("12345678901234567890.25",Rank("click_strength",7)));
            File.WriteAllText(path,legacy);var store=new PlayerProfileStore(path);var migrated=store.Load();
            Check(migrated!=null && migrated.Rewards.Wallet==12345678901234567890.25m && migrated.CaptureSnapshot().DishXpById[1]==50,"exact wallet/XP migration");
            Check(migrated.GetUpgradeRank("click_strength")==7 && migrated.GetUpgradeRank(UpgradeNodeRules.Strength)==7 && migrated.CaptureSnapshot().UpgradeRanksById.Count==1,"legacy maps once to node");
            Check(File.ReadAllText(path)==legacy,"load does not rewrite");
            var s=Session(migrated);Check(s.ClickDamage==51,"same migrated strength");Buy(s,UpgradeNodeRules.CriticalChance);Buy(s,UpgradeNodeRules.CriticalMultiplier);
            Check(store.Save(migrated) && File.ReadAllText(store.BackupPath)==legacy,"schema-1 backup retained on upgrade");
            string current=File.ReadAllText(path);Check(JsonUtility.FromJson<Envelope>(current).version==PlayerProfileStore.CurrentVersion,"schema 2 write");
            var reloaded=new PlayerProfileStore(path).Load();Check(reloaded.Rewards.Wallet==migrated.Rewards.Wallet && reloaded.GetUpgradeRank(UpgradeNodeRules.CriticalChance)==1 && reloaded.GetUpgradeRank(UpgradeNodeRules.CriticalMultiplier)==1 && reloaded.GetUpgradeRank("click_strength")==7,"all node ranks roundtrip");
            File.WriteAllText(path,"broken");var recovery=new PlayerProfileStore(path);Check(recovery.Load().GetUpgradeRank("click_strength")==7 && recovery.Status==ProfileLoadStatus.Recovered,"legacy backup remains readable");
            foreach(var ranks in new[]{
                new[]{Rank("click_strength",3)},new[]{Rank(UpgradeTreeDefinition.RootId,1)},new[]{Rank("node.critical_chance.02",1)},
                new[]{Rank(UpgradeNodeRules.CriticalChance,1)},new[]{Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.CriticalMultiplier,1)},
                new[]{Rank(UpgradeNodeRules.Strength,16)},new[]{Rank(UpgradeNodeRules.Strength,-1)},new[]{Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.Strength,3)} })
                Check(Decode(Signed(2,Payload("100",ranks)))==null,"signed invalid schema-2 state");
            Check(Decode(Signed(1,Payload("100",Rank(UpgradeNodeRules.Strength,3))))==null,"schema 1 disallows canonical IDs");
            string future=Signed(PlayerProfileStore.CurrentVersion+1,Payload("100",Rank(UpgradeNodeRules.Strength,3)));File.WriteAllText(path,future);
            var protectedStore=new PlayerProfileStore(path);Check(protectedStore.Load()==null && protectedStore.Status==ProfileLoadStatus.Unsupported && !protectedStore.Save(migrated) && File.ReadAllText(path)==future,"future version protected");
            Check(!store.Save(migrated),"future file appearing after load protected");
            return "U2a storage PASS: exact schema-1 migration, schema-2 roundtrip, legacy backup, canonical whitelist/bounds/prerequisites and future-version protection. Artifacts: "+folder;
        }

        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying,"prepare before Play");
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/ClickCookSaveTests/U2a-"+Guid.NewGuid().ToString("N"),"profile.json"));Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path,Signed(1,Payload("5000",Rank("click_strength",3))));
            SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;
        }
        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        public static string RunPurchaseUi()
        {
            var c=Controller;Check(PlayerProfileRuntime.Store.SavePath.Contains("U2a-"),"isolated migrated fixture");
            c.GetComponent<ProfileMenu>().Continue();c.OpenUpgrades();
            Check(c.OrderSession.ClickStrengthRank==3 && c.OrderSession.Rewards.Wallet==5000m,"live migration");
            var buy=Field<UnityEngine.UI.Button>(c,"buyUpgradeButton");var details=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");
            c.SelectUpgradeNode(UpgradeNodeRules.CriticalMultiplier);Check(!buy.interactable && details.text.Contains("Заблоковано передумовами") && !details.text.Contains("Ефект ще не реалізовано"),"implemented multiplier locked state");
            c.SelectUpgradeNode("node.click_hold.01");Check(buy.interactable && details.text.Contains("250,00"),"hold is implemented in U2b");
            c.SelectUpgradeNode(UpgradeNodeRules.CriticalChance);Check(buy.interactable && details.text.Contains("0%") && details.text.Contains("2%") && details.text.Contains("150,00"),"critical preview");
            var menu=c.GetComponent<ProfileMenu>();menu.Pause();buy.onClick.Invoke();Check(c.OrderSession.CriticalChanceRank==0,"pause blocks new node purchase");menu.Resume();
            buy.onClick.Invoke();buy.onClick.Invoke();Check(c.OrderSession.CriticalChanceRank==1 && c.OrderSession.CriticalMultiplierRank==0 && c.OrderSession.Rewards.Wallet==4850m,"one live critical purchase");
            var loaded=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(loaded.GetUpgradeRank(UpgradeNodeRules.CriticalChance)==1 && loaded.GetUpgradeRank("click_strength")==3,"immediate autosave");
            return "U2a UI purchase PASS: migrated profile, lock vs unavailable, chance preview, pause, one debit and autosave. Wait next frame.";
        }
        public static string RunMultiplierUi()
        {
            var c=Controller;var details=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");
            Check(details.text.Contains("285,00"),"new chance quote next frame");
            c.SelectUpgradeNode(UpgradeNodeRules.CriticalMultiplier);var buy=Field<UnityEngine.UI.Button>(c,"buyUpgradeButton");
            Check(buy.interactable && details.text.Contains($"×{2m:0.0}") && details.text.Contains($"×{2.1m:0.0}") && details.text.Contains("200,00"),"multiplier unlocked preview");
            buy.onClick.Invoke();Check(c.OrderSession.CriticalMultiplierRank==1 && c.OrderSession.Rewards.Wallet==4650m,"multiplier live debit");
            return "U2a multiplier UI PASS: newly satisfied prerequisite, exact preview and debit. Wait next frame.";
        }
        public static string RunCookingUi()
        {
            var c=Controller;var view=Field<UpgradeTreeView>(c,"upgradeTreeView");
            var stats=view.transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>();Canvas.ForceUpdateCanvases();
            Check(stats.text.Contains("Крит: 2%") && stats.text.Contains($"×{2.1m:0.0}") && stats.preferredHeight<=stats.rectTransform.rect.height+1,"actual critical statistics fit");
            // Replace only this test session's roll provider; production input/controller remain unchanged.
            c.CloseUpgrades();var deterministic=Session(c.OrderSession.Profile,()=>0);typeof(ClickCookPrototypeController).GetField("session",Private).SetValue(c,deterministic);
            decimal wallet=c.OrderSession.Rewards.Wallet;
            c.CookSelectedLocation();
            var dish=UnityEngine.Object.Instantiate(c.OrderSession.GetOrderAtSlot(0).Dish);
            var location=UnityEngine.Object.Instantiate(c.OrderSession.SelectedLocation);
            try
            {
                var data=new SerializedObject(dish);data.FindProperty("clickRequirement").intValue=30;data.ApplyModifiedPropertiesWithoutUndo();
                data=new SerializedObject(location);var pool=data.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;data.ApplyModifiedPropertiesWithoutUndo();
                deterministic.StartDay(location,1);c.SelectOrder(0);var order=c.OrderSession.ActiveOrder;int hp=order.RemainingClicks;
                c.ClickCookingDish();Check(order.RemainingClicks==hp-13 && c.OrderSession.LastHitWasCritical,"accepted critical reaches real cooking");
                var text=Field<UnityEngine.UI.Text>(c,"cookingHpText");Canvas.ForceUpdateCanvases();
                Check(text.text.Contains("Крит: −13 HP") && text.preferredHeight<=text.rectTransform.rect.height+1,"critical HP feedback fits");
                c.BackToBoard();c.FinishDay();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.CriticalMultiplier);view.RevealSelection();
                var loaded=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(loaded.GetUpgradeRank(UpgradeNodeRules.CriticalChance)==1 && loaded.GetUpgradeRank(UpgradeNodeRules.CriticalMultiplier)==1 && loaded.Rewards.Wallet==wallet,"both node ranks saved without unfinished-order reward");
            }
            finally { deterministic.EndDay();UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location); }
            return "U2a cooking UI PASS: actual stats fit, deterministic critical through controller/HP, Back/Finish and both new ranks persisted.";
        }
    }
}
