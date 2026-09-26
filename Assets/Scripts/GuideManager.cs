using UnityEngine;
using UnityEngine.SceneManagement;

public class GuideManager : MonoBehaviour
{
    public void StartRide()
    {
        SceneManager.LoadScene("BikeSelection");
    }
}
