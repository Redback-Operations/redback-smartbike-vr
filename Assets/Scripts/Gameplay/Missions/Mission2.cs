using System.Collections;
using UnityEngine;
using TMPro;

public class Mission2 : MonoBehaviour
{
    public GameObject silver_cin;
    public GameObject gold_coin;
    public GameObject Star;

    private GameObject[] requiredItems;
    private GameObject[] randomItems;
    private int arrayChange;
    private int currentItemIndex = 0;
    private float delay = 2.0f;
    //For time delay to reset.
    private bool isDelayed = false;
    public bool missionComplete = false; //Tracking whether mission completed
    public TextMeshProUGUI missionStatus; //Ref to TextMeshPro
    public TextMeshProUGUI missionCollect;
    public TextMeshProUGUI timer;
    float elapsedTime = 0f;

    //Modifying the code to work on a time trial version of the mission. Record time and change time when beaten the time.
    //Separating the items far enough to give the bike enough exercise.

    IEnumerator StartResetting()
    {
        // Set resetting flag to true to prevent multiple coroutines running simultaneously
        isDelayed = true;

        // Wait for the specified delay before resetting the text
        yield return new WaitForSeconds(delay);

        if (missionStatus != null)
            missionStatus.text = null;
        if (timer != null)
            timer.text = null;

        // Reset the text
        isDelayed = false;
    }

    private GameObject[] ShuffleArray(GameObject[] array)
    {
        GameObject[] newArray = array;
        for (int i = array.Length - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            GameObject temp = array[i];
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }

        return newArray;
    }

    // Start is called before the first frame update
    void Start()
    {
        requiredItems = new GameObject[] { silver_cin, gold_coin, Star };
        randomItems = ShuffleArray(requiredItems);

        string missionDescription = "Collect in order: ";

        foreach (GameObject item in randomItems)
        {
            missionDescription += item.name + " ";
        }

        if (missionCollect != null)
            missionCollect.text = missionDescription;
        Debug.Log(missionDescription);

        arrayChange = requiredItems.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (missionComplete)
            return;

        if (currentItemIndex >= arrayChange)
        {
            missionComplete = true;
            if (missionStatus != null)
                missionStatus.text = "Success!";
            Debug.Log("Success!");
            StartCoroutine(StartResetting());
            return;
            //Save the time count
        }

        //In a certain order...
        if (!missionComplete)
        {
            //Should make a new timer count, counting up and then when finishing the code, figure out how to save time.
            elapsedTime += Time.deltaTime;

            if (timer != null)
                timer.text = elapsedTime.ToString();

            // Check if the current required item is inactive
            if (requiredItems[currentItemIndex].activeSelf == false)
            {
                currentItemIndex++;
            }
            else
            {
                // Check for any item that is collected out of order
                for (int i = currentItemIndex + 1; i < requiredItems.Length; i++)
                {
                    if (!requiredItems[i].activeSelf)
                    {
                        missionComplete = true;
                        if (missionStatus != null)
                            missionStatus.text = "Failed!";
                        Debug.Log("Failed!");
                        if (timer != null)
                            timer.text = "0.00";
                        StartCoroutine(StartResetting());
                        return;
                    }
                }

            }

        }
    }

}
