# Ride Analytics HUD

This module creates the Smart Ride Analytics dashboard shown in the Figma mockup. The main ride screen now follows the Figma overlay style closely: large white metric labels and values, no metric cards, colored bottom navigation text, a top-right menu button, a speed icon beside current speed, and a bottom-center gear display.

## Files

- `RideAnalyticsManager.cs` collects speed, cadence, heart rate, power, ride time, distance, calories, mission, checkpoint, personal-best, and gear values.
- `RideAnalyticsDashboard.cs` formats analytics values into TextMeshPro UI labels.
- `RideAnalyticsHudBuilder.cs` builds the screen-space dashboard UI at runtime.
- `RideAnalyticsApiClient.cs` can start/end backend ride sessions and poll stored ride/sensor values from the Node/Express API.
- `Assets/Editor/RideAnalytics/RideAnalyticsHudPrefabCreator.cs` adds Unity menu tools for creating the HUD prefab or adding it to the current scene.

## Local Test

1. Open the project in Unity `2022.3.22f1`.
2. Wait for Unity to compile the scripts.
3. Open a playable scene such as `Assets/Scenes/CityScene.unity`.
4. Use `SmartBike > Ride Analytics > Add HUD To Current Scene`.
5. Press Play.

The HUD starts at zero unless it receives movement, MQTT values, or backend database values. It no longer requests generated mock HUD data from the backend.

If the HUD already exists in the scene and looks wrong after script changes:

1. Select `RideAnalyticsHUD` in the Hierarchy.
2. Use `SmartBike > Ride Analytics > Rebuild Selected HUD`.
3. Press Play again.

For the closest match to the Figma image, use the Game view or Simulator at a 16:9 resolution such as `1920x1080`.

## Create a Reusable Prefab

Use `SmartBike > Ride Analytics > Create HUD Prefab`.

Unity will create:

`Assets/Prefabs/UI/RideAnalyticsHUD.prefab`

Drop that prefab into any scene where the analytics dashboard should appear.

## Data Integration

The manager already reads these MQTT topics when `Mqtt.Instance` is connected:

- `bike/{DeviceId}/speed`
- `bike/{DeviceId}/cadence`
- `bike/{DeviceId}/heartrate`
- `bike/{DeviceId}/power`

For REST API integration, call:

```csharp
RideAnalyticsManager.Instance.SetApiValues(speedKmh, cadenceRpm, heartRateBpm, powerWatts, gear);
```

`RideAnalyticsApiClient` now calls `SetBackendHudValues` automatically when API polling is enabled. It reads the existing backend `rides` and `sensor_data` schema through `/api/rides/{ride_id}`, then maps the latest sensor row plus ride summary values into speed, cadence, heart rate, power, distance, calories, ride time, average speed, max speed, gear, and progress.

## Backend API Test

1. Start the backend project locally on port `5000`.
2. In Unity, select the `RideAnalyticsHUD` object.
3. In `Ride Analytics Api Client`, keep `Api Base Url` as `http://localhost:5000`.
4. Add a valid database `ride_id` to `Current Ride Id` to display a specific stored ride. For the current demo data, use `8b3f7e1c-4f8e-4c20-9f49-1b8b5f8a7c01`.
5. Add a valid Supabase `profiles.id` value to `User Id` if you want to test backend ride saving.
6. Use the component context menu:
   - `Start Backend Ride`
   - `Fetch Ride Data Once`
   - `End Backend Ride`
7. To keep polling stored database demo data, enable `Poll Dashboard Hud`.

If `User Id` is empty, the client still starts a local ride, but backend ride saving requires a valid Supabase profile UUID because the `rides.user_id` column is required.

To test the route outside Unity, open `http://localhost:5000/api/rides/8b3f7e1c-4f8e-4c20-9f49-1b8b5f8a7c01` in the browser. Use backend database demo data for dashboard testing. The final dashboard path should use the existing `rides` and `sensor_data` tables through `/api/rides/{ride_id}`, not a separate mock table or generated HUD-only mock response.
