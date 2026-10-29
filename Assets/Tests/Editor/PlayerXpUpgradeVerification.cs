using System;
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
    public static class PlayerXpUpgradeVerification
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        private static readonly string[] Ids={UpgradeNodeRules.PlayerXpFlat,UpgradeNodeRules.PlayerXpFlatII,UpgradeNodeRules.PlayerXpMultiplier,UpgradeNodeRules.PlayerXpMultiplierII};
        [Serializable] private sealed class Envelope{public int version;public string payload;public string checksum;}
        private static void Check(bool ok,string message){if(!ok)throw new Exception("Player XP upgrades: "+message);}
        private static T Field<T>(object o,string key)=>(T)o.GetType().GetField(key,Private).GetValue(o);
        private static string Signed(int version,string payload)
        {using var sha=SHA256.Create();return JsonUtility.ToJson(new Envelope{version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))});}
        private static string Payload(int version=20,int flat=0,int flat2=0,int mult=0,int mult2=0,string xp="0")
        {
            string ranks="{\"id\":\""+(version==1?UpgradeCatalog.ClickStrengthId:UpgradeNodeRules.Strength)+"\",\"rank\":1}";
            if(version>=12)ranks+=",{\"id\":\""+UpgradeNodeRules.DishXpFlat+"\",\"rank\":1}";
            if(version>=20){int[] values={flat,flat2,mult,mult2};for(int i=0;i<4;i++)ranks+=",{\"id\":\""+Ids[i]+"\",\"rank\":"+values[i]+"}";}
            return "{\"wallet\":\"10000000.25\",\"dishes\":[{\"id\":1,\"xp\":17"+(version>=13?",\"hundredths\":37":"")+"}],\"upgrades\":["+ranks+"]"+(version>=17?",\"recipes\":[1]":"")+(version>=18?",\"playerXp\":\""+xp+"\"":"")+"}";
        }
        private static PlayerProfile Decode(string json)=>(PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static PrototypeOrderSession Session(int f=0,int f2=0,int m=0,int m2=0,string xp="0")
        {var s=new PrototypeOrderSession(Decode(Signed(20,Payload(20,f,f2,m,m2,xp))));Check(s.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset")),"catalog");return s;}
        public static string AuthorCatalog()
        {
            Check(!EditorApplication.isPlaying,"author stopped");var catalog=AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");Undo.RecordObject(catalog,"Player XP upgrade definitions");var so=new SerializedObject(catalog);var list=so.FindProperty("upgrades");int added=0;
            foreach(string id in Ids)
            {
                if(catalog.FindForNode(id)!=null)continue;Check(UpgradeNodeRules.TryGetAdditionalTier(id,out var rule),"rule");int at=list.arraySize;list.InsertArrayElementAtIndex(at);var item=list.GetArrayElementAtIndex(at);
                item.FindPropertyRelative("id").stringValue=UpgradeNodeRules.CatalogId(id);item.FindPropertyRelative("displayName").stringValue=UpgradeTreeDefinition.Load().Find(id).title;
                item.FindPropertyRelative("maximumRank").intValue=rule.Cap;item.FindPropertyRelative("basePrice").intValue=rule.BasePrice;item.FindPropertyRelative("priceGrowthPercent").intValue=rule.Growth;added++;
            }
            so.ApplyModifiedProperties();EditorUtility.SetDirty(catalog);AssetDatabase.SaveAssets();return "Player XP definitions authored: "+added+"; existing multiplier catalog retained.";
        }
        private static decimal Credit(PlayerProfile p,decimal money)=>(decimal)typeof(PlayerProfile).GetMethod("CreditCompletedDay",Private).Invoke(p,new object[]{money});
        public static string RunModelChecks()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
            foreach(string id in Ids){var d=catalog.FindForNode(id);UpgradeNodeRules.TryGetAdditionalTier(id,out var r);Check(d!=null&&d.MaximumRank==r.Cap&&d.PriceForNextRank(0)==r.BasePrice,"catalog cap/base "+id);}
            Check(!Session().TryQuoteUpgrade(UpgradeNodeRules.PlayerXpMultiplier,out _)&&!Session(4).TryQuoteUpgrade(UpgradeNodeRules.PlayerXpFlatII,out _)&&!Session(1,0,14).TryQuoteUpgrade(UpgradeNodeRules.PlayerXpMultiplierII,out _),"purchased parents required");
            Check(Session(5).TryQuoteUpgrade(UpgradeNodeRules.PlayerXpFlatII,out _)&&Session(1).TryQuoteUpgrade(UpgradeNodeRules.PlayerXpMultiplier,out _)&&Session(1,0,15).TryQuoteUpgrade(UpgradeNodeRules.PlayerXpMultiplierII,out _),"graph boundary unlocks");
            var buyer=Session();decimal[] prices={150,255,434,737,1253};decimal spent=0;
            foreach(decimal price in prices){Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.PlayerXpFlat,out var q)&&q.Price==price,"explicit flat prices");buyer.IsPaused=true;Check(!buyer.TryPurchaseUpgrade(q),"paused buy");buyer.IsPaused=false;Check(buyer.TryPurchaseUpgrade(q)&&!buyer.TryPurchaseUpgrade(q),"purchase/replay");spent+=price;}
            Check(spent==2829&&buyer.Profile.PlayerXpFlatBonus==5&&!buyer.TryQuoteUpgrade(UpgradeNodeRules.PlayerXpFlat,out _),"flat cap/total");
            Check(Session(5,5,15,5).Profile.PlayerXpFlatBonus==30&&Session(5,5,15,5).Profile.PlayerXpMultiplier==2.7675m,"independent aggregate maxima");
            foreach(int f in Enumerable.Range(0,6))foreach(int f2 in new[]{0,5})foreach(int m in Enumerable.Range(0,16))foreach(int m2 in new[]{0,5})
            {
                if((f2>0&&f!=5)||(m>0&&f==0)||(m2>0&&m!=15))continue;
                foreach(long cents in new long[]{0,1,125,1025,50025})
                {
                    var p=Session(f,f2,m,m2).Profile;decimal expected=((cents+(f+5*f2)*10000L)*(100+7*m)*(100+7*m2))/100000000m;
                    decimal before=p.Rewards.Wallet;Check(Credit(p,cents/100m)==expected&&p.PlayerXp==expected&&p.Rewards.Wallet==before+cents/100m,"independent rational flat-before-multipliers oracle");
                }
            }
            var precise=Session(5,5,15,5).Profile;Check(Credit(precise,500.25m)==96.86941875m,"explicit eight-place XP");
            var capped=Session(5,5,15,5,"14999.99999999").Profile;Check(Credit(capped,500.25m)==.00000001m&&capped.PlayerXp==15000m,"last XP unit clamp");decimal wallet=capped.Rewards.Wallet;Check(Credit(capped,1m)==0&&capped.Rewards.Wallet==wallet+1,"cash at XP cap");
            return "Player XP upgrade model PASS: catalog/prerequisite/cap/prices/replay, exact flat-before-two-factors rational oracle, maxima+30/×2.7675, eight-place XP and cap unit/cash independence.";
        }
        public static string RunStorageChecks()
        {
            string root=Path.GetFullPath("Temp/ClickCookSaveTests/PlayerXpUpgrades-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
            for(int version=1;version<=19;version++)
            {
                string source=Signed(version,Payload(version,xp:"100.0001")),path=Path.Combine(root,version+".json");File.WriteAllText(path,source);var p=new PlayerProfileStore(path).Load();
                Check(p!=null&&p.PlayerXp==(version>=18?100.0001m:0)&&p.Rewards.Wallet==10000000.25m&&Ids.All(id=>p.GetUpgradeRank(id)==0)&&File.ReadAllText(path)==source,"legacy exact/no free nodes/no rewrite "+version);
                foreach(string id in Ids){string injected=Payload(version,xp:"100.0001").Replace("\"upgrades\":[","\"upgrades\":[{\"id\":\""+id+"\",\"rank\":0},");Check(Decode(Signed(version,injected))==null,"old schema rejects new node at0");}
            }
            foreach(string xp in new[]{"0","100.00000001","96.86941875","14999.99999999","15000"})
            {
                var p=Decode(Signed(20,Payload(20,5,5,15,5,xp)));var store=new PlayerProfileStore(Path.Combine(root,"xp"+xp+".json"));Check(p!=null&&store.Save(p),"save precise XP");var loaded=store.Load();
                Check(loaded.PlayerXp==decimal.Parse(xp,CultureInfo.InvariantCulture)&&loaded.PlayerXpFlatBonus==30&&loaded.PlayerXpMultiplier==2.7675m&&loaded.Rewards.Wallet==10000000.25m,"exact ranks/eight-place roundtrip");
                File.WriteAllText(store.SavePath,"broken");Check(new PlayerProfileStore(store.SavePath).Load().PlayerXp==loaded.PlayerXp,"eight-place backup");
                Check(store.Save(new PlayerProfile(),true)&&store.Load().PlayerXpFlatBonus==0&&store.Load().PlayerXp==0&&new PlayerProfileStore(store.BackupPath).Load().PlayerXpMultiplier==1,"both-copy reset");
            }
            Check(Decode(Signed(19,Payload(19,xp:"100.00000001")))==null&&Decode(Signed(18,Payload(18,xp:"100.00001")))==null,"old precision remains four");
            foreach(string xp in new[]{"0.000000001","15000.00000001","-1"})Check(Decode(Signed(20,Payload(xp:xp)))==null,"invalid XP precision/range");
            Check(Decode(Signed(20,Payload(flat:6)))==null&&Decode(Signed(20,Payload(flat:1,flat2:1)))==null&&Decode(Signed(20,Payload(flat:1,mult:16)))==null&&Decode(Signed(20,Payload(flat:1,mult:1,mult2:1)))==null,"caps/purchased prerequisites");
            string futurePath=Path.Combine(root,"future.json"),future=Signed(21,Payload());File.WriteAllText(futurePath,future);var futureStore=new PlayerProfileStore(futurePath);Check(futureStore.Load()==null&&futureStore.Status==ProfileLoadStatus.Unsupported&&!futureStore.Save(new PlayerProfile()),"future21 protection");
            return "Player XP upgrade storage PASS: schemas1–19 exact migration/no node grants, schema20 four nodes and eight-place XP, old precision gate, backup/reset/caps/prerequisites/future21.";
        }
        private static DishData dish;private static LocationData location;
        private static int Seed(int level,DishRarity rarity){for(int seed=0;seed<10000;seed++){UnityEngine.Random.InitState(seed);if(PrototypeDishRewards.ResolveRarity(level,UnityEngine.Random.Range(0,100))==rarity)return seed;}throw new Exception("rarity seed");}
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying,"prepare stopped");string path=Path.GetFullPath("Temp/ClickCookSaveTests/PlayerXpUpgradePlay-"+Guid.NewGuid().ToString("N")+"/profile.json");
            Check(new PlayerProfileStore(path).Save(Decode(Signed(20,Payload(xp:"99")))),"isolated profile");SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;
        }
        public static string VerifyPlay()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.PlayerXpFlat);
            Field<UpgradeTreeView>(c,"upgradeTreeView").RevealSelection();var stats=Field<UnityEngine.UI.Text>(Field<UpgradeTreeView>(c,"upgradeTreeView"),"statistics");Canvas.ForceUpdateCanvases();Check(stats.preferredHeight<=stats.rectTransform.rect.height+1,"eleven-stat fit");
            return "Player XP node UI PASS: flat0/5,150 price and11-stat panel. Ready for native flat purchase.";
        }
        public static string VerifyNativeFlat()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();c.GetComponent<ProfileMenu>().Resume();Check(c.OrderSession.Profile.PlayerXpFlatBonus==1&&c.OrderSession.Rewards.Wallet==9999850.25m,"native flat purchase");c.SelectUpgradeNode(UpgradeNodeRules.PlayerXpMultiplier);Field<UpgradeTreeView>(c,"upgradeTreeView").RevealSelection();return "Native flat150 PASS; multiplier200 selected.";
        }
        public static string VerifyNativeMultiplierAndDay()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();c.GetComponent<ProfileMenu>().Resume();var s=c.OrderSession;Check(s.Profile.PlayerXpMultiplier==1.07m&&s.Rewards.Wallet==9999650.25m,"native multiplier purchase");c.CloseUpgrades();c.CookSelectedLocation();
            dish=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset").Find(1));var ds=new SerializedObject(dish);ds.FindProperty("clickRequirement").intValue=1;ds.FindProperty("saleValue").intValue=1;ds.ApplyModifiedPropertiesWithoutUndo();
            location=UnityEngine.Object.Instantiate(s.SelectedLocation);var ls=new SerializedObject(location);var pool=ls.FindProperty("documentedAvailableDishes");pool.arraySize=1;pool.GetArrayElementAtIndex(0).objectReferenceValue=dish;ls.ApplyModifiedPropertiesWithoutUndo();dish.hideFlags=location.hideFlags=HideFlags.DontSave;s.StartDay(location,2);
            c.SelectOrder(0);UnityEngine.Random.InitState(Seed(s.Rewards.GetLevel(dish),DishRarity.Common));c.ClickCookingDish();c.ContinueFromDishResult();c.FinishDay();Check(c.DaySummary.PlayerXp==0&&s.Profile.PlayerXp==99,"early upgraded day still zero XP");c.NextDay();c.CookSelectedLocation();s.StartDay(location,2);
            for(int i=0;i<10;i++){c.SelectOrder(0);UnityEngine.Random.InitState(Seed(s.Rewards.GetLevel(dish),i==0?DishRarity.Uncommon:DishRarity.Common));c.ClickCookingDish();c.ContinueFromDishResult();}
            Check(c.DaySummary.Earnings==10.25m&&c.DaySummary.PlayerXp==1.179675m&&s.Profile.PlayerXp==100.179675m&&s.Rewards.Wallet==9999660.50m,"live exact six-place award/cash");
            var saved=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(saved.PlayerXp==100.179675m&&saved.PlayerXpFlatBonus==1&&saved.PlayerXpMultiplier==1.07m&&saved.Rewards.Wallet==9999660.50m,"rank/XP/cash autosave");
            var text=Field<UnityEngine.UI.Text>(c,"daySummaryText");Canvas.ForceUpdateCanvases();Check(text.preferredHeight<=text.rectTransform.rect.height+1&&text.text.Contains(1.179675m.ToString("0.########",CultureInfo.CurrentCulture)),"precise summary fit");return "Native flat/multiplier and controller day PASS: early0 XP; full10.25 cash,1.179675 XP, total100.179675, exact saved ranks/balance.";
        }
        public static string VerifyRestart()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();var p=c.OrderSession.Profile;Check(p.PlayerXp==100.179675m&&p.PlayerXpFlatBonus==1&&p.PlayerXpMultiplier==1.07m&&p.Rewards.Wallet==9999660.50m&&!c.OrderSession.IsDayActive,"restart exact state");return "Player XP modifiers restart/Continue PASS: exact fractional XP, flat/factor/ranks/wallet and idle day.";
        }
    }
}
