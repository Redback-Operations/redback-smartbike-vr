# Race Mission Upgrade — Integration Notes

Scope: real players race each other **and** NPCs using the existing Fusion
`NetworkPlayer` / `GameMode.Shared` setup, plus laps, rubber-band NPC
difficulty, and best-time persistence. Core correctness fixes (per-racer
checkpoint tracking, synced countdown/timer, ranked results) are included
regardless since the old version was broken for anything beyond exactly 1
player + 1 NPC.

## What was wrong with the old version

`Assets/Scripts/Gameplay/Missions/Race againts NPC/`:

- `CheckpointManager.cs` used a single shared `currentCheckpointIndex` for
  *both* racers combined, plus two fixed-size `bool[]` arrays sized for
  exactly one player and one NPC. A second real player would corrupt the
  race state immediately.
- Nothing in the mission was a `NetworkBehaviour`. `CheckpointManager` was a
  plain local singleton and `RaceBikeMove` moved the NPC using `Update()` +
  `Time.deltaTime` with no `NetworkTransform`/network sync at all — every
  client would've simulated a different, diverging race with no agreed
  winner.
- Checkpoint-hit logic was duplicated almost verbatim in both
  `PlayerBikeScript.cs` and `RaceBikeMove.cs`.
- No laps, no countdown, no live standings, no persisted best times, and
  the "result" was a single shared text field (`You Win!`/`You Lose!`).

## File map — what I changed in this repo

| Old file | New file | Status |
|---|---|---|
| `CheckpointManager.cs` | `RaceManager.cs` | **Added new. Old file still on disk — please delete it yourself** (I intentionally don't delete files). Authoritative `NetworkBehaviour`, tracks race state/countdown/results for any number of racers. |
| `PlayerBikeScript.cs` | `RacerIdentity.cs` | **Added new. Old file still on disk — please delete it yourself.** Now used on *both* player and NPC bikes. |
| `RaceBikeMove.cs` | `RaceBikeMove.cs` | **Replaced in place.** Now a `NetworkBehaviour`, moves in `FixedUpdateNetwork`, rubber-bands off the leading human racer. Checkpoint handling removed (lives in `RacerIdentity` now). |
| `NPCBikeManager.cs` | `NPCBikeManager.cs` | **Replaced in place.** Same job, `RaceBikeMove[]` instead of `GameObject[]`. |
| — | `RaceCheckpoint.cs` | **New.** Auto-attached to your checkpoint objects by `RaceManager` — nothing to configure. |
| — | `RaceStartZone.cs` | **New.** Put on a trigger at the start line. |
| — | `RaceTypes.cs` | **New.** `RaceState` enum + networked `RaceResultEntry` struct. |
| — | `RaceHUD.cs` | **New.** Countdown / timer / live standings / results panel / best-time display. |
| `Managers/SaveData.cs`, `Managers/SaveManager.cs` | same | **Replaced in place.** Added `RaceRecord` list + `SaveRaceBestTime`/`LoadRaceBestTime`. |
| `EventBus/Events.cs` | same | **Replaced in place.** Added `RaceCheckpointPassedEvent`, `RaceFinishedEvent`. |

**Important:** `CheckpointManager.cs` and `PlayerBikeScript.cs` are still in
your project, unmodified. I can't delete files from your machine — please
remove them (and their `.meta` files) from inside Unity's Project window
(right-click → Delete) rather than in Finder, so Unity cleans up the
`.meta` references properly. Nothing else in the codebase references them
(confirmed via grep before making these changes), so it's safe to delete.

## Scene / prefab setup checklist

1. **Delete** `CheckpointManager.cs` and `PlayerBikeScript.cs` (see note
   above), and remove any components referencing them from the
   scene/prefabs — Unity will flag them as missing scripts until you do.

2. **Player bike prefab** (the `NetworkPlayer` prefab spawned by
   `NetworkManagement.cs`): add a `RacerIdentity` component (`isNpc` =
   false). It needs to sit on the same `NetworkObject` as `NetworkPlayer`.

3. **NPC bike prefab(s):**
   - Add a `NetworkObject` component (Fusion) if it doesn't have one.
   - Add a `NetworkTransform` component so position/rotation replicate to
     every client — this was completely missing before and is the reason
     NPC movement wasn't multiplayer-safe.
   - Add `RacerIdentity` (`isNpc` = true, set a `npcDisplayName`).
   - Keep `RaceBikeMove` on it, wire the `waypoints` array as before, tune
     `catchUpStrength` / `minSpeedMultiplier` / `maxSpeedMultiplier` to taste.
   - **NPC bikes must be placed in the scene, not spawned at runtime**, so
     Fusion assigns them to the Shared-Mode Master Client automatically.

4. **Race scene:**
   - Add an empty GameObject with a `NetworkObject` + `RaceManager`
     component (this must also be scene-placed, not runtime-spawned).
     Wire `checkpoints` in start→finish order (checkpoint 0 doubles as the
     lap line), set `TotalLaps`, `countdownSeconds`, and `npcBikeManager`.
   - Put `RaceStartZone` on a trigger collider at the start line.
   - Checkpoint colliders keep their existing `"Checkpoint"` tag — no other
     change needed, `RaceManager` attaches `RaceCheckpoint` to each one
     automatically at `Awake()`.

5. **UI:** add a `RaceHUD` component somewhere in the mission's Canvas and
   wire up `countdownText`, `standingsText`, `resultsPanel` +
   `resultsText`, `bestTimeText`, and drag in your `SaveManager` instance.
   `raceTimeText` is optional — leave it empty to fall back to the existing
   `UIManager.UpdateTime("Race Time", …)` HUD element instead of adding a
   new one.

## How multiplayer + NPCs coexist

- Fusion's `GameMode.Shared` (already used by `NetworkManagement.cs`) gives
  each player both input authority *and* state authority over their own
  spawned bike. Scene-placed objects (NPC bikes, `RaceManager`) default to
  the **Shared Mode Master Client**.
- `RacerIdentity.OnTriggerEnter` fires on every client (physics runs
  locally everywhere) but only proceeds when `Object.HasStateAuthority` is
  true — so exactly one client processes each checkpoint hit: the racer's
  own client for players, the master client for NPCs.
- Finish order is decided centrally: a racer's own client detects it just
  completed the final lap and sends `RequestFinishRpc` to `RaceManager`
  (which lives with the state authority / master client). `RaceManager`
  assigns the next placement and writes it into a `Networked
  NetworkArray<RaceResultEntry>`, which Fusion replicates to every client
  automatically — no second RPC needed for that part.

## Rubber-banding

`RaceBikeMove.GetRubberBandMultiplier()` compares the NPC's
`lap * checkpointCount + checkpoint` progress score against the best human
racer's score and scales `baseSpeed` between `minSpeedMultiplier` and
`maxSpeedMultiplier`. `catchUpStrength` controls how aggressively it
reacts. With no human racers registered yet it just runs at `baseSpeed`.

## Known simplifications / good next steps

- `RaceManager.ActiveRacerCount()` uses `FindObjectsOfType<RacerIdentity>()`
  each finish — fine for a fixed roster of racers present before the
  countdown starts. If you want players to join mid-race, swap this for an
  explicit Networked registration counter instead.
- Checkpoints must be hit strictly in order (skipping is ignored, not
  penalized) — there's no shortcut-detection or "return to track" logic.
- No reconnect/host-migration handling beyond what Fusion gives you by
  default for Shared mode.
- Best-time persistence is keyed by scene name by default (`RaceHUD.trackKey`)
  — set it explicitly if you'll have multiple race layouts in one scene.

## Testing checklist

1. Single-player smoke test first: enter the start zone, confirm the
   countdown, timer, lap counter, and results panel all work with just the
   NPCs.
2. Two-headset (or two Editor + headset) LAN test: confirm both players see
   the same countdown, the same live standings, and the same final results
   order.
3. Confirm best time only updates when you actually beat it, and persists
   across a play session restart (check the `_PlayerData.json` file in
   `Application.persistentDataPath`).
4. Sanity-check NPC rubber-banding by deliberately falling behind/ahead and
   watching NPC speed react.

None of this has been compiled inside Unity/Fusion yet — please build it in
the Editor and fix up any small API-surface mismatches against your exact
Fusion package version before relying on it.
