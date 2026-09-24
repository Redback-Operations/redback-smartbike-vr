# Smart Ride Dashboards

This module contains two dashboard implementations:

- `SmartRideInteractiveDashboard.cs` builds real Unity UI screens with buttons, cards, labels, and live ride values.
- `SmartRideDashboardGallery.cs` displays the original Figma PNG screens as a static reference gallery.
- `RideAnalyticsApiClient.cs` can be added to the dashboard root to connect the dashboard to the local backend APIs.

Use the interactive dashboard for gameplay/riding. Keep the PNG gallery only as a visual reference.

## Included Screens

- Smart Ride - Select Bike
- Smart Ride - Ride Setting
- Smart Ride - Ready to Ride
- Overview Dashboard
- Rides Analytics Dashboard
- Map Dashboard
- User Statistics Dashboard
- Trip Details Dashboard
- Overview Dashboard alternate copy

## Local Test

1. Open the project in Unity `2022.3.22f1`.
2. Open a scene such as `Assets/Scenes/CityScene.unity`.
3. Use `SmartBike > Dashboards > Add Interactive Dashboards To Current Scene`.
4. Press Play.
5. In the Game tab, use a `16:9` resolution such as `1920x1080`.

The interactive dashboard starts on the Select Bike screen. Click the visible buttons and right-side navigation, or use:

- Right Arrow: next dashboard
- Left Arrow: previous dashboard

During Play Mode, the overview, analytics, and trip detail values update from `RideAnalyticsManager`. Without movement, MQTT, or backend database values, the dashboard remains at zero instead of generating local mock ride data.

New interactive dashboards created from the Unity menu also include `RideAnalyticsApiClient`. API polling is off by default. Enable `Poll Dashboard Hud` when the backend is running and you want the dashboard to consume database-backed `/api/rides/{ride_id}` values from the existing `rides` and `sensor_data` tables.

## Prefab

Use `SmartBike > Dashboards > Create Interactive Dashboard Prefab` to create:

`Assets/Prefabs/UI/SmartRideInteractiveDashboard.prefab`

Drop that prefab into any scene where the dashboard gallery should be available.

## Notes

The interactive dashboard is built from Unity UI components rather than static PNGs. The current backend demo path uses stored database rows from `rides` and `sensor_data`; the next production step is to connect real MQTT sensor payloads to the active `ride_id`.
