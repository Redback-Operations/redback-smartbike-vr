using System;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using UnityEngine;

public class Mqtt : MonoBehaviour
{
    [Header("HiveMQ Cloud (MQTT TLS)")]
    public string Host = "YOUR_CLUSTER.s1.eu.hivemq.cloud";
    public int Port = 8883;
    public string Username = "YOUR_USER";
    public string Password = "YOUR_PASS";
    public bool AutoConnect = true;

    [Header("Device / Topic")]
    [Tooltip("Must match the sender (website / Pi), e.g. 000001")]
    public string DeviceId = "000001";

    // One JSON topic per device
    public string ControlTopic => $"bike/{DeviceId}/control";

    public static Mqtt Instance { get; private set; }

    private IMqttClient _client;
    private IMqttClientOptions _options;

    private string _subscribedTopic = null;
    private string _lastDeviceId = null;

    public bool IsConnected => _client != null && _client.IsConnected;

    // Debug / status
    public string LastStatus { get; private set; } = "Not connected";
    public string LastTopic { get; private set; } = "-";
    public string LastPayload { get; private set; } = "-";

    [Header("Live Control State (Debug)")]
    public float WebSpeed = 0f;     // RAW speed from bike (0..40+)
    public int WebTurn = 0;         // -1, 0, 1
    public bool WebBrake = false;   // true/false
    public long WebTs = 0;

    public event Action<ControlPacket> ControlReceived;

    [Serializable]
    public class ControlPacket
    {
        public string device;  // "000001"
        public float speed;    // raw
        public int turn;       // -1,0,1
        public bool brake;     // true/false
        public long ts;        // ms timestamp
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        _lastDeviceId = DeviceId;

        var factory = new MqttFactory();
        _client = factory.CreateMqttClient();

        _client.UseConnectedHandler(_ =>
        {
            LastStatus = "Connected ✅";
            Debug.Log("MQTTnet: Connected ✅");
        });

        _client.UseDisconnectedHandler(e =>
        {
            LastStatus = "Disconnected ❌";
            Debug.LogWarning($"MQTTnet: Disconnected ❌ Reason={e.Reason}");
        });

        _client.UseApplicationMessageReceivedHandler(e =>
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = e.ApplicationMessage.Payload == null
                ? ""
                : Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            LastTopic = topic;
            LastPayload = payload;

            // Only process our device control topic
            if (!string.Equals(topic, ControlTopic, StringComparison.Ordinal))
                return;

            try
            {
                var pkt = JsonUtility.FromJson<ControlPacket>(payload);
                if (pkt == null)
                {
                    Debug.LogWarning($"MQTTnet: JSON parse returned null. Payload={payload}");
                    return;
                }

                // Optional but recommended: device match
                if (!string.IsNullOrEmpty(pkt.device) &&
                    !string.Equals(pkt.device, DeviceId, StringComparison.Ordinal))
                {
                    Debug.LogWarning($"MQTTnet: Packet device={pkt.device} but DeviceId={DeviceId}. Ignoring.");
                    return;
                }

                WebSpeed = pkt.speed;                       // keep RAW here
                WebTurn = Mathf.Clamp(pkt.turn, -1, 1);
                WebBrake = pkt.brake;
                WebTs = pkt.ts;

                Debug.Log($"MQTTnet: PARSED ✅ speed={WebSpeed} turn={WebTurn} brake={WebBrake} ts={WebTs}");

                ControlReceived?.Invoke(pkt);
            }
            catch (Exception ex)
            {
                Debug.LogError($"MQTTnet: JSON parse error ❌ {ex.Message}\nPayload={payload}");
            }
        });

        _options = new MqttClientOptionsBuilder()
            .WithClientId("unity-" + Guid.NewGuid().ToString("N"))
            .WithTcpServer(Host, Port)
            .WithCredentials(Username, Password)
            .WithCleanSession()
            .WithTls(new MqttClientOptionsBuilderTlsParameters
            {
                UseTls = true,
                SslProtocol = SslProtocols.Tls12,

                // OK for testing/class projects. For production validate properly.
                AllowUntrustedCertificates = true,
                IgnoreCertificateChainErrors = true,
                IgnoreCertificateRevocationErrors = true,
                CertificateValidationHandler = _ => true
            })
            .Build();

        if (AutoConnect)
            await ConnectAndSubscribe(ControlTopic);
    }

    private async void Update()
    {
        // If you change DeviceId in inspector while running,
        // auto move subscription to the new control topic.
        if (!IsConnected) return;

        if (_lastDeviceId != DeviceId)
        {
            _lastDeviceId = DeviceId;
            await ResubscribeTo(ControlTopic);
        }
    }

    public async Task ConnectAndSubscribe(params string[] topics)
    {
        try
        {
            LastStatus = $"Connecting to {Host}:{Port}...";
            Debug.Log($"MQTTnet: Connecting to {Host}:{Port}...");

            await _client.ConnectAsync(_options);

            foreach (var t in topics)
                await _client.SubscribeAsync(t);

            _subscribedTopic = topics.Length > 0 ? topics[0] : null;

            LastStatus = "Subscribed ✅: " + string.Join(", ", topics);
            Debug.Log($"MQTTnet: Subscribed ✅ {string.Join(", ", topics)}");
        }
        catch (MQTTnet.Adapter.MqttConnectingFailedException ex)
        {
            Debug.LogError($"MQTTnet: Connect failed ❌ ResultCode={ex.ResultCode} Message={ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError("MQTTnet: " + ex);
        }
    }

    private async Task ResubscribeTo(string newTopic)
    {
        try
        {
            if (!IsConnected) return;

            if (!string.IsNullOrEmpty(_subscribedTopic))
            {
                await _client.UnsubscribeAsync(_subscribedTopic);
                Debug.Log($"MQTTnet: Unsubscribed ⛔ {_subscribedTopic}");
            }

            await _client.SubscribeAsync(newTopic);
            _subscribedTopic = newTopic;

            Debug.Log($"MQTTnet: Subscribed ✅ {newTopic}");
            LastStatus = "Subscribed ✅: " + newTopic;
        }
        catch (Exception ex)
        {
            Debug.LogError("MQTTnet: Resubscribe error ❌ " + ex);
        }
    }

    public async void Publish(string topic, string msg)
    {
        if (!IsConnected)
        {
            Debug.LogWarning($"MQTTnet: Publish blocked (not connected). Topic={topic}");
            return;
        }

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(msg)
            .WithAtMostOnceQoS()
            .Build();

        await _client.PublishAsync(message);
    }
}
