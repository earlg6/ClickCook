using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ClickCook.Runtime
{
    /// <summary>Detached read-only profile values. PlayerProfileStore owns the versioned disk format.</summary>
    public sealed class PlayerProfileSnapshot
    {
        public decimal Wallet { get; }
        public decimal PlayerXp { get; }
        public IReadOnlyDictionary<int, int> DishXpById { get; }
        public IReadOnlyDictionary<int, int> DishXpHundredthsById { get; }
        public IReadOnlyDictionary<string, int> UpgradeRanksById { get; }
        public IReadOnlyList<int> OwnedRecipeIds { get; }

        internal PlayerProfileSnapshot(decimal wallet, IDictionary<int, int> dishXp,
            IDictionary<string, int> upgradeRanks, IDictionary<int, int> hundredths = null, IEnumerable<int> recipes = null, decimal playerXp = 0m)
        {
            Wallet = wallet;
            PlayerXp = playerXp;
            OwnedRecipeIds = new List<int>(recipes ?? new[] { 1 }).AsReadOnly();
            DishXpHundredthsById = new ReadOnlyDictionary<int, int>(hundredths != null ? new Dictionary<int, int>(hundredths) : new Dictionary<int, int>());
            DishXpById = new ReadOnlyDictionary<int, int>(new Dictionary<int, int>(dishXp));
            UpgradeRanksById = new ReadOnlyDictionary<string, int>(
                new Dictionary<string, int>(upgradeRanks, System.StringComparer.Ordinal));
        }
    }
}
