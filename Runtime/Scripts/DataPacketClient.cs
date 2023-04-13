using MQTTnet.Client;
using MQTTnet;
using UnityEngine;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections;

namespace UniTac
{
    /// <summary>
    /// MQTT-client that saves 5-minute-messages from the sensor to a log file.
    /// </summary>
    public class DataPacketClient : MonoBehaviour
    {
        /// <summary>
        /// File path of logfile.
        /// </summary>
        public string Path = "";
        /// <summary>
        /// The minimum log level a message from the client needs before it is printed in console.
        /// </summary>
        public LogLevel LogLevel = LogLevel.None;
        private IMqttClient Client;
        private Manager Manager;

        // Start is called before the first frame update
        void Start()
        {
            Manager = gameObject.GetComponent<Manager>();
            if (!Manager)
            {
                Debug.LogError("Manager not found");
                Application.Quit();
            }
            if (Path == "") Path = "./Assets/5-minute-message-log.txt";
            if (!File.Exists(Path))
            {
                File.Create(Path);
            }
            if (!File.Exists(Path))
            {
                Debug.LogError("Could not create file!");
            }
            Client = CreateClient();
            StartCoroutine(ConnectClient());
        }

        /// <summary>
        /// Creates an MQTT client that receives the sensor data from the MQTT server.
        /// </summary>
        /// <returns>The client object</returns>
        IMqttClient CreateClient()
        {
            var mqttFactory = new MqttFactory();

            if (LogLevel != LogLevel.None)
            {
                Logger logger = new(LogLevel);
                mqttFactory = new MqttFactory(logger);
            }

            var client = mqttFactory.CreateMqttClient();
            client.ApplicationMessageReceivedAsync += e => HandleMessage(e);
            return client;
        }

        /// <summary>
        /// Connects the client to the server.
        /// </summary>
        IEnumerator ConnectClient()
        {
            var mqttClientOptions = new MqttClientOptionsBuilder()
                .WithTcpServer("127.0.0.1")
                .Build();

            var mqttFactory = new MqttFactory();
            var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
                .WithTopicFilter(
                    f => { f.WithTopic("smx/device/+/5_min_data/#"); })
                .Build();

            yield return new WaitUntil(() => Manager.Server != null && Manager.Server.IsStarted);
            Client.ConnectAsync(mqttClientOptions, System.Threading.CancellationToken.None);
            yield return new WaitUntil(() => Client.IsConnected);
            Client.SubscribeAsync(mqttSubscribeOptions, System.Threading.CancellationToken.None);
        }

        /// <summary>
        /// A method to process message events from TAC-B sensors.
        /// </summary>
        /// <param name="e">The incoming message event</param>
        /// <returns>awaitable <see cref="Task"/></returns>
        private Task HandleMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            var json = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            using FileStream fs = new(Path, FileMode.Append, FileAccess.Write);
            using StreamWriter sw = new(fs);
            sw.WriteLine(json + ",");
            return Task.CompletedTask;
        }
    }
}