using UnityEngine;

namespace Gameplay.Tutorial
{
    
    public class TutorialPlayerDriver : MonoBehaviour
    {
        [Tooltip("Objects NetworkPlayer would activate for the local player (XR rig, cameras, UI).")]
        public GameObject[] localObjects;

        [SerializeField] private BikeSelector bikeSelector;
        [SerializeField] private SaveLoadBike saveLoadBike;

        private PlayerController _playerController;

        private void Awake()
        {
            var networkPlayer = GetComponent<NetworkPlayer>();
            if (networkPlayer != null)
                networkPlayer.enabled = false;

            foreach (var localObject in localObjects)
            {
                if (localObject != null)
                    localObject.SetActive(true);
            }

            if (bikeSelector == null)
                bikeSelector = GetComponentInChildren<BikeSelector>(true);

            var selectedBike = PlayerPrefs.GetInt("SelectedBike", 0);
            bikeSelector.DisplayBike(selectedBike);

            var customization = PlayerPrefs.GetString($"Bike_{selectedBike}");
            if (saveLoadBike != null && !string.IsNullOrEmpty(customization))
                saveLoadBike.LoadBikeData(customization);

            _playerController = GetComponent<PlayerController>();
        }

        private void FixedUpdate()
        {
            if (_playerController != null)
                _playerController.Tick(Time.fixedDeltaTime);
        }
    }
}
