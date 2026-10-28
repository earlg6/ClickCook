using System;
using System.Collections.Generic;

namespace ClickCook.Data
{
    /// <summary>Only implemented later nodes, with independent caps/prices and purchased prerequisites.</summary>
    public sealed class UpgradeTierRule
    {
        public readonly string Id, Family, Parent;
        public readonly int Cap, ParentRank, Version, BasePrice, Growth;
        public readonly decimal Step;
        public UpgradeTierRule(string id,string family,int cap,string parent,int parentRank,int version,int basePrice,int growth,decimal step)
        {Id=id;Family=family;Cap=cap;Parent=parent;ParentRank=parentRank;Version=version;BasePrice=basePrice;Growth=growth;Step=step;}
    }
    public static partial class UpgradeNodeRules
    {
        public const string UpgradeDiscount="node.upgrade_discount.01";
        public const string DishXpFlatII="node.dish_xp_flat.02", DishXpFlatIII="node.dish_xp_flat.03";
        public const string DishValueII="node.dish_value.02", DishValueIII="node.dish_value.03", DishValueIV="node.dish_value.04";
        private static readonly Dictionary<string,UpgradeTierRule> additionalTiers=new(StringComparer.Ordinal)
        {
            [UpgradeDiscount]=new(UpgradeDiscount,"upgrade_discount",5,DishValue,1,15,300,200,.01m),
            [DishXpFlatII]=new(DishXpFlatII,"dish_xp_flat",8,DishXpFlat,-1,15,2500,170,5m),
            [DishXpFlatIII]=new(DishXpFlatIII,"dish_xp_flat",5,DishXpFlatII,-1,15,25000,180,14m),
            [DishValueII]=new(DishValueII,"dish_value",5,DishValue,-1,15,250000,170,.04m),
            [DishValueIII]=new(DishValueIII,"dish_value",5,DishValueII,-1,15,2500000,180,.05m),
            [DishValueIV]=new(DishValueIV,"dish_value",5,DishValueIII,-1,15,25000000,190,.06m),
            ["node.click_strength.02"] = new("node.click_strength.02","click_strength",13,"node.click_strength.01",-1,16,250000,160,10m),
            ["node.click_strength.03"] = new("node.click_strength.03","click_strength",10,"node.click_strength.02",-1,16,2500000,160,17m),
            ["node.click_strength.04"] = new("node.click_strength.04","click_strength",5,"node.click_strength.03",-1,16,25000000,180,80m),
            ["node.click_strength.05"] = new("node.click_strength.05","click_strength",5,"node.click_strength.04",-1,16,100000000,180,180m),
            ["node.click_strength.06"] = new("node.click_strength.06","click_strength",5,"node.click_strength.05",-1,16,500000000,180,400m),
            ["node.click_multiplier.01"] = new("node.click_multiplier.01","click_multiplier",5,"node.click_strength.04",-1,16,100000000,180,0.08m),
            ["node.click_multiplier.02"] = new("node.click_multiplier.02","click_multiplier",5,"node.click_multiplier.01",-1,16,250000000,180,0.12m),
            ["node.click_multiplier.03"] = new("node.click_multiplier.03","click_multiplier",5,"node.click_multiplier.02",-1,16,500000000,180,0.3m),
            ["node.click_multiplier.04"] = new("node.click_multiplier.04","click_multiplier",5,"node.click_multiplier.03",-1,16,1000000000,180,0.25m),
            ["node.critical_chance.02"] = new("node.critical_chance.02","critical_chance",5,"node.critical_chance.01",-1,16,100000,180,0.02m),
            ["node.critical_chance.03"] = new("node.critical_chance.03","critical_chance",5,"node.critical_chance.02",-1,16,1000000,180,0.02m),
            ["node.critical_multiplier.02"] = new("node.critical_multiplier.02","critical_multiplier",5,"node.critical_multiplier.01",-1,16,250000,180,0.35m),
            ["node.critical_multiplier.03"] = new("node.critical_multiplier.03","critical_multiplier",5,"node.critical_multiplier.02",-1,16,2500000,180,0.5m),
            ["node.double_strike_chance.03"] = new("node.double_strike_chance.03","double_strike_chance",5,"node.double_strike_chance.02",-1,16,2000000,180,0.01m),
            ["node.double_strike_power.02"] = new("node.double_strike_power.02","double_strike_power",10,"node.double_strike_power.01",-1,16,1000000,180,0.05m),
            ["node.triple_strike_chance.02"] = new("node.triple_strike_chance.02","triple_strike_chance",5,"node.triple_strike_chance.01",-1,16,2000000,180,0.01m),
            ["node.hold_frequency.02"] = new("node.hold_frequency.02","hold_frequency",10,"node.hold_frequency.01",-1,16,100000,180,0.5m),
            ["node.auto_action_chance.02"] = new("node.auto_action_chance.02","auto_action_chance",5,"node.auto_action_chance.01",-1,16,50000,180,0.02m),
            ["node.auto_action_chance.03"] = new("node.auto_action_chance.03","auto_action_chance",5,"node.auto_action_chance.02",-1,16,250000,180,0.03m),
            ["node.auto_action_count.02"] = new("node.auto_action_count.02","auto_action_count",5,"node.auto_action_count.01",-1,16,250000,180,2m),
            ["node.auto_action_count.03"] = new("node.auto_action_count.03","auto_action_count",5,"node.auto_action_count.02",-1,16,2500000,180,2m),
            ["node.auto_frequency.02"] = new("node.auto_frequency.02","auto_frequency",5,"node.auto_frequency.01",-1,16,5000000,180,0.3m),
            ["node.auto_frequency.03"] = new("node.auto_frequency.03","auto_frequency",5,"node.auto_frequency.02",-1,16,25000000,180,0.45m),
            ["node.auto_frequency.04"] = new("node.auto_frequency.04","auto_frequency",5,"node.auto_frequency.03",-1,16,100000000,180,0.6m),
            ["node.auto_strength.02"] = new("node.auto_strength.02","auto_strength",5,"node.auto_strength.01",-1,16,25000000,180,36m),
            ["node.auto_strength.03"] = new("node.auto_strength.03","auto_strength",5,"node.auto_strength.02",-1,16,100000000,180,130m),
            ["node.auto_strength_multiplier.02"] = new("node.auto_strength_multiplier.02","auto_strength_multiplier",10,"node.auto_strength_multiplier.01",-1,16,1000000,180,0.1m)
        };
        public static IEnumerable<UpgradeTierRule> AdditionalTiers=>additionalTiers.Values;
        public static bool TryGetAdditionalTier(string id,out UpgradeTierRule rule)=>additionalTiers.TryGetValue(id??"",out rule);
        public static int IntroducedVersion(string id)=>TryGetAdditionalTier(id,out var rule)?rule.Version:1;
        public static decimal AdditionalFactor(string family,Func<string,int> rank)
        {decimal value=1;foreach(var rule in additionalTiers.Values)if(rule.Family==family)value*=1+rule.Step*rank(rule.Id);return value;}
        public static decimal AdditionalContribution(string family,Func<string,int> rank)
        {decimal value=0;foreach(var rule in additionalTiers.Values)if(rule.Family==family)value+=rule.Step*rank(rule.Id);return value;}
    }
}