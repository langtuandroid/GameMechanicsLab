using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class BoTW : MonoBehaviour
{
    [Header("References")]
    public GameObject Target;
    public CinemachineFreeLook cinemachine;
    public GameObject Aim;
    public Transform Hand;
    public Bomb bomb;
    public GameObject movingObj;
    private Rigidbody rb;
    private Animator anim;
    private PlayerMovement playerMovement;
    private LineRenderer line;

    [Header("Settings")]


    public int magnetState = 0;

    public float minDistance,maxDistance = 10f;
    public float magnetSpeed = 1;
    public float magnetDistance;   

    private bool isHoldingBomb = false;
    private bool isHoldingArm = false;
    private Vector2 previousMousePosition;
    private int screenHeight, screenWidth;

    private float iDelay, kDelay;


    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();
        screenHeight = Screen.height;
        screenWidth = Screen.width;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    
    private void AnimationUpdater()
    {
        // If player is not moving, switch to idle animation
        if(rb.velocity == Vector3.zero && anim.runtimeAnimatorController != playerMovement.idle)
        {
            // If player is holding a bomb, play idle animation for holding bomb
            if(anim.GetCurrentAnimatorStateInfo(1).IsName("Bomb"))
            {
                anim.runtimeAnimatorController = playerMovement.idle;
                anim.SetBool("isHoldingBomb", true);
                anim.Play("Bomb", 1, 50);
            }
            // If player is holding a bomb arm, play idle animation for holding bomb arm
            else if(anim.GetCurrentAnimatorStateInfo(1).IsName("HoldingArm"))
            {
                anim.runtimeAnimatorController = playerMovement.idle;
                anim.SetBool("isHoldingArm", true);
                anim.Play("HoldingArm", 1, 50);
            }
            // Otherwise, play normal idle animation
            else
                anim.runtimeAnimatorController = playerMovement.idle;
        }
        // If player is moving, switch to walking animation
        else if((rb.velocity.x > 0.1f || rb.velocity.x < -0.1f)&& (rb.velocity.z > 0.1f || rb.velocity.z < -0.1f) && anim.runtimeAnimatorController != playerMovement.walk)
        {  
            // If player is holding a bomb, play walking animation for holding bomb
            if(anim.GetCurrentAnimatorStateInfo(1).IsName("Bomb"))
            {
                anim.runtimeAnimatorController = playerMovement.walk;
                anim.SetBool("isHoldingBomb", true);
                anim.Play("Bomb", 1, 50);
            }
            // If player is holding a bomb arm, play walking animation for holding bomb arm
            else if(anim.GetCurrentAnimatorStateInfo(1).IsName("HoldingArm"))
            {
                anim.runtimeAnimatorController = playerMovement.walk;
                anim.SetBool("isHoldingArm", true);
                anim.Play("HoldingArm", 1, 50);
            }
            // Otherwise, play normal walking animation
            else
                anim.runtimeAnimatorController = playerMovement.walk;
        }
        //Reset the animation if the player is jumping and is grounded
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Jump") && playerMovement.IsGrounded())
        {
            anim.runtimeAnimatorController = anim.runtimeAnimatorController;
        }
    }
        private void BotwMechanics()
    {
        //Spawn sphere bomb
        if(Input.GetKeyDown(KeyCode.Q) && !playerMovement.isRagdoll && playerMovement.IsGrounded())
        {
            switch(bomb.currentState)
            {
                case 0:
                    bomb.Spawn(true);
                    anim.SetBool("isHoldingBomb", true);
                    isHoldingBomb = true;
                    break;
                case 1:
                    bomb.Throw();
                    anim.SetBool("isHoldingBomb", false);
                    isHoldingBomb = false;
                    break;
                case 2:
                    bomb.Explode();
                    break;
            }

        }
        //spawn square bomb
        if(Input.GetKeyDown(KeyCode.E) && !playerMovement.isRagdoll && playerMovement.IsGrounded())
        {
            switch(bomb.currentState)
            {
                case 0:
                    bomb.Spawn(false);
                    anim.SetBool("isHoldingBomb", true);
                    isHoldingBomb = true;
                    break;
                case 1:
                    bomb.Throw();
                    anim.SetBool("isHoldingBomb", false);
                    isHoldingBomb = false;
                    break;
                case 2:
                    bomb.Explode();
                    break;
            }

        }

        //Magnet
        if(Input.GetKeyDown(KeyCode.R))
        {
            switch(magnetState)
            {
                //Start the magnet
                case 0:
                    isHoldingArm = !isHoldingArm;
                    //Start animation
                    anim.SetBool("isHoldingArm", isHoldingArm);
                    //Change camera lookAt to selected object
                    cinemachine.LookAt = Target.transform;
                    magnetState = 1;
                    //Enable a crosshair
                    Aim.SetActive(true);
                    break;
                //Try to hook an object
                case 1:
                    //Calculate the center of the screen and cast a ray from there
                    Vector3 centerOfScreen = new Vector3(screenWidth / 2f, screenHeight / 2f, 0f);
                    Ray ray = Camera.main.ScreenPointToRay(centerOfScreen);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {  
                        //If the ray hits a world object, and the distance is greater than the minimum distance, prepare all the variables for move the object                             
                        if(hit.collider.gameObject.GetComponent<WorldObject>() && Vector3.Distance(transform.position, hit.collider.transform.position) > minDistance) 
                        {
                            //Disable crosshair
                            Aim.SetActive(false);
                            //Enable a line renderer between the player and the object
                            line.enabled = true;
                            //Set the collided object as the object to move
                            movingObj = hit.collider.gameObject;
                            //Avoid the player to rotate while moving the object
                            playerMovement.canRotate = false;
                            //Remove gravity and rotation from the object
                            movingObj.GetComponent<Rigidbody>().useGravity = false;
                            movingObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                            magnetState = 2;
                        }        

                    }
                    break;
                //Drop the object
                case 2:
                    //Disable line renderer
                    line.enabled = false;
                    //Return gravity and rotation to the object
                    movingObj.GetComponent<Rigidbody>().useGravity = true;
                    movingObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    movingObj = null;
                    //Allow the player to rotate again
                    playerMovement.canRotate = true;
                    //Stop animation
                    anim.SetBool("isHoldingArm", false);
                    isHoldingArm = false;
                    //Change camera lookAt to player again
                    cinemachine.LookAt = transform;
                    magnetState = 0;
                    break;
            }
        }
    }

    private void FixedUpdate() 
    {
        if(movingObj != null)
        {
            //Set the line renderer positions (Needs to be in FixedUpdate for prevent a weird position bug)
            line.SetPosition(0, Hand.transform.position);
            line.SetPosition(1, movingObj.transform.position);
        }
    }

    private void MagnetMovement()
    {
        if(movingObj != null)
        {
            //Look at the object
            Vector3 lookPos = new Vector3(movingObj.transform.position.x, transform.position.y, movingObj.transform.position.z);
            transform.LookAt(lookPos);
            
            //Calculate the direction from the object to the center of the screen
            Vector3 dir = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth / 2f, screenHeight / 2f, 10f)) - movingObj.transform.position;

            //Calculate the perpendicular direction from the object to the center of the screen (needs to be perpendicular to avoid the object to move in the direction of the camera)
            Vector3 perpendicularDir = dir - Vector3.Dot(dir, transform.forward) * transform.forward;

            //If the perpendicular direction is too small, set it to zero (To avoid shaking on low velocities)
            if(perpendicularDir.magnitude < 0.1f)
            {
                perpendicularDir = Vector3.zero;
            }

            Rigidbody movingObjRb = movingObj.GetComponent<Rigidbody>();

            //Move the object towards the center of the screen in the perpendicular directions
            movingObjRb.velocity = perpendicularDir.normalized * magnetSpeed;

            float distance = Vector3.Distance(movingObj.transform.position, transform.position);
            float horizontalDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(movingObj.transform.position.x, movingObj.transform.position.z));

            //limit the minimum and maximum distance the object can be from the player
            if(distance > maxDistance)
            {
                movingObjRb.velocity = Vector3.zero;
                movingObj.transform.position = transform.position + (movingObj.transform.position - transform.position).normalized * maxDistance;
            }
            else if (distance < minDistance)
            {
                movingObjRb.velocity = Vector3.zero;
                movingObj.transform.position = transform.position + (movingObj.transform.position - transform.position).normalized * minDistance;
            }

            //move the object towards the player if the key "I" is held
            if(Input.GetKey(KeyCode.I) && !Input.GetKey(KeyCode.K) && distance < maxDistance)
            {
                movingObjRb.velocity = (new Vector3(-dir.x, 0, -dir.z).normalized + perpendicularDir.normalized) * magnetSpeed;
            }
            else if(Input.GetKey(KeyCode.K) && !Input.GetKey(KeyCode.I) && horizontalDistance > minDistance)
            {
                movingObjRb.velocity = (new Vector3(dir.x, 0, dir.z).normalized + perpendicularDir.normalized) * magnetSpeed;
            }
            if(movingObjRb.velocity.magnitude < 0.1f)
                movingObjRb.velocity = Vector3.zero;

            //Update the system velocity
            movingObj.GetComponent<Rigidbody>().velocity += playerMovement.rb.velocity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        AnimationUpdater();
        BotwMechanics();
        MagnetMovement();
    }

}
