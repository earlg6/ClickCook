using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    public static class ExtendedCookingVerification
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        private static void Check(bool value,string message){if(!value)throw new Exception("Extended cooking: "+message);}
        private static T Field<T>(object obj,string name)=>(T)obj.GetType().GetField(name,Private).GetValue(obj);
        public static Dictionary<string,int> MaxRanks()
        {
            var ranks=UpgradeBatchVerification.FullRanks();foreach(var node in UpgradeTreeDefinition.Load().nodes.Where(n=>n.IsWorking))ranks[node.id]=UpgradeNodeRules.MaximumRank(node.id);return ranks;
        }
        private static PrototypeOrderSession Session(Dictionary<string,int> ranks,Func<double> crit=null,Func<double> proc=null,Func<double> start=null)
        {var s=new PrototypeOrderSession(UpgradeBatchVerification.Profile(ranks),crit??(()=>.99),proc??(()=>.99),start??(()=>.99));Check(s.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset")),"configure");return s;}
        private static void Hp(DishData d,int hp)
        {var so=new SerializedObject(d);so.FindProperty("dishId").intValue=1;so.FindProperty("clickRequirement").intValue=hp;so.FindProperty("saleValue").intValue=5;so.ApplyModifiedPropertiesWithoutUndo();}
        private static PrototypeOrder Begin(PrototypeOrderSession s,LocationData l){s.StartDay(l,2);Check(s.TrySelect(0),"select");return s.ActiveOrder;}
        public static string RunModelChecks()
        {
            var random=UnityEngine.Random.state;var dish=ScriptableObject.CreateInstance<DishData>();var location=ScriptableObject.CreateInstance<LocationData>();var ls=new SerializedObject(location);var pool=ls.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;ls.ApplyModifiedPropertiesWithoutUndo();
            try
            {
                var ranks=MaxRanks();var max=Session(ranks);Check(max.ClickDamage==101078&&max.ClickMultiplier==12.6m&&max.CriticalChance==.5&&max.CriticalMultiplier==7.75m&&max.DoubleStrikeChance==.35&&max.TripleStrikeChance==.15&&max.AdditionalStrikeChance==.5&&max.DoubleStrikePower==1.5m,"independent manual maxima");
                Check(max.HoldFrequency==14&&Math.Abs(max.HoldRepeatInterval-1d/14)<1e-12&&max.AutoActionChance==.45&&max.AutoActionsPerTick==31&&max.AutoBaseDamage==846&&max.AutoStrengthMultiplier==4&&max.AutoDamagePerAction==3384&&max.AutoFrequency==10&&max.AutoInterval==.1,"independent auto/Hold maxima");
                Hp(dish,10000000);
                foreach(var sample in new[]{(roll:.349999,hit:2),(roll:.35,hit:3),(roll:.499999,hit:3),(roll:.5,hit:1)})
                {int crits=0,procs=0;var s=Session(ranks,()=>{crits++;return .99;},()=>{procs++;return sample.roll;});var order=Begin(s,location);s.TryCookClick(order);int expected=101078+(sample.hit-1)*151617;Check(s.LastStrikeCount==sample.hit&&s.LastHitDamage==expected&&order.RemainingClicks==10000000-expected&&procs==1&&crits==sample.hit,"exclusive proc boundary/independent rolls "+sample.roll);s.EndDay();}
                var critical=Session(ranks,()=>0,()=>.35);var active=Begin(critical,location);critical.TryCookClick(active);Check(critical.LastStrikeCount==3&&critical.LastHitDamage==3133419&&active.RemainingClicks==6866581,"crit+extra power one-round damage oracle");critical.EndDay();
                var auto=Session(ranks,()=>throw new Exception("auto crit RNG"),()=>throw new Exception("auto proc RNG"),()=>0);active=Begin(auto,location);auto.TryCookAutoStart(active);Check(active.RemainingClicks==9895096&&auto.LastAutoActionCount==31&&auto.LastHitDamage==104904,"max start batch/plain RNG isolation");auto.TryCookAuto(active,0);auto.TryCookAuto(active,.099999);Check(active.RemainingClicks==9895096,"exact first interval");auto.TryCookAuto(active,.1);Check(active.RemainingClicks==9790192,"max periodic batch");auto.TryCookAuto(active,100);auto.TryCookAuto(active,100);auto.TryCookAuto(active,100.099999);Check(active.RemainingClicks==9685288,"one delayed batch/no backlog");auto.IsPaused=true;auto.IsPaused=false;auto.TryCookAuto(active,200);auto.TryCookAuto(active,200.099999);Check(active.RemainingClicks==9685288,"pause fresh interval");auto.TryCookAuto(active,200.1);Check(active.RemainingClicks==9580384,"resume max rate");auto.ReturnToBoard();auto.TrySelect(0);Check(!auto.TryCookAutoStart(auto.ActiveOrder)&&auto.ActiveOrder.RemainingClicks==10000000,"Back cannot reroll/max damage no carry");auto.EndDay();
                foreach(var sample in new[]{(roll:.449999,hit:true),(roll:.45,hit:false)}){var s=Session(ranks,start:()=>sample.roll);active=Begin(s,location);s.TryCookAutoStart(active);Check((active.RemainingClicks<10000000)==sample.hit,"strict 45% start boundary");s.EndDay();}
                var hold=Session(ranks);active=Begin(hold,location);Check(hold.TryBeginHold(active,0)&&!hold.TryTakeHoldRepeat(1d/14-1e-6)&&hold.TryTakeHoldRepeat(1d/14)&&!hold.TryTakeHoldRepeat(1d/14)&&hold.TryTakeHoldRepeat(100)&&!hold.TryTakeHoldRepeat(100),"14Hz hold/no backlog");hold.IsPaused=true;Check(!hold.IsHolding&&!hold.TryTakeHoldRepeat(200),"hold pause cleanup");hold.EndDay();
                foreach(int hp in new[]{1,3384,3385,104904,104905})
                {Hp(dish,hp);var s=Session(ranks);var order=Begin(s,location);int changes=0;s.Profile.Changed+=()=>changes++;s.TryCookAuto(order,0);bool done=s.TryCookAuto(order,.1);if(hp==104905){Check(!done&&order.RemainingClicks==1,"max batch partial");done=s.TryCookAuto(order,.2);}int expectedActions=hp==104905?1:(hp+3383)/3384;Check(done&&order.RemainingClicks==0&&s.LastAutoActionCount==expectedActions&&s.LastHitDamage==3384*expectedActions&&s.CompletedDishes==1&&changes==1,"bounded max-auto completion");decimal wallet=s.Rewards.Wallet;Check(!s.TryCookAuto(order,200)&&!s.TryCookClick(order)&&s.Rewards.Wallet==wallet&&changes==1,"max stale single reward");s.TrySelect(0);Check(s.ActiveOrder.RemainingClicks==hp,"no overflow/carry to replacement");s.EndDay();}
                return "Batch B model PASS: bounded aggregate maxima, strict proc/start boundaries, independent crit/extra damage, auto RNG isolation, 14Hz Hold/10Hz auto/no backlog/reset and max-batch single completion.";
            }
            finally{UnityEngine.Random.state=random;UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location);}
        }
        public static string RunVersionBoundaryChecks()
        {
            var all=MaxRanks();foreach(var rule in UpgradeNodeRules.AdditionalTiers.Where(r=>r.Version==16))for(int version=1;version<=15;version++)
            {var ranks=new Dictionary<string,int>{{UpgradeNodeRules.Strength,3},{rule.Id,0}};if(version==1){ranks.Remove(UpgradeNodeRules.Strength);ranks.Add("click_strength",3);}var decoded=typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{UpgradeBatchVerification.Json(version,1000,ranks,50,version>=13?37:0),ProfileLoadStatus.Invalid});Check(decoded==null,"version-specific new ID zero rejection");}
            string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/BatchBStore/"+Guid.NewGuid().ToString("N"),"profile.json"));var writer=new PlayerProfileStore(path);Check(writer.Save(UpgradeBatchVerification.Profile(all,1234.56m,50,37)),"full maxima save");var loaded=new PlayerProfileStore(path).Load().CaptureSnapshot();Check(all.All(x=>loaded.UpgradeRanksById[x.Key]==x.Value)&&loaded.DishXpHundredthsById[1]==37&&loaded.Wallet==1234.56m,"all tier maxima/fractions exact restore");return "Batch B schema PASS: all 27 IDs reject earlier versions including rank zero, exact maximum-tier roundtrip.";
        }
        private static ClickCookPrototypeController Controller=>UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying,"prepare stopped");var ranks=MaxRanks();ranks["node.auto_strength_multiplier.02"]=0;ranks["node.click_strength.06"]=0;string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/ClickCookSaveTests/BatchB-"+Guid.NewGuid().ToString("N"),"profile.json"));Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,UpgradeBatchVerification.Json(16,10000000000.25m,ranks));SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;
        }
        public static string ShowPurchase(string id)
        {var c=Controller;Check(PlayerProfileRuntime.Store.SavePath.Contains("BatchB-"),"isolated");c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();c.OpenUpgrades();c.SelectUpgradeNode(id);Field<UpgradeTreeView>(c,"upgradeTreeView").FocusNode(id);return "Batch B purchase "+id;}
        public static string CheckPurchase(string id)
        {var c=Controller;int rank=c.OrderSession.Profile.GetUpgradeRank(id);Check(rank==1,"native tier rank");var p=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(p.GetUpgradeRank(id)==1&&p.Rewards.Wallet==c.OrderSession.Rewards.Wallet,"tier native debit autosave");return "Native tier/autosave PASS: "+id;}
        public static string CheckUi()
        {
            var c=Controller;c.GetComponent<ProfileMenu>().Resume();var s=c.OrderSession;Check(s.AutoActionsPerTick==31&&s.AutoFrequency==10&&s.AutoStrengthMultiplier==2.2m&&s.AutoDamagePerAction==1862&&s.ClickDamage==80918,"live mixed tier stats");var stats=Field<UpgradeTreeView>(c,"upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>();Canvas.ForceUpdateCanvases();Check(stats.text.Contains("31×1862 HP")&&stats.text.Contains("50%")&&stats.preferredHeight<=stats.rectTransform.rect.height+1&&stats.preferredWidth<=stats.rectTransform.rect.width+1,"aggregate nine-line fit");c.SelectUpgradeNode(UpgradeNodeRules.AutoStrength);Check(Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text.Contains("1862 HP"),"first-tier preview includes later contributions");SessionState.SetString("ClickCook.BatchBWallet",s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture));return "Batch B live aggregate/preview/fit PASS.";
        }
        public static string VerifyRestart()
        {var c=Controller;var s=c.OrderSession;Check(PlayerProfileRuntime.Store.SavePath.Contains("BatchB-")&&s.Profile.GetUpgradeRank("node.click_strength.06")==1&&s.Profile.GetUpgradeRank("node.auto_strength_multiplier.02")==1&&s.Rewards.Wallet.ToString(System.Globalization.CultureInfo.InvariantCulture)==SessionState.GetString("ClickCook.BatchBWallet","")&&s.Profile.CaptureSnapshot().DishXpHundredthsById[1]==37&&!s.IsDayActive,"new tiers/wallet/fraction/idle restore");c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();return "Batch B restart/Continue PASS.";}
    }
}