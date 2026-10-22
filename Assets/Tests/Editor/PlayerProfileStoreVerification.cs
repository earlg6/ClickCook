using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.Verification
{
    /// <summary>Repeatable real-filesystem checks using disposable project Temp folders only.</summary>
    public static class PlayerProfileStoreVerification
    {
        private static int checks;
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private static void Check(bool ok, string detail) { checks++; if (!ok) throw new Exception("7B: " + detail); }
        private static void Same(PlayerProfileSnapshot a, PlayerProfileSnapshot b)
        {
            Check(a.Wallet == b.Wallet, "exact decimal wallet");
            Check(a.DishXpById.Count == b.DishXpById.Count && a.DishXpById.All(p => b.DishXpById.TryGetValue(p.Key, out int v) && v == p.Value), "all XP restored");
            Check(a.UpgradeRanksById.Count == b.UpgradeRanksById.Count && a.UpgradeRanksById.All(p => b.UpgradeRanksById.TryGetValue(p.Key, out int v) && v == p.Value), "all ranks restored");
        }
        private static void Fund(PlayerProfile profile, decimal wallet) => typeof(PrototypeDishRewards).GetField("<Wallet>k__BackingField", Private).SetValue(profile.Rewards, wallet);
        [Serializable] private sealed class TestEnvelope { public int version = 1; public string payload; public string checksum; }
        private static string Signed(string payload)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            return JsonUtility.ToJson(new TestEnvelope { payload = payload, checksum = Convert.ToBase64String(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payload))) });
        }
        public static string PreparePlayVerification()
        {
            if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode first");
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/" + Guid.NewGuid().ToString("N"), "profile.json"));
            SessionState.SetString("ClickCook.SaveVerificationPath", path);
            return path;
        }
        public static string FinishPlayVerification()
        {
            if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode first");
            SessionState.EraseString("ClickCook.SaveVerificationPath");
            SessionState.EraseString("ClickCook.ExpectedSave");
            return "Editor save test override removed; test artifacts remain under Temp/ClickCookSaveTests.";
        }
        public static string RunModelChecks()
        {
            checks = 0;
            string folder = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests/" + Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(folder);
            var random = UnityEngine.Random.state;
            try
            {
                string path = Path.Combine(folder, "profile.json");
                var store = new PlayerProfileStore(path);
                Check(store.Load() == null && store.Status == ProfileLoadStatus.Missing, "fresh storage");
                var profile = new PlayerProfile();
                Fund(profile, 12345678901234567890.25m);
                var session = new PrototypeOrderSession(profile);
                var catalog = AssetDatabase.LoadAssetAtPath<UpgradeCatalog>("Assets/Data/UpgradeCatalog.asset");
                session.ConfigureUpgrades(catalog);
                Check(session.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var quote) && session.TryPurchaseUpgrade(quote), "real rank purchase");
                foreach (var guid in AssetDatabase.FindAssets("t:DishData", new[] { "Assets/Data/Dishes" }))
                {
                    var dish = AssetDatabase.LoadAssetAtPath<DishData>(AssetDatabase.GUIDToAssetPath(guid));
                    var xp = (System.Collections.Generic.Dictionary<int, int>)typeof(PrototypeDishRewards).GetField("dishXp", Private).GetValue(profile.Rewards);
                    xp.Add(dish.DishId, dish.DishId * 10);
                }
                Check(store.Save(profile), "first save");
                Check(File.Exists(path) && File.Exists(store.BackupPath) && !File.Exists(path + ".tmp"), "committed primary and backup");
                var reloaded = new PlayerProfileStore(path);
                Same(profile.CaptureSnapshot(), reloaded.Load().CaptureSnapshot());
                Check(reloaded.Status == ProfileLoadStatus.Loaded, "normal load");
                string original = File.ReadAllText(path);
                Fund(profile, profile.Rewards.Wallet + .25m);
                Check(store.Save(profile) && File.ReadAllText(store.BackupPath) == original, "atomic rotation keeps previous valid save");
                using (var locked = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                    Check(!store.Save(profile) && store.LastError != null, "write failure reported");
                Check(new PlayerProfileStore(path).Load().Rewards.Wallet == profile.Rewards.Wallet, "failed write retains committed save");
                File.WriteAllText(path, "{truncated");
                var recovered = new PlayerProfileStore(path); var restored = recovered.Load();
                Check(restored != null && recovered.Status == ProfileLoadStatus.Recovered, "corrupt primary falls back to backup");
                Check(recovered.Save(restored) && File.ReadAllText(recovered.BackupPath) == original, "repair never rotates corruption over backup");
                File.WriteAllText(path, original.Replace("\"version\": 1", "\"version\": 99"));
                var future = new PlayerProfileStore(path);
                Check(!store.Save(profile), "newer file appearing after load cannot be overwritten");
                Check(future.Load() == null && future.Status == ProfileLoadStatus.Unsupported, "future schema protected");
                Check(!future.Save(profile) && File.ReadAllText(path).Contains("99"), "future file cannot be autosaved over");
                Check(future.Save(new PlayerProfile(), true), "explicit new game allowed");
                Check(future.Load().Rewards.Wallet == 0, "new game clears wallet");
                File.WriteAllText(path, "corrupt after reset");
                var resetBackup = new PlayerProfileStore(path).Load().CaptureSnapshot();
                Check(resetBackup.Wallet == 0 && resetBackup.DishXpById.Count == 0 && resetBackup.UpgradeRanksById.Count == 0, "reset backup cannot resurrect progress");
                File.WriteAllText(future.BackupPath, "broken");
                var invalid = new PlayerProfileStore(path);
                Check(invalid.Load() == null && invalid.Status == ProfileLoadStatus.Invalid && !invalid.Save(profile), "both corrupt protected");
                string emptyPath = Path.Combine(folder, "temp-only.json");
                File.WriteAllText(emptyPath + ".tmp", original);
                var incomplete = new PlayerProfileStore(emptyPath);
                Check(incomplete.Load() == null && incomplete.Status == ProfileLoadStatus.Missing, "uncommitted temp never loaded");
                File.WriteAllText(emptyPath + ".bak", original);
                Check(new PlayerProfileStore(emptyPath).Load() != null, "backup without primary recovers");
                var decode = typeof(PlayerProfileStore).GetMethod("Decode", BindingFlags.Static | BindingFlags.NonPublic);
                foreach (string input in new[] { "", "null", "{}", "[]", original.Replace(".25", ".75"), original.Replace("\"version\": 1", "\"version\": 0") })
                    Check(decode.Invoke(null, new object[] { input, ProfileLoadStatus.Invalid }) == null, "invalid/corrupt payload rejected");
                foreach (string payload in new[] {
                    "{}", "{\"wallet\":\"-1\",\"dishes\":[],\"upgrades\":[]}",
                    "{\"wallet\":\"1.001\",\"dishes\":[],\"upgrades\":[]}",
                    "{\"wallet\":\"1,25\",\"dishes\":[],\"upgrades\":[]}",
                    "{\"wallet\":\"1e2\",\"dishes\":[],\"upgrades\":[]}",
                    "{\"wallet\":\"0\",\"dishes\":[{\"id\":1,\"xp\":-1}],\"upgrades\":[]}",
                    "{\"wallet\":\"0\",\"dishes\":[{\"id\":33,\"xp\":1}],\"upgrades\":[]}",
                    "{\"wallet\":\"0\",\"dishes\":[{\"id\":1,\"xp\":2147483647}],\"upgrades\":[]}",
                    "{\"wallet\":\"0\",\"dishes\":[{\"id\":1,\"xp\":1},{\"id\":1,\"xp\":2}],\"upgrades\":[]}",
                    "{\"wallet\":\"0\",\"dishes\":[],\"upgrades\":[{\"id\":\"auto\",\"rank\":1}]}",
                    "{\"wallet\":\"0\",\"dishes\":[],\"upgrades\":[{\"id\":\"click_strength\",\"rank\":16}]}",
                    "{\"wallet\":\"0\",\"dishes\":[],\"upgrades\":[{\"id\":\"click_strength\",\"rank\":-1}]}",
                    "{\"wallet\":\"0\",\"dishes\":[],\"upgrades\":[{\"id\":\"click_strength\",\"rank\":1},{\"id\":\"click_strength\",\"rank\":1}]}"
                }) Check(decode.Invoke(null, new object[] { Signed(payload), ProfileLoadStatus.Invalid }) == null, "signed invalid values rejected");
                return "7B store model PASS: " + checks;
            }
            finally
            {
                UnityEngine.Random.state = random;
                // folder is an absolute GUID child created by this invocation, inside project Temp.
                string testRoot = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/ClickCookSaveTests")) + Path.DirectorySeparatorChar;
                if (!folder.StartsWith(testRoot, StringComparison.OrdinalIgnoreCase)) throw new Exception("Unexpected test cleanup target");
                Directory.Delete(folder, true);
            }
        }
        public static string RunPlayChecks()
        {
            if (!EditorApplication.isPlaying) throw new Exception("Play Mode required");
            checks = 0;
            var controller = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var menu = controller.GetComponent<ProfileMenu>();
            Check(menu.IsBlocking, "main menu blocks startup");
            controller.CookSelectedLocation(); controller.OpenUpgrades();
            Check(!controller.OrderSession.IsDayActive, "hidden menu callbacks guarded");
            var before = controller.OrderSession.Profile.CaptureSnapshot();
            menu.RequestNewGame(); menu.ConfirmNewGame();
            Check(!menu.IsBlocking && controller.OrderSession.Rewards.Wallet == 0, "confirmed new game enters location selection");
            controller.CookSelectedLocation(); controller.SelectOrder(0);
            var session = controller.OrderSession; var order = session.ActiveOrder;
            int hp = order.RemainingClicks;
            menu.Pause();
            Check(menu.IsPaused && session.IsPaused && Time.timeScale == 0, "pause freezes simulation");
            controller.ClickCookingDish(); controller.BackToBoard(); controller.ContinueFromDishResult(); controller.FinishDay();
            session.TryCookClick(order); session.ReturnToBoard();
            Check(session.ActiveOrder == order && order.RemainingClicks == hp && session.CompletedDishes == 0, "paused direct and UI callbacks cannot cook/back/pay");
            menu.Resume(); Check(!session.IsPaused && Time.timeScale == 1, "resume restores time");
            for (int i = 0; i < hp; i++) controller.ClickCookingDish();
            Check(session.CompletedDishes == 1 && session.Rewards.Wallet > 0 && !PlayerProfileRuntime.IsDirty, "receipt autosaved");
            var earned = session.Profile.CaptureSnapshot();
            Same(earned, new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot());
            controller.ClickCookingDish(); Check(session.Rewards.Wallet == earned.Wallet, "stale reward callback ignored");
            menu.Pause(); menu.MainMenu();
            Check(menu.IsBlocking && !session.IsDayActive && session.Orders.Count == 0, "main menu clears transient day");
            menu.RequestNewGame(); menu.CancelNewGame();
            Same(earned, controller.OrderSession.Profile.CaptureSnapshot());
            menu.Continue(); Check(!menu.IsBlocking && !session.IsDayActive, "continue restores progress without day");
            // A real purchase uses gameplay earnings; repeat small days until the approved first cost is affordable.
            while (session.Rewards.Wallet < 20)
            {
                controller.CookSelectedLocation(); controller.SelectOrder(0);
                int clicks = session.ActiveOrder.RemainingClicks;
                for (int i = 0; i < clicks; i++) controller.ClickCookingDish();
                menu.Pause(); menu.MainMenu(); menu.Continue();
            }
            Check(session.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var quote) && session.TryPurchaseUpgrade(quote), "purchase with actual earnings");
            Same(session.Profile.CaptureSnapshot(), new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot());
            Check(session.ClickDamage == 2 && !PlayerProfileRuntime.IsDirty, "purchase autosave contains rank and debit together");
            controller.CookSelectedLocation(); controller.SelectOrder(0); controller.ClickCookingDish();
            var partial = session.Profile.CaptureSnapshot();
            Same(partial, new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot());
            SessionState.SetString("ClickCook.ExpectedSave", File.ReadAllText(PlayerProfileRuntime.Store.SavePath));
            return "7B Play PASS: " + checks + "; left partial day for fresh Play restart verification";
        }
        public static string VerifyRestart()
        {
            checks = 0;
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var menu = c.GetComponent<ProfileMenu>();
            Check(menu.IsBlocking && PlayerProfileRuntime.HasProgress, "fresh run enables Continue");
            Check(!c.OrderSession.IsDayActive && c.OrderSession.Orders.Count == 0 && c.OrderSession.LastResult == null, "unfinished day and result discarded");
            Check(c.OrderSession.ClickStrengthRank == 1 && c.OrderSession.Rewards.Wallet >= 0 &&
                c.OrderSession.Profile.CaptureSnapshot().DishXpById.Count > 0, "rank wallet and XP restored");
            Check(File.ReadAllText(PlayerProfileRuntime.Store.SavePath) == SessionState.GetString("ClickCook.ExpectedSave", ""), "restart did not rewrite or award anything");
            Same(c.OrderSession.Profile.CaptureSnapshot(), new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot());
            menu.Continue();
            Check(!menu.IsBlocking && !c.OrderSession.IsDayActive && c.OrderSession.ClickDamage == 2, "Continue enters preserved progress");
            return "7B restart PASS: " + checks;
        }

        public static string RunFailurePlayChecks()
        {
            checks = 0;
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var menu = c.GetComponent<ProfileMenu>();
            if (menu.IsBlocking) menu.Continue();
            c.CookSelectedLocation(); c.SelectOrder(0);
            var session = c.OrderSession; var order = session.ActiveOrder;
            using (var locked = new FileStream(PlayerProfileRuntime.Store.SavePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                int hp = order.RemainingClicks;
                for (int i = 0; i < hp; i++) c.ClickCookingDish();
                Check(PlayerProfileRuntime.IsDirty && session.CompletedDishes == 1 && session.Rewards.Wallet > 0, "failed autosave retains single receipt in memory");
                menu.Pause(); menu.Quit();
                Check(EditorApplication.isPlaying && menu.IsPaused, "failed save does not quit silently");
            }
            menu.RetrySave(); Check(!PlayerProfileRuntime.IsDirty, "retry clears dirty flag");
            Same(session.Profile.CaptureSnapshot(), new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot());
            menu.MainMenu(); menu.RequestNewGame();
            var before = session.Profile.CaptureSnapshot();
            using (var locked = new FileStream(PlayerProfileRuntime.Store.SavePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) menu.ConfirmNewGame();
            Same(before, c.OrderSession.Profile.CaptureSnapshot());
            Check(menu.IsBlocking && ReferenceEquals(session, c.OrderSession), "failed reset keeps profile/session");
            menu.RequestNewGame(); menu.ConfirmNewGame(); menu.ConfirmNewGame();
            Check(!menu.IsBlocking && c.OrderSession.Rewards.Wallet == 0 && c.OrderSession.ClickStrengthRank == 0 && c.OrderSession.Profile.CaptureSnapshot().DishXpById.Count == 0, "reset clears exactly current progress");
            File.WriteAllText(PlayerProfileRuntime.Store.SavePath, "corrupt reset primary");
            var backup = new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load().CaptureSnapshot();
            Check(backup.Wallet == 0 && backup.DishXpById.Count == 0 && backup.UpgradeRanksById.Count == 0, "new-game backup is also fresh");
            PlayerProfileRuntime.SaveNow();
            return "7B write failure/reset Play PASS: " + checks;
        }

        public static string ShowLayoutPanel(string field)
        {
            var so = new SerializedObject(UnityEngine.Object.FindFirstObjectByType<ProfileMenu>());
            foreach (string name in new[] { "mainPanel", "pausePanel", "confirmationPanel" })
                ((GameObject)so.FindProperty(name).objectReferenceValue).SetActive(name == field);
            return "Wait for rendered frames before checking " + field;
        }

        public static string RunLayoutChecks()
        {
            checks = 0;
            var menu = UnityEngine.Object.FindFirstObjectByType<ProfileMenu>();
            var so = new SerializedObject(menu);
            var panels = new[] { "mainPanel", "pausePanel", "confirmationPanel" }.Select(n => (GameObject)so.FindProperty(n).objectReferenceValue).ToArray();
            var screen = new Rect(0, 0, Screen.width, Screen.height);
                foreach (var panel in panels.Where(p => p.activeInHierarchy))
                {
                    Canvas.ForceUpdateCanvases();
                    foreach (var button in panel.GetComponentsInChildren<UnityEngine.UI.Button>())
                    {
                        var corners = new Vector3[4]; button.GetComponent<RectTransform>().GetWorldCorners(corners);
                        Check(corners.All(p => screen.Contains(new Vector2(p.x, p.y))), "button fits " + button.name);
                        var data = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = (corners[0] + corners[2]) * .5f };
                        var hits = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
                        UnityEngine.EventSystems.EventSystem.current.RaycastAll(data, hits);
                        Check(hits.Count > 0 && (hits[0].gameObject == button.gameObject || hits[0].gameObject.transform.IsChildOf(button.transform)), "topmost ray " + button.name);
                    }
                    foreach (var text in panel.GetComponentsInChildren<UnityEngine.UI.Text>())
                    {
                        Check(text.preferredHeight <= text.rectTransform.rect.height + 1, "text fits " + text.name);
                        text.font.RequestCharactersInTexture(text.text, text.fontSize, text.fontStyle);
                        foreach (char ch in text.text.Where(ch => !char.IsWhiteSpace(ch)).Distinct())
                            Check(text.font.HasCharacter(ch), "glyph " + ch);
                    }
                }
            return "7B menu layout/raycast/glyph PASS: " + checks + " @ " + Screen.width + "x" + Screen.height;
        }
    }
}
