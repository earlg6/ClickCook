using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    public static class DishXpMultiplierVerification
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value,string message) { if(!value) throw new Exception("U5b: "+message); }
        private static T Field<T>(object target,string name)=>(T)target.GetType().GetField(name,Private).GetValue(target);
        private static UpgradeCatalog Catalog=>AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id,int rank)=>"{\"id\":\""+id+"\",\"rank\":"+rank+"}";
        private static string Seal(int version,string payload)
        { using var sha=SHA256.Create(); return JsonUtility.ToJson(new Envelope {version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))}); }
        private static string Signed(int version,decimal wallet,int xp,int cents,params string[] ranks)
        {
            string fraction=version>=13 || cents!=0 ? ",\"hundredths\":"+cents : "";
            return Seal(version,"{\"wallet\":\""+wallet.ToString(CultureInfo.InvariantCulture)+"\",\"dishes\":[{\"id\":1,\"xp\":"+xp+fraction+"},{\"id\":2,\"xp\":17}],\"upgrades\":["+string.Join(",",ranks)+"]}");
        }
        private static string[] Ranks(int multiplier=0,int flat=2)=>new[]{
            Rank(UpgradeNodeRules.Strength,3),Rank(UpgradeNodeRules.CriticalChance,1),Rank(UpgradeNodeRules.CriticalMultiplier,1),Rank(UpgradeNodeRules.Hold,1),Rank(UpgradeNodeRules.HoldFrequency,1),
            Rank(UpgradeNodeRules.DoubleStrikeChance,10),Rank(UpgradeNodeRules.DoubleStrikePower,1),Rank(UpgradeNodeRules.DoubleStrikeChanceII,1),Rank(UpgradeNodeRules.TripleStrikeChance,1),
            Rank(UpgradeNodeRules.AutoActionCount,1),Rank(UpgradeNodeRules.AutoActionChance,1),Rank(UpgradeNodeRules.AutoStrength,2),Rank(UpgradeNodeRules.AutoStrengthMultiplier,2),Rank(UpgradeNodeRules.AutoFrequency,2),Rank(UpgradeNodeRules.DishXpFlat,flat),Rank(UpgradeNodeRules.DishXpMultiplier,multiplier)};
        private static PlayerProfile Decode(string json)=>(PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static PrototypeOrderSession Session(PlayerProfile p,bool start=false)
        { var s=new PrototypeOrderSession(p,()=>0,()=>.22,()=>start?0:.99);Check(s.ConfigureUpgrades(Catalog),"configure");return s; }
        private static PrototypeOrderSession Session(int rank,int xp=0,int cents=0,int flat=2,bool start=false)=>Session(Decode(Signed(13,1000000.25m,xp,cents,Ranks(rank,flat))),start);
        private static void SetDish(DishData dish,int hp)
        { var so=new SerializedObject(dish);so.FindProperty("dishId").intValue=1;so.FindProperty("clickRequirement").intValue=hp;so.FindProperty("saleValue").intValue=5;so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetPool(LocationData l,DishData d)
        { var so=new SerializedObject(l);var pool=so.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=d;so.ApplyModifiedPropertiesWithoutUndo(); }
        private static PrototypeOrder Begin(PrototypeOrderSession s,LocationData l) { s.StartDay(l,2);Check(s.TrySelect(0),"select");return s.ActiveOrder; }
        private static int XpAtLevel(int level)=>Enumerable.Range(1,level-1).Sum(PrototypeDishRewards.RequiredXp);
        private static int SeedFor(int level,DishRarity rarity)
        { for(int seed=0;seed<10000;seed++){UnityEngine.Random.InitState(seed);if(PrototypeDishRewards.ResolveRarity(level,UnityEngine.Random.Range(0,100))==rarity)return seed;}throw new Exception("No rarity seed"); }
        private static void CompleteCommon(PrototypeOrderSession s,LocationData l)
        { var order=Begin(s,l);UnityEngine.Random.InitState(SeedFor(s.Rewards.GetLevel(order.Dish),DishRarity.Common));Check(s.TryCookClick(order),"common completion");s.EndDay(); }

        public static string RunModelChecks()
        {
            var random=UnityEngine.Random.state;var dish=ScriptableObject.CreateInstance<DishData>();var location=ScriptableObject.CreateInstance<LocationData>();
            try
            {
                SetDish(dish,1);SetPool(location,dish);
                var locked=Session(0,flat:0);Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.DishXpMultiplier,out _),"flat prerequisite");
                var poor=Session(Decode(Signed(13,99.99m,0,0,Ranks())));Check(poor.TryQuoteUpgrade(UpgradeNodeRules.DishXpMultiplier,out var costly)&&!poor.TryPurchaseUpgrade(costly)&&poor.Rewards.Wallet==99.99m,"insufficient funds");
                decimal[] prices={100,170,289,492,836,1420,2414,4104,6976,11859,20160,34272,58263,99046,168378};var buyer=Session(0);decimal spent=0;
                for(int n=0;n<15;n++)
                {
                    Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.DishXpMultiplier,out var q)&&q.Price==prices[n],"explicit price");buyer.IsPaused=true;Check(!buyer.TryPurchaseUpgrade(q),"pause purchase");buyer.IsPaused=false;buyer.StartDay(location,1);Check(!buyer.TryPurchaseUpgrade(q),"day purchase");buyer.EndDay();
                    Check(!locked.TryPurchaseUpgrade(q)&&buyer.TryPurchaseUpgrade(q)&&!buyer.TryPurchaseUpgrade(q),"owner/replay");spent+=prices[n];Check(buyer.DishXpMultiplierRank==n+1&&buyer.DishXpMultiplier==(100+7*(n+1))/100m&&buyer.DishXpBonus==6,"factor independent of flat");
                }
                Check(spent==408779&&buyer.Rewards.Wallet==1000000.25m-spent&&!buyer.TryQuoteUpgrade(UpgradeNodeRules.DishXpMultiplier,out _),"cap/total");
                foreach(string id in new[]{"node.dish_xp.02","node.dish_xp_flat.04","node.rarity_chance.01"})Check(!buyer.TryQuoteUpgrade(id,out _),"future effect unavailable");
                int[] awards={10,12,16,22,30};decimal[] payouts={5,6.25m,8.75m,12.5m,20};
                for(int rank=0;rank<=15;rank++)for(int tier=0;tier<5;tier++)
                {
                    int level=tier==0?1:7,xp=XpAtLevel(level);var s=Session(rank,xp,37);var order=Begin(s,location);int changes=0;s.Profile.Changed+=()=>changes++;UnityEngine.Random.InitState(SeedFor(level,(DishRarity)tier));decimal before=s.Rewards.Wallet;Check(s.TryCookClick(order),"manual completion");
                    decimal expected=(awards[tier]+6)*(100+7*rank)/100m,total=xp+.37m+expected;
                    Check(s.LastResult.Rarity==(DishRarity)tier&&s.LastResult.PreviousLevel==level&&s.LastResult.XpGained==expected&&s.Rewards.GetExactTotalXp(dish)==total&&s.Rewards.GetTotalXp(dish)==decimal.Truncate(total)&&s.LastResult.Payout==payouts[tier]&&s.Rewards.Wallet==before+payouts[tier]&&changes==1&&s.Profile.CaptureSnapshot().DishXpById[2]==17&&!s.Profile.CaptureSnapshot().DishXpHundredthsById.ContainsKey(2),"all-rarity rational award/flat before factor/exact total/old-level payout/isolation");
                    decimal exact=s.Rewards.GetExactTotalXp(dish),paid=s.Rewards.Wallet;Check(!s.TryCookClick(order)&&!s.TryCookAutoStart(order)&&!s.TryCookAuto(order,100)&&s.Rewards.GetExactTotalXp(dish)==exact&&s.Rewards.Wallet==paid&&changes==1,"stale rewards");s.EndDay();
                }
                foreach(string source in new[]{"hold","start","periodic"})
                {var s=Session(1,start:source=="start");var order=Begin(s,location);UnityEngine.Random.InitState(SeedFor(1,DishRarity.Common));bool done;if(source=="start")done=s.TryCookAutoStart(order);else if(source=="periodic"){s.TryCookAuto(order,0);done=s.TryCookAuto(order,s.AutoInterval);}else{Check(s.TryBeginHold(order,0),"hold begin");done=s.TryCookClick(order);}Check(done&&s.LastResult.XpGained==17.12m&&s.Rewards.GetExactTotalXp(dish)==17.12m&&s.CompletedDishes==1,"fractional source parity");s.EndDay();}
                var repeated=Session(1);CompleteCommon(repeated,location);CompleteCommon(repeated,location);CompleteCommon(repeated,location);Check(repeated.Rewards.GetExactTotalXp(dish)==51.36m&&repeated.Rewards.GetTotalXp(dish)==51&&repeated.Rewards.GetXpHundredths(dish)==36&&repeated.Rewards.GetLevel(dish)==3&&repeated.Rewards.GetExactLevelXp(dish)==7.36m,"repeated fractions/carry/level progress");
                var crossing=Session(15,19,95,5);CompleteCommon(crossing,location);Check(crossing.Rewards.GetExactTotalXp(dish)==71.20m&&crossing.Rewards.GetLevel(dish)==3&&crossing.Rewards.GetExactLevelXp(dish)==27.20m,"multi-level fractional crossing");
                int cap=XpAtLevel(18);foreach(int cents in new[]{0,99}){var s=Session(15,cap-1,cents);var order=Begin(s,location);s.TryCookClick(order);Check(s.Rewards.GetExactTotalXp(dish)==cap&&s.Rewards.GetXpHundredths(dish)==0&&s.LastResult.XpGained==1-cents/100m&&s.LastResult.Level==18,"near cap actual fraction");s.EndDay();}
                var mastered=Session(15,cap);var selected=Begin(mastered,location);mastered.TryCookClick(selected);Check(mastered.LastResult.XpGained==0&&mastered.Rewards.GetExactTotalXp(dish)==cap,"cap zero award");mastered.EndDay();
                SetDish(dish,1000);var partial=Session(1,0,37);selected=Begin(partial,location);partial.TryCookClick(selected);partial.ReturnToBoard();partial.EndDay();Check(partial.Rewards.GetExactTotalXp(dish)==.37m,"Back/end preserve fraction without XP");
                return "U5b model PASS: prices/cap/guards, all-rarity ranks 0–15, flat-before-factor, exact accumulation/carry/levels/cap, source parity/one receipt/isolation.";
            }
            finally{UnityEngine.Random.state=random;UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location);}
        }

        public static string RunStorageChecks()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/U5bStore/"+Guid.NewGuid().ToString("N")));Directory.CreateDirectory(folder);
            for(int version=1;version<=12;version++)
            {
                int length=version>=6?version+3:version==5?7:version+1;string[] ranks=version==1?new[]{Rank("click_strength",3)}:Ranks().Take(length).ToArray();string old=Signed(version,2500.25m,50,0,ranks),path=Path.Combine(folder,"v"+version+".json");File.WriteAllText(path,old);var store=new PlayerProfileStore(path);var p=store.Load();var before=p.CaptureSnapshot();
                Check(p.GetUpgradeRank(UpgradeNodeRules.DishXpMultiplier)==0&&Session(p).DishXpMultiplier==1&&before.DishXpHundredthsById.Count==0&&File.ReadAllText(path)==old,"old identity/no fraction/rewrite");Check(store.Save(p)&&JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version==PlayerProfileStore.CurrentVersion&&File.ReadAllText(store.BackupPath)==old,"schema/backup");var after=new PlayerProfileStore(path).Load().CaptureSnapshot();
                Check(after.Wallet==2500.25m&&after.DishXpById[1]==50&&after.DishXpById[2]==17&&after.DishXpHundredthsById.Count==0&&before.UpgradeRanksById.Count==after.UpgradeRanksById.Count&&before.UpgradeRanksById.All(x=>after.UpgradeRanksById[x.Key]==x.Value),"exact legacy state");File.WriteAllText(path,"broken");var recovery=new PlayerProfileStore(path);Check(recovery.Load()!=null&&recovery.Status==ProfileLoadStatus.Recovered,"old backup recovery");
                Check(Decode(Signed(version,1000,0,0,ranks.Concat(new[]{Rank(UpgradeNodeRules.DishXpMultiplier,0)}).ToArray()))==null&&Decode(Signed(version,1000,0,1,ranks))==null,"old schema rejects new node/fraction");
            }
            foreach(int rank in new[]{1,15})
            {string path=Path.Combine(folder,"r"+rank+".json");var p=Decode(Signed(13,1000.25m,19,95,Ranks(rank)));var snapshot=p.CaptureSnapshot();Check(new PlayerProfileStore(path).Save(p),"fraction write");var s=Session(new PlayerProfileStore(path).Load());Check(s.DishXpMultiplierRank==rank&&s.DishXpBonus==6&&s.Profile.CaptureSnapshot().DishXpById[1]==19&&s.Profile.CaptureSnapshot().DishXpHundredthsById[1]==95&&s.Rewards.Wallet==1000.25m&&!s.IsDayActive&&!s.IsHolding&&!s.LastActionWasAuto,"independent fractional idle roundtrip");
                bool rejected=false;try{((IDictionary<int,int>)snapshot.DishXpHundredthsById)[1]=0;}catch(NotSupportedException){rejected=true;}Check(rejected&&p.CaptureSnapshot().DishXpHundredthsById[1]==95,"snapshot immutable");
                var store=new PlayerProfileStore(path);Check(store.Save(p)&&store.Save(p)&&File.ReadAllText(path)==File.ReadAllText(store.BackupPath),"fraction backup preserved");File.WriteAllText(path,"broken");var recovered=new PlayerProfileStore(path);Check(recovered.Load().CaptureSnapshot().DishXpHundredthsById[1]==95&&recovered.Status==ProfileLoadStatus.Recovered,"fraction backup recovery");
                Check(recovered.Save(new PlayerProfile(),true),"new game reset");Check(new PlayerProfileStore(path).Load().CaptureSnapshot().DishXpHundredthsById.Count==0&&new PlayerProfileStore(store.BackupPath).Load().CaptureSnapshot().DishXpById.Count==0,"reset primary/backup fractions cleared");
            }
            foreach(var ranks in new[]{Ranks(-1),Ranks(16),Ranks(1,0),Ranks(1).Concat(new[]{Rank(UpgradeNodeRules.DishXpMultiplier,1)}).ToArray(),Ranks().Concat(new[]{Rank("node.dish_xp.02",1)}).ToArray(),Ranks().Concat(new[]{Rank("dish_xp",1)}).ToArray()})Check(Decode(Signed(13,1000,0,0,ranks))==null,"invalid node/cap/prereq/alias");
            int cap=XpAtLevel(18);foreach(var input in new[]{(xp:0,cents:-1),(xp:0,cents:100),(xp:cap,cents:1)})Check(Decode(Signed(13,1000,input.xp,input.cents,Ranks(1)))==null,"invalid fractions/cap");
            string future=Signed(PlayerProfileStore.CurrentVersion + 1,1000,0,0,Ranks()),futurePath=Path.Combine(folder,"future.json");File.WriteAllText(futurePath,future);var blocked=new PlayerProfileStore(futurePath);Check(blocked.Load()==null&&blocked.Status==ProfileLoadStatus.Unsupported&&!blocked.Save(Decode(Signed(13,1000,0,0,Ranks())))&&File.ReadAllText(futurePath)==future,"future protected");
            return "U5b storage PASS: schemas 1–12 to 13, exact legacy values, fractional roundtrip/snapshot/backup/reset, invalid fractions/nodes and version 14 protection.";
        }

        private static ClickCookPrototypeController Controller=>UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish;private static LocationData playLocation;
        public static string PreparePlay()
        {Check(!EditorApplication.isPlaying,"prepare stopped");string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/ClickCookSaveTests/U5b-"+Guid.NewGuid().ToString("N"),"profile.json"));Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,Signed(12,300.25m,0,0,Ranks().Take(15).ToArray()));SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;}
        public static string ShowNativePurchase()
        {Check(PlayerProfileRuntime.Store.SavePath.Contains("U5b-"),"isolated profile");var c=Controller;c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.DishXpMultiplier);Field<UpgradeTreeView>(c,"upgradeTreeView").FocusNode(UpgradeNodeRules.DishXpMultiplier);Check(Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text.Contains("0 / 15")&&Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"ready native purchase");return "U5b ready, wallet 300.25.";}
        public static string CheckNativePurchase(int rank)
        {var s=Controller.OrderSession;decimal wallet=rank==1?200.25m:30.25m;var disk=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(s.DishXpMultiplierRank==rank&&s.DishXpMultiplier==(rank==1?1.07m:1.14m)&&s.Rewards.Wallet==wallet&&disk.GetUpgradeRank(UpgradeNodeRules.DishXpMultiplier)==rank&&disk.Rewards.Wallet==wallet&&disk.CaptureSnapshot().DishXpById[1]==0&&disk.CaptureSnapshot().DishXpHundredthsById.Count==0,"native exact debit/factor/no purchase XP/autosave");return "U5b native rank "+rank+" PASS.";}
        public static string CheckUi()
        {
            CheckNativePurchase(2);var c=Controller;c.GetComponent<ProfileMenu>().Resume();string detail=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text;
            Check(detail.Contains("2 / 15")&&detail.Contains(1.14m.ToString("0.##"))&&detail.Contains(1.21m.ToString("0.##"))&&detail.Contains(ClickCookUiCopy.Money(289))&&!Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"detail factor/next/price/funds");
            var stats=Field<UpgradeTreeView>(c,"upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>();Canvas.ForceUpdateCanvases();Check(stats.text.Contains("+6 XP")&&stats.text.Contains("×"+1.14m.ToString("0.##"))&&stats.text.Split('\n').Length==9&&stats.preferredHeight<=stats.rectTransform.rect.height+1&&stats.preferredWidth<=stats.rectTransform.rect.width+1,"seven statistics fit");return "U5b UI PASS.";
        }
        public static string CompleteLive()
        {
            var c=Controller;c.GetComponent<ProfileMenu>().Resume();c.CloseUpgrades();var s=Session(c.OrderSession.Profile);typeof(ClickCookPrototypeController).GetField("session",Private).SetValue(c,s);c.CookSelectedLocation();playDish=UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish);playLocation=UnityEngine.Object.Instantiate(s.SelectedLocation);SetDish(playDish,1);SetPool(playLocation,playDish);s.StartDay(playLocation,2);c.SelectOrder(0);decimal beforeXp=s.Rewards.GetExactTotalXp(playDish),beforeWallet=s.Rewards.Wallet;
            UnityEngine.Random.InitState(SeedFor(s.Rewards.GetLevel(playDish),DishRarity.Common));c.ClickCookingDish();decimal expected=beforeXp+18.24m;
            Check(s.LastResult.XpGained==18.24m&&s.Rewards.GetExactTotalXp(playDish)==expected&&s.Rewards.Wallet==beforeWallet+5&&s.CompletedDishes==1&&Field<GameObject>(c,"dishResultScreen").activeSelf,"fractional controller result");
            var text=Field<UnityEngine.UI.Text>(c,"dishResultText");var mastery=Field<UnityEngine.UI.Text>(c,"resultMasteryText");Canvas.ForceUpdateCanvases();Check(text.text.Contains(18.24m.ToString("0.##")+" XP")&&mastery.text.Contains(s.Rewards.GetExactLevelXp(playDish).ToString("0.##"))&&text.preferredHeight<=text.rectTransform.rect.height+1,"exact result/mastery text");
            var fill=Field<UnityEngine.UI.Image>(c,"resultMasteryFill");Check(Math.Abs(fill.fillAmount-(float)(s.Rewards.GetExactLevelXp(playDish)/PrototypeDishRewards.RequiredXp(s.Rewards.GetLevel(playDish))))<1e-5,"fractional mastery bar");
            c.ClickCookingDish();Check(s.Rewards.GetExactTotalXp(playDish)==expected&&s.Rewards.Wallet==beforeWallet+5,"stale receipt blocked");var disk=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot();Check(disk.DishXpById[1]==decimal.Truncate(expected)&&disk.DishXpHundredthsById[1]==(expected-decimal.Truncate(expected))*100&&disk.DishXpById[2]==17,"fraction autosave/isolation");return "U5b exact result/autosave PASS: total XP "+expected;
        }
        public static string FinishLive()
        {var c=Controller;c.ReturnToProfileMenu();UnityEngine.Object.DestroyImmediate(playDish);UnityEngine.Object.DestroyImmediate(playLocation);c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.DishXpMultiplier);return "U5b fixture released.";}
        public static string VerifyRestart()
        {Check(PlayerProfileRuntime.Store.SavePath.Contains("U5b-"),"isolated restart");var c=Controller;var s=c.OrderSession;var snapshot=s.Profile.CaptureSnapshot();Check(snapshot.UpgradeRanksById.Count==16&&s.DishXpMultiplierRank==2&&s.DishXpMultiplier==1.14m&&s.DishXpBonus==6&&snapshot.DishXpById[1]==18&&snapshot.DishXpHundredthsById[1]==24&&snapshot.DishXpById[2]==17&&s.Rewards.Wallet==35.25m&&!s.IsDayActive&&!s.IsHolding&&!s.LastActionWasAuto,"exact whole/fraction/wallet/ranks/idle restored");c.GetComponent<ProfileMenu>().Continue();c.GetComponent<ProfileMenu>().Resume();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.DishXpMultiplier);return "U5b restart/Continue PASS.";}
        public static string Cleanup(){Check(!EditorApplication.isPlaying,"cleanup stopped");return PlayerProfileStoreVerification.FinishPlayVerification();}
    }
}