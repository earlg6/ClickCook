using System;
using System.Reflection;
using ClickCook.Runtime;
using UnityEditor;
using UnityEngine;

namespace ClickCook.EditorTools
{
    public static class UiA3Setup
    {
        private static T Field<T>(ClickCookPrototypeController c,string name)=>(T)typeof(ClickCookPrototypeController)
            .GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(c);
        private static void Stretch(RectTransform rt,float inset)
        {
            Undo.RecordObject(rt,"UI-A3 illustration layout");rt.anchorMin=Vector2.one*inset;
            rt.anchorMax=Vector2.one*(1-inset);rt.offsetMin=rt.offsetMax=Vector2.zero;
        }
        public static string Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.isDirty||scene.path!="Assets/Scenes/SampleScene.unity")throw new Exception("Expected clean stopped SampleScene.");
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var selection=Field<GameObject>(c,"locationSelectionScreen");
            if(selection.transform.Find("UIA3_ExitGame")!=null)throw new Exception("UI-A3 already authored.");
            UiA2Setup.PrepareStyle();var paper=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
            var exitGo=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/Components/SecondaryButton.prefab"),selection.transform);
            Undo.RegisterCreatedObjectUndo(exitGo,"UI-A3 Exit");exitGo.name="UIA3_ExitGame";
            var exit=exitGo.GetComponent<UnityEngine.UI.Button>();
            UiA2Setup.Button(exit,selection.transform,new Vector2(300,75),new Vector2(650,-425),"Вийти з гри",false);
            var cook=Field<UnityEngine.UI.Button>(c,"cookButton");var open=Field<UnityEngine.UI.Button>(c,"openUpgradesButton");
            var nav=exit.navigation;nav.mode=UnityEngine.UI.Navigation.Mode.Explicit;
            nav.selectOnLeft=nav.selectOnUp=cook;nav.selectOnRight=nav.selectOnDown=open;exit.navigation=nav;
            nav=cook.navigation;nav.selectOnDown=exit;cook.navigation=nav;
            nav=open.navigation;nav.selectOnDown=exit;open.navigation=nav;
            var board=Field<GameObject>(c,"workDayScreen");
            foreach(string name in new[]{"ClickCook_Prototype_WorkDay_Title","ClickCook_Prototype_WorkDay_Title_Paper","UIA2_BoardHint"})
            {
                var child=board.transform.Find(name);if(child!=null){Undo.RecordObject(child.gameObject,"UI-A3 clean board");child.gameObject.SetActive(false);}
            }
            var plate=board.transform.Find("UIA2_Plate") as RectTransform;
            UiA2Setup.Center(plate,new Vector2(680,350),new Vector2(0,-220));plate.SetAsFirstSibling();
            Stretch(plate.Find("PlateInner") as RectTransform,.045f);
            var orderCount=board.transform.Find("UIA2_OrderCount") as RectTransform;
            UiA2Setup.Center(orderCount,new Vector2(750,65),new Vector2(0,-365));
            var note=UiA2Setup.Panel("UIA3_ArtNote",board.transform,new Vector2(1050,50),new Vector2(0,-430),paper);
            UiA2Setup.Label("ArtNote",note.transform,"Ілюстрації страв — тимчасові ескізи",new Vector2(1020,40),Vector2.zero,24);
            var buttons=Field<UnityEngine.UI.Button[]>(c,"orderButtons");
            var fills=new UnityEngine.UI.Image[6];var texts=new UnityEngine.UI.Text[6];
            for(int i=0;i<buttons.Length;i++)
            {
                var inner=buttons[i].transform.Find("UIA2_Plate/PlateInner") as RectTransform;Stretch(inner,.045f);
                Stretch(inner.Find("DishSketch") as RectTransform,.28f);
                var track=UiA2Setup.Panel("UIA3_CardMastery",buttons[i].transform,new Vector2(310,22),new Vector2(0,-210),paper);
                track.color=new Color(.74f,.67f,.52f);
                fills[i]=UiA2Setup.Panel("Fill",track.transform,new Vector2(302,14),Vector2.zero,paper);
                fills[i].color=new Color(.26f,.51f,.20f);fills[i].type=UnityEngine.UI.Image.Type.Filled;
                fills[i].fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;fills[i].fillAmount=0;
                texts[i]=UiA2Setup.Label("UIA3_CardMasteryText",buttons[i].transform,"Майстерність: рівень 1\n0 / 20 XP",new Vector2(330,65),new Vector2(0,-135),24);
            }
            var cooking=Field<GameObject>(c,"cookingPrototypeScreen");
            var dish=Field<UnityEngine.UI.Button>(c,"cookingDishButton");
            UiA2Setup.Center(dish.GetComponent<RectTransform>(),new Vector2(650,330),new Vector2(0,-25));
            var dishPlate=dish.transform.Find("UIA2_Plate") as RectTransform;
            UiA2Setup.Center(dishPlate,new Vector2(600,290),Vector2.zero);
            Stretch(dishPlate.Find("PlateInner") as RectTransform,.045f);
            Stretch(dishPlate.Find("PlateInner/DishSketch") as RectTransform,.28f);
            UiA2Setup.Center(Field<UnityEngine.UI.Text>(c,"cookingDishLabel").rectTransform,new Vector2(550,60),new Vector2(0,-100));
            foreach(string name in new[]{"UIA2_CookingArtNote","UIA2_CookingArtNote_Paper"})
            {
                var child=cooking.transform.Find(name);if(child!=null){Undo.RecordObject(child.gameObject,"UI-A3 HP space");child.gameObject.SetActive(false);}
            }
            var hp=UiA2Setup.Panel("UIA3_CookingHP",cooking.transform,new Vector2(820,120),new Vector2(0,-245),paper);
            var hpText=UiA2Setup.Label("CookingHP",hp.transform,"HP: 5 / 5\nЗалишилось кліків: 5",new Vector2(780,65),new Vector2(0,15),26,true);
            var hpTrack=UiA2Setup.Panel("HPTrack",hp.transform,new Vector2(750,24),new Vector2(0,-40),paper);hpTrack.color=new Color(.74f,.67f,.52f);
            var hpFill=UiA2Setup.Panel("HPFill",hpTrack.transform,new Vector2(742,16),Vector2.zero,paper);
            hpFill.color=new Color(.85f,.38f,.12f);hpFill.type=UnityEngine.UI.Image.Type.Filled;
            hpFill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;hpFill.fillAmount=1;
            var so=new SerializedObject(c);
            so.FindProperty("exitGameButton").objectReferenceValue=exit;
            so.FindProperty("cookingHpFill").objectReferenceValue=hpFill;
            so.FindProperty("cookingHpText").objectReferenceValue=hpText;
            var array=so.FindProperty("orderMasteryFills");array.arraySize=6;
            var labels=so.FindProperty("orderMasteryTexts");labels.arraySize=6;
            for(int i=0;i<6;i++){array.GetArrayElementAtIndex(i).objectReferenceValue=fills[i];labels.GetArrayElementAtIndex(i).objectReferenceValue=texts[i];}
            so.ApplyModifiedProperties();
            // Author the initial two-card layout; runtime adjusts this to each location's existing capacity.
            for(int i=0;i<6;i++)
            {
                UiA2Setup.Center(buttons[i].GetComponent<RectTransform>(),new Vector2(360,540),new Vector2((i-.5f)*420,-15));
                UiA2Setup.Text(Field<UnityEngine.UI.Text[]>(c,"orderLabels")[i],buttons[i].transform,new Vector2(330,135),new Vector2(0,175),34,true);
                var p=buttons[i].transform.Find("UIA2_Plate") as RectTransform;UiA2Setup.Center(p,new Vector2(260,200),new Vector2(0,10));
            }
            PrefabUtility.SaveAsPrefabAsset(buttons[0].gameObject,"Assets/UI/Prefabs/Components/OrderCard.prefab");
            Undo.FlushUndoRecordObjects();EditorUtility.SetDirty(c);AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            return "UI-A3 authored: Exit, real cooking HP/click count, central adaptive cards and labeled mastery.";
        }
    }
}
