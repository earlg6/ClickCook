using System;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClickCook.EditorTools
{
    public static class ProfileMenuSetup
    {
        private static SerializedObject bindings;
        private static void Bind(string name, UnityEngine.Object value) => bindings.FindProperty(name).objectReferenceValue = value;
        private static GameObject FullPanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
            Undo.RegisterCreatedObjectUndo(go, "7B menu"); go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            go.GetComponent<UnityEngine.UI.Image>().color = new Color(.10f, .075f, .05f, .95f);
            return go;
        }
        private static Transform Card(GameObject screen, string title)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
            var card = UiA2Setup.Panel("MenuPaper", screen.transform, new Vector2(900, 820), Vector2.zero, sprite);
            UiA2Setup.Label("Title", card.transform, title, new Vector2(780, 80), new Vector2(0, 310), 48, true);
            return card.transform;
        }
        private static UnityEngine.UI.Button Button(string field, Transform parent, string text, float y, bool primary = false)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/Components/SecondaryButton.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(go, "7B button"); go.name = field;
            var button = go.GetComponent<UnityEngine.UI.Button>();
            UiA2Setup.Button(button, parent, new Vector2(620, 78), new Vector2(0, y), text, primary);
            Bind(field, button); return button;
        }
        private static void Navigation(params UnityEngine.UI.Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                var nav = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit };
                nav.selectOnUp = nav.selectOnLeft = buttons[(i + buttons.Length - 1) % buttons.Length];
                nav.selectOnDown = nav.selectOnRight = buttons[(i + 1) % buttons.Length];
                buttons[i].navigation = nav;
            }
        }
        public static string Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.isDirty || scene.path != "Assets/Scenes/SampleScene.unity")
                throw new InvalidOperationException("7B authoring requires saved SampleScene outside Play Mode.");
            var controller = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            if (controller == null || controller.GetComponent<ProfileMenu>() != null) throw new InvalidOperationException("Missing controller or menu already authored.");
            UiA2Setup.PrepareStyle();
            var menu = Undo.AddComponent<ProfileMenu>(controller.gameObject); bindings = new SerializedObject(menu);
            var root = new GameObject("ClickCook_ProfileMenu", typeof(RectTransform), typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler), typeof(UnityEngine.UI.GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(root, "7B canvas"); root.transform.SetParent(controller.transform, false);
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200;
            var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>(); scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var main = FullPanel("MainMenu", root.transform); Bind("mainPanel", main);
            var card = Card(main, "CLICKCOOK");
            Bind("statusText", UiA2Setup.Label("ProfileStatus", card, "Прогрес зберігається автоматично", new Vector2(780, 150), new Vector2(0, 160), 27));
            var cont = Button("continueButton", card, "Продовжити", 15, true);
            var fresh = Button("newGameButton", card, "Нова гра", -90);
            var quit = Button("quitButton", card, "Вийти з гри", -195); Navigation(cont, fresh, quit);
            var pause = FullPanel("Pause", root.transform); Bind("pausePanel", pause); card = Card(pause, "ПАУЗА");
            UiA2Setup.Label("PauseHint", card, "Гру призупинено. Esc — продовжити.\nДо меню: незавершені замовлення скинуться; зароблене залишиться.", new Vector2(780, 130), new Vector2(0, 175), 27);
            var resume = Button("resumeButton", card, "Продовжити", 15, true);
            var toMenu = Button("menuButton", card, "До головного меню", -90);
            var pauseQuit = Button("pauseQuitButton", card, "Зберегти й вийти", -195); Navigation(resume, toMenu, pauseQuit);
            var confirmation = FullPanel("NewGameConfirmation", root.transform); Bind("confirmationPanel", confirmation); card = Card(confirmation, "НОВА ГРА?");
            UiA2Setup.Label("ResetWarning", card, "Баланс, досвід усіх страв, куплені рецепти й покращення буде скинуто.\nПопередній прогрес і резервну копію буде замінено.", new Vector2(780, 200), new Vector2(0, 135), 30);
            var cancel = Button("cancelButton", card, "Скасувати", -60, true);
            var confirm = Button("confirmButton", card, "Скинути прогрес і почати", -170); Navigation(cancel, confirm);
            var pauseControl = Button("pauseButton", root.transform, "Пауза · Esc", 0);
            var pauseRt = pauseControl.GetComponent<RectTransform>(); pauseRt.anchorMin = pauseRt.anchorMax = pauseRt.pivot = Vector2.one;
            pauseRt.sizeDelta = new Vector2(240, 64); pauseRt.anchoredPosition = new Vector2(-24, -180);
            var warning = UiA2Setup.Label("SaveWarning", root.transform, "Не вдалося зберегти — прогрес поки лише в пам'яті", new Vector2(1100, 65), Vector2.zero, 26, true);
            warning.color = new Color(.75f, .06f, .04f); var wr = warning.rectTransform;
            wr.anchorMin = wr.anchorMax = wr.pivot = new Vector2(.5f, 0); wr.anchoredPosition = new Vector2(-130, 12); Bind("saveWarning", warning);
            var retry = Button("retryButton", root.transform, "Повторити запис", 0);
            var rr = retry.GetComponent<RectTransform>(); rr.anchorMin = rr.anchorMax = rr.pivot = new Vector2(1, 0);
            rr.sizeDelta = new Vector2(320, 65); rr.anchoredPosition = new Vector2(-24, 12);
            bindings.ApplyModifiedPropertiesWithoutUndo();
            pause.SetActive(false); confirmation.SetActive(false); pauseControl.gameObject.SetActive(false);
            warning.gameObject.SetActive(false); retry.gameObject.SetActive(false);
            AddSaveNotice(menu);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "7B main menu, pause, confirmation and save status authored; existing scene retained.";
        }

        public static string AddSaveNotice(ProfileMenu menu)
        {
            var so = new SerializedObject(menu);
            var text = (UnityEngine.UI.Text)so.FindProperty("saveWarning").objectReferenceValue;
            var retry = (UnityEngine.UI.Button)so.FindProperty("retryButton").objectReferenceValue;
            if (text.transform.parent.name == "SaveErrorNotice") return "Already authored";
            UiA2Setup.PrepareStyle();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
            var panel = UiA2Setup.Panel("SaveErrorNotice", text.transform.parent, new Vector2(1680, 90), Vector2.zero, sprite);
            var rt = panel.rectTransform; rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(.5f, 0);
            rt.anchoredPosition = new Vector2(0, 12);
            Undo.SetTransformParent(text.transform, panel.transform, "7B save warning");
            UiA2Setup.Center(text.rectTransform, new Vector2(1250, 65), new Vector2(-180, 0));
            Undo.SetTransformParent(retry.transform, panel.transform, "7B save retry");
            UiA2Setup.Center(retry.GetComponent<RectTransform>(), new Vector2(320, 65), new Vector2(640, 0));
            text.gameObject.SetActive(true); retry.gameObject.SetActive(true); panel.gameObject.SetActive(false);
            var pause = (UnityEngine.UI.Button)so.FindProperty("pauseButton").objectReferenceValue;
            var pr = pause.GetComponent<RectTransform>(); Undo.RecordObject(pr, "7B pause placement");
            pr.sizeDelta = new Vector2(240, 64); pr.anchoredPosition = new Vector2(-24, -180);
            EditorSceneManager.MarkSceneDirty(menu.gameObject.scene);
            return "Save error notice grouped; pause placed below existing header.";
        }
    }
}
