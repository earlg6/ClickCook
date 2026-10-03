using System.Collections.Generic;
using UnityEngine;

namespace ClickCook.Data
{
    /// <summary>
    /// Static design-time definition of a selectable location.
    /// Runtime unlock state, income collection, and order generation are intentionally out of scope.
    /// </summary>
    [CreateAssetMenu(fileName = "LocationData", menuName = "ClickCook/Data/Location")]
    public sealed class LocationData : ScriptableObject
    {
        [SerializeField] private int levelRequirement;
        [SerializeField] private string englishName;
        [SerializeField] private string russianName;
        [SerializeField] private int passiveIncomePerSecond;
        [SerializeField] private List<DishData> documentedAvailableDishes = new();

        public int LevelRequirement => levelRequirement;
        public string EnglishName => englishName;
        public string RussianName => russianName;
        public int PassiveIncomePerSecond => passiveIncomePerSecond;
        public IReadOnlyList<DishData> DocumentedAvailableDishes => documentedAvailableDishes;
    }
}
