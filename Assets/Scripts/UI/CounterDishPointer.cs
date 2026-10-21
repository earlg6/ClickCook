using ClickCook.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.UI
{
    /// <summary>Routes a click on the actual counter dish collider through the existing cooking guard.</summary>
    public sealed class CounterDishPointer : UnityEngine.UI.Selectable, IPointerClickHandler, ISubmitHandler
    {
        [SerializeField] private ClickCookPrototypeController controller;
        [SerializeField] private UnityEngine.UI.Text inputHint;

        public void OnPointerClick(PointerEventData data)
        {
            if (data.button == PointerEventData.InputButton.Left && IsActive() && IsInteractable() && controller != null)
                controller.ClickCookingDish();
        }

        public void OnSubmit(BaseEventData data)
        {
            if (IsActive() && IsInteractable() && controller != null)
                controller.ClickCookingDish();
        }

        public override void OnSelect(BaseEventData data)
        {
            base.OnSelect(data);
            if (inputHint != null) inputHint.text = IsBurger
                ? "Збирай бургер · клік або Enter · ↓ до замовлень"
                : "3D-ескіз · клік або Enter · ↓ до замовлень";
        }

        public override void OnDeselect(BaseEventData data)
        {
            base.OnDeselect(data);
            if (inputHint != null) inputHint.text = IsBurger
                ? "Натискай, щоб зібрати бургер"
                : "Тимчасовий 3D-ескіз · натискай на страву";
        }

        private bool IsBurger => controller != null && controller.OrderSession?.ActiveOrder?.Dish?.DishId == 6;
    }
}
