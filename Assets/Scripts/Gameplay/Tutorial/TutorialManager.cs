using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Tutorial
{
  
    public class TutorialManager : MonoBehaviour
    {
        private enum Phase
        {
            Welcome,
            Pedal,
            Steer,
            Gates,
            Complete
        }

        [SerializeField] private TutorialHUD hud;
        [SerializeField] private GameObject gatesRoot;
        [SerializeField] private string nextScene = "GarageScene";

        [Header("Step goals")]
        [SerializeField] private float lookAroundDegrees = 60f;
        [SerializeField] private float pedalDistance = 8f;
        [SerializeField] private float steerDegrees = 60f;
        [SerializeField] private float completeDelay = 8f;

        private Phase _phase = Phase.Welcome;
        private PlayerController _player;
        private Transform _playerTf;
        private Transform _headTf;

        private float _lookAccumulated;
        private float _lastHeadYaw;
        private float _distanceAccumulated;
        private Vector3 _lastPosition;
        private float _steerAccumulated;
        private float _lastPlayerYaw;
        private float _completeTimer;

        private readonly List<TutorialGate> _gates = new List<TutorialGate>();
        private int _gatesPassed;

        private void Start()
        {
            if (gatesRoot != null)
            {
                gatesRoot.GetComponentsInChildren(true, _gates);
                foreach (var gate in _gates)
                    gate.Init(this);
                gatesRoot.SetActive(false);
            }

            _player = FindObjectOfType<PlayerController>();
            if (_player != null)
            {
                _playerTf = _player.transform;
                _lastPosition = _playerTf.position;
                _lastPlayerYaw = _playerTf.eulerAngles.y;
            }

            EnterPhase(Phase.Welcome);
        }

        private void Update()
        {
            switch (_phase)
            {
                case Phase.Welcome:
                    UpdateWelcome();
                    break;
                case Phase.Pedal:
                    UpdatePedal();
                    break;
                case Phase.Steer:
                    UpdateSteer();
                    break;
                case Phase.Gates:
                    hud.SetProgress(
                        _gates.Count > 0 ? (float)_gatesPassed / _gates.Count : 0f,
                        $"{_gatesPassed}/{_gates.Count}",
                        "Gates");
                    break;
                case Phase.Complete:
                    _completeTimer += Time.deltaTime;
                    if (_completeTimer >= completeDelay)
                    {
                        enabled = false;
                        MapLoader.LoadScene(nextScene);
                    }
                    break;
            }
        }

        private void UpdateWelcome()
        {
            if (_headTf == null)
            {
                var cam = Camera.main;
                if (cam == null)
                    return;
                _headTf = cam.transform;
                _lastHeadYaw = _headTf.eulerAngles.y;
                return;
            }

            var yaw = _headTf.eulerAngles.y;
            _lookAccumulated += Mathf.Abs(Mathf.DeltaAngle(_lastHeadYaw, yaw));
            _lastHeadYaw = yaw;

            hud.SetProgress(
                _lookAccumulated / lookAroundDegrees,
                $"{Mathf.Min(_lookAccumulated / lookAroundDegrees, 1f):P0}",
                "Look around");

            if (_lookAccumulated >= lookAroundDegrees)
                EnterPhase(Phase.Pedal);
        }

        private void UpdatePedal()
        {
            if (_playerTf == null)
                return;

            var position = _playerTf.position;
            var delta = position - _lastPosition;
            delta.y = 0f;
            _distanceAccumulated += delta.magnitude;
            _lastPosition = position;

            hud.SetProgress(
                _distanceAccumulated / pedalDistance,
                $"{Mathf.Min(_distanceAccumulated, pedalDistance):0.0} m",
                $"Ride {pedalDistance:0} m");

            if (_distanceAccumulated >= pedalDistance)
                EnterPhase(Phase.Steer);
        }

        private void UpdateSteer()
        {
            if (_playerTf == null)
                return;

            var yaw = _playerTf.eulerAngles.y;
            _steerAccumulated += Mathf.Abs(Mathf.DeltaAngle(_lastPlayerYaw, yaw));
            _lastPlayerYaw = yaw;

            hud.SetProgress(
                _steerAccumulated / steerDegrees,
                $"{Mathf.Min(_steerAccumulated, steerDegrees):0}°",
                $"Turn {steerDegrees:0}°");

            if (_steerAccumulated >= steerDegrees)
                EnterPhase(Phase.Gates);
        }

        public void OnGatePassed(TutorialGate gate)
        {
            _gatesPassed++;

            if (_phase == Phase.Gates && _gatesPassed >= _gates.Count)
                EnterPhase(Phase.Complete);
        }

        private void EnterPhase(Phase phase)
        {
            _phase = phase;

            switch (phase)
            {
                case Phase.Welcome:
                    hud.ShowInstruction(
                        "Welcome to SmartBike VR!",
                        "First, get comfortable and look around.\n\n" +
                        "Headset: just turn your head.\n" +
                        "Desktop simulator: hold the RIGHT mouse button and move the mouse.");
                    break;

                case Phase.Pedal:
                    hud.ShowInstruction(
                        "Let's get moving",
                        "Pedal to ride forward.\n\n" +
                        "SmartBike: start pedalling.\n" +
                        "Keyboard: hold the UP ARROW key.");
                    break;

                case Phase.Steer:
                    hud.ShowInstruction(
                        "Steering",
                        "Now try turning left and right while riding.\n\n" +
                        "SmartBike: lean / use the handlebar controls.\n" +
                        "Keyboard: LEFT and RIGHT ARROW keys.");
                    break;

                case Phase.Gates:
                    if (gatesRoot != null)
                        gatesRoot.SetActive(true);

                    if (_playerTf != null)
                        _lastPosition = _playerTf.position;

                    hud.ShowInstruction(
                        "Follow the course",
                        "Ride through all the numbered gates ahead.\n" +
                        "They turn green when you pass them.");
                    break;

                case Phase.Complete:
                    hud.HideProgress();
                    hud.ShowInstruction(
                        "Tutorial complete!",
                        "Great riding! You know everything you need.\n" +
                        $"Heading to the garage in {completeDelay:0} seconds...",
                        persistent: true);
                    break;
            }
        }
    }
}
