using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

/// <summary>
/// Replaces the old "raceResultText.text = You Win!/You Lose!" wiring that
/// lived on CheckpointManager/PlayerBikeScript. Purely local/per-client -
/// it only *reads* RaceManager's networked state, it never writes it, so
/// it's safe to have this active on every client without any authority
/// checks.
///
/// Wire up in the Inspector inside the Race Mission's Canvas:
///  - countdownText: big "3 / 2 / 1 / GO" text, hide when idle/racing/finished.
///  - raceTimeText: optional, or leave empty and rely on UIManager's
///    existing TimeText/TimeLabel via UpdateTime("Race Time", ...).
///  - standingsText: multiline live leaderboard while racing.
///  - resultsPanel + resultsText: shown once the race finishes.
///  - bestTimeText: shows the persisted personal best for this track.
/// </summary>
public class RaceHUD : MonoBehaviour
{
    [Header("Race Manager")]
    public RaceManager raceManager;

    [Header("Countdown / Timer")]
    public TMP_Text countdownText;
    [Tooltip("If left empty, UIManager.Instance.UpdateTime(\"Race Time\", ...) is used instead.")]
    public TMP_Text raceTimeText;

    [Header("Live Standings")]
    public TMP_Text standingsText;

    [Header("Results")]
    public GameObject resultsPanel;
    public TMP_Text resultsText;

    [Header("Best Time")]
    public TMP_Text bestTimeText;
    public SaveManager saveManager;
    [Tooltip("Key used to store this track's best time. Defaults to the scene name if left blank.")]
    public string trackKey;

    private bool _reportedLocalFinish;

    private void Start()
    {
        if (resultsPanel != null)
            resultsPanel.SetActive(false);

        if (string.IsNullOrEmpty(trackKey))
            trackKey = gameObject.scene.name;

        RefreshBestTimeLabel();
    }

    private void Update()
    {
        if (raceManager == null)
            raceManager = RaceManager.Instance;

        if (raceManager == null)
            return;

        switch (raceManager.State)
        {
            case RaceState.Idle:
                SetCountdownText(string.Empty);
                break;

            case RaceState.Countdown:
                UpdateCountdown();
                break;

            case RaceState.Racing:
                SetCountdownText(string.Empty);
                UpdateRaceTime(raceManager.GetRaceClock());
                UpdateStandings();
                CheckLocalFinish();
                break;

            case RaceState.Finished:
                ShowResults();
                break;
        }
    }

    private void UpdateCountdown()
    {
        var remaining = raceManager.GetCountdownRemaining() ?? 0f;
        SetCountdownText(remaining > 0.35f ? Mathf.CeilToInt(remaining).ToString() : "GO!");
    }

    private void SetCountdownText(string text)
    {
        if (countdownText != null)
            countdownText.text = text;
    }

    private void UpdateRaceTime(float seconds)
    {
        if (raceTimeText != null)
        {
            var mins = (int)(seconds / 60f);
            var secs = seconds % 60f;
            raceTimeText.text = $"{mins:00}:{secs:00.0}";
        }
        else if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateTime("Race Time", seconds);
        }
    }

    private void UpdateStandings()
    {
        if (standingsText == null) return;

        var standings = raceManager.GetLiveStandings();
        var sb = new StringBuilder();

        for (var i = 0; i < standings.Count; i++)
        {
            var racer = standings[i];
            var lapDisplay = Mathf.Min(racer.CurrentLap + 1, raceManager.TotalLaps);
            sb.AppendLine($"{i + 1}. {racer.DisplayName} - Lap {lapDisplay}/{raceManager.TotalLaps}");
        }

        standingsText.text = sb.ToString();
    }

    private void CheckLocalFinish()
    {
        if (_reportedLocalFinish) return;

        var localRacer = FindObjectsOfType<RacerIdentity>().FirstOrDefault(r => r.Object != null && r.Object.HasInputAuthority);
        if (localRacer == null || !localRacer.HasFinished) return;

        var results = raceManager.GetResultsSummary();
        var mine = results.FirstOrDefault(r => r.RacerId == localRacer.Object.Id);
        if (mine.Placement == 0) return; // hasn't propagated through the RPC yet

        _reportedLocalFinish = true;

        EventBus<RaceFinishedEvent>.Raise(new RaceFinishedEvent
        {
            RacerId = localRacer.Object.Id,
            Placement = mine.Placement,
            FinishTime = mine.Time
        });

        var isBest = SaveBestTime(mine.Time);

        if (UIManager.Instance != null)
        {
            var place = mine.Placement switch { 1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{mine.Placement}th" };
            var suffix = isBest ? " - New Best Time!" : "";
            StartCoroutine(UIManager.Instance.ShowNotification($"Finished {place}!{suffix}", 3f));
        }
    }

    private bool SaveBestTime(float time)
    {
        if (saveManager == null) return false;

        var previousBest = saveManager.LoadRaceBestTime(trackKey);
        saveManager.SaveRaceBestTime(trackKey, time);
        RefreshBestTimeLabel();

        return previousBest < 0f || time < previousBest;
    }

    private void RefreshBestTimeLabel()
    {
        if (bestTimeText == null || saveManager == null) return;

        var best = saveManager.LoadRaceBestTime(trackKey);
        bestTimeText.text = best < 0f ? "Best: --:--" : $"Best: {FormatTime(best)}";
    }

    private static string FormatTime(float seconds)
    {
        var mins = (int)(seconds / 60f);
        var secs = seconds % 60f;
        return $"{mins:00}:{secs:00.0}";
    }

    private void ShowResults()
    {
        SetCountdownText(string.Empty);

        if (resultsPanel != null)
            resultsPanel.SetActive(true);

        if (resultsText == null) return;

        var results = raceManager.GetResultsSummary();
        var sb = new StringBuilder();
        sb.AppendLine("Race Results");

        foreach (var (_, name, time, placement, isNpc) in results)
        {
            var tag = isNpc ? " (NPC)" : "";
            sb.AppendLine($"{placement}. {name}{tag} - {FormatTime(time)}");
        }

        resultsText.text = sb.ToString();
    }
}
