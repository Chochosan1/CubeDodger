using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IInteractable
{
    enum EnemyAttack { Push, Explode }
    [Header("Properties")]
    [Tooltip("Push type will push the player to the side with a good chance for the player to recover from the hit. Explode will most likely throw the player off the platform.")]
    [SerializeField] private EnemyAttack enemyAttack;
    [Tooltip("How hard should the enemy push the player off the tiles?")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float explodeForce = 500f;
    [Tooltip("The push force will get scaled up based on how fast the enemy is. Higher value equals harder scaling.")]
    [SerializeField] private float pushForceMultiplier = 0.2f;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 6f;
    private float currentSpeed;
    private Transform thisTransform;

    private void Start()
    {
        thisTransform = transform;
    }

    public void AffectPlayer(PlayerController pc)
    {
        switch (enemyAttack)
        {
            case EnemyAttack.Explode:
                pc.ExplodePlayer(thisTransform.position, explodeForce);
                break;
            default:
                pc.PushPlayer(thisTransform.forward, pushForce);
                break;
        }

    }

    private void Update()
    {
        thisTransform.position = Vector3.MoveTowards(thisTransform.position, thisTransform.position + thisTransform.forward, Time.deltaTime * currentSpeed);
    }

    private void OnEnable()
    {
        CalculateSpeedAndPushForce();
    }

    public void CalculateSpeedAndPushForce()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        pushForce += currentSpeed * 0.2f;
    }
}
