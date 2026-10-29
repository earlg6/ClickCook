using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClickCook.Data;
using ClickCook.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.UI
{
    /// <summary>Upgrade presentation; all spending passes through the session's guarded node quote.</summary>
    public sealed class UpgradeTreeView : MonoBehaviour
    {
        private ClickCookPrototypeController controller;
        private UpgradeTreeDefinition tree;
        private Font font;
        private Sprite paper;
        private UnityEngine.UI.Text details, statistics, wallet, zoomLabel;
        private UnityEngine.UI.Button buy, back, statisticsToggle;
        private RectTransform viewport, content, statisticsPanel;
        private readonly Dictionary<string, UnityEngine.UI.Button> tiles = new();
        private readonly Dictionary<string, Vector2> positions = new();
        private readonly Dictionary<string, UnityEngine.UI.Text> labels = new();
        private readonly List<(UpgradeTreeNode child, UpgradeNodePrerequisite need, UnityEngine.UI.Image line)> edges = new();
        private UnityEngine.UI.Button minus, plus, home;
        private UnityEngine.UI.ScrollRect detailScroll;
        public string SelectedId { get; private set; } = UpgradeTreeDefinition.ClickStrengthNodeId;
        public UpgradeTreeDefinition Definition => tree;
        public RectTransform Viewport => viewport;
        public RectTransform Content => content;
        public float Zoom => content.localScale.x;
        public UnityEngine.UI.Button Tile(string id) => tiles[id];
        public bool CanInteract => controller != null && controller.enabled && !controller.GameplayBlocked &&
            !controller.OrderSession.IsDayActive && gameObject.activeInHierarchy;
        private static Color Hex(string value) { ColorUtility.TryParseHtmlString(value, out var color); return color; }
        private static readonly Color Ink = Hex("#342C24"), Cream = Hex("#FFF0CE"), Orange = Hex("#FFC35B");

        public void Initialize(ClickCookPrototypeController owner, UnityEngine.UI.Button closeButton,
            UnityEngine.UI.Button buyButton, UnityEngine.UI.Text detailText, UnityEngine.UI.Text statusText)
        {
            if (tree != null) return;
            controller = owner; back = closeButton; buy = buyButton; details = detailText;
            tree = UpgradeTreeDefinition.Load(); font = details.font;
            paper = back.GetComponent<UnityEngine.UI.Image>().sprite;
            var root = Rect("U1_UpgradeTree", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image(root, Hex("#142D50"), false);
            Move(back, root); Move(buy, root); Move(details, root); Move(statusText, root);
            // Retain old scene objects/bindings; the U1 view replaces their presentation at runtime.
            foreach (Transform child in transform) if (child != root) child.gameObject.SetActive(false);
            Text("Title", root, "РОЗВИТОК КУХНІ", new Vector2(24, -100), new Vector2(-310, -24), 40, Cream);
            wallet = Text("Wallet", root, "", new Vector2(24, -160), new Vector2(-520, -110), 27, Cream);
            Text("Help", root, "92 плитки • перетягування — рух • колесо / − + — масштаб\nСтрілки — вузли й кнопки • Enter — вибір", new Vector2(24, -218), new Vector2(-520, -164), 21, Cream);
            Place(back.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero, new Vector2(24, 24), new Vector2(264, 90));
            StyleExisting(back, 25);
            viewport = Rect("TreeViewport", root, Vector2.zero, Vector2.one, new Vector2(24, 120), new Vector2(-510, -230));
            Image(viewport, Hex("#1F4063"), true);
            viewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            var gesture = viewport.gameObject.AddComponent<UpgradeTreeGesture>(); gesture.View = this;
            content = Rect("TreeContent", viewport, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(.5f,.5f);
            var sidebar = Rect("NodeDetail", root, new Vector2(1, 0), Vector2.one, new Vector2(-486, 460), new Vector2(-24, -230));
            Image(sidebar, Cream, true);
            var scrollRoot = Rect("DetailScroll", sidebar, Vector2.zero, Vector2.one, new Vector2(22, 126), new Vector2(-22, -20));
            var detailViewport = Rect("Viewport", scrollRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image(detailViewport, Cream, true); detailViewport.gameObject.AddComponent<UnityEngine.UI.RectMask2D>();
            Move(details, detailViewport);
            Place(details.rectTransform, new Vector2(0,1), Vector2.one, Vector2.zero, Vector2.zero);
            details.rectTransform.pivot = new Vector2(.5f,1);
            StyleText(details, 23, Ink); details.alignment = TextAnchor.UpperLeft;
            var fit = details.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fit.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            detailScroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            detailScroll.viewport = detailViewport; detailScroll.content = details.rectTransform;
            detailScroll.horizontal = false; detailScroll.vertical = true;
            detailScroll.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
            detailScroll.scrollSensitivity = 28;
            Text("DetailScrollHint", sidebar, "Опис: колесо / Page Up / Page Down", new Vector2(22,98), new Vector2(-22,122), 17, Ink, false).rectTransform.anchorMax = new Vector2(1,0);
            Move(buy, sidebar);
            Place(buy.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1,0), new Vector2(20, 20), new Vector2(-20, 90));
            StyleExisting(buy, 23);
            statisticsPanel = Rect("ActiveStatistics", root, new Vector2(1,0), new Vector2(1,0), new Vector2(-486,120), new Vector2(-24,440));
            Image(statisticsPanel, Cream, true);
            statistics = Text("ActualEffects", statisticsPanel, "", new Vector2(18,18), new Vector2(-18,-62), 20, Ink, false);
            statisticsToggle = Button("StatisticsToggle", statisticsPanel, "Активні покращення · Сховати", Vector2.zero, Vector2.one, new Vector2(12,-56), new Vector2(-12,-8), ToggleStatistics);
            Place(statisticsToggle.GetComponent<RectTransform>(), new Vector2(0,1), Vector2.one, new Vector2(12,-56), new Vector2(-12,-8));
            Place(statusText.rectTransform, new Vector2(0,0), new Vector2(1,0), new Vector2(285,24), new Vector2(-24,94));
            StyleText(statusText, 22, Cream);
            minus = Button("ZoomOut", root, "−", Vector2.zero, Vector2.zero, new Vector2(24, -285), new Vector2(90,-225), () => SetZoom(Zoom / 1.2f, Vector2.zero));
            plus = Button("ZoomIn", root, "+", Vector2.zero, Vector2.zero, new Vector2(96,-285), new Vector2(162,-225), () => SetZoom(Zoom * 1.2f, Vector2.zero));
            home = Button("CenterSelection", root, "До вибраного", Vector2.zero, Vector2.zero, new Vector2(168,-285), new Vector2(414,-225), RevealSelection);
            // Toolbar floats above the canvas, outside the clipped graph.
            foreach (var control in new[] {minus, plus, home})
            {
                var rt = control.GetComponent<RectTransform>(); var low = rt.offsetMin; var high = rt.offsetMax;
                Place(rt, new Vector2(0,1), new Vector2(0,1), low, high);
            }
            // Viewport starts below toolbar.
            viewport.offsetMax = new Vector2(-510, -300);
            zoomLabel = Text("ZoomValue", root, "", new Vector2(430,-285), new Vector2(-510,-225), 22, Cream);
            BuildGraph();
            WireNavigation();
            content.localScale = Vector3.one * .8f;
        }

        private void BuildGraph()
        {
            var columns = tree.nodes.GroupBy(n => tree.Depth(n)).OrderBy(g => g.Key).ToArray();
            int maxRows = columns.Max(c => c.Count());
            var size = new Vector2(columns.Length * 300 + 80, maxRows * 140 + 80);
            content.sizeDelta = size;
            // Explicit dependency depth determines columns. Stable family order keeps tier chains readable.
            foreach (var column in columns)
            {
                var nodes = column.OrderBy(n => n.familyId, StringComparer.Ordinal).ThenBy(n => n.id, StringComparer.Ordinal).ToArray();
                for (int row = 0; row < nodes.Length; row++)
                    positions[nodes[row].id] = new Vector2(160 + column.Key * 300 - size.x / 2, ((nodes.Length - 1) * .5f - row) * 140);
            }
            foreach (var node in tree.nodes)
                foreach (var need in node.prerequisites)
                {
                    Vector2 start = positions[need.nodeId] + new Vector2(110,0), end = positions[node.id] - new Vector2(110,0);
                    var rt = Rect("Edge_" + need.nodeId + "_" + node.id, content, new Vector2(.5f,.5f), new Vector2(.5f,.5f), Vector2.zero, Vector2.zero);
                    rt.sizeDelta = new Vector2(Vector2.Distance(start,end), 3); rt.anchoredPosition = (start+end)*.5f;
                    rt.localRotation = Quaternion.Euler(0,0, Mathf.Atan2(end.y-start.y,end.x-start.x)*Mathf.Rad2Deg);
                    edges.Add((node, need, Image(rt, Hex("#59748B"), false)));
                }
            foreach (var node in tree.nodes)
            {
                var point = positions[node.id];
                var tile = Button(node.id, content, node.title, new Vector2(.5f,.5f), new Vector2(.5f,.5f), point-new Vector2(110,54), point+new Vector2(110,54), () => controller.SelectUpgradeNode(node.id));
                tiles.Add(node.id, tile); labels[node.id] = tile.GetComponentInChildren<UnityEngine.UI.Text>();
                labels[node.id].fontSize = 20;
                var cue = tile.gameObject.AddComponent<UpgradeTreeTile>(); cue.View = this; cue.NodeId = node.id;
            }
        }

        private void WireNavigation()
        {
            foreach (var node in tree.nodes)
            {
                var siblings = tree.nodes.Where(n => tree.Depth(n) == tree.Depth(node)).OrderByDescending(n => positions[n.id].y).ToArray();
                int index = Array.IndexOf(siblings, node);
                var child = tree.nodes.FirstOrDefault(n => n.prerequisites.Any(p => p.nodeId == node.id));
                Nav(tiles[node.id], node.prerequisites.Length > 0 ? tiles[node.prerequisites[0].nodeId] : back,
                    child != null ? tiles[child.id] : home, index > 0 ? tiles[siblings[index-1].id] : plus,
                    index+1 < siblings.Length ? tiles[siblings[index+1].id] : back);
            }
            Nav(back, statisticsToggle, tiles[SelectedId], tiles[SelectedId], minus);
            Nav(minus, back, plus, back, tiles[SelectedId]); Nav(plus, minus, home, back, tiles[SelectedId]);
            Nav(home, plus, statisticsToggle, back, tiles[SelectedId]);
            Nav(statisticsToggle, home, back, tiles[SelectedId], back);
        }

        public void Select(string id)
        {
            if (tree.Find(id) != null)
            {
                bool changed = SelectedId != id; SelectedId = id;
                if (changed) { detailScroll.StopMovement(); detailScroll.verticalNormalizedPosition = 1; }
            }
        }

        public void FocusNode(string id)
        {
            if (!CanInteract) return;
            controller.SelectUpgradeNode(id);
            var p = content.anchoredPosition + positions[id] * Zoom;
            var half = viewport.rect.size * .5f - new Vector2(115,60) * Zoom;
            if (Mathf.Abs(p.x) > half.x || Mathf.Abs(p.y) > half.y) RevealSelection();
        }

        public void Refresh(UpgradeCatalog catalog, UpgradePurchaseQuote quote)
        {
            var session = controller.OrderSession;
            var node = tree.Find(SelectedId);
            wallet.text = "Баланс: " + ClickCookUiCopy.Money(session.Rewards.Wallet);
            var text = new StringBuilder().AppendLine(node.title).AppendLine().AppendLine(node.description).AppendLine();
            if (node.IsRoot)
                text.AppendLine("Відкрито · безкоштовно").AppendLine("Ранг: 1 / 1").AppendLine("Доступ до дерева вже активний.");
            else if (node.IsWorking)
            {
                int rank = session.Profile.GetUpgradeRank(node.id), maximum = UpgradeNodeRules.MaximumRank(node.id);
                text.AppendLine($"Ранг: {rank} / {maximum}");
                text.AppendLine(EffectPreview(node.id, rank, session) + (rank < maximum ? " → " + EffectPreview(node.id,rank+1, session) : ""));
                if (node.id == UpgradeNodeRules.AutoStrengthMultiplier)
                    text.AppendLine("Округлення вгору після множення. Діє на стартові й періодичні авто-дії.");
                if (node.id == UpgradeNodeRules.AutoFrequency)
                    text.AppendLine("Цикл починається після повного інтервалу. Пауза не накопичує авто-дій.");
                if (node.id == UpgradeNodeRules.UpgradeDiscount)
                    text.AppendLine("Ціна вже враховує поточну знижку, також для цього вузла. Після покупки нова знижка діє на наступні покупки.");
                if (node.id == UpgradeNodeRules.RecipeDiscount)
                    text.AppendLine("Діє лише на нові рецепти. Ціна = базова × (1 − знижка), округлення вгору, мінімум 1. Придбане не перераховується.");
                if (node.id == UpgradeNodeRules.DishValue)
                    text.AppendLine("Множить оплату за якість. Округлення до копійок один раз наприкінці; половини вгору.");
                if (node.id == UpgradeNodeRules.DishXpMultiplier)
                    text.AppendLine("Множить суму XP за якість і додаткової XP. Дробова XP зберігається; межа майстерності незмінна.");
                if (node.id == UpgradeNodeRules.DishXpFlat)
                    text.AppendLine("До досвіду за якість, один раз за страву. На межі майстерності надлишок не нараховується.");
                if (node.id == UpgradeNodeRules.AutoActionChance)
                    text.AppendLine($"Успіх: {session.AutoActionsPerTick} × {session.AutoDamagePerAction} HP. Один шанс при першому вході в замовлення.");
                if (rank == maximum) text.AppendLine("Максимальний ранг");
                else
                {
                    decimal price = session.UpgradePriceForRank(node.id, rank);
                    text.AppendLine("Ціна: " + ClickCookUiCopy.Money(price));
                    text.AppendLine(!tree.PrerequisitesMet(node,session.Profile.GetUpgradeRank) ? "Заблоковано передумовами" :
                        session.Rewards.Wallet < price ? "Недостатньо коштів" : "Можна придбати");
                }
            }
            else
                text.AppendLine("Ефект ще не реалізовано").AppendLine("Придбано: 0 · межа рангу уточнюється").AppendLine("Значення й ціна: ще не визначені");
            text.AppendLine().AppendLine("Передумови (придбані ранги):");
            if (node.IsRoot) text.AppendLine("Немає");
            foreach (var need in node.prerequisites)
            {
                var parent = tree.Find(need.nodeId);
                int maximum = UpgradeTreeDefinition.MaximumRank(parent);
                int required = need.rank == -1 ? maximum : need.rank;
                bool met = required > 0 && UpgradeTreeDefinition.PurchasedRank(parent, session.Profile.GetUpgradeRank) >= required;
                text.AppendLine((met ? "✓ " : "× ") + parent.title + ": " + (need.rank == -1 ? "усі ранги" : need.rank.ToString()));
            }
            if (!node.IsRoot && !node.IsWorking && !tree.PrerequisitesMet(node, session.Profile.GetUpgradeRank)) text.AppendLine("Заблоковано передумовами");
            details.text = text.ToString().TrimEnd();
            buy.interactable = node.IsWorking && quote != null && session.Rewards.Wallet >= quote.Price && !session.IsPaused;
            buy.GetComponentInChildren<UnityEngine.UI.Text>().text = node.IsRoot ? "Вже відкрито" : !node.IsWorking ? "Ефект ще не реалізовано" :
                session.Profile.GetUpgradeRank(node.id) == UpgradeNodeRules.MaximumRank(node.id) ? "Максимальний ранг" :
                quote == null ? "Заблоковано передумовами" : "Придбати за " + ClickCookUiCopy.Money(quote.Price);
            statistics.text = $"Сила ручної дії: {session.ClickDamage} HP · ранг {session.ClickStrengthRank}\n" +
                $"Крит: {session.CriticalChance * 100:0.##}%" + (session.CriticalChanceRank == 0 ? " (неактивно)" : $" · ×{session.CriticalMultiplier:0.##}") +
                $"\nПодвійний: {session.DoubleStrikeChance * 100:0.##}% · потрійний: {session.TripleStrikeChance * 100:0.##}%" +
                $"\nСила повтору: ×{session.DoubleStrikePower:0.##}\nУтримання: " + (session.HasHold ? $"{session.HoldFrequency:0.#} рухи/с" : "вимкнено");
            string autoLine = (session.HasAuto ? $"Авто: {session.AutoActionsPerTick} дій × {session.AutoDamagePerAction} HP · {session.AutoFrequency:0.##}/с" : "Цикл: вимкнено") + $" · старт: {session.AutoActionChance * 100:0.##}%";
            if(session.HasAuto && (session.AutoActionsPerTick > 11 || session.AutoDamagePerAction > 32 || session.AutoFrequency > 3.25m || session.AutoActionChance > .2))
                autoLine = $"Авто: {session.AutoActionsPerTick}×{session.AutoDamagePerAction} HP · {session.AutoFrequency:0.##}/с · старт {session.AutoActionChance * 100:0.##}%";
            statistics.text += "\n" + autoLine;
            statistics.text += $"\nДосвід страви: +{session.DishXpBonus} XP · ×{session.DishXpMultiplier:0.##}";
            statistics.text += $"\nОплата за страву: ×{session.DishValueMultiplier:0.##}";
            statistics.text += $"\nЗнижка на покращення: {session.UpgradeDiscount * 100:0}%";
            statistics.text += $"\nЗнижка на рецепти: {session.RecipeDiscount * 100:0}%";
            foreach (var item in tree.nodes)
            {
                bool selected = item.id == SelectedId;
                int rank = session.Profile.GetUpgradeRank(item.id);
                int maximum = UpgradeNodeRules.MaximumRank(item.id);
                string state = item.IsRoot ? "ВІДКРИТО" : !item.IsWorking ? (tree.PrerequisitesMet(item,session.Profile.GetUpgradeRank) ? "НЕ РЕАЛІЗОВАНО" : "ЗАБЛОКОВАНО · НЕ РЕАЛІЗОВАНО") :
                    rank == maximum ? $"МАКСИМУМ · {rank}/{maximum}" : $"{rank}/{maximum} · " +
                    (!tree.PrerequisitesMet(item,session.Profile.GetUpgradeRank) ? "ЗАБЛОКОВАНО" :
                    session.Rewards.Wallet >= session.UpgradePriceForRank(item.id, rank) ? "ДОСТУПНО" : "БРАК КОШТІВ");
                labels[item.id].text = (selected ? "● " : "") + item.title + "\n" + state;
                tiles[item.id].GetComponent<UnityEngine.UI.Image>().color = selected ? Orange : item.IsRoot || item.IsWorking ? Cream : Hex("#C2C7CA");
            }
            foreach (var edge in edges)
            {
                var parent = tree.Find(edge.need.nodeId);
                int required = edge.need.rank == -1 ? UpgradeTreeDefinition.MaximumRank(parent) : edge.need.rank;
                edge.line.color = required > 0 && UpgradeTreeDefinition.PurchasedRank(parent,session.Profile.GetUpgradeRank) >= required ? Orange : Hex("#59748B");
            }
            Nav(buy, tiles[SelectedId], back, home, statisticsToggle);
            var nav = statisticsToggle.navigation; nav.selectOnUp = buy.interactable ? buy : tiles[SelectedId]; statisticsToggle.navigation = nav;
            nav = home.navigation; nav.selectOnRight = buy.interactable ? buy : statisticsToggle; home.navigation = nav;
            UpdateZoomLabel();
        }

        private static string EffectPreview(string id, int rank, PrototypeOrderSession session) {
            if(UpgradeNodeRules.TryGetAdditionalTier(id,out var tier)) return tier.Family switch {
                "upgrade_discount" => $"Знижка: {rank}%",
                "recipe_discount" => $"Знижка на рецепти: {rank}%",
                "dish_xp_flat" => $"Внесок досвіду: +{tier.Step * rank:0.##} XP",
                "dish_value" => $"Внесок оплати: +{tier.Step * rank * 100:0.##}%",
                "click_strength" or "auto_strength" => $"Внесок сили: +{tier.Step * rank:0.##} HP",
                "click_multiplier" or "auto_strength_multiplier" => $"Множник вузла: ×{1 + tier.Step * rank:0.##}",
                "critical_chance" or "double_strike_chance" or "triple_strike_chance" or "auto_action_chance" => $"Внесок шансу: +{tier.Step * rank * 100:0.##} п.п.",
                "critical_multiplier" or "double_strike_power" => $"Внесок множника: +{tier.Step * rank:0.##}",
                "auto_action_count" => $"Внесок дій: +{tier.Step * rank:0}",
                "auto_frequency" or "hold_frequency" => $"Внесок частоти: +{tier.Step * rank:0.##}/с", _ => "" };
            return id switch {
            UpgradeNodeRules.Strength => $"Сила: {decimal.Ceiling((UpgradeCatalog.ClickDamageAtRank(rank) + UpgradeNodeRules.AdditionalContribution("click_strength", session.Profile.GetUpgradeRank)) * session.ClickMultiplier)} HP",
            UpgradeNodeRules.CriticalChance => $"Шанс криту: {session.CriticalChance * 100 + 2 * (rank - session.CriticalChanceRank):0.##}%",
            UpgradeNodeRules.CriticalMultiplier => $"Множник криту: ×{session.CriticalMultiplier + .1m * (rank - session.CriticalMultiplierRank):0.##}",
            UpgradeNodeRules.Hold => rank == 0 ? "Утримання: вимкнено" : "Утримання: відкрито (базово 4 рухи/с)",
            UpgradeNodeRules.HoldFrequency => $"Ритм: {session.HoldFrequency + .5m * (rank - session.HoldFrequencyRank):0.##} рухи/с",
            UpgradeNodeRules.DoubleStrikeChance => $"Шанс повтору: {session.DoubleStrikeChance * 100 + 2 * (rank - session.DoubleStrikeChanceRank):0.##}%",
            UpgradeNodeRules.DoubleStrikePower => $"Сила повтору: ×{session.DoubleStrikePower + .05m * (rank - session.DoubleStrikePowerRank):0.##}",
            UpgradeNodeRules.DoubleStrikeChanceII => $"Внесок шансу: +{rank * 2} п.п.",
            UpgradeNodeRules.TripleStrikeChance => $"Шанс трьох рухів: {session.TripleStrikeChance * 100 + rank - session.TripleStrikeChanceRank:0.##}%",
            UpgradeNodeRules.AutoActionCount => rank == 0 ? "Авто: вимкнено" : $"Дій за цикл: {session.AutoActionsPerTick + rank - session.AutoActionCountRank} · {session.AutoDamagePerAction} HP · {session.AutoFrequency:0.##}/с",
            UpgradeNodeRules.AutoActionChance => $"Стартовий шанс: {session.AutoActionChance * 100 + 2 * (rank - session.AutoActionChanceRank):0.##}%",
            UpgradeNodeRules.AutoStrength => $"Сила авто-дії: {decimal.Ceiling((session.AutoBaseDamage + rank - session.AutoStrengthRank) * session.AutoStrengthMultiplier)} HP",
            UpgradeNodeRules.AutoStrengthMultiplier => $"Множник: ×{UpgradeNodeRules.AutoMultiplierAtRank(rank) * UpgradeNodeRules.AdditionalFactor("auto_strength_multiplier", session.Profile.GetUpgradeRank):0.##} · {decimal.Ceiling(session.AutoBaseDamage * UpgradeNodeRules.AutoMultiplierAtRank(rank) * UpgradeNodeRules.AdditionalFactor("auto_strength_multiplier", session.Profile.GetUpgradeRank))} HP",
            UpgradeNodeRules.AutoFrequency => $"Частота: {UpgradeNodeRules.AutoFrequencyAtRank(rank) + UpgradeNodeRules.AdditionalContribution("auto_frequency", session.Profile.GetUpgradeRank):0.##} циклу/с",
            UpgradeNodeRules.DishXpFlat => $"Досвід страви: +{session.DishXpBonus + 3 * (rank - session.DishXpFlatRank)} XP",
            UpgradeNodeRules.DishXpMultiplier => $"Множник XP: ×{UpgradeNodeRules.DishXpMultiplierAtRank(rank):0.##}",
            UpgradeNodeRules.DishValue => $"Множник оплати: ×{session.DishValueMultiplier + .03m * (rank - session.DishValueRank):0.##}",
            _ => ""
        }; }

        public void ToggleStatistics()
        {
            if (!CanInteract) return;
            statistics.gameObject.SetActive(!statistics.gameObject.activeSelf);
            statisticsToggle.GetComponentInChildren<UnityEngine.UI.Text>().text = statistics.gameObject.activeSelf ? "Активні покращення · Сховати" : "Активні покращення · Показати";
        }
        private void Update()
        {
            if (!CanInteract) return;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;
            if (keyboard.pageDownKey.wasPressedThisFrame) ScrollDetails(-.3f);
            if (keyboard.pageUpKey.wasPressedThisFrame) ScrollDetails(.3f);
        }
        public void ScrollDetails(float amount)
        {
            if (!CanInteract) return;
            detailScroll.StopMovement();
            detailScroll.verticalNormalizedPosition = Mathf.Clamp01(detailScroll.verticalNormalizedPosition + amount);
        }
        public void RevealSelection()
        {
            if (!CanInteract) return;
            Canvas.ForceUpdateCanvases(); content.anchoredPosition = -positions[SelectedId] * Zoom; ClampPan();
        }
        public void Pan(Vector2 delta)
        {
            if (!CanInteract) return;
            content.anchoredPosition += delta; ClampPan();
        }
        public void SetZoom(float value, Vector2 pivotInViewport)
        {
            if (!CanInteract) return;
            float next = Mathf.Clamp(value,.3f,1.4f), ratio = next / Zoom;
            content.anchoredPosition = pivotInViewport - (pivotInViewport - content.anchoredPosition) * ratio;
            content.localScale = Vector3.one * next; ClampPan(); UpdateZoomLabel();
        }
        private void ClampPan()
        {
            var limit = (content.sizeDelta * Zoom + viewport.rect.size) * .5f - new Vector2(70,70);
            var p = content.anchoredPosition;
            content.anchoredPosition = new Vector2(Mathf.Clamp(p.x,-Mathf.Max(0,limit.x),Mathf.Max(0,limit.x)),Mathf.Clamp(p.y,-Mathf.Max(0,limit.y),Mathf.Max(0,limit.y)));
        }
        private void UpdateZoomLabel() { if (zoomLabel != null) zoomLabel.text = $"{Zoom:P0}"; }

        private static RectTransform Rect(string name, Transform parent, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        {
            var rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>(); rt.SetParent(parent,false); Place(rt,min,max,low,high); return rt;
        }
        private static void Place(RectTransform rt, Vector2 min, Vector2 max, Vector2 low, Vector2 high)
        { rt.anchorMin=min; rt.anchorMax=max; rt.pivot=new Vector2(.5f,.5f); rt.offsetMin=low; rt.offsetMax=high; }
        private static void Move(Component component, Transform parent) { component.transform.SetParent(parent,false); component.gameObject.SetActive(true); }
        private static UnityEngine.UI.Image Image(RectTransform rt, Color color, bool raycast)
        { var img=rt.gameObject.AddComponent<UnityEngine.UI.Image>(); img.color=color; img.raycastTarget=raycast; return img; }
        private void StyleText(UnityEngine.UI.Text text, int size, Color color)
        { text.font=font; text.fontSize=size; text.fontStyle=FontStyle.Normal; text.color=color; text.raycastTarget=false; text.resizeTextForBestFit=false; text.alignment=TextAnchor.MiddleLeft; text.horizontalOverflow=HorizontalWrapMode.Wrap; text.verticalOverflow=VerticalWrapMode.Truncate; }
        private UnityEngine.UI.Text Text(string name, Transform parent, string value, Vector2 low, Vector2 high, int size, Color color, bool top=true)
        {
            var rt=Rect(name,parent, top ? new Vector2(0,1) : Vector2.zero, Vector2.one,low,high);
            var text=rt.gameObject.AddComponent<UnityEngine.UI.Text>(); StyleText(text,size,color); text.text=value; return text;
        }
        private void StyleExisting(UnityEngine.UI.Button button, int size)
        {
            StyleText(button.GetComponentInChildren<UnityEngine.UI.Text>(true),size,Ink);
            button.GetComponentInChildren<UnityEngine.UI.Text>(true).alignment=TextAnchor.MiddleCenter;
        }
        private UnityEngine.UI.Button Button(string name, Transform parent, string value, Vector2 min, Vector2 max, Vector2 low, Vector2 high, UnityEngine.Events.UnityAction action)
        {
            var rt=Rect(name,parent,min,max,low,high); var image=Image(rt,Cream,true); image.sprite=paper; image.type=UnityEngine.UI.Image.Type.Sliced;
            var button=rt.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic=image; button.onClick.AddListener(action);
            var label=Text("Label",rt,value,new Vector2(8,6),new Vector2(-8,-6),23,Ink,false); label.alignment=TextAnchor.MiddleCenter;
            var outline=rt.gameObject.AddComponent<UnityEngine.UI.Outline>(); outline.effectColor=Orange; outline.effectDistance=new Vector2(3,-3); outline.enabled=false;
            var focus=rt.gameObject.AddComponent<UpgradeControlFocus>(); focus.Outline=outline;
            return button;
        }
        private static void Nav(UnityEngine.UI.Button b, UnityEngine.UI.Button left, UnityEngine.UI.Button right, UnityEngine.UI.Button up, UnityEngine.UI.Button down)
        { b.navigation=new UnityEngine.UI.Navigation {mode=UnityEngine.UI.Navigation.Mode.Explicit,selectOnLeft=left,selectOnRight=right,selectOnUp=up,selectOnDown=down}; }
    }

    public sealed class UpgradeTreeGesture : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
    {
        public UpgradeTreeView View;
        private Vector2 previous;
        private void Point(PointerEventData e, out Vector2 point) => RectTransformUtility.ScreenPointToLocalPointInRectangle(View.Viewport,e.position,e.pressEventCamera,out point);
        public void OnBeginDrag(PointerEventData e) { Point(e,out previous); }
        public void OnDrag(PointerEventData e) { Point(e,out var point); View.Pan(point-previous); previous=point; }
        public void OnEndDrag(PointerEventData e) { }
        public void OnScroll(PointerEventData e) { Point(e,out var point); View.SetZoom(View.Zoom*Mathf.Pow(1.12f,e.scrollDelta.y),point); }
    }
    public sealed class UpgradeTreeTile : MonoBehaviour, ISelectHandler
    {
        public UpgradeTreeView View;
        public string NodeId;
        public void OnSelect(BaseEventData e) { View.FocusNode(NodeId); }
    }
    public sealed class UpgradeControlFocus : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public UnityEngine.UI.Outline Outline;
        private bool focused, hovered;
        private void Refresh() { if (Outline != null) { Outline.enabled=focused || hovered; Outline.effectColor=focused ? new Color(.25f,.75f,1) : new Color(1,.6f,.1f); } }
        public void OnSelect(BaseEventData e) { focused=true; Refresh(); }
        public void OnDeselect(BaseEventData e) { focused=false; Refresh(); }
        public void OnPointerEnter(PointerEventData e) { hovered=true; Refresh(); }
        public void OnPointerExit(PointerEventData e) { hovered=false; Refresh(); }
        private void OnDisable() { focused=hovered=false; Refresh(); }
    }
}
