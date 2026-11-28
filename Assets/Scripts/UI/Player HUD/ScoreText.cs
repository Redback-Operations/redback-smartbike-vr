using TMPro;
using UnityEngine;

// Shows the player's score on a TMP text. Updates when UIManager raises ScoreChanged.
[RequireComponent(typeof(TMP_Text))]
public class ScoreText : MonoBehaviour
{
    [Tooltip("Optional. Leave empty to use the UIManager on the same player.")]
    [SerializeField] private UIManager source;

    [Tooltip("{0} is replaced with the score.")]
    [SerializeField] private string format = "{0}";

    private TMP_Text _text;
    private UIManager _ui;

    void Awake()
    {
        _text = GetComponent<TMP_Text>();
    }

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
        _ui.ScoreChanged += Show;
        Show(_ui.Score);
    }

    private void Unbind()
    {
        if (_ui != null)
            _ui.ScoreChanged -= Show;

        _ui = null;
    }

    private void Show(int score)
    {
        _text.text = string.Format(format, score);
    }
}
