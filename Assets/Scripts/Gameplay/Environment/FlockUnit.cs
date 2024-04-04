using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script was mostly copied from this tutorial https://www.youtube.com/watch?v=mBVarJm3Tgk

public class FlockUnit : MonoBehaviour
{

    [SerializeField] private float FOVAngle;    
    [SerializeField] private float smoothDamp;

    private List<FlockUnit> cohesionNeighbours = new List<FlockUnit>();
    private Flock assignedFlock;
    private Vector3 currentVelocity;
    private float speed;

    public FlockUnit[] allUnits { get; set; }

    public Transform myTransform { get; set; }

    private void Awake()
    {
        myTransform = transform;
    }

    public void AssignFlock(Flock flock)
    {
        assignedFlock = flock;
    }

    public void InitialiseSpeed(float speed)
    {
        this.speed = speed;
    }

    public void MoveUnit()      //Movement function, called to in Flock's update function.
    {
        FindNeighbours();
        CalculateSpeed();

        var cohesionVector = CalculateCohesionVector();
        var boundsVector = CalculateBoundsVector();

        var moveVector = cohesionVector + boundsVector;
        moveVector = Vector3.SmoothDamp(myTransform.forward, moveVector, ref currentVelocity, smoothDamp);
        moveVector = moveVector.normalized * speed;
        if (moveVector == Vector3.zero)
        {
            moveVector = transform.forward;
        }
        
        myTransform.forward = moveVector;
        myTransform.position += moveVector * Time.deltaTime;
    }

    private void CalculateSpeed()       //If I fly near other crows, I match their speed. 
    {
        if (cohesionNeighbours.Count == 0)
        {
            return;
        }
        speed = 0;
        for (int i = 0; i < cohesionNeighbours.Count; i++)
        {
            speed += cohesionNeighbours[i].speed;
        }
        speed /= cohesionNeighbours.Count;
        speed = Mathf.Clamp(speed, assignedFlock.minSpeed, assignedFlock.maxSpeed);
    }

    private void FindNeighbours()           
    {
        cohesionNeighbours.Clear();
        var allUnits = assignedFlock.allUnits;
        for (int i = 0; i < allUnits.Length; i++)
        {
            var currentUnit = allUnits[i];
            if (currentUnit != this)
            {
                float currentNeighbourDistanceSqr = Vector3.SqrMagnitude(currentUnit.myTransform.position - myTransform.position);
                if (currentNeighbourDistanceSqr <= assignedFlock.cohesionDistance * assignedFlock.cohesionDistance)
                {
                    cohesionNeighbours.Add(currentUnit);
                }
            }   
        }
    }

    private Vector3 CalculateCohesionVector()       //If I'm near other crows, I fly near them.
    {
        var cohesionVector = Vector3.zero;
        int neighboursInFOV = 0;
        if(cohesionNeighbours.Count == 0)
        {
            return cohesionVector;
        }
        for (int i = 0; i < cohesionNeighbours.Count; i++)
        {
            if (IsInFOV(cohesionNeighbours[i].myTransform.position))
            {
                neighboursInFOV += 1;
                cohesionVector += cohesionNeighbours[i].myTransform.position;
            }
        }
        if (neighboursInFOV == 0)
        {
            return cohesionVector;
        }

        cohesionVector /= neighboursInFOV;
        cohesionVector -= myTransform.position;
        cohesionVector = Vector3.Normalize(cohesionVector);
        return cohesionVector;
    }

    private Vector3 CalculateBoundsVector()     //If I'm approaching the boundary, I turn back. 
    {
        var offsetToCenter = assignedFlock.transform.position - myTransform.position;
        bool isNearCenter = (offsetToCenter.magnitude >= assignedFlock.boundsDistance * 0.9f);
        return isNearCenter ? offsetToCenter.normalized : Vector3.zero;
    }

    private bool IsInFOV(Vector3 position)
    {
        return Vector3.Angle(myTransform.forward, position - myTransform.position) <= FOVAngle;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   

        
    }
}
