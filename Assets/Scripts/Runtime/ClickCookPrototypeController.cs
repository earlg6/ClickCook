using ClickCook.Data;
using ClickCook.Runtime.Locations;
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
        [SerializeField] private UnityEngine.UI.Text finishDayMessageText;
        [SerializeField] private UnityEngine.UI.Button previousButton;
        [SerializeField] private UnityEngine.UI.Button nextButton;
        [SerializeField] private UnityEngine.UI.Button cookButton;
        [SerializeField] private UnityEngine.UI.Button finishDayButton;

        private readonly PrototypeSession session = new();
        private int currentLocationIndex;

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

            locationSelectionScreen.SetActive(true);
            workDayScreen.SetActive(false);
            RefreshLocationSelection();
        }

        private bool HasRequiredReferences()
        {
            return locationCatalog != null &&
                   locationSelectionScreen != null &&
                   workDayScreen != null &&
                   locationNameText != null &&
                   locationLevelText != null &&
                   progressText != null &&
                   dishSlotsText != null &&
                   incomeText != null &&
                   workDayLocationText != null &&
                   finishDayMessageText != null &&
                   previousButton != null &&
                   nextButton != null &&
                   cookButton != null &&
                   finishDayButton != null;
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
            if (selectedLocation == null)
            {
                return;
            }

            session.SetSelectedLocation(selectedLocation);
            locationSelectionScreen.SetActive(false);
            workDayScreen.SetActive(true);
            workDayLocationText.text = $"Selected location: {selectedLocation.EnglishName}";
            finishDayMessageText.text = string.Empty;
        }

        public void FinishDay()
        {
            finishDayMessageText.text = "Finish Day is a prototype-only action.";
        }

        private LocationData CurrentLocation =>
            locationCatalog != null && locationCatalog.Locations.Count > 0
                ? locationCatalog.Locations[currentLocationIndex]
                : null;

        private void SelectRelativeLocation(int offset)
        {
            if (locationCatalog == null || locationCatalog.Locations.Count == 0)
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

        private sealed class PrototypeSession
        {
            public LocationData SelectedLocation { get; private set; }

            public void SetSelectedLocation(LocationData location)
            {
                SelectedLocation = location;
            }
        }
    }
}
