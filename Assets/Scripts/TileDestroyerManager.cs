using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileDestroyerManager : MonoBehaviour
{
    public static TileDestroyerManager Instance;

    [SerializeField] private GameObject leftTile;
    [SerializeField] private GameObject rightTile;
    [SerializeField] private Material warningMaterial, normalMaterial;
    [SerializeField] private float toggleTileAfterSeconds;
    [SerializeField] private float minTileDestroyCooldown, maxTileDestroyCooldown;
    [SerializeField] private float initialMinTileDestroyCooldown, initialMaxTileDestroyCooldown;
    private float tileDestroyTimestamp;
    private int tileToDestroy;
    private float timeLeftToToggleTile;
    private Animator leftTileAnim, rightTileAnim;
    private MeshRenderer leftTileRend, rightTileRend;
    private bool isAnyTileDestroyed = false;
    private bool isTileDestroyPaused = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        //the cooldown should be not be lower than twice the toggleTile time
        if (minTileDestroyCooldown < toggleTileAfterSeconds * 2f)
        {
            minTileDestroyCooldown = toggleTileAfterSeconds * 2f + 1f;
        }
        else if (maxTileDestroyCooldown < toggleTileAfterSeconds * 2f)
        {
            maxTileDestroyCooldown = toggleTileAfterSeconds * 2f + 1f;
        }

        tileDestroyTimestamp = Time.time + Random.Range(initialMinTileDestroyCooldown, initialMaxTileDestroyCooldown);
        leftTileAnim = leftTile.GetComponent<Animator>();
        rightTileAnim = rightTile.GetComponent<Animator>();
        leftTileRend = leftTile.GetComponent<MeshRenderer>();
        rightTileRend = rightTile.GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        if (Time.time >= tileDestroyTimestamp && !isAnyTileDestroyed && !isTileDestroyPaused)
        {
            tileToDestroy = Random.Range(0, 2);
            tileDestroyTimestamp = Time.time + Random.Range(minTileDestroyCooldown, maxTileDestroyCooldown);

            StartCoroutine(ShowIndicatorAndDestroyTile());
        }
    }

    public void SetPauseTileDestruction(bool value)
    {
        isTileDestroyPaused = value;
    }

    public bool IsAnyTileDestroyed()
    {
        return isAnyTileDestroyed;
    }

    private IEnumerator ShowIndicatorAndDestroyTile()
    {
        warningMaterial.SetFloat("_ShakeUvSpeed", 0);
        timeLeftToToggleTile = toggleTileAfterSeconds;
        isAnyTileDestroyed = true;

        if (tileToDestroy == 0)
        {
            leftTileRend.material = warningMaterial;
        }
        else if (tileToDestroy == 1)
        {
            rightTileRend.material = warningMaterial;
        }

        float shakeMultiplier = 0;
        while (timeLeftToToggleTile > 0)
        {
            shakeMultiplier += 0.8f;
            warningMaterial.SetFloat("_ShakeUvSpeed", shakeMultiplier);
        //    Debug.Log(timeLeftToToggleTile);
            yield return new WaitForSeconds(1f);
            timeLeftToToggleTile--;
        }

        if (tileToDestroy == 0)
        {
            leftTileAnim.SetBool("isDestroyed", true);
        }
        else if (tileToDestroy == 1)
        {
            rightTileAnim.SetBool("isDestroyed", true);
        }


        while (timeLeftToToggleTile < toggleTileAfterSeconds)
        {
            yield return new WaitForSeconds(1f);
            timeLeftToToggleTile++;
        }

        if (tileToDestroy == 0)
        {
            leftTileAnim.SetBool("isDestroyed", false);
            leftTileRend.material = normalMaterial;
        }
        else if (tileToDestroy == 1)
        {
            rightTileAnim.SetBool("isDestroyed", false);
            rightTileRend.material = normalMaterial;
        }

        isAnyTileDestroyed = false;
        warningMaterial.SetFloat("_ShakeUvSpeed", 0);
    }
}
