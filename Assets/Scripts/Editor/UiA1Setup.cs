using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using ClickCook.Runtime;
using ClickCook.UI;

namespace ClickCook.EditorTools
{
    /// <summary>One-time UI-A1 authoring via the live Editor. Does not alter gameplay definitions.</summary>
    public static class UiA1Setup
    {
        private static readonly Color Ink = Hex("#3B281B");
        private static readonly Color Muted = Hex("#6A4B32");
        private static Font font;
        private static Sprite paper, primary, secondary, background;
        private static Color Hex(string value) { ColorUtility.TryParseHtmlString(value, out var c); return c; }
        private static void Folder(string path)
        {
            if(AssetDatabase.IsValidFolder(path)) return;
            string parent=Path.GetDirectoryName(path).Replace('\\','/');
            Folder(parent); AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
        }
        private static Sprite Texture(string name, bool blue, bool button)
        {
            string path="Assets/UI/Art/"+name+".png";
            var existing=AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if(existing!=null) return existing;
            int size=128;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                if(blue)
                {
                    float distance=Vector2.Distance(new Vector2(x,y),new Vector2(64,64))/91f;
                    Color color=Color.Lerp(Hex("#234E90"),Hex("#10264D"),distance);
                    float speck=(x*19+y*37)%151==0 ? .035f : 0f;
                    texture.SetPixel(x,y,color+new Color(speck,speck,speck,0));
                    continue;
                }
                float dx=Mathf.Max(Mathf.Abs(x-63.5f)-41.5f,0);
                float dy=Mathf.Max(Mathf.Abs(y-63.5f)-41.5f,0);
                float rounded=Mathf.Sqrt(dx*dx+dy*dy);
                float edge=Mathf.Min(Mathf.Min(x,127-x),Mathf.Min(y,127-y));
                bool outside=rounded>22;
                bool border=rounded>19 || edge<3;
                Color fill=button ? Color.Lerp(Hex("#F3A532"),Hex("#FFD379"),y/127f) : Hex("#FFF0CE");
                float grain=((x*31+y*17)%13-6)/1200f;
                fill+=new Color(grain,grain,grain,0);
                texture.SetPixel(x,y,outside ? Color.clear : border ? Hex("#B67B29") : fill);
            }
            texture.Apply();
            File.WriteAllBytes(path,texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100;importer.spriteBorder=blue ? Vector4.zero : new Vector4(26,26,26,26);
            importer.mipmapEnabled=false;importer.alphaIsTransparency=true;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.filterMode=FilterMode.Bilinear;importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        private static RectTransform Rect(string name,Transform parent,Vector2 size,Vector2 position)
        {
            var go=new GameObject(name,typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go,"UI-A1 authoring");go.transform.SetParent(parent,false);
            var rt=go.GetComponent<RectTransform>();Center(rt,size,position);return rt;
        }
        private static void Center(RectTransform rt,Vector2 size,Vector2 pos)
        {
            Undo.RecordObject(rt,"UI-A1 layout");
            rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(.5f,.5f);rt.sizeDelta=size;rt.anchoredPosition=pos;
        }
        private static void Full(RectTransform rt)
        {
            Undo.RecordObject(rt,"UI-A1 fill");
            rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.pivot=new Vector2(.5f,.5f);
            rt.offsetMin=rt.offsetMax=Vector2.zero;
        }
        private static UnityEngine.UI.Image Panel(string name,Transform parent,Vector2 size,Vector2 pos,Sprite sprite)
        {
            var rt=Rect(name,parent,size,pos);
            var image=rt.gameObject.AddComponent<UnityEngine.UI.Image>();image.sprite=sprite;image.type=UnityEngine.UI.Image.Type.Sliced;
            image.color=Color.white;image.raycastTarget=false;return image;
        }
        private static UnityEngine.UI.Text Label(string name,Transform parent,string value,Vector2 size,Vector2 pos,int fontSize,bool bold=false)
        {
            var rt=Rect(name,parent,size,pos);var text=rt.gameObject.AddComponent<UnityEngine.UI.Text>();
            Text(text,size,pos,fontSize,bold);text.text=value;return text;
        }
        private static void Text(UnityEngine.UI.Text text,Vector2 size,Vector2 pos,int fontSize,bool bold=false)
        {
            Undo.RecordObject(text,"UI-A1 typography");
            Center(text.rectTransform,size,pos);text.font=font;text.fontSize=fontSize;
            text.fontStyle=bold ? FontStyle.Bold : FontStyle.Normal;text.color=Ink;
            text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;text.resizeTextForBestFit=false;
            text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;
        }
        private static void Reparent(Component child,Transform parent)
        {
            Undo.SetTransformParent(child.transform,parent,"UI-A1 grouping");
        }
        private static void StyleButton(UnityEngine.UI.Button button,Vector2 size,Vector2 pos,string label,bool accent)
        {
            Undo.RecordObject(button,"UI-A1 button");
            Center(button.GetComponent<RectTransform>(),size,pos);
            var image=button.GetComponent<UnityEngine.UI.Image>();Undo.RecordObject(image,"UI-A1 button art");
            image.sprite=accent ? primary : secondary;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Color.white;image.raycastTarget=true;
            var colors=button.colors;
            colors.normalColor=Color.white;colors.highlightedColor=new Color(1,.95f,.83f);
            colors.pressedColor=new Color(.82f,.69f,.50f);colors.selectedColor=new Color(1,.98f,.86f);
            colors.disabledColor=new Color(.70f,.68f,.63f);colors.fadeDuration=.10f;button.colors=colors;
            var text=button.GetComponentInChildren<UnityEngine.UI.Text>(true);
            Undo.RecordObject(text,"UI-A1 button copy");
            text.font=font;text.fontSize=28;text.fontStyle=FontStyle.Bold;text.color=Ink;text.alignment=TextAnchor.MiddleCenter;
            text.raycastTarget=false;text.resizeTextForBestFit=false;text.text=label;
            var focus=button.gameObject.AddComponent<UnityEngine.UI.Outline>();focus.effectColor=Hex("#17366B");focus.effectDistance=new Vector2(5,-5);focus.enabled=false;
            var cue=button.gameObject.AddComponent<UiFocusOutline>();
            var so=new SerializedObject(cue);so.FindProperty("focusOutline").objectReferenceValue=focus;so.ApplyModifiedProperties();
        }
        private static UnityEngine.UI.Button Button(string name,Transform parent,Vector2 size,Vector2 pos,string label,bool accent)
        {
            var rt=Rect(name,parent,size,pos);rt.gameObject.AddComponent<UnityEngine.UI.Image>();
            var button=rt.gameObject.AddComponent<UnityEngine.UI.Button>();button.targetGraphic=rt.GetComponent<UnityEngine.UI.Image>();
            var text=Label(name+"_Label",rt,label,size,Vector2.zero,28,true);
            Full(text.rectTransform);text.rectTransform.offsetMin=new Vector2(12,8);text.rectTransform.offsetMax=new Vector2(-12,-8);
            StyleButton(button,size,pos,label,accent);return button;
        }
        private static T Field<T>(ClickCookPrototypeController c,string name) =>
            (T)typeof(ClickCookPrototypeController).GetField(name,System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance).GetValue(c);
        private static void Nav(UnityEngine.UI.Button b,UnityEngine.UI.Button left,UnityEngine.UI.Button right,UnityEngine.UI.Button up,UnityEngine.UI.Button down)
        {
            var n=b.navigation;n.mode=UnityEngine.UI.Navigation.Mode.Explicit;n.selectOnLeft=left;n.selectOnRight=right;n.selectOnUp=up;n.selectOnDown=down;b.navigation=n;
        }

        public static string Build()
        {
            if(EditorApplication.isPlaying) throw new Exception("Stop Play Mode before authoring.");
            var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if(scene.path!="Assets/Scenes/SampleScene.unity" || scene.isDirty) throw new Exception("Expected clean SampleScene.");
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var selection=Field<GameObject>(c,"locationSelectionScreen").GetComponent<RectTransform>();
            if(selection.Find("UIA1_LocationPaper")!=null) throw new Exception("UI-A1 already authored.");
            font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if(!"ҐґЄєІіЇї’0123456789".All(font.HasCharacter)) throw new Exception("Font lacks Ukrainian glyphs.");
            Folder("Assets/UI/Art");Folder("Assets/UI/Prefabs/Components");
            paper=Texture("PaperPanel",false,false);primary=Texture("PrimaryButton",false,true);
            secondary=paper;background=Texture("MenuBlue",true,false);

            Full(selection);var rootImage=selection.GetComponent<UnityEngine.UI.Image>();
            Undo.RecordObject(rootImage,"UI-A1 background");rootImage.sprite=background;rootImage.type=UnityEngine.UI.Image.Type.Simple;rootImage.color=Color.white;
            var card=Panel("UIA1_LocationPaper",selection,new Vector2(1320,760),new Vector2(0,-30),paper);
            card.transform.SetAsFirstSibling();
            var title=selection.Find("ClickCook_Prototype_LocationSelection_Title").GetComponent<UnityEngine.UI.Text>();
            Text(title,new Vector2(600,80),new Vector2(0,420),48,true);title.text="ЛОКАЦІЇ";title.color=Hex("#FFF0CE");
            var wallet=Field<UnityEngine.UI.Text>(c,"progressText");
            Reparent(wallet,selection);Text(wallet,new Vector2(470,70),new Vector2(-570,420),28,true);wallet.text="Баланс: 0,00 у.е.";
            var badge=Panel("UIA1_WalletBadge",selection,new Vector2(500,84),new Vector2(-570,420),paper);badge.transform.SetSiblingIndex(wallet.transform.GetSiblingIndex());
            var open=Field<UnityEngine.UI.Button>(c,"openUpgradesButton");Reparent(open,selection);
            StyleButton(open,new Vector2(290,76),new Vector2(605,420),"Покращення",false);
            var name=Field<UnityEngine.UI.Text>(c,"locationNameText");Reparent(name,card.transform);Text(name,new Vector2(700,110),new Vector2(265,235),42,true);name.text="Гуртожиток";
            var level=Field<UnityEngine.UI.Text>(c,"locationLevelText");Reparent(level,card.transform);Text(level,new Vector2(700,50),new Vector2(265,150),30);level.text="Рівень локації: 0";
            var income=Field<UnityEngine.UI.Text>(c,"incomeText");Reparent(income,card.transform);Text(income,new Vector2(700,65),new Vector2(265,60),30,true);income.text="Дохід локації: 0 у.е./с";
            var hint=Label("UIA1_IncomeHint",card.transform,"Нарахування пасивного доходу — незабаром",new Vector2(690,50),new Vector2(265,5),24);hint.color=Muted;
            var dishes=Field<UnityEngine.UI.Text>(c,"dishSlotsText");Reparent(dishes,card.transform);Text(dishes,new Vector2(650,160),new Vector2(265,-110),28);dishes.text="Страви локації: 2\nСендвічі · Кекси у чашці";
            var cook=Field<UnityEngine.UI.Button>(c,"cookButton");Reparent(cook,card.transform);StyleButton(cook,new Vector2(360,82),new Vector2(265,-270),"Готувати!",true);
            var prev=Field<UnityEngine.UI.Button>(c,"previousButton");Reparent(prev,selection);StyleButton(prev,new Vector2(90,110),new Vector2(-765,-30),"‹",false);
            var next=Field<UnityEngine.UI.Button>(c,"nextButton");Reparent(next,selection);StyleButton(next,new Vector2(90,110),new Vector2(765,-30),"›",true);
            prev.GetComponentInChildren<UnityEngine.UI.Text>().fontSize=64;next.GetComponentInChildren<UnityEngine.UI.Text>().fontSize=64;
            var day=Field<UnityEngine.UI.Text>(c,"finishDayMessageText");Reparent(day,selection);Text(day,new Vector2(1400,86),new Vector2(0,-460),26);day.color=Hex("#FFF0CE");

            // Clean vector-style venue placeholder, authored from UI shapes, not a screenshot crop.
            var venue=Rect("UIA1_VenueIllustration",card.transform,new Vector2(450,430),new Vector2(-390,0));
            var silhouette=Panel("VenueBody",venue,new Vector2(330,270),new Vector2(0,-25),paper);silhouette.color=Hex("#F1BB60");
            var roof=Panel("VenueRoof",venue,new Vector2(370,62),new Vector2(0,135),primary);
            var sign=Label("VenueSign",venue,"ClickCook",new Vector2(270,50),new Vector2(0,88),34,true);
            for(int row=0;row<2;row++) for(int col=0;col<4;col++)
            {
                var window=Panel("Window_"+row+"_"+col,venue,new Vector2(45,50),new Vector2(-105+col*70,15-row*80),paper);
                window.color=Hex("#5175A4");
            }
            var door=Panel("VenueDoor",venue,new Vector2(62,88),new Vector2(0,-115),paper);door.color=Hex("#8B5835");
            var ground=Panel("VenueGround",venue,new Vector2(400,18),new Vector2(0,-163),paper);ground.color=Hex("#729C3D");
            Label("VenuePlaceholderLabel",card.transform,"Стилізована ілюстрація",new Vector2(420,42),new Vector2(-390,-250),24).color=Muted;

            var upgrades=Field<GameObject>(c,"upgradeScreen").GetComponent<RectTransform>();
            Full(upgrades);var ui=upgrades.GetComponent<UnityEngine.UI.Image>();Undo.RecordObject(ui,"UI-A1 upgrade background");
            ui.sprite=background;ui.type=UnityEngine.UI.Image.Type.Simple;ui.color=Color.white;
            var upgradePaper=Panel("UIA1_UpgradePaper",upgrades,new Vector2(1380,850),Vector2.zero,paper);upgradePaper.transform.SetAsFirstSibling();
            var upgradeTitle=upgrades.Find("ClickCook_Prototype_Upgrades_Title").GetComponent<UnityEngine.UI.Text>();
            Text(upgradeTitle,new Vector2(1100,70),new Vector2(0,350),48,true);upgradeTitle.text="ПОКРАЩЕННЯ";
            var details=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");Reparent(details,upgradePaper.transform);
            Text(details,new Vector2(620,310),new Vector2(-335,65),30);details.text="Баланс: 0,00 у.е.\n\nРанг: 0 / 15\nСила кліку: 1 → 2\n\nЦіна: 20,00 у.е.\nНедостатньо коштів";
            Label("UIA1_ClickStrengthTitle",upgradePaper.transform,"СИЛА КЛІКУ",new Vector2(620,65),new Vector2(-335,260),38,true);
            var futureTitle=upgrades.Find("ClickCook_Prototype_Upgrade_UnavailableTitle").GetComponent<UnityEngine.UI.Text>();Reparent(futureTitle,upgradePaper.transform);
            Text(futureTitle,new Vector2(590,70),new Vector2(335,260),30,true);futureTitle.text="Інші покращення — незабаром";
            var left=upgrades.Find("ClickCook_Prototype_Upgrade_UnavailableLeft").GetComponent<UnityEngine.UI.Text>();Reparent(left,upgradePaper.transform);
            Text(left,new Vector2(280,330),new Vector2(175,55),24);
            left.text="Частота критів\nМножник криту\nУтримання кліку\nСила автокліку\nЧастота автокліку\nВартість страв\nБонус вибору\nЦіни рецептів";
            var right=upgrades.Find("ClickCook_Prototype_Upgrade_UnavailableRight").GetComponent<UnityEngine.UI.Text>();Reparent(right,upgradePaper.transform);
            Text(right,new Vector2(280,330),new Vector2(475,55),24);
            right.text="Ціни покращень\nМножник доходу\nДосвід гравця\nДосвід ресторану\nДосвід страви\nПасивний дохід\nЧас вибору";
            var divider=Panel("UIA1_UpgradeDivider",upgradePaper.transform,new Vector2(2,460),new Vector2(0,20),paper);divider.color=Hex("#D8B56E");
            var buy=Field<UnityEngine.UI.Button>(c,"buyUpgradeButton");Reparent(buy,upgradePaper.transform);
            StyleButton(buy,new Vector2(570,82),new Vector2(-335,-215),"Придбати за 20,00 у.е.",true);
            var close=Field<UnityEngine.UI.Button>(c,"closeUpgradesButton");Reparent(close,upgradePaper.transform);
            StyleButton(close,new Vector2(300,82),new Vector2(335,-215),"До локацій",false);
            var status=Field<UnityEngine.UI.Text>(c,"upgradeStatusText");Reparent(status,upgradePaper.transform);
            Text(status,new Vector2(1250,70),new Vector2(0,-340),26);status.color=Muted;
            status.text="Покращення діють у поточному запуску гри. Збереження — незабаром.";

            Nav(prev,next,next,open,cook);Nav(next,prev,prev,open,cook);
            Nav(cook,prev,next,open,open);Nav(open,prev,next,cook,cook);
            Nav(buy,close,close,close,close);Nav(close,buy,buy,buy,buy);
            var panelTemplate=new GameObject("PaperPanel",typeof(RectTransform),typeof(UnityEngine.UI.Image));
            Center(panelTemplate.GetComponent<RectTransform>(),new Vector2(800,500),Vector2.zero);
            var panelImage=panelTemplate.GetComponent<UnityEngine.UI.Image>();panelImage.sprite=paper;
            panelImage.type=UnityEngine.UI.Image.Type.Sliced;panelImage.raycastTarget=false;
            PrefabUtility.SaveAsPrefabAsset(panelTemplate,"Assets/UI/Prefabs/Components/PaperPanel.prefab");
            UnityEngine.Object.DestroyImmediate(panelTemplate);
            PrefabUtility.SaveAsPrefabAsset(cook.gameObject,"Assets/UI/Prefabs/Components/PrimaryButton.prefab");
            PrefabUtility.SaveAsPrefabAsset(open.gameObject,"Assets/UI/Prefabs/Components/SecondaryButton.prefab");
            PrefabUtility.SaveAsPrefabAsset(badge.gameObject,"Assets/UI/Prefabs/Components/WalletBadge.prefab");
            AssetDatabase.SaveAssets();
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
            return "UI-A1 authored: two screens, three original procedural sprites, four component prefabs; bindings preserved.";
        }
    }
}
