using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Game_Manager : MonoBehaviour
{
    enum ButtonActionType { LoadNextLevel }

    [Header("Properties")]
    [SerializeField] private float fadeSpeed = 1f;



    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject playBtnText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject transitionPanel;

    private float currFadeAmount;
    private bool isFadeStarted = false;
    private int sceneToLoad;
    private ButtonActionType currentActionType;
    private Material playerMat;

    private void Start()
    {
        transitionPanel.SetActive(false);
        ResetShaderValues();
        coinsText.text = Chochosan.SaveLoadManager.savedGameData.gameData.coinsAmount.ToString();
        playerMat = player.GetComponentInChildren<Renderer>().material;
    }

    private void Update()
    {
        if(isFadeStarted)
        {
            currFadeAmount += Time.deltaTime * fadeSpeed;
            playerMat.SetFloat("_FadeAmount", currFadeAmount);

            if(currFadeAmount >= 0.65f)
            {
                DetermineAction(currentActionType);
            }
        }
    }

    public void GoToScene(int sceneIndex)
    {
        isFadeStarted = true;
        currentActionType = ButtonActionType.LoadNextLevel;
        sceneToLoad = sceneIndex;
    }

    private void DetermineAction(ButtonActionType buttonActionType)
    {
        switch (buttonActionType)
        {
            case ButtonActionType.LoadNextLevel:
                StartCoroutine(OpenTransitionThenLoad());
                break;
        }
    }

    private IEnumerator OpenTransitionThenLoad()
    {
        transitionPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(sceneToLoad);
    }

    [ContextMenu("Chochosan/ResetShaderValues")]
    public void ResetShaderValues()
    {
        player.GetComponentInChildren<Renderer>().material.SetFloat("_FadeAmount", -0.1f);
    }
}
