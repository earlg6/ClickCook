using ClickCook.Data;
using ClickCook.Runtime.Locations;
using System.Text;
using UnityEngine;

namespace ClickCook.Runtime
{
    /// <summary>
    /// Temporary Location Selection -> Work Day prototype controller.
    /// It intentionally keeps selected location state in memory only.
    /// </summary>
    public sealed class ClickCookPrototypeController : MonoBehaviour
    {
        [SerializeField] private LocationCatalog locationCatalog;
        [SerializeField] private GameObject locationSelectionScreen;
        [SerializeField] private GameObject workDayScreen;
        [SerializeField] private UnityEngine.UI.Text locationNameText;
        [SerializeField] private UnityEngine.UI.Text locationLevelText;
        [SerializeField] private UnityEngine.UI.Text progressText;
        [SerializeField] private UnityEngine.UI.Text dishSlotsText;
        [SerializeField] private UnityEngine.UI.Text incomeText;
        [SerializeField] private UnityEngine.UI.Text workDayLocationText;
        [SerializeField] private UnityEngine.UI.Text workDayDishCandidatesText;
        [SerializeField] private UnityEngine.UI.Text finishDayMessageText;
        [SerializeField] private UnityEngine.UI.Button previousButton;
        [SerializeField] private UnityEngine.UI.Button nextButton;
        [SerializeField] private UnityEngine.UI.Button cookButton;
        [SerializeField] private UnityEngine.UI.Button finishDayButton;
        [Header("Milestone 4A — provisional capacities by documented level (0–10)")]
        [SerializeField] private int[] prototypeCapacities = { 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 6 };
        [SerializeField] private UnityEngine.UI.Text orderBoardStatusText;
        [SerializeField] private UnityEngine.UI.Button[] orderButtons;
        [SerializeField] private UnityEngine.UI.Text[] orderLabels;
        [SerializeField] private GameObject cookingPrototypeScreen;
        [SerializeField] private UnityEngine.UI.Text cookingOrderText;
        [SerializeField] private UnityEngine.UI.Button backToBoardButton;
        [SerializeField] private UnityEngine.UI.Button cookingDishButton;
        [SerializeField] private UnityEngine.UI.Text cookingDishLabel;
        [SerializeField] private GameObject dishResultScreen;
        [SerializeField] private UnityEngine.UI.Text dishResultText;
        [SerializeField] private UnityEngine.UI.Button continueFromResultButton;

        private readonly PrototypeOrderSession session = new();
        private PrototypeOrder displayedCookingOrder;
        private int currentLocationIndex;

        public PrototypeOrderSession OrderSession => session;

        private void Awake()
        {
            if (!HasRequiredReferences())
            {
                Debug.LogError("ClickCook prototype is missing required scene references.", this);
                enabled = false;
                return;
            }

            previousButton.onClick.AddListener(ShowPreviousLocation);
            nextButton.onClick.AddListener(ShowNextLocation);
            cookButton.onClick.AddListener(CookSelectedLocation);
            finishDayButton.onClick.AddListener(FinishDay);
            backToBoardButton.onClick.AddListener(BackToBoard);
            cookingDishButton.onClick.AddListener(ClickCookingDish);
            continueFromResultButton.onClick.AddListener(ContinueFromDishResult);
            for (int i = 0; i < orderButtons.Length; i++)
            {
                int slot = i;
                orderButtons[i].onClick.AddListener(() => SelectOrder(slot));
            }

            locationSelectionScreen.SetActive(true);
            workDayScreen.SetActive(false);
            cookingPrototypeScreen.SetActive(false);
            ClearDishResult();
            RefreshLocationSelection();
        }

        private bool HasRequiredReferences()
        {
            if (orderButtons == null || orderLabels == null ||
                orderButtons.Length != PrototypeOrderSession.MaximumBoardCapacity ||
                orderLabels.Length != orderButtons.Length || prototypeCapacities == null ||
                prototypeCapacities.Length != 11) return false;
            for (int i = 0; i < orderButtons.Length; i++)
            {
                if (orderButtons[i] == null || orderLabels[i] == null) return false;
            }

            return locationCatalog != null &&
                   locationSelectionScreen != null &&
                   workDayScreen != null &&
                   locationNameText != null &&
                   locationLevelText != null &&
                   progressText != null &&
                   dishSlotsText != null &&
                   incomeText != null &&
                   workDayLocationText != null &&
                   workDayDishCandidatesText != null &&
                   finishDayMessageText != null &&
                   previousButton != null &&
                   nextButton != null &&
                   cookButton != null &&
                   finishDayButton != null && orderBoardStatusText != null &&
                   cookingPrototypeScreen != null && cookingOrderText != null &&
                   backToBoardButton != null && cookingDishButton != null && cookingDishLabel != null &&
                   dishResultScreen != null && dishResultText != null && continueFromResultButton != null;
        }

        public void ShowPreviousLocation()
        {
            SelectRelativeLocation(-1);
        }

        public void ShowNextLocation()
        {
            SelectRelativeLocation(1);
        }

        public void CookSelectedLocation()
        {
            LocationData selectedLocation = CurrentLocation;
            if (!enabled || !locationSelectionScreen.activeSelf || selectedLocation == null)
            {
                return;
            }

            int level = selectedLocation.LevelRequirement;
            int capacity = level >= 0 && level < prototypeCapacities.Length ? prototypeCapacities[level] : 0;
            session.StartDay(selectedLocation, capacity);
            displayedCookingOrder = null;
            ClearDishResult();
            locationSelectionScreen.SetActive(false);
            workDayScreen.SetActive(true);
            workDayLocationText.text = $"Selected location: {selectedLocation.EnglishName}";
            RefreshWorkDayDishCandidates();
            RefreshOrderBoard();
            cookingPrototypeScreen.SetActive(false);
            finishDayMessageText.text = string.Empty;
        }

        private void RefreshWorkDayDishCandidates()
        {
            var dishes = session.SelectedLocation.DocumentedAvailableDishes;
            var text = new StringBuilder();
            text.AppendLine($"Documented dish candidates: {dishes.Count}");
            foreach (DishData dish in dishes)
            {
                // Preserve every stored entry; missing data must not silently change eligibility.
                text.AppendLine(dish != null
                    ? $"{dish.DishId}. {dish.EnglishName}"
                    : "[Missing DishData reference]");
            }

            workDayDishCandidatesText.text = text.ToString();
        }

        public void FinishDay()
        {
            if (!enabled || !workDayScreen.activeSelf || session.ActiveOrder != null) return;
            EndWorkDay(false);
        }

        public void SelectOrder(int slotIndex)
        {
            if (!enabled || !workDayScreen.activeSelf || !session.TrySelect(slotIndex)) return;
            PrototypeOrder order = session.ActiveOrder;
            displayedCookingOrder = order;
            cookingDishLabel.text = $"{order.Dish.EnglishName}\nClick to cook\nTemporary dish placeholder";
            cookingOrderText.text = $"Location: {order.Location.EnglishName}\n" +
                                    $"Dish ID: {order.Dish.DishId}\nDish: {order.Dish.EnglishName}\n" +
                                    $"Slot: {order.SlotIndex + 1}\nOrder: {order.Identity}";
            RefreshOrderBoard();
            workDayScreen.SetActive(false);
            cookingPrototypeScreen.SetActive(true);
        }

        public void BackToBoard()
        {
            if (!enabled || !cookingPrototypeScreen.activeSelf) return;
            session.ReturnToBoard();
            displayedCookingOrder = null;
            cookingPrototypeScreen.SetActive(false);
            workDayScreen.SetActive(true);
            RefreshOrderBoard();
        }

        public void ClickCookingDish()
        {
            PrototypeOrder completedOrder = displayedCookingOrder;
            if (!enabled || !cookingPrototypeScreen.activeSelf ||
                !session.TryCookClick(completedOrder)) return;
            displayedCookingOrder = null;
            if (session.CompletedDishes >= PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY)
            {
                EndWorkDay(true);
                return;
            }
            cookingPrototypeScreen.SetActive(false);
            dishResultText.text = $"Dish: {completedOrder.Dish.EnglishName}\n" +
                $"Base sale value (reference only): {completedOrder.Dish.SaleValue} у.е.\n" +
                "Rarity: not implemented\nNo currency or XP awarded.";
            dishResultScreen.SetActive(true);
        }

        public void ContinueFromDishResult()
        {
            if (!enabled || !dishResultScreen.activeSelf || !session.IsDayActive) return;
            ClearDishResult();
            workDayScreen.SetActive(true);
            RefreshOrderBoard();
        }

        private void ClearDishResult()
        {
            dishResultScreen.SetActive(false);
            dishResultText.text = string.Empty;
        }

        private void EndWorkDay(bool limitReached)
        {
            string location = session.SelectedLocation != null ? session.SelectedLocation.EnglishName : string.Empty;
            int completed = session.CompletedDishes;
            session.EndDay();
            ClearDishResult();
            displayedCookingOrder = null;
            cookingOrderText.text = string.Empty;
            cookingDishLabel.text = string.Empty;
            cookingPrototypeScreen.SetActive(false);
            workDayScreen.SetActive(false);
            locationSelectionScreen.SetActive(true);
            RefreshOrderBoard();
            RefreshLocationSelection();
            finishDayMessageText.text = $"Day finished: {location}\n" +
                $"Completed dishes: {completed} / {PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY}" +
                (limitReached ? " — Daily limit reached" : string.Empty);
        }

        private void RefreshOrderBoard()
        {
            orderBoardStatusText.text = "Currency: not implemented\n" +
                $"Available orders: {session.Orders.Count}\n" +
                $"Board capacity: {session.BoardCapacity} / {PrototypeOrderSession.MaximumBoardCapacity} (provisional)\n" +
                $"Completed dishes: {session.CompletedDishes} / {PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY}" +
                (session.Orders.Count == 0 ? "\nNo documented dishes available." : string.Empty);
            for (int i = 0; i < orderButtons.Length; i++)
            {
                PrototypeOrder order = session.GetOrderAtSlot(i);
                bool populated = order != null;
                orderButtons[i].gameObject.SetActive(i < session.BoardCapacity);
                orderButtons[i].interactable = populated && session.ActiveOrder == null && order.Dish != null;
                orderLabels[i].text = populated
                    ? $"Slot {i + 1}\n" + (order.Dish != null
                        ? order.Dish.EnglishName : "Missing DishData")
                    : $"Slot {i + 1}\nEmpty";
            }
        }

        private LocationData CurrentLocation =>
            locationCatalog != null && locationCatalog.Locations.Count > 0
                ? locationCatalog.Locations[currentLocationIndex]
                : null;

        private void SelectRelativeLocation(int offset)
        {
            if (!enabled || !locationSelectionScreen.activeSelf || locationCatalog == null || locationCatalog.Locations.Count == 0)
            {
                return;
            }

            currentLocationIndex = (currentLocationIndex + offset + locationCatalog.Locations.Count) %
                                   locationCatalog.Locations.Count;
            RefreshLocationSelection();
        }

        private void RefreshLocationSelection()
        {
            LocationData location = CurrentLocation;
            if (location == null)
            {
                return;
            }

            locationNameText.text = location.EnglishName;
            locationLevelText.text = $"Documented level: {location.LevelRequirement}";
            progressText.text = "Progress: prototype placeholder";
            dishSlotsText.text = $"Documented dishes: {location.DocumentedAvailableDishes.Count}";
            incomeText.text = $"Income/sec: {location.PassiveIncomePerSecond}";
        }

    }
}
