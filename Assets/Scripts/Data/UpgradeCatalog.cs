using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClickCook.Data
{
    [Serializable]
    public sealed class UpgradeDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private int maximumRank;
        [SerializeField] private int basePrice;
        [SerializeField] private int priceGrowthPercent;

        public string Id => id;
        public string DisplayName => displayName;
        public int MaximumRank => maximumRank;
        public bool IsValid => !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(displayName) &&
            maximumRank > 0 && maximumRank <= 15 && basePrice > 0 && priceGrowthPercent >= 100 && priceGrowthPercent <= 300;

        public decimal PriceForNextRank(int ownedRank)
        {
            if (!IsValid || ownedRank < 0 || ownedRank >= maximumRank)
                throw new ArgumentOutOfRangeException(nameof(ownedRank));
            decimal price = basePrice;
            for (int i = 0; i < ownedRank; i++) price *= priceGrowthPercent / 100m;
            return decimal.Ceiling(price);
        }
    }

    /// <summary>Static prices only. A definition never enables an unfinished runtime effect.</summary>
    [CreateAssetMenu(fileName = "UpgradeCatalog", menuName = "ClickCook/Data/Upgrade Catalog")]
    public sealed class UpgradeCatalog : ScriptableObject
    {
        public const string ClickStrengthId = "click_strength";
        [SerializeField] private List<UpgradeDefinition> upgrades = new();
        public IReadOnlyList<UpgradeDefinition> Upgrades => upgrades.AsReadOnly();

        public bool IsValid
        {
            get
            {
                if (upgrades == null) return false;
                var ids = new HashSet<string>();
                foreach (var item in upgrades)
                    if (item == null || !item.IsValid || !ids.Add(item.Id)) return false;
                return ids.Contains(ClickStrengthId);
            }
        }

        public UpgradeDefinition Find(string id) => upgrades?.Find(item => item != null && item.Id == id);

        public static int ClickDamageAtRank(int rank)
        {
            if (rank < 0 || rank > 15) throw new ArgumentOutOfRangeException(nameof(rank));
            decimal damage = 1m;
            for (int i = 0; i < rank; i++) damage *= 1.75m;
            return (int)decimal.Ceiling(damage);
        }
    }
}
