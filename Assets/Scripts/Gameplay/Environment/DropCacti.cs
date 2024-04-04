using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropCacti : MonoBehaviour
{
    public float positionRandomness = 5f;
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in gameObject.transform)
        {
            //Randomise the X and Z position
            float randomX = Random.Range(-1f * positionRandomness, positionRandomness);
            float randomZ = Random.Range(-1f * positionRandomness, positionRandomness);
            Vector3 randomXZ = new Vector3(randomX, 0f, randomZ);
            child.localPosition = child.localPosition + randomXZ;

            //Make a downwards raycast to drop the object on the ground
            RaycastHit hit;
            Vector3 down = -Vector3.up * 1000;
            Ray downRay = new Ray(child.transform.position, down);
            //Debug.DrawRay(transform.position, down, Color.red);

            if (Physics.Raycast(downRay, out hit)){
                Vector3 groundDistance = new Vector3(0f, hit.distance * -1f, 0f);
                child.localPosition = child.localPosition + groundDistance;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
