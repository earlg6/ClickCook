using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public static class DishValueVerification
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope {public int version;public string payload;public string checksum;}
        private static void Check(bool value,string message){if(!value)throw new Exception("U5c: "+message);}
        private static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,Private).GetValue(target);
        private static UpgradeCatalog Catalog=>AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id,int rank)=>"{\"id\":\""+id+"\",\"rank\":"+rank+"}";
        private static string Signed(int version,decimal wallet,int xp,int cents,params string[] ranks)
        {
            string fraction=version>=13?",\"hundredths\":"+cents:"";
            string payload="{\"wallet\":\""+wallet.ToString(CultureInfo.InvariantCulture)+"\",\"dishes\":[{\"id\":1,\"xp\":"+xp+fraction+"},{\"id\":2,\"xp\":17}],\"upgrades\":["+string.Join(",",ranks)+"]}";
            using var sha=SHA256.Create();return JsonUtility.ToJson(new Envelope{version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))});
        }
        private static string[] Ranks(int value=0)=>new[]{Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.CriticalChance,1),Rank(UpgradeNodeRules.CriticalMultiplier,1),Rank(UpgradeNodeRules.Hold,1),Rank(UpgradeNodeRules.HoldFrequency,1),Rank(UpgradeNodeRules.DoubleStrikeChance,10),Rank(UpgradeNodeRules.DoubleStrikePower,1),Rank(UpgradeNodeRules.DoubleStrikeChanceII,1),Rank(UpgradeNodeRules.TripleStrikeChance,1),Rank(UpgradeNodeRules.AutoActionCount,1),Rank(UpgradeNodeRules.AutoActionChance,1),Rank(UpgradeNodeRules.AutoStrength,2),Rank(UpgradeNodeRules.AutoStrengthMultiplier,2),Rank(UpgradeNodeRules.AutoFrequency,2),Rank(UpgradeNodeRules.DishXpFlat,2),Rank(UpgradeNodeRules.DishXpMultiplier,2),Rank(UpgradeNodeRules.DishValue,value)};
        private static PlayerProfile Decode(string json)=>(PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static PrototypeOrderSession Session(PlayerProfile p,bool start=false)
        {var s=new PrototypeOrderSession(p,()=>0,()=>.22,()=>start?0:.99);Check(s.ConfigureUpgrades(Catalog),"configure");return s;}
        private static PrototypeOrderSession Session(int value,int xp=0,int cents=0,bool start=false)=>Session(Decode(Signed(14,1000000.25m,xp,cents,Ranks(value))),start);
        private static void SetDish(DishData d,int hp,int sale=5)
        {var so=new SerializedObject(d);so.FindProperty("dishId").intValue=1;so.FindProperty("clickRequirement").intValue=hp;so.FindProperty("saleValue").intValue=sale;so.ApplyModifiedPropertiesWithoutUndo();}
        private static void SetPool(LocationData l,DishData d)
        {var so=new SerializedObject(l);var pool=so.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=d;so.ApplyModifiedPropertiesWithoutUndo();}
        private static PrototypeOrder Begin(PrototypeOrderSession s,LocationData l){s.StartDay(l,2);Check(s.TrySelect(0),"select");return s.ActiveOrder;}
        private static int XpAtLevel(int level)=>Enumerable.Range(1,level-1).Sum(PrototypeDishRewards.RequiredXp);
        private static int SeedFor(int level,DishRarity rarity)
        {for(int seed=0;seed<10000;seed++){UnityEngine.Random.InitState(seed);if(PrototypeDishRewards.ResolveRarity(level,UnityEngine.Random.Range(0,100))==rarity)return seed;}throw new Exception("No rarity seed");}

        public static string RunModelChecks()
        {
            var random=UnityEngine.Random.state;var dish=ScriptableObject.CreateInstance<DishData>();var location=ScriptableObject.CreateInstance<LocationData>();
            try
            {
                SetDish(dish,1);SetPool(location,dish);
                var locked=Session(Decode(Signed(14,1000,0,0,Rank(UpgradeNodeRules.Strength,0))));Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.DishValue,out _),"strength prerequisite");
                var poor=Session(Decode(Signed(14,99.99m,0,0,Ranks())));Check(poor.TryQuoteUpgrade(UpgradeNodeRules.DishValue,out var costly)&&!poor.TryPurchaseUpgrade(costly)&&poor.Rewards.Wallet==99.99m,"insufficient funds");
                decimal[] prices={100,170,289,492,836,1420,2414,4104,6976,11859,20160,34272,58263,99046,168378};var buyer=Session(0);decimal spent=0;
                for(int n=0;n<15;n++)
                {Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.DishValue,out var q)&&q.Price==prices[n],"explicit price");buyer.IsPaused=true;Check(!buyer.TryPurchaseUpgrade(q),"pause purchase");buyer.IsPaused=false;buyer.StartDay(location,1);Check(!buyer.TryPurchaseUpgrade(q),"day purchase");buyer.EndDay();Check(!locked.TryPurchaseUpgrade(q)&&buyer.TryPurchaseUpgrade(q)&&!buyer.TryPurchaseUpgrade(q),"owner/replay");spent+=prices[n];Check(buyer.DishValueRank==n+1&&buyer.DishValueMultiplier==(100+3*(n+1))/100m,"rank factor");}
                Check(spent==408779&&buyer.Rewards.Wallet==1000000.25m-spent&&buyer.DishValueMultiplier==1.45m&&!buyer.TryQuoteUpgrade(UpgradeNodeRules.DishValue,out _),"cap/total");
                foreach(string id in new[]{"node.dish_value.05","node.upgrade_discount.02","node.recipe_discount.01","node.passive_unlock.01"})Check(!buyer.TryQuoteUpgrade(id,out _),"dependent effects remain unavailable");
                var tree=UpgradeTreeDefinition.Load();Check(tree.PrerequisitesMet(tree.Find("node.dish_value.02"),buyer.Profile.GetUpgradeRank),"tier II sees purchased max");
                int[] quality={100,125,175,250,400},awards={10,12,16,22,30};
                foreach(int sale in new[]{1,5,37})for(int rank=0;rank<=15;rank++)for(int tier=0;tier<5;tier++)
                {
                    SetDish(dish,1,sale);int level=tier==0?1:7,xp=XpAtLevel(level);var s=Session(rank,xp,37);var order=Begin(s,location);int changes=0;s.Profile.Changed+=()=>changes++;UnityEngine.Random.InitState(SeedFor(level,(DishRarity)tier));decimal before=s.Rewards.Wallet;Check(s.TryCookClick(order),"manual completion");
                    // Positive rational cent oracle; +50 implements half-up with integer arithmetic.
                    long numerator=(long)sale*quality[tier]*(100+3*rank);decimal expected=((numerator+50)/100)/100m;
                    decimal exactXp=xp+.37m+(awards[tier]+6)*1.14m;
                    Check(s.LastResult.Payout==expected&&s.Rewards.Wallet==before+expected&&decimal.Round(s.Rewards.Wallet,2)==s.Rewards.Wallet&&s.LastResult.Rarity==(DishRarity)tier&&s.LastResult.PreviousLevel==level&&s.Rewards.GetExactTotalXp(dish)==exactXp&&s.LastResult.XpGained==(awards[tier]+6)*1.14m&&changes==1&&s.Profile.CaptureSnapshot().DishXpById[2]==17,"all-rarity integer cent oracle/XP independence/single reward");
                    Check(!s.TryCookClick(order)&&!s.TryCookAutoStart(order)&&!s.TryCookAuto(order,100)&&s.Rewards.Wallet==before+expected&&s.Rewards.GetExactTotalXp(dish)==exactXp&&changes==1,"stale reward guard");s.EndDay();
                }
                SetDish(dish,1,1);var midpoint=Session(2);var selected=Begin(midpoint,location);UnityEngine.Random.InitState(SeedFor(1,DishRarity.Uncommon));midpoint.TryCookClick(selected);Check(midpoint.LastResult.Payout==1.33m,"explicit 1.325 half-up");midpoint.EndDay();
                SetDish(dish,1,5);var fractional=Session(1);selected=Begin(fractional,location);UnityEngine.Random.InitState(SeedFor(1,DishRarity.Uncommon));fractional.TryCookClick(selected);Check(fractional.LastResult.Payout==6.44m,"explicit 6.4375 final rounding");fractional.EndDay();
                foreach(string source in new[]{"hold","start","periodic"})
                {var s=Session(2,start:source=="start");var order=Begin(s,location);UnityEngine.Random.InitState(SeedFor(1,DishRarity.Common));bool done;if(source=="start")done=s.TryCookAutoStart(order);else if(source=="periodic"){s.TryCookAuto(order,0);done=s.TryCookAuto(order,s.AutoInterval);}else{Check(s.TryBeginHold(order,0),"hold begin");done=s.TryCookClick(order);}Check(done&&s.LastResult.Payout==5.30m&&s.Rewards.Wallet==1000005.55m&&s.LastResult.XpGained==18.24m&&s.CompletedDishes==1,"source parity/XP independence");s.EndDay();}
                var capped=Session(15,XpAtLevel(18));selected=Begin(capped,location);decimal prior=capped.Rewards.Wallet;capped.TryCookClick(selected);Check(capped.LastResult.XpGained==0&&capped.LastResult.Payout>0&&capped.Rewards.Wallet==prior+capped.LastResult.Payout,"mastered dishes still pay");capped.EndDay();
                SetDish(dish,1000);var partial=Session(15,0,37);selected=Begin(partial,location);partial.TryCookClick(selected);partial.ReturnToBoard();partial.EndDay();Check(partial.Rewards.Wallet==1000000.25m&&partial.Rewards.GetExactTotalXp(dish)==.37m,"partial/Back/end no payout");
                return "U5c model PASS: prices/cap/guards, three base prices × all qualities/ranks, rational cent oracle/midpoints, exact wallet/unchanged XP, all completion sources and single reward.";
            }
            finally{UnityEngine.Random.state=random;UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location);}
        }
        public static string RunStorageChecks()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/U5cStore/"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
            for(int version=1;version<=13;version++)
            {
                int length=version>=6?version+3:version==5?7:version+1;string[] ranks=version==1?new[]{Rank("click_strength",3)}:Ranks().Take(length).ToArray();int cents=version==13?37:0;string old=Signed(version,2500.25m,50,cents,ranks),path=Path.Combine(folder,"v"+version+".json");File.WriteAllText(path,old);var store=new PlayerProfileStore(path);var p=store.Load();var before=p.CaptureSnapshot();
                Check(p.GetUpgradeRank(UpgradeNodeRules.DishValue)==0&&Session(p).DishValueMultiplier==1&&File.ReadAllText(path)==old,"old identity/no rewrite");Check(store.Save(p)&&JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version==PlayerProfileStore.CurrentVersion&&File.ReadAllText(store.BackupPath)==old,"schema/backup");var after=new PlayerProfileStore(path).Load().CaptureSnapshot();
                Check(after.Wallet==2500.25m&&after.DishXpById[1]==50&&after.DishXpById[2]==17&&(after.DishXpHundredthsById.TryGetValue(1,out int remainder)?remainder:0)==cents&&before.UpgradeRanksById.Count==after.UpgradeRanksById.Count&&before.UpgradeRanksById.All(x=>after.UpgradeRanksById[x.Key]==x.Value),"exact legacy wallet/XP/fractions/ranks");File.WriteAllText(path,"broken");var recovery=new PlayerProfileStore(path);Check(recovery.Load()!=null&&recovery.Status==ProfileLoadStatus.Recovered,"old backup recovery");Check(Decode(Signed(version,1000,0,0,ranks.Concat(new[]{Rank(UpgradeNodeRules.DishValue,0)}).ToArray()))==null,"old schema rejects new ID even zero");
            }
            foreach(int rank in new[]{1,15})
            {string path=Path.Combine(folder,"r"+rank+".json");Check(new PlayerProfileStore(path).Save(Decode(Signed(14,6.63m,20,52,Ranks(rank)))),"value roundtrip write");var s=Session(new PlayerProfileStore(path).Load());Check(s.DishValueRank==rank&&s.DishValueMultiplier==(rank==1?1.03m:1.45m)&&s.Rewards.Wallet==6.63m&&s.Profile.CaptureSnapshot().DishXpById[1]==20&&s.Profile.CaptureSnapshot().DishXpHundredthsById[1]==52&&s.DishXpMultiplier==1.14m&&!s.IsDayActive&&!s.IsHolding&&!s.LastActionWasAuto,"independent cent-wallet/fraction/rank idle roundtrip");}
            foreach(var ranks in new[]{Ranks(-1),Ranks(16),new[]{Rank(UpgradeNodeRules.Strength,0),Rank(UpgradeNodeRules.DishValue,1)},Ranks(1).Concat(new[]{Rank(UpgradeNodeRules.DishValue,1)}).ToArray(),Ranks().Concat(new[]{Rank("node.dish_value.05",1)}).ToArray(),Ranks().Concat(new[]{Rank("dish_value",1)}).ToArray()})Check(Decode(Signed(14,1000,0,0,ranks))==null,"invalid cap/prerequisite/duplicate/tier/alias");
            string future=Signed(PlayerProfileStore.CurrentVersion + 1,1000,0,0,Ranks()),futurePath=Path.Combine(folder,"future.json");File.WriteAllText(futurePath,future);var blocked=new PlayerProfileStore(futurePath);Check(blocked.Load()==null&&blocked.Status==ProfileLoadStatus.Unsupported&&!blocked.Save(Decode(Signed(14,1000,0,0,Ranks())))&&File.ReadAllText(futurePath)==future,"future protected");return "U5c storage PASS: schemas 1–13 to 14, exact old wallet/XP/fractions/ranks/backups, independent roundtrip/whitelist/prerequisites and version 15 protection.";
        }
        private static ClickCookPrototypeController Controller=>UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish;private static LocationData playLocation;
        public static string PreparePlay()
        {Check(!EditorApplication.isPlaying,"prepare stopped");string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/ClickCookSaveTests/U5c-"+Guid.NewGuid().ToString("N"),"profile.json"));Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,Signed(13,300.25m,0,0,Ranks().Take(16).ToArray()));SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;}
        public static string ShowNativePurchase()
        {Check(PlayerProfileRuntime.Store.SavePath.Contains("U5c-"),"isolated profile");var c=Controller;c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.DishValue);Field<UpgradeTreeView>(c,"upgradeTreeView").FocusNode(UpgradeNodeRules.DishValue);Check(Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text.Contains("0 / 15")&&Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"ready native purchase");return "U5c ready, wallet 300.25.";}
        public static string CheckNativePurchase(int rank)
        {var s=Controller.OrderSession;decimal wallet=rank==1?200.25m:30.25m;var disk=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(s.DishValueRank==rank&&s.DishValueMultiplier==(rank==1?1.03m:1.06m)&&s.Rewards.Wallet==wallet&&disk.GetUpgradeRank(UpgradeNodeRules.DishValue)==rank&&disk.Rewards.Wallet==wallet&&disk.CaptureSnapshot().DishXpById[1]==0,"native single debit/factor/autosave");return "U5c native rank "+rank+" PASS.";}
        public static string CheckUi()
        {CheckNativePurchase(2);var c=Controller;c.GetComponent<ProfileMenu>().Resume();string detail=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text;Check(detail.Contains("2 / 15")&&detail.Contains(1.06m.ToString("0.##"))&&detail.Contains(1.09m.ToString("0.##"))&&!Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"detail factor/next/funds");var stats=Field<UpgradeTreeView>(c,"upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>();Canvas.ForceUpdateCanvases();Check(stats.text.Contains("Оплата за страву: ×"+1.06m.ToString("0.##"))&&stats.text.Split('\n').Length==9&&stats.preferredHeight<=stats.rectTransform.rect.height+1&&stats.preferredWidth<=stats.rectTransform.rect.width+1,"eight statistics fit");return "U5c UI PASS.";}
        public static string CompleteLive()
        {
            var c=Controller;c.GetComponent<ProfileMenu>().Resume();c.CloseUpgrades();var s=Session(c.OrderSession.Profile);typeof(ClickCookPrototypeController).GetField("session",Private).SetValue(c,s);c.CookSelectedLocation();playDish=UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish);playLocation=UnityEngine.Object.Instantiate(s.SelectedLocation);SetDish(playDish,1);SetPool(playLocation,playDish);s.StartDay(playLocation,2);c.SelectOrder(0);UnityEngine.Random.InitState(SeedFor(1,DishRarity.Uncommon));c.ClickCookingDish();
            Check(s.LastResult.Payout==6.63m&&s.Rewards.Wallet==36.88m&&s.LastResult.XpGained==20.52m&&s.Rewards.GetExactTotalXp(playDish)==20.52m&&s.CompletedDishes==1&&Field<GameObject>(c,"dishResultScreen").activeSelf,"controller final-rounding result/XP independence");
            var text=Field<UnityEngine.UI.Text>(c,"dishResultText");Canvas.ForceUpdateCanvases();Check(text.text.Contains(ClickCookUiCopy.Money(6.63m))&&text.text.Contains(ClickCookUiCopy.Money(36.88m))&&text.preferredHeight<=text.rectTransform.rect.height+1,"actual payout/wallet fit");c.ClickCookingDish();Check(s.Rewards.Wallet==36.88m&&s.CompletedDishes==1,"stale result blocked");var disk=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot();Check(disk.Wallet==36.88m&&disk.DishXpById[1]==20&&disk.DishXpHundredthsById[1]==52&&disk.DishXpById[2]==17,"rounded money/fraction XP autosave");return "U5c result 6.63 / wallet 36.88 / XP 20.52 PASS.";
        }
        public static string FinishLive()
        {var c=Controller;c.ReturnToProfileMenu();UnityEngine.Object.DestroyImmediate(playDish);UnityEngine.Object.DestroyImmediate(playLocation);c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.DishValue);return "U5c fixture released.";}
        public static string VerifyRestart()
        {Check(PlayerProfileRuntime.Store.SavePath.Contains("U5c-"),"isolated restart");var c=Controller;var s=c.OrderSession;var snapshot=s.Profile.CaptureSnapshot();Check(snapshot.UpgradeRanksById.Count==17&&s.DishValueRank==2&&s.DishValueMultiplier==1.06m&&s.DishXpMultiplier==1.14m&&snapshot.Wallet==36.88m&&snapshot.DishXpById[1]==20&&snapshot.DishXpHundredthsById[1]==52&&snapshot.DishXpById[2]==17&&!s.IsDayActive&&!s.IsHolding&&!s.LastActionWasAuto,"all ranks/exact wallet/XP/fractions/idle");c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.DishValue);return "U5c restart/Continue PASS.";}
        public static string Cleanup(){Check(!EditorApplication.isPlaying,"cleanup stopped");return PlayerProfileStoreVerification.FinishPlayVerification();}
    }
}