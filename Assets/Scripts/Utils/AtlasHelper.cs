using UnityEngine;
using System.Collections.Generic;
using System.Dynamic;

public static class AtlasHelper
{
    public static void PackRuntimeAtlas(BlockTheme.ColorSet[] allColorSets)
    {
        // Collect textures
        List<Texture2D> texturesToPack = new List<Texture2D>();

        List<(int setIndex, int spriteIndex)> mapping = new List<(int SetIndexBinder, int spriteIndex)>();

        for (int i = 0; i < allColorSets.Length; i++)
        {
            var set = allColorSets[i];

            if (set.sprites == null) continue;

            for (int j = 0; j < set.sprites.Length; j++)
            {
                // Texture exists and readable
                if (set.sprites[j] != null && set.sprites[j].texture != null)
                {
                    texturesToPack.Add(set.sprites[j].texture);
                    mapping.Add((i, j));
                }
            }
        }
        
        if (texturesToPack.Count == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Atlas Helper: There is nothing to pack");
#endif
            return;
        }

        
        // Create Atlas Texture
        Texture2D atlas = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);

        atlas.filterMode = FilterMode.Bilinear;

        Rect[] uvs = atlas.PackTextures(texturesToPack.ToArray(), 2, 2048);

        if (uvs == null)
        {
#if UNITY_EDITOR
            Debug.LogError("AtlasHelper: Textures does not fit or an error occurs during packing");
#endif
            return;
        }

        
        // Recreate sprites and assign
        for (int i = 0; i < texturesToPack.Count; i++)
        {
            var map = mapping[i];

            Rect uvRect = uvs[i];

            // Uv coordinates -> Pixel coordinate
            Rect pixelRect = new Rect(
                uvRect.x * atlas.width,
                uvRect.y * atlas.height,
                uvRect.width * atlas.width,
                uvRect.height * atlas.height
            );

            // I assume 0.5 , 0.5 is pivot
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            // The sprites that you gave me, has 256 pixel
            Sprite newSprite = Sprite.Create(atlas, pixelRect, pivot, 256.0f);
            
            newSprite.name = allColorSets[map.setIndex].sprites[map.spriteIndex].name + "_Packed";
            
            // Update scriptable object
            allColorSets[map.setIndex].sprites[map.spriteIndex] = newSprite;
        }
    }
}