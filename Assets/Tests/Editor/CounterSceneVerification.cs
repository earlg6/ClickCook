using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    public static class CounterSceneVerification
    {
        private static IEnumerator sequence;
        private static int nextFrame, assertions, spatialAssertions, expectedWidth, expectedHeight;
        public static string Report { get; private set; } = "Not run";

        private static void Assert(bool condition, string detail)
        {
            assertions++;
            if (!condition) throw new Exception(detail);
        }

        private static void Spatial(string stage)
        {
            string result = CheckCurrent(stage);
            spatialAssertions += int.Parse(result.Split(' ')[1]);
        }

        public static string BeginSceneChecks(int width=0, int height=0)
        {
            if (!EditorApplication.isPlaying || sequence != null) throw new Exception("Ready Play Mode and no running checks required.");
            assertions = spatialAssertions = 0;
            expectedWidth=width;expectedHeight=height;
            sequence = SceneChecks();
            Report = "Running rendered-frame checks";
            nextFrame = Time.frameCount;
            EditorApplication.update += Tick;
            return Report;
        }

        private static void Tick()
        {
            if (EditorApplication.isPlaying && Time.frameCount < nextFrame) return;
            try
            {
                if (!EditorApplication.isPlaying) throw new Exception("Play Mode stopped during verification.");
                if (sequence.MoveNext())
                {
                    Report = "Running: " + sequence.Current;
                    nextFrame = Time.frameCount + 3;
                    return;
                }
                Report = "PASS " + assertions + " lifecycle/input + " + spatialAssertions +
                    " rendered spatial assertions: 11 locations, Back/reset, one active order, same-board return, empty/duplicate fixtures, ten receipts, stale actions and day cleanup. EventSystem callbacks, NOT physical keyboard/mouse.";
            }
            catch (Exception error) { Report = "FAIL: " + error.Message; }
            EditorApplication.update -= Tick;
            (sequence as IDisposable)?.Dispose();
            sequence = null;
        }

        private static IEnumerator SceneChecks()
        {
            var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
            var view = c.GetComponent<CounterScenePresentation>();
            var dish = Field<GameObject>(view,"dishRoot");
            var sheets = Field<UnityEngine.UI.Button[]>(view,"sheets");
            var back = Field<UnityEngine.UI.Button>(c,"backToBoardButton");
            for (int level = 0; level < 11; level++)
            {
                UiA2Verification.ShowBoard(level);
                yield return "board " + level;
                Spatial("board");
                var orders = c.OrderSession.Orders.ToArray();
                Assert(c.OrderSession.SelectedLocation.LevelRequirement == level,"selected location handoff");
                Assert(orders.All(o => c.OrderSession.SelectedLocation.DocumentedAvailableDishes.Contains(o.Dish)),"candidate pool membership");
                Assert(orders.Select(o => o.Identity).Distinct().Count() == orders.Length,"independent order identities");
                ExecuteEvents.Execute(sheets[0].gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerClickHandler);
                var selected = c.OrderSession.ActiveOrder;
                Assert(selected == orders[0],"paper card hands off exact instance");
                c.SelectOrder(1);
                Assert(c.OrderSession.ActiveOrder == selected,"second selection rejected");
                yield return "cooking " + level;
                Spatial("cooking");
                Assert(EventSystem.current.currentSelectedGameObject == dish,"dish receives keyboard focus");
                ExecuteEvents.Execute(dish,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Down},ExecuteEvents.moveHandler);
                Assert(EventSystem.current.currentSelectedGameObject == back.gameObject,"dish to Back navigation");
                ExecuteEvents.Execute(back.gameObject,new AxisEventData(EventSystem.current){moveDir=MoveDirection.Up},ExecuteEvents.moveHandler);
                Assert(EventSystem.current.currentSelectedGameObject == dish,"Back to dish navigation");
                if (selected.RemainingClicks > c.OrderSession.ClickDamage)
                {
                    int hp = selected.RemainingClicks;
                    ExecuteEvents.Execute(dish,new BaseEventData(EventSystem.current),ExecuteEvents.submitHandler);
                    Assert(selected.RemainingClicks == hp-c.OrderSession.ClickDamage,"dish Submit cooks once");
                }
                back.onClick.Invoke();
                yield return "Back " + level;
                Spatial("board");
                Assert(orders.SequenceEqual(c.OrderSession.Orders),"Back keeps every exact order");
                Assert(c.OrderSession.CompletedDishes == 0 && c.OrderSession.ActiveOrder == null,"Back does not complete");
                c.SelectOrder(0);
                Assert(selected.RemainingClicks == selected.Dish.ClickRequirement,"reselect restores full HP");
                c.BackToBoard();
                c.FinishDay();
                yield return "Finish " + level;
                Spatial("menu");
                Assert(!c.OrderSession.IsDayActive && c.OrderSession.Orders.Count == 0,"Finish clears temporary orders");
            }

            UiA2Verification.ShowBoard(0);
            var empty = ScriptableObject.CreateInstance<ClickCook.Data.LocationData>();
            try
            {
                c.OrderSession.StartDay(empty,2);
                typeof(ClickCookPrototypeController).GetMethod("RefreshOrderBoard",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
                typeof(ClickCookPrototypeController).GetMethod("FocusBoard",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
                yield return "empty board";
                Spatial("board");
                Assert(c.OrderSession.Orders.Count == 0 && sheets.All(b=>!b.interactable),"empty pool has no selectable fake orders");
                c.FinishDay();
                yield return "empty Finish";
                Spatial("menu");
            }
            finally { UnityEngine.Object.DestroyImmediate(empty); }

            UiA2Verification.ShowBoard(0);
            var duplicate = ScriptableObject.CreateInstance<ClickCook.Data.LocationData>();
            try
            {
                var fixture = new SerializedObject(duplicate);
                var pool = fixture.FindProperty("documentedAvailableDishes");
                pool.arraySize=1;
                pool.GetArrayElementAtIndex(0).objectReferenceValue=c.OrderSession.SelectedLocation.DocumentedAvailableDishes[0];
                fixture.ApplyModifiedPropertiesWithoutUndo();
                c.OrderSession.StartDay(duplicate,2);
                typeof(ClickCookPrototypeController).GetMethod("RefreshOrderBoard",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(c,null);
                yield return "independent duplicates";
                Spatial("board");
                Assert(c.OrderSession.Orders[0].Dish==c.OrderSession.Orders[1].Dish &&
                    c.OrderSession.Orders[0].Identity!=c.OrderSession.Orders[1].Identity,"same dish / separate identities");
                c.FinishDay();
            }
            finally { UnityEngine.Object.DestroyImmediate(duplicate); }

            UiA2Verification.ShowBoard(0);
            for (int completion = 1; completion <= 10; completion++)
            {
                yield return "completion board " + completion;
                Spatial("board");
                var original = c.OrderSession.GetOrderAtSlot(0);
                c.SelectOrder(0);
                yield return "completion cooking " + completion;
                Spatial("cooking");
                UiA2Verification.ShowResult();
                yield return "result " + completion;
                Spatial("result");
                var wallet = c.OrderSession.Rewards.Wallet;
                c.ClickCookingDish();c.SelectOrder(1);c.FinishDay();
                Assert(c.OrderSession.CompletedDishes==completion && c.OrderSession.Rewards.Wallet==wallet,"stale actions cannot pay twice/end result");
                Assert(completion==10 ? c.OrderSession.GetOrderAtSlot(0)==null : c.OrderSession.GetOrderAtSlot(0)!=original,"replacement before cap only");
                c.ContinueFromDishResult();
                Assert(c.OrderSession.Rewards.Wallet==wallet,"Continue does not repeat payout");
            }
            yield return "tenth Continue cleanup";
            Spatial("menu");
            Assert(!c.OrderSession.IsDayActive && c.OrderSession.Orders.Count==0,"tenth Continue clears day");
        }
        private static T Field<T>(object target,string key)=>(T)target.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        public static string CheckCurrent(string stage)
        {
            int checks=0;void Check(bool ok,string detail){checks++;if(!ok)throw new Exception("Counter scene: "+detail);}
            Check(EditorApplication.isPlaying,"Play Mode required");
            var c=UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();var view=c.GetComponent<CounterScenePresentation>();
            Check(c.enabled&&view.enabled,"controller/view ready");var camera=Field<Camera>(view,"sceneCamera");
            var canvas=Field<RectTransform>(view,"boardCanvas").GetComponent<Canvas>();var dish=Field<GameObject>(view,"dishRoot");
            var collider=Field<Collider>(view,"dishCollider");
            Check(canvas.renderMode==RenderMode.WorldSpace&&canvas.worldCamera==camera,"real world-space board with explicit event camera");
            if(expectedWidth>0)
                Check(Screen.width==expectedWidth && Screen.height==expectedHeight && camera.pixelWidth==expectedWidth && camera.pixelHeight==expectedHeight,"actual rendered/input resolution");
            var screenRoot=Field<GameObject>(c,stage=="board" ? "workDayScreen" : stage=="cooking" ? "cookingPrototypeScreen" : stage=="result" ? "dishResultScreen" : "locationSelectionScreen");
            Check(screenRoot.activeInHierarchy,"expected screen active");
            foreach(var text in screenRoot.GetComponentsInChildren<UnityEngine.UI.Text>())
            {
                Check(text.preferredHeight<=text.rectTransform.rect.height+2,"HUD text clipped: "+text.name);
                Check(!text.raycastTarget,"HUD text blocks clicks: "+text.name);
                foreach(char glyph in text.text.Where(ch=>!char.IsWhiteSpace(ch)))
                    Check(text.font.HasCharacter(glyph),"missing glyph: "+glyph);
            }
            foreach(var button in screenRoot.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.interactable))
            {
                var corners=new Vector3[4];button.GetComponent<RectTransform>().GetWorldCorners(corners);
                var overlay=button.GetComponentInParent<Canvas>();
                Check(corners.All(p=>p.x>=0&&p.x<=overlay.pixelRect.width&&p.y>=0&&p.y<=overlay.pixelRect.height),"HUD button outside render bounds: "+button.name);
                var hits=new System.Collections.Generic.List<RaycastResult>();
                EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=button.transform.position},hits);
                Check(hits.Count>0&&hits[0].gameObject==button.gameObject,"HUD button raycast: "+button.name);
            }
            if(stage=="board")
            {
                Check(canvas.gameObject.activeInHierarchy&&!dish.activeInHierarchy,"board visible / actual plate empty");
                Check(!Field<Renderer>(view,"boardPaperLeft").enabled&&!Field<Renderer>(view,"boardPaperRight").enabled,"original decorative paper temporarily replaced");
                int count=0;
                foreach(var b in Field<UnityEngine.UI.Button[]>(view,"sheets"))
                {
                    if(!b.gameObject.activeInHierarchy)continue;count++;
                    Check(b.transform.localScale==Vector3.one&&Mathf.Abs(b.transform.localPosition.z)<.001f,"paper local scale/depth");
                    var point=camera.WorldToViewportPoint(b.transform.position);
                    Check(point.z>0&&point.x>0&&point.x<1&&point.y>0&&point.y<1,"sheet center visible: "+b.name+" "+point);
                    var screen=camera.WorldToScreenPoint(b.transform.position);var hits=new System.Collections.Generic.List<RaycastResult>();
                    EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=screen},hits);
                    Check(hits.Count>0&&hits[0].gameObject==b.gameObject,"sheet topmost raycast: "+b.name);
                    var corners = new Vector3[4]; b.GetComponent<RectTransform>().GetWorldCorners(corners);
                    Check(corners.Select(camera.WorldToViewportPoint).All(p=>p.z>0 && p.x>=0 && p.x<=1 && p.y>=0 && p.y<=1),"whole paper visible: "+b.name);
                    foreach(var text in b.GetComponentsInChildren<UnityEngine.UI.Text>())Check(text.preferredHeight<=text.rectTransform.rect.height+2,"paper text clipped: "+text.name);
                }
                Check(count==c.OrderSession.BoardCapacity,"existing capacity preserved");
                Check(!collider.enabled,"no dish clicks on board");
            }
            else if(stage=="cooking"||stage=="result")
            {
                Check(dish.activeInHierarchy,"actual scene dish visible");var plate=Field<Renderer>(view,"plate");
                Check(Mathf.Abs(dish.transform.position.x-plate.bounds.center.x)<.001f&&Mathf.Abs(dish.transform.position.z-plate.bounds.center.z)<.001f,"dish centered on original plate");
                Check(Mathf.Abs(dish.transform.position.y-plate.bounds.max.y-.008f)<.001f,"dish sits on plate surface");
                Check(!camera.GetComponent<CameraCursorFollow>().enabled&&camera.fieldOfView==40,"focused camera, parallax suspended");
                Check(collider.enabled==(stage=="cooking"),"click collider only while cooking");
                Check(!canvas.gameObject.activeInHierarchy,"other order sheets hidden during close-up/result");
                Check(new[]{"SandwichSketch","MugCakeSketch","GenericDishSketch"}.Count(n=>dish.transform.Find(n).gameObject.activeSelf)==1,"one appropriate visual");
                if(stage=="cooking")
                {
                    var hits=new System.Collections.Generic.List<RaycastResult>();EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=camera.WorldToScreenPoint(collider.bounds.center)},hits);
                    Check(hits.Count>0&&hits[0].gameObject==dish,"actual collider topmost raycast");
                    Check(dish.GetComponent<CounterDishPointer>()!=null,"existing cooking callback routed through physical target");
                }
            }
            else
            {
                Check(!canvas.gameObject.activeInHierarchy&&!dish.activeInHierarchy,"day visuals cleared");
                Check(Field<Renderer>(view,"boardPaperLeft").enabled&&Field<Renderer>(view,"boardPaperRight").enabled,"original paper restored");
                Check(camera.GetComponent<CameraCursorFollow>().enabled&&camera.fieldOfView==60,"original camera mode restored");
            }
            return "PASS "+checks+" spatial "+stage+" assertions (raycast tests are not native input claims).";
        }
    }
}
