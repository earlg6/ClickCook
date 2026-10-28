using System.Collections.Generic;
using ClickCook.Data;

namespace ClickCook.Runtime
{
    /// <summary>A read-only report of already-paid receipts. Reading it never awards progress.</summary>
    public sealed class PrototypeDaySummary
    {
        public LocationData Location { get; }
        public IReadOnlyList<PrototypeDishResult> Receipts { get; }
        public decimal Earnings { get; }
        public decimal DishXp { get; }
        public bool LimitReached => Receipts.Count == PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY;

        internal PrototypeDaySummary(LocationData location, List<PrototypeDishResult> receipts)
        {
            Location = location;
            Receipts = new List<PrototypeDishResult>(receipts).AsReadOnly();
            foreach (var receipt in Receipts)
            {
                Earnings += receipt.Payout;
                DishXp += receipt.XpGained;
            }
        }
    }
}
