using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace UniTac
{
    public class LogSessionData : MonoBehaviour
    {
        /// <summary>
        /// File path of logfile.
        /// </summary>
        public string Path = "";
        private Sensor Sensor = null;
        private StreamWriter StreamWriter = null;

        /// <summary>
        /// Gets the sensor and creates a log file if it does not already exist.
        /// </summary>
        void Start()
        {
            Sensor = gameObject.GetComponent<Sensor>();
            if (!Sensor)
            {
                Debug.LogError("Sensor not found");
                Application.Quit();
            }
            Sensor.StatusChangedEvent.AddListener(LogStatus);

            if (Path == "") Path = "./Assets/" + Sensor.Serial + "-Log.json";
            if (!File.Exists(Path))
            {
                File.Create(Path);
            }
            if (File.Exists(Path))
            {
                FileStream fs = new(Path, FileMode.Append, FileAccess.Write);
                StreamWriter = new StreamWriter(fs);
            }
        }

        /// <summary>
        /// Writes the session data to the file.
        /// </summary>
        private void LogStatus()
        {
            var data = JsonConvert.SerializeObject(Sensor.LastSession);
            StreamWriter.WriteLine(data + ",");
        }
    }
}