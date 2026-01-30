using System;
using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager instance { get; private set; }

    [Header("Data Source")] [SerializeField]
    private BlockTheme defaultThemeAsset;
    
    public BlockTheme activeTheme { get; private set; }

    // This is for after first atlases
    private Dictionary<(int, int), Sprite> _originalSpritesBackup;
    
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

        BackupOriginalSprites();
        AtlasHelper.PackRuntimeAtlas(activeTheme.colors);
        
#if UNITY_EDITOR
        Debug.Log("ThemeManager: Theme loaded successfully");
#endif
    }

    private void BackupOriginalSprites()
    {
        _originalSpritesBackup = new Dictionary<(int, int), Sprite>();
        
        for (int i = 0; i < activeTheme.colors.Length; i++)
        {
            var set = activeTheme.colors[i];
            for (int j = 0; j < set.sprites.Length; j++)
            {
                if (set.sprites[j] != null)
                {
                    _originalSpritesBackup.Add((i, j), set.sprites[j]);
                }
            }
        }
    }

    public void RefreshTHemeAtlas()
    {
        if (_originalSpritesBackup == null) return;

        // Replace sprites with originals
        for (int i = 0; i < activeTheme.colors.Length; i++)
        {
            var set = activeTheme.colors[i];
            for (int j = 0; j < set.sprites.Length; j++)
            {
                if (_originalSpritesBackup.TryGetValue((i, j), out Sprite original))
                {
                    set.sprites[j] = original;
                }
            }
        }
        
        // Pack them again
        AtlasHelper.PackRuntimeAtlas(activeTheme.colors);
#if UNITY_EDITOR
        Debug.Log("ThemeManager: Atlas created again");
#endif
    }
}
