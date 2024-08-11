using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using System;
public class AntBehaviour : MonoBehaviour
{

    [SerializeField] float antSpeed;
    [SerializeField] float rotationSpeed = 10f; 
    [SerializeField] float offset = -90f; 
    [SerializeField] Rigidbody rb;
    [SerializeField] int frameInterval = 100;
    [SerializeField] float capSpeedFactor = 0.7f;
    [SerializeField] int turnFrameCounterLimit = 1000;
    [SerializeField] float turnCoefficient = 1;
    [SerializeField] GameObject food;
    [SerializeField] GameObject nest;
    [SerializeField] float foodAttractionRadius;
    [SerializeField] float foundFoodRadius = 0.1f;
    [SerializeField] float deliverFoodRadius = 0.1f;
    int countFrames = 0;
    Random random = new Random();
    private bool turnLeft = true;
    private bool currentlyTurning = false;
    private float randTurnMultiplier = 1;
    private int randTurnFrameCounterLimit = 1000;
    private int turnFrameCounter = 0;

    [SerializeField] int trailFrameStep = 500;
    private int trailFrameCounter = 0;
    private List<Vector3> currentTrail = new List<Vector3>();
    private List<Vector3> resetTrail = new List<Vector3>();
    
    private int antMovementState = Constants.ANT_MOVEMENT_STATE_NORMAL;

    //Tracks the current index the ant is moving toward in the trail
    private int currentTrailStepIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = new Vector3(antSpeed * 0.2f,0,0);

        currentTrail.Add(nest.transform.position);
        resetTrail.Add(nest.transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        if (antMovementState != Constants.ANT_MOVEMENT_STATE_FOUND_FOOD) {
            logTrail();
        }
        antMovementState = determineMovementState();
        // Debug.Log(antMovementState);

        switch (antMovementState) {
            case Constants.ANT_MOVEMENT_STATE_NORMAL:
                //Random Movement
                if (currentlyTurning) {
                    keepTurning();
                } else {
                    startNewTurn();
                }
            break;
            case Constants.ANT_MOVEMENT_STATE_SEES_FOOD:
                goToFood();
            break;
            case Constants.ANT_MOVEMENT_STATE_FOUND_FOOD:
                foundFood();
            break;
        }
        
        capVelocity();
    }

    private int determineMovementState() {
        switch (antMovementState) {
            //if in state found food, check if need to go back to normal
            case Constants.ANT_MOVEMENT_STATE_FOUND_FOOD:
                if ((nest.transform.position - transform.position).magnitude < deliverFoodRadius) {
                    GetComponent<Renderer>().material.color = Color.black;

                    //reset the currentTrail
                    currentTrail = resetTrail;
                    return Constants.ANT_MOVEMENT_STATE_NORMAL;
                }
            break;
            //else if in sees food state, check if need to switch to found food
            case Constants.ANT_MOVEMENT_STATE_SEES_FOOD:
                if ((food.transform.position - transform.position).magnitude < foundFoodRadius) {
                    GetComponent<Renderer>().material.color = Color.blue;
                    currentTrailStepIndex = currentTrail.Count - 1;
                    return Constants.ANT_MOVEMENT_STATE_FOUND_FOOD;
                }
            break;
            //else if in normal (random movement) state, check if ant can see food
            case Constants.ANT_MOVEMENT_STATE_NORMAL:
                if ((food.transform.position - transform.position).magnitude < foodAttractionRadius) {
                    GetComponent<Renderer>().material.color = Color.red;
                    return Constants.ANT_MOVEMENT_STATE_SEES_FOOD;
                }
            break;
            default:
                return Constants.ANT_MOVEMENT_STATE_NORMAL;
        }
        
        //if no change needed, return current state
        return antMovementState;
    }

    public void RotateTowardsTarget() {
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle + offset, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    public void capVelocity() {
        if (rb.velocity.magnitude > antSpeed) {
            Vector3 normalised = rb.velocity;
            normalised.Normalize();
            rb.velocity = Vector3.Lerp(rb.velocity, normalised * antSpeed, capSpeedFactor);
        }
    }

    //randomly returns true or false to turn the ant left or right
    //left is true
    public bool leftOrRight() {
        if (GetRandomNumber(-1,1) < 0) {
            return true;
        } else {
            return false;
        } 
    }

    //handle current turning motion
    public void keepTurning() {
        if (turnFrameCounter < randTurnFrameCounterLimit) {
            Vector3 normVel = rb.velocity;
            normVel.Normalize();
            //calculate force perpendicular to motion
            Vector3 crossVec = Vector3.Cross(normVel, Vector3.up);
            if (turnLeft) {
                crossVec *= randTurnMultiplier * turnCoefficient;
            } else {
                crossVec *= -randTurnMultiplier * turnCoefficient;
            }
            rb.AddForce(crossVec);
            turnFrameCounter++;
        } else {
            currentlyTurning = false;
            return;
        }
    }

    //start a new turn
    public void startNewTurn() {
        randTurnFrameCounterLimit = UnityEngine.Random.Range(0, turnFrameCounterLimit);
        randTurnMultiplier = GetRandomNumber(0, 1);
        turnLeft = leftOrRight();
        turnFrameCounter = 0;
        currentlyTurning = true;
    }

    //Move ant towards food
    private void goToFood() {
        Vector3 attractedToFoodVector = food.transform.position - transform.position;
        attractedToFoodVector.Normalize();
        rb.velocity = attractedToFoodVector;
    }

    private void logTrail() {
        if (trailFrameCounter == trailFrameStep) {
            currentTrail.Add(transform.position);
            trailFrameCounter = 0;
        } else {
            trailFrameCounter++;
        }
    }

    //Follow trail
    public void foundFood() {

        //Special case
        if (currentTrailStepIndex == -1 ) {
            Vector3 goHome = nest.transform.position - transform.position;
            goHome.Normalize();
            rb.velocity = goHome * antSpeed;
            return;
        }

        Vector3 nextStep = currentTrail[currentTrailStepIndex];
        Vector3 goToNextStepVector = nextStep - transform.position;

        //If at the next step, remove from array and continue along trail
        //Note checking for distance being half ant speed prevents bug where ant moves back and forth
        //over intended point
        if (goToNextStepVector.magnitude <= antSpeed / 2) {
            currentTrailStepIndex--;
            return;
        } else {
            goToNextStepVector.Normalize();
            rb.velocity = goToNextStepVector * antSpeed;
        }
    }

    public float GetRandomNumber(float minimum, float maximum)
    {
        float rnd = (float)this.random.NextDouble();
        return ((maximum - minimum) * rnd) + minimum; 
    }
}
