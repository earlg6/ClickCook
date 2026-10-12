using System;
using System.Linq;
using System.Reflection;
using ClickCook.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    public static class UiA1Verification
    {
        private static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.FlattenHierarchy;
        private static object sizeGroup,testSize;
        private static EditorWindow gameView;
        private static int originalIndex,testIndex;
        private static int checks;
        private static T Field<T>(ClickCookPrototypeController c,string name)=>
            (T)typeof(ClickCookPrototypeController).GetField(name,Flags).GetValue(c);
        private static void Check(bool ok,string detail){checks++;if(!ok)throw new Exception("UI-A1: "+detail);}
        private static float Luminance(Color c)
        {
            float Linear(float x)=> x<=.04045f ? x/12.92f : Mathf.Pow((x+.055f)/1.055f,2.4f);
            return .2126f*Linear(c.r)+.7152f*Linear(c.g)+.0722f*Linear(c.b);
        }
        public static string SetResolution(int width,int height)
        {
            if(!EditorApplication.isPlaying) throw new Exception("Start Play Mode first; restore the temporary size before stopping.");
            if(gameView==null)
            {
                gameView=Resources.FindObjectsOfTypeAll<EditorWindow>().First(w=>w.GetType().Name=="GameView");
                var sizes=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                var instance=sizes.GetProperty("instance",Flags).GetValue(null);
                sizeGroup=sizes.GetProperty("currentGroup",Flags).GetValue(instance);
                originalIndex=(int)gameView.GetType().GetProperty("selectedSizeIndex",Flags).GetValue(gameView);
                testIndex=(int)sizeGroup.GetType().GetMethod("GetTotalCount",Flags).Invoke(sizeGroup,null);
                var type=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
                var enumType=typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                testSize=Activator.CreateInstance(type,new object[]{Enum.Parse(enumType,"FixedResolution"),width,height,"UI-A1 temporary verification"});
                sizeGroup.GetType().GetMethod("AddCustomSize",Flags).Invoke(sizeGroup,new[]{testSize});
            }
            else
            {
                testSize.GetType().GetProperty("width",Flags).SetValue(testSize,width);
                testSize.GetType().GetProperty("height",Flags).SetValue(testSize,height);
            }
            gameView.GetType().GetProperty("selectedSizeIndex",Flags).SetValue(gameView,testIndex);
            gameView.Repaint();
            if(EditorApplication.isPlaying){EditorApplication.isPaused=true;EditorApplication.Step();}
            return "Requested fixed Game View "+width+"x"+height+"; verify after next frame.";
        }
        public static string RestoreResolution()
        {
            if(gameView==null) return "No temporary size.";
            gameView.GetType().GetProperty("selectedSizeIndex",Flags).SetValue(gameView,originalIndex);
            object current=sizeGroup.GetType().GetMethod("GetGameViewSize",Flags).Invoke(sizeGroup,new object[]{testIndex});
            if(!ReferenceEquals(current,testSize)) throw new Exception("Temporary size identity changed; no removal.");
            sizeGroup.GetType().GetMethod("RemoveCustomSize",Flags).Invoke(sizeGroup,new object[]{testIndex});
            gameView.Repaint();gameView=null;sizeGroup=null;testSize=null;
            return "Restored original Game View selection and removed only the temporary test size.";
        }
        public static string LayoutAndInputChecks(int width,int height,bool upgrades)
        {
            checks=0;
            Check(EditorApplication.isPlaying,"Play Mode");
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var canvas=c.GetComponentInChildren<Canvas>(true);
            Check(canvas.pixelRect.width==width && canvas.pixelRect.height==height,"actual Canvas render size "+canvas.pixelRect);
            bool inputContextMatches=Screen.width==width && Screen.height==height;
            Check(c.enabled,"controller references");
            var screen=Field<GameObject>(c,upgrades ? "upgradeScreen" : "locationSelectionScreen");
            Check(screen.activeSelf,"screen visible");
            foreach(var text in screen.GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                Check(text.preferredHeight<=text.rectTransform.rect.height+2,"text clipped: "+text.name);
                Check(!text.raycastTarget,"decorative text blocks input: "+text.name);
                foreach(char ch in text.text.Where(ch=>!char.IsWhiteSpace(ch)))
                    Check(text.font.HasCharacter(ch),"missing glyph "+ch);
            }
            string[] buttons=upgrades ? new[]{"closeUpgradesButton","buyUpgradeButton"} :
                new[]{"previousButton","nextButton","cookButton","openUpgradesButton"};
            foreach(string id in buttons)
            {
                var b=Field<UnityEngine.UI.Button>(c,id);var rt=b.GetComponent<RectTransform>();
                var corners=new Vector3[4];rt.GetWorldCorners(corners);
                Check(corners.All(p=>p.x>=0 && p.x<=width && p.y>=0 && p.y<=height),"button offscreen: "+id);
                // MCP eval can run with Editor-default Screen dimensions despite the correct rendered Canvas.
                // Do not claim pointer coverage if Unity's input context differs from the rendered size.
                if(inputContextMatches)
                {
                    var point=RectTransformUtility.WorldToScreenPoint(null,rt.TransformPoint(rt.rect.center));
                    var pointer=new PointerEventData(EventSystem.current){position=point};
                    var hits=new System.Collections.Generic.List<RaycastResult>();
                    EventSystem.current.RaycastAll(pointer,hits);
                    Check(hits.Count>0 && hits[0].gameObject==b.gameObject,"topmost raycast: "+id);
                }
            }
            var target=Field<UnityEngine.UI.Button>(c,upgrades ? "closeUpgradesButton" : "cookButton");
            EventSystem.current.SetSelectedGameObject(target.gameObject);
            Check(target.GetComponent<UnityEngine.UI.Outline>().enabled,"visible focus outline");
            Check(target.navigation.mode==UnityEngine.UI.Navigation.Mode.Explicit,"explicit focus navigation");
            var move=new AxisEventData(EventSystem.current){moveDir=MoveDirection.Right};
            ExecuteEvents.Execute(target.gameObject,move,ExecuteEvents.moveHandler);
            bool hasOther=!upgrades || Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable;
            Check((EventSystem.current.currentSelectedGameObject!=target.gameObject)==hasOther,"focus skips disabled controls without activation");
            EventSystem.current.SetSelectedGameObject(null);
            Check(!target.GetComponent<UnityEngine.UI.Outline>().enabled,"focus clears");
            return "PASS "+checks+" glyph/layout/focus assertions at rendered Canvas "+width+"x"+height+" ("+(upgrades?"upgrades":"locations")+"). Pointer raycast: "+
                (inputContextMatches ? "PASS" : "NOT VERIFIED at this resolution (MCP Screen context mismatch)")+".";
        }
        public static string StaticChecks()
        {
            checks=0;
            var font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            Check("ҐґЄєІіЇї’0123456789".All(font.HasCharacter),"UA font coverage");
            foreach(string name in new[]{"PaperPanel","PrimaryButton","WalletBadge","SecondaryButton"})
                Check(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Prefabs/Components/"+name+".prefab")!=null,"prefab "+name);
            foreach(string name in new[]{"PaperPanel","PrimaryButton","MenuBlue"})
                Check(AssetDatabase.LoadAssetAtPath<Sprite>("Assets/UI/Art/"+name+".png")!=null,"sprite "+name);
            ColorUtility.TryParseHtmlString("#3B281B",out var ink);
            ColorUtility.TryParseHtmlString("#FFF0CE",out var paper);
            ColorUtility.TryParseHtmlString("#6A4B32",out var muted);
            ColorUtility.TryParseHtmlString("#234E90",out var blue);
            ColorUtility.TryParseHtmlString("#F3A532",out var orange);
            foreach(var pair in new[]{new[]{paper,ink},new[]{paper,muted},new[]{paper,blue},new[]{orange,ink}})
            {
                float a=Luminance(pair[0]),b=Luminance(pair[1]);
                Check((Mathf.Max(a,b)+.05f)/(Mathf.Min(a,b)+.05f)>=4.5f,"palette contrast");
            }
            return "PASS "+checks+" static asset/font/contrast checks. Texture and rendered-screen review is separate.";
        }
    }
}
