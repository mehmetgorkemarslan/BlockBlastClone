using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Scene Management")] 
    [SerializeField]
    private int gameSceneIndex = 1;
    
    private UIDocument _document;

    private Button _startBtn;
    private Button _settingsBtn;
    private Button _quitBtn;

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        
        if(_document == null) return;
        
        QueryElements();
    }

    private void QueryElements()
    {
        VisualElement root = _document.rootVisualElement;

        _startBtn = root.Q<Button>("StartButton");
        _settingsBtn = root.Q<Button>("SettingsButton");
        _quitBtn = root.Q<Button>("QuitButton");
    }

    #region Events And Subscribtions
    private void SubscribeEvents()
    {
        _startBtn.RegisterCallback<ClickEvent>(OnStartGameClick);
        _settingsBtn.RegisterCallback<ClickEvent>(OnSettingsClick);
        _quitBtn.RegisterCallback<ClickEvent>(OnQuitClick);
            
        _document.rootVisualElement.RegisterCallback<ClickEvent>(OnAnyClickReceived);
    }

    private void UnsubscribeEvents()
    {
        _startBtn?.UnregisterCallback<ClickEvent>(OnStartGameClick);
        _settingsBtn?.UnregisterCallback<ClickEvent>(OnSettingsClick);
        _quitBtn?.UnregisterCallback<ClickEvent>(OnQuitClick);
        
        _document.rootVisualElement.UnregisterCallback<ClickEvent>(OnAnyClickReceived);
    }

    private void OnAnyClickReceived(ClickEvent evt)
    {
        if(evt.target is Button clickedButton) GlobalButtonEvent(evt);
    }
    
    private void GlobalButtonEvent(ClickEvent evt)
    {
        Debug.Log("This part is for all buttons");
    }

    private void OnStartGameClick(ClickEvent evt)
    {
        Debug.Log($"Game Starting... Game Scene Index: {gameSceneIndex}");
        SceneManager.LoadScene(gameSceneIndex);
    }

    private void OnSettingsClick(ClickEvent evt)
    {
        Debug.Log("Not implemented yet.");
    }

    private void OnQuitClick(ClickEvent evt)
    {
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion
    
    private void OnEnable()
    {
        SubscribeEvents();    
    }

    private void OnDisable()
    {
        if (_document == null || _document.rootVisualElement == null) return;
        UnsubscribeEvents();
    }
}
