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
    [SerializeField] private float fadeButtonSpeed = 1f;

    [Header("Buttons")]
    [SerializeField] private Image playBtnImage;
    [SerializeField] private GameObject playBtnText;
    [SerializeField] private TextMeshProUGUI coinsText;

    private float currFadeAmount;
    private bool isFadeStarted = false;
    private Image currentButtonClickedImage;
    private int sceneToLoad;
    private ButtonActionType currentActionType;

    private void Start()
    {
        ResetShaderValues();
        coinsText.text = Chochosan.SaveLoadManager.savedGameData.gameData.coinsAmount.ToString();
    }

    private void Update()
    {
        if(isFadeStarted)
        {
            currFadeAmount += Time.deltaTime * fadeButtonSpeed;
            currentButtonClickedImage.material.SetFloat("_FadeAmount", currFadeAmount);

            if(currFadeAmount >= 0.95f)
            {
                DetermineAction(currentActionType);
            }
        }
    }

    public void GoToScene(int sceneIndex)
    {
        playBtnText.SetActive(false);
        isFadeStarted = true;
        currentActionType = ButtonActionType.LoadNextLevel;
        currentButtonClickedImage = playBtnImage;
        sceneToLoad = sceneIndex;
    }

    private void DetermineAction(ButtonActionType buttonActionType)
    {
        switch (buttonActionType)
        {
            case ButtonActionType.LoadNextLevel:
                SceneManager.LoadScene(sceneToLoad);
                break;
        }
    }

    [ContextMenu("Chochosan/ResetShaderValues")]
    public void ResetShaderValues()
    {
        playBtnImage.material.SetFloat("_FadeAmount", -0.1f);
    }
}
