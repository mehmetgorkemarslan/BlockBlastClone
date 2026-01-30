using System;
using System.Collections.Generic;
using UnityEngine;

public class BlockPoolManager : MonoBehaviour
{
    [SerializeField] private GameObject blockPrefab; 
    [SerializeField] private int safeBuffer = 20;
    [SerializeField] private Transform container;
    private BlockTheme _theme;

    // Stack because Stack is more cache friendly than Queue
    private Stack<BlockVisual> _pool = new Stack<BlockVisual>();

    private void Awake()
    {
        
        if (ThemeManager.instance == null || ThemeManager.instance.activeTheme == null)
        {
            Debug.LogError("Theme Manager Cant found. Please Start from main menu");
            return;
        }

        _theme = ThemeManager.instance.activeTheme;
        InitializePool();
    }

    private void InitializePool()
    {   
        int totalSize = (_theme.rows * _theme.columns) + safeBuffer;
        for (int i = 0; i < totalSize; i++)
        {
            CreateNewBlock(false);
        }
    }

    private BlockVisual CreateNewBlock(bool isActive)
    {
        GameObject obj = Instantiate(blockPrefab, container);
        obj.SetActive(isActive);
        
        // Cache blocks visual component
        BlockVisual visual = obj.GetComponent<BlockVisual>();
        
        if (!isActive)
        {
            _pool.Push(visual);
        }
        
        return visual;
    }

    /// <summary>
    /// Use instead of Instantiate new object
    /// </summary>
    /// <param name="parent">block parent</param>
    /// <param name="localPosition">block's position</param>
    /// <returns>Block to use</returns>
    public BlockVisual GetBlock(Transform parent, Vector3 localPosition)
    {
        BlockVisual visual;

        if (_pool.Count > 0)
        {
            visual = _pool.Pop();
        }
        else
        {
            visual = CreateNewBlock(true);
        }

        visual.gameObject.SetActive(true);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one;

        return visual;
    }

    /// <summary>
    /// Use instead of Destoy
    /// </summary>
    /// <param name="block">Block to destroy</param>
    public void ReturnBlock(BlockVisual block)
    {
        block.gameObject.SetActive(false);
        block.transform.SetParent(container);
        _pool.Push(block);
    }
}