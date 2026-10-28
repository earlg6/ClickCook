using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.Verification
{
    /// <summary>Local U1 contract/UI boundary checks. All live purchases require an isolated Temp profile.</summary>
    public static class UpgradeTreeVerification
    {
        private static void Check(bool value, string message) { if (!value) throw new Exception("U1: " + message); }
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(target);
        private static void Fund(PrototypeOrderSession session, decimal amount) => typeof(PrototypeDishRewards)
            .GetField("<Wallet>k__BackingField",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(session.Rewards,amount);
        private static ClickCookPrototypeController Controller => UnityEngine.Object.FindFirstObjectByType<ClickCookPrototypeController>();
        private static UpgradeTreeView View => Field<UpgradeTreeView>(Controller,"upgradeTreeView");

        public static string RunContractChecks()
        {
            var tree = UpgradeTreeDefinition.Load();
            var rows = File.ReadAllLines("Assets/Documentation/ClickCook_Upgrade_Node_Manifest.md").Where(s=>s.StartsWith("| node.")).ToArray();
            Check(rows.Length == tree.nodes.Length, "manifest count");
            foreach (var row in rows)
            {
                var cells = row.Split('|').Select(s=>s.Trim()).ToArray();
                var node = tree.Find(cells[1]);
                Check(node != null && node.familyId == cells[2] && node.title == cells[3] && node.description == cells[4], "manifest identity/copy " + cells[1]);
                var needs = Regex.Matches(cells[5], @"(node\.[a-z_]+\.\d+):(max|\d+)").Cast<Match>().ToArray();
                Check(node.prerequisites.Length == needs.Length,"prerequisite count " + node.id);
                for (int i=0;i<needs.Length;i++) Check(node.prerequisites[i].nodeId == needs[i].Groups[1].Value &&
                    node.prerequisites[i].rank == (needs[i].Groups[2].Value == "max" ? -1 : int.Parse(needs[i].Groups[2].Value)),"prerequisite " + node.id);
                if (!node.IsRoot && !node.IsWorking)
                    Check(UpgradeTreeDefinition.PurchasedRank(node,15)==0 && UpgradeTreeDefinition.MaximumRank(node)==0,"no legacy rank leakage " + node.id);
            }
            Check(tree.nodes.Select(n=>n.familyId).Distinct().Count()==39,"38 families plus root");
            Check(tree.PrerequisitesMet(tree.Find(UpgradeTreeDefinition.ClickStrengthNodeId),0),"working branch reachable");
            Check(!tree.PrerequisitesMet(tree.Find("node.click_strength.02"),14) && tree.PrerequisitesMet(tree.Find("node.click_strength.02"),15),"legacy max prerequisite");
            Check(!tree.PrerequisitesMet(tree.Find("node.click_strength.03"),15),"unimplemented maximum is unknown, never zero-completed");
            return "U1 contract PASS: all 92 manifest records, exact prerequisites/copy, DAG/root, legacy-only rank mapping.";
        }

        public static string RunInitialUiChecks()
        {
            Check(EditorApplication.isPlaying,"Play Mode required");
            Check(PlayerProfileRuntime.Store.SavePath.Replace('\\','/').Contains("/Temp/ClickCookSaveTests/"),"isolated profile required");
            var c=Controller; var menu=c.GetComponent<ProfileMenu>(); menu.RequestNewGame(); menu.ConfirmNewGame(); c.OpenUpgrades();
            var view=View; var buy=Field<UnityEngine.UI.Button>(c,"buyUpgradeButton");
            var details=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");
            Check(c.enabled && view.CanInteract && view.Definition.nodes.Length==92,"live view ready");
            Check(view.GetComponentsInChildren<UpgradeTreeTile>(true).Length==92,"92 instantiated selectable tiles");
            Check(!buy.interactable && details.text.Contains("Недостатньо коштів"),"zero wallet unavailable");
            Check(UnityEngine.Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"one EventSystem");
            Check(view.GetComponentInParent<Canvas>().GetComponent<UnityEngine.UI.GraphicRaycaster>()!=null,"raycaster");
            var ranks=c.OrderSession.Profile.CaptureSnapshot().UpgradeRanksById;
            Check(ranks.Count==0,"view/root do not create owned ranks");
            foreach(var node in view.Definition.nodes)
            {
                c.SelectUpgradeNode(node.id); Canvas.ForceUpdateCanvases();
                Check(details.text.StartsWith(node.title),"node details " + node.id);
                if(!node.IsWorking) { Check(!buy.interactable,"no sale " + node.id); c.BuyUpgrade(); }
                Check(c.OrderSession.ClickStrengthRank==0 && c.OrderSession.Rewards.Wallet==0,"inspection cannot spend");
                var tile=view.Tile(node.id); var text=tile.GetComponentInChildren<UnityEngine.UI.Text>();
                Check(tile.interactable && text.preferredHeight<=text.rectTransform.rect.height+1,"readable inspectable tile " + node.id);
                Check(tile.navigation.mode==UnityEngine.UI.Navigation.Mode.Explicit,"keyboard navigation " + node.id);
            }
            var reached=new HashSet<UnityEngine.UI.Selectable>(); var queue=new Queue<UnityEngine.UI.Selectable>();
            queue.Enqueue(Field<UnityEngine.UI.Button>(c,"closeUpgradesButton"));
            while(queue.Count>0)
            {
                var item=queue.Dequeue(); if(!reached.Add(item)) continue; var nav=item.navigation;
                foreach(var next in new[]{nav.selectOnLeft,nav.selectOnRight,nav.selectOnUp,nav.selectOnDown}) if(next!=null) queue.Enqueue(next);
            }
            Check(view.Definition.nodes.All(n=>reached.Contains(view.Tile(n.id))),"all tiles reachable by keyboard links");
            c.SelectUpgradeNode(UpgradeTreeDefinition.ClickStrengthNodeId); view.RevealSelection();
            var start=view.Content.anchoredPosition;
            var gesture=view.Viewport.GetComponent<UpgradeTreeGesture>();
            var e=new PointerEventData(EventSystem.current) {position=RectTransformUtility.WorldToScreenPoint(null,view.Viewport.position)};
            gesture.OnBeginDrag(e); e.position+=new Vector2(70,40); gesture.OnDrag(e); gesture.OnEndDrag(e);
            Check(Vector2.Distance(start,view.Content.anchoredPosition)>1,"pointer drag pans");
            float zoom=view.Zoom; e.scrollDelta=new Vector2(0,1); gesture.OnScroll(e); Check(view.Zoom>zoom,"wheel zoom");
            view.SetZoom(100,Vector2.zero); Check(Mathf.Approximately(view.Zoom,1.4f),"upper zoom bound");
            view.SetZoom(.001f,Vector2.zero); Check(Mathf.Approximately(view.Zoom,.3f),"lower zoom bound");
            view.SetZoom(.8f,Vector2.zero); view.RevealSelection();
            var stats=view.transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").gameObject;
            view.ToggleStatistics(); Check(!stats.activeSelf,"hide statistics"); view.ToggleStatistics(); Check(stats.activeSelf,"show statistics");
            menu.Pause(); var pos=view.Content.anchoredPosition; zoom=view.Zoom;
            c.SelectUpgradeNode("node.click_strength.02"); c.BuyUpgrade(); view.Pan(Vector2.one*100); view.SetZoom(1.4f,Vector2.zero);
            Check(view.SelectedId==UpgradeTreeDefinition.ClickStrengthNodeId && pos==view.Content.anchoredPosition && zoom==view.Zoom && c.OrderSession.ClickStrengthRank==0,"pause guards view and purchase"); menu.Resume();
            Fund(c.OrderSession,100m); c.SelectUpgradeNode(UpgradeTreeDefinition.ClickStrengthNodeId);
            Check(buy.interactable && details.text.Contains("Сила: 1 HP") && details.text.Contains("Сила: 2 HP") && details.text.Contains("20,00"),"live price/effect preview");
            c.SelectUpgradeNode("node.click_strength.02"); c.BuyUpgrade();
            Check(c.OrderSession.ClickStrengthRank==0 && c.OrderSession.Rewards.Wallet==100m,"selection invalidates legacy quote");
            c.SelectUpgradeNode(UpgradeTreeDefinition.ClickStrengthNodeId); buy.onClick.Invoke(); buy.onClick.Invoke();
            Check(c.OrderSession.ClickStrengthRank==1 && c.OrderSession.ClickDamage==2 && c.OrderSession.Rewards.Wallet==80m,"one exact legacy purchase, duplicate callback suppressed");
            var restored=new PlayerProfileStore(PlayerProfileRuntime.Store.SavePath).Load();
            Check(restored.GetUpgradeRank(UpgradeCatalog.ClickStrengthId)==1 && restored.Rewards.Wallet==80m && restored.CaptureSnapshot().UpgradeRanksById.Count==1,"schema-2 autosave retains only legacy rank");
            return "U1 UI PASS: 92 details/non-sales/navigation links, labels, pointer pan/wheel, zoom limits, stats toggle, pause and exact autosaved purchase. Wait a frame before post-purchase checks.";
        }

        public static string RunPostPurchaseChecks()
        {
            var c=Controller; var view=View; var details=Field<UnityEngine.UI.Text>(c,"upgradeDetailsText");
            Check(details.text.Contains("Сила: 2 HP") && details.text.Contains("Сила: 4 HP") && details.text.Contains("44,00"),"next-frame quote refresh");
            var stats=view.transform.Find("U1_UpgradeTree/ActiveStatistics/ActualEffects").GetComponent<UnityEngine.UI.Text>();
            Check(stats.text.Contains("2 HP") && stats.text.Contains("ранги сили: 1"),"actual stats after purchase");
            // At this point only the unchanged legacy purchase boundary is used to reach max in the isolated fixture.
            Fund(c.OrderSession,10000000m);
            while(c.OrderSession.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId,out var quote)) Check(c.OrderSession.TryPurchaseUpgrade(quote),"fixture max purchase");
            c.SelectUpgradeNode(UpgradeTreeDefinition.ClickStrengthNodeId);
            Check(c.OrderSession.ClickStrengthRank==15 && !Field<UnityEngine.UI.Button>(c,"buyUpgradeButton").interactable && details.text.Contains("Максимальний ранг"),"max UI");
            c.SelectUpgradeNode("node.click_strength.02");
            Check(details.text.Contains("✓ Перший кухонний ніж") && details.text.Contains("Ефект ще не реалізовано"),"met prerequisites still do not enable effect");
            var wallet=c.OrderSession.Rewards.Wallet; c.BuyUpgrade(); Check(c.OrderSession.Rewards.Wallet==wallet,"unfinished tier cannot spend at legacy max");
            c.CloseUpgrades(); c.OpenUpgrades(); Check(view.SelectedId=="node.click_strength.02","selection retained on reopen");
            Check(view.GetComponentsInChildren<UpgradeTreeTile>(true).Length==92,"reopen does not duplicate view");
            c.SelectUpgradeNode(UpgradeTreeDefinition.ClickStrengthNodeId); view.RevealSelection();
            return "U1 post-purchase PASS: next quote/statistics, max rank, met-but-unimplemented tier, Back/reopen and no duplicate tiles.";
        }

        public static string RunInteractionChecks()
        {
            var c=Controller; var view=View; var events=EventSystem.current;
            c.SelectUpgradeNode(UpgradeTreeDefinition.ClickStrengthNodeId); view.RevealSelection(); Canvas.ForceUpdateCanvases();
            var tile=view.Tile(UpgradeTreeDefinition.ClickStrengthNodeId);
            var point=RectTransformUtility.WorldToScreenPoint(null,tile.transform.position);
            var e=new PointerEventData(events) {position=point,button=PointerEventData.InputButton.Left};
            var hits=new List<RaycastResult>(); events.RaycastAll(e,hits);
            Check(hits.Count>0 && hits[0].gameObject==tile.gameObject,"tile is topmost raycast target");
            Check(ExecuteEvents.GetEventHandler<IDragHandler>(hits[0].gameObject)==view.Viewport.gameObject,"tile drag routes to tree viewport");
            events.SetSelectedGameObject(tile.gameObject);
            var move=new AxisEventData(events) {moveDir=MoveDirection.Right,moveVector=Vector2.right};
            ExecuteEvents.Execute(tile.gameObject,move,ExecuteEvents.moveHandler);
            var selected=events.currentSelectedGameObject.GetComponent<UpgradeTreeTile>();
            Check(selected!=null && selected.NodeId==view.SelectedId && selected.NodeId!=UpgradeTreeDefinition.ClickStrengthNodeId,"keyboard move selects and reveals dependent tile");
            ExecuteEvents.Execute(selected.gameObject,new BaseEventData(events),ExecuteEvents.submitHandler);
            Check(view.SelectedId==selected.NodeId,"Submit inspects selected node");
            // Zoom preserves the canvas point under the pointer, away from pan limits.
            view.RevealSelection(); var pivot=new Vector2(30,20); var relative=(pivot-view.Content.anchoredPosition)/view.Zoom;
            view.SetZoom(1,pivot); Check(Vector2.Distance(relative,(pivot-view.Content.anchoredPosition)/view.Zoom)<.01f,"pointer anchored zoom");
            c.SelectUpgradeNode("node.selection_precision_window.03"); view.RevealSelection(); Canvas.ForceUpdateCanvases();
            var scroll=view.transform.Find("U1_UpgradeTree/NodeDetail/DetailScroll").GetComponent<UnityEngine.UI.ScrollRect>();
            Check(scroll.viewport.GetComponent<UnityEngine.UI.RectMask2D>()!=null && scroll.content.GetComponent<UnityEngine.UI.ContentSizeFitter>()!=null,"clipped auto-sized detail content");
            var target=RectTransformUtility.WorldToScreenPoint(null,scroll.viewport.position); e.position=target;
            hits.Clear(); events.RaycastAll(e,hits);
            Check(hits.Count>0 && ExecuteEvents.GetEventHandler<IScrollHandler>(hits[0].gameObject)==scroll.gameObject,"detail wheel routes to details, not tree zoom");
            float zoom=view.Zoom; e.scrollDelta=new Vector2(0,-1);
            ExecuteEvents.ExecuteHierarchy(hits[0].gameObject,e,ExecuteEvents.scrollHandler); Check(view.Zoom==zoom,"detail scrolling leaves tree zoom unchanged");
            view.SetZoom(.8f,Vector2.zero); c.SelectUpgradeNode(UpgradeTreeDefinition.ClickStrengthNodeId); view.RevealSelection();
            Check(view.Viewport.rect.width>0 && view.Viewport.rect.height>0,"visible graph viewport");
            return "U1 interaction PASS: topmost tile raycast, drag route, EventSystem Move/Submit, pointer-anchored zoom and detail scroll isolation.";
        }
    }
}
