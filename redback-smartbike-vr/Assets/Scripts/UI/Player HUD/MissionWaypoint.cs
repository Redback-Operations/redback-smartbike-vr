using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionWaypoint : MonoBehaviour
{
    public Image img;
    public Transform target;

    void Start()
    {

    }

    private void Update()
    {
        if (target != null)
        {
            // Keep x within the width of the screen
            float minX = img.GetPixelAdjustedRect().width / 2;
            float maxX = Screen.width - minX;

            // Keep y within the height of the screen
            float minY = img.GetPixelAdjustedRect().height / 2;
            float maxY = Screen.height - minY;

            // Find the camera associated with your XR setup (adjust the tag accordingly)
            Camera xrCamera = Camera.main; // Replace with your XR camera finding logic

            if (xrCamera != null)
            {
                Vector2 pos = xrCamera.WorldToScreenPoint(target.position);

                // Consistently change position
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                img.transform.position = pos;
            }
        }
    }

}