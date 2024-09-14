using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Fusion;
using Fusion.Sockets;

using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class NetworkManagement : MonoBehaviour, INetworkRunnerCallbacks
{
    public string ActiveScene;
    public static NetworkManagement Instance;

    private NetworkRunner _runner;

    // prefabs for spawning
    public GameObject NetworkPlayer;
 
    public Transform SpawnTarget;
    private MissionSpawn[] _spawnPoints;

    public List<GameObject> Prefabs;

    public GameObject[] avatarPrefabs;
    public Transform spawnPoint;

    public GameObject bike;
    private PlayerMovementController movementController;

    private GameObject avatar;

    // local version of the player
    public NetworkObject Player;

    // network based items of players and NPC's
    private Dictionary<PlayerRef, NetworkObject> _players;
    private List<NetworkObject> _networkItems;


    
    
    
    void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        // set the active scene to ensure items are spawned in this scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(ActiveScene));

        // get the selected mission
        var mission = PlayerPrefs.GetInt("MissionNumber");

        _spawnPoints = FindObjectsOfType<MissionSpawn>();

        if (mission > 0 && _spawnPoints.Any(e => e.Mission == mission))
            SpawnTarget = _spawnPoints.First(e => e.Mission == mission).transform;

        _players = new Dictionary<PlayerRef, NetworkObject>();
        _networkItems = new List<NetworkObject>();

        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        _runner.AddCallbacks(this);
        _runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = ActiveScene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        Debug.Log("Connecting...");
    }

    public void Disconnect()
    {
        _runner.Shutdown();
        _runner.RemoveCallbacks(this);

        Instance = null;
    }

    void OnApplicationQuit()
    {
        Disconnect();
    }

    void Update() {
        /* send that to the other clients
        _runner.LocalPlayer.SetCustomProperties(Player.Properties.Serialize());
        _frames++;

        if (_frames >= _updateOn)
        {
            // get any updated properties to send
            var hashtable = new Hashtable();

            // send their updated location
            foreach (var item in _networkItems)
            {
                var target = item.GetComponent<NetworkObject>();

                if (target.HasMoved())
                {
                    var updates = target.Properties().Serialize();
                    foreach (var update in updates)
                    {
                        hashtable.Add(update.Key, update.Value);
                    }
                }
            }

            if (hashtable.Count > 0)
                _runner.OpSetCustomPropertiesOfRoom(hashtable);

            _frames = 0;
        }

        _runner.Service();*/

  
        //Getting the avatar's Animator
        Animator avatarAnimator = avatar.GetComponent<Animator>();

        //Finding the PlayerMovementController on the bike
        movementController = bike.GetComponent<PlayerMovementController>();

        if (avatarAnimator != null && movementController != null) {
            movementController.AssignCharacterAnimator(avatarAnimator); //Passing the avatar's animator to PlayerMovementController
        } else {
            Debug.LogError("No animator found on avatar prefab or PlayerMovementController is missing.");
        }
       
    }

    void SpawnPlayer()
    {
        // set the spawn location for the player
        Player = _runner.Spawn(NetworkPlayer, SpawnTarget.position, SpawnTarget.rotation);

        Player.AssignInputAuthority(_runner.LocalPlayer);
        Player.RequestStateAuthority();

        _runner.SetPlayerObject(_runner.LocalPlayer, Player);



        // hide the loading scene
        MapLoader.UnloadScene("LoadingScene");

        // is this the server owner?
        if (!_runner.IsServer)
            return;

        // spawn the bikes with brains, others will show based on the current location of the bikes on the server

        // Spawn the avatar as a child of the bike (Player)
        int selectedAvatar = PlayerPrefs.GetInt("selectedAvatar", 0);
        GameObject prefab = avatarPrefabs[selectedAvatar];

        GameObject avatar = Instantiate(prefab, Player.transform.position, Player.transform.rotation);
        avatar.transform.SetParent(Player.transform);  // Set avatar as a child of the Player (bike)

        // Adjusting the avatar's position relative to the bike
        avatar.transform.localPosition = new Vector3(-0.02f, 0.55f, 0f); // Adjust as needed
        avatar.transform.localRotation = Quaternion.identity; // Resets rotation to align with bike

        Vector3 scale = new Vector3(0.45f, 0.45f, 0.45f); // Adjust scale as needed (0.5 is 50% of the original size)
        avatar.transform.localScale = scale;

        Animator avatarAnimator = avatar.GetComponent<Animator>();

        
        PlayerMovementController movementController = Player.GetComponent<PlayerMovementController>();

        
        if (avatarAnimator != null && movementController != null) {
            movementController.AssignCharacterAnimator(avatarAnimator);  // Assigning avatar's animator to the bike's controller
        } else {
            Debug.LogError("No animator found on avatar prefab or PlayerMovementController is missing.");
        }
        

    }

    private NetworkPlayer FindNetworkPlayer(int id)
    {
        Debug.Log("Searching");

        foreach (var player in _runner.ActivePlayers)
        {
            var instance = _runner.GetPlayerObject(player);

            if (instance == null)
                continue;

            var target = instance.GetComponent<NetworkPlayer>();

            if (target == null)
                continue;

            if (target.PlayerID == id)
                return target;
        }

        return null;
    }

    private void CreateNetworkObject(string name, Vector3 position, Quaternion rotation)
    {
        var target = Prefabs.FirstOrDefault(e => e.name == name);

        if (target == null)
            return;

        var spawn = _runner.Spawn(target, position, rotation);
        _networkItems.Add(spawn);
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        SpawnPlayer();

        if (!runner.IsServer)
            return;

        //var spawned = _runner.Spawn(NetworkPlayer, Vector3.zero, Quaternion.identity);
        //spawned.AssignInputAuthority(player);
        
        //runner.SetPlayerObject(player, spawned);
        //_players.Add(player, spawned);
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
        if (Player == null)
            return;

        var local = Player.GetComponent<LocalPlayer>();

        if (local == null)
            return;

        var rig = new RigInput
        {
            playAreaPosition = local.Properties.Position,
            playAreaRotation = local.Properties.Rotation,
            headsetPosition = local.Properties.HeadLocalPosition,
            headsetRotation = local.Properties.HeadLocalRotation,
            leftHandPosition = local.Properties.HandLeftLocalPosition,
            leftHandRotation = local.Properties.HandLeftLocalRotation,
            rightHandPosition = local.Properties.HandRightLocalPosition,
            rightHandRotation = local.Properties.HandRightLocalRotation,
        };

        input.Set(rig);
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadDone");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log("OnSceneLoadStart");
    }

    #region Unused Callbacks
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
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
