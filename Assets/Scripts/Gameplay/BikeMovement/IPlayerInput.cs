
using System;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.XR;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Gameplay.BikeMovement
{
    public interface IPlayerInput
    {
        Vector2 GetDirection();
    }

    public class AxisInput : IPlayerInput
    {
        public Vector2 GetDirection()
        {
            return new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );
        }
    }

    public class MQTTInput : IPlayerInput, IDisposable
    {
        private readonly object telemetryLock = new object();
        private readonly Mqtt mqtt;

        private float mqttSpeed = 0f;
        private float mqttCadence = 0f;

        private long lastSpeedUpdate;
        private long lastCadenceUpdate;

        private string leftTurn = "LOW";
        private string rightTurn = "LOW";

        private bool disposed;

        private const double TelemetryTimeoutSeconds = 3.0;

        public MQTTInput()
        {
            mqtt = Mqtt.Instance;

            mqtt.Subscribe(
                OnMessageReceived,
                Mqtt.LeftTurnTopic,
                Mqtt.RightTurnTopic,
                Mqtt.SpeedTopic,
                Mqtt.CadenceTopic
            );
        }

        public float LiveSpeed
        {
            get
            {
                lock (telemetryLock)
                {
                    return IsFresh(lastSpeedUpdate)
                        ? mqttSpeed : 0f;
                }
            }
        }

        public float LiveCadence
        {
            get
            {
                lock (telemetryLock)
                {
                    return IsFresh(lastCadenceUpdate)
                        ? mqttCadence : 0f;
                }
            }
        }

        private static bool IsFresh(long lastUpdate)
        {
            if (lastUpdate == 0)
                return false;

            double elapsed =
                (Stopwatch.GetTimestamp() - lastUpdate)
                / (double)Stopwatch.Frequency;

            return elapsed <= TelemetryTimeoutSeconds;
        }

        private void OnMessageReceived(
            object sender,
            MqttMsgPublishEventArgs e)
        {
            string message = Encoding.UTF8.GetString(e.Message);

            lock (telemetryLock)
            {
                if (disposed)
                    return;

                if (e.Topic == Mqtt.SpeedTopic)
                {
                    if (TryReadValue(message, out float value))
                    {
                        mqttSpeed = value;
                        lastSpeedUpdate = Stopwatch.GetTimestamp();
                    }
                }
                else if (e.Topic == Mqtt.CadenceTopic)
                {
                    if (TryReadValue(message, out float value))
                    {
                        mqttCadence = value;
                        lastCadenceUpdate = Stopwatch.GetTimestamp();
                    }
                }
                else if (e.Topic == Mqtt.RightTurnTopic)
                {
                    leftTurn = message.Trim();
                }
                else if (e.Topic == Mqtt.LeftTurnTopic)
                {
                    rightTurn = message.Trim();
                }
            }
        }

        private static bool TryReadValue(
            string json,
            out float value)
        {
            value = 0f;

            if (string.IsNullOrWhiteSpace(json))
                return false;

            int keyIndex = json.IndexOf(
                "\"value\"",
                StringComparison.Ordinal
            );

            if (keyIndex < 0)
                return false;

            int colonIndex = json.IndexOf(':', keyIndex);

            if (colonIndex < 0)
                return false;

            int endIndex = json.IndexOfAny(
                new char[] { ',', '}' },
                colonIndex + 1
            );

            if (endIndex < 0)
                return false;

            string rawValue = json.Substring(
                colonIndex + 1,
                endIndex - colonIndex - 1
            ).Trim();

            if (!float.TryParse(
                rawValue,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value))
            {
                return false;
            }

            return !float.IsNaN(value)
                && !float.IsInfinity(value)
                && value >= 0f;
        }

        public Vector2 GetDirection()
        {
            lock (telemetryLock)
            {
                Vector2 direction = Vector2.zero;

                direction.y = IsFresh(lastSpeedUpdate)
                    ? mqttSpeed : 0f;

                if (leftTurn == "LEFT")
                    direction.x = -1f;

                if (rightTurn == "RIGHT")
                    direction.x = 1f;

                return direction;
            }
        }

        public void Dispose()
        {
            lock (telemetryLock)
            {
                if (disposed)
                    return;

                disposed = true;
            }

            if (mqtt != null)
                mqtt.Unsubscribe(OnMessageReceived);
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
            if (_controller.TryGetFeatureValue(
                CommonUsages.primary2DAxis,
                out var direction))
            {
                return direction;
            }

            return Vector2.zero;
        }
    }
}
