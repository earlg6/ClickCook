using System;
using System.Collections.Generic;
using ClickCook.Data;

namespace ClickCook.Runtime
{
    public enum DishRarity { Common, Uncommon, Rare, Fantastic, Legendary }

    /// <summary>Immutable receipt for one successful completion, not a pending transaction.</summary>
    public sealed class PrototypeDishResult
    {
        public DishData Dish { get; }
        public DishRarity Rarity { get; }
        public decimal Payout { get; }
        public decimal XpGained { get; }
        public int PreviousLevel { get; }
        public int Level { get; }

        internal PrototypeDishResult(DishData dish, DishRarity rarity, decimal payout,
            decimal xpGained, int previousLevel, int level)
        {
            Dish = dish;
            Rarity = rarity;
            Payout = payout;
            XpGained = xpGained;
            PreviousLevel = previousLevel;
            Level = level;
        }
    }

    /// <summary>Profile-owned run-local wallet and per-dish mastery. No persistence or player XP.</summary>
    public sealed class PrototypeDishRewards
    {
        public const int MaximumDishLevel = 18;
        private static readonly int[,] Probabilities =
        {
            {85,13,2,0,0}, {71,27,2,0,0}, {57,36,7,0,0}, {42,42,16,0,0},
            {28,44,25,3,0}, {14,42,35,9,0}, {0,38,44,17,1}, {0,26,46,24,4},
            {0,14,45,32,9}, {0,2,42,39,17}, {0,2,33,41,24}, {0,2,33,41,24},
            {0,2,14,40,44}, {0,2,14,40,44}, {0,2,5,30,63}, {0,2,5,30,63},
            {0,2,5,16,77}, {0,2,5,9,84}
        };
        private static readonly int[] XpAwards = {10,12,16,22,30};
        private static readonly decimal[] PayoutMultipliers = {1m,1.25m,1.75m,2.5m,4m};
        private readonly Dictionary<int, int> dishXp = new();
        private readonly Dictionary<int, int> dishXpHundredths = new();

        public decimal Wallet { get; private set; }

        internal Dictionary<int, int> CopyDishXp() => new(dishXp);
        internal Dictionary<int, int> CopyDishXpHundredths() => new(dishXpHundredths);

        internal void Restore(decimal wallet, Dictionary<int, int> xp, Dictionary<int, int> hundredths = null)
        {
            Wallet = wallet;
            dishXp.Clear();
            foreach (var pair in xp) dishXp.Add(pair.Key, pair.Value);
            dishXpHundredths.Clear();
            if (hundredths != null) foreach (var pair in hundredths) dishXpHundredths.Add(pair.Key, pair.Value);
        }

        internal bool TrySpend(decimal amount)
        {
            if (amount <= 0m || Wallet < amount) return false;
            Wallet -= amount;
            return true;
        }

        public int GetXpHundredths(DishData dish) => dish != null && dishXpHundredths.TryGetValue(dish.DishId, out int value) ? value : 0;
        public decimal GetExactTotalXp(DishData dish) => GetTotalXp(dish) + GetXpHundredths(dish) / 100m;
        public decimal GetExactLevelXp(DishData dish) => GetLevelXp(dish) + GetXpHundredths(dish) / 100m;

        public int GetTotalXp(DishData dish) => dish != null && dishXp.TryGetValue(dish.DishId, out int xp) ? xp : 0;

        public int GetLevel(DishData dish)
        {
            int xp = GetTotalXp(dish);
            int level = 1;
            while (level < MaximumDishLevel && xp >= RequiredXp(level))
            {
                xp -= RequiredXp(level);
                level++;
            }
            return level;
        }

        public int GetLevelXp(DishData dish)
        {
            int xp = GetTotalXp(dish);
            int level = GetLevel(dish);
            for (int previous = 1; previous < level; previous++) xp -= RequiredXp(previous);
            return xp;
        }

        public static int RequiredXp(int level)
        {
            if (level < 1 || level >= MaximumDishLevel) throw new ArgumentOutOfRangeException(nameof(level));
            return (int)Math.Round(20 * Math.Pow(1.18, level - 1), MidpointRounding.AwayFromZero);
        }

        /// <summary>Integer percent roll in [0,99]; zero-probability tiers cannot be selected.</summary>
        public static DishRarity ResolveRarity(int level, int roll)
        {
            if (level < 1 || level > MaximumDishLevel) throw new ArgumentOutOfRangeException(nameof(level));
            if (roll < 0 || roll >= 100) throw new ArgumentOutOfRangeException(nameof(roll));
            int cumulative = 0;
            for (int tier = 0; tier < 5; tier++)
            {
                cumulative += Probabilities[level - 1, tier];
                if (roll < cumulative) return (DishRarity)tier;
            }
            throw new InvalidOperationException("Rarity probabilities must total 100%.");
        }

        // Only the exact-successful-completion branch of PrototypeOrderSession calls this.
        internal PrototypeDishResult Complete(DishData dish, int flatXpBonus = 0, decimal xpMultiplier = 1m, decimal valueMultiplier = 1m)
        {
            int previousLevel = GetLevel(dish);
            DishRarity rarity = ResolveRarity(previousLevel, UnityEngine.Random.Range(0, 100));
            decimal payout = decimal.Round(dish.SaleValue * PayoutMultipliers[(int)rarity] * valueMultiplier, 2, MidpointRounding.AwayFromZero);
            int maximumXp = 0;
            for (int level = 1; level < MaximumDishLevel; level++) maximumXp += RequiredXp(level);
            decimal previousXp = GetExactTotalXp(dish);
            decimal gained = Math.Min((XpAwards[(int)rarity] + flatXpBonus) * xpMultiplier, maximumXp - previousXp);
            decimal total = previousXp + gained;
            int whole = (int)decimal.Truncate(total);
            dishXp[dish.DishId] = whole;
            int hundredths = (int)((total - whole) * 100m);
            if (hundredths == 0) dishXpHundredths.Remove(dish.DishId);
            else dishXpHundredths[dish.DishId] = hundredths;
            Wallet += payout;
            return new PrototypeDishResult(dish, rarity, payout, gained, previousLevel, GetLevel(dish));
        }
    }
}
