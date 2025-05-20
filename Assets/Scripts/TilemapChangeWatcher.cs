using UnityEngine;
using UnityEngine.Tilemaps;
using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;




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


        var bounds = tilemap.cellBounds;
        CacheCurrentTiles(bounds);
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

            if (!AreTilesEqual(currentTiles, previousTiles, bounds, out List <Vector2Int> changedPositions)) {
                //Debug.Log($"Tilemap '{tilemap.name}' changed!");
                CacheCurrentTiles(bounds);
                //OnTilemapChangedUpdateAll();
                OnTilemapChangedUpdateAll();
            }
        }
        if(tileMapMode == AutoUpdateMode.INSTANTFAST) {
            // Check tiles...
            var bounds = tilemap.cellBounds;
            var currentTiles = tilemap.GetTilesBlock(bounds);

            if (!AreTilesEqual(currentTiles, previousTiles, bounds, out List <Vector2Int> changedPositions)) {
                //Debug.Log($"Tilemap '{tilemap.name}' changed!");
                CacheCurrentTiles(bounds);
                //OnTilemapChangedUpdateAll();
                OnTilemapChangedUpdateOnlyAffectedTiles(changedPositions);

            }
        }
    }

    Dictionary<Vector3Int, Sprite> previousSprites = new();
    private void CacheCurrentTiles(BoundsInt bounds)
    {
        previousTiles = tilemap.GetTilesBlock(bounds);
        previousSprites.Clear();
        foreach (var pos in bounds.allPositionsWithin) {
            previousSprites[pos] = tilemap.GetSprite(pos);
        }
    }
    bool AreTilesEqual(TileBase[] currentTiles, TileBase[] previousTiles, BoundsInt bounds, out List<Vector2Int> changedPositions)
    {
        changedPositions = new List<Vector2Int>();

        if (currentTiles == null || previousTiles == null || currentTiles.Length != previousTiles.Length) {
            // All tiles are considered changed
            for (int x = 0; x < bounds.size.x; x++) {
                for (int y = 0; y < bounds.size.y; y++) {
                    changedPositions.Add(new Vector2Int(bounds.xMin + x, bounds.yMin + y));
                }
            }
            return false;
        }

        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                int index = x + y * bounds.size.x;
                TileBase current = currentTiles[index];
                TileBase previous = previousTiles[index];
                if (current != previous) {
                    changedPositions.Add(new Vector2Int(bounds.xMin + x, bounds.yMin + y));
                    continue;
                }
                Vector3Int pos = new Vector3Int(bounds.xMin + x, bounds.yMin + y, 0);

                // Compare the visual sprite
                Sprite currentSprite = tilemap.GetSprite(pos);
                Sprite previousSprite = previous != null ? GetCachedSprite(previous, pos, tilemap) : null;

                if (currentSprite != previousSprite) {
                    changedPositions.Add(new Vector2Int(pos.x, pos.y));
                }

            }
        }

        return changedPositions.Count == 0;


    }
    Sprite GetCachedSprite(TileBase tile, Vector3Int pos, Tilemap tilemap)
    {
        return previousSprites.TryGetValue(pos, out var sprite) ? sprite : null;
    }
    private void OnTilemapChangedUpdateOnlyAffectedTiles(List<Vector2Int> changedPositions)
    {
        if (tilemap3d == null)
            tilemap3d = GetComponent<Tilemap3D>();
        tilemap3d.Generate3DTilesForcedSpecific(changedPositions);
    }
    private void OnTilemapChangedUpdateAll()
    {
        Debug.Log($"Detected tile change on: {name}");
        // Custom callback

        if (tilemap3d == null)
            tilemap3d = GetComponent<Tilemap3D>();
        tilemap3d.Generate3DTilesForced();
    }
}
