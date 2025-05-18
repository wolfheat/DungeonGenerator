using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class TilemapChangeWatcher : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap3D tilemap3d;

    private BoundsInt previousBounds;
    private TileBase[] previousTiles;

    private void OnEnable()
    {
        if (tilemap3d == null)
            tilemap3d = GetComponent<Tilemap3D>();

        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();



        CacheCurrentTiles();
        EditorApplication.update += OnEditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (tilemap3d == null)
            return;

        var tileMapMode = tilemap3d.UpdateMode;
        
        if (tileMapMode == AutoUpdateMode.OFF)
            return;

        if(tileMapMode == AutoUpdateMode.INSTANT) {
            // Check tiles...
            var bounds = tilemap.cellBounds;
            var currentTiles = tilemap.GetTilesBlock(bounds);

            if (!AreTilesEqual(currentTiles, previousTiles)) {
                Debug.Log($"Tilemap '{tilemap.name}' changed!");
                CacheCurrentTiles();
                OnTilemapChanged();
            }
        }
    }

    private void CacheCurrentTiles()
    {
        previousBounds = tilemap.cellBounds;
        previousTiles = tilemap.GetTilesBlock(previousBounds);
    }

    private bool AreTilesEqual(TileBase[] a, TileBase[] b)
    {
        if (a == null || b == null || a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) {
            if (a[i] != b[i]) return false;
        }
        return true;
    }

    private void OnTilemapChanged()
    {
        Debug.Log($"Detected tile change on: {name}");
        // Custom callback

        if (tilemap3d == null)
            tilemap3d = GetComponent<Tilemap3D>();
        tilemap3d.Generate3DTilesForced();
    }
}
