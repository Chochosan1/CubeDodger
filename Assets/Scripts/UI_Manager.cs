using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        [SerializeField] private TextMeshProUGUI notificationDescriptionText;
        [SerializeField] private TextMeshProUGUI notifcationValueText;

        private void Awake()
        {
            Chochosan.EventManager.OnRequiresNotification += ShowNotification;
        }

        private void OnDisable()
        {
            Chochosan.EventManager.OnRequiresNotification -= ShowNotification;
        }

        private void OpenMainMenu()
        {
            mainMenu.SetActive(true);
        }

        //called by a button
        public void RespawnPlayer()
        {
            PlayerController.Instance.Respawn();
            Time.timeScale = 1f;
            mainMenu.SetActive(false);
            notificationPanel.SetActive(false);
        }

        //called by a button
        public void ReloadScene(int sceneToReload)
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToReload);
        }

        private void ShowNotification(NotificationType notificationType, float value)
        {
            switch(notificationType)
            {
                case NotificationType.NewHighscore:
                    notificationDescriptionText.text = "New Highscore!";
                    notifcationValueText.text = value.ToString("F0");
                    break;
                case NotificationType.GameLost:
                    notificationDescriptionText.text = "You were shattered!";
                    notifcationValueText.text = value.ToString("F0");
                    break;
            }
            notificationPanel.SetActive(true);
        //    StartCoroutine(DisableGameObjectAfter(notificationPanel, 2f));
        }

        private IEnumerator DisableGameObjectAfter(GameObject objectToDisable, float duration)
        {
            yield return new WaitForSeconds(duration);
            objectToDisable.SetActive(false);
        }
    }
}
