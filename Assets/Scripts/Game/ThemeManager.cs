using System;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager instance { get; private set; }

    [Header("Data Source")] [SerializeField]
    private BlockTheme defaultThemeAsset;
    
    public BlockTheme activeTheme { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        
        DontDestroyOnLoad(gameObject);

        InitializeTheme();
    }

    private void InitializeTheme()
    {
        if (defaultThemeAsset == null)
        {
            Debug.LogError("ThemeManager Error: There is no default Default Theme");
            return;
        }

        activeTheme = Instantiate(defaultThemeAsset);
        
#if UNITY_EDITOR
        Debug.Log("ThemeManager: Theme loaded successfully");
#endif
    }
}
