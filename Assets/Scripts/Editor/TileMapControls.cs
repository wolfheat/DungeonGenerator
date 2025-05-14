using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEditor.Tilemaps;

public class TileMapControls : EditorWindow
{

    [MenuItem("Window/TileMapControls")]
    public static void ShowWindow()
    {
        TileMapControls example = GetWindow<TileMapControls>("Tile Map Controls");
    }

    private void OnGUI()
    {
        GUILayout.Label("Label");

        if(GUILayout.Button("Toggle Level 0")) {
            ToggleVisability(0);
        }
        if(GUILayout.Button("Toggle Level -1")) {
            ToggleVisability(-1);
        }
        if(GUILayout.Button("Toggle Level -2")) {
            ToggleVisability(-2);
        }
        if(GUILayout.Button("Toggle Level -3")) {
            ToggleVisability(-3);
        }
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
                GameObject holder = item.GetHolder;
                if (holder == null) continue;

                TilemapRenderer renderer = item.gameObject.GetComponent<TilemapRenderer>();
                if (renderer == null) continue;

                if (!item.gameObject.activeSelf) {
                    Debug.Log("Show tilemap, hide object for " + item.name);
                    // Show tilemap
                    item.gameObject.SetActive(true);    
                    holder.gameObject.SetActive(false);
                    renderer.enabled = true;
                    delayedSelectTileMap = item;
                    continue;
                }
                else if (!holder.gameObject.activeSelf) {
                    Debug.Log("Show tile objects for "+item.name);
                    // Show tileObjects
                    renderer.enabled = false;    
                    holder.gameObject.SetActive(true);
                    continue;
                }
                // Hide all
                Debug.Log("Hide all disable "+item.name);
                item.gameObject.SetActive(false);
            }
        }
        if(delayedSelectTileMap != null)
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
