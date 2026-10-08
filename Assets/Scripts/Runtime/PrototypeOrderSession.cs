using System.Collections.Generic;
using ClickCook.Data;
using UnityEngine;

namespace ClickCook.Runtime
{
    /// <summary>Initial board population and selection only. No completion or replacement logic.</summary>
    public sealed class PrototypeOrderSession
    {
        public const int MaximumBoardCapacity = 6;
        public const int MAX_COMPLETED_DISHES_PER_DAY = 10;

        private readonly List<PrototypeOrder> orders = new();
        private readonly IReadOnlyList<PrototypeOrder> readOnlyOrders;

        public LocationData SelectedLocation { get; private set; }
        public int BoardCapacity { get; private set; }
        public int CompletedDishes => 0; // Placeholder until Milestone 4B.
        public IReadOnlyList<PrototypeOrder> Orders => readOnlyOrders;
        public PrototypeOrder ActiveOrder { get; private set; }

        public PrototypeOrderSession()
        {
            readOnlyOrders = orders.AsReadOnly();
        }

        public void StartDay(LocationData location, int capacity)
        {
            ReturnToBoard();
            orders.Clear();
            SelectedLocation = location;
            BoardCapacity = Mathf.Clamp(capacity, 0, MaximumBoardCapacity);
            if (location == null) return;

            var candidates = new List<DishData>();
            foreach (DishData dish in location.DocumentedAvailableDishes)
            {
                if (dish != null) candidates.Add(dish);
            }

            if (candidates.Count == 0) return;
            for (int slot = 0; slot < BoardCapacity; slot++)
            {
                DishData dish = candidates[Random.Range(0, candidates.Count)];
                orders.Add(new PrototypeOrder(dish, location, slot));
            }
        }

        public bool TrySelect(int slotIndex)
        {
            if (ActiveOrder != null || slotIndex < 0 || slotIndex >= orders.Count) return false;
            PrototypeOrder order = orders[slotIndex];
            if (order.Dish == null || order.Location == null || order.IsReserved) return false;
            order.IsReserved = true;
            ActiveOrder = order;
            return true;
        }

        public void ReturnToBoard()
        {
            if (ActiveOrder != null) ActiveOrder.IsReserved = false;
            ActiveOrder = null;
        }
    }
}
