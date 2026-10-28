using ClickCook.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.UI
{
    /// <summary>Discrete clicks/Submit and purchased pointer hold share the guarded manual-hit path.</summary>
    public sealed class CounterDishPointer : UnityEngine.UI.Selectable, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private ClickCookPrototypeController controller;
        [SerializeField] private UnityEngine.UI.Text inputHint;
        private int? heldPointerId;
        private bool suppressReleaseClick;

        public override void OnPointerDown(PointerEventData data)
        {
            base.OnPointerDown(data);
            if (data.button != PointerEventData.InputButton.Left || !CanAccept) return;
            // A fresh press in a reset/unowned profile must not inherit a consumed gesture from the previous run.
            if (!controller.OrderSession.HasHold) { suppressReleaseClick = false; return; }
            suppressReleaseClick = true;
            if (controller.OrderSession.IsHolding) return;
            if (controller.BeginCookingHold()) heldPointerId = data.pointerId;
        }
        public override void OnPointerUp(PointerEventData data)
        {
            base.OnPointerUp(data);
            if (data.button == PointerEventData.InputButton.Left && heldPointerId == data.pointerId) CancelHold();
        }
        public override void OnPointerExit(PointerEventData data)
        {
            base.OnPointerExit(data);
            if (heldPointerId == data.pointerId) CancelHold();
        }
        public void CancelHold() { heldPointerId = null; controller?.CancelCookingHold(); }
        protected override void OnDisable() { CancelHold(); base.OnDisable(); }
        private void OnApplicationFocus(bool focused) { if (!focused) CancelHold(); }
        private void OnApplicationPause(bool paused) { if (paused) CancelHold(); }
        private bool CanAccept => IsActive() && IsInteractable() && controller != null && controller.CanCookDisplayedOrder;

        public void OnPointerClick(PointerEventData data)
        {
            if (data.button != PointerEventData.InputButton.Left) return;
            // With Hold, down already applied the initial hit. Release/click can never add a second one.
            if (suppressReleaseClick || controller?.OrderSession?.HasHold == true) { suppressReleaseClick = false; return; }
            if (CanAccept) controller.ClickCookingDish();
        }

        public void OnSubmit(BaseEventData data)
        {
            if (CanAccept) controller.ClickCookingDish();
        }

        public override void OnSelect(BaseEventData data)
        {
            base.OnSelect(data);
            if (inputHint != null && controller?.OrderSession?.HasHold == true)
            { inputHint.text = "Утримуй ЛКМ · Enter — один рух · ↓ до замовлень"; return; }
            if (inputHint != null) inputHint.text = IsBurger
                ? "Збирай бургер · клік або Enter · ↓ до замовлень"
                : "3D-ескіз · клік або Enter · ↓ до замовлень";
        }

        public override void OnDeselect(BaseEventData data)
        {
            base.OnDeselect(data);
            CancelHold();
            if (inputHint != null && controller?.OrderSession?.HasHold == true)
            { inputHint.text = $"Утримуй ЛКМ на страві · {controller.OrderSession.HoldFrequency:0.#} рухи/с"; return; }
            if (inputHint != null) inputHint.text = IsBurger
                ? "Натискай, щоб зібрати бургер"
                : "Тимчасовий 3D-ескіз · натискай на страву";
        }

        private bool IsBurger => controller != null && controller.OrderSession?.ActiveOrder?.Dish?.DishId == 6;
    }
}
