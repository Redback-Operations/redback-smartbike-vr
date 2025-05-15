using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    // TODO suggest using a similar system to the collectable's and having event registration that listens to specific things happening on the UI
    //      see the bottom section for the registration of score updates, you can then have TMP_Text items be notified of the change in the score
    //      and make their own specific adjustments to how it is viewed - registration is done by UIManager.Instance.RegisterScoreListener(FUNCTIONNAME)

    // the instance for the UI data
    public static UIManager Instance;

    private int _objectiveId;
    private List<Objective> _objectives;

    public Canvas Objectives;
    public Canvas ObjectivesVR;

    public TMP_Text Notification;
    public TMP_Text NotificationVR;

    private int _score;
    public TMP_Text Score;
    public TMP_Text ScoreVR;

    void Start()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;

        _score = 0;

        _objectiveId = 0;
        _objectives = new List<Objective>();

        if (Notification != null)
            Notification.text = "";

        if (NotificationVR != null)
            NotificationVR.text = "";
    }

    void Update()
    {
        foreach (var objective in _objectives)
        {
            if (objective.State == objective.LastState)
                continue;

            if (objective.Mesh != null)
            {
                var tmp = objective.Mesh.GetComponent<TextMeshProUGUI>();

                switch (objective.State)
                {
                    case ObjectiveState.Success:
                    case ObjectiveState.Complete:
                    {
                            tmp.color = Color.green;
                    } break;

                    case ObjectiveState.Failed:
                    {
                        tmp.color = Color.red;
                        tmp.fontStyle = FontStyles.Strikethrough;
                    } break;
                }
            }

            objective.LastState = objective.State;
        }
    }

    void OnDestroy()
    {
        // if this is the current UI instance, clear it
        if (Instance == this)
            Instance = null;
    }

    public IEnumerator ShowNotification(string text, float length)
    {
        // if neither are set, return without doing anything
        if (Notification == null && NotificationVR == null)
            yield break;

        // update the text and initial alpha for both possible notifications
        if (Notification != null)
        {
            Notification.text = text;
            Notification.color = UpdateAlpha(Notification.color, 0);
        }

        if (NotificationVR != null)
        {
            NotificationVR.text = text;
            NotificationVR.color = UpdateAlpha(NotificationVR.color, 0);
        }

        var start = 0f;

        // fade in over half a second
        while (start < 0.5f)
        {
            start += Time.deltaTime;

            var alpha = Mathf.Lerp(0, 1, start * 2);

            if (Notification != null)
                Notification.color = UpdateAlpha(Notification.color, alpha);

            if (NotificationVR != null)
                NotificationVR.color = UpdateAlpha(NotificationVR.color, alpha);

            yield return null;
        }

        // display the text for the specified length
        yield return new WaitForSeconds(length);

        start = 0f;

        // fade out over half a second
        while (start < 0.5)
        {
            start += Time.deltaTime;

            var alpha = Mathf.Lerp(1, 0, start * 2);

            if (Notification != null)
                Notification.color = UpdateAlpha(Notification.color, alpha);

            if (NotificationVR != null)
                NotificationVR.color = UpdateAlpha(NotificationVR.color, alpha);

            yield return null;
        }

        if (Notification != null)
            Notification.text = "";

        if (NotificationVR != null)
            NotificationVR.text = "";
    }

    public void SetScore(int value)
    {
        _score = value;
        Score.text = $"{_score}";
        ScoreVR.text = $"{_score}";
    }

    public void AddScore(int value)
    {
        _score += value;
        Score.text = $"{_score}";
        ScoreVR.text = $"{_score}";

        // invoke the score listener
        _scoreListener?.Invoke(_score);
    }

    private UnityEvent<int> _scoreListener;
    public void RegisterScoreListener(UnityAction<int> call)
    {
        if (_scoreListener == null)
            _scoreListener = new UnityEvent<int>();
        _scoreListener.AddListener(call);
    }

    public void DeregisterScoreListener(UnityAction<int> call)
    {
        _scoreListener?.RemoveListener(call);
    }

    public int AddObjective(string text)
    {
        var id = ++_objectiveId;
        var objective = new Objective
        {
            ID = id,
            Description = text
        };

        var offsetY = _objectives.Count * -50;

        Debug.Log($"Objective {id} added: {text}");

        if (Objectives != null)
        {
            objective.Mesh = new GameObject($"Objective_{id}_TMP");
            objective.Mesh.transform.parent = Objectives.transform;

            var tmp = objective.Mesh.AddComponent<TextMeshProUGUI>();
            tmp.text = text;

            var offset = objective.Mesh.GetComponent<RectTransform>();

            offset.pivot = new Vector2(0, 1);
            offset.anchorMin = new Vector2(0, 1);
            offset.anchorMax = new Vector2(0, 1);

            offset.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400);
            offset.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 50);

            objective.Mesh.transform.SetLocalPositionAndRotation(new Vector3(0, offsetY), Quaternion.identity);
        }

        if (ObjectivesVR != null)
        {
            objective.MeshVR = new GameObject($"Objective_{id}_TMP_VR");
            objective.MeshVR.transform.parent = ObjectivesVR.transform;
        }

        _objectives.Add(objective);

        return id;
    }

    public void RemoveObjective(int id)
    {
        var objective = _objectives.FirstOrDefault(e => e.ID == id);

        if (objective == null)
            return;

        if (objective.Mesh != null)
            Destroy(objective.Mesh);
        if (objective.MeshVR != null)
            Destroy(objective.MeshVR);

        _objectives.Remove(objective);
    }

    public IEnumerator ClearObjectives()
    {
        // only try to clear if there are objectives
        if (_objectives.Count == 0)
            yield break;

        // store the objectives in an array
        var objectives = _objectives.ToList();

        // reset the objectives
        _objectiveId = 0;
        _objectives = new List<Objective>();

        var time = 0f;

        while (time < 1f)
        {
            // loop over the saved objectives
            foreach (var objective in objectives)
            {
                time += Time.deltaTime;
                var alpha = Mathf.Lerp(1, 0, time);

                if (objective.Mesh != null)
                {
                    var tmp = objective.Mesh.GetComponent<TextMeshProUGUI>();
                    tmp.color = UpdateAlpha(tmp.color, alpha);
                }

                if (objective.MeshVR != null)
                {
                    var tmp = objective.MeshVR.GetComponent<TextMeshProUGUI>();
                    tmp.color = UpdateAlpha(tmp.color, alpha);
                }
            }

            yield return null;
        }

        foreach (var objective in objectives)
        {
            if (objective.Mesh != null)
                Destroy(objective.Mesh);

            if (objective.MeshVR != null)
                Destroy(objective.MeshVR);
        }
    }

    public void SetObjectiveState(int id, ObjectiveState state)
    {
        // is this all or a specific objective?
        if (id == -1)
        {
            // loop through each and update their state
            foreach (var objective in _objectives)
            {
                objective.State = state;
            }
        }
        else
        {
            // find the specific objective and update its state
            var objective = _objectives.FirstOrDefault(e => e.ID == id);

            if (objective != null)
                objective.State = state;
        }
    }

    public class Objective
    {
        public int ID;
        public string Description;
        public GameObject Mesh;
        public GameObject MeshVR;

        public ObjectiveState State;
        public ObjectiveState LastState;
    }

    public enum ObjectiveState
    {
        None,

        Failed,
        Success,

        Complete
    }

    private Color UpdateAlpha(Color original, float alpha)
    {
        return new Color(original.r, original.g, original.b, alpha);
    }
}
