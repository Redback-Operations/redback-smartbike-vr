using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class ChainAudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    private float speed;
    private Vector3 prevPos; // position from previous update
    private List<float> speeds; // store a number of speed readings to average
    private int arrayLength = 50; // number of speed readings to keep
    public AudioSource chainSource; // AudioSource playing chain audio
    public AudioClip chainAudio; // Audio clip for bike chain
    private float audioMod; // float to adjust audio speed, pitch

    void Start()
    {
        prevPos = transform.position;
        speeds = new List<float>();
    }

    void FixedUpdate()
    {
        speed = Vector3.Distance(transform.position, prevPos) ; // get distance between current position and previous position
        prevPos = transform.position; // update previous position for next update
        UpdateArray();
        GetAverageSpeed();
        UpdateAudioSource();
        //Debug.Log("Bike speed: " + speed);
    }

    void UpdateArray()
    {
        if (speeds.Count >= arrayLength) // if stored speed values is exceeded
        {
            speeds.RemoveAt(0); // remove oldest speed value
            speeds.Add(speed); // add new speed value
        } else
        {
            speeds.Add(speed); // add new speed value
        }
    }

    float GetAverageSpeed ()
    {
        //Debug.Log("Average speed: " + speeds.Average());
        return speeds.Average();
    }

    void UpdateAudioSource ()
    {
        audioMod = GetAverageSpeed() / 0.064f; // normalise to approx max speed
        if (audioMod > 1)
            audioMod = 1; // cap at 1
        //Debug.Log("audioMod: " + audioMod);

        if (Mathf.Approximately(audioMod, 0))
            chainSource.volume = 0; // mute chain if bike is stopped

        chainSource.volume = audioMod;

        audioMod = audioMod * 2.5f;
        chainSource.pitch = audioMod;
    }
}
