using System;
using ClickCook.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClickCook.EditorTools
{
    public static class DaySummarySetup
    {
        public static string Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.isDirty || scene.path != "Assets/Scenes/SampleScene.unity")
                throw new InvalidOperationException("8A authoring requires saved SampleScene outside Play Mode.");
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var so = new SerializedObject(c);
            if (so.FindProperty("daySummaryScreen").objectReferenceValue != null) return "8A already authored.";
            var existing = (GameObject)so.FindProperty("locationSelectionScreen").objectReferenceValue;
            UiA2Setup.PrepareStyle();
            var screen = new GameObject("DaySummaryScreen", typeof(RectTransform), typeof(UnityEngine.UI.Image));
            Undo.RegisterCreatedObjectUndo(screen, "8A summary");
            screen.transform.SetParent(existing.transform.parent, false);
            var rt = screen.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            screen.GetComponent<UnityEngine.UI.Image>().color = new Color(.1f, .075f, .05f, .95f);
            var paper = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
            var card = UiA2Setup.Panel("SummaryPaper", screen.transform, new Vector2(1640, 920), Vector2.zero, paper);
            UiA2Setup.Label("Title", card.transform, "ПІДСУМОК ДНЯ", new Vector2(1450, 80), new Vector2(0, 380), 44, true);
            var text = UiA2Setup.Label("Receipts", card.transform, "", new Vector2(1460, 610), new Vector2(0, 25), 25);
            text.alignment = TextAnchor.UpperLeft;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/Components/SecondaryButton.prefab");
            var next = MakeButton(prefab, card.transform, "NextDay", "Наступний день", -350, true);
            var upgrades = MakeButton(prefab, card.transform, "Upgrades", "Апгрейди", 350, false);
            foreach (var button in new[] { next, upgrades })
            {
                var other = button == next ? upgrades : next;
                button.navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit,
                    selectOnLeft = other, selectOnRight = other, selectOnUp = other, selectOnDown = other };
            }
            so.FindProperty("daySummaryScreen").objectReferenceValue = screen;
            so.FindProperty("daySummaryText").objectReferenceValue = text;
            so.FindProperty("nextDayButton").objectReferenceValue = next;
            so.FindProperty("summaryUpgradesButton").objectReferenceValue = upgrades;
            so.ApplyModifiedProperties();
            screen.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "8A summary authored with existing paper/button style.";
        }

        private static UnityEngine.UI.Button MakeButton(GameObject prefab, Transform parent, string name, string text, float x, bool primary)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(go, "8A button"); go.name = name;
            var button = go.GetComponent<UnityEngine.UI.Button>();
            UiA2Setup.Button(button, parent, new Vector2(600, 78), new Vector2(x, -370), text, primary);
            return button;
        }
    }
}
