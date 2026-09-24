using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SmartRideInteractiveDashboard : MonoBehaviour
{
    private enum ScreenId
    {
        SelectBike,
        RideSetting,
        ReadyToRide,
        Overview,
        RideAnalytics,
        Map,
        UserStatistics,
        TripDetails,
        InterfaceGuide
    }

    [SerializeField] private bool buildOnAwake = true;
    [SerializeField] private RideAnalyticsManager analyticsManager;

    private readonly List<TMP_Text> liveLabels = new List<TMP_Text>();
    private Transform contentRoot;
    private ScreenId currentScreen;
    private string selectedBike = "E-Bike Pro";
    private string assistanceLevel = "Eco";
    private string rideMode = "Leisure";
    private string terrain = "Flat";
    private string timeOfDay = "Day";
    private string weather = "Clear";
    private int rideDurationMinutes = 45;

    private static readonly Color Background = new Color(0.02f, 0.07f, 0.1f, 1f);
    private static readonly Color Card = new Color(0.07f, 0.1f, 0.16f, 0.96f);
    private static readonly Color Active = new Color(0.12f, 0.22f, 0.42f, 1f);
    private static readonly Color ButtonBlue = new Color(0.06f, 0.15f, 0.35f, 1f);
    private static readonly Color TextPink = new Color(0.82f, 0.52f, 0.52f, 1f);
    private static readonly Color GoodGreen = new Color(0.05f, 0.7f, 0.18f, 1f);
    private static Sprite roundedSprite;

    private void Awake()
    {
        if (buildOnAwake && transform.childCount == 0)
            Build();
    }

    private void Update()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            ShowNextScreen();
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
            ShowPreviousScreen();

        UpdateLiveLabels();
    }

    [ContextMenu("Rebuild Interactive Dashboard")]
    public void Rebuild()
    {
        ClearChildren(transform);
        Build();
    }

    public void Build()
    {
        if (analyticsManager == null)
            analyticsManager = RideAnalyticsManager.Instance;
        if (analyticsManager == null)
            analyticsManager = gameObject.AddComponent<RideAnalyticsManager>();

        Canvas canvas = CreateCanvas(transform);
        CreatePanel(canvas.transform, "Background", Vector2.zero, new Vector2(1920, 1080), Background, TextAnchor.MiddleCenter);

        GameObject content = new GameObject("InteractiveDashboardContent");
        content.transform.SetParent(canvas.transform, false);
        contentRoot = content.AddComponent<RectTransform>();
        Stretch((RectTransform)contentRoot);

        ShowScreen(ScreenId.SelectBike);
    }

    private void ShowScreen(ScreenId screen)
    {
        currentScreen = screen;
        liveLabels.Clear();
        ClearChildren(contentRoot);

        switch (screen)
        {
            case ScreenId.SelectBike:
                BuildSelectBike();
                break;
            case ScreenId.RideSetting:
                BuildRideSetting();
                break;
            case ScreenId.ReadyToRide:
                BuildReadyToRide();
                break;
            case ScreenId.Overview:
                BuildOverview();
                break;
            case ScreenId.RideAnalytics:
                BuildRideAnalytics();
                break;
            case ScreenId.Map:
                BuildMap();
                break;
            case ScreenId.UserStatistics:
                BuildUserStatistics();
                break;
            case ScreenId.TripDetails:
                BuildTripDetails();
                break;
            case ScreenId.InterfaceGuide:
                BuildInterfaceGuide();
                break;
        }
    }

    private void ShowNextScreen()
    {
        int next = ((int)currentScreen + 1) % Enum.GetValues(typeof(ScreenId)).Length;
        ShowScreen((ScreenId)next);
    }

    private void ShowPreviousScreen()
    {
        int previous = (int)currentScreen - 1;
        if (previous < 0)
            previous = Enum.GetValues(typeof(ScreenId)).Length - 1;
        ShowScreen((ScreenId)previous);
    }

    private void BuildSelectBike()
    {
        CreateStartHeader(0);
        CreateSideNav(ScreenId.SelectBike);

        CreateBikeCard("E-Bike Pro", "Range - 100km\nTop speed - 60km/h", new Vector2(145, -208), selectedBike == "E-Bike Pro", () =>
        {
            selectedBike = "E-Bike Pro";
            ShowScreen(ScreenId.SelectBike);
        });
        CreateBikeCard("Mountain Bike", "Gears - 8\nTop speed - 25km/h", new Vector2(555, -208), selectedBike == "Mountain Bike", () =>
        {
            selectedBike = "Mountain Bike";
            ShowScreen(ScreenId.SelectBike);
        });
        CreateBikeCard("Road Bike", "Gears - 12\nTop speed - 45km/h", new Vector2(950, -208), selectedBike == "Road Bike", () =>
        {
            selectedBike = "Road Bike";
            ShowScreen(ScreenId.SelectBike);
        });

        CreateButton(contentRoot, "Next: Ride Setting", new Vector2(500, -800), new Vector2(420, 96), new Color(0.12f, 0.36f, 0.88f), Color.white, () => ShowScreen(ScreenId.RideSetting), 32);
    }

    private void BuildRideSetting()
    {
        CreateStartHeader(1);
        CreateSideNav(ScreenId.RideSetting);
        CreateCard(new Vector2(168, -198), new Vector2(1040, 520));

        float y = -250;
        CreateOptionRow("Assistance Level", new[] { "Eco", "Sport", "Normal", "Turbo" }, assistanceLevel, v => assistanceLevel = v, y);
        CreateOptionRow("Ride Mode", new[] { "Fitness", "Leisure", "Commute" }, rideMode, v => rideMode = v, y - 78);
        CreateOptionRow("Terrain Preference", new[] { "Flat", "Mixed", "Mountain" }, terrain, v => terrain = v, y - 156);

        CreateText(contentRoot, "RideDurationLabel", "Ride Duration", new Vector2(204, -496), new Vector2(340, 42), 32, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        CreateButton(contentRoot, "-", new Vector2(607, -486), new Vector2(70, 44), Color.white, Color.black, () =>
        {
            rideDurationMinutes = Mathf.Max(10, rideDurationMinutes - 5);
            ShowScreen(ScreenId.RideSetting);
        }, 30);
        CreateText(contentRoot, "RideDurationValue", rideDurationMinutes + " min", new Vector2(685, -496), new Vector2(180, 42), 32, FontStyles.Bold, TextAlignmentOptions.Center, Color.black, Color.white);
        CreateButton(contentRoot, "+", new Vector2(875, -486), new Vector2(70, 44), Color.white, Color.black, () =>
        {
            rideDurationMinutes = Mathf.Min(180, rideDurationMinutes + 5);
            ShowScreen(ScreenId.RideSetting);
        }, 30);

        CreateOptionRow("Time of the Day", new[] { "Day", "Night", "Evening" }, timeOfDay, v => timeOfDay = v, y - 312);
        CreateOptionRow("Weather", new[] { "Clear", "Rainy", "Windy" }, weather, v => weather = v, y - 390);

        CreateButton(contentRoot, "Back", new Vector2(118, -800), new Vector2(335, 96), Color.gray, Color.white, () => ShowScreen(ScreenId.SelectBike), 32);
        CreateButton(contentRoot, "Next: Ready", new Vector2(500, -800), new Vector2(420, 96), new Color(0.12f, 0.36f, 0.88f), Color.white, () => ShowScreen(ScreenId.ReadyToRide), 32);
    }

    private void BuildReadyToRide()
    {
        CreateStartHeader(2);
        CreateSideNav(ScreenId.ReadyToRide);
        CreateCard(new Vector2(168, -198), new Vector2(1040, 520));

        CreateText(contentRoot, "SummaryTitle", "Ride Summary", new Vector2(204, -246), new Vector2(330, 45), 32, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        CreateText(contentRoot, "Summary", "Bike: " + selectedBike + "\n\nAssistance: " + assistanceLevel + "\n\nRide Mode: " + rideMode + "\n\nTerrain: " + terrain + "\n\nDistance: " + EstimateTargetDistance() + "km", new Vector2(204, -326), new Vector2(330, 330), 31, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);

        CreateText(contentRoot, "RoutePreviewTitle", "Route preview", new Vector2(740, -214), new Vector2(360, 50), 34, FontStyles.Normal, TextAlignmentOptions.Center, Color.white);
        CreateText(contentRoot, "ChangeMap", "Change Map", new Vector2(990, -260), new Vector2(160, 40), 25, FontStyles.Bold, TextAlignmentOptions.Center, Color.blue);
        CreateRoutePreview(new Vector2(573, -284), new Vector2(565, 380));
        CreateText(contentRoot, "PreviewStats", "Duration\n" + rideDurationMinutes + " min       Calories\n420 Kcal       Weather\n" + weather + " (" + timeOfDay + ")", new Vector2(620, -548), new Vector2(500, 70), 25, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);

        CreateButton(contentRoot, "Back", new Vector2(118, -800), new Vector2(335, 96), Color.gray, Color.white, () => ShowScreen(ScreenId.RideSetting), 32);
        CreateButton(contentRoot, "Start Ride", new Vector2(500, -800), new Vector2(420, 96), new Color(0.12f, 0.36f, 0.88f), Color.white, () => ShowScreen(ScreenId.Overview), 32);
    }

    private void BuildOverview()
    {
        CreateDashboardHeader("Overview Dashboard", "This week");
        CreateMetricCards(new[]
        {
            "Total Rides|1,248|+ 12.5%",
            "Total Distance|3,482 km|+ 8.5%",
            "Total Time|120h 45m|+ 10.2%",
            "Calories Burned|24,560 kcal|+ 9.7%"
        });

        CreateText(contentRoot, "RideDetails", "Ride Details", new Vector2(216, -482), new Vector2(500, 46), 36, FontStyles.Bold, TextAlignmentOptions.Center, TextPink);
        CreateBarChart(new Vector2(104, -556), new Vector2(590, 365), new[] { 175f, 230f, 280f, 335f, 230f, 178f }, new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" });
        CreateText(contentRoot, "RideType", "Rides by Type", new Vector2(1067, -482), new Vector2(350, 46), 36, FontStyles.Bold, TextAlignmentOptions.Center, TextPink);
        CreatePieLikeChart(new Vector2(1030, -552), new Vector2(385, 385));
    }

    private void BuildRideAnalytics()
    {
        CreateDashboardHeader("Rides Analytics Dashboard", "ALL BIKE TYPE", "ALL LOCATION");
        CreateMetricCards(new[]
        {
            "Total Rides|1,248|",
            "AVG Distance|3.48 km|",
            "AVG Time|45m|",
            "Max Speed|24.56 km/h|"
        });

        CreateText(contentRoot, "DistanceTimeTitle", "Distance Over Time", new Vector2(216, -510), new Vector2(500, 46), 36, FontStyles.Bold, TextAlignmentOptions.Center, TextPink);
        CreateLineChart(new Vector2(104, -560), new Vector2(590, 365), new[] { 210f, 330f, 330f, 395f, 360f, 445f, 620f, 770f, 500f, 555f, 330f }, "km", "MAY");
        CreateText(contentRoot, "TopLocationsTitle", "Top Locations", new Vector2(1067, -510), new Vector2(350, 46), 36, FontStyles.Bold, TextAlignmentOptions.Center, TextPink);
        CreateHorizontalChart(new Vector2(1085, -580), new Vector2(520, 330), new[] { "Germany", "Canada", "Australia", "Italy", "America", "India", "Nepal", "China" }, new[] { 480f, 210f, 310f, 435f, 700f, 365f, 420f, 265f });
    }

    private void BuildMap()
    {
        CreateDashboardHeader("Map Dashboard", "This week", "ALL LOCATION");
        CreateSideNav(ScreenId.Map);
        CreateInfoCard("Popular Routes", "28", new Vector2(26, -166), new Vector2(438, 235));
        CreateInfoCard("Total Distance", "2,482 km", new Vector2(26, -440), new Vector2(438, 228));
        CreateInfoCard("Total Rides", "2,482", new Vector2(26, -706), new Vector2(438, 228));
        CreateMapGraphic(new Vector2(588, -240), new Vector2(725, 665));
    }

    private void BuildUserStatistics()
    {
        CreateDashboardHeader("User Statistics Dashboard", "This week");
        CreateMetricCards(new[]
        {
            "Total Users|1,248|+ 15.5%",
            "New Users|348|+ 6.5%",
            "Active users|563|+ 9.5%",
            "Returning users|280|+ 12.5%"
        });

        CreateText(contentRoot, "UsersOverTime", "Users Over Time", new Vector2(216, -510), new Vector2(500, 46), 36, FontStyles.Bold, TextAlignmentOptions.Center, TextPink);
        CreateLineChart(new Vector2(104, -560), new Vector2(590, 365), new[] { 210f, 315f, 330f, 395f, 360f, 445f, 620f, 770f, 500f, 555f, 330f }, "users", "MAY");
        CreateText(contentRoot, "Distribution", "User Distribution", new Vector2(925, -510), new Vector2(420, 46), 36, FontStyles.Bold, TextAlignmentOptions.Center, TextPink);
        CreateLegend(new Vector2(1225, -555));
    }

    private void BuildTripDetails()
    {
        CreateDashboardHeader("Trip Details Dashboard", "May 25", "7.00Am - 9.00Am");
        CreateSideNav(ScreenId.TripDetails);
        CreateCard(new Vector2(26, -166), new Vector2(485, 785));
        TMP_Text details = CreateText(contentRoot, "TripDetails", "", new Vector2(52, -184), new Vector2(420, 740), 36, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);
        liveLabels.Add(details);
        CreateText(contentRoot, "Route", "Route", new Vector2(780, -146), new Vector2(200, 45), 32, FontStyles.Bold, TextAlignmentOptions.Left, Color.blue);
        CreateRoutePreview(new Vector2(756, -183), new Vector2(435, 514));
        CreateText(contentRoot, "Elevation", "Elevation", new Vector2(780, -726), new Vector2(200, 45), 32, FontStyles.Bold, TextAlignmentOptions.Left, Color.blue);
        CreateLineChart(new Vector2(724, -772), new Vector2(515, 214), new[] { 3f, 3f, 9f, 8f, 12f, 12f, 16f, 16f, 18f, 19f, 21f, 13f, 10f, 4f }, "km/h", "km");
    }

    private void BuildInterfaceGuide()
    {
        CreateDashboardHeader("Interface Guide", "Interactive");
        CreateSideNav(ScreenId.InterfaceGuide);
        CreateCard(new Vector2(220, -210), new Vector2(1050, 560));
        CreateText(contentRoot, "GuideTitle", "Interactive Dashboard Controls", new Vector2(270, -250), new Vector2(920, 60), 40, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        CreateText(contentRoot, "GuideBody", "Use the right-side menu to switch dashboards.\n\nSelect Bike and Ride Setting buttons update the ride summary.\n\nRide statistics are real TextMeshPro labels and update from RideAnalyticsManager while Play Mode is running.\n\nKeyboard shortcuts: Left Arrow, Right Arrow.", new Vector2(270, -335), new Vector2(900, 330), 31, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);
    }

    private void CreateStartHeader(int step)
    {
        CreateText(contentRoot, "StartRideTitle", "START RIDE", new Vector2(520, -42), new Vector2(450, 70), 58, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        string[] labels = { "Select Bike", "Ride setting", "Ready to Ride" };
        for (int i = 0; i < labels.Length; i++)
        {
            Color color = i == step ? Color.white : TextPink;
            CreateText(contentRoot, "Step" + i, (i == step ? "● " : "") + labels[i], new Vector2(156 + i * 320, -130), new Vector2(300, 44), 32, FontStyles.Bold, TextAlignmentOptions.Left, color);
            if (i < 2)
                CreateText(contentRoot, "Arrow" + i, "→", new Vector2(355 + i * 320, -124), new Vector2(80, 46), 52, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.26f, 0.58f, 1f));
        }
        CreateMenuButton();
    }

    private void CreateDashboardHeader(string title, string filterOne, string filterTwo = null)
    {
        CreateText(contentRoot, "Title", title, new Vector2(74, -60), new Vector2(780, 70), 58, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        CreateButton(contentRoot, filterOne + " ▼", new Vector2(932, -44), new Vector2(345, 92), ButtonBlue, Color.white, delegate { }, 34);
        if (!string.IsNullOrEmpty(filterTwo))
            CreateButton(contentRoot, filterTwo + "▼", new Vector2(1314, -44), new Vector2(345, 92), ButtonBlue, Color.white, delegate { }, 34);
        CreateMenuButton();
    }

    private void CreateSideNav(ScreenId activeScreen)
    {
        CreateButton(contentRoot, "Start Ride", new Vector2(1478, -178), new Vector2(360, 54), Color.clear, activeScreen == ScreenId.ReadyToRide ? TextPink : Color.white, () => ShowScreen(ScreenId.ReadyToRide), 32);
        CreateButton(contentRoot, "Over View Dashboard", new Vector2(1478, -258), new Vector2(410, 54), Color.clear, activeScreen == ScreenId.Overview ? TextPink : Color.white, () => ShowScreen(ScreenId.Overview), 32);
        CreateButton(contentRoot, "Ride Analytics dashboard", new Vector2(1478, -338), new Vector2(430, 54), Color.clear, activeScreen == ScreenId.RideAnalytics ? TextPink : Color.white, () => ShowScreen(ScreenId.RideAnalytics), 32);
        CreateButton(contentRoot, "Map Dashboard", new Vector2(1478, -418), new Vector2(360, 54), Color.clear, activeScreen == ScreenId.Map ? TextPink : Color.white, () => ShowScreen(ScreenId.Map), 32);
        CreateButton(contentRoot, "User Statistics Dashboard", new Vector2(1478, -498), new Vector2(430, 54), Color.clear, activeScreen == ScreenId.UserStatistics ? TextPink : Color.white, () => ShowScreen(ScreenId.UserStatistics), 32);
        CreateButton(contentRoot, "Trip Details Dashboard", new Vector2(1478, -578), new Vector2(430, 54), Color.clear, activeScreen == ScreenId.TripDetails ? TextPink : Color.white, () => ShowScreen(ScreenId.TripDetails), 32);
        CreateButton(contentRoot, "Interface Guide", new Vector2(1478, -658), new Vector2(360, 54), Color.clear, activeScreen == ScreenId.InterfaceGuide ? TextPink : Color.white, () => ShowScreen(ScreenId.InterfaceGuide), 32);
    }

    private void CreateMetricCards(string[] metrics)
    {
        for (int i = 0; i < metrics.Length; i++)
        {
            string[] parts = metrics[i].Split('|');
            Vector2 position = new Vector2(26 + i * 476, -166);
            CreateCard(position, new Vector2(i == 0 ? 394 : 438, 228));
            CreateText(contentRoot, parts[0] + "Title", parts[0], position + new Vector2(26, -20), new Vector2(340, 45), 34, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);
            TMP_Text value = CreateText(contentRoot, parts[0] + "Value", parts[1], position + new Vector2(26, -70), new Vector2(340, 50), 36, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
            if (parts[0].Contains("Distance") || parts[0].Contains("Time") || parts[0].Contains("Calories") || parts[0].Contains("Speed"))
                liveLabels.Add(value);
            if (parts.Length > 2 && !string.IsNullOrEmpty(parts[2]))
                CreateText(contentRoot, parts[0] + "Trend", parts[2], position + new Vector2(26, -155), new Vector2(260, 50), 36, FontStyles.Normal, TextAlignmentOptions.Left, GoodGreen);
        }
    }

    private void CreateOptionRow(string label, string[] values, string selectedValue, Action<string> onSelect, float y)
    {
        CreateText(contentRoot, label, label, new Vector2(204, y), new Vector2(330, 42), 32, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        float x = 582;
        foreach (string value in values)
        {
            float width = Mathf.Max(105, value.Length * 18 + 34);
            string captured = value;
            CreateButton(contentRoot, value, new Vector2(x, y + 6), new Vector2(width, 42), value == selectedValue ? Active : Color.white, Color.black, () =>
            {
                onSelect(captured);
                ShowScreen(ScreenId.RideSetting);
            }, 29);
            x += width + 24;
        }
    }

    private void CreateBikeCard(string title, string body, Vector2 position, bool selected, UnityEngine.Events.UnityAction onClick)
    {
        CreateButton(contentRoot, "", position, new Vector2(340, 520), selected ? Active : Card, Color.white, onClick, 1);
        CreatePanel(contentRoot, title + "Image", position + new Vector2(60, -40), new Vector2(220, 220), selected ? Color.white : Color.black, TextAnchor.UpperLeft);
        CreateText(contentRoot, title + "Icon", title.Contains("Mountain") ? "MTB" : title.Contains("Road") ? "ROAD" : "E-BIKE", position + new Vector2(78, -116), new Vector2(180, 50), 34, FontStyles.Bold, TextAlignmentOptions.Center, selected ? Color.black : Color.white);
        CreateText(contentRoot, title + "Title", title, position + new Vector2(0, -300), new Vector2(340, 45), 28, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        CreateText(contentRoot, title + "Body", body, position + new Vector2(54, -380), new Vector2(250, 110), 25, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);
    }

    private void CreateInfoCard(string title, string value, Vector2 position, Vector2 size)
    {
        CreateCard(position, size);
        CreateText(contentRoot, title + "Title", title, position + new Vector2(26, -18), new Vector2(size.x - 52, 45), 34, FontStyles.Normal, TextAlignmentOptions.Left, Color.white);
        CreateText(contentRoot, title + "Value", value, position + new Vector2(26, -70), new Vector2(size.x - 52, 50), 36, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
    }

    private void CreateBarChart(Vector2 position, Vector2 size, float[] values, string[] labels)
    {
        CreatePanel(contentRoot, "BarChartBg", position, size, Color.white, TextAnchor.UpperLeft);
        float max = 400f;
        for (int i = 0; i < values.Length; i++)
        {
            float height = Mathf.Clamp01(values[i] / max) * (size.y - 70);
            Color color = Color.HSVToRGB(i / 7f, 0.85f, 0.85f);
            CreatePanel(contentRoot, "Bar" + i, position + new Vector2(52 + i * 82, -size.y + 35), new Vector2(58, height), color, TextAnchor.LowerLeft);
            CreateText(contentRoot, "BarLabel" + i, labels[i], position + new Vector2(45 + i * 82, -size.y - 8), new Vector2(78, 40), 30, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        }
    }

    private void CreateLineChart(Vector2 position, Vector2 size, float[] values, string yLabel, string xLabel)
    {
        CreatePanel(contentRoot, "LineChartBg", position, size, Color.white, TextAnchor.UpperLeft);
        float max = 1000f;
        if (yLabel == "km/h")
            max = 24f;

        Vector2 previous = Vector2.zero;
        for (int i = 0; i < values.Length; i++)
        {
            float x = position.x + 65 + i * ((size.x - 95) / Mathf.Max(1, values.Length - 1));
            float y = position.y - size.y + 45 + Mathf.Clamp01(values[i] / max) * (size.y - 75);
            Vector2 current = new Vector2(x, y);
            CreatePanel(contentRoot, "LinePoint" + i, new Vector2(x - 5, y + 5), new Vector2(10, 10), new Color(1f, 0.3f, 0.12f), TextAnchor.MiddleCenter);
            if (i > 0)
                CreateLine(contentRoot, "LineSeg" + i, previous, current, new Color(1f, 0.3f, 0.12f), 6f);
            previous = current;
        }

        CreateText(contentRoot, "YLabel" + yLabel, yLabel, position + new Vector2(-55, -150), new Vector2(100, 40), 30, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        CreateText(contentRoot, "XLabel" + xLabel, xLabel, position + new Vector2(size.x * 0.35f, -size.y - 55), new Vector2(180, 40), 30, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
    }

    private void CreateHorizontalChart(Vector2 position, Vector2 size, string[] labels, float[] values)
    {
        CreatePanel(contentRoot, "HorizontalChartBg", position, size, Color.white, TextAnchor.UpperLeft);
        float max = 750f;
        for (int i = 0; i < labels.Length; i++)
        {
            float y = position.y - 25 - i * 37;
            CreateText(contentRoot, "Location" + i, labels[i], new Vector2(position.x - 125, y + 8), new Vector2(150, 36), 28, FontStyles.Normal, TextAlignmentOptions.Left, new Color(0.1f, 0.38f, 1f));
            CreatePanel(contentRoot, "LocationBar" + i, new Vector2(position.x + 15, y), new Vector2(values[i] / max * (size.x - 65), 24), new Color(1f, 0.15f, 0.18f), TextAnchor.UpperLeft);
        }
        CreateText(contentRoot, "UsersAxis", "Users", position + new Vector2(205, -size.y - 15), new Vector2(150, 40), 30, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
    }

    private void CreatePieLikeChart(Vector2 position, Vector2 size)
    {
        CreatePanel(contentRoot, "PieChartBg", position, size, Color.white, TextAnchor.UpperLeft);
        CreatePanel(contentRoot, "PieMixed", position + new Vector2(80, -5), new Vector2(260, 150), new Color(0.42f, 0.78f, 0.5f), TextAnchor.UpperLeft);
        CreatePanel(contentRoot, "PieFitness", position + new Vector2(15, -145), new Vector2(170, 200), new Color(1f, 0.9f, 0.45f), TextAnchor.UpperLeft);
        CreatePanel(contentRoot, "PieFlat", position + new Vector2(180, -160), new Vector2(160, 210), new Color(0.35f, 0.78f, 0.85f), TextAnchor.UpperLeft);
        CreatePanel(contentRoot, "PieMountain", position + new Vector2(315, -130), new Vector2(70, 160), new Color(0.95f, 0.35f, 0.32f), TextAnchor.UpperLeft);
        CreateText(contentRoot, "PieLabels", "Mixed 40%\n\nFitness 30%\n\nFlat 20%     Mountain 10%", position + new Vector2(-55, -50), new Vector2(520, 320), 28, FontStyles.Normal, TextAlignmentOptions.Center, Color.black);
    }

    private void CreateMapGraphic(Vector2 position, Vector2 size)
    {
        CreatePanel(contentRoot, "MapBg", position, size, new Color(0.86f, 0.9f, 0.82f), TextAnchor.UpperLeft);
        for (int i = 0; i < 12; i++)
            CreateLine(contentRoot, "MapRoad" + i, position + new Vector2(UnityEngine.Random.Range(40, size.x - 40), -UnityEngine.Random.Range(40, size.y - 40)), position + new Vector2(UnityEngine.Random.Range(40, size.x - 40), -UnityEngine.Random.Range(40, size.y - 40)), new Color(0.8f, 0.8f, 0.78f), 5f);
        CreateLine(contentRoot, "MapRoute1", position + new Vector2(120, -560), position + new Vector2(260, -430), new Color(0.1f, 0.35f, 1f), 9f);
        CreateLine(contentRoot, "MapRoute2", position + new Vector2(260, -430), position + new Vector2(350, -265), new Color(0.1f, 0.35f, 1f), 9f);
        CreateLine(contentRoot, "MapRoute3", position + new Vector2(350, -265), position + new Vector2(560, -120), new Color(0.1f, 0.35f, 1f), 9f);
        CreatePanel(contentRoot, "MapStart", position + new Vector2(110, -548), new Vector2(22, 22), Color.red, TextAnchor.MiddleCenter);
        CreatePanel(contentRoot, "MapEnd", position + new Vector2(548, -108), new Vector2(22, 22), GoodGreen, TextAnchor.MiddleCenter);
    }

    private void CreateRoutePreview(Vector2 position, Vector2 size)
    {
        CreatePanel(contentRoot, "RouteBg", position, size, new Color(0.55f, 0.67f, 0.6f), TextAnchor.UpperLeft);
        CreatePanel(contentRoot, "RouteBlock1", position + new Vector2(35, -48), new Vector2(180, 130), new Color(0.56f, 0.46f, 0.32f), TextAnchor.UpperLeft);
        CreatePanel(contentRoot, "RouteBlock2", position + new Vector2(260, -86), new Vector2(210, 145), new Color(0.52f, 0.42f, 0.28f), TextAnchor.UpperLeft);
        CreateLine(contentRoot, "PreviewRoute1", position + new Vector2(70, -320), position + new Vector2(220, -240), Color.blue, 8f);
        CreateLine(contentRoot, "PreviewRoute2", position + new Vector2(220, -240), position + new Vector2(410, -300), Color.blue, 8f);
        CreateLine(contentRoot, "PreviewRoute3", position + new Vector2(410, -300), position + new Vector2(500, -130), Color.blue, 8f);
    }

    private void CreateLegend(Vector2 position)
    {
        string[] labels = { "Male", "Female", "Other" };
        Color[] colors = { Color.blue, new Color(0.9f, 0.2f, 0.05f), new Color(1f, 0.6f, 0.03f) };
        for (int i = 0; i < labels.Length; i++)
        {
            CreatePanel(contentRoot, "LegendDot" + i, position + new Vector2(0, -i * 34), new Vector2(22, 22), colors[i], TextAnchor.UpperLeft);
            CreateText(contentRoot, "LegendLabel" + i, labels[i], position + new Vector2(34, -i * 34 + 3), new Vector2(140, 28), 22, FontStyles.Bold, TextAlignmentOptions.Left, Color.white);
        }
    }

    private void UpdateLiveLabels()
    {
        if (analyticsManager == null || analyticsManager.Snapshot == null)
            return;

        RideAnalyticsSnapshot snapshot = analyticsManager.Snapshot;
        foreach (TMP_Text label in liveLabels)
        {
            if (label == null)
                continue;

            if (label.name.Contains("Total Distance"))
                label.text = snapshot.distanceKm.ToString("0.00") + " km";
            else if (label.name.Contains("Total Time") || label.name.Contains("AVG Time"))
                label.text = FormatMinutes(snapshot.rideTimeSeconds);
            else if (label.name.Contains("Calories"))
                label.text = snapshot.caloriesKcal.ToString("0") + " kcal";
            else if (label.name.Contains("Max Speed"))
                label.text = snapshot.maxSpeedKmh.ToString("0.0") + " km/h";
            else if (label.name.Contains("TripDetails"))
                label.text = "Trip id : #223\n\nDate : " + DateTime.Now.ToString("MMM dd, yyyy") + "\n\nStart time : 08.15AM\n\nEnd time : 08.45AM\n\nDistance : " + snapshot.distanceKm.ToString("0.00") + "km\n\nTime : " + FormatMinutes(snapshot.rideTimeSeconds) + "\n\nAVG Speed : " + snapshot.averageSpeedKmh.ToString("0.0") + " km/h\n\nCalories : " + snapshot.caloriesKcal.ToString("0") + " kcal";
        }
    }

    private int EstimateTargetDistance()
    {
        return Mathf.Max(5, Mathf.RoundToInt(rideDurationMinutes * 0.65f));
    }

    private static string FormatMinutes(float seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
        if (time.TotalHours >= 1f)
            return string.Format("{0:0}h {1:00}m", Math.Floor(time.TotalHours), time.Minutes);
        return string.Format("{0:0}m", Math.Max(1, time.Minutes));
    }

    private void CreateCard(Vector2 position, Vector2 size)
    {
        CreatePanel(contentRoot, "Card", position, size, Card, TextAnchor.UpperLeft);
    }

    private TMP_Text CreateText(Transform parent, string name, string value, Vector2 position, Vector2 size, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color, Color? background = null)
    {
        if (background.HasValue)
            CreatePanel(parent, name + "Background", position, size, background.Value, TextAnchor.UpperLeft);

        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        TMP_Text text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = alignment;
        text.enableWordWrapping = true;

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return text;
    }

    private Button CreateButton(Transform parent, string label, Vector2 position, Vector2 size, Color background, Color textColor, UnityEngine.Events.UnityAction onClick, float fontSize)
    {
        GameObject buttonObject = new GameObject(string.IsNullOrEmpty(label) ? "Button" : label.Replace(" ", "") + "Button");
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = background.a <= 0f ? new Color(1f, 1f, 1f, 0.001f) : background;
        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(onClick);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        if (!string.IsNullOrEmpty(label))
            CreateText(buttonObject.transform, "Label", label, Vector2.zero, size, fontSize, FontStyles.Bold, TextAlignmentOptions.Center, textColor);

        return button;
    }

    private static Image CreatePanel(Transform parent, string name, Vector2 position, Vector2 size, Color color, TextAnchor anchor)
    {
        GameObject panelObject = new GameObject(name);
        panelObject.transform.SetParent(parent, false);
        Image image = panelObject.AddComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = color;

        RectTransform rect = panelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = anchor == TextAnchor.LowerLeft ? new Vector2(0f, 0f) : new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return image;
    }

    private static void CreateLine(Transform parent, string name, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 delta = end - start;
        Image line = CreatePanel(parent, name, start, new Vector2(delta.magnitude, thickness), color, TextAnchor.MiddleCenter);
        RectTransform rect = line.rectTransform;
        rect.pivot = new Vector2(0f, 0.5f);
        rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
    }

    private void CreateMenuButton()
    {
        CreateButton(contentRoot, "≡", new Vector2(1776, -42), new Vector2(100, 100), new Color(0.22f, 0.55f, 0.82f), Color.white, ShowNextScreen, 56);
    }

    private static Canvas CreateCanvas(Transform parent)
    {
        GameObject canvasObject = new GameObject("SmartRideInteractiveCanvas");
        canvasObject.transform.SetParent(parent, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1300;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        Stretch(canvasObject.GetComponent<RectTransform>());
        return canvas;
    }

    private static Sprite GetRoundedSprite()
    {
        if (roundedSprite != null)
            return roundedSprite;

        const int size = 32;
        const int radius = 7;
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.name = "SmartRideRoundedRect";
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Max(radius - x, x - (size - radius - 1), 0);
                float dy = Mathf.Max(radius - y, y - (size - radius - 1), 0);
                float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + 1f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        roundedSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 1, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
        return roundedSprite;
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
        if (parent == null)
            return;

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
