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

        public PrototypeDishRewards Rewards { get; } = new();
        public bool HasActiveDay => activeDay != null;

        public int GetUpgradeRank(string id) =>
            id != null && upgradeRanks.TryGetValue(id, out int rank) ? rank : 0;

        public PlayerProfileSnapshot CaptureSnapshot() =>
            new(Rewards.Wallet, Rewards.CopyDishXp(), upgradeRanks);

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
        {
            if (HasActiveDay || GetUpgradeRank(UpgradeCatalog.ClickStrengthId) != expectedRank ||
                !Rewards.TrySpend(price)) return false;
            upgradeRanks[UpgradeCatalog.ClickStrengthId] = expectedRank + 1;
            return true;
        }
    }
}
