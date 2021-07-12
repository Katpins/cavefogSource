using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace caveFog
{
    public class SceneLoader : MonoBehaviour
    {
        public void LoadScene(string _name)
        {
            PauseMenu.GamePaused = false;
            Time.timeScale = 1;
            SceneManager.LoadScene(_name);
        }

        public void Quit()
        {
            if (!Application.isEditor) Application.Quit();
        }
    }

}
