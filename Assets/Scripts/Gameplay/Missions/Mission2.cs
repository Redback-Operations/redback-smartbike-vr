using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Mission2 : Mission
{
    public override int MissionNumber => 2;
    public override string MissionName => "Collect in Order";

    public GameObject silver_cin;
    public GameObject gold_coin;
    public GameObject Star;

    private GameObject[] requiredItems;
    private GameObject[] randomItems;
    private List<int> objectives;
    private int overallObjective;

    private int arrayChange;
    private int currentItemIndex = 0;
    private float delay = 2.0f;
    private bool missionComplete = false; //Tracking whether mission completed
    private bool missionSuccess = false;

    float elapsedTime = 0f;

    //Modifying the code to work on a time trial version of the mission. Record time and change time when beaten the time.
    //Separating the items far enough to give the bike enough exercise.

    IEnumerator StartResetting()
    {
        Debug.Log("Start Resetting");
        yield return UIManager.Instance.ShowNotification(missionSuccess ? "Mission Complete!" : "Mission Failed!", delay);
        yield return UIManager.Instance.ClearObjectives();
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
    public override void StartMission()
    {
        Debug.Log("Start Mission Called!");

        requiredItems = new GameObject[] { silver_cin, gold_coin, Star };
        randomItems = ShuffleArray(requiredItems);
        objectives = new List<int>();

        overallObjective = UIManager.Instance.AddObjective("Collect in Order:");

        foreach (GameObject item in randomItems)
        {
            var id = UIManager.Instance.AddObjective($" - {item.name}");
            objectives.Add(id);
        }

        arrayChange = requiredItems.Length;

        base.StartMission();
    }

    // Update is called once per frame
    void Update()
    {
        if (!MissionStarted || missionComplete)
            return;

        if (currentItemIndex >= arrayChange)
        {
            missionComplete = true;
            missionSuccess = true;

            UIManager.Instance.SetObjectiveState(overallObjective, UIManager.ObjectiveState.Complete);

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

            // Check if the current required item is inactive
            if (requiredItems[currentItemIndex].activeSelf == false)
            {
                UIManager.Instance.SetObjectiveState(objectives[currentItemIndex], UIManager.ObjectiveState.Success);
                currentItemIndex++;
            }
            else
            {
                // Check for any item that is collected out of order
                for (int i = currentItemIndex + 1; i < requiredItems.Length; i++)
                {
                    if (requiredItems[i].activeSelf)
                        continue;

                    // set all to failed
                    UIManager.Instance.SetObjectiveState(-1, UIManager.ObjectiveState.Failed);
                    missionComplete = true;

                    Debug.Log("Failed!");

                    StartCoroutine(StartResetting());
                    return;
                }

            }

        }
    }

}
