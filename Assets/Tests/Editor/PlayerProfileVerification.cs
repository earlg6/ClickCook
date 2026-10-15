using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.Runtime.Locations;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ClickCook.Verification
{
    /// <summary>Editor-only repeatable checks; no production currency setters or restore API.</summary>
    public static class PlayerProfileVerification
    {
        private static int checks;
        private static PlayerProfile beforeReload;
        private static PlayerProfileSnapshot snapshotBeforeReload;
        private static PrototypeOrderSession oldSession;
        private static PrototypeOrder oldOrder;
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        private static void Check(bool condition, string message)
        {
            checks++;
            if (!condition) throw new Exception("7A: " + message);
        }

        private static void SameValues(PlayerProfileSnapshot a, PlayerProfileSnapshot b)
        {
            Check(a.Wallet == b.Wallet, "exact wallet retained");
            Check(a.DishXpById.Count == b.DishXpById.Count &&
                a.DishXpById.All(pair => b.DishXpById.TryGetValue(pair.Key, out int xp) && xp == pair.Value), "XP retained");
            Check(a.UpgradeRanksById.Count == b.UpgradeRanksById.Count &&
                a.UpgradeRanksById.All(pair => b.UpgradeRanksById.TryGetValue(pair.Key, out int rank) && rank == pair.Value), "ranks retained");
        }

        public static string RunModelChecks()
        {
            checks = 0;
            var random = UnityEngine.Random.state;
            var fixture = ScriptableObject.CreateInstance<LocationData>();
            try
            {
                var dishes = AssetDatabase.FindAssets("t:DishData", new[] { "Assets/Data/Dishes" })
                    .Select(g => AssetDatabase.LoadAssetAtPath<DishData>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
                Check(dishes.Length == 32 && dishes.All(d => d != null && d.DishId > 0) &&
                    dishes.Select(d => d.DishId).Distinct().Count() == 32, "32 positive unique authored dish IDs");
                var locations = AssetDatabase.LoadAssetAtPath<LocationCatalog>("Assets/Data/LocationCatalog.asset");
                Check(locations.Locations.Count == 11, "11 authored location keys");
                for (int level = 0; level <= 10; level++)
                    Check(locations.TryGetByDocumentedLevel(level, out var location) &&
                        locations.Locations.Count(x => x != null && x.LevelRequirement == level) == 1 &&
                        locations.Locations[level] == location, "unique ordered location key " + level);
                var catalog = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
                Check(catalog.IsValid, "stable unique upgrade IDs");
                var profile = new PlayerProfile();
                var s = new PrototypeOrderSession(profile);
                var other = new PrototypeOrderSession(profile);
                Check(s.ConfigureUpgrades(catalog) && other.ConfigureUpgrades(catalog), "catalog per session");
                Check(ReferenceEquals(s.Rewards, other.Rewards) && ReferenceEquals(profile.Rewards, s.Rewards), "one progress owner");
                var empty = profile.CaptureSnapshot();
                Check(empty.Wallet == 0 && empty.DishXpById.Count == 0 && empty.UpgradeRanksById.Count == 0, "new profile empty");
                Check(profile.GetUpgradeRank(null) == 0 && profile.GetUpgradeRank("unknown") == 0, "absent rank zero");
                var dish = dishes.Single(d => d.DishId == 1);
                var serialized = new SerializedObject(fixture);
                var pool = serialized.FindProperty("documentedAvailableDishes");
                pool.arraySize = 1; pool.GetArrayElementAtIndex(0).objectReferenceValue = dish;
                serialized.ApplyModifiedPropertiesWithoutUndo();
                Check(s.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var quote), "initial offer");
                s.StartDay(fixture, 2);
                other.StartDay(fixture, 2);
                Check(s.IsDayActive && !other.IsDayActive && other.Orders.Count == 0, "no concurrent day on same profile");
                Check(!other.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out _) && !s.TryPurchaseUpgrade(quote), "purchase blocked profile-wide");
                other.EndDay();
                Check(profile.HasActiveDay, "foreign cleanup cannot release active day");
                for (int n = 0; n < 4; n++)
                {
                    Check(s.TrySelect(0), "select exact order");
                    var order = s.ActiveOrder;
                    for (int i = 0; i < dish.ClickRequirement; i++) s.TryCookClick(order);
                    decimal wallet = profile.Rewards.Wallet;
                    Check(order.IsCompleted && !s.TryCookClick(order) && profile.Rewards.Wallet == wallet, "receipt only once");
                }
                var earned = profile.CaptureSnapshot();
                Check(earned.Wallet >= 20 && earned.DishXpById[dish.DishId] > 0, "real completion funded profile");
                Check(empty.Wallet == 0 && empty.DishXpById.Count == 0 && empty.UpgradeRanksById.Count == 0, "old snapshot detached");
                s.EndDay();
                Check(!profile.HasActiveDay && s.Orders.Count == 0 && s.LastResult == null, "day state cleared");
                SameValues(earned, profile.CaptureSnapshot());
                Check(other.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var foreignQuote), "other offer");
                Check(!other.TryPurchaseUpgrade(quote), "foreign quote owner rejected");
                Check(s.TryPurchaseUpgrade(quote), "original offer exact purchase");
                Check(!other.TryPurchaseUpgrade(foreignQuote) && !s.TryPurchaseUpgrade(quote), "stale same-rank offers rejected");
                Check(profile.GetUpgradeRank(UpgradeCatalog.ClickStrengthId) == 1 && other.ClickDamage == 2 &&
                    profile.Rewards.Wallet == earned.Wallet - 20m, "shared rank and exact debit");
                var purchased = profile.CaptureSnapshot();
                Check(earned.Wallet == purchased.Wallet + 20m && earned.UpgradeRanksById.Count == 0, "purchase cannot mutate snapshot");
                var copy = (IDictionary<int, int>)purchased.DishXpById;
                bool rejected = false;
                try { copy[dish.DishId] = 0; } catch (NotSupportedException) { rejected = true; }
                Check(rejected, "XP snapshot rejects external mutation");
                rejected = false;
                try { ((IDictionary<string, int>)purchased.UpgradeRanksById)[UpgradeCatalog.ClickStrengthId] = 15; }
                catch (NotSupportedException) { rejected = true; }
                Check(rejected, "rank snapshot rejects external mutation");
                var next = new PrototypeOrderSession(profile);
                Check(next.ConfigureUpgrades(catalog), "replacement session catalog");
                next.StartDay(fixture, 2);
                Check(next.ClickStrengthRank == 1 && next.ClickDamage == 2 && next.CompletedDishes == 0, "new session keeps profile, resets day");
                SameValues(purchased, next.Profile.CaptureSnapshot());
                Check(next.TrySelect(0), "new day selection");
                var pending = next.ActiveOrder;
                Check(!next.TryCookClick(pending) && pending.RemainingClicks == 3, "new day uses stored strength");
                next.EndDay();
                Check(!next.TryCookClick(pending) && next.ActiveOrder == null && !profile.HasActiveDay, "discarded order cannot reward");
                SameValues(purchased, profile.CaptureSnapshot());
                next.StartDay(null, 6);
                Check(!next.IsDayActive && next.SelectedLocation == null && next.BoardCapacity == 0 && !profile.HasActiveDay, "null start leaves no lease");
                var fresh = new PrototypeOrderSession();
                Check(fresh.Rewards.Wallet == 0 && fresh.ClickStrengthRank == 0 && fresh.Rewards.GetTotalXp(dish) == 0, "independent fresh run");
                bool nullRejected = false;
                try { _ = new PrototypeOrderSession(null); } catch (ArgumentNullException) { nullRejected = true; }
                Check(nullRejected, "null profile rejected");
                return "PASS " + checks + " profile/ID/snapshot/transaction/lifetime assertions. No disk save/load executed.";
            }
            finally { UnityEngine.Random.state = random; UnityEngine.Object.DestroyImmediate(fixture); }
        }

        public static string BeginSceneReloadCheck()
        {
            if (!EditorApplication.isPlaying) throw new Exception("Play Mode required.");
            checks = 0;
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            UiA2Verification.ShowBoard(0);
            Check(ReferenceEquals(c.OrderSession.Profile, PlayerProfileRuntime.Current), "controller uses run owner");
            Check(c.OrderSession.Rewards.Wallet > 0 && c.OrderSession.ClickStrengthRank > 0, "run regression first to earn/buy legitimately");
            c.SelectOrder(0);
            oldSession = c.OrderSession;
            oldOrder = oldSession.ActiveOrder;
            beforeReload = oldSession.Profile;
            snapshotBeforeReload = beforeReload.CaptureSnapshot();
            SceneManager.LoadScene("SampleScene");
            return "Reload requested in Play Mode; call FinishSceneReloadCheck after rendered frames.";
        }

        public static string FinishSceneReloadCheck()
        {
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            Check(c != null && c.enabled && c.OrderSession != oldSession, "scene created fresh controller/session");
            Check(ReferenceEquals(c.OrderSession.Profile, beforeReload) && ReferenceEquals(PlayerProfileRuntime.Current, beforeReload), "profile outlives scene");
            SameValues(snapshotBeforeReload, c.OrderSession.Profile.CaptureSnapshot());
            Check(!oldSession.IsDayActive && !oldSession.TryCookClick(oldOrder), "destroyed view releases old day and stale order");
            Check(!c.OrderSession.IsDayActive && c.OrderSession.Orders.Count == 0 && !beforeReload.HasActiveDay, "reload retains no unfinished orders");
            c.CookSelectedLocation();
            Check(c.OrderSession.IsDayActive && c.OrderSession.CompletedDishes == 0 && c.OrderSession.Orders.Count == 2, "fresh day after screen reload");
            Check(c.OrderSession.ClickStrengthRank == snapshotBeforeReload.UpgradeRanksById[UpgradeCatalog.ClickStrengthId], "damage rank retained after reload");
            c.FinishDay();
            SameValues(snapshotBeforeReload, c.OrderSession.Profile.CaptureSnapshot());
            return "PASS " + checks + " Play Mode scene-reload/profile retention checks; scene asset not modified. Callbacks, not physical input.";
        }
    }
}
