
using UnityEngine;
using UnityEngine.XR;


namespace Gameplay.BikeMovement
{
    public interface IPlayerInput
    {
        /// <summary>
        /// Returns the input direction (x for horizontal, y for forward/backward).
        /// </summary>
        Vector2 GetDirection();
    }

    public class AxisInput : IPlayerInput
    {
        public Vector2 GetDirection()
        {
            Vector2 direction = Vector2.zero;
            direction.y = Input.GetAxis("Vertical");
            direction.x = Input.GetAxis("Horizontal");
            return direction;
        }
    }

    public class WebJsonInput : IPlayerInput
    {
        public Vector2 GetDirection()
        {
            if (Mqtt.Instance == null || !Mqtt.Instance.IsConnected)
                return Vector2.zero;

            // x = turn, y = speed
            return new Vector2(
                Mqtt.Instance.WebTurn,
                Mqtt.Instance.WebSpeed
            );
        }
    }

    public class XRInput : IPlayerInput
    {
        private InputDevice _controller;
        public XRInput(InputDevice inputDevice)
        {
            _controller = inputDevice;
        }
        public Vector2 GetDirection()
        {
            if (_controller.TryGetFeatureValue(CommonUsages.primary2DAxis, out var dir))
                return dir;

            return Vector2.zero;
        }
    }
}