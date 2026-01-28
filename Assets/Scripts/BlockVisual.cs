using System.Collections;
using UnityEngine;

public class BlockVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    public Vector2Int gridPosition;
    
    private Coroutine _moveCoroutine;

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

    
    public void MovePos(Vector3 target, float duration)
    {
        if(_moveCoroutine != null) StopCoroutine(_moveCoroutine);
        _moveCoroutine = StartCoroutine(MoveRoutine(target, duration));
    }

    private IEnumerator MoveRoutine(Vector3 target, float duration)
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        while (elapsed <= duration)
        {
            float t = elapsed / duration;
            t = t*t*(3-2*t);

            transform.localPosition = Vector3.Lerp(startPos, target, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = target;
    }
}
