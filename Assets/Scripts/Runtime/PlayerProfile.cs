using System;
using System.Collections.Generic;
using ClickCook.Data;

namespace ClickCook.Runtime
{
    /// <summary>One run's progress, independent of screens and temporary order sessions. No disk I/O.</summary>
    public sealed class PlayerProfile
    {
        private readonly Dictionary<string, int> upgradeRanks = new(StringComparer.Ordinal);
        private PrototypeOrderSession activeDay;
        private readonly HashSet<int> ownedRecipes = new() { 1 };
        public bool OwnsRecipe(int id) => ownedRecipes.Contains(id);
        public decimal PlayerXp { get; private set; }
        public int PlayerLevel => PlayerProgression.LevelAt(PlayerXp);
        public bool CanWorkAt(LocationData location) => PlayerLevel >= PlayerProgression.RequiredLevel(location);
        internal void RestorePlayerXp(decimal value) => PlayerXp = value;

        internal decimal CreditCompletedDay(decimal earnings)
        {
            Rewards.CreditCompletedDay(earnings);
            decimal gained = Math.Min(earnings / 100m, PlayerProgression.MaximumXp - PlayerXp);
            PlayerXp += gained;
            return gained;
        }

        public PrototypeDishRewards Rewards { get; } = new();
        public bool HasActiveDay => activeDay != null;
        public event Action Changed;

        internal void NotifyChanged() => Changed?.Invoke();

        internal static PlayerProfile Restore(decimal wallet, Dictionary<int, int> xp, Dictionary<string, int> ranks, Dictionary<int, int> hundredths = null)
        {
            var profile = new PlayerProfile();
            profile.Rewards.Restore(wallet, xp, hundredths);
            foreach (var pair in ranks) profile.upgradeRanks.Add(pair.Key, pair.Value);
            return profile;
        }

        public int GetUpgradeRank(string id) =>
            id != null && upgradeRanks.TryGetValue(UpgradeNodeRules.CanonicalId(id), out int rank) ? rank : 0;

        public PlayerProfileSnapshot CaptureSnapshot() =>
            new(Rewards.Wallet, Rewards.CopyDishXp(), upgradeRanks, Rewards.CopyDishXpHundredths(), ownedRecipes, PlayerXp);

        internal void RestoreRecipes(IEnumerable<int> recipes)
        {
            ownedRecipes.Clear();
            foreach (int id in recipes) ownedRecipes.Add(id);
        }

        internal bool TryPurchaseRecipe(int id, decimal price)
        {
            if (HasActiveDay || id < 2 || id > 32 || ownedRecipes.Contains(id) || price <= 0 || !Rewards.TrySpend(price)) return false;
            ownedRecipes.Add(id);
            NotifyChanged();
            return true;
        }

        internal bool TryBeginDay(PrototypeOrderSession session)
        {
            if (activeDay != null) return false;
            activeDay = session;
            return true;
        }

        internal void ReleaseDay(PrototypeOrderSession session)
        {
            if (activeDay == session) activeDay = null;
        }

        internal bool TryPurchaseClickStrength(int expectedRank, decimal price)
            => TryPurchaseNode(UpgradeNodeRules.Strength, expectedRank, price);

        internal bool TryPurchaseNode(string nodeId, int expectedRank, decimal price)
        {
            nodeId = UpgradeNodeRules.CanonicalId(nodeId);
            if (HasActiveDay || UpgradeNodeRules.MaximumRank(nodeId) == 0 || expectedRank < 0 ||
                expectedRank >= UpgradeNodeRules.MaximumRank(nodeId) || !UpgradeNodeRules.PrerequisitesMet(nodeId, GetUpgradeRank) ||
                GetUpgradeRank(nodeId) != expectedRank || price <= 0 ||
                !Rewards.TrySpend(price)) return false;
            upgradeRanks[nodeId] = expectedRank + 1;
            NotifyChanged();
            return true;
        }
    }
}
