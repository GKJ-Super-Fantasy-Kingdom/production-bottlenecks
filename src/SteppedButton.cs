using UnityEngine;
using UnityEngine.UI;

namespace ProductionBottlenecks
{
    internal sealed class SteppedButton : Button
    {
        private static Sprite steppedEdge;
        private Image edge;
        private bool allActiveStates;

        internal void SetUpEdge(Image plate, bool needsUntintedEdge)
        {
            if (plate.sprite == null) return;
            allActiveStates = needsUntintedEdge;
            var obj = new GameObject("Active edge", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)obj.transform;
            rect.SetParent(transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            edge = obj.GetComponent<Image>();
            edge.sprite = GetEdge();
            edge.type = Image.Type.Sliced;
            edge.pixelsPerUnitMultiplier = plate.pixelsPerUnitMultiplier * plate.sprite.pixelsPerUnit / edge.sprite.pixelsPerUnit;
            edge.raycastTarget = false;
            UpdateEdge(currentSelectionState);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            UpdateEdge(state);
        }

        private void UpdateEdge(SelectionState state)
        {
            if (edge != null)
                edge.enabled = state == SelectionState.Pressed ||
                    (allActiveStates && (state == SelectionState.Highlighted || state == SelectionState.Selected));
        }

        private static Sprite GetEdge()
        {
            if (steppedEdge != null) return steppedEdge;
            // A transparent center lets the game draw each state's fill, including pressed.
            const string pattern =
                "  W  " +
                " WKW " +
                "WK KW" +
                " WKW " +
                "  W  ";
            var pixels = new Color32[pattern.Length];
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == 'W') pixels[i] = Color.white;
                if (pattern[i] == 'K') pixels[i] = new Color32(6, 6, 8, 255);
            }

            var texture = new Texture2D(5, 5, TextureFormat.RGBA32, false)
            {
                name = "Production Bottlenecks button edge",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            steppedEdge = Sprite.Create(texture, new Rect(0, 0, 5, 5), Vector2.one * .5f, 100,
                0, SpriteMeshType.FullRect, Vector4.one * 2);
            return steppedEdge;
        }
    }
}
