using System;
using ClickCook.Data;

namespace ClickCook.Runtime
{
    /// <summary>Owner-approved cumulative chef XP thresholds and explicit location mapping.</summary>
    public static class PlayerProgression
    {
        public const int MaximumLevel = 10;
        public const decimal MaximumXp = 15000m;
        private static readonly decimal[] thresholds = { 0, 100, 300, 700, 1400, 2500, 4200, 6600, 10000, 15000 };
        public static decimal ThresholdForLevel(int level)
        {
            if (level < 1 || level > MaximumLevel) throw new ArgumentOutOfRangeException(nameof(level));
            return thresholds[level - 1];
        }
        public static int LevelAt(decimal xp)
        {
            int level = 1;
            while (level < MaximumLevel && xp >= thresholds[level]) level++;
            return level;
        }
        public static int RequiredLevel(LocationData location) =>
            location == null || location.LevelRequirement < 0 || location.LevelRequirement > 10 ? int.MaxValue :
                Math.Min(location.LevelRequirement + 1, MaximumLevel);
    }
}
