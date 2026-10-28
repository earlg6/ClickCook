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
    public static class RecipeVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static T Field<T>(object o, string name) => (T)o.GetType().GetField(name, Private).GetValue(o);
        private static void Check(bool value, string message) { if (!value) throw new Exception("8B: " + message); }
        private static RecipeCatalog Catalog => AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset");
        private static LocationCatalog Locations => AssetDatabase.LoadAssetAtPath<LocationCatalog>("Assets/Data/LocationCatalog.asset");
        private static string Signed(int version, string payload)
        {
            using var sha = SHA256.Create();
            return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string Payload(string recipes = "[1]", bool xp = false, bool legacy = false) =>
            "{\"wallet\":\"1000.25\",\"dishes\":[{\"id\":2,\"xp\":" + (xp ? "17" : "0") + (legacy ? "" : ",\"hundredths\":37") + "}],\"upgrades\":[]" + (recipes == null ? "" : ",\"recipes\":" + recipes) + "}";
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PlayerProfile Funded() => Decode(Signed(17, "{\"wallet\":\"1000.25\",\"dishes\":[],\"upgrades\":[],\"recipes\":[1]}"));
        private static PrototypeOrderSession Session(PlayerProfile profile)
        {
            var s = new PrototypeOrderSession(profile); s.ConfigureRecipes(Catalog);
            s.ConfigureUpgrades(AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset")); return s;
        }
        public static string RunModelChecks()
        {
            var random = UnityEngine.Random.state;
            try
            {
                Check(Catalog.Dishes.Count == 32 && Catalog.Dishes.Select(d => d.DishId).SequenceEqual(Enumerable.Range(1, 32)), "ordered stable catalog");
                Check(Catalog.Find(1).RecipeCost == 0 && Catalog.Find(2).RecipeCost == 150 && Catalog.Dishes.Skip(1).All(d => d.RecipeCost > 0), "authored prices/free starter");
                Check(Locations.Locations.Last().DocumentedAvailableDishes.Select(d => d.DishId).SequenceEqual(new[] { 29, 30, 31, 32 }), "owner Michelin decision");
                var fresh = Session(new PlayerProfile());
                Check(fresh.Profile.CaptureSnapshot().OwnedRecipeIds.SequenceEqual(new[] { 1 }), "fresh only starter");
                foreach (var location in Locations.Locations)
                    Check(fresh.AvailableRecipeCount(location) == location.DocumentedAvailableDishes.Count(d => d.DishId == 1), "owned pool intersection");
                var all = Session(Decode(Signed(17, "{\"wallet\":\"0\",\"dishes\":[],\"upgrades\":[],\"recipes\":[" + string.Join(",", Enumerable.Range(1, 32)) + "]}")));
                foreach (var location in Locations.Locations)
                {
                    Check(all.AvailableRecipeCount(location) == location.DocumentedAvailableDishes.Count, "all-owned exact authored pool");
                    all.StartDay(location, 6);
                    Check(all.Orders.Count == 6 && all.Orders.All(o => location.DocumentedAvailableDishes.Contains(o.Dish)), "board retains authored eligibility");
                    all.EndDay();
                }
                Check(!fresh.TryQuoteRecipe(1, out _) && !fresh.TryQuoteRecipe(0, out _) && !fresh.TryQuoteRecipe(33, out _), "starter/invalid quote rejected");
                Check(fresh.TryQuoteRecipe(2, out var poor) && !fresh.TryPurchaseRecipe(poor) && fresh.Rewards.Wallet == 0 && !fresh.Profile.OwnsRecipe(2), "insufficient money no mutation");
                var s = Session(Funded()); var other = Session(s.Profile); int changes = 0; s.Profile.Changed += () => changes++;
                Check(s.TryQuoteRecipe(2, out var quote) && quote.Price == 150 && !other.TryPurchaseRecipe(quote), "price/owner guard");
                s.IsPaused = true; Check(!s.TryPurchaseRecipe(quote) && !s.TryQuoteRecipe(2, out _), "pause guard"); s.IsPaused = false;
                other.StartDay(Locations.Locations[0], 2);
                Check(!s.TryQuoteRecipe(2, out _) && !s.TryPurchaseRecipe(quote), "shared-profile day guard"); other.EndDay();
                Check(s.TryPurchaseRecipe(quote) && !s.TryPurchaseRecipe(quote) && !s.TryQuoteRecipe(2, out _) && s.Rewards.Wallet == 850.25m && changes == 1,
                    "exact single debit/notification/ownership");
                Check(s.Rewards.GetExactTotalXp(Catalog.Find(2)) == 0 && s.Profile.OwnsRecipe(2) && !s.Profile.OwnsRecipe(3), "purchase no XP/unrelated grants");
                Check(s.AvailableRecipeCount(Locations.Locations[0]) == 2 && s.AvailableRecipeCount(Locations.Locations[1]) == 2, "ownership global across eligible locations");
                s.StartDay(Locations.Locations[1], 6);
                for (int n = 0; n < 10; n++)
                {
                    Check(s.Orders.All(o => o.Dish.DishId == 1 || o.Dish.DishId == 2), "initial/replacement pool excludes unowned");
                    s.TrySelect(0); var order = s.ActiveOrder;
                    while (s.ActiveOrder != null) s.TryCookClick(order);
                }
                Check(s.TryFinishDay(out var report) && report.Receipts.Count == 10 && report.Receipts.All(r => s.Profile.OwnsRecipe(r.Dish.DishId)), "receipt summary integration");
                var snapshot = s.Profile.CaptureSnapshot(); s.TryQuoteRecipe(3, out var third); s.TryPurchaseRecipe(third);
                Check(!snapshot.OwnedRecipeIds.Contains(3) && snapshot.OwnedRecipeIds.Contains(2), "detached snapshot");
                bool immutable = false; try { ((IList<int>)snapshot.OwnedRecipeIds).Add(32); } catch (NotSupportedException) { immutable = true; }
                Check(immutable, "read-only ownership");
                var clone = UnityEngine.Object.Instantiate(Catalog); var dish = UnityEngine.Object.Instantiate(Catalog.Find(2));
                try
                {
                    var cs = new SerializedObject(clone); cs.FindProperty("dishes").GetArrayElementAtIndex(1).objectReferenceValue = dish; cs.ApplyModifiedPropertiesWithoutUndo();
                    var guarded = Session(Funded()); guarded.ConfigureRecipes(clone); guarded.TryQuoteRecipe(2, out var stale);
                    var ds = new SerializedObject(dish); ds.FindProperty("recipeCost").intValue = 1; ds.ApplyModifiedPropertiesWithoutUndo();
                    Check(!guarded.TryPurchaseRecipe(stale) && guarded.Rewards.Wallet == 1000.25m, "changed price quote rejected");
                }
                finally { UnityEngine.Object.DestroyImmediate(clone); UnityEngine.Object.DestroyImmediate(dish); }
                return "8B model PASS: catalog/prices/Michelin, starter ownership, pool intersections/replacements, quote/funds/pause/day/owner/replay/price guards, exact debit, snapshot isolation and 8A receipts.";
            }
            finally { UnityEngine.Random.state = random; }
        }
        public static string RunStorageChecks()
        {
            string root = Path.GetFullPath("Temp/ClickCookSaveTests/Recipes-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(root);
            for (int v = 1; v <= 16; v++)
            {
                string path = Path.Combine(root, "legacy" + v + ".json");
                string old = Signed(v, Payload(null, true, v < 13)); File.WriteAllText(path, old);
                var p = new PlayerProfileStore(path).Load();
                Check(p != null && p.OwnsRecipe(1) && p.OwnsRecipe(2) && !p.OwnsRecipe(3) && p.Rewards.Wallet == 1000.25m &&
                    p.Rewards.GetExactTotalXp(Catalog.Find(2)) == (v < 13 ? 17m : 17.37m) && File.ReadAllText(path) == old, "legacy migration exact/no eager rewrite v" + v);
            }
            Check(!Decode(Signed(16, Payload(null, false, true))).OwnsRecipe(2), "zero-XP legacy no free recipe");
            var store = new PlayerProfileStore(Path.Combine(root, "current.json"));
                var original = Decode(Signed(17, Payload("[1,2]", true).Replace("\"upgrades\":[]", "\"upgrades\":[{\"id\":\"node.click_strength.01\",\"rank\":3}]")));
            Check(store.Save(original), "save current"); var loaded = store.Load();
            Check(loaded.OwnsRecipe(1) && loaded.OwnsRecipe(2) && loaded.GetUpgradeRank(UpgradeNodeRules.Strength) == 3 && loaded.Rewards.Wallet == 1000.25m && loaded.Rewards.GetExactTotalXp(Catalog.Find(2)) == 17.37m && !loaded.HasActiveDay, "current exact idle roundtrip");
            File.WriteAllText(store.SavePath, "broken"); loaded = new PlayerProfileStore(store.SavePath).Load();
            Check(loaded != null && loaded.OwnsRecipe(2), "ownership backup recovery");
            Check(store.Save(new PlayerProfile(), true) && !store.Load().OwnsRecipe(2) && !new PlayerProfileStore(store.BackupPath).Load().OwnsRecipe(2), "new game resets both copies");
            foreach (var ids in new[] { null, "[]", "[2]", "[1,1]", "[1,0]", "[1,33]", "[1,-1]", "[1]" })
                Check(Decode(Signed(17, Payload(ids, true))) == null, "invalid ownership/XP relation " + ids);
            Check(Decode(Signed(16, Payload("[1,2]", true))) == null, "legacy cannot smuggle recipe field");
            string future = Signed(18, Payload("[1,2]", true)); File.WriteAllText(store.SavePath, future);
            var protectedStore = new PlayerProfileStore(store.SavePath);
            Check(protectedStore.Load() == null && protectedStore.Status == ProfileLoadStatus.Unsupported && !protectedStore.Save(new PlayerProfile()) && File.ReadAllText(store.SavePath) == future, "future protected");
            return "8B storage PASS: schema1–16 migration, exact money/fraction XP, no eager rewrite/free locked recipes, schema17 ownership/backup/reset, malformed data and future-version protection.";
        }
        public static string PreparePlay()
        {
            Check(!EditorApplication.isPlaying, "prepare stopped");
            string path = Path.GetFullPath("Temp/ClickCookSaveTests/RecipesPlay-" + Guid.NewGuid().ToString("N") + "/profile.json");
            Check(new PlayerProfileStore(path).Save(Funded()), "isolated save"); SessionState.SetString("ClickCook.SaveVerificationPath", path); return path;
        }
        public static string VerifyPlay()
        {
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>(); var menu = c.GetComponent<ProfileMenu>(); menu.Continue(); menu.Resume();
            var s = c.OrderSession;
            Check(s.Profile.OwnsRecipe(1) && !s.Profile.OwnsRecipe(2), "isolated new ownership");
            c.CookSelectedLocation(); Check(s.Orders.Count == 2 && s.Orders.All(o => o.Dish.DishId == 1), "fresh board only sandwich");
            c.OpenRecipes(); Check(!Field<RecipeShopView>(c, "recipeShop").gameObject.activeSelf, "shop blocked during day");
            c.FinishDay(); c.NextDay(); c.ShowNextLocation(); c.ShowNextLocation();
            Check(!Field<UnityEngine.UI.Button>(c, "cookButton").interactable, "location with no recipes has disabled Cook"); c.CookSelectedLocation(); Check(!s.IsDayActive, "callback cannot bypass empty pool");
            c.OpenRecipes(); var view = Field<RecipeShopView>(c, "recipeShop"); view.SelectRow(1);
            var details = Field<UnityEngine.UI.Text>(view, "details"); Canvas.ForceUpdateCanvases();
            Check(view.gameObject.activeInHierarchy && details.preferredHeight <= details.rectTransform.rect.height + 1 && Field<UnityEngine.UI.Button>(view, "buy").interactable, "shop detail/buy fit");
            return "8B Play preparation PASS: owned board, active-day shop guard, zero-pool Cook guard and shop detail fit. Mug-cake recipe selected for native purchase.";
        }
        public static string VerifyNativePurchase()
        {
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>(); var s = c.OrderSession; var view = Field<RecipeShopView>(c, "recipeShop");
            Check(s.Profile.OwnsRecipe(2) && s.Rewards.Wallet == 850.25m, "native purchase exact once"); view.Purchase();
            Check(s.Rewards.Wallet == 850.25m, "replayed native purchase no debit");
            var disk = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(disk.OwnsRecipe(2) && disk.Rewards.Wallet == 850.25m, "native purchase autosave");
            return "8B native purchase/autosave PASS. Wallet850.25, recipe2 owned once.";
        }
        public static string VerifyRestart()
        {
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var m = c.GetComponent<ProfileMenu>();m.Continue();m.Resume();
            Check(c.OrderSession.Profile.OwnsRecipe(2) && c.OrderSession.Rewards.Wallet == 850.25m && !c.OrderSession.IsDayActive, "restart Continue retains ownership/wallet and idle day");
            c.CookSelectedLocation(); Check(c.OrderSession.AvailableRecipeCount(c.OrderSession.SelectedLocation) == 2 && c.OrderSession.Orders.All(o => o.Dish.DishId == 1 || o.Dish.DishId == 2), "purchased recipe available after restart");
            c.FinishDay(); c.NextDay(); return "8B restart/Continue PASS: exact wallet/ownership, eligible board, clean 8A finish.";
        }
    }
}
