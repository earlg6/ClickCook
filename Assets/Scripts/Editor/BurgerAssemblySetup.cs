using System;
using System.Linq;
using ClickCook.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClickCook.EditorTools
{
    /// <summary>Imports the existing authored burger into a six-layer presentation prefab.</summary>
    public static class BurgerAssemblySetup
    {
        private const string Folder = "Assets/Models/Food/Burger/";
        private const string PrefabPath = Folder + "BurgerAssembly.prefab";

        public static string Install()
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/SampleScene.unity" || scene.isDirty)
                throw new InvalidOperationException("Open clean SampleScene outside Play Mode first.");
            var view = UnityEngine.Object.FindFirstObjectByType<CounterScenePresentation>();
            if (view == null) throw new InvalidOperationException("Existing counter presentation required.");
            var bindings = new SerializedObject(view);
            var cookingScreen=(GameObject)bindings.FindProperty("cookingScreen").objectReferenceValue;
            var namePanel=cookingScreen.transform.Find("UIA2_CookingName") as RectTransform;
            if(namePanel==null)throw new InvalidOperationException("Existing cooking name panel required.");
            bindings.FindProperty("cookingNamePanel").objectReferenceValue=namePanel;
            var dishRoot = (GameObject)bindings.FindProperty("dishRoot").objectReferenceValue;
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (prefab == null) prefab = BuildPrefab();
            if(bindings.FindProperty("burger").objectReferenceValue==null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, dishRoot.transform);
                instance.name = "ClickCook_BurgerAssembly";
                instance.SetActive(false);
                bindings.FindProperty("burger").objectReferenceValue = instance.GetComponent<BurgerAssemblyPresentation>();
            }
            bindings.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Scene save failed.");
            return "Installed six-layer burger prefab for Dish ID 6; SampleScene saved.";
        }

        private static GameObject BuildPrefab()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + "ClickCook_Burger.fbx");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + "Textures/Burger_BaseColor.png");
            if (source == null || texture == null) throw new InvalidOperationException("Existing burger FBX and atlas required.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "Burger_URP.mat");
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new InvalidOperationException("URP Lit shader required.");
                material = new Material(shader) { name = "Burger_URP" };
                material.SetTexture("_BaseMap", texture);
                material.SetColor("_BaseColor", Color.white);
                material.SetFloat("_Smoothness", .15f);
                material.SetFloat("_Metallic", 0f);
                AssetDatabase.CreateAsset(material, Folder + "Burger_URP.mat");
            }
            var root = new GameObject("BurgerAssembly");
            try
            {
                var imported = (GameObject)PrefabUtility.InstantiatePrefab(source, root.transform);
                PrefabUtility.UnpackPrefabInstance(imported, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                var meshes = imported.GetComponentsInChildren<MeshRenderer>(true);
                if (meshes.Length != 15) throw new InvalidOperationException("Expected 15 authored burger meshes.");
                var bounds = meshes[0].bounds;
                foreach (var mesh in meshes) bounds.Encapsulate(mesh.bounds);
                float width = Mathf.Max(bounds.size.x, bounds.size.z);
                if (width <= 0f) throw new InvalidOperationException("Burger bounds are empty.");
                imported.transform.localScale *= .34f / width;
                bounds = meshes[0].bounds;
                foreach (var mesh in meshes) bounds.Encapsulate(mesh.bounds);
                imported.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                string[] names = { "01_Bottom_Bun", "02_Beef_Patty", "03_Draped_Cheddar",
                    "04_Ruffled_Lettuce", "05_Tomato_Slice", "06_Golden_Top_Bun" };
                var layers = names.Select(name => new GameObject(name + "_Layer").transform).ToArray();
                foreach (var layer in layers) layer.SetParent(root.transform, false);
                foreach (var mesh in meshes)
                {
                    int index = Array.IndexOf(names, mesh.name);
                    if (mesh.name.StartsWith("07_Sesame_", StringComparison.Ordinal)) index = 5;
                    if (index < 0) throw new InvalidOperationException("Unknown burger mesh: " + mesh.name);
                    mesh.transform.SetParent(layers[index], true);
                    mesh.sharedMaterial = material;
                }
                UnityEngine.Object.DestroyImmediate(imported);
                var assembly = root.AddComponent<BurgerAssemblyPresentation>();
                var serialized = new SerializedObject(assembly);
                var entries = serialized.FindProperty("layers");
                entries.arraySize = layers.Length;
                for (int i = 0; i < layers.Length; i++) entries.GetArrayElementAtIndex(i).objectReferenceValue = layers[i];
                serialized.ApplyModifiedPropertiesWithoutUndo();
                var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
                if (prefab == null) throw new InvalidOperationException("Burger prefab save failed.");
                return prefab;
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
