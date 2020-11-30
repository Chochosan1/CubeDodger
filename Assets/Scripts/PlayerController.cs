using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the movement, swiping and score logic. Attached to the player gameobject.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public enum CubeColor { Red, Blue, Yellow, Green }
    public static PlayerController Instance;

    private const float MIN_SPEED = 6f;

    [Header("Movement")]
    [SerializeField] private int cubesPerMove = 1;
    [Range(MIN_SPEED, 100)]
    [SerializeField] private float moveSpeed = MIN_SPEED;

    [Header("Colours")]
    [ColorUsage(true, true)]
    [SerializeField] private Color redColor;
    [ColorUsage(true, true)]
    [SerializeField] private Color blueColor;
    [ColorUsage(true, true)]
    [SerializeField] private Color greenColor;
    [ColorUsage(true, true)]
    [SerializeField] private Color yellowColor;
    [SerializeField] private CubeColor startColor;
    private CubeColor lastColor;
    private CubeColor currentColor;

    [Header("Camera")]
    [SerializeField] private Transform cameraGame;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeMagnitude = 0.5f;

    [Header("References")]
    [SerializeField] private Transform respawnTransform;
    [SerializeField] private GameObject moveParticle;
    [SerializeField] private TMPro.TextMeshProUGUI scoreText;
    [SerializeField] private TMPro.TextMeshProUGUI highscoreText;
    [SerializeField] private TMPro.TextMeshProUGUI multiplierText;
    [SerializeField] private UnityEngine.UI.Slider multiplierBar;
    private Animator multiplierTextAnim;
    [Tooltip("The part of the player prefab that consists of the visual part of the player (not the entire player prefab).")]
    [SerializeField] private GameObject playerCube;
    [SerializeField] private GameObject playerExplodedPrefab;
    private Material playerMat;

    [Header("Collision")]
    [SerializeField] private LayerMask layersToGetAffectedBy;

    [Header("Score counter")]
    [Tooltip("The starting score multiplier. It will increase over time while playing until an enemy hits the player.")]
    [SerializeField] private float scoreMultiplier = 1f;
    [Tooltip("The maximum multiplier should not exceed this value.")]
    [SerializeField] private float maxScoreMultiplier = 10f;
    [Tooltip("How many seconds should pass without getting hit in order to increase the score multiplier?")]
    [SerializeField] private float secondsToIncreaseMultiplier = 5f;
    [Tooltip("How much should the multiplier increase with?")]
    [SerializeField] private float multiplierIncreasePerTick = 1f;
    private float secondsToIncreaseMultiplierTimestamp;
    private bool isGameLost = false;
    private float currentScore = 0f;
    public float CurrentScore
    {
        get { return currentScore; }
        set { currentScore = value < 0 ? 0 : value; }
    }
    public float ScoreMultiplier
    {
        get { return scoreMultiplier; }
        set
        {
            scoreMultiplier = value > maxScoreMultiplier ? maxScoreMultiplier : value;
            multiplierText.text = $"x{scoreMultiplier}";
        }
    }


    private float highscore;

    private Transform thisTransform; //cache
    private Animator anim;
    private Rigidbody rb;
    private Vector3 targetPosition;
    private GameObject playerExplodedObject;

    //TOUCH
    private Vector2 firstPressPos;
    private Vector2 secondPressPos;
    private Vector2 currentSwipe;

    private bool canRollForward = false;
    private bool canRollBackwards = false;
    private bool isCurrentlyMoving = false;
    private bool isCameraShaking = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        Application.targetFrameRate = 60;
        if (Application.isMobilePlatform)
        {
            QualitySettings.vSyncCount = 0;
        }
    }

    void Start()
    {
        playerMat = GetComponentInChildren<Renderer>().material;
        //   defaultColor = playerMat.GetColor("_EmissionColor");

        currentColor = startColor;
        SwitchPlayerColour(currentColor);

     

        thisTransform = transform;
        anim = GetComponentInChildren<Animator>();
        multiplierTextAnim = multiplierText.gameObject.GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        ScoreMultiplier = scoreMultiplier;
        secondsToIncreaseMultiplierTimestamp = Time.time + secondsToIncreaseMultiplier;
        multiplierBar.maxValue = secondsToIncreaseMultiplier;
        multiplierBar.value = 0f;

        if (Chochosan.SaveLoadManager.IsSaveExists())
        {
            highscore = Chochosan.SaveLoadManager.savedGameData.gameData.highscore;
            Debug.Log("SAVE DETECTED");
        }

        highscoreText.text = highscore.ToString("F0");
    }

    void Update()
    {
        Swipe();

        if (isGameLost)
            return;

        currentScore += Time.deltaTime * ScoreMultiplier;
        scoreText.text = currentScore.ToString("F0");
        multiplierBar.value += Time.deltaTime;

        if (Time.time >= secondsToIncreaseMultiplierTimestamp)
        {
            IncreaseMultiplier();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Respawn();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (canRollForward || canRollBackwards)
                return;

            anim.SetBool("moveForward", true);
            if (moveSpeed < cubesPerMove * MIN_SPEED)
                moveSpeed = cubesPerMove * MIN_SPEED;

            targetPosition = thisTransform.position + new Vector3(0, 0, -cubesPerMove);
            StartCoroutine(RollForward());
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (canRollForward || canRollBackwards)
                return;

            anim.SetBool("moveBackwards", true);
            if (moveSpeed < cubesPerMove * MIN_SPEED)
                moveSpeed = cubesPerMove * MIN_SPEED;

            targetPosition = thisTransform.position + new Vector3(0, 0, cubesPerMove);
            StartCoroutine(RollBackwards());
        }

        if (canRollForward)
        {
            thisTransform.position = Vector3.MoveTowards(thisTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            isCurrentlyMoving = true;
        }
        else if (canRollBackwards)
        {
            thisTransform.position = Vector3.MoveTowards(thisTransform.position, targetPosition, moveSpeed * Time.deltaTime);
            isCurrentlyMoving = true;
        }
        else
        {
            isCurrentlyMoving = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isGameLost)
            return;

        if (layersToGetAffectedBy == (layersToGetAffectedBy | (1 << other.gameObject.layer)))
        {
            if (!isCameraShaking)
                StartCoroutine(CameraShake(shakeDuration, shakeMagnitude));

            if (other.gameObject.CompareTag("KillZone"))
            {
                LoseGame();
                return;
            }

            other.gameObject.GetComponent<IInteractable>().AffectPlayer(this, isCurrentlyMoving);
        }
    }

    /// <summary>
    /// Push the player in a desired direction with a desired force. Remove some of his score and reset the score multiplier.
    /// </summary>
    /// <param name="pushDir">Push direction.</param>
    /// <param name="force">Push force.</param>
    public void PushPlayer(Vector3 pushDir, float force)
    {
        rb.AddForce(pushDir * force, ForceMode.Impulse);
        CurrentScore -= force;
        ResetMultiplier();
    }

    public void ExplodePlayer(Vector3 position, float force)
    {
        rb.AddExplosionForce(force, position, 2f, 1f);
        ResetMultiplier();
    }

    private void ResetMultiplier()
    {
        ScoreMultiplier = 1f;
        multiplierTextAnim.SetBool("decreaseMultiplier", true);
        secondsToIncreaseMultiplierTimestamp = Time.time + secondsToIncreaseMultiplier;
        multiplierBar.maxValue = secondsToIncreaseMultiplier;
        multiplierBar.value = 0f;
    }

    public void IncreaseMultiplier()
    {
        ScoreMultiplier += multiplierIncreasePerTick;
        secondsToIncreaseMultiplierTimestamp = Time.time + secondsToIncreaseMultiplier;
        multiplierTextAnim.SetBool("increaseMultiplier", true);
        multiplierBar.maxValue = secondsToIncreaseMultiplier;
        multiplierBar.value = 0f;

        SwitchPlayerColourRandomly();
    }

    private void LoseGame()
    {
        if (isGameLost)
            return;

        Handheld.Vibrate();
        StartCoroutine(WaitBeforeStoppingGame());
    }

    private IEnumerator WaitBeforeStoppingGame()
    {
        isGameLost = true;
        playerCube.SetActive(false);
        playerExplodedObject = Instantiate(playerExplodedPrefab, thisTransform.position, thisTransform.rotation);
        yield return new WaitForSeconds(2f);
        if (CurrentScore > highscore)
        {
            highscore = CurrentScore;
            highscoreText.text = highscore.ToString("F0");

            Chochosan.EventManager.OnRequiresNotification?.Invoke(Chochosan.UI_Manager.NotificationType.NewHighscore, highscore);
        }
        else
        {
            Chochosan.EventManager.OnRequiresNotification?.Invoke(Chochosan.UI_Manager.NotificationType.GameLost, CurrentScore);
        }

        Chochosan.SaveLoadManager.SaveGameState();
        //   Time.timeScale = 0f;
    }

    private void SwitchPlayerColourRandomly()
    {
        currentColor = (CubeColor)Random.Range(0, 3);
        if (currentColor == lastColor)
        {
            currentColor++;
            Debug.Log("CAUGHT SAME COLOR");
        }
        switch (currentColor)
        {
            case CubeColor.Blue:
                playerMat.SetColor("_EmissionColor", blueColor);
                break;
            case CubeColor.Red:
                playerMat.SetColor("_EmissionColor", redColor);
                break;
            case CubeColor.Yellow:
                playerMat.SetColor("_EmissionColor", yellowColor);
                break;
            case CubeColor.Green:
                playerMat.SetColor("_EmissionColor", greenColor);
                break;
        }
        lastColor = currentColor;
    }

    private void SwitchPlayerColour(CubeColor currentColor)
    {
        switch (currentColor)
        {
            case CubeColor.Blue:
                playerMat.SetColor("_EmissionColor", blueColor);
                break;
            case CubeColor.Red:
                playerMat.SetColor("_EmissionColor", redColor);
                break;
            case CubeColor.Yellow:
                playerMat.SetColor("_EmissionColor", yellowColor);
                break;
            case CubeColor.Green:
                playerMat.SetColor("_EmissionColor", greenColor);
                break;
        }
        lastColor = currentColor;
    }

    public CubeColor GetPlayerCurrentColor()
    {
        return currentColor;
    }

    public void Respawn()
    {
        thisTransform.position = respawnTransform.position;
        thisTransform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        canRollForward = canRollBackwards = false;
        isGameLost = false;
        playerCube.SetActive(true);
        ScoreMultiplier = 1f;
        Destroy(playerExplodedObject);
    }

    private IEnumerator RollForward()
    {
        yield return new WaitForSeconds(0.05f);
        canRollForward = true;
        moveParticle.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        canRollForward = false;
        anim.SetBool("moveForward", false);
        moveParticle.SetActive(false);
    }

    private IEnumerator RollBackwards()
    {
        yield return new WaitForSeconds(0.05f);
        canRollBackwards = true;
        moveParticle.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        canRollBackwards = false;
        anim.SetBool("moveBackwards", false);
        moveParticle.SetActive(false);
    }

    public void StopRolls()
    {
        canRollForward = false;
        anim.SetBool("moveForward", false);

        canRollBackwards = false;
        anim.SetBool("moveBackwards", false);
    }

    public GameData GetGameDataToSave()
    {
        GameData gameData = new GameData();
        gameData.highscore = highscore;

        return gameData;
    }

    private IEnumerator CameraShake(float duration, float magnitude)
    {
        isCameraShaking = true;
        Vector3 originalPos = cameraGame.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(cameraGame.position.x - magnitude, cameraGame.position.x + magnitude);
            float y = Random.Range(cameraGame.position.y - magnitude, cameraGame.position.y + magnitude);

            cameraGame.position = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;

            yield return null;
        }

        cameraGame.position = originalPos;

        isCameraShaking = false;
    }

    #region MobileInput
    private void Swipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //save began touch 2d point
            firstPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        if (Input.GetMouseButtonUp(0))
        {
            //save ended touch 2d point
            secondPressPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            //create vector from the two points
            currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);

            //normalize the 2d vector
            currentSwipe.Normalize();

            //swipe upwards
            if (currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                if (canRollForward || canRollBackwards)
                    return;

                anim.SetBool("moveForward", true);
                if (moveSpeed < cubesPerMove * MIN_SPEED)
                    moveSpeed = cubesPerMove * MIN_SPEED;

                targetPosition = thisTransform.position + new Vector3(0, 0, -cubesPerMove);
                StartCoroutine(RollForward());
                //      Debug.Log("up swipe");
            }
            //swipe down
            if (currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                if (canRollForward || canRollBackwards)
                    return;

                anim.SetBool("moveBackwards", true);
                if (moveSpeed < cubesPerMove * MIN_SPEED)
                    moveSpeed = cubesPerMove * MIN_SPEED;

                targetPosition = thisTransform.position + new Vector3(0, 0, cubesPerMove);
                StartCoroutine(RollBackwards());
                //     Debug.Log("down swipe");
            }
            //swipe left
            if (currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                //    Debug.Log("left swipe");
                if (canRollForward || canRollBackwards)
                    return;

                anim.SetBool("moveBackwards", true);
                if (moveSpeed < cubesPerMove * MIN_SPEED)
                    moveSpeed = cubesPerMove * MIN_SPEED;

                targetPosition = thisTransform.position + new Vector3(0, 0, cubesPerMove);
                StartCoroutine(RollBackwards());
            }
            //swipe right
            if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                //    Debug.Log("right swipe");
                if (canRollForward || canRollBackwards)
                    return;

                anim.SetBool("moveForward", true);
                if (moveSpeed < cubesPerMove * MIN_SPEED)
                    moveSpeed = cubesPerMove * MIN_SPEED;

                targetPosition = thisTransform.position + new Vector3(0, 0, -cubesPerMove);
                StartCoroutine(RollForward());
            }
        }
    }
    #endregion
}
