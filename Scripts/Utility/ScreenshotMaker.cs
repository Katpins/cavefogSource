using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace caveFog
{

    public class ScreenshotMaker : MonoBehaviour
    {

        private string _filePath;
        private bool _screenshotFlag;
        private bool _screenshotTakenFlag;

        [SerializeField]
        private GameObject _screenshotPanel;

        [SerializeField]
        private TextMeshProUGUI _pathText;

        private Coroutine _takeScreenshotNote;

        private void Start()
        {
            _screenshotPanel.SetActive(false);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {

                TakeScreenshot();
                StopAllCoroutines();
                _screenshotPanel.SetActive(false);
                ScreenCapture.CaptureScreenshot(_filePath, 2);
                _takeScreenshotNote = StartCoroutine(CO_ShowScreenshotMesg());
            }

            // if (_screenshotTakenFlag)
            // {
            //     if (_takeScreenshotNote != null)
            //     {
            //         StopCoroutine(_takeScreenshotNote);
            //         _screenshotPanel.SetActive(false);
            //     }
            //     _screenshotTakenFlag = false;
            //     _takeScreenshotNote = StartCoroutine(CO_ShowScreenshotMesg());
            // }
        }

        private void OnPostRender()
        {
            if (_screenshotFlag)
            {
                _screenshotFlag = false;
                //_screenshotTakenFlag = true;
            }
        }

        private IEnumerator CO_ShowScreenshotMesg()
        {
            yield return null;
            //Debug.Log(_filePath);
            _pathText.text = string.Concat(Application.dataPath + "/", _filePath);
            _screenshotPanel.SetActive(true);

            yield return new WaitForSeconds(5f);
            _screenshotPanel.SetActive(false);

        }
        private void TakeScreenshot()
        {

            if (!System.IO.Directory.Exists(Application.dataPath + "/Screenshots")) System.IO.Directory.CreateDirectory(Application.dataPath + "/Screenshots");

            _filePath = "Screenshots/CaveFog" + System.DateTime.Now.ToLongTimeString() + ".png";
            if (Application.isEditor) _filePath = "CaveFog" + System.DateTime.Now.ToLongTimeString() + ".png";
            //_filePath = _filePath.Replace("/", "");
            _filePath = _filePath.Replace(" ", "");
            _filePath = _filePath.Replace(":", "");


            _screenshotFlag = true;
        }
    }
}


