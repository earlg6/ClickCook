using System.Collections.Generic;
using UnityEngine;

namespace ClickCook.Data
{
    /// <summary>
    /// Static design-time definition of a cookable dish.
    /// Runtime recipe ownership, click progress, rarity, and XP are intentionally out of scope.
    /// </summary>
    [CreateAssetMenu(fileName = "DishData", menuName = "ClickCook/Data/Dish")]
    public sealed class DishData : ScriptableObject
    {
        [SerializeField] private int dishId;
        [SerializeField] private string englishName;
        [SerializeField] private string russianName;
        [SerializeField] private int saleValue;
        [SerializeField] private int recipeCost;
        [SerializeField] private int clickRequirement;
        [SerializeField] private List<LocationData> documentedAvailableLocations = new();

        public int DishId => dishId;
        public string EnglishName => englishName;
        public string RussianName => russianName;
        public int SaleValue => saleValue;
        public int RecipeCost => recipeCost;
        public int ClickRequirement => clickRequirement;
        public IReadOnlyList<LocationData> DocumentedAvailableLocations => documentedAvailableLocations;
    }
}
