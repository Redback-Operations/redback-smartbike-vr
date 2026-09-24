using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RideAnalyticsHudBuilder : MonoBehaviour
{
    [SerializeField] private bool buildOnAwake = true;
    [SerializeField] private RideAnalyticsManager analyticsManager;
    [SerializeField] private bool showAdvancedCenterStats = false;

    private static readonly Color White = Color.white;
    private static readonly Color HudPanel = new Color(0.02f, 0.05f, 0.08f, 0.74f);
    private static readonly Color HudPanelStrong = new Color(0.015f, 0.03f, 0.05f, 0.9f);
    private static readonly Color TextMuted = new Color(0.78f, 0.87f, 0.95f, 1f);
    private static readonly Color AccentCyan = new Color(0.25f, 0.78f, 0.95f, 1f);
    private static readonly Color AccentGreen = new Color(0.2f, 0.86f, 0.45f, 1f);
    private static readonly Color AccentAmber = new Color(1f, 0.75f, 0.22f, 1f);
    private static readonly Color AccentRed = new Color(1f, 0.34f, 0.34f, 1f);
    private static readonly Color MenuBlue = new Color(0.06f, 0.18f, 0.32f, 0.96f);
    private static readonly Color ProgressTrack = new Color(0.85f, 0.93f, 1f, 0.28f);
    private static readonly Color ProgressFill = new Color(0.2f, 0.86f, 0.45f, 0.98f);
    private static readonly Color Scrim = new Color(0.005f, 0.012f, 0.02f, 0.62f);
    private static readonly Color Stroke = new Color(0.58f, 0.82f, 1f, 0.24f);
    private static Sprite circleSprite;

    private void Awake()
    {
        if (buildOnAwake && transform.childCount == 0)
            Build(gameObject, analyticsManager);
    }

    [ContextMenu("Rebuild Ride Analytics HUD")]
    public void Rebuild()
    {
        ClearChildren(transform);
        Build(gameObject, analyticsManager);
    }

    public static RideAnalyticsDashboard Build(GameObject root, RideAnalyticsManager manager)
    {
        if (manager == null)
            manager = root.GetComponent<RideAnalyticsManager>();
        if (manager == null)
            manager = root.AddComponent<RideAnalyticsManager>();
        if (root.GetComponent<RideAnalyticsApiClient>() == null)
            root.AddComponent<RideAnalyticsApiClient>();

        Canvas canvas = CreateCanvas(root.transform);
        GameObject dashboardObject = new GameObject("RideAnalyticsDashboard");
        dashboardObject.transform.SetParent(canvas.transform, false);
        Stretch(dashboardObject.AddComponent<RectTransform>());

        RideAnalyticsDashboard dashboard = dashboardObject.AddComponent<RideAnalyticsDashboard>();

        TMP_Text currentSpeed = CreateMetric(dashboardObject.transform, "Current Speed", "0.0 km/h", new Vector2(22, -18), TextAnchor.UpperLeft, White);
        TMP_Text averageSpeed = CreateMetric(dashboardObject.transform, "Average Speed", "0.0 km/h", new Vector2(22, -250), TextAnchor.UpperLeft, White);
        TMP_Text maxSpeed = CreateMetric(dashboardObject.transform, "Max Speed", "0.0 km/h", new Vector2(22, -482), TextAnchor.UpperLeft, White);
        TMP_Text distance = CreateMetric(dashboardObject.transform, "Distance", "0.00 km", new Vector2(22, -713), TextAnchor.UpperLeft, White);

        TMP_Text duration = CreateMetric(dashboardObject.transform, "Ride duration", "00:00:00", new Vector2(1345, -18), TextAnchor.UpperLeft, White);
        TMP_Text calories = CreateMetric(dashboardObject.transform, "Calories burned", "0 kcal", new Vector2(1345, -248), TextAnchor.UpperLeft, White);
        TMP_Text heartRate = CreateMetric(dashboardObject.transform, "Heart Rate", "0 bpm", new Vector2(1345, -481), TextAnchor.UpperLeft, White);
        TMP_Text progress = CreateMetric(dashboardObject.transform, "Distance Progress", "0%", new Vector2(1345, -711), TextAnchor.UpperLeft, White);

        TMP_Text mission = null;
        TMP_Text checkpoints = null;
        TMP_Text personalBest = null;

        if (root.GetComponent<RideAnalyticsHudBuilder>() != null && root.GetComponent<RideAnalyticsHudBuilder>().showAdvancedCenterStats)
        {
            mission = CreateSmallMetric(dashboardObject.transform, "Mission", "Free Ride", new Vector2(0, -82), TextAnchor.UpperCenter);
            checkpoints = CreateSmallMetric(dashboardObject.transform, "Checkpoints", "0/8", new Vector2(0, -172), TextAnchor.UpperCenter);
            personalBest = CreateSmallMetric(dashboardObject.transform, "Personal Best", "45.4 km/h", new Vector2(0, -262), TextAnchor.UpperCenter);
        }

        Image fill = CreateProgressBar(dashboardObject.transform);
        TMP_Text gear = CreateGearDisplay(dashboardObject.transform);

        CreateSpeedIcon(dashboardObject.transform);
        CreateMenuButton(dashboardObject.transform);
        TMP_Text pauseNav = CreateBottomNav(dashboardObject.transform, dashboard);
        GameObject pausePage = CreatePausePage(dashboardObject.transform, dashboard, out TMP_Text pauseSummary);
        GameObject mapPage = CreateMapPage(dashboardObject.transform, dashboard);
        GameObject analyticsPage = CreateAnalyticsPage(dashboardObject.transform, dashboard, out TMP_Text analyticsSummary);
        GameObject endRidePage = CreateEndRidePage(dashboardObject.transform, dashboard, out TMP_Text endRideSummary);

        dashboard.Bind(manager, currentSpeed, averageSpeed, maxSpeed, distance, duration, calories, heartRate, progress, gear, mission, checkpoints, personalBest, fill);
        dashboard.BindPages(pausePage, mapPage, analyticsPage, endRidePage, pauseNav, pauseSummary, analyticsSummary, endRideSummary);
        return dashboard;
    }

    private static Canvas CreateCanvas(Transform parent)
    {
        GameObject canvasObject = new GameObject("RideAnalyticsCanvas");
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        Stretch(canvasObject.GetComponent<RectTransform>());
        return canvas;
    }

    private static TMP_Text CreateMetric(Transform parent, string title, string value, Vector2 offset, TextAnchor alignment, Color valueColor)
    {
        GameObject group = new GameObject(title.Replace(" ", "") + "Metric");
        group.transform.SetParent(parent, false);
        RectTransform groupRect = group.AddComponent<RectTransform>();
        ConfigureAnchored(groupRect, offset, alignment, new Vector2(555, 150));

        TMP_Text titleText = CreateText(group.transform, title + "Title", title, 66, FontStyles.Bold, alignment, White);
        TMP_Text valueText = CreateText(group.transform, title + "Value", value, 64, FontStyles.Bold, alignment, valueColor);

        ConfigureChild(titleText.rectTransform, Vector2.zero, alignment, new Vector2(555, 72));
        ConfigureChild(valueText.rectTransform, new Vector2(0, -78), alignment, new Vector2(555, 72));
        return valueText;
    }

    private static TMP_Text CreateSmallMetric(Transform parent, string title, string value, Vector2 offset, TextAnchor alignment)
    {
        GameObject panel = CreatePanel(parent, title.Replace(" ", "") + "SmallPanel", offset, new Vector2(330, 82), HudPanel, alignment);
        TMP_Text titleText = CreateText(panel.transform, title + "Title", title, 19, FontStyles.Bold, alignment, TextMuted);
        TMP_Text valueText = CreateText(panel.transform, title + "Value", value, 24, FontStyles.Bold, alignment, White);

        ConfigureChild(titleText.rectTransform, new Vector2(18, -10), alignment, new Vector2(294, 28));
        ConfigureChild(valueText.rectTransform, new Vector2(18, -42), alignment, new Vector2(294, 32));
        return valueText;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float size, FontStyles style, TextAnchor anchor, Color color)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        TMP_Text label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.fontStyle = style;
        label.color = color;
        label.alignment = ToTextAlignment(anchor);
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.enableAutoSizing = true;
        label.fontSizeMax = size;
        label.fontSizeMin = Mathf.Max(14f, size * 0.62f);
        label.raycastTarget = false;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.72f);
        shadow.effectDistance = new Vector2(2f, -2f);

        return label;
    }

    private static TextAlignmentOptions ToTextAlignment(TextAnchor anchor)
    {
        if (anchor == TextAnchor.UpperRight)
            return TextAlignmentOptions.TopRight;
        if (anchor == TextAnchor.UpperCenter)
            return TextAlignmentOptions.Top;
        return TextAlignmentOptions.TopLeft;
    }

    private static void ConfigureAnchored(RectTransform rect, Vector2 offset, TextAnchor alignment, Vector2 size)
    {
        if (alignment == TextAnchor.UpperRight)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
        }
        else if (alignment == TextAnchor.UpperCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
        }
        else if (alignment == TextAnchor.LowerCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
        }
        else
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
        }

        rect.anchoredPosition = offset;
        rect.sizeDelta = size;
    }

    private static void ConfigureChild(RectTransform rect, Vector2 inset, TextAnchor alignment, Vector2 size)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = inset;
        rect.sizeDelta = size;

        if (alignment == TextAnchor.UpperRight)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-inset.x, inset.y);
        }
        else if (alignment == TextAnchor.UpperCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, inset.y);
        }
    }

    private static Image CreateProgressBar(Transform parent)
    {
        GameObject trackObject = new GameObject("DistanceProgressTrack");
        trackObject.transform.SetParent(parent, false);
        Image track = trackObject.AddComponent<Image>();
        track.color = Color.clear;
        RectTransform trackRect = trackObject.GetComponent<RectTransform>();
        trackRect.anchorMin = new Vector2(1f, 1f);
        trackRect.anchorMax = new Vector2(1f, 1f);
        trackRect.pivot = new Vector2(1f, 1f);
        trackRect.anchoredPosition = new Vector2(-72, -843);
        trackRect.sizeDelta = new Vector2(500, 20);

        GameObject fillObject = new GameObject("DistanceProgressFill");
        fillObject.transform.SetParent(trackObject.transform, false);
        Image fill = fillObject.AddComponent<Image>();
        fill.color = Color.clear;
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillOrigin = 0;
        fill.fillAmount = 0.62f;
        Stretch(fillObject.GetComponent<RectTransform>());
        return fill;
    }

    private static TMP_Text CreateGearDisplay(Transform parent)
    {
        GameObject panelObject = new GameObject("GearPanel");
        panelObject.transform.SetParent(parent, false);
        Image panel = panelObject.AddComponent<Image>();
        panel.color = Color.black;
        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 182f);
        rect.sizeDelta = new Vector2(97f, 140f);

        GameObject stemObject = new GameObject("GearStem");
        stemObject.transform.SetParent(parent, false);
        Image stem = stemObject.AddComponent<Image>();
        stem.color = Color.white;
        RectTransform stemRect = stemObject.GetComponent<RectTransform>();
        stemRect.anchorMin = new Vector2(0.5f, 0f);
        stemRect.anchorMax = new Vector2(0.5f, 0f);
        stemRect.pivot = new Vector2(0.5f, 0f);
        stemRect.anchoredPosition = new Vector2(0f, 80f);
        stemRect.sizeDelta = new Vector2(97f, 118f);

        TMP_Text label = CreateText(panelObject.transform, "GearLabel", "GEAR", 18, FontStyles.Bold, TextAnchor.UpperCenter, new Color(0.65f, 0.68f, 0.72f, 1f));
        ConfigureAnchored(label.rectTransform, new Vector2(0, -7), TextAnchor.UpperCenter, new Vector2(78, 24));

        TMP_Text value = CreateText(panelObject.transform, "GearValue", "6", 88, FontStyles.Bold, TextAnchor.UpperCenter, new Color(0.08f, 0.12f, 1f, 1f));
        ConfigureAnchored(value.rectTransform, new Vector2(0, -34), TextAnchor.UpperCenter, new Vector2(88, 92));
        return value;
    }

    private static void CreateSpeedIcon(Transform parent)
    {
        GameObject iconRoot = new GameObject("SpeedIcon");
        iconRoot.transform.SetParent(parent, false);
        RectTransform rect = iconRoot.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(515, -33);
        rect.sizeDelta = new Vector2(109, 109);

        GameObject tile = new GameObject("Tile");
        tile.transform.SetParent(iconRoot.transform, false);
        Image tileImage = tile.AddComponent<Image>();
        tileImage.color = new Color(1f, 1f, 1f, 0.68f);
        Stretch(tile.GetComponent<RectTransform>());

        GameObject circle = new GameObject("Dial");
        circle.transform.SetParent(iconRoot.transform, false);
        Image circleImage = circle.AddComponent<Image>();
        circleImage.sprite = GetCircleSprite();
        circleImage.color = new Color(0.02f, 0.47f, 0.64f, 0.96f);
        RectTransform circleRect = circle.GetComponent<RectTransform>();
        circleRect.anchorMin = new Vector2(0.5f, 0.5f);
        circleRect.anchorMax = new Vector2(0.5f, 0.5f);
        circleRect.pivot = new Vector2(0.5f, 0.5f);
        circleRect.sizeDelta = new Vector2(62, 62);

        CreateLine(iconRoot.transform, "Needle", new Vector2(55, 55), new Vector2(8, 44), -42f, White);
        CreateLine(iconRoot.transform, "SpeedLine1", new Vector2(9, 46), new Vector2(30, 5), 0f, new Color(0f, 0.54f, 0.45f));
        CreateLine(iconRoot.transform, "SpeedLine2", new Vector2(13, 64), new Vector2(25, 5), 0f, new Color(0f, 0.54f, 0.45f));
        CreateLine(iconRoot.transform, "SpeedLine3", new Vector2(20, 82), new Vector2(20, 5), 0f, new Color(0f, 0.54f, 0.45f));
    }

    private static void CreateMenuButton(Transform parent)
    {
        GameObject button = new GameObject("MenuButton");
        button.transform.SetParent(parent, false);
        Image image = button.AddComponent<Image>();
        image.sprite = GetCircleSprite();
        image.color = new Color(0.14f, 0.55f, 0.83f, 1f);
        RectTransform rect = button.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-36, -33);
        rect.sizeDelta = new Vector2(100, 100);

        CreateLine(button.transform, "Bar1", new Vector2(50, 69), new Vector2(58, 12), 0f, White);
        CreateLine(button.transform, "Bar2", new Vector2(50, 50), new Vector2(58, 12), 0f, White);
        CreateLine(button.transform, "Bar3", new Vector2(50, 31), new Vector2(58, 12), 0f, White);
    }

    private static TMP_Text CreateBottomNav(Transform parent, RideAnalyticsDashboard dashboard)
    {
        TMP_Text pause = CreateNavText(parent, "Pause", new Vector2(-350, 11), new Color(0.91f, 0.07f, 0.30f, 1f), dashboard.TogglePause);
        CreateNavText(parent, "Map", new Vector2(-158, 11), new Color(0.15f, 0.27f, 0.6f, 1f), dashboard.ShowMap);
        CreateNavText(parent, "Analytics", new Vector2(95, 11), new Color(0.91f, 0.88f, 0.13f, 1f), dashboard.ShowAnalytics);
        CreateNavText(parent, "End Ride", new Vector2(430, 11), Color.black, dashboard.EndRide);
        return pause;
    }

    private static TMP_Text CreateNavText(Transform parent, string text, Vector2 offset, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = new GameObject("Nav" + text.Replace(" ", "") + "Button");
        buttonObject.transform.SetParent(parent, false);
        RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
        ConfigureAnchored(buttonRect, offset, TextAnchor.LowerCenter, new Vector2(text == "Analytics" ? 330 : 270, 88));
        Image hitArea = buttonObject.AddComponent<Image>();
        hitArea.color = Color.clear;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = hitArea;
        button.onClick.AddListener(onClick);
        ConfigureButtonColors(button, color);

        TMP_Text label = CreateText(buttonObject.transform, "Label", text, 62, FontStyles.Bold, TextAnchor.UpperCenter, color);
        RectTransform rect = label.rectTransform;
        Stretch(rect);
        return label;
    }

    private static GameObject CreatePausePage(Transform parent, RideAnalyticsDashboard dashboard, out TMP_Text summary)
    {
        GameObject page = CreatePage(parent, "PausePage");
        CreateText(page.transform, "PauseTitle", "Ride Paused", 58, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(page.transform.Find("PauseTitle").GetComponent<RectTransform>(), new Vector2(0, -112), TextAnchor.UpperCenter, new Vector2(820, 80));

        GameObject panel = CreatePanel(page.transform, "PauseSummaryPanel", new Vector2(0, -245), new Vector2(760, 370), HudPanelStrong, TextAnchor.UpperCenter);
        AddOutline(panel, Stroke, new Vector2(1.5f, -1.5f));

        summary = CreateText(panel.transform, "PauseSummary", "Ride paused", 38, FontStyles.Bold, TextAnchor.UpperCenter, White);
        Stretch(summary.rectTransform);
        summary.rectTransform.offsetMin = new Vector2(44, 34);
        summary.rectTransform.offsetMax = new Vector2(-44, -34);

        CreatePageButton(page.transform, "Back to Ride", new Vector2(0, 100), new Vector2(360, 74), dashboard.ClosePages);
        page.SetActive(false);
        return page;
    }

    private static GameObject CreateMapPage(Transform parent, RideAnalyticsDashboard dashboard)
    {
        GameObject page = CreatePage(parent, "MapPage");
        CreateText(page.transform, "MapTitle", "Route Map", 58, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(page.transform.Find("MapTitle").GetComponent<RectTransform>(), new Vector2(0, -82), TextAnchor.UpperCenter, new Vector2(700, 80));

        GameObject mapPanel = CreatePanel(page.transform, "MapPanel", new Vector2(0, -190), new Vector2(920, 560), new Color(0.88f, 0.92f, 0.86f, 0.96f), TextAnchor.UpperCenter);
        CreateLine(mapPanel.transform, "MapRoad1", new Vector2(165, 120), new Vector2(460, 12), 23f, new Color(0.72f, 0.72f, 0.68f));
        CreateLine(mapPanel.transform, "MapRoad2", new Vector2(430, 285), new Vector2(600, 12), -18f, new Color(0.72f, 0.72f, 0.68f));
        CreateLine(mapPanel.transform, "MapRoute1", new Vector2(230, 155), new Vector2(340, 16), 28f, AccentCyan);
        CreateLine(mapPanel.transform, "MapRoute2", new Vector2(455, 300), new Vector2(360, 16), -15f, AccentCyan);
        CreateMapMarker(mapPanel.transform, new Vector2(86, 82), Color.red);
        CreateMapMarker(mapPanel.transform, new Vector2(790, 365), new Color(0.1f, 0.75f, 0.24f));

        TMP_Text details = CreateText(page.transform, "MapDetails", "Current route\nCity ride loop\nCheckpoints update from distance progress", 34, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(details.rectTransform, new Vector2(0, -775), TextAnchor.UpperCenter, new Vector2(920, 130));
        CreatePageButton(page.transform, "Back to Ride", new Vector2(0, 70), new Vector2(360, 74), dashboard.ClosePages);
        page.SetActive(false);
        return page;
    }

    private static GameObject CreateAnalyticsPage(Transform parent, RideAnalyticsDashboard dashboard, out TMP_Text summary)
    {
        GameObject page = CreatePage(parent, "AnalyticsPage");
        summary = CreateText(page.transform, "AnalyticsSummary", "Ride analytics", 44, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(summary.rectTransform, new Vector2(0, -135), TextAnchor.UpperCenter, new Vector2(860, 480));

        CreatePanel(page.transform, "SpeedBar", new Vector2(-330, -650), new Vector2(120, 230), AccentRed, TextAnchor.UpperCenter);
        CreatePanel(page.transform, "DistanceBar", new Vector2(-110, -590), new Vector2(120, 290), AccentCyan, TextAnchor.UpperCenter);
        CreatePanel(page.transform, "CaloriesBar", new Vector2(110, -680), new Vector2(120, 200), AccentAmber, TextAnchor.UpperCenter);
        CreatePanel(page.transform, "GearBar", new Vector2(330, -620), new Vector2(120, 260), MenuBlue, TextAnchor.UpperCenter);

        CreatePageButton(page.transform, "Back to Ride", new Vector2(0, 70), new Vector2(360, 74), dashboard.ClosePages);
        page.SetActive(false);
        return page;
    }

    private static GameObject CreateEndRidePage(Transform parent, RideAnalyticsDashboard dashboard, out TMP_Text summary)
    {
        GameObject page = CreatePage(parent, "EndRidePage");
        summary = CreateText(page.transform, "EndRideSummary", "Ride complete", 46, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureAnchored(summary.rectTransform, new Vector2(0, -150), TextAnchor.UpperCenter, new Vector2(920, 520));
        CreatePageButton(page.transform, "View Ride", new Vector2(-210, 70), new Vector2(320, 74), dashboard.ClosePages);
        CreatePageButton(page.transform, "End Screen", new Vector2(210, 70), new Vector2(320, 74), dashboard.ReturnToGarage);
        page.SetActive(false);
        return page;
    }

    private static GameObject CreatePage(Transform parent, string name)
    {
        GameObject page = new GameObject(name);
        page.transform.SetParent(parent, false);
        Image background = page.AddComponent<Image>();
        background.color = new Color(0.02f, 0.06f, 0.09f, 0.92f);
        Stretch(page.GetComponent<RectTransform>());
        return page;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color, TextAnchor anchor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        Image image = panel.AddComponent<Image>();
        image.color = color;
        ConfigureAnchored(panel.GetComponent<RectTransform>(), position, anchor, size);
        return panel;
    }

    private static void CreatePageButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreatePanel(parent, label.Replace(" ", "") + "Button", position, size, MenuBlue, TextAnchor.UpperCenter);
        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(onClick);
        ConfigureButtonColors(button, AccentCyan);

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, 32, FontStyles.Bold, TextAnchor.UpperCenter, White);
        Stretch(text.rectTransform);
    }

    private static void CreateMapMarker(Transform parent, Vector2 position, Color color)
    {
        GameObject marker = new GameObject("MapMarker");
        marker.transform.SetParent(parent, false);
        Image image = marker.AddComponent<Image>();
        image.sprite = GetCircleSprite();
        image.color = color;
        RectTransform rect = marker.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(34, 34);
    }

    private static void CreateLine(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, float angle, Color color)
    {
        GameObject line = new GameObject(name);
        line.transform.SetParent(parent, false);
        Image image = line.AddComponent<Image>();
        image.color = color;
        RectTransform rect = line.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 0f);
        rect.anchorMax = new Vector2(0f, 0f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private static void CreateScreenGradient(Transform parent)
    {
        CreateBand(parent, "TopReadabilityScrim", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, 0f), new Vector2(0f, 170f), Scrim);
        CreateBand(parent, "BottomCommandScrim", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(0f, 140f), new Color(0.005f, 0.012f, 0.02f, 0.72f));
        CreateBand(parent, "LeftMetricScrim", new Vector2(0f, 0.12f), new Vector2(0f, 0.94f), new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(500f, 0f), new Color(0.005f, 0.012f, 0.02f, 0.36f));
        CreateBand(parent, "RightMetricScrim", new Vector2(1f, 0.12f), new Vector2(1f, 0.94f), new Vector2(1f, 0.5f), new Vector2(0f, 0f), new Vector2(500f, 0f), new Color(0.005f, 0.012f, 0.02f, 0.36f));
    }

    private static void CreateStatusHeader(Transform parent)
    {
        GameObject header = CreatePanel(parent, "RideAnalyticsHeader", new Vector2(0f, -28f), new Vector2(520f, 72f), HudPanelStrong, TextAnchor.UpperCenter);
        AddOutline(header, Stroke, new Vector2(1.5f, -1.5f));

        TMP_Text title = CreateText(header.transform, "HeaderTitle", "RIDE ANALYTICS", 26, FontStyles.Bold, TextAnchor.UpperCenter, White);
        ConfigureChild(title.rectTransform, new Vector2(0, -11), TextAnchor.UpperCenter, new Vector2(450, 30));

        TMP_Text status = CreateText(header.transform, "HeaderStatus", "Movement + MQTT ready", 17, FontStyles.Bold, TextAnchor.UpperCenter, AccentGreen);
        ConfigureChild(status.rectTransform, new Vector2(0, -42), TextAnchor.UpperCenter, new Vector2(450, 24));
    }

    private static void CreateBand(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 position, Vector2 size, Color color)
    {
        GameObject band = new GameObject(name);
        band.transform.SetParent(parent, false);
        Image image = band.AddComponent<Image>();
        image.color = color;

        RectTransform rect = band.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void AddOutline(GameObject target, Color color, Vector2 distance)
    {
        UnityEngine.UI.Outline outline = target.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = color;
        outline.effectDistance = distance;
    }

    private static void ConfigureButtonColors(Button button, Color accent)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(accent.r, accent.g, accent.b, 0.88f);
        colors.pressedColor = new Color(accent.r, accent.g, accent.b, 1f);
        colors.selectedColor = new Color(accent.r, accent.g, accent.b, 0.78f);
        colors.disabledColor = new Color(0.28f, 0.34f, 0.4f, 0.55f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }

    private static Sprite GetCircleSprite()
    {
        if (circleSprite != null)
            return circleSprite;

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.name = "RideAnalyticsCircle";

        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.48f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance + 1f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        return circleSprite;
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
