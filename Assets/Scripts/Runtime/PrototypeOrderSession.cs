using System.Collections.Generic;
using ClickCook.Data;
using UnityEngine;

namespace ClickCook.Runtime
{
    public sealed class RecipePurchaseQuote
    {
        internal PrototypeOrderSession Owner { get; }
        public DishData Dish { get; }
        public decimal Price { get; }
        public int DishId { get; }
        internal RecipePurchaseQuote(PrototypeOrderSession owner, DishData dish)
        { Owner = owner; Dish = dish; DishId = dish.DishId; Price = dish.RecipeCost; }
    }
    /// <summary>Immutable offer bound to one session and one owned rank.</summary>
    public sealed class UpgradePurchaseQuote
    {
        internal PrototypeOrderSession Owner { get; }
        public int OwnedRank { get; }
        public decimal Price { get; }
        public string NodeId { get; }
        public int DiscountRank { get; }
        internal UpgradePurchaseQuote(PrototypeOrderSession owner, string nodeId, int rank, decimal price, int discountRank)
        {
            Owner = owner;
            NodeId = nodeId;
            OwnedRank = rank;
            Price = price;
            DiscountRank = discountRank;
        }
    }

    /// <summary>In-memory prototype orders, cooking and daily lifecycle with run-local rewards.</summary>
    public sealed class PrototypeOrderSession
    {
        public const int MaximumBoardCapacity = 6;
        public const int MAX_COMPLETED_DISHES_PER_DAY = 10;

        private readonly List<PrototypeOrder> orders = new();
        private readonly List<PrototypeDishResult> dayReceipts = new();
        private readonly IReadOnlyList<PrototypeOrder> readOnlyOrders;

        public LocationData SelectedLocation { get; private set; }
        public int BoardCapacity { get; private set; }
        public int CompletedDishes { get; private set; }
        public bool IsDayActive { get; private set; }
        private bool isPaused;
        public bool IsPaused
        {
            get => isPaused;
            set { isPaused = value; if (value) { CancelHold(); SuspendAuto(); } }
        }
        public IReadOnlyList<PrototypeOrder> Orders => readOnlyOrders;
        public PrototypeOrder ActiveOrder { get; private set; }
        public PlayerProfile Profile { get; }
        public PrototypeDishRewards Rewards => Profile.Rewards;
        public PrototypeDishResult LastResult { get; private set; }
        private UpgradeCatalog upgradeCatalog;
        private RecipeCatalog recipeCatalog;
        public void ConfigureRecipes(RecipeCatalog catalog) { if (!IsDayActive) recipeCatalog = catalog; }
        public bool TryQuoteRecipe(int id, out RecipePurchaseQuote quote)
        {
            quote = null;
            var dish = recipeCatalog != null ? recipeCatalog.Find(id) : null;
            if (IsPaused || Profile.HasActiveDay || dish == null || id < 2 || id > 32 || dish.RecipeCost <= 0 || Profile.OwnsRecipe(id)) return false;
            quote = new RecipePurchaseQuote(this, dish);
            return true;
        }
        public bool TryPurchaseRecipe(RecipePurchaseQuote quote)
        {
            if (quote == null || quote.Dish == null || quote.Owner != this || IsPaused || Profile.HasActiveDay || recipeCatalog == null ||
                recipeCatalog.Find(quote.DishId) != quote.Dish || quote.Dish.DishId != quote.DishId || quote.Dish.RecipeCost != quote.Price) return false;
            return Profile.TryPurchaseRecipe(quote.DishId, quote.Price);
        }
        public int AvailableRecipeCount(LocationData location) => GetValidCandidates(location).Count;
        public int ClickStrengthRank => Profile.GetUpgradeRank(UpgradeCatalog.ClickStrengthId);
        public decimal ClickMultiplier => UpgradeNodeRules.AdditionalFactor("click_multiplier", Profile.GetUpgradeRank);
        public int ClickDamage => (int)decimal.Ceiling((UpgradeCatalog.ClickDamageAtRank(ClickStrengthRank) + UpgradeNodeRules.AdditionalContribution("click_strength", Profile.GetUpgradeRank)) * ClickMultiplier);
        public int DishXpFlatRank => Profile.GetUpgradeRank(UpgradeNodeRules.DishXpFlat);
        public int DishXpBonus => UpgradeNodeRules.DishXpBonusAtRank(DishXpFlatRank) + (int)UpgradeNodeRules.AdditionalContribution("dish_xp_flat", Profile.GetUpgradeRank);
        public int DishXpMultiplierRank => Profile.GetUpgradeRank(UpgradeNodeRules.DishXpMultiplier);
        public decimal DishXpMultiplier => UpgradeNodeRules.DishXpMultiplierAtRank(DishXpMultiplierRank);
        public int DishValueRank => Profile.GetUpgradeRank(UpgradeNodeRules.DishValue);
        public decimal DishValueMultiplier => UpgradeNodeRules.DishValueAtRank(DishValueRank) + UpgradeNodeRules.AdditionalContribution("dish_value", Profile.GetUpgradeRank);
        public int UpgradeDiscountRank => Profile.GetUpgradeRank(UpgradeNodeRules.UpgradeDiscount);
        public decimal UpgradeDiscount => .01m * UpgradeDiscountRank;
        public decimal UpgradePriceForRank(string id, int rank) => System.Math.Max(1m, decimal.Ceiling(upgradeCatalog.FindForNode(id).PriceForNextRank(rank) * (1m - UpgradeDiscount)));
        public int CriticalChanceRank => Profile.GetUpgradeRank(UpgradeNodeRules.CriticalChance);
        public int CriticalMultiplierRank => Profile.GetUpgradeRank(UpgradeNodeRules.CriticalMultiplier);
        public double CriticalChance => (double)(CriticalChanceRank / 50m + UpgradeNodeRules.AdditionalContribution("critical_chance", Profile.GetUpgradeRank));
        public decimal CriticalMultiplier => UpgradeNodeRules.MultiplierAtRank(CriticalMultiplierRank) + UpgradeNodeRules.AdditionalContribution("critical_multiplier", Profile.GetUpgradeRank);
        public bool LastHitWasCritical { get; private set; }
        public int LastHitDamage { get; private set; }
        public int LastStrikeCount { get; private set; }
        public int LastExtraStrikeDamage { get; private set; }
        public int DoubleStrikeChanceRank => Profile.GetUpgradeRank(UpgradeNodeRules.DoubleStrikeChance);
        public int DoubleStrikePowerRank => Profile.GetUpgradeRank(UpgradeNodeRules.DoubleStrikePower);
        public int DoubleStrikeChanceIIRank => Profile.GetUpgradeRank(UpgradeNodeRules.DoubleStrikeChanceII);
        public int TripleStrikeChanceRank => Profile.GetUpgradeRank(UpgradeNodeRules.TripleStrikeChance);
        public double DoubleStrikeChance => (double)((DoubleStrikeChanceRank + DoubleStrikeChanceIIRank) / 50m + UpgradeNodeRules.AdditionalContribution("double_strike_chance", Profile.GetUpgradeRank));
        public double TripleStrikeChance => (double)(TripleStrikeChanceRank / 100m + UpgradeNodeRules.AdditionalContribution("triple_strike_chance", Profile.GetUpgradeRank));
        public double AdditionalStrikeChance => (double)((decimal)DoubleStrikeChance + (decimal)TripleStrikeChance);
        public decimal DoubleStrikePower => UpgradeNodeRules.DoubleStrikePowerAtRank(DoubleStrikePowerRank) + UpgradeNodeRules.AdditionalContribution("double_strike_power", Profile.GetUpgradeRank);
        private readonly System.Func<double> criticalRoll;
        private readonly System.Func<double> doubleStrikeRoll;
        private readonly System.Func<double> autoStartRoll;
        private PrototypeOrder heldOrder;
        private double nextHoldTime;
        private PrototypeOrder autoOrder;
        private double nextAutoTime;
        public int AutoActionCountRank => Profile.GetUpgradeRank(UpgradeNodeRules.AutoActionCount);
        public int AutoActionChanceRank => Profile.GetUpgradeRank(UpgradeNodeRules.AutoActionChance);
        public double AutoActionChance => (double)(AutoActionChanceRank / 50m + UpgradeNodeRules.AdditionalContribution("auto_action_chance", Profile.GetUpgradeRank));
        public bool HasAuto => AutoActionCountRank > 0;
        public int AutoActionsPerTick => 1 + AutoActionCountRank + (int)UpgradeNodeRules.AdditionalContribution("auto_action_count", Profile.GetUpgradeRank);
        public int AutoStrengthRank => Profile.GetUpgradeRank(UpgradeNodeRules.AutoStrength);
        public int AutoStrengthMultiplierRank => Profile.GetUpgradeRank(UpgradeNodeRules.AutoStrengthMultiplier);
        public int AutoBaseDamage => 1 + AutoStrengthRank + (int)UpgradeNodeRules.AdditionalContribution("auto_strength", Profile.GetUpgradeRank);
        public decimal AutoStrengthMultiplier => UpgradeNodeRules.AutoMultiplierAtRank(AutoStrengthMultiplierRank) * UpgradeNodeRules.AdditionalFactor("auto_strength_multiplier", Profile.GetUpgradeRank);
        public int AutoDamagePerAction => (int)decimal.Ceiling(AutoBaseDamage * AutoStrengthMultiplier);
        public int AutoFrequencyRank => Profile.GetUpgradeRank(UpgradeNodeRules.AutoFrequency);
        public decimal AutoFrequency => HasAuto ? UpgradeNodeRules.AutoFrequencyAtRank(AutoFrequencyRank) + UpgradeNodeRules.AdditionalContribution("auto_frequency", Profile.GetUpgradeRank) : 0m;
        public double AutoInterval => HasAuto ? 1d / (double)AutoFrequency : 1d;
        public bool LastActionWasAuto { get; private set; }
        public int LastAutoActionCount { get; private set; }
        public bool HasHold => Profile.GetUpgradeRank(UpgradeNodeRules.Hold) == 1;
        public int HoldFrequencyRank => Profile.GetUpgradeRank(UpgradeNodeRules.HoldFrequency);
        public decimal HoldFrequency => HasHold ? UpgradeNodeRules.HoldFrequencyAtRank(HoldFrequencyRank) + UpgradeNodeRules.AdditionalContribution("hold_frequency", Profile.GetUpgradeRank) : 0m;
        public double HoldRepeatInterval => HasHold ? 1d / (double)HoldFrequency : UpgradeNodeRules.HoldInterval;
        public bool IsHolding => heldOrder != null;

        public bool TryBeginHold(PrototypeOrder order, double now)
        {
            if (!HasHold || IsHolding || IsPaused || !IsDayActive || order == null || order != ActiveOrder ||
                order.IsCompleted || order.RemainingClicks <= 0 || double.IsNaN(now) || double.IsInfinity(now)) return false;
            heldOrder = order; nextHoldTime = now + HoldRepeatInterval;
            return true;
        }

        /// <summary>At most one repeat per frame; a delayed frame never accrues a burst of overdue hits.</summary>
        public bool TryTakeHoldRepeat(double now)
        {
            if (!IsHolding) return false;
            if (IsPaused || !IsDayActive || heldOrder != ActiveOrder || heldOrder.IsCompleted || !HasHold ||
                double.IsNaN(now) || double.IsInfinity(now)) { CancelHold(); return false; }
            if (now < nextHoldTime) return false;
            nextHoldTime = now + HoldRepeatInterval;
            return true;
        }
        public void CancelHold() { heldOrder = null; nextHoldTime = 0; }
        public void SuspendAuto() { autoOrder = null; nextAutoTime = 0; }

        /// <summary>Bind a fresh full interval after selection/resume. Never catch up missed ticks.</summary>
        public bool TryCookAuto(PrototypeOrder expectedOrder, double now)
        {
            if (!HasAuto || IsPaused || !IsDayActive || expectedOrder == null || ActiveOrder != expectedOrder ||
                expectedOrder.IsCompleted || expectedOrder.Dish == null || expectedOrder.RemainingClicks <= 0 ||
                CompletedDishes >= MAX_COMPLETED_DISHES_PER_DAY || double.IsNaN(now) || double.IsInfinity(now))
            { SuspendAuto(); return false; }
            if (autoOrder != expectedOrder) { autoOrder = expectedOrder; nextAutoTime = now + AutoInterval; return false; }
            if (now < nextAutoTime) return false;
            nextAutoTime = now + AutoInterval;
            return ApplyAutoBatch(expectedOrder);
        }

        /// <summary>First cooking entry only, including a miss/zero chance. Back never grants another roll.</summary>
        public bool TryCookAutoStart(PrototypeOrder expectedOrder)
        {
            if (IsPaused || !IsDayActive || expectedOrder == null || ActiveOrder != expectedOrder ||
                expectedOrder.IsCompleted || expectedOrder.Dish == null || expectedOrder.RemainingClicks <= 0 ||
                CompletedDishes >= MAX_COMPLETED_DISHES_PER_DAY || expectedOrder.AutoStartChecked) return false;
            expectedOrder.AutoStartChecked = true;
            return RollSucceeds(AutoActionChance, autoStartRoll) && ApplyAutoBatch(expectedOrder);
        }

        private bool ApplyAutoBatch(PrototypeOrder expectedOrder)
        {
            ResetHitFeedback(); LastActionWasAuto = true;
            int actions = AutoActionsPerTick, damage = AutoDamagePerAction;
            for (int i = 0; i < actions && expectedOrder.RemainingClicks > 0; i++)
            {
                expectedOrder.RemainingClicks = Mathf.Max(0, expectedOrder.RemainingClicks - damage);
                LastAutoActionCount++; LastHitDamage += damage;
            }
            return expectedOrder.RemainingClicks == 0 && CompleteOrder(expectedOrder);
        }

        public bool ConfigureUpgrades(UpgradeCatalog catalog)
        {
            if (IsDayActive || upgradeCatalog != null || catalog == null || !catalog.IsValid) return false;
            upgradeCatalog = catalog;
            return true;
        }

        public bool TryQuoteUpgrade(string id, out UpgradePurchaseQuote quote)
        {
            quote = null;
            id = UpgradeNodeRules.CanonicalId(id);
            if (IsPaused || Profile.HasActiveDay || UpgradeNodeRules.MaximumRank(id) == 0 || upgradeCatalog == null || !upgradeCatalog.IsValid ||
                !UpgradeNodeRules.PrerequisitesMet(id, Profile.GetUpgradeRank)) return false;
            var definition = upgradeCatalog.FindForNode(id);
            int rank = Profile.GetUpgradeRank(id);
            if (definition == null || definition.MaximumRank != UpgradeNodeRules.MaximumRank(id) || rank >= definition.MaximumRank) return false;
            quote = new UpgradePurchaseQuote(this, id, rank, UpgradePriceForRank(id, rank), UpgradeDiscountRank);
            return true;
        }

        public bool TryPurchaseUpgrade(UpgradePurchaseQuote quote)
        {
            if (quote == null || quote.Owner != this || quote.DiscountRank != UpgradeDiscountRank || quote.OwnedRank != Profile.GetUpgradeRank(quote.NodeId) ||
                !TryQuoteUpgrade(quote.NodeId, out var current) ||
                quote.Price != current.Price) return false;
            return Profile.TryPurchaseNode(quote.NodeId, quote.OwnedRank, current.Price);
        }

        public PrototypeOrderSession() : this(new PlayerProfile()) { }

        public PrototypeOrderSession(PlayerProfile profile, System.Func<double> criticalRoll = null, System.Func<double> doubleStrikeRoll = null, System.Func<double> autoStartRoll = null)
        {
            Profile = profile ?? throw new System.ArgumentNullException(nameof(profile));
            this.criticalRoll = criticalRoll ?? (() => Random.Range(0, int.MaxValue) / (double)int.MaxValue);
            this.doubleStrikeRoll = doubleStrikeRoll ?? (() => Random.Range(0, int.MaxValue) / (double)int.MaxValue);
            this.autoStartRoll = autoStartRoll ?? (() => Random.Range(0, int.MaxValue) / (double)int.MaxValue);
            readOnlyOrders = orders.AsReadOnly();
        }

        public void StartDay(LocationData location, int capacity)
        {
            if (IsPaused) return;
            EndDay();
            if (location == null || !Profile.TryBeginDay(this)) return;
            SelectedLocation = location;
            BoardCapacity = Mathf.Clamp(capacity, 0, MaximumBoardCapacity);
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
            if (IsPaused || !IsDayActive || CompletedDishes >= MAX_COMPLETED_DISHES_PER_DAY || ActiveOrder != null) return false;
            PrototypeOrder order = GetOrderAtSlot(slotIndex);
            if (order == null || order.Dish == null || order.Location == null || order.IsReserved ||
                order.IsCompleted || order.Dish.ClickRequirement <= 0) return false;
            order.IsReserved = true;
            order.RemainingClicks = order.Dish.ClickRequirement;
            CancelHold();
            SuspendAuto();
            ResetHitFeedback();
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
            if (IsPaused || !IsDayActive || expectedOrder == null || ActiveOrder != expectedOrder ||
                expectedOrder.IsCompleted || expectedOrder.Dish == null || expectedOrder.RemainingClicks <= 0 ||
                CompletedDishes >= MAX_COMPLETED_DISHES_PER_DAY) return false;

            ResetHitFeedback();
            ApplyManualStrike(expectedOrder, 1m);
            // One bounded transaction, bound to this order. A lethal primary hit consumes no proc roll.
            if (expectedOrder.RemainingClicks > 0 && AdditionalStrikeChance > 0)
            {
                double roll = ReadCombatRoll(doubleStrikeRoll);
                int extras = roll < DoubleStrikeChance ? 1 : roll < AdditionalStrikeChance ? 2 : 0;
                for (int i = 0; i < extras && expectedOrder.RemainingClicks > 0; i++)
                    LastExtraStrikeDamage += ApplyManualStrike(expectedOrder, DoubleStrikePower);
            }
            if (expectedOrder.RemainingClicks > 0) return false;

            return CompleteOrder(expectedOrder);
        }

        private bool CompleteOrder(PrototypeOrder expectedOrder)
        {
            expectedOrder.IsCompleted = true;
            CancelHold();
            SuspendAuto();
            expectedOrder.IsReserved = false;
            ActiveOrder = null;
            int index = orders.IndexOf(expectedOrder);
            orders.RemoveAt(index);
            CompletedDishes++;
            LastResult = Rewards.Complete(expectedOrder.Dish, DishXpBonus, DishXpMultiplier, DishValueMultiplier);
            dayReceipts.Add(LastResult);
            Profile.NotifyChanged();
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

        private static bool RollSucceeds(double chance, System.Func<double> provider)
        {
            if (chance <= 0) return false;
            return ReadCombatRoll(provider) < chance;
        }

        private static double ReadCombatRoll(System.Func<double> provider)
        {
            double roll = provider();
            if (double.IsNaN(roll) || roll < 0 || roll >= 1) throw new System.InvalidOperationException("Combat roll must be in [0,1).");
            return roll;
        }

        private int ApplyManualStrike(PrototypeOrder order, decimal strengthFactor)
        {
            bool critical = RollSucceeds(CriticalChance, criticalRoll);
            // Multiply before the single ceiling, including the additional strike's strength.
            int damage = (int)decimal.Ceiling(ClickDamage * strengthFactor * (critical ? CriticalMultiplier : 1m));
            LastHitWasCritical |= critical;
            LastHitDamage += damage;
            LastStrikeCount++;
            order.RemainingClicks = Mathf.Max(0, order.RemainingClicks - damage);
            return damage;
        }

        private void ResetHitFeedback()
        { LastHitDamage = 0; LastHitWasCritical = false; LastStrikeCount = 0; LastExtraStrikeDamage = 0; LastActionWasAuto = false; LastAutoActionCount = 0; }

        public void ReturnToBoard()
        {
            CancelHold();
            SuspendAuto();
            if (IsPaused) return;
            if (ActiveOrder != null)
            {
                ActiveOrder.IsReserved = false;
                ActiveOrder.RemainingClicks = 0;
            }
            ActiveOrder = null;
            ResetHitFeedback();
        }

        public bool TryFinishDay(out PrototypeDaySummary summary)
        {
            summary = null;
            if (IsPaused || !IsDayActive || ActiveOrder != null) return false;
            summary = new PrototypeDaySummary(SelectedLocation, dayReceipts);
            EndDay();
            return true;
        }

        public void EndDay()
        {
            IsPaused = false;
            ReturnToBoard();
            orders.Clear();
            dayReceipts.Clear();
            SelectedLocation = null;
            BoardCapacity = 0;
            CompletedDishes = 0;
            IsDayActive = false;
            LastResult = null;
            ResetHitFeedback();
            Profile.ReleaseDay(this);
        }

        private List<DishData> GetValidCandidates() => GetValidCandidates(SelectedLocation);
        private List<DishData> GetValidCandidates(LocationData location)
        {
            var candidates = new List<DishData>();
            if (location == null) return candidates;
            var seen = new HashSet<int>();
            foreach (DishData dish in location.DocumentedAvailableDishes)
            {
                if (dish != null && dish.ClickRequirement > 0 && Profile.OwnsRecipe(dish.DishId) && seen.Add(dish.DishId)) candidates.Add(dish);
            }
            return candidates;
        }
    }
}
