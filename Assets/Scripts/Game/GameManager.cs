using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int mainMenuSceneIndex = 0;
    
    public bool _isGamePaused {get; private set; }
    
    
    private UIDocument _document;

    private VisualElement _pauseContainer;

    private Button _pauseBtn;
    private Button _resumeBtn;
    private Button _mainMenuBtn;
    private Button _exitBtn;
    
    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        
        QueryElements();
    }

    private void OnEnable()
    {
        Subscribe();
    }

    private void OnDisable()
    {
        if (_document == null || _document.rootVisualElement == null) return;
        Unsubscribe();
    }
    
    public bool ToggleGamePauseState()
    {
        _isGamePaused = !_isGamePaused;
        if (_isGamePaused)
        {
            _pauseContainer.style.display = DisplayStyle.Flex;
            Time.timeScale = 0f;
        }
        else
        {
            _pauseContainer.style.display = DisplayStyle.None;
            Time.timeScale = 1f;
        }
        return _isGamePaused;
    }

    private void QueryElements()
    {
        var root = _document.rootVisualElement;

        _pauseContainer = root.Q<VisualElement>("PauseContainer");

        _resumeBtn = root.Q<Button>("ResumeBtn");
        _mainMenuBtn = root.Q<Button>("MainMenuButton");
        _exitBtn = root.Q<Button>("ExitGameButton");
        _pauseBtn = root.Q<Button>("StopBtn");
    }

    #region Subscibtion & Events

    private void Subscribe()
    {
        _resumeBtn?.RegisterCallback<ClickEvent>(OnResumeClick);
        _mainMenuBtn?.RegisterCallback<ClickEvent>(OnMainMenuClick);
        _exitBtn?.RegisterCallback<ClickEvent>(OnExitClick);
        _pauseBtn?.RegisterCallback<ClickEvent>(OnPauseClick);
        
        _document.rootVisualElement.RegisterCallback<ClickEvent>(OnGlobalClick);
    }

    private void Unsubscribe()
    {
        _resumeBtn?.UnregisterCallback<ClickEvent>(OnResumeClick);
        _mainMenuBtn?.UnregisterCallback<ClickEvent>(OnMainMenuClick);
        _exitBtn?.UnregisterCallback<ClickEvent>(OnExitClick);
        _pauseBtn?.UnregisterCallback<ClickEvent>(OnPauseClick);
        
        _document.rootVisualElement.UnregisterCallback<ClickEvent>(OnGlobalClick);
    }

    private void OnGlobalClick(ClickEvent evt)
    {
        if(evt.target is Button btn) OnGlobalButtonClick(evt);
    }

    private void OnGlobalButtonClick(ClickEvent evt)
    {
#if UNITY_EDITOR
        Debug.Log("Global Button Clik");
#endif
    }

    private void OnResumeClick(ClickEvent evt)
    {
#if UNITY_EDITOR
        Debug.Log("ResumeClicked");
#endif
        ToggleGamePauseState();
    }

    private void OnMainMenuClick(ClickEvent evt)
    {
#if UNITY_EDITOR
        Debug.Log("Main Menu button");
#endif
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    private void OnExitClick(ClickEvent evt)
    {
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnPauseClick(ClickEvent evt)
    {
        ToggleGamePauseState();
    }
    #endregion
    
    
}
