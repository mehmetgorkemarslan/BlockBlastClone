using UnityEngine;

public class BlockVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Vector2Int gridPosition;

    public void Init(int r, int c)
    {
        gridPosition = new Vector2Int(r, c);
    }

    public void UpdateSprite(Sprite sprite)
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"BlockVisual missing SpriteRenderer on '{gameObject.name}'");
            return;
        }

        if (spriteRenderer.sprite != sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }
}
