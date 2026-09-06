using UnityEngine;

/// <summary>
/// Whether the player has agreed to have their bike/session data collected
/// and (eventually) sent to the project's backend. Backed by PlayerPrefs so
/// the choice survives between sessions. Defaults to "not asked" rather than
/// assuming consent either way.
///
/// Wire a consent prompt's Yes/No buttons to Grant()/Deny() - check
/// HasBeenAsked wherever a session would normally start, and show the prompt
/// first if it's still false. No UI is included here since this project
/// doesn't have a settled place for this kind of prompt yet.
/// </summary>
public static class PlayerDataConsent
{
    private const string AskedKey = "PlayerDataConsent_Asked";
    private const string GrantedKey = "PlayerDataConsent_Granted";

    public static bool HasBeenAsked => PlayerPrefs.GetInt(AskedKey, 0) == 1;

    public static bool IsGranted => PlayerPrefs.GetInt(GrantedKey, 0) == 1;

    public static void Grant()
    {
        PlayerPrefs.SetInt(AskedKey, 1);
        PlayerPrefs.SetInt(GrantedKey, 1);
        PlayerPrefs.Save();
    }

    public static void Deny()
    {
        PlayerPrefs.SetInt(AskedKey, 1);
        PlayerPrefs.SetInt(GrantedKey, 0);
        PlayerPrefs.Save();
    }
}
