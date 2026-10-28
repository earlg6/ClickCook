using System.Collections.Generic;
using UnityEngine;

namespace ClickCook.Data
{
    [CreateAssetMenu(fileName = "RecipeCatalog", menuName = "ClickCook/Data/Recipes")]
    public sealed class RecipeCatalog : ScriptableObject
    {
        [SerializeField] private List<DishData> dishes = new();
        public IReadOnlyList<DishData> Dishes => dishes;
        public DishData Find(int id) => dishes.Find(d => d != null && d.DishId == id);
    }
}
