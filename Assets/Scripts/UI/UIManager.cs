using System.Collections;
using System.Collections.Generic;
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

    private int _score;
    public TMP_Text Score;
    public TMP_Text ScoreVR;

    void Start()
    {
        if (Instance != null)
            Destroy(gameObject);

        Instance = this;
        _score = 0;
    }

    void OnDestroy()
    {
        // if this is the current UI instance, clear it
        if (Instance == this)
            Instance = null;
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
}
