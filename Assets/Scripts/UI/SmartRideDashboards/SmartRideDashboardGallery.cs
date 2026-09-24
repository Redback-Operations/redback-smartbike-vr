using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartRideDashboardGallery : MonoBehaviour
{
    [SerializeField] private bool buildOnAwake = true;
    [SerializeField] private int startingScreenIndex;
    [SerializeField] private Sprite[] dashboardScreens;

    private Image dashboardImage;
    private int currentIndex;

    private void Awake()
    {
        if (buildOnAwake && transform.childCount == 0)
            Build();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            NextScreen();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            PreviousScreen();

        for (int i = 0; i < dashboardScreens.Length && i < 9; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
                ShowScreen(i);
        }
    }

    [ContextMenu("Rebuild Smart Ride Dashboards")]
    public void Rebuild()
    {
        ClearChildren(transform);
        Build();
    }

    public void SetScreens(Sprite[] screens)
    {
        dashboardScreens = screens;
        if (dashboardImage != null)
            ShowScreen(currentIndex);
    }

    public void ShowScreen(int index)
    {
        if (dashboardScreens == null || dashboardScreens.Length == 0 || dashboardImage == null)
            return;

        currentIndex = Mathf.Clamp(index, 0, dashboardScreens.Length - 1);
        dashboardImage.sprite = dashboardScreens[currentIndex];
        dashboardImage.enabled = dashboardImage.sprite != null;
    }

    public void NextScreen()
    {
        if (dashboardScreens == null || dashboardScreens.Length == 0)
            return;

        ShowScreen((currentIndex + 1) % dashboardScreens.Length);
    }

    public void PreviousScreen()
    {
        if (dashboardScreens == null || dashboardScreens.Length == 0)
            return;

        int previous = currentIndex - 1;
        if (previous < 0)
            previous = dashboardScreens.Length - 1;
        ShowScreen(previous);
    }

    private void Build()
    {
        Canvas canvas = CreateCanvas(transform);

        GameObject imageObject = new GameObject("DashboardScreenImage");
        imageObject.transform.SetParent(canvas.transform, false);
        dashboardImage = imageObject.AddComponent<Image>();
        dashboardImage.color = Color.white;
        dashboardImage.preserveAspect = true;
        dashboardImage.raycastTarget = false;
        Stretch(imageObject.GetComponent<RectTransform>());

        CreateNavigationHitAreas(canvas.transform);

        if (dashboardScreens == null || dashboardScreens.Length == 0)
            CreateMissingScreensMessage(canvas.transform);
        else
            ShowScreen(startingScreenIndex);
    }

    private static Canvas CreateCanvas(Transform parent)
    {
        GameObject canvasObject = new GameObject("SmartRideDashboardsCanvas");
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1200;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        Stretch(canvasObject.GetComponent<RectTransform>());
        return canvas;
    }

    private void CreateNavigationHitAreas(Transform parent)
    {
        CreateHitArea(parent, "NavStartRide", new Vector2(1510, -190), new Vector2(360, 70), () => ShowScreen(0));
        CreateHitArea(parent, "NavOverview", new Vector2(1510, -270), new Vector2(410, 70), () => ShowScreen(3));
        CreateHitArea(parent, "NavRideAnalytics", new Vector2(1510, -350), new Vector2(470, 70), () => ShowScreen(4));
        CreateHitArea(parent, "NavMap", new Vector2(1510, -430), new Vector2(360, 70), () => ShowScreen(5));
        CreateHitArea(parent, "NavUserStatistics", new Vector2(1510, -515), new Vector2(500, 70), () => ShowScreen(6));
        CreateHitArea(parent, "NavTripDetails", new Vector2(1510, -595), new Vector2(500, 70), () => ShowScreen(7));
        CreateHitArea(parent, "NavInterfaceGuide", new Vector2(1510, -675), new Vector2(430, 70), () => ShowScreen(8));

        CreateHitArea(parent, "MenuNext", new Vector2(1828, -92), new Vector2(120, 120), NextScreen);
        CreateHitArea(parent, "BottomBack", new Vector2(286, -850), new Vector2(350, 110), PreviousScreen);
        CreateHitArea(parent, "BottomNext", new Vector2(706, -850), new Vector2(430, 110), NextScreen);
    }

    private static void CreateHitArea(Transform parent, string name, Vector2 topLeftAnchoredPosition, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.001f);
        image.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();
        button.transition = Selectable.Transition.None;
        button.onClick.AddListener(action);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = topLeftAnchoredPosition;
        rect.sizeDelta = size;
    }

    private static void CreateMissingScreensMessage(Transform parent)
    {
        GameObject textObject = new GameObject("MissingDashboardScreensMessage");
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = "Smart Ride dashboard sprites are not assigned.";
        text.fontSize = 42;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(900, 120);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
    }

    private static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
