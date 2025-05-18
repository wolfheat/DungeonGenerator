using System.Linq;
using UnityEditor;
using UnityEngine;




[InitializeOnLoad]
public static class TileMapControlsEditorInitializer
{   

    private static bool initialized = false;
    static TileMapControlsEditorInitializer ()
    {
        // Code that runs as the application starts
        //EditorApplication.delayCall += UpdateAllTilemap3DModes;
        EditorApplication.update += WaitForFocusToDoUpdate;
    }

    // Update All modes
    public static void WaitForFocusToDoUpdate()
    {
        if (initialized) return;
        initialized = true;

        UpdateAllTilemap3DModes();
        
    }
    


    // Update All modes
    public static void UpdateAllTilemap3DModes()
    {
        Tilemap3D[] maps = GetTileMap3Ds();
        foreach (var map in maps) {
            map.SetCurrentMode();
        }
    }

    private static Tilemap3D[] GetTileMap3Ds()
    {
        return Resources.FindObjectsOfTypeAll<Tilemap3D>()
            .Where(go =>
                !EditorUtility.IsPersistent(go) &&            // Excludes prefab assets
                go.hideFlags == HideFlags.None)// &&             // Excludes hidden objects like gizmo handles
                                               //go.scene.IsValid())                           // Ensures it's part of a loaded scene
            .OrderByDescending(x => x.transform.position.y)
            .ToArray();
    }
}
