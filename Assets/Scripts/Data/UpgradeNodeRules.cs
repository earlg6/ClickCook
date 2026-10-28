using System;

namespace ClickCook.Data
{
    /// <summary>Only implemented nodes. Future tiers are deliberately absent from purchase/save rules.</summary>
    public static partial class UpgradeNodeRules
    {
        public const string Strength = "node.click_strength.01";
        public const string CriticalChance = "node.critical_chance.01";
        public const string CriticalMultiplier = "node.critical_multiplier.01";
        public const string Hold = "node.click_hold.01";
        public const string HoldFrequency = "node.hold_frequency.01";
        public const string DoubleStrikeChance = "node.double_strike_chance.01";
        public const string DoubleStrikePower = "node.double_strike_power.01";
        public const string DoubleStrikeChanceII = "node.double_strike_chance.02";
        public const string TripleStrikeChance = "node.triple_strike_chance.01";
        public const string AutoActionCount = "node.auto_action_count.01";
        public const string AutoActionChance = "node.auto_action_chance.01";
        public const string AutoStrength = "node.auto_strength.01";
        public const string AutoStrengthMultiplier = "node.auto_strength_multiplier.01";
        public const string AutoFrequency = "node.auto_frequency.01";
        public const string DishXpFlat = "node.dish_xp_flat.01";
        public const string DishXpMultiplier = "node.dish_xp.01";
        public const string DishValue = "node.dish_value.01";
        public const double HoldInterval = .25d;
        public static string CanonicalId(string id) => id == UpgradeCatalog.ClickStrengthId ? Strength : id;
        public static string FamilyId(string id) { if(TryGetAdditionalTier(id,out var tier)) return tier.Family; return CanonicalId(id) switch {
            AutoActionCount => "auto_action_count",
            AutoActionChance => "auto_action_chance",
            AutoStrength => "auto_strength",
            AutoStrengthMultiplier => "auto_strength_multiplier",
            AutoFrequency => "auto_frequency",
            DishXpFlat => "dish_xp_flat",
            DishXpMultiplier => "dish_xp",
            DishValue => "dish_value",
            Strength => "click_strength", CriticalChance => "critical_chance", CriticalMultiplier => "critical_multiplier", Hold => "click_hold", HoldFrequency => "hold_frequency", DoubleStrikeChance or DoubleStrikeChanceII => "double_strike_chance", DoubleStrikePower => "double_strike_power", TripleStrikeChance => "triple_strike_chance", _ => null
        }; }
        public static string CatalogId(string id) => (TryGetAdditionalTier(id,out _) && id != UpgradeDiscount) || id == DoubleStrikeChanceII ? id : FamilyId(id);
        public static int MaximumRank(string id) { if(TryGetAdditionalTier(id,out var tier)) return tier.Cap; return id == DoubleStrikeChanceII || id == DishXpFlat ? 5 : id == Hold ? 1 : id == HoldFrequency || id == DoubleStrikeChance || id == DoubleStrikePower || id == TripleStrikeChance || id == AutoActionCount || id == AutoActionChance || id == AutoStrengthMultiplier ? 10 : FamilyId(id) != null ? 15 : 0; }
        public static bool PrerequisitesMet(string id, Func<string,int> rank) { if(TryGetAdditionalTier(id,out var tier)) return rank(tier.Parent) >= (tier.ParentRank == -1 ? MaximumRank(tier.Parent) : tier.ParentRank); return CanonicalId(id) switch {
            AutoActionCount => rank(Strength) >= 2,
            AutoActionChance => rank(Strength) >= 2,
            AutoStrength => rank(AutoActionCount) >= 1,
            AutoStrengthMultiplier => rank(AutoStrength) >= 1,
            AutoFrequency => rank(AutoActionCount) >= 1,
            DishXpFlat => rank(Strength) >= 1,
            DishXpMultiplier => rank(DishXpFlat) >= 1,
            DishValue => rank(Strength) >= 1,
            Strength => true, CriticalChance => rank(Strength) >= 3, CriticalMultiplier => rank(CriticalChance) >= 1, Hold => rank(Strength) >= 2, HoldFrequency => rank(Hold) >= 1, DoubleStrikeChance => rank(Strength) >= 2, DoubleStrikePower => rank(DoubleStrikeChance) >= 1, DoubleStrikeChanceII => rank(DoubleStrikeChance) >= MaximumRank(DoubleStrikeChance), TripleStrikeChance => rank(DoubleStrikeChanceII) >= 1, _ => false
        }; }
        public static decimal HoldFrequencyAtRank(int rank) => 4m + .5m * rank;
        public static int DishXpBonusAtRank(int rank) => 3 * rank;
        public static decimal DishXpMultiplierAtRank(int rank) => 1m + .07m * rank;
        public static decimal DishValueAtRank(int rank) => 1m + .03m * rank;
        public static int AutoDamageAtRank(int rank) => 1 + rank;
        public static decimal AutoMultiplierAtRank(int rank) => 1m + .1m * rank;
        public static decimal AutoFrequencyAtRank(int rank) => 1m + .15m * rank;
        public static int AutoDamageAtRanks(int strengthRank, int multiplierRank) =>
            (int)decimal.Ceiling(AutoDamageAtRank(strengthRank) * AutoMultiplierAtRank(multiplierRank));
        public static double ChanceAtRank(int rank) => rank / 50d;
        public static decimal MultiplierAtRank(int rank) => 2m + .1m * rank;
        public static double DoubleStrikeChanceAtRank(int rank) => rank / 50d;
        public static decimal DoubleStrikePowerAtRank(int rank) => .5m + .05m * rank;
        public static int CriticalDamage(int baseDamage, int multiplierRank) => (int)decimal.Ceiling(baseDamage * MultiplierAtRank(multiplierRank));
    }
}
