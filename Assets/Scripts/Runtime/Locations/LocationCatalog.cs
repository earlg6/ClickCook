using System.Collections.Generic;
using ClickCook.Data;
using UnityEngine;

namespace ClickCook.Runtime.Locations
{
    /// <summary>
    /// Read-only catalog of the documented location definitions, ordered by their documented level.
    /// This asset does not implement location unlocks, ownership, recipes, or gameplay state.
    /// </summary>
    [CreateAssetMenu(fileName = "LocationCatalog", menuName = "ClickCook/Runtime/Location Catalog")]
    public sealed class LocationCatalog : ScriptableObject
    {
        [SerializeField] private List<LocationData> locations = new();

        public IReadOnlyList<LocationData> Locations => locations;

        public bool TryGetByDocumentedLevel(int level, out LocationData location)
        {
            foreach (LocationData candidate in locations)
            {
                if (candidate != null && candidate.LevelRequirement == level)
                {
                    location = candidate;
                    return true;
                }
            }

            location = null;
            return false;
        }
    }
}
