using System.Collections.Generic;
using ClickCook.Data;

namespace ClickCook.Runtime
{
    /// <summary>Read-only report of paid or forfeited day receipts. Reading never awards progress.</summary>
    public sealed class PrototypeDaySummary
    {
        public LocationData Location { get; }
        public IReadOnlyList<PrototypeDishResult> Receipts { get; }
        public decimal Earnings { get; }
        public decimal ForfeitedEarnings { get; }
        public decimal DishXp { get; }
        public decimal PlayerXp { get; }
        public bool LimitReached => Receipts.Count == PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY;

        internal PrototypeDaySummary(LocationData location, List<PrototypeDishResult> receipts, decimal playerXp = 0m)
        {
            Location = location;
            PlayerXp = playerXp;
            Receipts = new List<PrototypeDishResult>(receipts).AsReadOnly();
            foreach (var receipt in Receipts)
            {
                if (LimitReached) Earnings += receipt.Payout;
                else ForfeitedEarnings += receipt.Payout;
                DishXp += receipt.XpGained;
            }
        }
    }
}
