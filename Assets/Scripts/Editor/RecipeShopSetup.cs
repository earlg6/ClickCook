using System;
using System.Linq;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClickCook.EditorTools
{
    public static class RecipeShopSetup
    {
        private static UnityEngine.UI.Button Button(Transform parent, string name, string label, Vector2 size, Vector2 position)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/Components/SecondaryButton.prefab");
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            Undo.RegisterCreatedObjectUndo(go, "8B recipes"); go.name = name;
            var b = go.GetComponent<UnityEngine.UI.Button>();
            UiA2Setup.Button(b, parent, size, position, label, false); return b;
        }
        public static string Build()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.isDirty || scene.path != "Assets/Scenes/SampleScene.unity")
                throw new InvalidOperationException("8B requires saved SampleScene outside Play.");
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var cs = new SerializedObject(c);
            if (cs.FindProperty("recipeShop").objectReferenceValue != null) return "Already authored";
            var catalog = AssetDatabase.LoadAssetAtPath<RecipeCatalog>("Assets/Data/RecipeCatalog.asset");
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<RecipeCatalog>();
                var ds = AssetDatabase.FindAssets("t:DishData").Select(id => AssetDatabase.LoadAssetAtPath<DishData>(AssetDatabase.GUIDToAssetPath(id))).OrderBy(d => d.DishId).ToArray();
                if (ds.Length != 32 || !ds.Select(d => d.DishId).SequenceEqual(Enumerable.Range(1, 32))) throw new Exception("32 stable dishes required");
                var so = new SerializedObject(catalog); var list = so.FindProperty("dishes"); list.arraySize = ds.Length;
                for (int i = 0; i < ds.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = ds[i];
                so.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.CreateAsset(catalog, "Assets/Data/RecipeCatalog.asset");
            }
            var location = (GameObject)cs.FindProperty("locationSelectionScreen").objectReferenceValue;
            UiA2Setup.PrepareStyle();
            var screen = new GameObject("RecipeShop", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(RecipeShopView));
            Undo.RegisterCreatedObjectUndo(screen, "8B recipes"); screen.transform.SetParent(location.transform.parent, false);
            var rt = screen.GetComponent<RectTransform>(); rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
            screen.GetComponent<UnityEngine.UI.Image>().color = new Color(.1f, .075f, .05f, .95f);
            var card = UiA2Setup.Panel("RecipePaper", screen.transform, new Vector2(1640, 920), Vector2.zero, AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png"));
            UiA2Setup.Label("Title", card.transform, "РЕЦЕПТИ", new Vector2(1300, 70), new Vector2(0, 390), 44, true);
            var view = screen.GetComponent<RecipeShopView>(); var vs = new SerializedObject(view);
            void Bind(string name, UnityEngine.Object value) => vs.FindProperty(name).objectReferenceValue = value;
            Bind("catalog", catalog); Bind("locations", cs.FindProperty("locationCatalog").objectReferenceValue);
            var rows = vs.FindProperty("rows"); rows.arraySize = 8;
            for (int i = 0; i < 8; i++)
            {
                var row = Button(card.transform, "Recipe" + i, "Рецепт", new Vector2(740, 62), new Vector2(-380, 245 - 70 * i));
                row.GetComponentInChildren<UnityEngine.UI.Text>().fontSize = 24;
                rows.GetArrayElementAtIndex(i).objectReferenceValue = row;
            }
            Bind("wallet", UiA2Setup.Label("Wallet", card.transform, "", new Vector2(1300, 50), new Vector2(0, 320), 27));
            Bind("details", UiA2Setup.Label("RecipeDetails", card.transform, "", new Vector2(620, 470), new Vector2(420, 20), 27));
            Bind("status", UiA2Setup.Label("PurchaseStatus", card.transform, "", new Vector2(1450, 70), new Vector2(0, -315), 25));
            Bind("pageLabel", UiA2Setup.Label("Page", card.transform, "", new Vector2(110, 55), new Vector2(-340, -390), 25));
            Bind("previous", Button(card.transform, "PreviousPage", "‹", new Vector2(100, 64), new Vector2(-460, -390)));
            Bind("next", Button(card.transform, "NextPage", "›", new Vector2(100, 64), new Vector2(-220, -390)));
            Bind("back", Button(card.transform, "Back", "До локацій", new Vector2(260, 64), new Vector2(-670, -390)));
            Bind("buy", Button(card.transform, "BuyRecipe", "Купити рецепт", new Vector2(570, 74), new Vector2(420, -390)));
            vs.ApplyModifiedPropertiesWithoutUndo();
            var open = Button(location.transform, "OpenRecipes", "Рецепти", new Vector2(290, 76), new Vector2(-605, 310));
            var nav = open.navigation; nav.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            nav.selectOnDown = (UnityEngine.UI.Button)cs.FindProperty("cookButton").objectReferenceValue;
            nav.selectOnUp = (UnityEngine.UI.Button)cs.FindProperty("openUpgradesButton").objectReferenceValue; open.navigation = nav;
            var cook = (UnityEngine.UI.Button)cs.FindProperty("cookButton").objectReferenceValue;
            var cn = cook.navigation; cn.mode = UnityEngine.UI.Navigation.Mode.Explicit; cn.selectOnUp = open; cook.navigation = cn;
            var slots = (UnityEngine.UI.Text)cs.FindProperty("dishSlotsText").objectReferenceValue;
            slots.resizeTextForBestFit = true; slots.resizeTextMinSize = 20; slots.resizeTextMaxSize = slots.fontSize;
            cs.FindProperty("recipeCatalog").objectReferenceValue = catalog;
            cs.FindProperty("recipeShop").objectReferenceValue = view;
            cs.FindProperty("openRecipesButton").objectReferenceValue = open;
            cs.ApplyModifiedProperties(); screen.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            return "8B recipe catalog and paginated shop authored; existing food/location data retained.";
        }
    }
}
