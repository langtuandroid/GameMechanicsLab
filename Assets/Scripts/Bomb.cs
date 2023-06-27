using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject Player;
    public PhysicMaterial physicMaterial;
    public GameObject explosionZone;
    private Rigidbody rb;
    private PlayerMovement playerMovement;

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
    }
    public void Spawn()
    {
        gameObject.SetActive(true);
        transform.parent = Player.transform;
        transform.position = Player.transform.position + Player.transform.up * 2.6f;
        currentState = 1;
    }

    public void Throw()
    {
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
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        //Add a force to any objects within the explosion radius.
        foreach (Collider nearbyObject in colliders)
        {
            if (nearbyObject.gameObject == Player)
            {
                playerMovement.SetRagdoll(true);
                //add explosion force to the ragdoll
                Rigidbody[] ragdollRbs = Player.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in ragdollRbs)
                {
                    rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }

                return;
            }
            

            Rigidbody colRb = nearbyObject.GetComponent<Rigidbody>();
            if (colRb != null && colRb != rb)
            {
                colRb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
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
}
