using UnityEngine;

public class CountdownTimer : MonoBehaviour
{
    public GameManager gameManager;

    private float elapsedTime = 0f;
    private bool timerRunning = false;

    void Start()
    {
        StartTimer();
    }

    void Update()
    {
        if (!timerRunning)
            return;

        elapsedTime += Time.deltaTime;
    }

    public void StartTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
    }

    public void StopTimer()
    {
        timerRunning = false;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    void OnGUI()
    {
        Rect timerRect = new Rect(Screen.width - 300, 15, 270, 55);

        Color oldColor = GUI.color;
        GUI.color = new Color(0f, 0f, 0f, 1f);
        GUI.Box(timerRect, "");
        GUI.color = oldColor;

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 26;
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.normal.textColor = Color.white;
        labelStyle.fontStyle = FontStyle.Bold;

        GUI.Label(timerRect, "Run Time: " + elapsedTime.ToString("F1") + "s", labelStyle);
    }
}