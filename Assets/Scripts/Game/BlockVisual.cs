using System;
using UnityEngine;
using DG.Tweening;

public class BlockVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Vector2Int gridPosition;
    
    public void Init(int r, int c)
    {
        gridPosition = new Vector2Int(r, c);
        ResetState();
    }

    public void ResetState()
    {
        transform.DOKill();
        if (spriteRenderer)
        {
            spriteRenderer.color = Color.white;
        }
        // Todo: Stop particle things
        transform.localScale = Vector3.one; 
        transform.localRotation = Quaternion.identity;
    }
    
    public void UpdateSprite(Sprite sprite)
    {
        if (!spriteRenderer)
        {
            Debug.LogWarning($"BlockVisual missing SpriteRenderer on '{gameObject.name}'");
            return;
        }

        if (spriteRenderer.sprite != sprite)
        {
            spriteRenderer.sprite = sprite;
        }
    }

    
    public void MovePos(Vector3 target, float duration)
    {
        transform.DOKill();
        transform.DOLocalMove(target, duration).SetEase(Ease.OutQuad);
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
