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
        currentVel = minVel + (playerMovement.currentSpeed * (maxVel - minVel)) / (playerMovement.runSpeed);
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
    }
    public void Spawn(bool sphere)
    {
        // Set the mesh and collider based on the bomb type.
        if (sphere)
        {
            Mesh.mesh = SphereBomb;
            sphereCollider.enabled = false;
            boxCollider.enabled = false;
        }
        else
        {
            Mesh.mesh = SquareBomb;
            sphereCollider.enabled = false;
            boxCollider.enabled = true;
        }

        // Activate the bomb and position it above the player.
        gameObject.SetActive(true);
        transform.parent = BombPos;
        transform.position = BombPos.position;
        transform.eulerAngles = Vector3.zero;
        currentState = 1;
    }

    public void Throw()
    {
        sphereCollider.enabled = true;
        transform.parent = null;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(((Player.transform.forward  * throwForce) + (Player.transform.up * throwHeight)) * currentVel, ForceMode.Impulse);
        currentState = 2;
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
                isExploding = false;
                explosionZone.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                gameObject.SetActive(false);
                meshRenderer.enabled = true;
                currentState = 0;
                rb.constraints = RigidbodyConstraints.FreezeAll;
            }

        }
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
