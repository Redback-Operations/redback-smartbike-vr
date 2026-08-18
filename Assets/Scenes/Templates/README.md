# Mission Scene Template

`_MissionSceneTemplate.unity` is the starting point for every new mission /
level scene in SunCycle VR. It contains the base components needed to run the
VR bike and activate a mission, plus the standard GameObject hierarchy, and
nothing else.

## Why a scene and not a prefab

A prefab root can be reverted — one accidental **Prefab ▸ Revert All** and an
entire built level snaps back to the base template, taking hours of layout with
it. A scene has no such link: once copied it is independent forever. So the
template is a scene that you **copy**, never a prefab that you instantiate.

## Making a new mission scene

**Tools ▸ Missions ▸ New Mission Scene from Template…**

It copies the template into `Assets/Scenes/`, opens it, sets
`NetworkManagement.ActiveScene` to the new scene's name, offers to add it to
Build Settings, and runs the validator.

Doing it by hand instead: select `_MissionSceneTemplate` in the Project window,
`Ctrl/Cmd+D`, drag the copy into `Assets/Scenes/`, rename it, then fix
`NetworkManagement.ActiveScene` yourself. The menu item exists because that last
step is the one everybody forgets.

Never edit `_MissionSceneTemplate.unity` while building a level. Changes to the
template are their own PR.

## The hierarchy

Keep these names and this shape. Tooling, code reviews and everyone else's
muscle memory depend on it. Add your level content inside the existing groups
rather than creating new roots.

```
GameManager               GameManager, SaveManager
NetworkManager            NetworkManagement  → spawns the player over Fusion
MQTT                      Mqtt, SpeedListener → live data from the physical bike
EventSystem               EventSystem, StandaloneInputModule
XR Interaction Manager    XRInteractionManager, InputActionManager
Environment
  Global Volume           URP post-processing (Assets/Settings/MissionTemplate
                          Volume Profile.asset — shared by every template scene)
  Directional Light       the sun
  Terrain                 terrain objects
  Geometry                static level geometry (buildings, ground)
  Props                   set dressing, pickups, decoration
  Road                    road splines / road tiles
SpawnPoints
  MainSpawn               fallback spawn when no mission is selected
  MissionSpawns
    MissionSpawn_1        MissionSpawn (Mission = 1)
Objectives
  Missions                Mission_Activator
    Mission0_Template     placeholder Mission — replace with your own
NpcObjects
  Waypoints               NPC / race waypoint transforms
  NpcBikes                NPC bike instances
UI                        world-space canvases that belong to the scene
```

### Why the managers sit at the scene root

They look like they want a tidy `Managers` parent. They can't have one.
Fusion's `NetworkRunner` calls `DontDestroyOnLoad` on its own GameObject, and
`DontDestroyOnLoad` silently does nothing on a nested object — it only logs
`"DontDestroyOnLoad only works for root GameObjects"` and moves on. Anything
that has to survive a scene load has to be a root. CityScene and TerrainScene
already keep these at root; the template matches.

The player is **not** in the scene. `NetworkManagement` spawns `Player_New`
(XR Origin, bike models, bike controllers, player HUD) at runtime, at the
`MissionSpawn` matching `PlayerPrefs["MissionNumber"]`, falling back to
`SpawnTarget` (MainSpawn). This is why the template has no camera: pressing
Play on it directly shows nothing until the network player spawns. Enter
mission scenes through `MainMenu → LevelSelection` the way the game does.

## Shared assets the template points at

- `Assets/Prefabs/Player_New.prefab` — the networked player (XR Origin, bikes,
  bike controllers, HUD). Referenced by `NetworkManagement.NetworkPlayer`.
- `Assets/Prefabs/Mission/MissionSpawn.prefab` — the spawn marker.
- `Assets/Settings/MissionTemplate Volume Profile.asset` — the default
  post-processing look. Deliberately *not* CityScene's profile: editing it
  changes every template-derived scene and nothing else. If a scene needs its
  own look, duplicate the profile into that scene's folder and reassign it.

## MQTT is off by default

The `MQTT` object has `AutoConnect` unticked. With it on, a scene opened
without the physical bike (or a local broker) throws
`MqttConnectionException: Connection refused` on every play. Tick it when you
are actually testing against bike hardware.

## Adding a mission to your scene

1. Duplicate `Assets/Scripts/Gameplay/Missions/MissionTemplate.cs`, rename the
   class *and* the file to `MissionN.cs`, and set `MissionNumber` / `MissionName`.
2. Add a child under `Objectives/Missions`, attach your `MissionN` component,
   and put everything specific to that mission underneath it —
   `Mission_Activator` disables every non-active mission subtree.
3. Add the component to `Mission_Activator.Missions`.
4. Duplicate `MissionSpawns/MissionSpawn_1`, rename it `MissionSpawn_N`, set
   its `Mission` field to `N`, and position it where that mission starts.
5. Run **Tools ▸ Missions ▸ Validate Active Mission Scene**.

## Validation

**Tools ▸ Missions ▸ Validate Active Mission Scene** checks the things that
fail silently at runtime:

- `NetworkManagement.ActiveScene` matches the scene name
- `NetworkPlayer` and `SpawnTarget` are assigned
- a `Mission_Activator` exists
- no two `MissionSpawn`s claim the same mission number
- exactly one `XRInteractionManager`, and an `EventSystem` is present
- no missing (`None`) script components
- the scene is enabled in Build Settings

## Before you commit

- Scene name matches `NetworkManagement.ActiveScene`.
- Scene added to Build Settings, and to `MapLoader.Scene` if code loads it by
  enum.
- Validator is clean.
- Assets filed under the right `Assets/…` folders (see the root `README.md`).
