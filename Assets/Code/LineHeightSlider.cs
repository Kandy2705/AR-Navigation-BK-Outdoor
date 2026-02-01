using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LineHeightSlider : MonoBehaviour
{
    [SerializeField] private NewIndoorNav navigation;
    [SerializeField] private Slider slider;
    [SerializeField] private bool autoCreateSlider = true;
    [SerializeField] private float minOffset = -0.5f;
    [SerializeField] private float maxOffset = 0.2f;
    [SerializeField] private Vector2 sliderSize = new Vector2(520f, 30f);
    [SerializeField] private Vector2 sliderAnchorPosition = new Vector2(0f, -140f);

    private void Start()
    {
        if (navigation == null)
        {
            navigation = FindObjectOfType<NewIndoorNav>();
        }

        if (slider == null && autoCreateSlider)
        {
            slider = CreateSliderUI();
        }

        if (navigation == null || slider == null)
        {
            return;
        }

        slider.minValue = minOffset;
        slider.maxValue = maxOffset;
        slider.value = navigation.LineVerticalOffset;
        slider.onValueChanged.AddListener(HandleSliderChanged);
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(HandleSliderChanged);
        }
    }

    private void HandleSliderChanged(float value)
    {
        if (navigation != null)
        {
            navigation.SetLineVerticalOffset(value);
        }
    }

    private Slider CreateSliderUI()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("LineHeightSliderCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        GameObject sliderObject = new GameObject("LineHeightSlider");
        sliderObject.transform.SetParent(canvas.transform, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.sizeDelta = sliderSize;
        sliderRect.anchoredPosition = sliderAnchorPosition;

        Slider createdSlider = sliderObject.AddComponent<Slider>();

        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(sliderObject.transform, false);
        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = new Color(0f, 0f, 0f, 0.4f);
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = new Vector2(0f, 0.25f);
        backgroundRect.anchorMax = new Vector2(1f, 0.75f);
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(10f, 0f);
        fillAreaRect.offsetMax = new Vector2(-10f, 0f);

        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform, false);
        Image fillImage = fillObject.AddComponent<Image>();
        fillImage.color = new Color(0.2f, 0.9f, 0.3f, 0.9f);
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0f, 0f);
        fillRect.anchorMax = new Vector2(1f, 1f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        GameObject handleSlideAreaObject = new GameObject("Handle Slide Area");
        handleSlideAreaObject.transform.SetParent(sliderObject.transform, false);
        RectTransform handleSlideRect = handleSlideAreaObject.AddComponent<RectTransform>();
        handleSlideRect.anchorMin = new Vector2(0f, 0f);
        handleSlideRect.anchorMax = new Vector2(1f, 1f);
        handleSlideRect.offsetMin = Vector2.zero;
        handleSlideRect.offsetMax = Vector2.zero;

        GameObject handleObject = new GameObject("Handle");
        handleObject.transform.SetParent(handleSlideAreaObject.transform, false);
        Image handleImage = handleObject.AddComponent<Image>();
        handleImage.color = new Color(1f, 1f, 1f, 0.9f);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(24f, 24f);

        createdSlider.fillRect = fillRect;
        createdSlider.handleRect = handleRect;
        createdSlider.targetGraphic = handleImage;
        createdSlider.direction = Slider.Direction.LeftToRight;

        return createdSlider;
    }
}