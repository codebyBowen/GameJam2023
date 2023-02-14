using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlusMusic
{
    public class PlusMusicSettings : MonoBehaviour
    {
        public Slider loadingBar;
        public TMP_Text loadingText;
        private bool hasLoaded;
        public Action<bool> SetPause;
        public bool isPauseMenu = true;
        private bool hideDebugText = false;
        public GameObject settingsRoot;
        public GameObject debugText;
        public bool HasLoaded { get => hasLoaded; set => hasLoaded = value; }

        private void Awake()
        {
            Debug.Log("PlusMusicSettings.Awake()");
        }

        private void Start()
        {
        
            Debug.Log("PlusMusicSettings.Start()");
        
            OpenSettings(true);
            if (PlusMusic_DJ.Instance == null) 
            { 
                Debug.LogError("There is no DJ in the scene!"); 
                return; 
            }

            PlusMusic_DJ.Instance.LoadingProgress += SetLoadingBarProgress;
            //SetMusicVolume(1.0f);
        }
        
        public void Mute(bool value)
        {
            PlusMusic_DJ.Instance.SetMute(value);
        }
        
        public void SetMusicVolume(float value)
        {
            Debug.Log("PlusMusicSettings.SetMusicVolume()");
            
            //value = Mathf.Pow(value, 2); //Volume bars feel better when they are in a logaritmic
            PlusMusic_DJ.Instance.SetVolume(value);
        }
        
        public void SetLoadingBarProgress(float value)
        {
            loadingBar.value = value;
            if (value < 1)
            {
                hasLoaded = false;
                loadingText.text = "Loading ...";
            }
            else
            {
                loadingText.text = "Done";
                hasLoaded = true;
            }
        }
        
        private void Update()
        {
            if (isPauseMenu && (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape)))
            {
                if (hasLoaded)
                {
                    OpenSettings(!settingsRoot.activeSelf);
                }
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {
                if (!hideDebugText)
                {
                    hideDebugText = true;
                    if (debugText != null) debugText.SetActive(false);
                }
                else
                {
                    hideDebugText = false;
                    if (debugText != null) debugText.SetActive(true);
                }
            }
        }
        public void Close()
        {
            if (hasLoaded && isPauseMenu)
            {
                OpenSettings(!settingsRoot.activeSelf);
            }
        }
        public void OpenSettings(bool isActive)
        {
            if (settingsRoot != null)
            {
                settingsRoot.SetActive(isActive);
                SetPause?.Invoke(isActive);
                if (PlusMusic_DJ.Instance != null)
                {
                    PlusMusic_DJ.Instance.SetLowPassFilter(isActive);
                }
            }
        }
    }

}
