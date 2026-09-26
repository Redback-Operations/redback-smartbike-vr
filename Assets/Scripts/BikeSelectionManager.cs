using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BikeSelectionManager : MonoBehaviour
{
    [Header("Bike Cards")]
    public Button bikeCard1;
    public Button bikeCard2;
    public Button bikeCard3;

    private Color normalColor = new Color(0.08f, 0.25f, 0.30f);
    private Color selectedColor = new Color(0.2f, 0.5f, 1f);

    private string selectedBike = "";

    void Start()
    {
        selectedBike = PlayerPrefs.GetString("SelectedBike", "");

        if (!string.IsNullOrEmpty(selectedBike))
        {
            HighlightSelectedBike(selectedBike);
        }
    }

    public void SelectBike(string bikeName)
    {
        selectedBike = bikeName;

        PlayerPrefs.SetString("SelectedBike", bikeName);
        PlayerPrefs.Save();

        Debug.Log("Selected Bike: " + bikeName);

        HighlightSelectedBike(bikeName);
    }

    void HighlightSelectedBike(string bikeName)
    {
        bikeCard1.image.color = normalColor;
        bikeCard2.image.color = normalColor;
        bikeCard3.image.color = normalColor;

        if (bikeName == "EBikePro")
        {
            bikeCard1.image.color = selectedColor;
        }
        else if (bikeName == "MountainBike")
        {
            bikeCard2.image.color = selectedColor;
        }
        else if (bikeName == "RoadBike")
        {
            bikeCard3.image.color = selectedColor;
        }
    }

    public void GoToRideSetting()
    {
        if (string.IsNullOrEmpty(selectedBike))
        {
            Debug.Log("Please select a bike first.");
            return;
        }

        SceneManager.LoadScene("RideSetting");
    }
}
