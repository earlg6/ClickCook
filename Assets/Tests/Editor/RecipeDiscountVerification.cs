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

namespace ClickCook.Verification
{
    public static class RecipeDiscountVerification
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope{public int version;public string payload;public string checksum;}
        private static void Check(bool ok,string message){if(!ok)throw new Exception("Recipe discount: "+message);}
        private static T Field<T>(object o,string key)=>(T)o.GetType().GetField(key,Private).GetValue(o);
        private static RecipeCatalog Recipes=>AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset");
        private static string Signed(int version,string payload)
        {using var sha=SHA256.Create();return JsonUtility.ToJson(new Envelope{version=version,payload=payload,checksum=Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload)))});}
        private static string Payload(int version,int? rank=null,int upgradeDiscount=0,bool value=true)
        {
            string ranks="{\"id\":\""+(version==1?UpgradeCatalog.ClickStrengthId:UpgradeNodeRules.Strength)+"\",\"rank\":1}";
            if(version>=14&&value)ranks+=",{\"id\":\""+UpgradeNodeRules.DishValue+"\",\"rank\":1}";
            if(upgradeDiscount>0)ranks+=",{\"id\":\""+UpgradeNodeRules.UpgradeDiscount+"\",\"rank\":"+upgradeDiscount+"}";
            if(rank.HasValue)ranks+=",{\"id\":\""+UpgradeNodeRules.RecipeDiscount+"\",\"rank\":"+rank+"}";
            return "{\"wallet\":\"10000.25\",\"dishes\":[{\"id\":1,\"xp\":17"+(version>=13?",\"hundredths\":37":"")+"}],\"upgrades\":["+ranks+"]"+(version>=17?",\"recipes\":[1]":"")+(version>=18?",\"playerXp\":\"100.0001\"":"")+"}";
        }
        private static PlayerProfile Decode(string json)=>(PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{json,ProfileLoadStatus.Invalid});
        private static PrototypeOrderSession Session(int rank=0,int upgrade=0,bool value=true)
        {var s=new PrototypeOrderSession(Decode(Signed(19,Payload(19,rank,upgrade,value))));s.ConfigureRecipes(Recipes);Check(s.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset")),"catalog");return s;}
        public static string RunModelChecks()
        {
            var locked=new PrototypeOrderSession(Decode(Signed(19,Payload(19,null,0,false))));locked.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset"));
            Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.RecipeDiscount,out _),"purchased Dish Value prerequisite");
            foreach(int rank in Enumerable.Range(0,6))
            {
                var s=Session(rank);
                foreach(var dish in Recipes.Dishes)
                {
                    decimal expected=dish.RecipeCost==0?0:Math.Max(1,(dish.RecipeCost*(100-rank)+99)/100);
                    Check(s.RecipePrice(dish)==expected,"independent integer ceil oracle "+dish.DishId+"/"+rank);
                }
                Check(s.UpgradePriceForRank(UpgradeNodeRules.Strength,0)==20,"recipe discount does not discount upgrades");
            }
            var buyer=Session();decimal[] prices={150,300,600,1200,2400};decimal spent=0;
            for(int rank=0;rank<5;rank++)
            {
                buyer.TryQuoteRecipe(2,out var stale);Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.RecipeDiscount,out var quote)&&quote.Price==prices[rank],"authored node prices");
                buyer.IsPaused=true;Check(!buyer.TryPurchaseUpgrade(quote),"pause");buyer.IsPaused=false;
                Check(buyer.TryPurchaseUpgrade(quote)&&!buyer.TryPurchaseUpgrade(quote),"rank purchase/replay");spent+=prices[rank];
                Check(!buyer.TryPurchaseRecipe(stale)&&buyer.RecipeDiscountRank==rank+1&&buyer.Rewards.Wallet==10000.25m-spent,"old recipe quote invalidated without debit");
            }
            Check(spent==4650&&!buyer.TryQuoteUpgrade(UpgradeNodeRules.RecipeDiscount,out _)&&buyer.RecipeDiscount==.05m,"cap and total cost");
            Check(buyer.TryQuoteRecipe(2,out var recipe)&&recipe.BasePrice==150&&recipe.Price==143&&buyer.TryPurchaseRecipe(recipe)&&!buyer.TryPurchaseRecipe(recipe)&&buyer.Rewards.Wallet==5207.25m,"effective recipe debit once");
            var discounted=Session(0,5);Check(discounted.TryQuoteUpgrade(UpgradeNodeRules.RecipeDiscount,out var cheap)&&cheap.Price==143&&discounted.RecipePrice(Recipes.Find(2))==150,"upgrade and recipe discounts independent");
            var rc=UnityEngine.Object.Instantiate(Recipes);var d=UnityEngine.Object.Instantiate(Recipes.Find(2));
            try
            {
                var ds=new SerializedObject(d);ds.FindProperty("recipeCost").intValue=1;ds.ApplyModifiedPropertiesWithoutUndo();var cs=new SerializedObject(rc);cs.FindProperty("dishes").GetArrayElementAtIndex(1).objectReferenceValue=d;cs.ApplyModifiedPropertiesWithoutUndo();
                var s=Session();s.ConfigureRecipes(rc);s.TryQuoteRecipe(2,out var unchanged);s.TryQuoteUpgrade(UpgradeNodeRules.RecipeDiscount,out var q);s.TryPurchaseUpgrade(q);
                Check(unchanged.Price==1&&s.RecipePrice(d)==1&&!s.TryPurchaseRecipe(unchanged),"rank stamp rejects stale quote even when rounded price is identical");
            }
            finally{UnityEngine.Object.DestroyImmediate(rc);UnityEngine.Object.DestroyImmediate(d);}
            return "Recipe discount model PASS: all32 prices×ranks0–5, prerequisite/cap/authored prices, distinct upgrade discount, stale-rank/equal-rounded-price guards and exact single recipe debit.";
        }
        public static string RunStorageChecks()
        {
            string root=Path.GetFullPath("Temp/ClickCookSaveTests/RecipeDiscount-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
            for(int version=1;version<=18;version++)
            {
                string source=Signed(version,Payload(version)),path=Path.Combine(root,version+".json");File.WriteAllText(path,source);var p=new PlayerProfileStore(path).Load();
                Check(p!=null&&p.GetUpgradeRank(UpgradeNodeRules.RecipeDiscount)==0&&p.Rewards.Wallet==10000.25m&&p.PlayerXp==(version>=18?100.0001m:0)&&File.ReadAllText(path)==source,"legacy no free discount/no rewrite "+version);
                Check(Decode(Signed(version,Payload(version,0)))==null,"old schema rejects node even at zero "+version);
            }
            for(int rank=1;rank<=5;rank++)
            {
                var store=new PlayerProfileStore(Path.Combine(root,"rank"+rank+".json"));var p=Decode(Signed(19,Payload(19,rank,5)));
                Check(p!=null&&store.Save(p),"save rank");var loaded=store.Load();Check(loaded.GetUpgradeRank(UpgradeNodeRules.RecipeDiscount)==rank&&loaded.GetUpgradeRank(UpgradeNodeRules.UpgradeDiscount)==5&&loaded.PlayerXp==100.0001m&&loaded.Rewards.Wallet==10000.25m,"rank/XP/wallet roundtrip");
                File.WriteAllText(store.SavePath,"broken");Check(new PlayerProfileStore(store.SavePath).Load().GetUpgradeRank(UpgradeNodeRules.RecipeDiscount)==rank,"discount backup recovery");
                Check(store.Save(new PlayerProfile(),true)&&store.Load().GetUpgradeRank(UpgradeNodeRules.RecipeDiscount)==0&&new PlayerProfileStore(store.BackupPath).Load().GetUpgradeRank(UpgradeNodeRules.RecipeDiscount)==0,"reset both rank copies");
            }
            Check(Decode(Signed(19,Payload(19,6)))==null&&Decode(Signed(19,Payload(19,-1)))==null&&Decode(Signed(19,Payload(19,1,0,false)))==null,"invalid cap/negative/prerequisite");
            string futurePath=Path.Combine(root,"future.json"),future=Signed(PlayerProfileStore.CurrentVersion+1,Payload(19,1));File.WriteAllText(futurePath,future);var futureStore=new PlayerProfileStore(futurePath);
            Check(futureStore.Load()==null&&futureStore.Status==ProfileLoadStatus.Unsupported&&!futureStore.Save(new PlayerProfile())&&File.ReadAllText(futurePath)==future,"future20 protected");
            return "Recipe discount storage PASS: schemas1–18 exact/no free ranks, schema19 independent discounts/PlayerXP/wallet, backup/reset/caps/prerequisite and future20 protection.";
        }
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying,"prepare stopped");string path=Path.GetFullPath("Temp/ClickCookSaveTests/RecipeDiscountPlay-"+Guid.NewGuid().ToString("N")+"/profile.json");
            Check(new PlayerProfileStore(path).Save(Decode(Signed(19,Payload(19,0)))),"isolated profile");SessionState.SetString("ClickCook.SaveVerificationPath",path);return path;
        }
        public static string VerifyPlay()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();c.OpenUpgrades();c.SelectUpgradeNode(UpgradeNodeRules.RecipeDiscount);
            var tree=Field<UpgradeTreeView>(c,"upgradeTreeView");tree.RevealSelection();var text=Field<UnityEngine.UI.Text>(tree,"statistics");Canvas.ForceUpdateCanvases();
            Check(text.text.Contains("Знижка на рецепти: 0%")&&text.preferredHeight<=text.rectTransform.rect.height+1&&Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"new node/statistics fit");
            return "Recipe discount UI PASS: working node rank0/5,150 price,10-stat panel fits. Ready for native node purchase.";
        }
        public static string VerifyNativeUpgrade()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();c.GetComponent<ProfileMenu>().Resume();Check(c.OrderSession.RecipeDiscountRank==1&&c.OrderSession.Rewards.Wallet==9850.25m,"native upgrade exact");
            var saved=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(saved.GetUpgradeRank(UpgradeNodeRules.RecipeDiscount)==1&&saved.Rewards.Wallet==9850.25m,"upgrade autosave");
            c.CloseUpgrades();c.OpenRecipes();var v=Field<RecipeShopView>(c,"recipeShop");v.SelectRow(1);var details=Field<UnityEngine.UI.Text>(v,"details");Canvas.ForceUpdateCanvases();
            Check(details.text.Contains(ClickCookUiCopy.Money(149))&&details.preferredHeight<=details.rectTransform.rect.height+1,"shop effective price and fit");return "Native discount upgrade/autosave PASS. Shop recipe2 now149 instead of150.";
        }
        public static string VerifyNativeRecipe()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();Check(c.OrderSession.Profile.OwnsRecipe(2)&&c.OrderSession.Rewards.Wallet==9701.25m,"native discounted recipe debit");
            var loaded=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();Check(loaded.OwnsRecipe(2)&&loaded.GetUpgradeRank(UpgradeNodeRules.RecipeDiscount)==1&&loaded.Rewards.Wallet==9701.25m&&loaded.PlayerXp==100.0001m,"discounted recipe autosave exact");
            return "Native discounted recipe149/autosave PASS: wallet9701.25, recipe2, discount1 and prior PlayerXP unchanged.";
        }
        public static string VerifyRestart()
        {
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m=c.GetComponent<ProfileMenu>();m.Continue();m.Resume();var s=c.OrderSession;
            Check(s.RecipeDiscountRank==1&&s.Profile.OwnsRecipe(2)&&s.Rewards.Wallet==9701.25m&&s.Profile.PlayerXp==100.0001m&&!s.IsDayActive,"restart exact state");
            c.OpenRecipes();return "Discount restart/Continue PASS: rank/recipe/wallet/XP exact, idle day.";
        }
    }
}
