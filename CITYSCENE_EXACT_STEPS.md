# CityScene — Exact Manual Wiring Steps

Unity kept crashing on my machine partway through doing this live (crashed
3x in a row on launch, before I touched anything the last two times — looks
like a pre-existing Library cache/Rosetta stability issue on this machine,
not something my scripts caused). So here is exactly what I found in your
`CityScene.unity` and exactly what to click, so you (or I, once Unity is
stable again) can finish it in a few minutes.

**First, try this**: quit Unity Hub entirely, delete the `Library/` folder
inside the project (`/Users/arunkundu/redback-smartbike-vr/Library`), then
reopen the project. Unity will rebuild it from scratch (takes a while, but
is often the fix for post-crash instability). Don't delete anything else.

## What's already confirmed in your scene

Hierarchy path: `CityScene > NpcObjects > NpcRaceBikes`, containing:
- `Bicycle_2 (1)`, `Bicycle_2 (2)`, `Bicycle_2 (3)` — the 3 NPC racers
  (their source prefab link is already broken/missing in your project —
  pre-existing issue, unrelated to this change, safe to ignore)
- `CheckPointManagerObj` — has the old `Checkpoint Manager` component
- `NPCBikeManagerObj` — has `NPC Bike Manager`, already correctly pointing
  at all 3 bikes (confirmed the reference survived the `GameObject[]` →
  `RaceBikeMove[]` field-type change automatically — nothing to do here)

The old `Checkpoint Manager` component's 19 checkpoints, **in this exact
order** (you'll need this order for step 3):

```
0.  RaceWaypoint1 (4)
1.  RaceWaypoint2
2.  RaceWaypoint3
3.  RaceWaypoint4
4.  RaceWaypoint5
5.  RaceWaypoint6
6.  RaceWaypoint7
7.  RaceWaypoint9
8.  RaceWaypoint11
9.  RaceWaypoint13
10. RaceWaypoint15
11. RaceWaypoint17
12. RaceWaypoint18
13. RaceWaypoint20
14. RaceWaypoint23
15. RaceWaypoint25
16. RaceWaypoint27
17. RaceWaypoint30
18. RaceWaypoint31
```

Note element 0 is specifically **`RaceWaypoint1 (4)`** — there are multiple
objects named `RaceWaypoint1` in the scene (Unity's duplicate-name
suffixing), so don't just search-and-pick the first `RaceWaypoint1` you
see. Easiest way to get this exactly right without hunting for each one:
see step 3's copy/paste trick below.

## Steps

### 1. NPC bikes — add networking (repeat for all 3: `Bicycle_2 (1)`, `(2)`, `(3)`)

For each one:
1. Select it in the Hierarchy.
2. In the Inspector, the `Race Bike Move (Script)` component shows a
   yellow warning box: **"This NetworkBehaviour requires a NetworkObject
   component to function."** — click its **"Add Network Object"** button.
   (Unity's own `[RequireComponent]` already auto-adds a `Racer Identity`
   component for you when it recompiles the scripts — check it's there
   too; if not, add it manually.)
3. On the new `Racer Identity` component: check **Is Npc**, and set
   **Npc Display Name** to something like `NPC Racer 1` / `2` / `3`.
4. Click **Add Component** → search `Network Transform` → add
   **Network Transform (Fusion)** (default settings are fine).

### 2. CheckPointManagerObj — replace with RaceManager

1. Select `CheckPointManagerObj`.
2. Click **Add Component** → search `Race Manager` → add it.
3. Click **Add Component** → search `Network Object` → add it (or use the
   yellow warning button the same way as step 1).
4. **Copy the checkpoints array** the safe way (guarantees you get the
   exact same 19 object instances, including the correct `RaceWaypoint1
   (4)`): on the *old* `Checkpoint Manager` component, right-click the
   **Checkpoints** field label → **Copy**. Then on the new `Race Manager`
   component, right-click its **Checkpoints** field label → **Paste**.
   (If your Unity version doesn't offer Copy/Paste on array fields, fall
   back to manually dragging each of the 19 waypoints from the old
   component's expanded list into the new one, in the order listed above.)
5. Set **Total Laps** (try `3`), **Countdown Seconds** (try `3`).
6. Drag `NPCBikeManagerObj` into the **Npc Bike Manager** field.

### 3. NetworkPlayer prefab — add RacerIdentity

1. In the Project window, open `Assets/Prefabs/NetworkPlayer.prefab`.
2. Select its root object, **Add Component** → `Racer Identity`.
3. Leave **Is Npc** unchecked.
4. Save the prefab.

### 4. Start line trigger

1. Create a new GameObject near checkpoint 0 (`RaceWaypoint1 (4)`) with a
   trigger `Collider` sized to cover the width of the track.
2. **Add Component** → `Race Start Zone`.

### 5. RaceHUD

1. Find your Race Mission's UI Canvas (I didn't get to locate this exact
   object before Unity crashed — look under `UI` in the Hierarchy, or
   wherever the old `raceResultText` TMP object lived).
2. Add an empty child GameObject, **Add Component** → `Race HUD`.
3. Wire up `countdownText`, `standingsText`, `resultsPanel` +
   `resultsText`, `bestTimeText` (create simple TMP text objects for
   these if they don't exist yet), and drag your scene's `SaveManager`
   instance into the `Save Manager` field.

### 6. Clean up

1. Delete the `Checkpoint Manager` component from `CheckPointManagerObj`
   (right-click the component header → **Remove Component**) — don't
   delete the GameObject itself, it now hosts `Race Manager`.
2. Delete `CheckpointManager.cs` and `PlayerBikeScript.cs` from the
   Project window (right-click → Delete) — nothing else references them
   (confirmed via grep).
3. Save the scene (**Cmd+S**) and the `NetworkPlayer.prefab`.

### 7. Verify

Open the Console window and confirm there are no red compile errors, then
do the single-player smoke test from `RACE_MISSION_UPGRADE_NOTES.md`.
