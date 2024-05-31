using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MQTTSettings : EditorWindow
{
    private const string MQTTHostText = "MQTT Hostname (or IP):";
    private static string MQTTHost;

    [MenuItem("MQTT/Host Settings...")]
    static void Init()
    {
        // get MQTT hostname
        MQTTHost = PlayerPrefs.GetString("MQTTHost");
        EditorWindow.GetWindow<MQTTSettings>().Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(MQTTHostText);
        MQTTHost = EditorGUILayout.TextField("", MQTTHost);

        if (GUILayout.Button("Save"))
            SaveSettings(false);

        if (GUILayout.Button("Save & Close"))
            SaveSettings(true);
    }

    private void SaveSettings(bool close)
    {
        PlayerPrefs.SetString("MQTTHost", MQTTHost);
        PlayerPrefs.Save();

        if (close)
            Close();
    }
}
