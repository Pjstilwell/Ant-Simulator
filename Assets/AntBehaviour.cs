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
    [SerializeField] GameObject nest;
    //speed of ant
    [SerializeField] float antSpeed;
    //speed ant should rotate
    [SerializeField] float rotationSpeed = 10f; 
    //unknown, investigate, to do with rotation
    [SerializeField] float offset = -90f;
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
    //interval by which a position is inserted into the trail
    [SerializeField] int trailFrameStep = 500;

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
    //stores current trail of ant normal movement behaviour
    private List<Vector3> currentTrail = new List<Vector3>();
    //copies currentTrail when ant deliver's food, used to trace back path
    private List<Vector3> oldTrail = new List<Vector3>();
    //used to store current ant movement state
    private int antMovementState = Constants.ANT_MOVEMENT_STATE_NORMAL;

    //Tracks the current index the ant is moving toward in the trail
    //Note that this currently is intended to handle going both back 
    //and forth between food
    private int currentTrailStepIndex = 0;

    private GameObject foodFoundGo;


    // Start is called before the first frame update
    void Start()
    {
        // transform.position = new Vector3(nest.transform.position.x, 0.1f, nest.transform.position.z);
        rb.velocity = new Vector3(antSpeed * 0.2f,0,0);
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
            case Constants.ANT_MOVEMENT_STATE_FOLLOW_FOOD_TRAIL:
                followFoodTrail();
            break;
        }
        
        capVelocity();
        RotateTowardsTarget();
    }

    private int determineMovementState() {
        switch (antMovementState) {
            //if in state found food, check if need to go back to normal
            case Constants.ANT_MOVEMENT_STATE_FOUND_FOOD:
                if ((nest.transform.position - transform.position).magnitude < deliverFoodRadius) {
                    // GetComponent<Renderer>().material.color = Color.green;

                    //reset the currentTrail
                    // oldTrail = currentTrail;
                    oldTrail = new List<Vector3>();
                    foreach (Vector3 position in currentTrail) {
                        oldTrail.Add(position);
                    }

                    currentTrail = new List<Vector3>();
                    currentTrail.Add(nest.transform.position);
                    // currentTrail = resetTrail;

                    currentTrailStepIndex = 1;

                    // Debug.Log(oldTrail[oldTrail.Count -1]);
                    return Constants.ANT_MOVEMENT_STATE_FOLLOW_FOOD_TRAIL;
                }
            break;
            //else if in sees food state, check if need to switch to found food
            case Constants.ANT_MOVEMENT_STATE_SEES_FOOD:
            if ((foodFoundGo.transform.position - transform.position).magnitude < foundFoodRadius) {
                    // GetComponent<Renderer>().material.color = Color.magenta;
                    currentTrail.Add(foodFoundGo.transform.position);
                    //Since we add food position, go to step before food position
                    currentTrailStepIndex = currentTrail.Count - 2;
                    // Debug.Log(currentTrail.Count);
                    return Constants.ANT_MOVEMENT_STATE_FOUND_FOOD;
                }
                break;
            case Constants.ANT_MOVEMENT_STATE_FOLLOW_FOOD_TRAIL:
                if ((foodFoundGo.transform.position - transform.position).magnitude < foundFoodRadius) {
                    // GetComponent<Renderer>().material.color = Color.blue;
                    currentTrail.Add(foodFoundGo.transform.position);
                    //Since we add food position, go to step before food position
                    currentTrailStepIndex = currentTrail.Count - 2;
                    // Debug.Log(currentTrail.Count);
                    return Constants.ANT_MOVEMENT_STATE_FOUND_FOOD;
                }
            break;
            //else if in normal (random movement) state, check if ant can see food
            case Constants.ANT_MOVEMENT_STATE_NORMAL:
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, foodAttractionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.CompareTag("food")) {
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

    public void RotateTowardsTarget() {

        var rotation = Quaternion.LookRotation(rb.velocity);
        rotation *= Quaternion.Euler(-90, 0, -90); // this adds a rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
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
        Vector3 attractedToFoodVector = foodFoundGo.transform.position - transform.position;
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

    //Follow trail back to nest
    public void foundFood() {
        // Debug.Log(currentTrailStepIndex);

        //Unknown bug why it ever hits this
        if (currentTrailStepIndex == -1 ) {
            return;
        }

        Vector3 nextStep = currentTrail[currentTrailStepIndex];
        Vector3 goToNextStepVector = nextStep - transform.position;

        //If at the next step, remove from array and continue along trail
        //Note checking for distance being half ant speed prevents bug where ant moves back and forth
        //over intended point
        if (goToNextStepVector.magnitude <= deliverFoodRadius) {
            currentTrailStepIndex--;
            return;
        } else {
            goToNextStepVector.Normalize();
            rb.velocity = goToNextStepVector * antSpeed;
        }
    }

    //Follow trail back to food
    public void followFoodTrail() {

        //Unknown bug why it ever hits this
        if (currentTrailStepIndex == oldTrail.Count) {
            return;
        }

        // Debug.Log(currentTrailStepIndex);
        //             Debug.Log(oldTrail.Count);

        Vector3 nextStep = oldTrail[currentTrailStepIndex];
        Vector3 goToNextStepVector = nextStep - transform.position;

        //If at the next step, remove from array and continue along trail
        //Note checking for distance being half ant speed prevents bug where ant moves back and forth
        //over intended point
        if (goToNextStepVector.magnitude <= foundFoodRadius) {
            currentTrailStepIndex++;
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
