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
    public string ActiveScene;
    // Named _localRunner, not _runner: Fusion's SimulationBehaviour base class
    // already serializes a field called _runner, and Unity refuses to serialize
    // the same field name twice in a hierarchy.
    private NetworkRunner _localRunner;
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

        _localRunner = gameObject.AddComponent<NetworkRunner>();
        _localRunner.ProvideInput = true;

        _localRunner.AddCallbacks(this);
        _localRunner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = ActiveScene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log("Connecting...");
    }

    public void Disconnect()
    {
        if (_localRunner == null)
            return;

        _localRunner.Shutdown();
        _localRunner.RemoveCallbacks(this);
        _localRunner = null;
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
            SpawnedPlayer = _localRunner.Spawn(NetworkPlayer, SpawnTarget.position, SpawnTarget.rotation, player);
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