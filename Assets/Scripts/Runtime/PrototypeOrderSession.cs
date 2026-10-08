using System.Collections.Generic;
using ClickCook.Data;
using UnityEngine;

namespace ClickCook.Runtime
{
    /// <summary>In-memory prototype orders, base cooking and daily-limit lifecycle; no rewards.</summary>
    public sealed class PrototypeOrderSession
    {
        public const int MaximumBoardCapacity = 6;
        public const int MAX_COMPLETED_DISHES_PER_DAY = 10;

        private readonly List<PrototypeOrder> orders = new();
        private readonly IReadOnlyList<PrototypeOrder> readOnlyOrders;

        public LocationData SelectedLocation { get; private set; }
        public int BoardCapacity { get; private set; }
        public int CompletedDishes { get; private set; }
        public bool IsDayActive { get; private set; }
        public IReadOnlyList<PrototypeOrder> Orders => readOnlyOrders;
        public PrototypeOrder ActiveOrder { get; private set; }

        public PrototypeOrderSession()
        {
            readOnlyOrders = orders.AsReadOnly();
        }

        public void StartDay(LocationData location, int capacity)
        {
            EndDay();
            SelectedLocation = location;
            BoardCapacity = Mathf.Clamp(capacity, 0, MaximumBoardCapacity);
            if (location == null) return;
            IsDayActive = true;

            var candidates = GetValidCandidates();

            if (candidates.Count == 0) return;
            for (int slot = 0; slot < BoardCapacity; slot++)
            {
                DishData dish = candidates[Random.Range(0, candidates.Count)];
                orders.Add(new PrototypeOrder(dish, location, slot));
            }
        }

        public bool TrySelect(int slotIndex)
        {
            if (!IsDayActive || CompletedDishes >= MAX_COMPLETED_DISHES_PER_DAY || ActiveOrder != null) return false;
            PrototypeOrder order = GetOrderAtSlot(slotIndex);
            if (order == null || order.Dish == null || order.Location == null || order.IsReserved ||
                order.IsCompleted || order.Dish.ClickRequirement <= 0) return false;
            order.IsReserved = true;
            order.RemainingClicks = order.Dish.ClickRequirement;
            ActiveOrder = order;
            return true;
        }

        public PrototypeOrder GetOrderAtSlot(int slotIndex)
        {
            return orders.Find(order => order.SlotIndex == slotIndex);
        }

        /// <summary>Returns true only when this click completes the exact active order.</summary>
        public bool TryCookClick(PrototypeOrder expectedOrder)
        {
            if (!IsDayActive || expectedOrder == null || ActiveOrder != expectedOrder ||
                expectedOrder.IsCompleted || expectedOrder.Dish == null || expectedOrder.RemainingClicks <= 0 ||
                CompletedDishes >= MAX_COMPLETED_DISHES_PER_DAY) return false;

            expectedOrder.RemainingClicks--;
            if (expectedOrder.RemainingClicks > 0) return false;

            expectedOrder.IsCompleted = true;
            expectedOrder.IsReserved = false;
            ActiveOrder = null;
            int index = orders.IndexOf(expectedOrder);
            orders.RemoveAt(index);
            CompletedDishes++;
            if (CompletedDishes < MAX_COMPLETED_DISHES_PER_DAY)
            {
                var candidates = GetValidCandidates();
                if (candidates.Count > 0)
                {
                    orders.Insert(index, new PrototypeOrder(candidates[Random.Range(0, candidates.Count)],
                        SelectedLocation, expectedOrder.SlotIndex));
                }
            }
            return true;
        }

        public void ReturnToBoard()
        {
            if (ActiveOrder != null)
            {
                ActiveOrder.IsReserved = false;
                ActiveOrder.RemainingClicks = 0;
            }
            ActiveOrder = null;
        }

        public void EndDay()
        {
            ReturnToBoard();
            orders.Clear();
            SelectedLocation = null;
            BoardCapacity = 0;
            CompletedDishes = 0;
            IsDayActive = false;
        }

        private List<DishData> GetValidCandidates()
        {
            var candidates = new List<DishData>();
            if (SelectedLocation == null) return candidates;
            foreach (DishData dish in SelectedLocation.DocumentedAvailableDishes)
            {
                if (dish != null) candidates.Add(dish);
            }
            return candidates;
        }
    }
}
