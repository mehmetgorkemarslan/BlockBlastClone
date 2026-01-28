using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "GameBlocksVisuals", menuName = "Game/Blocks")]
public class BlockTheme : ScriptableObject
{
    [System.Serializable]
    public struct ColorSet
    {
        public string name;

        [Tooltip("Sprites to use.\nFirst one is default second is first threshold etc")]
        public Sprite[] sprites;

        /// <summary>
        /// </summary>
        /// <param name="iconType">0 for default
        /// 1 for first threshold etc</param>
        /// <returns>icon to use
        /// Returns default icon if there is any out of index situation</returns>
        public Sprite GetIcon(int iconType)
        {
            if (sprites == null || sprites.Length == 0)
            {
                Debug.LogWarning($"ColorSet '{name}' has no sprites configured.");
                return null;
            }

            if (iconType >= 0 && iconType < sprites.Length)
            {
                return sprites[iconType];
            }

            Debug.LogWarning($"GetIcon: invalid iconType {iconType} for color '{name}'. Returning default sprite.");
            return sprites[0];
        }
        
    }
    
    [Tooltip("Sprite Chance Thresholds\nFirst threshold uses Sprites[1]")]
    public int[] thresholds;

    [Tooltip("Order of colors is important")]
    public ColorSet[] colors;

    /// <summary>
    /// Returns Block visual with desired icon and color.
    /// Color index and icons determined by Import order.
    /// </summary>
    /// <param name="colorIndex">Index of color. Please check scriptable object</param>
    /// <param name="iconType">Icon type based on threshold and group size</param>
    /// <returns>Sprite to use</returns>
    public Sprite GetSprite(int colorIndex, int iconType)
    {
        if (colors == null || colors.Length == 0)
        {
            Debug.LogWarning("BlockTheme has no colors configured.");
            return null;
        }

        if (colorIndex < 0 || colorIndex >= colors.Length)
        {
            Debug.LogWarning($"GetSprite: invalid colorIndex {colorIndex} for theme '{name}'.");
            return null;
        }

        return colors[colorIndex].GetIcon(iconType);
    }
}