using UnityEngine;

namespace ClickCook.UI
{
    /// <summary>Visual assembly driven by accepted cooking damage. Never advances gameplay itself.</summary>
    public sealed class BurgerAssemblyPresentation : MonoBehaviour
    {
        [SerializeField] private Transform[] layers;
        [SerializeField, Min(.01f)] private float settleSeconds = .22f;
        [SerializeField, Min(0f)] private float ingredientDropHeight = .09f;
        [SerializeField, Range(0f, .1f)] private float clickSquash = .035f;
        private Vector3[] restPositions;
        private Vector3 restScale;
        private object orderIdentity;
        private float targetProgress, punchTime;

        public float DisplayedProgress { get; private set; }

        private void Initialize()
        {
            if (restPositions != null) return;
            restScale = transform.localScale;
            restPositions = new Vector3[layers.Length];
            for (int i = 0; i < layers.Length; i++) restPositions[i] = layers[i].localPosition;
        }

        public void Present(object identity, float progress)
        {
            if (layers == null || layers.Length < 2 || identity == null) return;
            Initialize();
            progress = Mathf.Clamp01(progress);
            if (!ReferenceEquals(orderIdentity, identity))
            {
                orderIdentity = identity;
                DisplayedProgress = 0f;
                targetProgress = 0f;
                punchTime = 0f;
            }
            if (progress > targetProgress) punchTime = settleSeconds;
            targetProgress = progress;
            DisplayedProgress = Mathf.MoveTowards(DisplayedProgress, targetProgress,
                Time.deltaTime / Mathf.Max(.01f, settleSeconds));
            punchTime = Mathf.Max(0f, punchTime - Time.deltaTime);
            float punch = Mathf.Sin(Mathf.PI * punchTime / Mathf.Max(.01f, settleSeconds)) * clickSquash;
            transform.localScale = Vector3.Scale(restScale, new Vector3(1f + punch, 1f - punch, 1f + punch));

            layers[0].gameObject.SetActive(true);
            layers[0].localPosition = restPositions[0];
            for (int i = 1; i < layers.Length; i++)
            {
                float stage = Mathf.Clamp01(DisplayedProgress * (layers.Length - 1) - (i - 1));
                layers[i].gameObject.SetActive(stage > 0f);
                float ease = stage * stage * (3f - 2f * stage);
                layers[i].localPosition = restPositions[i] + Vector3.up * ingredientDropHeight * (1f - ease);
            }
        }

        private void OnDisable()
        {
            orderIdentity = null;
            DisplayedProgress = targetProgress = punchTime = 0f;
            if (restPositions == null) return;
            transform.localScale = restScale;
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i].localPosition = restPositions[i];
                layers[i].gameObject.SetActive(i == 0);
            }
        }
    }
}
