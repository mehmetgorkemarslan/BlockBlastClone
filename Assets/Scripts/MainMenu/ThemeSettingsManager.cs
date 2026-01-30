using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ThemeSettingsManager : MonoBehaviour
{
    private BlockTheme theme;

    // UI Elements
    private VisualElement _settingsRoot;
    
    // Theme Settings
    private VisualElement _colorsContainer;
    private VisualElement _thresholdListContainer;
    private IntegerField _activeCountInput;
    private Button _addThresholdBtn;
    private Button _removeThresholdBtn;
    private Button _addColorBtn;
    
    // Board Settings
    private IntegerField _rowsInput;
    private IntegerField _colsInput;
    private Slider _clusterSlider;
    private Label _clusterValueLabel;

    public void Initialize(VisualElement root)
    {
        _settingsRoot = root;

        _colorsContainer = root.Q<VisualElement>("ColorsContainer");
        _thresholdListContainer = root.Q<VisualElement>("ThresholdListContainer");
        _activeCountInput = root.Q<IntegerField>("ActiveCountInput");

        _addThresholdBtn = root.Q<Button>("AddThresholdBtn");
        _removeThresholdBtn = root.Q<Button>("RemoveThresholdBtn");
        _addColorBtn = root.Q<Button>("AddNewColorBtn");
        
        
        _rowsInput = root.Q<IntegerField>("RowsInput");
        _colsInput = root.Q<IntegerField>("ColsInput");
        _clusterSlider = root.Q<Slider>("ClusterSlider");
        _clusterValueLabel = root.Q<Label>("ClusterValueLabel");

        if (ThemeManager.instance == null || ThemeManager.instance.activeTheme == null)
        {
            Debug.LogError("Theme Manager Cant found. Please Start from main menu");
            return;
        }

        theme = ThemeManager.instance.activeTheme;
        
        SubscribeEvents();

        RefreshUI();
    }

    private void SubscribeEvents()
    {
        // Board
        _rowsInput?.RegisterValueChangedCallback(evt => {
            // From case pdf
            int val = Mathf.Clamp(evt.newValue, 2, 10);
            if(theme.rows != val) 
            {
                theme.rows = val;
                _rowsInput.SetValueWithoutNotify(val);
                SetDirty();
            }
        });

        _colsInput?.RegisterValueChangedCallback(evt => {
            // From case pdf
            int val = Mathf.Clamp(evt.newValue, 2, 10);
            if(theme.columns != val)
            {
                theme.columns = val;
                _colsInput.SetValueWithoutNotify(val);
                SetDirty();
            }
        });

        _clusterSlider?.RegisterValueChangedCallback(evt => {
            theme.clusterChance = evt.newValue;
            UpdateClusterLabel(evt.newValue);
            SetDirty();
        });
        
        // Theme
        _activeCountInput?.RegisterValueChangedCallback(evt =>
        {
            int max = theme.colors.Length > 0 ? theme.colors.Length : 1;
            int val = Math.Clamp(evt.newValue, 1, max);

            if (theme.activeColorCount != val)
            {
                theme.activeColorCount = val;
                _activeCountInput.SetValueWithoutNotify(val);
                SetDirty();
                RefreshUI();
            }
        });

        _addThresholdBtn.clicked += () => ModifyThresholds(1);
        _removeThresholdBtn.clicked += () => ModifyThresholds(-1);
        _addColorBtn.clicked += OnAddNewColor;
    }

    private void RefreshUI()
    {
        RenderBoardSettings();
        RenderHeader();
        RenderBody();
    }

    private void RenderBoardSettings()
    {
        if (_rowsInput != null) _rowsInput.value = theme.rows;
        if (_colsInput != null) _colsInput.value = theme.columns;
        
        if (_clusterSlider != null)
        {
            _clusterSlider.value = theme.clusterChance;
            UpdateClusterLabel(theme.clusterChance);
        }
    }
    
    private void UpdateClusterLabel(float value)
    {
        if (_clusterValueLabel != null)
            _clusterValueLabel.text = $"{(value * 100):F0}%";
    }
    
    #region Header Logic

    private void RenderHeader()
    {
        _activeCountInput.SetValueWithoutNotify(theme.activeColorCount);
        _thresholdListContainer.Clear();

        if (theme.thresholds == null) theme.thresholds = new int[0];

        for (int i = 0; i < theme.thresholds.Length; i++)
        {
            int index = i;
            var field = new IntegerField();
            field.value = theme.thresholds[i];
            field.style.width = 50;
            field.style.marginRight = 5;
            field.RegisterValueChangedCallback(evt =>
            {
                theme.thresholds[index] = evt.newValue;
                SetDirty();
                RefreshUI();
            });
            _thresholdListContainer.Add(field);
        }
    }

    private void ModifyThresholds(int direction)
    {
        var list = theme.thresholds.ToList();
        if (direction > 0) list.Add(list.Count > 5 ? list.Last() + 5 : 5);
        else if (list.Count > 0) list.Remove(list.Count - 1);
        else return;

        theme.thresholds = list.ToArray();
        ResizeAllSpriteArrays();
        SetDirty();
        RefreshUI();
    }

    private void ResizeAllSpriteArrays()
    {
        int targetSize = theme.thresholds.Length + 1;
        for (int i = 0; i < theme.colors.Length; i++)
        {
            var sprites = theme.colors[i].sprites.ToList();
            while (sprites.Count < targetSize) sprites.Add(null);
            while (sprites.Count > targetSize) sprites.RemoveAt(sprites.Count - 1);
            theme.colors[i].sprites = sprites.ToArray();
        }
    }

    #endregion

    #region Body Logic

    private void RenderBody()
    {
        _colorsContainer.Clear();
        if (theme.colors == null) return;

        for (int i = 0; i < theme.colors.Length; i++)
        {
            _colorsContainer.Add(CreateColorCard(i));
        }
    }

    private VisualElement CreateColorCard(int index)
    {
        var card = new VisualElement();
        card.AddToClassList("color-card");

        bool isActive = index < theme.activeColorCount;

        if (isActive) card.AddToClassList("card-active");
        else card.AddToClassList("card-reserve");

        var header = new VisualElement();
        header.AddToClassList("cart-header");

        var nameField = new TextField { value = theme.colors[index].name, style = { flexGrow = 1 } };
        nameField.RegisterValueChangedCallback(evt =>
        {
            theme.colors[index].name = evt.newValue;
            SetDirty();
        });

        var actions = new VisualElement();
        actions.AddToClassList("card-actions");
        actions.Add(CreateActionButton("▲", () => MoveColor(index, -1)));
        actions.Add(CreateActionButton("▼", () => MoveColor(index, 1)));

        var delBtn = CreateActionButton("X", () => DeleteColor(index));
        delBtn.style.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
        actions.Add(delBtn);

        header.Add(nameField);
        header.Add(actions);
        card.Add(header);

        var spriteRow = new VisualElement();
        spriteRow.AddToClassList("sprite-row");

        for (int s = 0; s < theme.colors[index].sprites.Length; s++)
        {
            spriteRow.Add(CreateSpriteSlot(index, s));
        }

        card.Add(spriteRow);

        if (!isActive) card.Add(new Label("(RESERVE)") { style = { color = Color.yellow, fontSize = 10 } });

        return card;
    }

    private Button CreateActionButton(string text, System.Action onClick)
    {
        var btn = new Button(onClick) { text = text };
        btn.AddToClassList("action-icon");
        return btn;
    }

    private VisualElement CreateSpriteSlot(int colorIndex, int spriteIndex)
    {
        var slot = new VisualElement();
        slot.AddToClassList("sprite-slot");

        var preview = new VisualElement();
        preview.AddToClassList("sprite-preview");
        Sprite spr = theme.colors[colorIndex].sprites[spriteIndex];
        if (spr != null) preview.style.backgroundImage = new StyleBackground(spr);

        // Label
        string infoText = spriteIndex == 0 ? "Default" : $"> {theme.thresholds[spriteIndex - 1]}";
        var info = new Label(infoText);
        info.AddToClassList("slot-info");

        slot.Add(preview);
        slot.Add(info);

        slot.RegisterCallback<ClickEvent>(evt =>
        {
            // Todo: File system
#if UNITY_EDITOR
            
            Debug.Log($"Change Sprite: Color {colorIndex}, Level {spriteIndex}.");
#endif
        });

        return slot;
    }

    #endregion

    #region List Operations

    private void MoveColor(int index, int direction)
    {
        int newIndex = index + direction;
        if (newIndex < 0 || newIndex >= theme.colors.Length) return;

        var temp = theme.colors[index];
        theme.colors[index] = theme.colors[newIndex];
        theme.colors[newIndex] = temp;
        SetDirty();
        RefreshUI();
    }

    private void DeleteColor(int index)
    {
        var list = theme.colors.ToList();
        list.RemoveAt(index);
        theme.colors = list.ToArray();

        if (theme.activeColorCount > theme.colors.Length)
            theme.activeColorCount = theme.colors.Length;

        SetDirty();
        RefreshUI();
    }

    private void OnAddNewColor()
    {
        var list = theme.colors.ToList();
        var newSet = new BlockTheme.ColorSet
        {
            name = "New Color",
            sprites = new Sprite[theme.thresholds.Length + 1]
        };
        list.Add(newSet);
        theme.colors = list.ToArray();
        SetDirty();
        RefreshUI();
    }

    #endregion

    private void SetDirty()
    {
        //TODO: Implement a device storage system
#if UNITY_EDITOR
        // Stores values. Its work just for editor
        EditorUtility.SetDirty(theme);
#endif
    }
}