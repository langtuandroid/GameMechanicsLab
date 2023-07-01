using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject Player;
    public PhysicMaterial physicMaterial;
    public GameObject explosionZone;
    public Mesh SphereBomb, SquareBomb;
    public MeshFilter Mesh;
    
    public Transform BombPos;
    private Rigidbody rb;
    private PlayerMovement playerMovement;
    private BoxCollider boxCollider;
    private SphereCollider sphereCollider;
    private MeshRenderer meshRenderer;
    private BoTW boTW;
    public int currentState = 0;
    public float throwForce = 10;
    public float throwHeight = 3;
    public float explosionRadius;
    public float explosionForce;
    public bool isExploding = false;

    public float friction = 0.6f;

    public float maxVel, minVel, currentVel;

    private void FixedUpdate()
    {
        if(transform.parent != BombPos)
            boTW.anim.SetBool("isHoldingBomb", false);
    }
    private void Start()
    {
        playerMovement = Player.GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        meshRenderer = GetComponent<MeshRenderer>();
        physicMaterial.staticFriction = friction;
        physicMaterial.dynamicFriction = friction;
        boxCollider = GetComponent<BoxCollider>();
        sphereCollider = GetComponent<SphereCollider>();
        boTW = Player.GetComponent<BoTW>();
    }

    public void HideBomb()
    {
        isExploding = false;
        explosionZone.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        gameObject.SetActive(false);
        meshRenderer.enabled = true;
        sphereCollider.enabled = false;
        boxCollider.enabled = false;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        currentState = 0;
    }

    
    public void Update()
    {
        if(isExploding)
        {
            //Gradually expand the explosion zone and collider until they reach the explosion radius.
            explosionZone.transform.localScale = Vector3.Lerp(explosionZone.transform.localScale, new Vector3(explosionRadius, explosionRadius, explosionRadius), 0.1f);

            //If the explosion zone is close enough to the explosion radius, hide the bomb.
            if (explosionZone.transform.localScale.x >= explosionRadius - 0.1f)
            {
                HideBomb();
            }

        }
    }
    public void Spawn(bool sphere)
    {
        // Set the mesh based on the bomb type.
        if (sphere)
            Mesh.mesh = SphereBomb;
        else
            Mesh.mesh = SquareBomb;
        // Activate the bomb and position it above the player.
        gameObject.SetActive(true);
        transform.parent = BombPos;
        transform.position = BombPos.position;
        transform.eulerAngles = Vector3.zero;
        currentState = 1;

        //Avoid the player from run and jump
        playerMovement.canRun = false;
        playerMovement.canJump = false;
    }

    public void Throw()
    {
        //Allow player to run and jump again
        playerMovement.canRun = true;
        playerMovement.canJump = true;

        transform.parent = null;
        rb.constraints = RigidbodyConstraints.None;
        rb.velocity += playerMovement.GetComponent<Rigidbody>().velocity;
        rb.AddForce((Player.transform.forward  * throwForce) + (Player.transform.up * throwHeight), ForceMode.Impulse);

        if(Mesh.mesh == SphereBomb)
            sphereCollider.enabled = true;
        else
            boxCollider.enabled = true;

        currentState = 2;
    }


    public void Explode()
    {
        meshRenderer.enabled = false;
        isExploding = true;

        //Cast a sphere to check for any objects within the explosion radius.
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius / 3.5f);

        //Add a force to any objects within the explosion radius.
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.gameObject == Player)
            {
                playerMovement.SetRagdoll(true);
            }
            

            Rigidbody colRb = nearbyObject.GetComponent<Rigidbody>();
            if (colRb != null && colRb != rb && colRb.gameObject != Player)
            {
                Debug.Log(colRb.name);
                colRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                //If the object has a WorldObject script, call the TakeDamage function.
                WorldObject worldObject = nearbyObject.GetComponent<WorldObject>();
                if (worldObject != null)
                {
                    worldObject.TakeDamage(5, this.gameObject);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.name);

        if (other.GetComponent<Rigidbody>() != null)
        {
            other.GetComponent<Rigidbody>().AddExplosionForce(explosionForce, transform.position, explosionRadius);
        }

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius / 4);
    }
}
