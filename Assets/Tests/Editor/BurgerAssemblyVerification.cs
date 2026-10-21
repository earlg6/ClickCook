using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    /// <summary>Rendered-frame presentation checks in a disposable fresh Play run.</summary>
    public static class BurgerAssemblyVerification
    {
        private static IEnumerator sequence;
        private static float nextTime;
        private static int frame, checks;
        public static string Report { get; private set; } = "Not run";
        private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;
        private static T Field<T>(object owner, string name) => (T)owner.GetType().GetField(name, Flags).GetValue(owner);
        private static void Check(bool condition, string label)
        { checks++; if (!condition) throw new Exception(label); }

        public static string Begin()
        {
            if (!EditorApplication.isPlaying || sequence != null) throw new Exception("Fresh Play Mode required.");
            checks = 0; frame = Time.frameCount; nextTime = Time.realtimeSinceStartup;
            sequence = Run(); Report = "Running burger checks"; EditorApplication.update += Tick;
            return Report;
        }

        private static void Tick()
        {
            if (EditorApplication.isPlaying && (Time.frameCount <= frame || Time.realtimeSinceStartup < nextTime)) return;
            try
            {
                if (!EditorApplication.isPlaying) throw new Exception("Play Mode stopped.");
                if (sequence.MoveNext())
                { frame = Time.frameCount; nextTime = Time.realtimeSinceStartup + (float)sequence.Current; return; }
                Report = "PASS " + checks + " burger animation/lifecycle/raycast checks; callbacks, not physical mouse. Includes temporary max-strength wallet fixture.";
            }
            catch (Exception error) { Report = "FAIL " + error; }
            EditorApplication.update -= Tick; (sequence as IDisposable)?.Dispose(); sequence = null;
        }

        private static int StartBurger(ClickCookPrototypeController c)
        {
            UiA2Verification.ShowBoard(2);
            for (int attempt = 0; attempt < 64; attempt++)
            {
                var order = c.OrderSession.Orders.FirstOrDefault(o => o.Dish.DishId == 6);
                if (order != null) { c.SelectOrder(order.SlotIndex); return order.SlotIndex; }
                c.FinishDay(); c.CookSelectedLocation();
            }
            throw new Exception("No burger drawn from actual location pool.");
        }

        private static IEnumerator Run()
        {
            var random = UnityEngine.Random.state;
            UnityEngine.Random.InitState(606);
            try
            {
                var c = UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
                var assembly = c.GetComponentsInChildren<BurgerAssemblyPresentation>(true).Single();
                var layers = Field<Transform[]>(assembly, "layers");
                var dishRoot = assembly.transform.parent.gameObject;
                Check(c.OrderSession.ClickStrengthRank == 0, "fresh unupgraded run");
                Check(layers.Length == 6 && layers[5].GetComponentsInChildren<MeshRenderer>(true).Length == 10, "six stages, sesame travels with top bun");
                int slot = StartBurger(c);
                yield return .3f;
                var s = c.OrderSession; var order = s.ActiveOrder;
                decimal wallet = s.Rewards.Wallet;
                var ids = s.Orders.Select(o => o.Identity).ToArray();
                Check(order.Dish.DishId == 6 && order.RemainingClicks == 42, "authored burger and HP");
                Check(assembly.gameObject.activeInHierarchy && layers.Count(l => l.gameObject.activeSelf) == 1, "bottom bun at start");
                Check(!Field<GameObject>(c.GetComponent<CounterScenePresentation>(), "genericDish").activeSelf, "fallback hidden for burger");
                ExecuteEvents.Execute(dishRoot, new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
                yield return .3f;
                Check(order.RemainingClicks == 41 && layers[1].gameObject.activeSelf && layers[1].localPosition.y > .04f, "pointer click introduces descending patty");
                Check(s.Rewards.Wallet == wallet && s.CompletedDishes == 0, "animation grants nothing");
                int clicked = 1;
                foreach (int total in new[] { 9, 17, 26, 34 })
                {
                    while (clicked < total) { c.ClickCookingDish(); clicked++; }
                    yield return .3f;
                    Check(order.RemainingClicks == 42 - total && s.ActiveOrder == order, "exact HP at " + total);
                    Check(Mathf.Abs(assembly.DisplayedProgress - total / 42f) < .0001f, "visual follows damage at " + total);
                    int expected = 1 + Mathf.CeilToInt(total / 42f * 5);
                    Check(layers.Count(l => l.gameObject.activeSelf) == expected, "stage visibility at " + total);
                    Check(s.Orders.Select(o => o.Identity).SequenceEqual(ids) && s.Rewards.Wallet == wallet, "same board and no premature payout");
                }
                string spatial = CounterSceneVerification.CheckCurrent("cooking");
                Check(spatial.StartsWith("PASS"), "current cooking collider/layout: " + spatial);
                c.BackToBoard(); yield return .3f;
                Check(!assembly.gameObject.activeInHierarchy && assembly.DisplayedProgress == 0f, "Back clears animation");
                c.SelectOrder(slot); yield return .3f;
                Check(s.ActiveOrder == order && order.RemainingClicks == 42 && layers.Count(l => l.gameObject.activeSelf) == 1, "same-order reselect resets assembly and HP");
                for (int i = 0; i < 42; i++) c.ClickCookingDish();
                yield return .3f;
                Check(order.IsCompleted && s.CompletedDishes == 1 && s.Rewards.Wallet == wallet + s.LastResult.Payout, "one completion/payout");
                Check(assembly.DisplayedProgress == 1f && layers.All(l => l.gameObject.activeSelf && l.localPosition.sqrMagnitude < .000001f), "assembled result pose");
                Check((assembly.transform.localScale - Vector3.one).sqrMagnitude < .000001f, "click squash settles");
                decimal paid = s.Rewards.Wallet; int xp = s.Rewards.GetTotalXp(order.Dish);
                ExecuteEvents.Execute(dishRoot, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                Check(s.Rewards.Wallet == paid && s.Rewards.GetTotalXp(order.Dish) == xp && s.CompletedDishes == 1, "stale Submit cannot reward");
                c.ContinueFromDishResult(); c.FinishDay(); yield return .3f;
                Check(!assembly.gameObject.activeInHierarchy && s.Orders.Count == 0, "day cleanup hides presentation");

                // Editor-only fixture: exercise a strength that crosses every visual threshold at once.
                typeof(PrototypeDishRewards).GetField("<Wallet>k__BackingField", Flags).SetValue(s.Rewards, 10000000m);
                for (int rank = 0; rank < 15; rank++)
                { Check(s.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out var quote) && s.TryPurchaseUpgrade(quote), "fixture purchase " + rank); }
                StartBurger(c); yield return .3f;
                var fast = s.ActiveOrder; c.ClickCookingDish(); yield return .3f;
                Check(fast.IsCompleted && s.CompletedDishes == 1 && assembly.DisplayedProgress == 1f && layers.All(l => l.gameObject.activeSelf), "one-hit damage assembles all stages once");
                c.ContinueFromDishResult(); c.FinishDay();
                UiA2Verification.ShowBoard(0); c.SelectOrder(0); yield return .3f;
                Check(!assembly.gameObject.activeInHierarchy && dishRoot.GetComponent<BoxCollider>().size.y == .25f, "other dishes keep original visual/collider");
                c.BackToBoard(); c.FinishDay();
            }
            finally { UnityEngine.Random.state = random; }
        }
    }
}
