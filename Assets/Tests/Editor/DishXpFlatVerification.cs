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
    public static class DishXpFlatVerification
    {
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        [Serializable] private sealed class Envelope { public int version; public string payload; public string checksum; }
        private static void Check(bool value, string message) { if (!value) throw new Exception("U5a: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static UpgradeCatalog Catalog => AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
        private static string Rank(string id, int rank) => "{\"id\":\"" + id + "\",\"rank\":" + rank + "}";
        private static string Signed(int version, decimal wallet, int xp, params string[] ranks)
        {
            string payload = "{\"wallet\":\"" + wallet.ToString(CultureInfo.InvariantCulture) + "\",\"dishes\":[{\"id\":1,\"xp\":" + xp + "},{\"id\":2,\"xp\":17}],\"upgrades\":[" + string.Join(",", ranks) + "]}";
            using var sha = SHA256.Create(); return JsonUtility.ToJson(new Envelope { version = version, payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(payload))) });
        }
        private static string[] Ranks(int flat = 0) => new[] {
            Rank(UpgradeNodeRules.Strength, 3), Rank(UpgradeNodeRules.CriticalChance, 1), Rank(UpgradeNodeRules.CriticalMultiplier, 1), Rank(UpgradeNodeRules.Hold, 1), Rank(UpgradeNodeRules.HoldFrequency, 1),
            Rank(UpgradeNodeRules.DoubleStrikeChance, 10), Rank(UpgradeNodeRules.DoubleStrikePower, 1), Rank(UpgradeNodeRules.DoubleStrikeChanceII, 1), Rank(UpgradeNodeRules.TripleStrikeChance, 1),
            Rank(UpgradeNodeRules.AutoActionCount, 1), Rank(UpgradeNodeRules.AutoActionChance, 1), Rank(UpgradeNodeRules.AutoStrength, 2), Rank(UpgradeNodeRules.AutoStrengthMultiplier, 2), Rank(UpgradeNodeRules.AutoFrequency, 2), Rank(UpgradeNodeRules.DishXpFlat, flat) };
        private static PlayerProfile Decode(string json) => (PlayerProfile)typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { json, ProfileLoadStatus.Invalid });
        private static PrototypeOrderSession Session(PlayerProfile p, bool start = false)
        { var s = new PrototypeOrderSession(p, () => 0, () => .22, () => start ? 0 : .99); Check(s.ConfigureUpgrades(Catalog), "configure"); return s; }
        private static PrototypeOrderSession Session(int flat, int xp = 0, bool start = false) => Session(Decode(Signed(12, 10000.25m, xp, Ranks(flat))), start);
        private static void SetDish(DishData dish, int hp)
        { var so = new SerializedObject(dish); so.FindProperty("dishId").intValue = 1; so.FindProperty("clickRequirement").intValue = hp; so.FindProperty("saleValue").intValue = 5; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static void SetPool(LocationData location, DishData dish)
        { var so = new SerializedObject(location); var pool = so.FindProperty("documentedAvailableDishes"); pool.arraySize = 1; pool.GetArrayElementAtIndex(0).objectReferenceValue = dish; so.ApplyModifiedPropertiesWithoutUndo(); }
        private static PrototypeOrder Begin(PrototypeOrderSession s, LocationData l) { s.StartDay(l, 2); Check(s.TrySelect(0), "select"); return s.ActiveOrder; }
        private static int XpAtLevel(int level) => Enumerable.Range(1, level - 1).Sum(PrototypeDishRewards.RequiredXp);
        private static int SeedFor(int level, DishRarity rarity)
        { for (int seed = 0; seed < 10000; seed++) { UnityEngine.Random.InitState(seed); if (PrototypeDishRewards.ResolveRarity(level, UnityEngine.Random.Range(0, 100)) == rarity) return seed; } throw new Exception("No rarity seed"); }

        public static string RunModelChecks()
        {
            var random = UnityEngine.Random.state; var dish = ScriptableObject.CreateInstance<DishData>(); var location = ScriptableObject.CreateInstance<LocationData>();
            try
            {
                SetDish(dish, 1); SetPool(location, dish);
                var locked = Session(Decode(Signed(12, 1000, 0, Rank(UpgradeNodeRules.Strength, 0)))); Check(!locked.TryQuoteUpgrade(UpgradeNodeRules.DishXpFlat, out _), "strength prerequisite");
                var poor = Session(Decode(Signed(12, 99.99m, 0, Ranks()))); Check(poor.TryQuoteUpgrade(UpgradeNodeRules.DishXpFlat, out var costly) && !poor.TryPurchaseUpgrade(costly) && poor.Rewards.Wallet == 99.99m, "insufficient wallet");
                decimal[] prices = {100,170,289,492,836}; var buyer = Session(0); decimal spent = 0;
                for (int n = 0; n < 5; n++)
                {
                    Check(buyer.TryQuoteUpgrade(UpgradeNodeRules.DishXpFlat, out var q) && q.Price == prices[n], "known price"); buyer.IsPaused = true; Check(!buyer.TryPurchaseUpgrade(q), "pause purchase"); buyer.IsPaused = false; buyer.StartDay(location, 1); Check(!buyer.TryPurchaseUpgrade(q), "day purchase"); buyer.EndDay();
                    Check(!locked.TryPurchaseUpgrade(q) && buyer.TryPurchaseUpgrade(q) && !buyer.TryPurchaseUpgrade(q), "owner/replay"); spent += prices[n]; Check(buyer.DishXpFlatRank == n + 1 && buyer.DishXpBonus == 3 * (n + 1), "rank contribution");
                }
                Check(spent == 1887 && buyer.Rewards.Wallet == 10000.25m - spent && !buyer.TryQuoteUpgrade(UpgradeNodeRules.DishXpFlat, out _), "cap/total");
                foreach (string id in new[] {"node.dish_xp.02","node.dish_xp_flat.04","node.dish_xp_flat.04","node.character_xp_flat.01","node.restaurant_xp_flat.01"}) Check(!buyer.TryQuoteUpgrade(id, out _), "unfinished effect unavailable");
                var tree = UpgradeTreeDefinition.Load(); Check(tree.PrerequisitesMet(tree.Find("node.dish_xp.01"), buyer.Profile.GetUpgradeRank) && tree.PrerequisitesMet(tree.Find("node.dish_xp_flat.02"), buyer.Profile.GetUpgradeRank), "manifest sees purchased flat ranks");
                int[] awards = {10,12,16,22,30}; decimal[] payouts = {5,6.25m,8.75m,12.5m,20};
                for (int rank = 0; rank <= 5; rank++) for (int tier = 0; tier < 5; tier++)
                {
                    int previousLevel = tier == 0 ? 1 : 7, xp = XpAtLevel(previousLevel); var s = Session(rank, xp); var order = Begin(s, location); int changes = 0; s.Profile.Changed += () => changes++;
                    UnityEngine.Random.InitState(SeedFor(previousLevel, (DishRarity)tier)); decimal before = s.Rewards.Wallet; Check(s.TryCookClick(order), "manual completion");
                    Check(s.LastResult.Rarity == (DishRarity)tier && s.LastResult.PreviousLevel == previousLevel && s.LastResult.XpGained == awards[tier] + rank * 3 && s.Rewards.GetTotalXp(dish) == xp + awards[tier] + rank * 3 && s.LastResult.Payout == payouts[tier] && s.Rewards.Wallet == before + payouts[tier] && changes == 1 && s.Profile.CaptureSnapshot().DishXpById[2] == 17, "seeded rarity/base+flat/old-level payout/single notification/cross-dish isolation");
                    var receipt = s.LastResult; decimal paid = s.Rewards.Wallet; Check(!s.TryCookClick(order) && !s.TryCookAutoStart(order) && !s.TryCookAuto(order, 100) && s.LastResult == receipt && s.Rewards.Wallet == paid && changes == 1, "stale reward guards"); s.EndDay();
                }
                int cap = XpAtLevel(18);
                foreach (int previous in new[] { cap - 5, cap })
                { var s = Session(5, previous); var order = Begin(s, location); s.TryCookClick(order); Check(s.Rewards.GetTotalXp(dish) == cap && s.LastResult.XpGained == cap - previous && s.LastResult.Level == 18 && s.Rewards.GetLevelXp(dish) == 0, "mastery clamp/actual receipt"); s.EndDay(); }
                var crossing = Session(5, 19); var selected = Begin(crossing, location); UnityEngine.Random.InitState(SeedFor(1,DishRarity.Common)); crossing.TryCookClick(selected); Check(crossing.LastResult.XpGained == 25 && crossing.LastResult.PreviousLevel == 1 && crossing.LastResult.Level == 3 && crossing.Rewards.GetTotalXp(dish) == 44, "multi-level crossing"); crossing.EndDay();
                foreach (string source in new[] {"hold","start","periodic"})
                {
                    var s = Session(2, start: source == "start"); var order = Begin(s, location); UnityEngine.Random.InitState(SeedFor(1,DishRarity.Common));
                    bool done; if (source == "start") done = s.TryCookAutoStart(order); else if (source == "periodic") { s.TryCookAuto(order,0); done=s.TryCookAuto(order,s.AutoInterval); } else { Check(s.TryBeginHold(order,0),"hold begin"); done=s.TryCookClick(order); }
                    Check(done && s.LastResult.XpGained == 16 && s.Rewards.GetTotalXp(dish) == 16 && s.CompletedDishes == 1, "same flat once for " + source); s.EndDay();
                }
                SetDish(dish, 1000); var partial = Session(5); selected = Begin(partial, location); partial.TryCookClick(selected); partial.ReturnToBoard(); partial.EndDay(); Check(partial.Rewards.GetTotalXp(dish)==0 && partial.LastResult==null, "no XP for partial/Back/end");
                return "U5a model PASS: prices/cap/guards, all five rarities across ranks 0–5, old-level rarity/payout, mastery clamp/multi-level crossing, source parity and single isolated receipt.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(dish); UnityEngine.Object.DestroyImmediate(location); }
        }

        public static string RunStorageChecks()
        {
            string folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/U5aStore/"+Guid.NewGuid().ToString("N"))); Directory.CreateDirectory(folder);
            for(int version=1;version<=11;version++)
            {
                int length=version>=6?version+3:version==5?7:version+1; string[] ranks=version==1?new[]{Rank("click_strength",3)}:Ranks().Take(length).ToArray(); string old=Signed(version,2500.25m,50,ranks), path=Path.Combine(folder,"v"+version+".json"); File.WriteAllText(path,old);
                var store=new PlayerProfileStore(path); var p=store.Load(); var before=p.CaptureSnapshot(); Check(p.GetUpgradeRank(UpgradeNodeRules.DishXpFlat)==0 && Session(p).DishXpBonus==0 && File.ReadAllText(path)==old,"old bonus/no rewrite");
                Check(store.Save(p) && JsonUtility.FromJson<Envelope>(File.ReadAllText(path)).version==PlayerProfileStore.CurrentVersion && File.ReadAllText(store.BackupPath)==old,"schema/backup"); var after=new PlayerProfileStore(path).Load().CaptureSnapshot();
                Check(after.Wallet==2500.25m && after.DishXpById[1]==50 && after.DishXpById[2]==17 && before.UpgradeRanksById.Count==after.UpgradeRanksById.Count && before.UpgradeRanksById.All(x=>after.UpgradeRanksById[x.Key]==x.Value),"exact old state"); File.WriteAllText(path,"broken"); var recovery=new PlayerProfileStore(path); Check(recovery.Load()!=null && recovery.Status==ProfileLoadStatus.Recovered,"old backup recovery");
                Check(Decode(Signed(version,1000,0,ranks.Concat(new[]{Rank(UpgradeNodeRules.DishXpFlat,0)}).ToArray()))==null,"old schema rejects new ID even zero");
            }
            foreach(int rank in new[]{1,5})
            { string path=Path.Combine(folder,"r"+rank+".json"); Check(new PlayerProfileStore(path).Save(Decode(Signed(12,1000.25m,50,Ranks(rank)))),"write flat"); var s=Session(new PlayerProfileStore(path).Load()); Check(s.DishXpFlatRank==rank && s.DishXpBonus==rank*3 && s.AutoFrequencyRank==2 && s.AutoDamagePerAction==4 && s.Rewards.Wallet==1000.25m && s.Profile.CaptureSnapshot().DishXpById[1]==50 && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto,"independent idle roundtrip"); }
            foreach(var ranks in new[]{Ranks(-1),Ranks(6),new[]{Rank(UpgradeNodeRules.Strength,0),Rank(UpgradeNodeRules.DishXpFlat,1)},Ranks(1).Concat(new[]{Rank(UpgradeNodeRules.DishXpFlat,1)}).ToArray(),Ranks().Concat(new[]{Rank("node.dish_xp_flat.04",1)}).ToArray(),Ranks().Concat(new[]{Rank("dish_xp_flat",1)}).ToArray()}) Check(Decode(Signed(12,1000,0,ranks))==null,"invalid cap/prerequisite/duplicate/tier/alias");
            string future=Signed(PlayerProfileStore.CurrentVersion + 1,1000,0,Ranks()), futurePath=Path.Combine(folder,"future.json"); File.WriteAllText(futurePath,future); var blocked=new PlayerProfileStore(futurePath); Check(blocked.Load()==null && blocked.Status==ProfileLoadStatus.Unsupported && !blocked.Save(Decode(Signed(12,1000,0,Ranks()))) && File.ReadAllText(futurePath)==future,"future protected");
            return "U5a storage PASS: schemas 1–11 to 12, exact wallet/XP/ranks/backups, cap/prerequisite/whitelist/idle roundtrip and version 13 protection.";
        }

        private static ClickCookPrototypeController Controller=>UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static DishData playDish; private static LocationData playLocation;
        public static string PreparePlay()
        { Check(!EditorApplication.isPlaying,"prepare stopped"); string path=Path.GetFullPath(Path.Combine(Application.dataPath,"../Temp/ClickCookSaveTests/U5a-"+Guid.NewGuid().ToString("N"),"profile.json")); Directory.CreateDirectory(Path.GetDirectoryName(path)); File.WriteAllText(path,Signed(11,300.25m,0,Ranks().Take(14).ToArray())); SessionState.SetString("ClickCook.SaveVerificationPath",path); return path; }
        public static string ShowNativePurchase()
        { Check(PlayerProfileRuntime.Store.SavePath.Contains("U5a-"),"isolated profile"); var c=Controller; c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.DishXpFlat); Field<UpgradeTreeView>(c,"upgradeTreeView").FocusNode(UpgradeNodeRules.DishXpFlat); Check(Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text.Contains("0 / 5") && Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"ready native purchase"); return "U5a ready, wallet 300.25."; }
        public static string CheckNativePurchase(int rank)
        { var s=Controller.OrderSession; decimal wallet=rank==1?200.25m:30.25m; var disk=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(s.DishXpFlatRank==rank && s.DishXpBonus==rank*3 && s.Rewards.Wallet==wallet && disk.GetUpgradeRank(UpgradeNodeRules.DishXpFlat)==rank && disk.Rewards.Wallet==wallet && disk.CaptureSnapshot().DishXpById[1]==0,"native single debit/bonus/no purchase XP/autosave"); return "U5a native rank "+rank+" PASS."; }
        public static string CheckUiAndComplete()
        {
            CheckNativePurchase(2); var c=Controller; c.GetComponent<ProfileMenu>().Resume(); string detail=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText").text;
            Check(detail.Contains("2 / 5") && detail.Contains("+6 XP") && detail.Contains("+9 XP") && detail.Contains(ClickCookUiCopy.Money(289)) && !Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable,"detail bonus/next/funds");
            var stats=Field<UpgradeTreeView>(c,"upgradeTreeView").transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>(); Canvas.ForceUpdateCanvases(); Check(stats.text.Contains("Досвід страви: +6 XP") && stats.text.Split('\n').Length==9 && stats.preferredHeight<=stats.rectTransform.rect.height+1 && stats.preferredWidth<=stats.rectTransform.rect.width+1,"seven stats fit");
            c.CloseUpgrades(); var s=Session(c.OrderSession.Profile); typeof(ClickCookPrototypeController).GetField("session",Private).SetValue(c,s); c.CookSelectedLocation(); playDish=UnityEngine.Object.Instantiate(s.GetOrderAtSlot(0).Dish); playLocation=UnityEngine.Object.Instantiate(s.SelectedLocation); SetDish(playDish,1); SetPool(playLocation,playDish); s.StartDay(playLocation,2); c.SelectOrder(0);
            int seed=SeedFor(1,DishRarity.Common); UnityEngine.Random.InitState(seed); c.ClickCookingDish(); Check(s.LastResult.XpGained==16 && s.Rewards.GetTotalXp(playDish)==16 && s.CompletedDishes==1 && s.Rewards.Wallet==35.25m && Field<GameObject>(c,"dishResultScreen").activeSelf,"controller flat receipt/result");
            var result=Field<UnityEngine.UI.Text>(c,"dishResultText"); Canvas.ForceUpdateCanvases(); Check(result.text.Contains("+16 XP") && result.preferredHeight<=result.rectTransform.rect.height+1,"actual result text fit"); c.ClickCookingDish(); Check(s.Rewards.Wallet==35.25m && s.Rewards.GetTotalXp(playDish)==16 && s.CompletedDishes==1,"stale result guards");
            var disk=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load(); Check(disk.GetUpgradeRank(UpgradeNodeRules.DishXpFlat)==2 && disk.Rewards.Wallet==35.25m && disk.CaptureSnapshot().DishXpById[1]==16 && disk.CaptureSnapshot().DishXpById[2]==17,"result autosave"); return "U5a UI/controller result +16 XP/autosave PASS.";
        }
        public static string FinishLive()
        { var c=Controller; c.ReturnToProfileMenu(); UnityEngine.Object.DestroyImmediate(playDish); UnityEngine.Object.DestroyImmediate(playLocation); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.DishXpFlat); Field<UpgradeTreeView>(c,"upgradeTreeView").FocusNode(UpgradeNodeRules.DishXpFlat); return "U5a fixture released."; }
        public static string VerifyRestart()
        { Check(PlayerProfileRuntime.Store.SavePath.Contains("U5a-"),"isolated restart"); var c=Controller; var s=c.OrderSession; Check(s.Profile.CaptureSnapshot().UpgradeRanksById.Count==15 && s.DishXpFlatRank==2 && s.DishXpBonus==6 && s.Rewards.Wallet==35.25m && s.Profile.CaptureSnapshot().DishXpById[1]==16 && s.Profile.CaptureSnapshot().DishXpById[2]==17 && s.AutoFrequencyRank==2 && s.AutoStrengthMultiplierRank==2 && s.AutoDamagePerAction==4 && !s.IsDayActive && !s.IsHolding && !s.LastActionWasAuto,"all ranks/exact wallet/XP/idle"); c.GetComponent<ProfileMenu>().Continue(); c.GetComponent<ProfileMenu>().Resume(); c.OpenUpgrades(); c.SelectUpgradeNode(UpgradeNodeRules.DishXpFlat); return "U5a restart/Continue PASS."; }
        public static string Cleanup() { Check(!EditorApplication.isPlaying,"cleanup stopped"); return PlayerProfileStoreVerification.FinishPlayVerification(); }
    }
}