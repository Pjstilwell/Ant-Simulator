using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System;
public class AntBehaviour : MonoBehaviour
{
    Random random = new Random();

    //ant's rigid body
    [SerializeField] Rigidbody rb;
    //food GO
    // [SerializeField] GameObject food;
    [SerializeField] Collider foodCollider;
    //nest GO
    [SerializeField] public GameObject nest;
    //speed of ant
    [SerializeField] float antSpeed;
    //factor by which ants speed is capped
    [SerializeField] float capSpeedFactor = 0.7f;
    //limits number of frames used for a ant's turn in movement
    [SerializeField] int turnFrameCounterLimit = 1000;
    //turn speed multplier (cross vector)
    [SerializeField] float turnCoefficient = 1;
    //distance in which an ant will see food and move towards it
    [SerializeField] float foodAttractionRadius;
    //distance in which an ant will obtain food from the position of the food itself
    [SerializeField] float foundFoodRadius = 0.1f;
    //as above but delivering food to nest
    [SerializeField] float deliverFoodRadius = 0.1f;
    //distance in which an ant will advertise state to other ants
    [SerializeField] float advertiseRadius = 0.2f;
    //interval by which a position is inserted into the trail
    [SerializeField] int trailFrameStep = 500;
    //interval by which a marker is dropped
    [SerializeField] int trailMarkerStep = 500;
    //Trail Marker Object
    [SerializeField] GameObject trailMarker;
    //Stopped
    [SerializeField] public bool stopped;
    //How many frames to stop when advertised to
    [SerializeField] long stopFramesCount;
    //How many frames to stop periodically, actual amount will range randomly
    [SerializeField] int inputStopFramesPeriodicallyCount;
    private int stopFramesPeriodicallyCount;
    //How many frames til stop periodically, actual amount will range randomly
    [SerializeField] int inputFramesTilStopPeriodicallyCount;
    private int framesTilStopPeriodicallyCount;

    //measures if an ant should turn left or right in the current turn
    private bool turnLeft = true;
    //measures if an ant is currently in a movement turn
    private bool currentlyTurning = false;
    //variable to randomly alter turn factor
    private float randTurnMultiplier = 1;
    //variable to randomly alter turn length
    private int randTurnFrameCounterLimit = 1000;
    //counts frames during a turn
    private int turnFrameCounter = 0;
    //counts frames between trail position intervals
    private int trailFrameCounter = 0;
    //counts frames til drop a marker
    private int dropMarkerCounter = 0;
    //counts frames stopped
    private long stopFramesCounter = 0;
    //counts frames til stop periodically
    private long stopFramesPeriodicallyCounter = 0;
    //stores current trail of ant normal movement behaviour
    public List<Vector3> currentTrail = new List<Vector3>();
    //copies currentTrail when ant deliver's food, used to trace back path
    public List<Vector3> oldTrail = new List<Vector3>();
    //used to store current ant movement state
    public int antMovementState = Constants.ANT_MOVEMENT_STATE_NORMAL;

    //Tracks the current index the ant is moving toward in the trail
    //Note that this currently is intended to handle going both back 
    //and forth between food
    public int currentTrailStepIndex = 0;

    public GameObject foodFoundGo;

    //variable used to access script and variables from another ant
    private AntBehaviour anotherAntScript;

    //store velocity for wait time
    private Vector3 storedVel;


    // Start is called before the first frame update
    void Start()
    {
        //Start with random direction
        rb.velocity = new Vector3(GetRandomNumber(-1, 1), GetRandomNumber(-1, 1), 0);

        //Add nest to trail
        currentTrail.Add(nest.transform.position);
        RandomiseStopPeriodicallyVals();
    }

    // Update is called once per frame
    void Update()
    {
        if (stopped)
        {
            Wait();
            return;
        }

        if (antMovementState != Constants.ANT_MOVEMENT_STATE_FOUND_FOOD)
        {
            logTrail();
        }
        antMovementState = determineMovementState();

        switch (antMovementState)
        {
            case Constants.ANT_MOVEMENT_STATE_NORMAL:
                //Random Movement
                if (currentlyTurning)
                {
                    keepTurning();
                }
                else
                {
                    startNewTurn();
                }
                break;
            case Constants.ANT_MOVEMENT_STATE_SEES_FOOD:
                goToFood();
                break;
            case Constants.ANT_MOVEMENT_STATE_FOUND_FOOD:
                foundFood();
                break;
            case Constants.ANT_MOVEMENT_STATE_FOLLOW_FOOD_TRAIL:
                followFoodTrail();
                break;
        }

        capVelocity();
        RotateTowardsTarget();
        dropMarker();
        StopPeriodically();
    }

    private int determineMovementState()
    {
        switch (antMovementState)
        {
            //if in state found food, check if need to go back to normal
            case Constants.ANT_MOVEMENT_STATE_FOUND_FOOD:
                advertiseFood();
                if ((nest.transform.position - transform.position).magnitude < deliverFoodRadius)
                {
                    //reset the currentTrail
                    oldTrail = new List<Vector3>();
                    foreach (Vector3 position in currentTrail)
                    {
                        oldTrail.Add(position);
                    }

                    currentTrail = new List<Vector3>
                    {
                        nest.transform.position
                    };

                    currentTrailStepIndex = 1;

                    return Constants.ANT_MOVEMENT_STATE_FOLLOW_FOOD_TRAIL;
                }
                break;
            //else if in sees food state, check if need to switch to found food
            case Constants.ANT_MOVEMENT_STATE_SEES_FOOD:
                if ((foodFoundGo.transform.position - transform.position).magnitude < foundFoodRadius)
                {
                    // GetComponent<Renderer>().material.color = Color.magenta;
                    currentTrail.Add(foodFoundGo.transform.position);
                    //Since we add food position, go to step before food position
                    currentTrailStepIndex = currentTrail.Count - 1;
                    return Constants.ANT_MOVEMENT_STATE_FOUND_FOOD;
                }
                break;
            case Constants.ANT_MOVEMENT_STATE_FOLLOW_FOOD_TRAIL:
                advertiseFood();
                if ((foodFoundGo.transform.position - transform.position).magnitude < foundFoodRadius)
                {
                    // GetComponent<Renderer>().material.color = Color.blue;
                    currentTrail.Add(foodFoundGo.transform.position);
                    //Since we add food position, go to step before food position
                    currentTrailStepIndex = currentTrail.Count - 1;
                    return Constants.ANT_MOVEMENT_STATE_FOUND_FOOD;
                }
                break;
            //else if in normal (random movement) state, check if ant can see food
            case Constants.ANT_MOVEMENT_STATE_NORMAL:
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, foodAttractionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.CompareTag("food"))
                    {
                        // GetComponent<Renderer>().material.color = Color.red;
                        foodFoundGo = hitCollider.gameObject;
                        return Constants.ANT_MOVEMENT_STATE_SEES_FOOD;
                    }
                }
                break;
            default:
                return antMovementState;
        }

        //if no change needed, return current state
        return antMovementState;
    }

    public void RotateTowardsTarget()
    {

        var rotation = Quaternion.LookRotation(rb.velocity);
        rotation *= Quaternion.Euler(-90, 0, -90); // this adds a rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
    }

    public void capVelocity()
    {
        if (rb.velocity.magnitude > antSpeed)
        {
            Vector3 normalised = rb.velocity;
            normalised.Normalize();
            rb.velocity = Vector3.Lerp(rb.velocity, normalised * antSpeed, capSpeedFactor);
        }
    }

    //randomly returns true or false to turn the ant left or right
    //left is true
    public bool leftOrRight()
    {
        if (GetRandomNumber(-1, 1) < 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //handle current turning motion
    public void keepTurning()
    {
        if (turnFrameCounter < randTurnFrameCounterLimit)
        {
            Vector3 normVel = rb.velocity;
            normVel.Normalize();
            //calculate force perpendicular to motion
            Vector3 crossVec = Vector3.Cross(normVel, Vector3.up);
            if (turnLeft)
            {
                crossVec *= randTurnMultiplier * turnCoefficient;
            }
            else
            {
                crossVec *= -randTurnMultiplier * turnCoefficient;
            }
            rb.AddForce(crossVec);
            Vector3 setAntSpeedVector = rb.velocity;
            setAntSpeedVector.Normalize();
            rb.velocity = setAntSpeedVector * antSpeed;
            turnFrameCounter++;
        }
        else
        {
            currentlyTurning = false;
            return;
        }
    }

    //start a new turn
    public void startNewTurn()
    {
        randTurnFrameCounterLimit = UnityEngine.Random.Range(0, turnFrameCounterLimit);
        randTurnMultiplier = GetRandomNumber(0, 1);
        turnLeft = leftOrRight();
        turnFrameCounter = 0;
        currentlyTurning = true;
    }

    //Move ant towards food
    private void goToFood()
    {
        Vector3 attractedToFoodVector = foodFoundGo.transform.position - transform.position;
        attractedToFoodVector.Normalize();
        rb.velocity = attractedToFoodVector;
    }

    private void logTrail()
    {
        if (trailFrameCounter == trailFrameStep)
        {
            currentTrail.Add(transform.position);
            trailFrameCounter = 0;
        }
        else
        {
            trailFrameCounter++;
        }
    }

    //Follow trail back to nest
    public void foundFood()
    {
        Vector3 nextStep = currentTrail[currentTrailStepIndex];
        Vector3 goToNextStepVector = nextStep - transform.position;

        //If at the next step, remove from array and continue along trail
        //Note checking for distance being half ant speed prevents bug where ant moves back and forth
        //over intended point
        if (goToNextStepVector.magnitude <= deliverFoodRadius)
        {
            currentTrailStepIndex--;
            return;
        }
        else
        {
            goToNextStepVector.Normalize();
            rb.velocity = goToNextStepVector * antSpeed;
        }
    }

    //Follow trail back to food
    public void followFoodTrail()
    {

        //Unknown bug why it ever hits this
        if (currentTrailStepIndex == oldTrail.Count)
        {
            return;
        }

        Vector3 nextStep = oldTrail[currentTrailStepIndex];
        Vector3 goToNextStepVector = nextStep - transform.position;

        //If at the next step, remove from array and continue along trail
        //Note checking for distance being half ant speed prevents bug where ant moves back and forth
        //over intended point
        if (goToNextStepVector.magnitude <= foundFoodRadius)
        {
            currentTrailStepIndex++;
            return;
        }
        else
        {
            goToNextStepVector.Normalize();
            rb.velocity = goToNextStepVector * antSpeed;
        }
    }

    //Drop marker at current position
    public void dropMarker()
    {
        if (dropMarkerCounter == trailMarkerStep)
        {
            GameObject trailMarkerClone = Instantiate(trailMarker, transform.position, Quaternion.identity);
            trailMarkerClone.GetComponent<TrailMarker>().enabled = true;
            dropMarkerCounter = 0;
        }
        else
        {
            dropMarkerCounter++;
        }
    }

    public bool advertiseFood()
    {
        bool antConverted = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, advertiseRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.CompareTag("ant"))
            {

                GameObject anotherAnt = hitCollider.gameObject;
                AntBehaviour anotherAntScript = anotherAnt.GetComponent<AntBehaviour>();
                int anotherAntState = anotherAntScript.antMovementState;

                //Other ant inherits the state of this ant given it does not know about the food
                if (anotherAntState == Constants.ANT_MOVEMENT_STATE_NORMAL)
                {
                    anotherAntScript.antMovementState = antMovementState;
                    anotherAntScript.currentTrail = currentTrail;
                    anotherAntScript.currentTrailStepIndex = currentTrailStepIndex;
                    anotherAntScript.oldTrail = oldTrail;
                    anotherAntScript.foodFoundGo = foodFoundGo;
                    anotherAntScript.Stop(stopFramesCount);
                    antConverted = true;
                }
            }
        }
        return antConverted;
    }

    public void StopPeriodically()
    {
        stopFramesPeriodicallyCounter++;
        if (stopFramesPeriodicallyCounter == framesTilStopPeriodicallyCount)
        {
            stopFramesPeriodicallyCounter = 0;
            Stop(stopFramesPeriodicallyCount);
            RandomiseStopPeriodicallyVals();
        }
    }

    private void RandomiseStopPeriodicallyVals()
    {
        framesTilStopPeriodicallyCount = random.Next(inputFramesTilStopPeriodicallyCount / 2, inputFramesTilStopPeriodicallyCount * 2);
        stopFramesPeriodicallyCount = random.Next(inputStopFramesPeriodicallyCount / 2, inputStopFramesPeriodicallyCount * 2);
    }

    public void Stop(long stopFramesCount)
    {
        this.stopFramesCount = stopFramesCount;
        stopped = true;
        storedVel = rb.velocity;
        rb.velocity = Vector3.zero;
        GetComponent<Animator>().enabled = false;
        Wait();
    }

    private bool Wait()
    {
        stopFramesCounter++;
        if (stopFramesCounter == stopFramesCount)
        {
            StartAgain();
            stopFramesCounter = 0;
            return true;
        }
        return false;
    }

    private void StartAgain()
    {
        stopped = false;
        rb.velocity = storedVel;
        GetComponent<Animator>().enabled = true;
    }

    public float GetRandomNumber(float min, float max)
    {
        float rnd = (float)random.NextDouble();
        return ((max - min) * rnd) + min;
    }
}
