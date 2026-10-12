using System.Collections.Generic;
using ClickCook.Data;
using UnityEngine;

namespace ClickCook.Runtime
{
    /// <summary>Immutable offer bound to one session and one owned rank.</summary>
    public sealed class UpgradePurchaseQuote
    {
        internal PrototypeOrderSession Owner { get; }
        public int OwnedRank { get; }
        public decimal Price { get; }
        internal UpgradePurchaseQuote(PrototypeOrderSession owner, int rank, decimal price)
        {
            Owner = owner;
            OwnedRank = rank;
            Price = price;
        }
    }

    /// <summary>In-memory prototype orders, cooking and daily lifecycle with run-local rewards.</summary>
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
        public PrototypeDishRewards Rewards { get; } = new();
        public PrototypeDishResult LastResult { get; private set; }
        private UpgradeCatalog upgradeCatalog;
        public int ClickStrengthRank { get; private set; }
        public int ClickDamage => UpgradeCatalog.ClickDamageAtRank(ClickStrengthRank);

        public bool ConfigureUpgrades(UpgradeCatalog catalog)
        {
            if (IsDayActive || upgradeCatalog != null || catalog == null || !catalog.IsValid) return false;
            upgradeCatalog = catalog;
            return true;
        }

        public bool TryQuoteUpgrade(string id, out UpgradePurchaseQuote quote)
        {
            quote = null;
            // Other catalog entries remain unavailable until their effects are implemented.
            if (IsDayActive || id != UpgradeCatalog.ClickStrengthId || upgradeCatalog == null || !upgradeCatalog.IsValid) return false;
            var definition = upgradeCatalog.Find(id);
            if (ClickStrengthRank >= definition.MaximumRank) return false;
            quote = new UpgradePurchaseQuote(this, ClickStrengthRank, definition.PriceForNextRank(ClickStrengthRank));
            return true;
        }

        public bool TryPurchaseUpgrade(UpgradePurchaseQuote quote)
        {
            if (quote == null || quote.Owner != this || quote.OwnedRank != ClickStrengthRank ||
                !TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var current) ||
                quote.Price != current.Price || !Rewards.TrySpend(current.Price)) return false;
            ClickStrengthRank++;
            return true;
        }

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

            expectedOrder.RemainingClicks = Mathf.Max(0, expectedOrder.RemainingClicks - ClickDamage);
            if (expectedOrder.RemainingClicks > 0) return false;

            expectedOrder.IsCompleted = true;
            expectedOrder.IsReserved = false;
            ActiveOrder = null;
            int index = orders.IndexOf(expectedOrder);
            orders.RemoveAt(index);
            CompletedDishes++;
            LastResult = Rewards.Complete(expectedOrder.Dish);
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
            LastResult = null;
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
