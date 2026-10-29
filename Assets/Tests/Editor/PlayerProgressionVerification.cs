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
using ClickCook.Runtime.Locations;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    public static class PlayerProgressionVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,Private).GetValue(o);
        private static void Check(bool condition,string message){if(!condition)throw new Exception("8C: "+message);}
        private static LocationCatalog Locations=>AssetDatabase.LoadAssetAtPath<LocationCatalog>("Assets/Data/LocationCatalog.asset");
        private static void SetXp(PlayerProfile p,decimal xp)=>typeof(PlayerProfile).GetMethod("RestorePlayerXp",Private).Invoke(p,new object[]{xp});
        private static string Signed(int version,string payload)
        {using var sha=SHA256.Create();return JsonUtility.ToJson(new Envelope{version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))});}
        private static string Payload(string xp=null,int version=18)=>"{\"wallet\":\"123.25\",\"dishes\":[{\"id\":1,\"xp\":17"+(version>=13?",\"hundredths\":37":"")+"}],\"upgrades\":[]"+(version>=17?",\"recipes\":[1,2]":"")+(xp==null?"":",\"playerXp\":\""+xp+"\"")+"}";
        private static PlayerProfile Decode(string json)=>(PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static DishData dish;
        private static LocationData location;
        private static void Fixture(int sale)
        {
            dish=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset").Find(1));
            var ds=new SerializedObject(dish);ds.FindProperty("clickRequirement").intValue=1;ds.FindProperty("saleValue").intValue=sale;ds.ApplyModifiedPropertiesWithoutUndo();
            location=UnityEngine.Object.Instantiate(Locations.Locations[0]);var ls=new SerializedObject(location);var pool=ls.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;ls.ApplyModifiedPropertiesWithoutUndo();
            dish.hideFlags=location.hideFlags=HideFlags.DontSave;
        }
        private static int CommonSeed(int level)
        {for(int seed=0;seed<10000;seed++){UnityEngine.Random.InitState(seed);if(PrototypeDishRewards.ResolveRarity(level,UnityEngine.Random.Range(0,100))==DishRarity.Common)return seed;}throw new Exception("Common seed");}
        private static void Complete(PrototypeOrderSession s)
        {Check(s.TrySelect(0),"select");UnityEngine.Random.InitState(CommonSeed(s.Rewards.GetLevel(dish)));Check(s.TryCookClick(s.ActiveOrder),"fixture completion");}
        public static string RunModelChecks()
        {
            var random=UnityEngine.Random.state;Fixture(1);
            try
            {
                decimal[] expected={0,100,300,700,1400,2500,4200,6600,10000,15000};
                for(int level=1;level<=10;level++)
                {
                    Check(PlayerProgression.ThresholdForLevel(level)==expected[level-1]&&PlayerProgression.LevelAt(expected[level-1])==level,"threshold table "+level);
                    if(level>1)Check(PlayerProgression.LevelAt(expected[level-1]-.0001m)==level-1,"just below "+level);
                    if(level<10)Check(PlayerProgression.LevelAt(expected[level-1]+.0001m)==level,"just above "+level);
                    var p=new PlayerProfile();SetXp(p,expected[level-1]);
                    for(int index=0;index<11;index++)Check(p.CanWorkAt(Locations.Locations[index])==(level>=Math.Min(index+1,10)),"explicit level/location mapping");
                }
                var fresh=new PrototypeOrderSession(new PlayerProfile());fresh.StartDay(Locations.Locations[1],2);Check(!fresh.IsDayActive,"model rejects locked day");
                fresh.StartDay(location,1);fresh.StartDay(Locations.Locations[10],6);Check(fresh.IsDayActive&&fresh.SelectedLocation==location,"locked start does not discard valid day");fresh.EndDay();
                var profile=Decode(Signed(18,Payload("99.9500")));var s=new PrototypeOrderSession(profile);int notifications=0;profile.Changed+=()=>notifications++;
                s.StartDay(location,2);
                for(int i=1;i<=10;i++){Complete(s);Check(profile.PlayerXp==(i<10?99.95m:100.05m),"XP waits for tenth and carries exact fraction");}
                Check(profile.PlayerLevel==2&&s.DayPlayerXpGained==.10m&&profile.Rewards.Wallet==133.25m&&notifications==10,"one cash/XP transaction");
                Check(s.TryFinishDay(out var report)&&report.PlayerXp==.1m&&report.Earnings==10m,"day XP receipt");
                Check(!s.TryFinishDay(out _)&&profile.PlayerXp==100.05m,"summary replay no XP");
                var snapshot=profile.CaptureSnapshot();s.StartDay(location,1);Complete(s);s.TryFinishDay(out var early);
                Check(early.PlayerXp==0&&profile.PlayerXp==100.05m&&snapshot.PlayerXp==100.05m,"early no Player XP/snapshot detached");
                s.StartDay(location,1);Complete(s);s.EndDay();Check(profile.PlayerXp==100.05m,"abandoned no Player XP");
                var capped=Decode(Signed(18,Payload("14999.9999")));s=new PrototypeOrderSession(capped);s.StartDay(location,2);for(int i=0;i<10;i++)Complete(s);
                Check(capped.PlayerXp==15000m&&capped.PlayerLevel==10&&s.DayPlayerXpGained==.0001m,"exact cap actual award");s.EndDay();
                capped=Decode(Signed(18,Payload("15000")));s=new PrototypeOrderSession(capped);s.StartDay(location,2);for(int i=0;i<10;i++)Complete(s);
                Check(capped.PlayerXp==15000m&&s.DayPlayerXpGained==0&&capped.Rewards.Wallet==133.25m,"cap retains cash payout");s.EndDay();
                var ds=new SerializedObject(dish);ds.FindProperty("saleValue").intValue=10000;ds.ApplyModifiedPropertiesWithoutUndo();
                var jump=new PrototypeOrderSession(new PlayerProfile());jump.StartDay(location,2);for(int i=0;i<10;i++)Complete(jump);
                Check(jump.Profile.PlayerXp==1000&&jump.Profile.PlayerLevel==4,"multi-level crossing without subtracting cumulative XP");jump.EndDay();
                return "8C model PASS: exact threshold boundaries, ten levels/eleven locations, locked-start guard, one tenth-receipt cash/XP award, early/abandon/replay, fraction/cap/cash-at-cap and multi-level crossing.";
            }
            finally{UnityEngine.Random.state=random;UnityEngine.Object.DestroyImmediate(dish);UnityEngine.Object.DestroyImmediate(location);}
        }
        public static string RunStorageChecks()
        {
            string root=Path.GetFullPath("Temp/ClickCookSaveTests/PlayerXp-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
            for(int version=1;version<=17;version++)
            {
                string path=Path.Combine(root,version+".json"),source=Signed(version,Payload(null,version));File.WriteAllText(path,source);var p=new PlayerProfileStore(path).Load();
                Check(p!=null&&p.PlayerXp==0&&p.PlayerLevel==1&&p.Rewards.Wallet==123.25m&&p.Rewards.GetExactTotalXp(AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset").Find(1))==(version<13?17m:17.37m)&&File.ReadAllText(path)==source,"legacy exact/no invented XP/no eager rewrite "+version);
                Check(p.OwnsRecipe(2)==(version==17),"legacy recipe retention "+version);
            }
            foreach(decimal xp in new[]{0m,.0001m,99.9999m,100m,14999.9999m,15000m})
            {
                var p=Decode(Signed(18,Payload(xp.ToString(CultureInfo.InvariantCulture))));var store=new PlayerProfileStore(Path.Combine(root,"xp"+xp+".json"));
                Check(p!=null&&store.Save(p),"save XP");var loaded=store.Load();Check(loaded.PlayerXp==xp&&loaded.PlayerLevel==PlayerProgression.LevelAt(xp)&&loaded.OwnsRecipe(2)&&loaded.Rewards.Wallet==123.25m&&!loaded.HasActiveDay,"exact current XP/level/recipe/wallet roundtrip");
                File.WriteAllText(store.SavePath,"broken");var recovery=new PlayerProfileStore(store.SavePath);Check(recovery.Load().PlayerXp==xp&&recovery.Status==ProfileLoadStatus.Recovered,"XP backup recovery");
                Check(store.Save(new PlayerProfile(),true)&&store.Load().PlayerXp==0&&new PlayerProfileStore(store.BackupPath).Load().PlayerXp==0,"reset both XP copies");
            }
            foreach(var bad in new[]{null,"-1","15000.0001","0.00001","NaN","Infinity","1e2","01","+1"})Check(Decode(Signed(18,Payload(bad)))==null,"invalid XP "+bad);
            Check(Decode(Signed(17,Payload("0",17)))==null,"old schema XP injection rejected");
            string futurePath=Path.Combine(root,"future.json"),future=Signed(19,Payload("0"));File.WriteAllText(futurePath,future);var protectedStore=new PlayerProfileStore(futurePath);
            Check(protectedStore.Load()==null&&protectedStore.Status==ProfileLoadStatus.Unsupported&&!protectedStore.Save(new PlayerProfile())&&File.ReadAllText(futurePath)==future,"future19 protected");
            return "8C storage PASS: schemas1–17 exact migration to zero XP, fractional/cap boundaries, ownership/wallet retention, schema18 backup/reset, malformed/old-injection/future19 protection.";
        }
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying,"prepare stopped");string path=Path.GetFullPath("Temp/ClickCookSaveTests/PlayerXpPlay-"+Guid.NewGuid().ToString("N")+"/profile.json");
            Check(new PlayerProfileStore(path).Save(Decode(Signed(18,Payload("99.9500")))),"isolated save");SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;
        }
        public static string VerifyPlay()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();
            var s=c.OrderSession;var catalog=AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset");
            Check(s.Profile.OwnsRecipe(2)&&s.Profile.PlayerLevel==1,"preexisting owned recipe does not bypass level");
            c.ShowNextLocation();Check(!Field<UnityEngine.UI.Button>(c,"cookButton").interactable,"owned Cafe level lock");c.CookSelectedLocation();Check(!s.IsDayActive,"controller locked-start guard");
            c.ShowPreviousLocation();c.CookSelectedLocation();Fixture(1);s.StartDay(location,2);
            for(int i=1;i<=10;i++){c.SelectOrder(0);UnityEngine.Random.InitState(CommonSeed(s.Rewards.GetLevel(dish)));c.ClickCookingDish();Check(s.Profile.PlayerXp==(i<10?99.95m:100.05m),"controller exact XP settlement");c.ContinueFromDishResult();}
            Check(c.DaySummary.PlayerXp==.1m&&s.Profile.PlayerLevel==2&&c.DaySummary.Earnings==10,"summary Player XP");
            var summary=Field<UnityEngine.UI.Text>(c,"daySummaryText");Canvas.ForceUpdateCanvases();Check(summary.preferredHeight<=summary.rectTransform.rect.height+1&&summary.text.Contains("Досвід кухаря"),"summary XP text/fit");
            c.NextDay();c.ShowNextLocation();Check(Field<UnityEngine.UI.Button>(c,"cookButton").interactable,"Cafe unlocked with owned recipes");
            var progress=Field<UnityEngine.UI.Text>(c,"progressText");Canvas.ForceUpdateCanvases();Check(progress.preferredHeight<=progress.rectTransform.rect.height+1,"profile XP header fits");
            var saved=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(saved.PlayerXp==100.05m&&saved.Rewards.Wallet==133.25m&&saved.OwnsRecipe(2),"same-transaction XP/cash autosave");
            SessionState.SetString("ClickCook.PlayerXpExpected","100.05");
            return "8C Play PASS: owned-but-locked Cafe, exact tenth XP/cash settlement, summary/profile text fit, level2 unlock and exact autosave. Cafe left visible.";
        }
        public static string VerifyRestart()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();var s=c.OrderSession;
            Check(s.Profile.PlayerXp==100.05m&&s.Profile.PlayerLevel==2&&s.Rewards.Wallet==133.25m&&!s.IsDayActive&&s.DayPlayerXpGained==0,"restart exact idle XP/cash");c.ShowNextLocation();c.CookSelectedLocation();
            Check(s.IsDayActive&&s.SelectedLocation.LevelRequirement==1&&s.Orders.All(o=>s.Profile.OwnsRecipe(o.Dish.DishId)),"restored Cafe access and recipe filter");c.FinishDay();Check(c.DaySummary.PlayerXp==0&&s.Profile.PlayerXp==100.05m,"empty/early day no XP");c.NextDay();
            return "8C restart/Continue PASS: exact XP/level/wallet, real Cafe start, recipe filter and zero extra early-day XP.";
        }
    }
}
