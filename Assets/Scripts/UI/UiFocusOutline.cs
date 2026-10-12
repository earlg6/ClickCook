using UnityEngine;
using UnityEngine.EventSystems;

namespace ClickCook.UI
{
    /// <summary>Explicit keyboard focus cue; does not change button hit bounds.</summary>
    public sealed class UiFocusOutline : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private UnityEngine.UI.Outline focusOutline;
        private void OnEnable() { if (focusOutline != null) focusOutline.enabled = false; }
        private void OnDisable() { if (focusOutline != null) focusOutline.enabled = false; }
        public void OnSelect(BaseEventData data) { if (focusOutline != null) focusOutline.enabled = true; }
        public void OnDeselect(BaseEventData data) { if (focusOutline != null) focusOutline.enabled = false; }
    }
}
