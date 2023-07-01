using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public RuntimeAnimatorController walk;
    public RuntimeAnimatorController idle;
    public Rigidbody rb;
    public Animator anim;
    private Transform cameraRotation;
    public Transform Hips, Root;
    
    [Header("Movement")]
    public float currentSpeed;
    public float moveSpeed;
    public float runSpeed;
    public bool isRagdoll = false;
    public bool canRotate = true;
    public bool canRun = true;
    private float turnSmoothTime = 0.1f;
    private float rotationSpeed;

    [Header("Jump")]
    public float jumpHeight;
    public float jumpDelay;
    public bool canJump = true;

    [Header("World")]
    public CurrentWorld World;

    public enum CurrentWorld
    {
        botw
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetRagdoll(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cameraRotation = Camera.main.transform;

    }

    private void ChangeVelocity()
    {
        // If the player is walking, check to see if they are holding shift to run.
        if (anim.runtimeAnimatorController != walk)
            return;
        
        // If the player is holding shift, set their speed to runSpeed, otherwise set it to moveSpeed.
        if(Input.GetKey(KeyCode.LeftShift) && canRun)
        {
            currentSpeed = Mathf.Clamp(currentSpeed, 0, runSpeed);
            anim.SetBool("isRunning", true);
        }
        else
        {
            currentSpeed = Mathf.Clamp(currentSpeed, 0, moveSpeed);
            anim.SetBool("isRunning", false);
        }
    }

    private void Movement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(x, 0, z).normalized;
        if (direction.magnitude >= 0.1f && IsGrounded() && !isRagdoll)
        {
            // Gradually increase the player's speed
            currentSpeed += Time.deltaTime * 3;

            // Calculate the angle the player should be facing
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraRotation.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, turnSmoothTime);

            // Rotate the player
            if(canRotate)
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calculate the direction the player should be moving
            Vector3 moveDir = Quaternion.Euler(0f, cameraRotation.eulerAngles.y, 0f) * direction;

            // Move the player
            rb.velocity = new Vector3(moveDir.x * currentSpeed, rb.velocity.y, moveDir.z * currentSpeed);
        }
        else if (IsGrounded())
        {
            currentSpeed = 0;
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }

    }

    public bool IsGrounded()
    {
        //Check if player is grounded
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }

    //Jump delay function
    private void JumpDelay()
    {
        if (jumpDelay > 0 && IsGrounded()) 
            jumpDelay -= Time.deltaTime;
    }

    private void Jump()
    {
        JumpDelay();
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded() && jumpDelay <= 0 && !isRagdoll && canJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
            Debug.Log(anim.runtimeAnimatorController.name);
            anim.Play("Jump", 0);
            anim.Play("Jump", 1);

            jumpDelay = 1f;
        }
    }



    IEnumerator DisableRagdoll()
    {
        // Wait for the character to be grounded
        yield return new WaitForSeconds(3);
        if(IsGrounded())
        {
            // Move the character to the Hips's position
            this.transform.position = Hips.transform.position;
            SetRagdoll(false);
        }
        // Keep trying to disable the ragdoll
        else
            StartCoroutine(DisableRagdoll());
    }

    public void SetRagdoll(bool areEnable)
    {
        //Activate all the colliders in player children objects, avoid to get the player collider
        Collider[] colliders = Root.GetComponentsInChildren<Collider>(includeInactive: true);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject)
              collider.enabled = areEnable;
        }
        //Enable or disable the gravity
        rb.useGravity = !areEnable;
        //Enable or disable the animation
        anim.enabled = !areEnable;
        //Set the ragdoll state
        isRagdoll = areEnable;
        //Disable the capsule collider of the player
        GetComponent<CapsuleCollider>().enabled = !areEnable;
        //Start the coroutine to disable the ragdoll
        if(areEnable)
            StartCoroutine(DisableRagdoll());
    }

    public void StopHorizontalMovement()
    {
        currentSpeed = 0;
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
    }


    // Update is called once per frame
    void Update()
    {

        ChangeVelocity();
        Movement();
        Jump();

        anim.SetBool("isGrounded", IsGrounded());


    }
}

