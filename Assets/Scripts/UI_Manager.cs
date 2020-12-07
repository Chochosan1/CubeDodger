using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Chochosan
{
    /// <summary>
    /// Handles the UI logic.
    /// </summary>
    public class UI_Manager : MonoBehaviour
    {
        public enum NotificationType { NewHighscore, GameLost }
        private NotificationType notificationType;

        [Header("References")]
        [SerializeField] private GameObject mainMenu;
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private GameObject continuePanelInfo;
        [SerializeField] private TextMeshProUGUI notificationDescriptionText;
        [SerializeField] private TextMeshProUGUI notifcationValueRewardsText;
        [SerializeField] private Button continueButton;

        private void Awake()
        {
            Chochosan.EventManager.OnRequiresNotification += ShowNotification;
            Chochosan.EventManager.OnPlayerUsedContinueOption += ContinueOptionChosenUI;
        }

        private void OnDisable()
        {
            Chochosan.EventManager.OnRequiresNotification -= ShowNotification;
            Chochosan.EventManager.OnPlayerUsedContinueOption -= ContinueOptionChosenUI;
        }

        private void OpenMainMenu()
        {
            mainMenu.SetActive(true);
        }

        private void ContinueOptionChosenUI()
        {
            mainMenu.SetActive(false);
            notificationPanel.SetActive(false);
       //     continuePanelInfo.SetActive(true);
        }

        //called by a button
        public void ReloadScene(int sceneToReload)
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToReload);
        }

        private void ShowNotification(NotificationType notificationType, string message)
        {
            switch(notificationType)
            {
                case NotificationType.NewHighscore:
                    notificationDescriptionText.text = "New Highscore!";
                    notifcationValueRewardsText.text = message;
                    break;
                case NotificationType.GameLost:
                    notificationDescriptionText.text = "You were shattered!";
                    notifcationValueRewardsText.text = message;
                    break;
            }
            notificationPanel.SetActive(true);

            if (PlayerController.Instance.IsPlayerAbleToRevive())
                continueButton.interactable = true;
            else
                continueButton.interactable = false;
        //    StartCoroutine(DisableGameObjectAfter(notificationPanel, 2f));
        }

        private IEnumerator DisableGameObjectAfter(GameObject objectToDisable, float duration)
        {
            yield return new WaitForSeconds(duration);
            objectToDisable.SetActive(false);
        }
    }
}
