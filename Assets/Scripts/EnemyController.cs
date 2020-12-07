using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour, IInteractable
{
    enum EnemyAttack { Push, Explode }
    [Header("Properties")]
    [Tooltip("The player will not be affected by enemy projectiles that have the same color type as the current player color type. E.g. if the player is red and hits a red projectile then the player should not fly off the platform.")]
    [SerializeField] private PlayerController.CubeColor enemyColor;
    [Tooltip("Push type will push the player to the side with a good chance for the player to recover from the hit. Explode will most likely throw the player off the platform.")]
    [SerializeField] private EnemyAttack enemyAttack;
    [Tooltip("How hard should the enemy push the player off the tiles?")]
    [SerializeField] private float pushForce = 5f;
    [SerializeField] private float explodeForce = 500f;
    [Tooltip("The push force will get scaled up based on how fast the enemy is. Higher value equals harder scaling.")]
    [SerializeField] private float pushForceMultiplier = 0.2f;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 6f;
    [SerializeField] private float minPointsWhenAbsorbed = 10f;
    [SerializeField] private float maxPointsWhenAbsorbed = 17f;
    [Tooltip("When the player has reached the maximum multiplier, the points he gains from absorbing enemies will get multiplied by this value.")]
    [SerializeField] private float pointsMultiplierWhenMaxScoreMultiplierReached = 2.5f;
    [Tooltip("How long after hitting the player should the gameobject get disabled?")]
    [SerializeField] private float disableObjectAfter = 0.25f;

    [Header("References")]
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private GameObject mainParticle;

    private float currentSpeed;
    private Transform thisTransform;
    public bool isVisible = false;
    private bool isPlayerAlreadyAffected = false;

    private void Start()
    {
        thisTransform = transform;
        CalculateSpeedAndPushForce();
        hitParticle.SetActive(false);
        mainParticle.SetActive(true);
    }

    private void OnEnable()
    {
        Chochosan.EventManager.OnPlayerUsedContinueOption += DeactivateIfVisible;
    }

    private void OnDisable()
    {
        Chochosan.EventManager.OnPlayerUsedContinueOption -= DeactivateIfVisible;
    }

    public void AffectPlayer(PlayerController pc, bool isCurrentlyMoving)
    {
        if (isPlayerAlreadyAffected)
            return;

        if (pc.GetPlayerCurrentColor() != enemyColor)
        {
            switch (enemyAttack)
            {
                case EnemyAttack.Explode:
                    pc.ExplodePlayer(thisTransform.position, explodeForce);
                    break;
                case EnemyAttack.Push:
                    if (isCurrentlyMoving)
                        pc.PushPlayer(thisTransform.forward, pushForce * 1.5f);
                    else
                        pc.PushPlayer(thisTransform.forward, pushForce);
                    break;
            }
        }
        else
        {
            if (pc.IsMaxMultiplierReached())
                pc.CurrentScore += Random.Range(minPointsWhenAbsorbed, maxPointsWhenAbsorbed) * pointsMultiplierWhenMaxScoreMultiplierReached;
            else
                pc.CurrentScore += Random.Range(minPointsWhenAbsorbed, maxPointsWhenAbsorbed);

            pc.IncreaseMultiplier();
        }

        isPlayerAlreadyAffected = true;
        hitParticle.SetActive(true);
        mainParticle.SetActive(false);
        StartCoroutine(DisableAfter(gameObject, disableObjectAfter));
    }

    private void OnBecameVisible()
    {
        isVisible = true;
    }

    private void OnBecameInvisible()
    {
        DeactivateIfVisible();
    }

    //first check if the object is visible in order to avoid instant deactivation when it spawns off-screen
    private void DeactivateIfVisible()
    {
        if (isVisible)
        {
            gameObject.SetActive(false);
            isVisible = false;
        }
    }

    public void Reset(Vector3 pos, Quaternion rot)
    {
        if (thisTransform == null)
            thisTransform = transform;

        isPlayerAlreadyAffected = false;
        hitParticle.SetActive(false);
        mainParticle.SetActive(true);
        thisTransform.position = pos;
        thisTransform.rotation = rot;
        isVisible = false;
        CalculateSpeedAndPushForce();
    }

    private void Update()
    {
        thisTransform.position = Vector3.MoveTowards(thisTransform.position, thisTransform.position + thisTransform.forward, Time.deltaTime * currentSpeed);
    }

    private void CalculateSpeedAndPushForce()
    {
        currentSpeed = Random.Range(minSpeed, maxSpeed);
        currentSpeed += SpawnerManager.Instance.GetCurrentSpeedBoost();

        pushForce += currentSpeed * 0.2f;      
    }

    private IEnumerator DisableAfter(GameObject objectToDisable, float duration)
    {
        yield return new WaitForSeconds(duration);
        objectToDisable.SetActive(false);
    }
}
