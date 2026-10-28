using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClickCook.Runtime
{
    /// <summary>Detached read-only profile values. PlayerProfileStore owns the versioned disk format.</summary>
    public sealed class PlayerProfileSnapshot
    {
        public decimal Wallet { get; }
        public IReadOnlyDictionary<int, int> DishXpById { get; }
        public IReadOnlyDictionary<int, int> DishXpHundredthsById { get; }
        public IReadOnlyDictionary<string, int> UpgradeRanksById { get; }

        internal PlayerProfileSnapshot(decimal wallet, IDictionary<int, int> dishXp,
            IDictionary<string, int> upgradeRanks, IDictionary<int, int> hundredths = null)
        {
            Wallet = wallet;
            DishXpHundredthsById = new ReadOnlyDictionary<int, int>(hundredths != null ? new Dictionary<int, int>(hundredths) : new Dictionary<int, int>());
            DishXpById = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>(dishXp));
            UpgradeRanksById = new ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>(upgradeRanks, System.StringComparer.Ordinal));
        }
    }
}
