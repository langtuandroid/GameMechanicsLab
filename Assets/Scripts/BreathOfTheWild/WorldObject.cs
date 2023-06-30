using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldObject : MonoBehaviour
{
    [Header("References")]
    public Material yellowMat;

    public GameObject BrokenPrefab;
    private MeshRenderer mesh;
    private Material normalMat;
    private BoTW boTW;

    [Header("Settings")]
    public float damageToBreak;
    public float currentDamage;

    public bool isMetal;
    void Start()
    {
        mesh = GetComponent<MeshRenderer>();
        boTW = GameObject.FindGameObjectWithTag("Player").GetComponent<BoTW>();
        normalMat = mesh.material;
    }

    // Update is called once per frame
    void Update()
    {
        if(boTW.magnetState != 0)
            mesh.material = yellowMat;
        else{
            mesh.material = normalMat;
        }
    }

    public void TakeDamage(float damage, GameObject hitter)
    {
        currentDamage += damage;

        if(currentDamage >= damageToBreak)
        {
            damageToBreak = 0;
            GameObject instantiateBrokenPref = Instantiate(BrokenPrefab, transform.position, transform.rotation);
            //if hitter contains bomb script, add explosion force to all rigidbodies in broken prefab
            if (hitter.GetComponent<Bomb>())
            {
                Rigidbody[] rigidbodies = instantiateBrokenPref.GetComponentsInChildren<Rigidbody>();
                foreach (Rigidbody rb in rigidbodies)
                {
                    rb.AddExplosionForce(hitter.GetComponent<Bomb>().explosionForce, hitter.transform.position, hitter.GetComponent<Bomb>().explosionRadius);
                }
            }
            gameObject.SetActive(false);
        }
    }
}
