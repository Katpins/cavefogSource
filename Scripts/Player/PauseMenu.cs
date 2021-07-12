using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace caveFog
{
    public class PauseMenu : MonoBehaviour
    {

        public static bool GamePaused;

        [SerializeField]
        private GameObject _pauseMenu;

        private void Awake()
        {
            GamePaused = false;
            _pauseMenu.SetActive(false);

            Cursor.lockState = GamePaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = GamePaused;

        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GamePaused = !GamePaused;
                UpdatePause();

            }
        }

        public void Pause()
        {
            GamePaused = true;
            UpdatePause();
        }

        public void UnPause()
        {
            GamePaused = false;
            UpdatePause();
        }


        private void UpdatePause()
        {
            _pauseMenu.SetActive(GamePaused);

            Time.timeScale = GamePaused ? 0 : 1;

            Cursor.lockState = GamePaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = GamePaused;
        }

    }
}

