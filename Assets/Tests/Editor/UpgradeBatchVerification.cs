using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    public static class UpgradeBatchVerification
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope{public int version;public string payload;public string checksum;}
        private static void Check(bool condition,string message){if(!condition)throw new Exception("Upgrade batch: "+message);}
        private static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,Private).GetValue(target);
        public static PlayerProfile Profile(Dictionary<string,int> ranks,decimal wallet=1000000000000.25m,int xp=0,int cents=0)
            =>(PlayerProfile)typeof(PlayerProfile).GetMethod("Restore",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{wallet,new Dictionary<int,int>{{1,xp},{2,17}},ranks,cents==0?new Dictionary<int,int>():new Dictionary<int,int>{{1,cents}}});
        private static PrototypeOrderSession Session(PlayerProfile p)
        {var s=new PrototypeOrderSession(p,()=>.99,()=>.99,()=>.99);Check(s.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset")),"configure");return s;}
        private static void Parents(string id,Dictionary<string,int> ranks)
        {
            var tree=UpgradeTreeDefinition.Load();var node=tree.Find(id);foreach(var need in node.prerequisites){if(need.nodeId==UpgradeTreeDefinition.RootId)continue;int rank=need.rank<0?UpgradeNodeRules.MaximumRank(need.nodeId):need.rank;if(!ranks.TryGetValue(need.nodeId,out int n)||n<rank)ranks[need.nodeId]=rank;Parents(need.nodeId,ranks);}
        }
        public static Dictionary<string,int> FullRanks()
        {
            var ranks=new Dictionary<string,int>{{UpgradeNodeRules.Strength,3},{UpgradeNodeRules.DishXpFlat,5},{UpgradeNodeRules.DishXpMultiplier,2},{UpgradeNodeRules.DishValue,15}};
            foreach(var tier in UpgradeNodeRules.AdditionalTiers){Parents(tier.Id,ranks);ranks[tier.Id]=tier.Cap;}return ranks;
        }
        private static string Seal(int version,string payload)
        {using var sha=SHA256.Create();return JsonUtility.ToJson(new Envelope{version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))});}
        public static string Json(int version,decimal wallet,IEnumerable<KeyValuePair<string,int>> ranks,int xp=50,int cents=37)
        {
            string entries=string.Join(",",ranks.Select(p=>"{\"id\":\""+p.Key+"\",\"rank\":"+p.Value+"}"));
            return Seal(version,"{\"wallet\":\""+wallet.ToString(CultureInfo.InvariantCulture)+"\",\"dishes\":[{\"id\":1,\"xp\":"+xp+(version>=13?",\"hundredths\":"+cents:"")+"},{\"id\":2,\"xp\":17}],\"upgrades\":["+entries+"]}");
        }
        private static PlayerProfile Decode(string json)=>(PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static void SetDish(DishData d,int hp)
        {var so=new SerializedObject(d);so.FindProperty("dishId").intValue=1;so.FindProperty("clickRequirement").intValue=hp;so.FindProperty("saleValue").intValue=5;so.ApplyModifiedPropertiesWithoutUndo();}
        private static LocationData Location(DishData dish)
        {var l=ScriptableObject.CreateInstance<LocationData>();var so=new SerializedObject(l);var pool=so.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;so.ApplyModifiedPropertiesWithoutUndo();return l;}
        public static bool AvailabilityMatches(PrototypeOrderSession s,string id) => s.TryQuoteUpgrade(id,out _) == (UpgradeNodeRules.MaximumRank(id)>0 && s.Profile.GetUpgradeRank(id)<UpgradeNodeRules.MaximumRank(id) && UpgradeNodeRules.PrerequisitesMet(id,s.Profile.GetUpgradeRank));
        public static string RunModelChecks()
        {
            var random=UnityEngine.Random.state;var dish=ScriptableObject.CreateInstance<DishData>();SetDish(dish,1);var location=Location(dish);
            try
            {
                foreach(var rule in UpgradeNodeRules.AdditionalTiers)
                {
                    var ranks=new Dictionary<string,int>();Parents(rule.Id,ranks);var buyer=Session(Profile(ranks));var foreign=Session(Profile(new Dictionary<string,int>(ranks)));decimal spent=0;
                    for(int n=0;n<rule.Cap;n++)
                    {
                        decimal raw=rule.BasePrice;for(int i=0;i<n;i++)raw*=rule.Growth/100m;decimal expected=decimal.Ceiling(decimal.Ceiling(raw)*(1-buyer.UpgradeDiscount));
                        Check(buyer.TryQuoteUpgrade(rule.Id,out var q)&&q.Price==expected,"independent catalog/cap/price "+rule.Id);buyer.IsPaused=true;Check(!buyer.TryPurchaseUpgrade(q),"pause");buyer.IsPaused=false;buyer.StartDay(location,1);Check(!buyer.TryPurchaseUpgrade(q),"day");buyer.EndDay();Check(!foreign.TryPurchaseUpgrade(q)&&buyer.TryPurchaseUpgrade(q)&&!buyer.TryPurchaseUpgrade(q),"owner/replay");spent+=expected;
                    }
                    Check(buyer.Profile.GetUpgradeRank(rule.Id)==rule.Cap&&!buyer.TryQuoteUpgrade(rule.Id,out _)&&buyer.Rewards.Wallet==1000000000000.25m-spent,"own cap/exact debit");
                    var bad=new Dictionary<string,int>(ranks);bad[rule.Parent]=(rule.ParentRank<0?UpgradeNodeRules.MaximumRank(rule.Parent):rule.ParentRank)-1;Check(!Session(Profile(bad)).TryQuoteUpgrade(rule.Id,out _),"purchased parent boundary");
                }
                var discountRanks=new Dictionary<string,int>{{UpgradeNodeRules.Strength,1},{UpgradeNodeRules.DishValue,1}};var discounted=Session(Profile(discountRanks,20000.25m));
                decimal[] self={300,594,1176,2328,4608};
                for(int n=0;n<5;n++)
                {Check(discounted.TryQuoteUpgrade(UpgradeNodeRules.Strength,out var stale),"other quote");Check(discounted.TryQuoteUpgrade(UpgradeNodeRules.UpgradeDiscount,out var q)&&q.Price==self[n],"explicit self price");Check(discounted.TryPurchaseUpgrade(q)&&!discounted.TryPurchaseUpgrade(stale),"discount invalidates even rounded-equal other quote");}
                Check(discounted.Rewards.Wallet==10994.25m&&discounted.UpgradeDiscount==.05m,"self total/cap");
                foreach(var node in UpgradeTreeDefinition.Load().nodes.Where(n=>n.IsWorking))
                {int maximum=UpgradeNodeRules.MaximumRank(node.id);for(int rank=0;rank<maximum;rank++)Check(discounted.UpgradePriceForRank(node.id,rank)>=1&&discounted.UpgradePriceForRank(node.id,rank)==decimal.Truncate(discounted.UpgradePriceForRank(node.id,rank)),"all active prices positive integral");}
                var full=Session(Profile(FullRanks(),1000.25m,0,37));Check(full.DishXpBonus==125&&full.DishValueMultiplier==2.20m,"aggregate caps");full.StartDay(location,2);full.TrySelect(0);int changes=0;full.Profile.Changed+=()=>changes++;UnityEngine.Random.InitState(1);Check(full.TryCookClick(full.ActiveOrder),"aggregate completion");
                int[] baseXp={10,12,16,22,30};decimal[] quality={1,1.25m,1.75m,2.5m,4};var result=full.LastResult;Check(result.XpGained==(baseXp[(int)result.Rarity]+125)*1.14m&&result.Payout==decimal.Round(5*quality[(int)result.Rarity]*2.20m,2,MidpointRounding.AwayFromZero)&&full.Rewards.GetExactTotalXp(dish)==.37m+result.XpGained&&full.Rewards.Wallet==1000.25m+result.Payout&&changes==1,"aggregate shared receipt/XP fraction/payout/one notification");full.EndDay();
                return "Batch A model PASS: independent tier caps/prices/guards, explicit self prices, all-price positive integral, stale discount quotes and aggregate reward caps.";
            }
            finally{UnityEngine.Random.state=random;UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location);}
        }
        public static string RunStorageChecks()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/UpgradeBatchStore/"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
            string[] oldIds={UpgradeNodeRules.Strength,UpgradeNodeRules.CriticalChance,UpgradeNodeRules.CriticalMultiplier,UpgradeNodeRules.Hold,UpgradeNodeRules.HoldFrequency,UpgradeNodeRules.DoubleStrikeChance,UpgradeNodeRules.DoubleStrikePower,UpgradeNodeRules.DoubleStrikeChanceII,UpgradeNodeRules.TripleStrikeChance,UpgradeNodeRules.AutoActionCount,UpgradeNodeRules.AutoActionChance,UpgradeNodeRules.AutoStrength,UpgradeNodeRules.AutoStrengthMultiplier,UpgradeNodeRules.AutoFrequency,UpgradeNodeRules.DishXpFlat,UpgradeNodeRules.DishXpMultiplier,UpgradeNodeRules.DishValue};
            for(int version=1;version<=14;version++)
            {
                int length=version>=6?version+3:version==5?7:version+1;var ranks=oldIds.Take(length).ToDictionary(id=>id,id=>id==UpgradeNodeRules.Strength?3:id==UpgradeNodeRules.DoubleStrikeChance?10:id==UpgradeNodeRules.Hold?1:1);if(version==1){ranks.Clear();ranks.Add("click_strength",3);}string old=Json(version,1000.25m,ranks),path=Path.Combine(folder,"v"+version+".json");File.WriteAllText(path,old);var store=new PlayerProfileStore(path);var p=store.Load();Check(p!=null&&File.ReadAllText(path)==old,"old profile loads without rewrite");var before=p.CaptureSnapshot();Check(store.Save(p)&&File.ReadAllText(store.BackupPath)==old,"old backup");var after=new PlayerProfileStore(path).Load().CaptureSnapshot();Check(after.Wallet==before.Wallet&&after.DishXpById[1]==50&&after.DishXpHundredthsById.Count==before.DishXpHundredthsById.Count&&before.UpgradeRanksById.All(x=>after.UpgradeRanksById[x.Key]==x.Value)&&before.DishXpHundredthsById.All(x=>after.DishXpHundredthsById[x.Key]==x.Value),"old values exact");
                foreach(var rule in UpgradeNodeRules.AdditionalTiers){var invalid=new Dictionary<string,int>(ranks){{rule.Id,0}};Check(Decode(Json(version,1000,invalid))==null,"new ID rejected in old schema");}
            }
            var full=FullRanks();string current=Path.Combine(folder,"full.json");var writer=new PlayerProfileStore(current);Check(writer.Save(Profile(full,1234.56m,50,37)),"new save");var loaded=new PlayerProfileStore(current).Load().CaptureSnapshot();Check(loaded.Wallet==1234.56m&&loaded.DishXpHundredthsById[1]==37&&full.All(x=>loaded.UpgradeRanksById[x.Key]==x.Value),"all new nodes exact roundtrip");
            foreach(var rule in UpgradeNodeRules.AdditionalTiers)
            {
                foreach(int badRank in new[]{-1,rule.Cap+1}){var invalid=new Dictionary<string,int>(full);invalid[rule.Id]=badRank;Check(Decode(Json(PlayerProfileStore.CurrentVersion,1000,invalid))==null,"invalid rank");}
                var missing=new Dictionary<string,int>(full);missing[rule.Parent]=0;Check(Decode(Json(PlayerProfileStore.CurrentVersion,1000,missing))==null,"invalid prerequisite");
            }
            string future=Path.Combine(folder,"future.json");File.WriteAllText(future,Json(PlayerProfileStore.CurrentVersion+1,1000,full));var blocked=new PlayerProfileStore(future);Check(blocked.Load()==null&&blocked.Status==ProfileLoadStatus.Unsupported&&!blocked.Save(Profile(full)),"future protected");return "Batch A storage PASS: schemas 1–14 migration/new ID boundary, exact old values/backup, all new nodes/fractions roundtrip, invalid ranks/prerequisites and future protection.";
        }
        private static ClickCookPrototypeController Controller=>UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        public static string PreparePlay()
        {Check(!EditorApplication.isPlaying,"prepare stopped");string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/ClickCookSaveTests/BatchA-"+Guid.NewGuid().ToString("N"),"profile.json"));Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,Json(14,2000.25m,new Dictionary<string,int>{{UpgradeNodeRules.Strength,3},{UpgradeNodeRules.DishValue,15},{UpgradeNodeRules.DishXpFlat,5},{UpgradeNodeRules.DishXpMultiplier,2}}));SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;}
        public static string ShowDiscount()
        {var c=Controller;Check(PlayerProfileRuntime.Store.SavePath.Contains("BatchA-"),"isolated");c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.UpgradeDiscount);Field<UpgradeTreeView>(c,"upgradeTreeView").FocusNode(UpgradeNodeRules.UpgradeDiscount);return "Batch A native discount purchase ready.";}
        public static string CheckDiscountPurchase()
        {var c=Controller;Check(c.OrderSession.UpgradeDiscountRank==1&&c.OrderSession.Rewards.Wallet==1700.25m,"native discount single debit");var disk=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(disk.GetUpgradeRank(UpgradeNodeRules.UpgradeDiscount)==1&&disk.Rewards.Wallet==1700.25m,"discount autosave");c.GetComponent<ProfileMenu>().Resume();c.SelectUpgradeNode(UpgradeNodeRules.DishXpFlatII);var text=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");Check(text.text.Contains(ClickCookUiCopy.Money(2475))&&!Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"discounted unaffordable tier matches detail");c.SelectUpgradeNode(UpgradeNodeRules.UpgradeDiscount);Check(Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text.Contains(ClickCookUiCopy.Money(594)),"self next discounted price");var stats=Field<UpgradeTreeView>(c,"upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>();Canvas.ForceUpdateCanvases();Check(stats.text.Split('\n').Length==9&&stats.preferredHeight<=stats.rectTransform.rect.height+1,"nine stats fit");return "Batch A native/UI/discounted prices/autosave PASS.";}
        public static string VerifyRestart()
        {var c=Controller;Check(PlayerProfileRuntime.Store.SavePath.Contains("BatchA-")&&c.OrderSession.UpgradeDiscountRank==1&&c.OrderSession.Rewards.Wallet==1700.25m&&c.OrderSession.Profile.CaptureSnapshot().DishXpHundredthsById[1]==37&&!c.OrderSession.IsDayActive,"discount/remainder restart");c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();return "Batch A restart/Continue PASS.";}
    }
}