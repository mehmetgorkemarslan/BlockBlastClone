using UnityEngine;
using UnityEngine.Serialization;

public class BlockVisual : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Vector2Int gridPosition;

    public void Init(int r, int c)
    {
        gridPosition = new Vector2Int(r, c);
    }

    public void UpdateSprite(Sprite sprite)
    {
        if (spriteRenderer.sprite != sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
