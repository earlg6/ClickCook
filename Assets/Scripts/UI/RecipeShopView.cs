using System.Linq;
using ClickCook.Data;
using ClickCook.Runtime;
using ClickCook.Runtime.Locations;
using UnityEngine;

namespace ClickCook.UI
{
    /// <summary>Paginated recipe purchases. Authored location pools are the availability authority.</summary>
    public sealed class RecipeShopView : MonoBehaviour
    {
        [SerializeField] private RecipeCatalog catalog;
        [SerializeField] private LocationCatalog locations;
        [SerializeField] private UnityEngine.UI.Button[] rows;
        [SerializeField] private UnityEngine.UI.Button previous, next, buy, back;
        [SerializeField] private UnityEngine.UI.Text details, status, wallet, pageLabel;
        private ClickCookPrototypeController controller;
        private int page, selectedId = 1;
        private RecipePurchaseQuote quote;
        private bool refreshNextFrame;
        public int SelectedId => selectedId;
        public bool CanInteract => controller != null && controller.enabled && !controller.GameplayBlocked &&
            !controller.OrderSession.Profile.HasActiveDay && gameObject.activeInHierarchy;

        public void Initialize(ClickCookPrototypeController owner)
        {
            controller = owner;
            for (int i = 0; i < rows.Length; i++)
            {
                int slot = i;
                rows[i].onClick.AddListener(() => SelectRow(slot));
            }
            previous.onClick.AddListener(() => ChangePage(-1));
            next.onClick.AddListener(() => ChangePage(1));
            buy.onClick.AddListener(Purchase);
            back.onClick.AddListener(controller.CloseRecipes);
        }
        public void Show()
        {
            page = 0; selectedId = 1; refreshNextFrame = false;
            status.text = "Куплений рецепт доступний у всіх локаціях, де його подають.";
            Refresh();
        }
        private void ChangePage(int delta)
        {
            if (!CanInteract || refreshNextFrame) return;
            int pages = (catalog.Dishes.Count + rows.Length - 1) / rows.Length;
            page = (page + delta + pages) % pages;
            selectedId = catalog.Dishes[page * rows.Length].DishId;
            Refresh();
        }
        public void SelectRow(int slot)
        {
            if (!CanInteract || refreshNextFrame || slot < 0 || slot >= rows.Length) return;
            int index = page * rows.Length + slot;
            if (index >= catalog.Dishes.Count) return;
            selectedId = catalog.Dishes[index].DishId;
            Refresh();
        }
        public void Purchase()
        {
            if (!CanInteract || refreshNextFrame || quote == null || quote.DishId != selectedId) return;
            var offer = quote; quote = null; buy.interactable = false;
            status.text = controller.OrderSession.TryPurchaseRecipe(offer)
                ? $"Рецепт «{ClickCookUiCopy.Dish(offer.Dish)}» придбано!" : "Покупка не вдалася. Перевір баланс.";
            refreshNextFrame = true;
        }
        private void LateUpdate()
        {
            if (!refreshNextFrame || !CanInteract) return;
            refreshNextFrame = false; Refresh();
        }
        private void Refresh()
        {
            var session = controller.OrderSession;
            wallet.text = "Баланс: " + ClickCookUiCopy.Money(session.Rewards.Wallet) + $" · Знижка на рецепти: {session.RecipeDiscount * 100:0}%";
            pageLabel.text = $"{page + 1} / {(catalog.Dishes.Count + rows.Length - 1) / rows.Length}";
            for (int i = 0; i < rows.Length; i++)
            {
                int index = page * rows.Length + i;
                rows[i].gameObject.SetActive(index < catalog.Dishes.Count);
                if (index >= catalog.Dishes.Count) continue;
                var dish = catalog.Dishes[index];
                rows[i].GetComponentInChildren<UnityEngine.UI.Text>().text =
                    (dish.DishId == selectedId ? "› " : "") + ClickCookUiCopy.Dish(dish) +
                    (session.Profile.OwnsRecipe(dish.DishId) ? " · Є" : " · " + ClickCookUiCopy.Money(session.RecipePrice(dish)));
            }
            var selected = catalog.Find(selectedId);
            bool owned = session.Profile.OwnsRecipe(selectedId);
            details.text = ClickCookUiCopy.Dish(selected) + "\n\n" +
                (owned ? "Рецепт уже доступний" : "Ціна рецепта: " + ClickCookUiCopy.Money(session.RecipePrice(selected)) +
                    $"\nЗнижка: {session.RecipeDiscount * 100:0}% · базова {ClickCookUiCopy.Money(selected.RecipeCost)}") +
                "\nПродаж страви: " + ClickCookUiCopy.Money(selected.SaleValue) +
                "\n\nПодають у локаціях:\n" + string.Join("\n", locations.Locations.Where(l => l.DocumentedAvailableDishes.Contains(selected))
                    .Select(l => ClickCookUiCopy.Location(l) + $" · рівень {PlayerProgression.RequiredLevel(l)}"));
            session.TryQuoteRecipe(selectedId, out quote);
            buy.interactable = quote != null && session.Rewards.Wallet >= quote.Price;
            buy.GetComponentInChildren<UnityEngine.UI.Text>().text = owned ? "Придбано" : buy.interactable ? "Купити рецепт" : "Недостатньо коштів";
            var buttons = rows.Where(r => r.gameObject.activeSelf).Concat(new[] { previous, next, back }).Concat(buy.interactable ? new[] { buy } : new UnityEngine.UI.Button[0]).ToArray();
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].navigation = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit,
                    selectOnUp = buttons[(i + buttons.Length - 1) % buttons.Length], selectOnLeft = buttons[(i + buttons.Length - 1) % buttons.Length],
                    selectOnDown = buttons[(i + 1) % buttons.Length], selectOnRight = buttons[(i + 1) % buttons.Length] };
            var events = UnityEngine.EventSystems.EventSystem.current;
            if (events != null && (events.currentSelectedGameObject == null || (!buy.interactable && events.currentSelectedGameObject == buy.gameObject)))
                events.SetSelectedGameObject(rows[0].gameObject);
        }
    }
}
