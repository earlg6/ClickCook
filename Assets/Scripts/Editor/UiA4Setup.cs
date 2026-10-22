using System;
using System.Reflection;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.EditorTools
{
    public static class UiA4Setup
    {
        private static T Field<T>(ClickCookPrototypeController c,string key)=>(T)typeof(ClickCookPrototypeController).GetField(key,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(c);
        private static void Folder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;var parent=System.IO.Path.GetDirectoryName(path).Replace('\\','/');Folder(parent);AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(path));
        }
        private static Material Material(string name,Color color)
        {
            string path="Assets/Art/Prototype/Materials/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(material!=null)return material;
            var shader=Shader.Find("Universal Render Pipeline/Lit");if(shader==null)throw new Exception("URP Lit required.");
            material=new Material(shader){name=name,color=color};material.SetFloat("_Smoothness",.2f);AssetDatabase.CreateAsset(material,path);return material;
        }
        private static GameObject Part(string name,Transform parent,PrimitiveType type,Vector3 pos,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;Undo.RegisterCreatedObjectUndo(go,"UI-A4 dish sketch");go.transform.SetParent(parent,false);
            go.transform.localPosition=pos;go.transform.localScale=scale;go.GetComponent<Renderer>().sharedMaterial=material;
            // Only remove the collider generated on this new decorative primitive; root owns one hit collider.
            UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
        }
        private static GameObject Group(string name,Transform parent)
        {
            var go=new GameObject(name);Undo.RegisterCreatedObjectUndo(go,"UI-A4 scene presentation");go.transform.SetParent(parent,false);return go;
        }
        public static string Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.isDirty||scene.path!="Assets/Scenes/SampleScene.unity")throw new Exception("Clean stopped SampleScene required.");
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            if(c.GetComponent<CounterScenePresentation>()!=null)throw new Exception("Counter presentation already authored.");
            var plate=GameObject.Find("Plate")?.GetComponent<Renderer>();var left=GameObject.Find("Sticker_1")?.GetComponent<Renderer>();var right=GameObject.Find("Sticker_2")?.GetComponent<Renderer>();
            var camera=GameObject.Find("Camera")?.GetComponent<Camera>();if(plate==null||left==null||right==null||camera==null)throw new Exception("Expected Dormitory plate and board anchors.");
            var go=new GameObject("UIA4_OrderBoardWorldCanvas",typeof(RectTransform),typeof(Canvas),typeof(UnityEngine.UI.GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(go,"UI-A4 board canvas");go.transform.SetParent(c.transform,false);
            var rect=go.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(1060,740);rect.localScale=Vector3.one*.001f;
            rect.rotation=left.transform.rotation*Quaternion.Euler(0,180,0);rect.position=(left.bounds.center+right.bounds.center)*.5f+Vector3.up*.04f-rect.forward*.055f;
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;
            go.GetComponent<UnityEngine.UI.GraphicRaycaster>().blockingObjects=UnityEngine.UI.GraphicRaycaster.BlockingObjects.ThreeD;
            var sheets=Field<UnityEngine.UI.Button[]>(c,"orderButtons");
            for(int i=0;i<sheets.Length;i++)
            {
                Undo.SetTransformParent(sheets[i].transform,rect,"UI-A4 attach order sheet to board");
                Undo.RecordObject(sheets[i].transform,"UI-A4 world sheet scale");
                sheets[i].transform.localScale=Vector3.one;sheets[i].transform.localRotation=Quaternion.identity;
                var sheetRect=sheets[i].GetComponent<RectTransform>();sheetRect.anchoredPosition3D=new Vector3(sheetRect.anchoredPosition.x,sheetRect.anchoredPosition.y,0);
            }
            var board=Field<GameObject>(c,"workDayScreen");
            foreach(string name in new[]{"UIA2_Plate","UIA3_ArtNote"}){var child=board.transform.Find(name);if(child!=null){Undo.RecordObject(child.gameObject,"UI-A4 remove floating presentation");child.gameObject.SetActive(false);}}
            var count=board.transform.Find("UIA2_OrderCount") as RectTransform;
            UiA2Setup.PrepareStyle();UiA2Setup.Center(count,new Vector2(1000,70),new Vector2(0,-365));
            var status=Field<UnityEngine.UI.Text>(c,"orderBoardStatusText");UiA2Setup.Text(status,count,new Vector2(960,65),Vector2.zero,28,true);
            var locationBadge=board.transform.Find("UIA2_LocationBadge") as RectTransform;
            Undo.RecordObject(locationBadge,"UI-A4 clear board view");locationBadge.sizeDelta=new Vector2(760,65);locationBadge.anchoredPosition=new Vector2(-430,-140);
            var locationText=Field<UnityEngine.UI.Text>(c,"workDayLocationText");UiA2Setup.Text(locationText,locationBadge,new Vector2(730,60),Vector2.zero,28,true);
            // Retain the old binding for compatibility; the spatial target handles mouse and UI submit.
            Undo.RecordObject(Field<UnityEngine.UI.Button>(c,"cookingDishButton").gameObject,"UI-A4 dish on actual plate");Field<UnityEngine.UI.Button>(c,"cookingDishButton").gameObject.SetActive(false);
            var result=Field<GameObject>(c,"dishResultScreen");
            foreach(string name in new[]{"UIA2_Plate","UIA2_ResultArtNote","UIA2_ResultArtNote_Paper"}){var child=result.transform.Find(name);if(child!=null){Undo.RecordObject(child.gameObject,"UI-A4 scene result dish");child.gameObject.SetActive(false);}}
            Folder("Assets/Art/Prototype/Materials");
            var bread=Material("BreadSketch",new Color(.82f,.51f,.20f));var lettuce=Material("LettuceSketch",new Color(.21f,.46f,.08f));
            var cheese=Material("CheeseSketch",new Color(1f,.70f,.13f));var mug=Material("MugSketch",new Color(.85f,.88f,.89f));
            var chocolate=Material("CakeSketch",new Color(.22f,.10f,.05f));var neutral=Material("GenericDishSketch",new Color(.65f,.58f,.43f));
            var dish=Group("UIA4_CounterDish",c.transform);dish.transform.position=new Vector3(plate.bounds.center.x,plate.bounds.max.y+.008f,plate.bounds.center.z);
            var sandwich=Group("SandwichSketch",dish.transform);
            Part("BottomBread",sandwich.transform,PrimitiveType.Cube,new Vector3(0,.025f,0),new Vector3(.38f,.05f,.30f),bread);
            Part("Lettuce",sandwich.transform,PrimitiveType.Cube,new Vector3(0,.065f,0),new Vector3(.39f,.025f,.31f),lettuce);
            Part("Cheese",sandwich.transform,PrimitiveType.Cube,new Vector3(0,.09f,0),new Vector3(.37f,.018f,.29f),cheese);
            Part("TopBread",sandwich.transform,PrimitiveType.Cube,new Vector3(0,.12f,0),new Vector3(.38f,.05f,.30f),bread);
            var cake=Group("MugCakeSketch",dish.transform);
            Part("Mug",cake.transform,PrimitiveType.Cylinder,new Vector3(0,.085f,0),new Vector3(.22f,.085f,.22f),mug);
            Part("Cake",cake.transform,PrimitiveType.Sphere,new Vector3(0,.18f,0),new Vector3(.205f,.10f,.205f),chocolate);
            Part("HandleTop",cake.transform,PrimitiveType.Cube,new Vector3(.145f,.135f,0),new Vector3(.075f,.025f,.03f),mug);
            Part("HandleEdge",cake.transform,PrimitiveType.Cube,new Vector3(.18f,.085f,0),new Vector3(.025f,.12f,.03f),mug);
            Part("HandleBottom",cake.transform,PrimitiveType.Cube,new Vector3(.145f,.035f,0),new Vector3(.075f,.025f,.03f),mug);
            var generic=Group("GenericDishSketch",dish.transform);Part("FoodPlaceholder",generic.transform,PrimitiveType.Sphere,new Vector3(0,.08f,0),new Vector3(.36f,.16f,.30f),neutral);
            var collider=Undo.AddComponent<BoxCollider>(dish);collider.center=new Vector3(0,.12f,0);collider.size=new Vector3(.44f,.25f,.36f);
            var pointer=Undo.AddComponent<CounterDishPointer>(dish);var pointerSo=new SerializedObject(pointer);pointerSo.FindProperty("controller").objectReferenceValue=c;pointerSo.ApplyModifiedProperties();
            if(camera.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>()==null)Undo.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>(camera.gameObject);
            var view=Undo.AddComponent<CounterScenePresentation>(c.gameObject);var so=new SerializedObject(view);
            void Bind(string key,UnityEngine.Object value){so.FindProperty(key).objectReferenceValue=value;}
            Bind("controller",c);Bind("sceneCamera",camera);Bind("cursorFollow",camera.GetComponent<CameraCursorFollow>());
            Bind("workDayScreen",board);Bind("cookingScreen",Field<GameObject>(c,"cookingPrototypeScreen"));Bind("resultScreen",result);
            Bind("boardCanvas",rect);Bind("boardPaperLeft",left);Bind("boardPaperRight",right);Bind("plate",plate);
            Bind("dishRoot",dish);Bind("sandwich",sandwich);Bind("mugCake",cake);Bind("genericDish",generic);Bind("dishCollider",collider);
            var array=so.FindProperty("sheets");array.arraySize=6;for(int i=0;i<6;i++)array.GetArrayElementAtIndex(i).objectReferenceValue=sheets[i];so.ApplyModifiedProperties();
            go.SetActive(false);dish.SetActive(false);FinishHint();ConfigureDishInput();Undo.FlushUndoRecordObjects();AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            return "Scene order sheets and collider-clickable 3D dish sketches authored; original board/tray/plate retained.";
        }
        public static string FinishHint()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first.");
            UiA2Setup.PrepareStyle();var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var cooking=Field<GameObject>(c,"cookingPrototypeScreen");
            if(cooking.transform.Find("UIA4_DishClickHint")==null)
            {
                var paper=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
                var hint=UiA2Setup.Panel("UIA4_DishClickHint",cooking.transform,new Vector2(820,50),new Vector2(0,-150),paper);
                UiA2Setup.Label("DishClickHint",hint.transform,"Натискай на 3D-страву на тарілці",new Vector2(790,45),Vector2.zero,26,true);
            }
            Undo.FlushUndoRecordObjects();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(c.gameObject.scene);return "Dish click hint saved";
        }

        public static string ConfigureDishInput()
        {
            if (EditorApplication.isPlaying) throw new Exception("Stop Play Mode first.");
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var pointer = c.GetComponentInChildren<CounterDishPointer>(true);
            var view = c.GetComponent<CounterScenePresentation>();
            Undo.RecordObject(view,"UI-A4 board framing");
            var viewSo = new SerializedObject(view);
            // Historical world-board scenes may still contain this field; front selection has no forced yaw.
            var legacyYaw=viewSo.FindProperty("boardYawBias");
            if(legacyYaw!=null)legacyYaw.floatValue=12f;
            viewSo.ApplyModifiedProperties();
            var cooking = Field<GameObject>(c,"cookingPrototypeScreen");
            var hint = cooking.transform.Find("UIA4_DishClickHint/DishClickHint").GetComponent<UnityEngine.UI.Text>();
            var back = Field<UnityEngine.UI.Button>(c,"backToBoardButton");
            Undo.RecordObject(pointer,"UI-A4 dish keyboard input");
            var so = new SerializedObject(pointer);
            so.FindProperty("inputHint").objectReferenceValue = hint;
            so.ApplyModifiedProperties();
            var navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit,
                selectOnDown = back, selectOnUp = back, selectOnLeft = back, selectOnRight = back };
            pointer.navigation = navigation;
            Undo.RecordObject(back,"UI-A4 keyboard return to dish");
            navigation = back.navigation;
            navigation.selectOnUp = navigation.selectOnDown = navigation.selectOnLeft = navigation.selectOnRight = pointer;
            back.navigation = navigation;
            Undo.RecordObject(hint,"UI-A4 temporary art label");
            hint.text = "Тимчасовий 3D-ескіз · натискай на страву";
            Undo.FlushUndoRecordObjects();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(c.gameObject.scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(c.gameObject.scene);
            return "Existing dish target accepts UI submit/move; explicit Back navigation and temporary-art copy saved.";
        }
    }
}
