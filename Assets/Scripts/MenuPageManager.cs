using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPageManager : MonoBehaviour
{
    public void BackToRideSetting()
    {
        SceneManager.LoadScene("RideSetting");
    }

    public void GoToStartRide()
    {
        SceneManager.LoadScene("BikeSelection");
    }

    public void GoToInterfaceGuide()
    {
        SceneManager.LoadScene("SmartBikeUI");
    }
}