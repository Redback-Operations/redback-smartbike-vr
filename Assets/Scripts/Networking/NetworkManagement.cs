using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class NetworkManagement : SimulationBehaviour, INetworkRunnerCallbacks
{
    [Header("Testing")]
    [Tooltip("Runs Fusion in offline Single mode instead of connecting to Photon Cloud. Use this for local testing when Photon is unreachable (e.g. restrictive network/firewall). Do NOT leave this checked when you push/merge - it disables real multiplayer.")]
    public bool offlineTestMode = false;

    public string ActiveScene;
    // renamed from _runner: that name collides with a field Fusion's
    // SimulationBehaviour base class already serializes internally, which
    // Unity flags as "The same field name is serialized multiple times".
    private NetworkRunner _networkRunner;
    // prefabs for spawning
    public GameObject NetworkPlayer;

    public Transform SpawnTarget;
    private MissionSpawn[] _spawnPoints;
    
    // local version of the player
    [FormerlySerializedAs("Player")] public NetworkObject SpawnedPlayer;

    // network based items of players and NPC's
    private Dictionary<PlayerRef, NetworkObject> _players;
    private List<NetworkObject> _networkItems;

    private EventBinding<TeleportEvent> teleportEventBinding;
    public static event Action<NetworkManagement> OnManagerReady;

    private void OnEnable()
    {
        teleportEventBinding = new EventBinding<TeleportEvent>(OnTeleport);
        EventBus<TeleportEvent>.Register(teleportEventBinding);
    }
    
    private void OnDisable()
    {
        EventBus<TeleportEvent>.Deregister(teleportEventBinding);
    }

    
    private void OnTeleport(TeleportEvent teleportEvent)
    {
        // guard against a TeleportGate with an unset TargetSceneName - without
        // this, an empty target scene falls through to MapLoader's "no scene
        // set" fallback, which force-loads AvatarSelection over whatever was
        // running (see MapLoader.LoadAfter()).
        if (string.IsNullOrWhiteSpace(teleportEvent.targetScene))
        {
            Debug.LogWarning("[NetworkManagement] OnTeleport received an empty targetScene - ignoring. Check the TeleportGate that triggered this for a blank Target Scene Name.");
            return;
        }

        Disconnect();
        MapLoader.LoadScene(teleportEvent.targetScene);
    }

    void Start()
    {
        
        OnManagerReady?.Invoke(this);
        // set the active scene to ensure items are spawned in this scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(ActiveScene));

        // get the selected mission
        var mission = PlayerPrefs.GetInt("MissionNumber");

        _spawnPoints = FindObjectsOfType<MissionSpawn>();

        if (mission > 0 && _spawnPoints.Any(e => e.Mission == mission))
            SpawnTarget = _spawnPoints.First(e => e.Mission == mission).transform;

        _players = new Dictionary<PlayerRef, NetworkObject>();
        _networkItems = new List<NetworkObject>();

        _networkRunner = gameObject.AddComponent<NetworkRunner>();
        _networkRunner.ProvideInput = true;

        _networkRunner.AddCallbacks(this);
        _networkRunner.StartGame(new StartGameArgs
        {
            GameMode = offlineTestMode ? GameMode.Single : GameMode.Shared,
            SessionName = ActiveScene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log(offlineTestMode ? "Starting offline test session (no Photon connection)..." : "Connecting...");
    }

    public void Disconnect()
    {
        _networkRunner.Shutdown();
        _networkRunner.RemoveCallbacks(this);
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            // set the spawn location for the player
            SpawnedPlayer = _networkRunner.Spawn(NetworkPlayer, SpawnTarget.position, SpawnTarget.rotation, player);
            SpawnedPlayer.name = $"Player_{player.PlayerId}";
            // hide the loading scene
            MapLoader.UnloadScene("LoadingScene");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!_players.TryGetValue(player, out NetworkObject target))
            return;

        runner.Despawn(target);
        _players.Remove(player);
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }
    
    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadDone");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadStart");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.Log($"[NetworkManagement] OnDisconnectedFromServer fired, reason: {reason} - returning to GarageScene");
        // load back to the garage when you disconnect
        MapLoader.LoadScene("GarageScene");
    }

    #region Unused Callbacks
    
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }


    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    #endregion
}