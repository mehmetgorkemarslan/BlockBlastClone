using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Dependencies")] 
    [SerializeField]
    private ThemeSettingsManager themeManager;
    [SerializeField]
    private int gameSceneIndex = 1;
    
    private UIDocument _document;

    private VisualElement _mainContainer;
    private VisualElement _settingsContainer;
    
    private Button _startBtn;
    private Button _settingsBtn;
    private Button _quitBtn;
    private Button _backBtn;

    #region Unity Built-in

    private void Awake()
    {
        _document = GetComponent<UIDocument>();
        
        if(_document == null) return;
        
        QueryElements();
    }
    
    private void OnEnable()
    {
        SubscribeEvents();

        ChanceMenuOrSettings(true);
    }

    private void OnDisable()
    {
        if (_document == null || _document.rootVisualElement == null) return;
        UnsubscribeEvents();
    }

    #endregion

    
    private void QueryElements()
    {
        VisualElement root = _document.rootVisualElement;
        
        _mainContainer = root.Q<VisualElement>("MainContainer");
        _settingsContainer = root.Q<VisualElement>("SettingsContainer");

        _startBtn = root.Q<Button>("StartButton");
        _settingsBtn = root.Q<Button>("SettingsButton");
        _quitBtn = root.Q<Button>("QuitButton");
        _backBtn = root.Q<Button>("BackBtn");
        
        if (themeManager != null)
        {
            themeManager.Initialize(_settingsContainer); 
        }
    }

    private void ChanceMenuOrSettings(bool menuActive)
    {
        if (menuActive)
        {
            _settingsContainer.style.display =  DisplayStyle.None;
            _mainContainer.style.display = DisplayStyle.Flex;    
        }
        else
        {
            _settingsContainer.style.display =  DisplayStyle.Flex;
            _mainContainer.style.display = DisplayStyle.None;  
        }
        
    }

    #region Events And Subscribtions
    private void SubscribeEvents()
    {
        _startBtn.RegisterCallback<ClickEvent>(OnStartGameClick);
        _settingsBtn.RegisterCallback<ClickEvent>(OnSettingsClick);
        _quitBtn.RegisterCallback<ClickEvent>(OnQuitClick);
        _backBtn?.RegisterCallback<ClickEvent>(OnBackClick);
    }

    private void UnsubscribeEvents()
    {
        _startBtn?.UnregisterCallback<ClickEvent>(OnStartGameClick);
        _settingsBtn?.UnregisterCallback<ClickEvent>(OnSettingsClick);
        _quitBtn?.UnregisterCallback<ClickEvent>(OnQuitClick);
        _backBtn?.UnregisterCallback<ClickEvent>(OnBackClick);
    }
    
    private void OnStartGameClick(ClickEvent evt)
    {
        Debug.Log($"Game Starting... Game Scene Index: {gameSceneIndex}");
        SceneManager.LoadScene(gameSceneIndex);
    }

    private void OnSettingsClick(ClickEvent evt)
    {
        ChanceMenuOrSettings(false);
    }
    
    private void OnBackClick(ClickEvent evt)
    {
        ChanceMenuOrSettings(true);
    }

    private void OnQuitClick(ClickEvent evt)
    {
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
    #endregion
}
