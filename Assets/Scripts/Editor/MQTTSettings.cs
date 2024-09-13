using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MQTTSettings : EditorWindow
{
    private const string MQTTHostText = "MQTT Hostname (or IP):";
    private static string MQTTHost;

    private const string MQTTUsernameText = "MQTT Username:";
    private static string MQTTUsername;
    private const string MQTTPasswordText = "MQTT Password:";
    private static string MQTTPassword;

    [MenuItem("MQTT/Host Settings...")]
    static void Init()
    {
        // get MQTT hostname
        MQTTHost = PlayerPrefs.GetString("MQTTHost");
        MQTTUsername = PlayerPrefs.GetString("MQTTUsername");
        MQTTPassword = PlayerPrefs.GetString("MQTTPassword");

        EditorWindow.GetWindow<MQTTSettings>().Show();
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField(MQTTHostText);
        MQTTHost = EditorGUILayout.TextField("", MQTTHost);

        EditorGUILayout.Separator();

        MQTTUsername = EditorGUILayout.TextField(MQTTUsernameText, MQTTUsername);
        MQTTPassword = EditorGUILayout.PasswordField(MQTTPasswordText, MQTTPassword);

        if (GUILayout.Button("Save"))
            SaveSettings(false);

        if (GUILayout.Button("Save & Close"))
            SaveSettings(true);
    }

    private void SaveSettings(bool close)
    {
        PlayerPrefs.SetString("MQTTHost", MQTTHost);
        PlayerPrefs.SetString("MQTTUsername", MQTTUsername);
        PlayerPrefs.SetString("MQTTPassword", MQTTPassword);
        PlayerPrefs.Save();

        if (close)
            Close();
    }
}
