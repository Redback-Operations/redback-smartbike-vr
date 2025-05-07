
using UnityEngine;
using UnityEngine.XR;
using uPLibrary.Networking.M2Mqtt.Messages;


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

    public class MQTTInput : IPlayerInput
    {
        private float mqttSpeed = 0f;
        private string leftTurn = "LOW";
        private string rightTurn = "LOW";

        public MQTTInput()
        {
            Mqtt.Instance.Subscribe(client_MqttMsgReceived, Mqtt.LeftTurnTopic);
            Mqtt.Instance.Subscribe(client_MqttMsgReceived, Mqtt.RightTurnTopic);
            Mqtt.Instance.Subscribe(client_MqttMsgReceived, Mqtt.SpeedTopic);
        }

        ~MQTTInput()
        {
            Mqtt.Instance.Unsubscribe(client_MqttMsgReceived);
        }

        void client_MqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
        {
            if (e.Topic == Mqtt.RightTurnTopic)
            {
                leftTurn = System.Text.Encoding.UTF8.GetString(e.Message);
            }
            else if (e.Topic == Mqtt.LeftTurnTopic)
            {
                rightTurn = System.Text.Encoding.UTF8.GetString(e.Message);
            }
            else if (e.Topic == Mqtt.SpeedTopic)
            {
                string json = System.Text.Encoding.UTF8.GetString(e.Message);
                string valueKey = "\"value\":";
                int startIndex = json.IndexOf(valueKey) + valueKey.Length;
                int endIndex = json.IndexOf(",", startIndex);
                string valueStr = json.Substring(startIndex, endIndex - startIndex);
                mqttSpeed = float.Parse(valueStr);
            }
        }
        public Vector2 GetDirection()
        {
            Vector2 direction = Vector2.zero;

            // If MQTT speed is provided, override forward direction:
            direction.y = mqttSpeed;

            // Apply left/right signals:
            if (leftTurn == "LEFT")
                direction.x = -1;
            if (rightTurn == "RIGHT")
                direction.x = 1;

            return direction;
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