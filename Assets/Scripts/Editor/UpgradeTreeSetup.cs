using System;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ClickCook.EditorTools
{
    public static class UpgradeTreeSetup
    {
        public static string Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring U1.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != "Assets/Scenes/SampleScene.unity" || scene.isDirty)
                throw new InvalidOperationException("Expected clean SampleScene; preserve pending scene work first.");
            var controller = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var serialized = new SerializedObject(controller);
            var panel = (GameObject)serialized.FindProperty("upgradeScreen").objectReferenceValue;
            var view = panel.GetComponent<UpgradeTreeView>();
            if (view == null) view = Undo.AddComponent<UpgradeTreeView>(panel);
            serialized.FindProperty("upgradeTreeView").objectReferenceValue = view;
            serialized.ApplyModifiedProperties();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            return "U1 view attached to the existing upgrade screen; legacy purchase references retained.";
        }
    }
}
