using ClickCook.Data;
using ClickCook.Runtime.Locations;
using System.Text;
using System.Globalization;
using UnityEngine;
using ClickCook.UI;

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
        [Header("Milestone 6A — between-day upgrades")]
        [SerializeField] private UpgradeCatalog upgradeCatalog;
        [SerializeField] private GameObject upgradeScreen;
        [SerializeField] private UnityEngine.UI.Button openUpgradesButton;
        [SerializeField] private UnityEngine.UI.Button closeUpgradesButton;
        [SerializeField] private UnityEngine.UI.Button buyUpgradeButton;
        [SerializeField] private UnityEngine.UI.Text upgradeDetailsText;
        [SerializeField] private UnityEngine.UI.Text upgradeStatusText;
        [Header("UI-A2 — Work Day / result presentation")]
        [SerializeField] private UnityEngine.UI.Text dayWalletText;
        [SerializeField] private UnityEngine.UI.Text dayCompletedText;
        [SerializeField] private UnityEngine.UI.Text resultRarityText;
        [SerializeField] private UnityEngine.UI.Text resultMasteryText;
        [SerializeField] private UnityEngine.UI.Image resultMasteryFill;
        [Header("UI-A3 — designer review")]
        [SerializeField] private UnityEngine.UI.Button exitGameButton;
        [SerializeField] private UnityEngine.UI.Image cookingHpFill;
        [SerializeField] private UnityEngine.UI.Text cookingHpText;
        [SerializeField] private UnityEngine.UI.Image[] orderMasteryFills;
        [SerializeField] private UnityEngine.UI.Text[] orderMasteryTexts;
        private UpgradePurchaseQuote displayedUpgradeQuote;
        private bool refreshUpgradeNextFrame;

        private readonly PrototypeOrderSession session = new();
        private PrototypeOrder displayedCookingOrder;
        private int currentLocationIndex;

        public PrototypeOrderSession OrderSession => session;

        private void Start()
        {
            if (enabled && locationSelectionScreen.activeSelf) FocusUi(cookButton);
        }

        private static void FocusUi(UnityEngine.UI.Button button)
        {
            var events = UnityEngine.EventSystems.EventSystem.current;
            if (events != null) events.SetSelectedGameObject(button.gameObject);
        }

        private void Awake()
        {
            if (!HasRequiredReferences() || !session.ConfigureUpgrades(upgradeCatalog))
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
            openUpgradesButton.onClick.AddListener(OpenUpgrades);
            closeUpgradesButton.onClick.AddListener(CloseUpgrades);
            exitGameButton.onClick.AddListener(ExitGame);
            buyUpgradeButton.onClick.AddListener(BuyUpgrade);
            for (int i = 0; i < orderButtons.Length; i++)
            {
                int slot = i;
                orderButtons[i].onClick.AddListener(() => SelectOrder(slot));
            }

            locationSelectionScreen.SetActive(true);
            upgradeScreen.SetActive(false);
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
                prototypeCapacities.Length != 11 || orderMasteryFills == null || orderMasteryTexts == null ||
                orderMasteryFills.Length != orderButtons.Length || orderMasteryTexts.Length != orderButtons.Length) return false;
            for (int i = 0; i < orderButtons.Length; i++)
            {
                if (orderButtons[i] == null || orderLabels[i] == null || orderMasteryFills[i] == null || orderMasteryTexts[i] == null) return false;
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
                   dishResultScreen != null && dishResultText != null && continueFromResultButton != null &&
                   upgradeCatalog != null && upgradeScreen != null && openUpgradesButton != null &&
                   closeUpgradesButton != null && buyUpgradeButton != null &&
                   upgradeDetailsText != null && upgradeStatusText != null &&
                   dayWalletText != null && dayCompletedText != null && resultRarityText != null &&
                   resultMasteryText != null && resultMasteryFill != null && exitGameButton != null &&
                   cookingHpFill != null && cookingHpText != null;
        }

        public void ExitGame()
        {
            if (!enabled || !locationSelectionScreen.activeSelf) return;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void OpenUpgrades()
        {
            if (!enabled || !locationSelectionScreen.activeSelf || session.IsDayActive) return;
            locationSelectionScreen.SetActive(false);
            upgradeScreen.SetActive(true);
            upgradeStatusText.text = "Покращення діють у поточному запуску гри. Збереження — незабаром.";
            RefreshUpgrade();
            FocusUi(buyUpgradeButton.interactable ? buyUpgradeButton : closeUpgradesButton);
        }

        public void CloseUpgrades()
        {
            if (!enabled || !upgradeScreen.activeSelf) return;
            displayedUpgradeQuote = null;
            refreshUpgradeNextFrame = false;
            upgradeScreen.SetActive(false);
            locationSelectionScreen.SetActive(true);
            RefreshLocationSelection();
            FocusUi(openUpgradesButton);
        }

        public void BuyUpgrade()
        {
            if (!enabled || !upgradeScreen.activeSelf || session.IsDayActive || displayedUpgradeQuote == null) return;
            var quote = displayedUpgradeQuote;
            displayedUpgradeQuote = null;
            buyUpgradeButton.interactable = false;
            upgradeStatusText.text = session.TryPurchaseUpgrade(quote)
                ? $"Силу кліку покращено до рангу {session.ClickStrengthRank}!"
                : "Не вдалося придбати. Перевір баланс і поточну ціну.";
            // Consume this offer for the rest of the frame: repeated callbacks cannot buy twice.
            refreshUpgradeNextFrame = true;
        }

        private void LateUpdate()
        {
            if (!refreshUpgradeNextFrame) return;
            refreshUpgradeNextFrame = false;
            if (upgradeScreen.activeSelf)
            {
                RefreshUpgrade();
                // Disabling the consumed offer can clear EventSystem selection immediately.
                var events = UnityEngine.EventSystems.EventSystem.current;
                if (events != null && events.currentSelectedGameObject == null)
                    FocusUi(buyUpgradeButton.interactable ? buyUpgradeButton : closeUpgradesButton);
            }
        }

        private void RefreshUpgrade()
        {
            var definition = upgradeCatalog.Find(UpgradeCatalog.ClickStrengthId);
            bool hasNext = session.TryQuoteUpgrade(UpgradeCatalog.ClickStrengthId, out displayedUpgradeQuote);
            upgradeDetailsText.text = $"Баланс: {ClickCookUiCopy.Money(session.Rewards.Wallet)}\n\n" +
                $"Ранг: {session.ClickStrengthRank} / {definition.MaximumRank}\n" +
                $"Сила кліку: {session.ClickDamage}" +
                (hasNext ? $" → {UpgradeCatalog.ClickDamageAtRank(session.ClickStrengthRank + 1)}\n\n" +
                    $"Ціна: {ClickCookUiCopy.Money(displayedUpgradeQuote.Price)}\n" +
                    (session.Rewards.Wallet >= displayedUpgradeQuote.Price ? "Можна придбати" : "Недостатньо коштів")
                    : "\n\nМаксимальний ранг");
            buyUpgradeButton.interactable = hasNext && session.Rewards.Wallet >= displayedUpgradeQuote.Price;
            buyUpgradeButton.GetComponentInChildren<UnityEngine.UI.Text>().text = hasNext
                ? $"Придбати за {ClickCookUiCopy.Money(displayedUpgradeQuote.Price)}" : "Максимальний ранг";
            var navigation = closeUpgradesButton.navigation;
            var nextFocus = buyUpgradeButton.interactable ? buyUpgradeButton : null;
            navigation.selectOnLeft = navigation.selectOnRight = navigation.selectOnUp = navigation.selectOnDown = nextFocus;
            closeUpgradesButton.navigation = navigation;
            var events = UnityEngine.EventSystems.EventSystem.current;
            if (!buyUpgradeButton.interactable && events != null && events.currentSelectedGameObject == buyUpgradeButton.gameObject)
                FocusUi(closeUpgradesButton);
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
            workDayLocationText.text = ClickCookUiCopy.Location(selectedLocation);
            RefreshWorkDayDishCandidates();
            RefreshOrderBoard();
            cookingPrototypeScreen.SetActive(false);
            finishDayMessageText.text = string.Empty;
            FocusBoard();
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
            cookingDishLabel.text = "Натискай на страву";
            cookingOrderText.text = $"{ClickCookUiCopy.Dish(order.Dish)}\n{ClickCookUiCopy.Location(order.Location)}";
            RefreshCookingHp();
            RefreshOrderBoard();
            workDayScreen.SetActive(false);
            cookingPrototypeScreen.SetActive(true);
            FocusUi(cookingDishButton);
        }

        public void BackToBoard()
        {
            if (!enabled || !cookingPrototypeScreen.activeSelf) return;
            session.ReturnToBoard();
            displayedCookingOrder = null;
            RefreshCookingHp();
            cookingPrototypeScreen.SetActive(false);
            workDayScreen.SetActive(true);
            RefreshOrderBoard();
            FocusBoard();
        }

        public void ClickCookingDish()
        {
            PrototypeOrder completedOrder = displayedCookingOrder;
            if (!enabled || !cookingPrototypeScreen.activeSelf) return;
            bool completed = session.TryCookClick(completedOrder);
            RefreshCookingHp();
            if (!completed) return;
            displayedCookingOrder = null;
            cookingPrototypeScreen.SetActive(false);
            PrototypeDishResult result = session.LastResult;
            bool mastered = result.Level == PrototypeDishRewards.MaximumDishLevel;
            int xp = session.Rewards.GetLevelXp(result.Dish);
            int needed = mastered ? 1 : PrototypeDishRewards.RequiredXp(result.Level);
            resultMasteryText.text = mastered ? "Майстерність: максимум" : $"Досвід до наступного рівня: {xp} / {needed} XP";
            resultMasteryFill.fillAmount = mastered ? 1f : (float)xp / needed;
            resultRarityText.text = ClickCookUiCopy.Rarity(result.Rarity);
            Color[] rarityInk = { new Color(.23f,.16f,.11f), new Color(.17f,.34f,.17f),
                new Color(.10f,.25f,.46f), new Color(.36f,.17f,.46f), new Color(.44f,.27f,.03f) };
            resultRarityText.color = rarityInk[(int)result.Rarity];
            dishResultText.text = $"{ClickCookUiCopy.Dish(result.Dish)}\n\n" +
                $"Зароблено: {ClickCookUiCopy.Money(result.Payout)}\nБаланс: {ClickCookUiCopy.Money(session.Rewards.Wallet)}\n\n" +
                $"Досвід страви: +{result.XpGained} XP\nРівень страви: {result.PreviousLevel} → {result.Level}";
            continueFromResultButton.GetComponentInChildren<UnityEngine.UI.Text>().text =
                session.CompletedDishes >= PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY ? "Завершити день" : "Продовжити";
            dishResultScreen.SetActive(true);
            FocusUi(continueFromResultButton);
        }

        public void ContinueFromDishResult()
        {
            if (!enabled || !dishResultScreen.activeSelf || !session.IsDayActive) return;
            if (session.CompletedDishes >= PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY)
            {
                EndWorkDay(true);
                return;
            }
            ClearDishResult();
            workDayScreen.SetActive(true);
            RefreshOrderBoard();
            FocusBoard();
        }

        private void ClearDishResult()
        {
            dishResultScreen.SetActive(false);
            dishResultText.text = string.Empty;
            resultRarityText.text = resultMasteryText.text = string.Empty;
            resultMasteryFill.fillAmount = 0f;
        }

        private void EndWorkDay(bool limitReached)
        {
            string location = ClickCookUiCopy.Location(session.SelectedLocation);
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
            finishDayMessageText.text = $"День завершено: {location}\n" +
                $"Приготовано: {completed} / {PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY}" +
                (limitReached ? " — Ліміт дня досягнуто" : string.Empty);
            FocusUi(cookButton);
        }

        private void RefreshOrderBoard()
        {
            dayWalletText.text = $"Баланс: {ClickCookUiCopy.Money(session.Rewards.Wallet)}";
            dayCompletedText.text = $"Приготовано: {session.CompletedDishes} / {PrototypeOrderSession.MAX_COMPLETED_DISHES_PER_DAY}";
            orderBoardStatusText.text = $"Замовлення: {session.Orders.Count} / {session.BoardCapacity}" +
                (session.Orders.Count == 0 ? "\nНемає доступних замовлень" : string.Empty);
            for (int i = 0; i < orderButtons.Length; i++)
            {
                PrototypeOrder order = session.GetOrderAtSlot(i);
                bool populated = order != null;
                orderButtons[i].gameObject.SetActive(i < session.BoardCapacity);
                orderButtons[i].interactable = populated && session.ActiveOrder == null && order.Dish != null;
                orderLabels[i].text = populated ? ClickCookUiCopy.Dish(order.Dish) : "Порожньо";
                var dish = order?.Dish;
                int masteryLevel = dish != null ? session.Rewards.GetLevel(dish) : 1;
                bool maximum = masteryLevel == PrototypeDishRewards.MaximumDishLevel;
                int xp = dish != null ? session.Rewards.GetLevelXp(dish) : 0;
                int cost = maximum ? 1 : PrototypeDishRewards.RequiredXp(masteryLevel);
                orderMasteryFills[i].fillAmount = maximum ? 1f : (float)xp / cost;
                orderMasteryTexts[i].text = dish == null ? "Немає страви" : maximum ? "Майстерність: максимум" :
                    session.BoardCapacity > 3 ? $"Майстерність: {xp}/{cost} XP" : $"Майстерність: рівень {masteryLevel}\n{xp} / {cost} XP";
            }
            LayoutOrderCards();
            ConfigureBoardNavigation();
        }

        private void RefreshCookingHp()
        {
            var order = displayedCookingOrder;
            if (order?.Dish == null) { cookingHpFill.fillAmount = 0; cookingHpText.text = string.Empty; return; }
            int maximum = order.Dish.ClickRequirement;
            int remaining = order.RemainingClicks;
            int clicks = (remaining + session.ClickDamage - 1) / session.ClickDamage;
            cookingHpFill.fillAmount = maximum > 0 ? (float)remaining / maximum : 0;
            cookingHpText.text = $"HP: {remaining} / {maximum}\nЗалишилось кліків: {clicks}";
        }

        private void LayoutOrderCards()
        {
            int count = session.BoardCapacity;
            int columns = Mathf.Min(3, count);
            bool compact = count > 3;
            for (int i = 0; i < count; i++)
            {
                int row = i / columns;
                int inRow = Mathf.Min(columns, count - row * columns);
                var rt = orderButtons[i].GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(360, compact ? 300 : 540);
                rt.anchoredPosition = new Vector2(((i % columns) - (inRow - 1) * .5f) * 420, compact ? 150 - row * 330 : -15);
                orderLabels[i].rectTransform.sizeDelta = new Vector2(330, compact ? 80 : 135);
                orderLabels[i].rectTransform.anchoredPosition = new Vector2(0, compact ? 105 : 175);
                orderLabels[i].fontSize = compact ? 28 : 34;
                var plate = orderButtons[i].transform.Find("UIA2_Plate") as RectTransform;
                plate.sizeDelta = new Vector2(compact ? 150 : 260, compact ? 105 : 200);
                plate.anchoredPosition = new Vector2(0, compact ? 0 : 10);
                // Stretch nested illustration layers to the resized plate.
                orderMasteryTexts[i].rectTransform.sizeDelta = new Vector2(330, compact ? 45 : 65);
                orderMasteryTexts[i].rectTransform.anchoredPosition = new Vector2(0, compact ? -78 : -135);
                var track = orderMasteryFills[i].transform.parent as RectTransform;
                track.anchoredPosition = new Vector2(0, compact ? -125 : -210);
            }
        }

        private void FocusBoard()
        {
            foreach (var button in orderButtons)
                if (button.gameObject.activeInHierarchy && button.interactable) { FocusUi(button); return; }
            FocusUi(finishDayButton);
        }

        private void ConfigureBoardNavigation()
        {
            var available = new System.Collections.Generic.List<UnityEngine.UI.Button>();
            foreach (var button in orderButtons)
                if (button.gameObject.activeSelf && button.interactable) available.Add(button);
            for (int i = 0; i < available.Count; i++)
            {
                var n = available[i].navigation; n.mode = UnityEngine.UI.Navigation.Mode.Explicit;
                n.selectOnLeft = i > 0 ? available[i-1] : finishDayButton;
                n.selectOnRight = i+1 < available.Count ? available[i+1] : finishDayButton;
                n.selectOnUp = i >= 3 ? available[i-3] : finishDayButton;
                n.selectOnDown = i+3 < available.Count ? available[i+3] : finishDayButton;
                available[i].navigation = n;
            }
            var finish = finishDayButton.navigation; finish.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            finish.selectOnLeft = finish.selectOnUp = available.Count > 0 ? available[available.Count-1] : null;
            finish.selectOnRight = finish.selectOnDown = available.Count > 0 ? available[0] : null;
            finishDayButton.navigation = finish;
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

            locationNameText.text = ClickCookUiCopy.Location(location);
            locationLevelText.text = $"Рівень локації: {location.LevelRequirement}";
            progressText.text = $"Баланс: {ClickCookUiCopy.Money(session.Rewards.Wallet)}";
            dishSlotsText.text = ClickCookUiCopy.DishPreview(location);
            incomeText.text = $"Дохід локації: {location.PassiveIncomePerSecond} у.е./с";
        }

        private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture) + " у.е.";
    }
}
