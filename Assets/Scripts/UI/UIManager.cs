using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

// Holds the player's HUD state (score, time, objectives) and raises an event whenever it changes.
// UI elements listen for these events and update themselves, instead of UIManager writing to them.
// See ScoreText and TimeDisplay in Scripts/UI/Player HUD for examples of listeners.
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public static event Action<UIManager> OnPlayerUIManagerReady;

    // HUD events
    public event Action<int> ScoreChanged;
    public event Action<string, float> TimeChanged;
    public event Action<Objective> ObjectiveAdded;
    public event Action<Objective> ObjectiveStateChanged;
    public event Action<Objective> ObjectiveRemoved;
    public event Action ObjectivesCleared;

    public Canvas Objectives;
    public Canvas ObjectivesVR;

    public TMP_Text Notification;
    public TMP_Text NotificationVR;

    public Canvas Addons; // mission specific UI objects

    // current state, so listeners can show the right value when they first subscribe
    public int Score { get; private set; }
    public string TimeLabel { get; private set; } = "";
    public float TimeValue { get; private set; }
    public bool HasTime { get; private set; }

    private int _objectiveId;
    private List<Objective> _objectives = new List<Objective>();

    void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetNotification("", 0);
        OnPlayerUIManagerReady?.Invoke(this);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // finds the UIManager belonging to the same player as a UI element, falling back to Instance
    public static UIManager FindFor(Component component)
    {
        for (var parent = component.transform; parent != null; parent = parent.parent)
        {
            var ui = parent.GetComponentInChildren<UIManager>(true);
            if (ui != null)
                return ui;
        }

        return Instance;
    }

    // ---------- score and time ----------

    public void SetScore(int value)
    {
        Score = value;
        ScoreChanged?.Invoke(Score);
    }

    public void AddScore(int value)
    {
        SetScore(Score + value);
    }

    public void UpdateTime(string label, float value)
    {
        TimeLabel = label;
        TimeValue = value;
        HasTime = true;
        TimeChanged?.Invoke(label, value);
    }

    // ---------- notifications ----------

    public IEnumerator ShowNotification(string text, float length)
    {
        if (Notification == null && NotificationVR == null)
            yield break;

        SetNotification(text, 0);

        // fade in, hold, fade out
        for (var t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            SetNotification(text, t * 2);
            yield return null;
        }

        SetNotification(text, 1);
        yield return new WaitForSeconds(length);

        for (var t = 0f; t < 0.5f; t += Time.deltaTime)
        {
            SetNotification(text, 1 - t * 2);
            yield return null;
        }

        SetNotification("", 0);
    }

    private void SetNotification(string text, float alpha)
    {
        foreach (var label in new[] { Notification, NotificationVR })
        {
            if (label == null)
                continue;

            label.text = text;
            label.color = WithAlpha(label.color, alpha);
        }
    }

    // ---------- objectives ----------

    public int AddObjective(string text)
    {
        var id = ++_objectiveId;
        var offsetY = _objectives.Count * -50;

        var objective = new Objective
        {
            ID = id,
            Description = text,
            Mesh = CreateObjectiveText(Objectives, $"Objective_{id}_TMP", text, offsetY),
            MeshVR = CreateObjectiveText(ObjectivesVR, $"Objective_{id}_TMP_VR", text, offsetY)
        };

        _objectives.Add(objective);
        Debug.Log($"Objective {id} added: {text}");

        ObjectiveAdded?.Invoke(objective);
        return id;
    }

    public void SetObjectiveState(int id, ObjectiveState state)
    {
        // id -1 updates every objective
        foreach (var objective in _objectives.Where(o => id == -1 || o.ID == id).ToList())
        {
            if (objective.State == state)
                continue;

            objective.State = state;
            ApplyObjectiveStyle(objective);
            ObjectiveStateChanged?.Invoke(objective);
        }
    }

    public void RemoveObjective(int id)
    {
        var objective = _objectives.FirstOrDefault(o => o.ID == id);

        if (objective == null)
            return;

        DestroyObjective(objective);
        _objectives.Remove(objective);
        ObjectiveRemoved?.Invoke(objective);
    }

    public IEnumerator ClearObjectives()
    {
        if (_objectives.Count == 0)
            yield break;

        var objectives = _objectives;

        _objectiveId = 0;
        _objectives = new List<Objective>();
        ObjectivesCleared?.Invoke();

        // fade out over one second, then remove
        for (var t = 0f; t < 1f; t += Time.deltaTime)
        {
            foreach (var objective in objectives)
            {
                foreach (var tmp in TextsOf(objective))
                    tmp.color = WithAlpha(tmp.color, 1 - t);
            }

            yield return null;
        }

        foreach (var objective in objectives)
            DestroyObjective(objective);
    }

    private GameObject CreateObjectiveText(Canvas canvas, string name, string text, float offsetY)
    {
        if (canvas == null)
            return null;

        var mesh = new GameObject(name);
        mesh.transform.SetParent(canvas.transform, false);

        var tmp = mesh.AddComponent<TextMeshProUGUI>();
        tmp.text = text;

        var rect = mesh.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0, 1);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(0, 1);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
        rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);
        rect.SetLocalPositionAndRotation(new Vector3(0, offsetY), Quaternion.identity);

        return mesh;
    }

    private void ApplyObjectiveStyle(Objective objective)
    {
        foreach (var tmp in TextsOf(objective))
        {
            switch (objective.State)
            {
                case ObjectiveState.Success:
                case ObjectiveState.Complete:
                    tmp.color = Color.green;
                    break;

                case ObjectiveState.Failed:
                    tmp.color = Color.red;
                    tmp.fontStyle = FontStyles.Strikethrough;
                    break;
            }
        }
    }

    private IEnumerable<TextMeshProUGUI> TextsOf(Objective objective)
    {
        foreach (var mesh in new[] { objective.Mesh, objective.MeshVR })
        {
            var tmp = mesh != null ? mesh.GetComponent<TextMeshProUGUI>() : null;
            if (tmp != null)
                yield return tmp;
        }
    }

    private void DestroyObjective(Objective objective)
    {
        if (objective.Mesh != null)
            Destroy(objective.Mesh);

        if (objective.MeshVR != null)
            Destroy(objective.MeshVR);
    }

    // ---------- addons ----------

    public void InsertAddon(GameObject addonPrefab)
    {
        if (Addons == null)
        {
            Debug.LogWarning("UIManager: Addons canvas is not assigned, cannot insert " + addonPrefab.name);
            return;
        }

        Instantiate(addonPrefab, Addons.transform, worldPositionStays: false);
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, Mathf.Clamp01(alpha));
    }

    public class Objective
    {
        public int ID;
        public string Description;
        public GameObject Mesh;
        public GameObject MeshVR;
        public ObjectiveState State;
    }

    public enum ObjectiveState
    {
        None,
        Failed,
        Success,
        Complete
    }
}
