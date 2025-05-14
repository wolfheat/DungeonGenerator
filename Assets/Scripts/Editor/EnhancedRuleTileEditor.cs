using UnityEngine;
using UnityEditor;

namespace UnityEditor
{
    [CustomEditor(typeof(EnhancedRuleTile))]
    public class EnhancedRuleTileEditor : RuleTileEditor
    {

        [SerializeField] private Texture2D solidTexture;
        [SerializeField] private Texture2D hollowTexture;
        [SerializeField] private Texture2D emptyTexture;

        // 3 = Solid
        // 4 = Hollow
        // 5 = Null
        // 6 = NotNull

        public override void RuleOnGUI(Rect rect, Vector3Int position, int neighbor)
        {
            switch (neighbor) {
                case 3: // 
                    GUI.DrawTexture(rect, solidTexture);
                    return;

                case 4:
                    GUI.DrawTexture(rect, hollowTexture);
                    return;

                case 5:
                    GUI.DrawTexture(rect, emptyTexture);
                    return;
            }


            base.RuleOnGUI(rect, position, neighbor);
        }
    }
}
