using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerExploded : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float powerMin = 300f;
    [SerializeField] private float powerMax = 500f;
    private float power;

    void Start()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
            {
                power = Random.Range(powerMin, powerMax);
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
            }

        }
    }
}
