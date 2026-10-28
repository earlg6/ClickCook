using ClickCook.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace ClickCook.UI
{
    /// <summary>Minimal 7B main menu, reset confirmation and pause. Uses existing uGUI art.</summary>
    public sealed class ProfileMenu : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel, pausePanel, confirmationPanel;
        [SerializeField] private UnityEngine.UI.Text statusText, saveWarning;
        [SerializeField] private UnityEngine.UI.Button continueButton, newGameButton, quitButton;
        [SerializeField] private UnityEngine.UI.Button pauseButton, resumeButton, menuButton, pauseQuitButton;
        [SerializeField] private UnityEngine.UI.Button confirmButton, cancelButton, retryButton;
        private ClickCookPrototypeController controller;
        private GameObject previousFocus;
        private float previousTimeScale;
        private bool paused;
        public bool IsBlocking => paused || (mainPanel != null && mainPanel.activeSelf);
        public bool IsPaused => paused;

        private void Start()
        {
            controller = GetComponent<ClickCookPrototypeController>();
            continueButton.onClick.AddListener(Continue);
            newGameButton.onClick.AddListener(RequestNewGame);
            quitButton.onClick.AddListener(Quit);
            pauseButton.onClick.AddListener(Pause);
            resumeButton.onClick.AddListener(Resume);
            menuButton.onClick.AddListener(MainMenu);
            pauseQuitButton.onClick.AddListener(Quit);
            confirmButton.onClick.AddListener(ConfirmNewGame);
            cancelButton.onClick.AddListener(CancelNewGame);
            retryButton.onClick.AddListener(RetrySave);
            pausePanel.SetActive(false); confirmationPanel.SetActive(false);
            mainPanel.SetActive(true);
            Refresh();
            Focus(continueButton.interactable ? continueButton : newGameButton);
        }
        private static void Focus(UnityEngine.UI.Button button)
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
        private void Update()
        {
            if (controller == null) return;
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                if (confirmationPanel.activeSelf) CancelNewGame();
                else if (paused) Resume();
                else if (!mainPanel.activeSelf) Pause();
            }
            pauseButton.gameObject.SetActive(!IsBlocking);
            saveWarning.transform.parent.gameObject.SetActive(PlayerProfileRuntime.IsDirty);
            retryButton.gameObject.SetActive(PlayerProfileRuntime.IsDirty);
        }
        private void Refresh()
        {
            continueButton.interactable = PlayerProfileRuntime.HasProgress;
            var buttons = continueButton.interactable ? new[] { continueButton, newGameButton, quitButton } : new[] { newGameButton, quitButton };
            for (int i = 0; i < buttons.Length; i++)
            {
                var nav = new UnityEngine.UI.Navigation { mode = UnityEngine.UI.Navigation.Mode.Explicit };
                nav.selectOnUp = nav.selectOnLeft = buttons[(i + buttons.Length - 1) % buttons.Length];
                nav.selectOnDown = nav.selectOnRight = buttons[(i + 1) % buttons.Length];
                buttons[i].navigation = nav;
            }
            var status = PlayerProfileRuntime.Store.Status;
            statusText.text = PlayerProfileRuntime.IsDirty ? "Не вдалося зберегти. Прогрес у пам'яті; натисни «Повторити запис»." :
                PlayerProfileRuntime.Store.LastError != null ? "Не вдалося записати. Перевір доступ до диска й повтори дію." :
                status == ProfileLoadStatus.Unsupported ? "Збереження з новішої версії гри. Continue недоступне; файл збережено." :
                status == ProfileLoadStatus.Invalid ? "Збереження та резервна копія пошкоджені. Нову гру потрібно підтвердити." :
                status == ProfileLoadStatus.Unavailable ? "Немає доступу до збереження. Перевір доступ і перезапусти гру." :
                PlayerProfileRuntime.RecoveredBackup ? "Відновлено резервну копію. Остання зміна могла не зберегтися." :
                PlayerProfileRuntime.HasProgress ? $"Баланс: {ClickCookUiCopy.Money(PlayerProfileRuntime.Current.Rewards.Wallet)}\nПрогрес зберігається автоматично." :
                "Почни нову гру. Гроші, досвід страв, рецепти й покращення зберігатимуться автоматично.";
        }
        public void Continue()
        {
            if (!mainPanel.activeSelf || confirmationPanel.activeSelf || !PlayerProfileRuntime.HasProgress) return;
            mainPanel.SetActive(false);
            controller.ReturnToProfileMenu();
            controller.FocusLocationMenu();
        }
        public void RequestNewGame()
        {
            if (!mainPanel.activeSelf || confirmationPanel.activeSelf) return;
            confirmationPanel.SetActive(true); Focus(cancelButton);
        }
        public void CancelNewGame()
        {
            if (!confirmationPanel.activeSelf) return;
            confirmationPanel.SetActive(false); Focus(newGameButton);
        }
        public void ConfirmNewGame()
        {
            if (!mainPanel.activeSelf || !confirmationPanel.activeSelf) return;
            if (!PlayerProfileRuntime.NewGame()) { CancelNewGame(); Refresh(); return; }
            confirmationPanel.SetActive(false);
            controller.BindNewProfile();
            Refresh(); Continue();
        }
        public void Pause()
        {
            if (controller == null || IsBlocking) return;
            previousFocus = EventSystem.current?.currentSelectedGameObject;
            previousTimeScale = Time.timeScale; Time.timeScale = 0;
            paused = true; controller.OrderSession.IsPaused = true;
            pausePanel.SetActive(true); Focus(resumeButton);
        }
        public void Resume()
        {
            if (!paused) return;
            paused = false; controller.OrderSession.IsPaused = false;
            Time.timeScale = previousTimeScale; pausePanel.SetActive(false);
            if (previousFocus != null && previousFocus.activeInHierarchy && EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(previousFocus);
        }
        public void MainMenu()
        {
            if (!paused) return;
            Resume();
            controller.ReturnToProfileMenu();
            PlayerProfileRuntime.SaveNow();
            mainPanel.SetActive(true); Refresh(); Focus(continueButton.interactable ? continueButton : newGameButton);
        }
        public void RetrySave() { PlayerProfileRuntime.SaveNow(); Refresh(); }
        public void Quit()
        {
            if (!IsBlocking || confirmationPanel.activeSelf) return;
            if (!PlayerProfileRuntime.SaveNow()) { Refresh(); return; }
            Resume();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        private void OnApplicationFocus(bool focused)
        {
            if (!focused && controller != null && !IsBlocking) { Pause(); PlayerProfileRuntime.SaveNow(); }
        }
        private void OnApplicationQuit() => PlayerProfileRuntime.SaveNow();
        private void OnDestroy() { if (paused) Time.timeScale = previousTimeScale; }
    }
}
