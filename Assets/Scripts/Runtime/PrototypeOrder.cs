using System;
using ClickCook.Data;

namespace ClickCook.Runtime
{
    /// <summary>A transient order instance; duplicate dishes still have independent identities.</summary>
    public sealed class PrototypeOrder
    {
        public string Identity { get; } = Guid.NewGuid().ToString("N");
        public DishData Dish { get; }
        public LocationData Location { get; }
        public int SlotIndex { get; }
        public bool IsReserved { get; internal set; }
        public bool IsCompleted { get; internal set; }
        public int RemainingClicks { get; internal set; }

        internal PrototypeOrder(DishData dish, LocationData location, int slotIndex)
        {
            Dish = dish;
            Location = location;
            SlotIndex = slotIndex;
        }
    }
}
