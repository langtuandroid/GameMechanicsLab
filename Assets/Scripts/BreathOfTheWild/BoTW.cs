using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class BoTW : MonoBehaviour
{
    
    public GameObject Target;
    public CinemachineFreeLook cinemachine;
    public GameObject Aim;
    public Transform Hand;

    public Bomb bomb;
    public GameObject movingObj;
    public float magnetSpeed = 1;

    private bool isHoldingBomb = false;
    private bool isHoldingArm = false;

    private Rigidbody rb;
    private Animator anim;
    private Vector2 previousMousePosition;
    public int magnetState = 0;

    public float minDistance,maxDistance = 10f;
    private PlayerMovement playerMovement;


    public float magnetDistance;   
    private int screenHeight, screenWidth;

    private float iDelay, kDelay;

    private LineRenderer line;

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
            //Seleccionar objeto
            switch(magnetState)
            {
                case 0:
                    isHoldingArm = !isHoldingArm;
                    anim.SetBool("isHoldingArm", isHoldingArm);
                    cinemachine.LookAt = Target.transform;
                    magnetState = 1;
                    Aim.SetActive(true);
                    break;
                case 1:
                    Vector3 centerOfScreen = new Vector3(screenWidth / 2f, screenHeight / 2f, 0f);
                    Ray ray = Camera.main.ScreenPointToRay(centerOfScreen);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {           
                        if(hit.collider.gameObject.GetComponent<WorldObject>() && Vector3.Distance(transform.position, hit.collider.transform.position) > minDistance) 
                        {
                            Aim.SetActive(false);
                            Debug.Log(Vector3.Distance(transform.position, hit.collider.transform.position));
                            line.enabled = true;
                            movingObj = hit.collider.gameObject;
                            playerMovement.canRotate = false;
                            movingObj.GetComponent<Rigidbody>().useGravity = false;
                            movingObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                            magnetState = 2;
                        }        

                    }
                    break;
                case 2:
                    line.enabled = false;
                    movingObj.GetComponent<Rigidbody>().useGravity = true;
                    movingObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    movingObj = null;
                    playerMovement.canRotate = true;
                    anim.SetBool("isHoldingArm", false);
                    isHoldingArm = false;
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
            line.SetPosition(0, Hand.transform.position);
            line.SetPosition(1, movingObj.transform.position);
        }
    }

    private void MagnetMovement()
    {
        if(movingObj != null)
        {

            Vector3 lookPos = new Vector3(movingObj.transform.position.x, transform.position.y, movingObj.transform.position.z);
            transform.LookAt(lookPos);
            
            Vector3 dir = Camera.main.ScreenToWorldPoint(new Vector3(screenWidth / 2f, screenHeight / 2f, 10f)) - movingObj.transform.position;

            Vector3 perpendicularDir = dir - Vector3.Dot(dir, transform.forward) * transform.forward;

            if(perpendicularDir.magnitude < 0.1f)
            {
                perpendicularDir = Vector3.zero;
            }

            Rigidbody movingObjRb = movingObj.GetComponent<Rigidbody>();

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

            Debug.Log(horizontalDistance);
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
