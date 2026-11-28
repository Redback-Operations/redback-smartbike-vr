using System;
using TMPro;
using UnityEngine;

// Shows a timer label and value (mm:ss). Updates when UIManager raises TimeChanged.
public class TimeDisplay : MonoBehaviour
{
    [Tooltip("Optional. Leave empty to use the UIManager on the same player.")]
    [SerializeField] private UIManager source;

    [SerializeField] private TMP_Text label;
    [SerializeField] private TMP_Text time;

    private UIManager _ui;

    void OnEnable()
    {
        UIManager.OnPlayerUIManagerReady += OnUIManagerReady;
        Bind(source != null ? source : UIManager.FindFor(this));
    }

    void OnDisable()
    {
        UIManager.OnPlayerUIManagerReady -= OnUIManagerReady;
        Unbind();
    }

    // only used if no UIManager existed when this was enabled
    private void OnUIManagerReady(UIManager ui)
    {
        if (_ui == null)
            Bind(ui);
    }

    private void Bind(UIManager ui)
    {
        if (ui == null || ui == _ui)
            return;

        Unbind();
        _ui = ui;
        _ui.TimeChanged += Show;

        // stay blank until a mission sends a time
        if (_ui.HasTime)
            Show(_ui.TimeLabel, _ui.TimeValue);
        else
            SetText("", "");
    }

    private void Unbind()
    {
        if (_ui != null)
            _ui.TimeChanged -= Show;

        _ui = null;
    }

    private void Show(string labelText, float seconds)
    {
        var t = TimeSpan.FromSeconds(Mathf.Max(0, seconds));
        SetText(labelText, $"{(int)t.TotalMinutes:00}:{t.Seconds:00}");
    }

    private void SetText(string labelText, string timeText)
    {
        if (label != null) label.text = labelText;
        if (time != null) time.text = timeText;
    }
}
