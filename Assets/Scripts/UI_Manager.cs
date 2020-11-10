using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chochosan
{
    public class UI_Manager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject mainMenu;

        private void Awake()
        {
            Chochosan.EventManager.OnPlayerLost += OpenMainMenu;
        }

        private void OnDisable()
        {
            Chochosan.EventManager.OnPlayerLost -= OpenMainMenu;
        }

        private void OpenMainMenu()
        {
            mainMenu.SetActive(true);
        }

        public void RespawnPlayer()
        {
            PlayerController.Instance.Respawn();
            Time.timeScale = 1f;
            mainMenu.SetActive(false);
        }

        public void ReloadScene(int sceneToReload)
        {
            Time.timeScale = 1f;
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToReload);
        }
    }
}
