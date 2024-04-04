using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script was mostly copied from this tutorial https://www.youtube.com/watch?v=mBVarJm3Tgk
//To use, attach Flock.cs to an empty game object to act as the spawnpoint.
//Attach AnimatedBird or any other prefab to flockUnitPrefacb, to act as the flocking object. 
//Adjust Flock Size, Cohesion and Bounds as preferred.



public class Flock : MonoBehaviour
{
    [Header("Spawn Setup")]                //Generates a set number of crows, within a set boundary. 
    [SerializeField] private FlockUnit flockUnitPrefab;
    [SerializeField] private int flockSize;
    [SerializeField] private Vector3 spawnBounds;

    [Header("Speed Setup")]
    [SerializeField] private float _minSpeed;
    public float minSpeed { get { return _minSpeed; } }
    //[Range(0, 10)]
    [SerializeField] private float _maxSpeed;
    public float maxSpeed { get { return _maxSpeed; } }
    //[Range(0, 10)]

    [Header("Detection Distances")]
    [Range(0, 10)]
    [SerializeField] private float _cohesionDistance;   //Crows will attempt to stick together. Lower Cohesion = Crows fly apart.

    [SerializeField] private float _boundsDistance;     //Crows will only fly within this radius. 
    public float boundsDistance {get { return _boundsDistance;} }

    public float cohesionDistance { get { return _cohesionDistance;} }

    public FlockUnit[] allUnits { get; set; }

    private void GenerateUnits()    //Spawns the flock.
    {
        allUnits = new FlockUnit[flockSize];
        for (int i = 0; i < flockSize; i++)
        {
            var randomVector = UnityEngine.Random.insideUnitSphere;
            randomVector = new Vector3 (randomVector.x * spawnBounds.x, randomVector.y * spawnBounds.y, randomVector.z * spawnBounds.z);
            var spawnPosition = transform.position + randomVector;
            var rotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
            allUnits[i] = Instantiate(flockUnitPrefab, spawnPosition, rotation);
            allUnits[i].AssignFlock(this);   
            allUnits[i].InitialiseSpeed(UnityEngine.Random.Range(minSpeed, maxSpeed));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateUnits();
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i  < allUnits.Length; i++)
        {
            allUnits[i].MoveUnit();
        }
    }
}
