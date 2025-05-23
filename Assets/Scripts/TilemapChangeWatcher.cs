using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;

[ExecuteAlways]
public class TilemapChangeWatcher : MonoBehaviour
{
    public Tilemap tilemap;
    public Tilemap3D tilemap3d;

    private TileBase[] previousTiles;
    private int[] previousRotations;

    Dictionary<Vector3Int, Sprite> previousSprites = new();

    private void OnEnable()
    {
        if (tilemap3d == null)
            tilemap3d = GetComponent<Tilemap3D>();

        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();


        var bounds = tilemap.cellBounds;
        CacheCurrentTiles(bounds, GetTilemapRotations(tilemap,bounds));
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
        var bounds = tilemap.cellBounds;
        var currentTiles = tilemap.GetTilesBlock(bounds);
        var currentRotations = GetTilemapRotations(tilemap,bounds);

        switch (tileMapMode) {
            case AutoUpdateMode.OFF:
                return;
            case AutoUpdateMode.ON:
                break;
            case AutoUpdateMode.INSTANT:
                // Check tiles...
                if (!AreTilesEqual(currentTiles, previousTiles, currentRotations)) {
                    //Debug.Log($"Tilemap '{tilemap.name}' changed!");
                    CacheCurrentTiles(bounds, currentRotations);
                    //OnTilemapChangedUpdateAll();
                    OnTilemapChangedUpdateAll();
                }
                break;
            case AutoUpdateMode.INSTANTFAST:
                // Check tiles...
                if (!AreTilesEqualGetChanges(currentTiles, previousTiles, bounds, currentRotations, out List <Vector2Int> changedPositions)) {
                    //Debug.Log($"Tilemap '{tilemap.name}' changed!");
                    CacheCurrentTiles(bounds, currentRotations);
                    //OnTilemapChangedUpdateAll();
                    OnTilemapChangedUpdateOnlyAffectedTiles(changedPositions);

                }
                break;
        }
    }

    private void CacheCurrentTiles(BoundsInt bounds, int[] currentRotations)
    {
        previousTiles = tilemap.GetTilesBlock(bounds);
        previousRotations = currentRotations;

        previousSprites.Clear();
        foreach (var pos in bounds.allPositionsWithin) {
            previousSprites[pos] = tilemap.GetSprite(pos);
        }
    }

    private int[] GetTilemapRotations(Tilemap tilemap, BoundsInt bounds)
    {
        int[] rotations = new int[bounds.size.x * bounds.size.y];
        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                float currentRotation = tilemap.GetTransformMatrix(new Vector3Int(x, y, 0)).rotation.eulerAngles.z;
                int rotationInt = Mathf.RoundToInt(currentRotation + 360) % 360;
                rotations[bounds.size.x * y + x] = rotationInt;
            }
        }
        return rotations;
    }

    bool AreTilesEqual(TileBase[] currentTiles, TileBase[] previousTiles, int[] currentRotations)
    {        
        if (currentTiles == null || previousTiles == null || currentTiles.Length != previousTiles.Length || previousRotations.Length != currentRotations.Length) {
            return true;
        }

        // The amount of tiles are the same
        for (int i = 0; i < currentTiles.Length; i++) {
            TileBase current = currentTiles[i];
            TileBase previous = previousTiles[i];

            int currentRotation = currentRotations[i];
            int previousRotation = previousRotations[i];
            
            if (current != previous || currentRotation != previousRotation) {
                return false;
            }
        }
        return true;
    }
    
    bool AreTilesEqualGetChanges(TileBase[] currentTiles, TileBase[] previousTiles, BoundsInt bounds, int[] currentRotations, out List<Vector2Int> changedPositions)
    {
        changedPositions = new List<Vector2Int>();

        // Update ALL
        if (currentTiles == null || previousTiles == null || currentTiles.Length != previousTiles.Length || previousRotations.Length != currentRotations.Length) {
            // All tiles are considered changed
            for (int x = 0; x < bounds.size.x; x++) {
                for (int y = 0; y < bounds.size.y; y++) {
                    changedPositions.Add(new Vector2Int(bounds.xMin + x, bounds.yMin + y));
                }
            }
            return false;
        }

        // Get the positions to update
        for (int x = 0; x < bounds.size.x; x++) {
            for (int y = 0; y < bounds.size.y; y++) {
                int index = x + y * bounds.size.x;
                TileBase current = currentTiles[index];
                TileBase previous = previousTiles[index];
                int currentRotation = currentRotations[index];
                int previousRotation = previousRotations[index];

                if (current != previous || currentRotation != previousRotation) {
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

    Sprite GetCachedSprite(TileBase tile, Vector3Int pos, Tilemap tilemap) => previousSprites.TryGetValue(pos, out var sprite) ? sprite : null;

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
#endif