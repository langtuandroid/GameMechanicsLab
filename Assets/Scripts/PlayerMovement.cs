using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public RuntimeAnimatorController walk;
    public RuntimeAnimatorController idle;
    private Rigidbody rb;
    public Animator anim;
    private Transform cameraRotation;
    public Transform Hips, Root;
    
    [Header("Movement")]
    public float currentSpeed;
    public float moveSpeed;
    public float runSpeed;
    public bool isRagdoll = false;
    private float turnSmoothTime = 0.1f;
    private float rotationSpeed;

    [Header("Jump")]
    public float jumpHeight;


    [Header("World")]

    public CurrentWorld World;
    public enum CurrentWorld
    {
        botw
    }

    [Header("Botw")]
    public Bomb sphereBomb;

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
        if(Input.GetKey(KeyCode.LeftShift))
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
            // Set walk animator controller
            anim.runtimeAnimatorController = walk;

            // Gradually increase the player's speed
            currentSpeed += Time.deltaTime * 3;

            // Calculate the angle the player should be facing
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraRotation.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, turnSmoothTime);

            // Rotate the player
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Calculate the direction the player should be moving
            Vector3 moveDir = Quaternion.Euler(0f, cameraRotation.eulerAngles.y, 0f) * direction;

            // Move the player
            rb.velocity = new Vector3(moveDir.x * currentSpeed, rb.velocity.y, moveDir.z * currentSpeed);
        }
        else if (IsGrounded())
        {
            currentSpeed = 0;
            rb.velocity = Vector3.zero;
        }

    }

    private bool IsGrounded()
    {
        //Check if player is grounded
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }



    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
            anim.Play("Jump");
        }
    }

    private void BotwMechanics()
    {
        //Spawn bomb
        if(Input.GetKeyDown(KeyCode.Q) && !isRagdoll)
        {
            switch(sphereBomb.currentState)
            {
                case 0:
                    sphereBomb.Spawn(true);
                    break;
                case 1:
                    sphereBomb.Throw();
                    break;
                case 2:
                    sphereBomb.Explode();
                    break;
            }

        }

        if(Input.GetKeyDown(KeyCode.E) && !isRagdoll)
        {
            switch(sphereBomb.currentState)
            {
                case 0:
                    sphereBomb.Spawn(false);
                    break;
                case 1:
                    sphereBomb.Throw();
                    break;
                case 2:
                    sphereBomb.Explode();
                    break;
            }

        }
    }

    IEnumerator DisableRagdoll()
    {
        yield return new WaitForSeconds(3);
        if(IsGrounded())
        {
            this.transform.position = Hips.transform.position;
            SetRagdoll(false);
        }
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
        rb.useGravity = !areEnable;
        anim.enabled = !areEnable;
        isRagdoll = areEnable;
        GetComponent<CapsuleCollider>().enabled = !areEnable;
        if(areEnable)
            StartCoroutine(DisableRagdoll());
    }


    // Update is called once per frame
    void Update()
    {
        ChangeVelocity();
        Movement();
        Jump();

        anim.SetBool("isGrounded", IsGrounded());

        if(rb.velocity == Vector3.zero)
        {
            // Set idle animator controller
            anim.runtimeAnimatorController = idle;
        }

        if(World == CurrentWorld.botw)
        {
            BotwMechanics();
        }
    }
}
