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
    int countFrames = 0;
    Random random = new Random();
    private bool turnLeft = true;
    private bool currentlyTurning = false;
    private float randTurnMultiplier = 1;
    private int randTurnFrameCounterLimit = 1000;
    private int turnFrameCounter = 0;


    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = new Vector3(antSpeed * 0.2f,0,0);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentlyTurning) {
            keepTurning();
        } else {
            startNewTurn();
        }

        capVelocity();
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


    public float GetRandomNumber(float minimum, float maximum)
    {
        float rnd = (float)this.random.NextDouble();
        return ((maximum - minimum) * rnd) + minimum; 
    }
}
