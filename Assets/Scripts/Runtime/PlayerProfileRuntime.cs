using UnityEngine;

namespace ClickCook.Runtime
{
    /// <summary>Run lifetime, not screen lifetime. Entering Play resets even with domain reload disabled.</summary>
    public static class PlayerProfileRuntime
    {
        private static PlayerProfile current;
        private static PlayerProfileStore store;
        public static PlayerProfileStore Store => store ??= new PlayerProfileStore(DefaultPath);
        public static bool HasProgress { get; private set; }
        public static bool IsDirty { get; private set; }
        public static bool RecoveredBackup { get; private set; }
        public static PlayerProfile Current
        {
            get
            {
                if (current == null)
                {
                    var loaded = Store.Load();
                    HasProgress = loaded != null;
                    RecoveredBackup = Store.Status == ProfileLoadStatus.Recovered;
                    Bind(loaded ?? new PlayerProfile());
                }
                return current;
            }
        }
        private static string DefaultPath
        {
            get
            {
#if UNITY_EDITOR
                string verificationPath = UnityEditor.SessionState.GetString("ClickCook.SaveVerificationPath", "");
                if (!string.IsNullOrEmpty(verificationPath)) return verificationPath;
                // Editor play/testing never overwrites a standalone player's profile.
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath, "../UserSettings/ClickCookProfile/profile.json"));
#else
                return System.IO.Path.Combine(Application.persistentDataPath, "profile.json");
#endif
            }
        }
        private static void Bind(PlayerProfile profile)
        {
            if (current != null) current.Changed -= OnChanged;
            current = profile;
            current.Changed += OnChanged;
        }
        private static void OnChanged() { IsDirty = true; SaveNow(); }
        public static bool SaveNow()
        {
            if (current == null || (!HasProgress && !IsDirty)) return true;
            if (!Store.Save(current)) { IsDirty = true; return false; }
            HasProgress = true; IsDirty = false;
            return true;
        }
        public static bool NewGame()
        {
            if (Current.HasActiveDay) return false;
            var fresh = new PlayerProfile();
            if (!Store.Save(fresh, true)) return false;
            Bind(fresh); HasProgress = true; IsDirty = false; RecoveredBackup = false;
            return true;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForNewRun()
        {
            if (current != null) current.Changed -= OnChanged;
            current = null; store = null; HasProgress = IsDirty = RecoveredBackup = false;
        }
    }
}
