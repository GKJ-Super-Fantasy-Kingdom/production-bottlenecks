using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ProductionBottlenecks
{
    internal static class ResourceIcon
    {
        private static readonly Dictionary<Sprite, string> CustomTags = new Dictionary<Sprite, string>();
        private static readonly char[] InvalidName = { '"', '<', '>' };

        internal static bool MatchesGlyph(TMP_SpriteAsset atlas, int index, string name, Sprite sprite)
        {
            return atlas != null && index >= 0 && index < atlas.spriteCharacterTable.Count &&
                atlas.GetSpriteIndexFromName(name) == index &&
                atlas.spriteCharacterTable[index]?.glyph is TMP_SpriteGlyph glyph && glyph.sprite == sprite;
        }

        internal static string Tag(TextMeshProUGUI label, Sprite sprite)
        {
            if (label == null || !label.richText) return null;
            string name = sprite.name;
            var atlas = label.spriteAsset != null ? label.spriteAsset : TMP_Settings.defaultSpriteAsset;
            if (atlas != null && !string.IsNullOrEmpty(name) && name.IndexOfAny(InvalidName) < 0)
            {
                var found = TMP_SpriteAsset.SearchForSpriteByHashCode(atlas, TMP_TextUtilities.GetSimpleHashCode(name), true, out int index);
                // An asset pack can replace a sprite while keeping its atlas name.
                if (MatchesGlyph(found, index, name, sprite))
                    return "<sprite name=\"" + name + "\">";
            }

            // Deposits such as Ore and Tree have a world sprite but no text glyph.
            // TMP keeps named sprite assets for the session; register only our own.
            if (CustomTags.TryGetValue(sprite, out var tag)) return tag;
            if (sprite.texture == null || (sprite.packed && (sprite.packingMode == SpritePackingMode.Tight ||
                sprite.packingRotation != SpritePackingRotation.None))) return null;
            var rect = sprite.textureRect;
            var shader = Shader.Find("TextMeshPro/Sprite");
            if (shader == null || rect.width <= 0 || rect.height <= 0) return null;

            string assetName = "ProductionBottlenecks_" + sprite.GetInstanceID();
            int hash = TMP_TextUtilities.GetSimpleHashCode(assetName);
            while (MaterialReferenceManager.TryGetSpriteAsset(hash, out _))
            {
                assetName += "_";
                hash = TMP_TextUtilities.GetSimpleHashCode(assetName);
            }
            var asset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            asset.name = assetName;
            asset.spriteSheet = sprite.texture;
            asset.material = new Material(shader) { mainTexture = sprite.texture };
            asset.spriteInfoList = new List<TMP_Sprite>
            {
                new TMP_Sprite
                {
                    name = "Resource", sprite = sprite, x = rect.x, y = rect.y, width = rect.width,
                    height = rect.height, yOffset = rect.height, xAdvance = rect.width, scale = 1f
                }
            };
            asset.UpdateLookupTables();
            MaterialReferenceManager.AddSpriteAsset(hash, asset);
            tag = "<sprite=\"" + assetName + "\" index=0>";
            CustomTags.Add(sprite, tag);
            return tag;
        }
    }
}
