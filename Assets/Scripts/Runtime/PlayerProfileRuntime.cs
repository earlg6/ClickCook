using UnityEngine;

namespace ClickCook.Runtime
{
    /// <summary>Run lifetime, not screen lifetime. Entering Play resets even with domain reload disabled.</summary>
    public static class PlayerProfileRuntime
    {
        private static PlayerProfile current;
        public static PlayerProfile Current => current ??= new PlayerProfile();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewRun() => current = null;
    }
}
