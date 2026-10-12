using System;
using System.IO;
using System.Reflection;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;

namespace ClickCook.EditorTools
{
    /// <summary>Explicit one-time authoring of the existing day screens. No gameplay data edits.</summary>
    public static class UiA2Setup
    {
        private static Font font;
        private static Sprite paper, primary, circle;
        private static Color Ink => new Color(.23f,.16f,.11f);
        private static T Field<T>(ClickCookPrototypeController c,string key) =>
            (T)typeof(ClickCookPrototypeController).GetField(key,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(c);
        internal static void Center(RectTransform rt,Vector2 size,Vector2 pos)
        {
            Undo.RecordObject(rt,"UI-A2 layout");
            rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=pos;
        }
        private static void Full(RectTransform rt)
        {
            Undo.RecordObject(rt,"UI-A2 fill");rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;
            rt.pivot=new Vector2(.5f,.5f);rt.offsetMin=rt.offsetMax=Vector2.zero;
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 pos)
        {
            var go=new GameObject(name,typeof(RectTransform));Undo.RegisterCreatedObjectUndo(go,"UI-A2 authoring");
            go.transform.SetParent(parent,false);var rt=go.GetComponent<RectTransform>();Center(rt,size,pos);return rt;
        }
        internal static UnityEngine.UI.Image Panel(string name,Transform parent,Vector2 size,Vector2 pos,Sprite sprite)
        {
            var rt=Rect(name,parent,size,pos);var image=rt.gameObject.AddComponent<UnityEngine.UI.Image>();
            image.sprite=sprite;image.type=sprite==circle ? UnityEngine.UI.Image.Type.Simple : UnityEngine.UI.Image.Type.Sliced;
            image.raycastTarget=false;return image;
        }
        internal static void Text(UnityEngine.UI.Text t,Transform parent,Vector2 size,Vector2 pos,int px,bool bold=false)
        {
            Undo.SetTransformParent(t.transform,parent,"UI-A2 grouping");Undo.RecordObject(t,"UI-A2 typography");
            Center(t.rectTransform,size,pos);t.font=font;t.fontSize=px;t.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;
            t.color=Ink;t.alignment=TextAnchor.MiddleCenter;t.raycastTarget=false;t.resizeTextForBestFit=false;
            t.horizontalOverflow=HorizontalWrapMode.Wrap;t.verticalOverflow=VerticalWrapMode.Truncate;
        }
        internal static UnityEngine.UI.Text Label(string name,Transform parent,string copy,Vector2 size,Vector2 pos,int px,bool bold=false)
        {
            var rt=Rect(name,parent,size,pos);var t=rt.gameObject.AddComponent<UnityEngine.UI.Text>();
            Text(t,parent,size,pos,px,bold);t.text=copy;return t;
        }
        internal static void Button(UnityEngine.UI.Button b,Transform parent,Vector2 size,Vector2 pos,string copy,bool accent)
        {
            Undo.SetTransformParent(b.transform,parent,"UI-A2 grouping");Undo.RecordObject(b,"UI-A2 button");
            Center(b.GetComponent<RectTransform>(),size,pos);var image=b.GetComponent<UnityEngine.UI.Image>();
            Undo.RecordObject(image,"UI-A2 art");image.sprite=accent?primary:paper;image.color=Color.white;
            image.type=UnityEngine.UI.Image.Type.Sliced;image.raycastTarget=true;b.targetGraphic=image;
            var colors=b.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.94f,.8f);
            colors.pressedColor=new Color(.82f,.70f,.52f);colors.selectedColor=new Color(1,.98f,.88f);
            colors.disabledColor=new Color(.68f,.66f,.61f);colors.fadeDuration=.1f;b.colors=colors;
            var text=b.GetComponentInChildren<UnityEngine.UI.Text>(true);
            Text(text,b.transform,size-new Vector2(32,20),Vector2.zero,28,true);text.text=copy;
            var outline=b.GetComponent<UnityEngine.UI.Outline>()??Undo.AddComponent<UnityEngine.UI.Outline>(b.gameObject);
            outline.effectColor=new Color(.09f,.21f,.42f);outline.effectDistance=new Vector2(5,-5);outline.enabled=false;
            var cue=b.GetComponent<UiFocusOutline>()??Undo.AddComponent<UiFocusOutline>(b.gameObject);
            var so=new SerializedObject(cue);so.FindProperty("focusOutline").objectReferenceValue=outline;so.ApplyModifiedProperties();
        }
        private static void Nav(UnityEngine.UI.Button a,UnityEngine.UI.Button other)
        {
            var n=a.navigation;n.mode=UnityEngine.UI.Navigation.Mode.Explicit;
            n.selectOnLeft=n.selectOnRight=n.selectOnUp=n.selectOnDown=other;a.navigation=n;
        }
        internal static void PrepareStyle()
        {
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            paper=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
            primary=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PrimaryButton.png");
            circle=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PlateCircle.png");
            if(paper==null||primary==null||circle==null)throw new Exception("UI-A2 assets required.");
        }
        private static void Header(RectTransform rt,Vector2 size,float x)
        {
            Center(rt,size,Vector2.zero);rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,1);
            rt.anchoredPosition=new Vector2(x,-40);
        }
        private static void Screen(GameObject go)
        {
            Full(go.GetComponent<RectTransform>());var img=go.GetComponent<UnityEngine.UI.Image>();
            if(img!=null){Undo.RecordObject(img,"UI-A2 transparent screen");img.color=Color.clear;img.raycastTarget=false;}
        }
        private static Sprite Circle()
        {
            const string path="Assets/UI/Art/PlateCircle.png";
            var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);if(existing!=null)return existing;
            const int size=256;var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float d=Vector2.Distance(new Vector2(x+.5f,y+.5f),new Vector2(128,128));
                texture.SetPixel(x,y,new Color(1,1,1,Mathf.Clamp01(126-d)));
            }
            texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static void Plate(Transform parent,Vector2 size,Vector2 pos,bool sketch)
        {
            var outer=Panel("UIA2_Plate",parent,size,pos,circle);outer.color=new Color(.76f,.70f,.61f);
            var inner=Panel("PlateInner",outer.transform,size*.91f,Vector2.zero,circle);inner.color=new Color(.97f,.95f,.89f);
            if(sketch)
            {
                var mark=Panel("DishSketch",inner.transform,size*.43f,Vector2.zero,paper);mark.color=new Color(.94f,.67f,.28f);
                Label("SketchLabel",mark.transform,size.x>300?"Ескіз":string.Empty,size*.4f,Vector2.zero,size.x>300?34:24,true);
            }
        }
        public static string Build()
        {
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(EditorApplication.isPlaying||scene.isDirty||scene.path!="Assets/Scenes/SampleScene.unity")
                throw new Exception("Expected stopped, clean SampleScene.");
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var board=Field<GameObject>(c,"workDayScreen");var cooking=Field<GameObject>(c,"cookingPrototypeScreen");
            var result=Field<GameObject>(c,"dishResultScreen");
            if(board.transform.Find("UIA2_WalletBadge")!=null)throw new Exception("UI-A2 already authored.");
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            paper=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
            primary=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PrimaryButton.png");
            if(paper==null||primary==null)throw new Exception("UI-A1 assets required.");circle=Circle();
            Screen(board);Screen(cooking);Screen(result);
            var serialized=new SerializedObject(c);
            var wallet=Panel("UIA2_WalletBadge",board.transform,new Vector2(450,85),Vector2.zero,paper);
            Header(wallet.rectTransform,new Vector2(450,85),-645);
            var walletText=Label("DayWallet",wallet.transform,"Баланс: 0,00 у.е.",new Vector2(420,70),Vector2.zero,28,true);
            var count=Panel("UIA2_DayCount",board.transform,new Vector2(420,85),Vector2.zero,paper);
            Header(count.rectTransform,new Vector2(420,85),0);
            var countText=Label("DayCompleted",count.transform,"Приготовано: 0 / 10",new Vector2(400,70),Vector2.zero,30,true);
            var finish=Field<UnityEngine.UI.Button>(c,"finishDayButton");
            Button(finish,board.transform,new Vector2(400,85),Vector2.zero,"Завершити день",false);
            Header(finish.GetComponent<RectTransform>(),new Vector2(400,85),665);
            // Its label has its own center relative to the button, not the screen header.
            Text(finish.GetComponentInChildren<UnityEngine.UI.Text>(),finish.transform,new Vector2(368,65),Vector2.zero,28,true);
            var location=Field<UnityEngine.UI.Text>(c,"workDayLocationText");
            var locationPanel=Panel("UIA2_LocationBadge",board.transform,new Vector2(1120,80),Vector2.zero,paper);
            Header(locationPanel.rectTransform,new Vector2(1120,80),0);locationPanel.rectTransform.anchoredPosition=new Vector2(0,-140);
            Text(location,locationPanel.transform,new Vector2(1090,70),Vector2.zero,32,true);
            var title=board.transform.Find("ClickCook_Prototype_WorkDay_Title")?.GetComponent<UnityEngine.UI.Text>();
            if(title!=null){Text(title,board.transform,new Vector2(680,80),new Vector2(-460,125),42,true);title.text="ОБЕРИ ЗАМОВЛЕННЯ";title.color=new Color(1,.94f,.81f);}
            // The complete documented pool stays available in the Editor; active cards remain distinct.
            Field<UnityEngine.UI.Text>(c,"workDayDishCandidatesText").gameObject.SetActive(false);
            var hint=Panel("UIA2_BoardHint",board.transform,new Vector2(690,85),new Vector2(-460,30),paper);
            Label("BoardHint",hint.transform,"Обери картку праворуч, щоб почати готувати",new Vector2(650,75),Vector2.zero,28);
            Plate(board.transform,new Vector2(590,350),new Vector2(-460,-220),false);
            var status=Field<UnityEngine.UI.Text>(c,"orderBoardStatusText");
            var statusPanel=Panel("UIA2_OrderCount",board.transform,new Vector2(750,75),new Vector2(485,280),paper);
            Text(status,statusPanel.transform,new Vector2(720,70),Vector2.zero,28,true);
            var buttons=Field<UnityEngine.UI.Button[]>(c,"orderButtons");var labels=Field<UnityEngine.UI.Text[]>(c,"orderLabels");
            for(int i=0;i<buttons.Length;i++)
            {
                Button(buttons[i],board.transform,new Vector2(235,300),new Vector2(235+(i%3)*250,80-(i/3)*325),"Замовлення",false);
                Text(labels[i],buttons[i].transform,new Vector2(215,165),new Vector2(0,-62),26,true);
                Plate(buttons[i].transform,new Vector2(110,90),new Vector2(0,90),true);
            }
            PrefabUtility.SaveAsPrefabAsset(buttons[0].gameObject,"Assets/UI/Prefabs/Components/OrderCard.prefab");
            var cookingTitle=cooking.transform.Find("ClickCook_Prototype_Cooking_Title")?.GetComponent<UnityEngine.UI.Text>();
            if(cookingTitle!=null){Text(cookingTitle,cooking.transform,new Vector2(700,80),new Vector2(0,370),42,true);cookingTitle.text="ГОТУВАННЯ";cookingTitle.color=new Color(1,.94f,.81f);}
            var namePanel=Panel("UIA2_CookingName",cooking.transform,new Vector2(1200,145),new Vector2(0,240),paper);
            Text(Field<UnityEngine.UI.Text>(c,"cookingOrderText"),namePanel.transform,new Vector2(1150,130),Vector2.zero,34,true);
            var dish=Field<UnityEngine.UI.Button>(c,"cookingDishButton");
            Button(dish,cooking.transform,new Vector2(650,390),new Vector2(0,-40),"Натискай на страву",false);
            var dishImage=dish.GetComponent<UnityEngine.UI.Image>();dishImage.sprite=circle;dishImage.type=UnityEngine.UI.Image.Type.Simple;
            Plate(dish.transform,new Vector2(600,350),Vector2.zero,true);
            Text(Field<UnityEngine.UI.Text>(c,"cookingDishLabel"),dish.transform,new Vector2(550,60),new Vector2(0,-125),30,true);
            var sketch=Label("UIA2_CookingArtNote",cooking.transform,"Тимчасовий ескіз страви",new Vector2(700,60),new Vector2(0,-270),26);
            sketch.color=new Color(1,.94f,.81f);
            var back=Field<UnityEngine.UI.Button>(c,"backToBoardButton");
            Button(back,cooking.transform,new Vector2(440,85),new Vector2(0,-360),"До замовлень",false);Nav(dish,back);Nav(back,dish);
            var backHint=Label("UIA2_BackHint",cooking.transform,"Повернення скине прогрес приготування цієї страви",new Vector2(1200,60),new Vector2(0,-425),26);
            backHint.color=new Color(1,.94f,.81f);
            var resultTitle=result.transform.Find("ClickCook_Prototype_DishResult_Title")?.GetComponent<UnityEngine.UI.Text>();
            if(resultTitle!=null){Text(resultTitle,result.transform,new Vector2(700,80),new Vector2(-450,260),44,true);resultTitle.text="СТРАВА ГОТОВА!";resultTitle.color=new Color(1,.94f,.81f);}
            Plate(result.transform,new Vector2(570,370),new Vector2(-450,-40),true);
            var note=Label("UIA2_ResultArtNote",result.transform,"Тимчасовий ескіз страви",new Vector2(650,60),new Vector2(-450,-285),26);note.color=new Color(1,.94f,.81f);
            var resultPaper=Panel("UIA2_ResultPaper",result.transform,new Vector2(840,850),new Vector2(435,0),paper);
            var rarity=Label("ResultRarity",resultPaper.transform,"Звичайна",new Vector2(780,70),new Vector2(0,325),36,true);
            Text(Field<UnityEngine.UI.Text>(c,"dishResultText"),resultPaper.transform,new Vector2(780,390),new Vector2(0,80),32,true);
            var mastery=Label("ResultMastery",resultPaper.transform,"Досвід до наступного рівня",new Vector2(780,90),new Vector2(0,-160),28);
            var track=Panel("MasteryTrack",resultPaper.transform,new Vector2(720,28),new Vector2(0,-230),paper);track.color=new Color(.74f,.67f,.52f);
            var fill=Panel("MasteryFill",track.transform,new Vector2(710,20),Vector2.zero,paper);fill.color=new Color(.26f,.51f,.20f);
            fill.type=UnityEngine.UI.Image.Type.Filled;fill.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;fill.fillOrigin=0;fill.fillAmount=0;
            var next=Field<UnityEngine.UI.Button>(c,"continueFromResultButton");Button(next,resultPaper.transform,new Vector2(550,85),new Vector2(0,-335),"Продовжити",true);Nav(next,null);
            serialized.FindProperty("dayWalletText").objectReferenceValue=walletText;
            serialized.FindProperty("dayCompletedText").objectReferenceValue=countText;
            serialized.FindProperty("resultRarityText").objectReferenceValue=rarity;
            serialized.FindProperty("resultMasteryText").objectReferenceValue=mastery;
            serialized.FindProperty("resultMasteryFill").objectReferenceValue=fill;
            serialized.ApplyModifiedProperties();
            FinishContrast();
            AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            return "UI-A2 authored and saved: existing board/cooking/result bindings reused; no environment/data edits.";
        }
        public static string FinishContrast()
        {
            if(EditorApplication.isPlaying)throw new Exception("Stop Play Mode first.");
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            paper=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/PaperPanel.png");
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            string[] names={"ClickCook_Prototype_WorkDay_Title","ClickCook_Prototype_Cooking_Title",
                "UIA2_CookingArtNote","UIA2_BackHint","ClickCook_Prototype_DishResult_Title","UIA2_ResultArtNote"};
            foreach(var name in names)
            {
                foreach(var t in c.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                {
                    if(t.name!=name)continue;
                    var rt=t.rectTransform;string badgeName=name+"_Paper";
                    if(rt.parent.Find(badgeName)==null)
                    {
                        var badge=Panel(badgeName,rt.parent,rt.sizeDelta+new Vector2(24,12),rt.anchoredPosition,paper);
                        badge.transform.SetSiblingIndex(rt.GetSiblingIndex());
                    }
                    Undo.RecordObject(t,"UI-A2 contrast");t.color=Ink;
                }
            }
            Field<UnityEngine.UI.Text>(c,"cookingDishLabel").transform.SetAsLastSibling();
            foreach(var t in c.GetComponentsInChildren<UnityEngine.UI.Text>(true))
            {
                if(t.name=="SketchLabel" && t.fontSize==24) { Undo.RecordObject(t,"UI-A2 icon copy");t.text=string.Empty; }
                if(t.name=="BoardHint") { Undo.RecordObject(t,"UI-A2 placeholder note");t.text="Обери картку праворуч, щоб готувати\nІлюстрації страв — тимчасові"; }
            }
            var scene=c.gameObject.scene;UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
            return "Paper-backed headings/hints and cooking label draw order corrected and saved.";
        }
    }
}
