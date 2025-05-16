using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEditor.Tilemaps;

public class TileMapControls : EditorWindow
{
    private bool autoUpdate = true;

    [MenuItem("Window/TileMapControls")]
    public static void ShowWindow()
    {
        TileMapControls example = GetWindow<TileMapControls>("Tile Map Controls");
    }

    private void OnGUI()
    {
        Tilemap3DModes[] modes = GetMapModes();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Dungeon Generator Controls");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Auto Generate is "+(autoUpdate?"ON":"OFF"))) {
            ToggleAutoUpdate();
        }

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Levels");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Level 0",GUILayout.Width(150))) {
            ToggleVisability(0);
        }
        if (modes.Length >= 1) GUILayout.Label(" " + modes[0]);
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Level -1",GUILayout.Width(150))) {
            ToggleVisability(-1);
        }
        if (modes.Length >= 2) GUILayout.Label(" " + modes[1]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Toggle Level -2",GUILayout.Width(150))) {
            ToggleVisability(-2);
        }
        if (modes.Length >= 3) GUILayout.Label(" " + modes[2]);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Toggle Level -3",GUILayout.Width(150))) {
            ToggleVisability(-3);
        }
        if(modes.Length >= 4) GUILayout.Label(" " + modes[3]);
        GUILayout.EndHorizontal();


    }

    private void ToggleAutoUpdate() => autoUpdate = !autoUpdate;

    private Tilemap3DModes[] GetMapModes()
    {
        return Resources.FindObjectsOfTypeAll<Tilemap3D>()
            .Where(go =>
                !EditorUtility.IsPersistent(go) &&            // Excludes prefab assets
                go.hideFlags == HideFlags.None)// &&             // Excludes hidden objects like gizmo handles
                                               //go.scene.IsValid())                           // Ensures it's part of a loaded scene
            .OrderByDescending(x => x.transform.position.y)
            .Select(x => x.Mode).ToArray();
    }
    private void ToggleVisability(int layer)
    {
        Tilemap3D[] tilemaps = Resources.FindObjectsOfTypeAll<Tilemap3D>()
            .Where(go =>
                !EditorUtility.IsPersistent(go) &&            // Excludes prefab assets
                go.hideFlags == HideFlags.None)// &&             // Excludes hidden objects like gizmo handles
                //go.scene.IsValid())                           // Ensures it's part of a loaded scene
            .ToArray();

        Tilemap3D delayedSelectTileMap = null;

        foreach (var item in tilemaps) {
            if(item.transform.position.y == layer) {
                // Correct layer
                if (item.Mode == Tilemap3DModes.OFF) {
                    item.gameObject.SetActive(true);
                    item.TileMapView();
                    delayedSelectTileMap = item; // Used to turn on the new tilemap and select it from editor calls
                    continue;
                }
                else if (item.Mode == Tilemap3DModes.TileMap) {
                    item.ObjectView(autoUpdate);
                    
                    continue;
                }
                // Hide all
                item.TurnOff();
            }
        }

        // Used to turn on the new tilemap and select it from editor calls
        if (delayedSelectTileMap != null)
        EditorApplication.delayCall += () => {
            Selection.activeGameObject = delayedSelectTileMap.gameObject;
            GridPaintingState.scenePaintTarget = delayedSelectTileMap.gameObject;
            //SceneView.lastActiveSceneView.FrameSelected(); // Optional: focus on it
        };
    }

    Transform GetFirstChildOf(Transform parent)
    {
        return Resources.FindObjectsOfTypeAll<Transform>()
            .Where(t =>
                t != null &&
                t.parent == parent &&
                !EditorUtility.IsPersistent(t) &&
                t.gameObject.scene.IsValid()).First();
    }
    Transform[] GetAllChildrenIncludingInactive(Transform parent)
    {
        return Resources.FindObjectsOfTypeAll<Transform>()
            .Where(t =>
                t != null &&
                t.parent == parent &&
                !EditorUtility.IsPersistent(t) &&
                t.gameObject.scene.IsValid())
            .ToArray();
    }
}
