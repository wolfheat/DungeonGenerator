using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Tilemaps;


public class TileMapControls : EditorWindow
{
    public static AutoUpdateMode autoUpdate = AutoUpdateMode.OFF;

    public static TileMapControls Instance;
    //public static AutoUpdateMode ActiveAutoUpdateMode => Instance.autoUpdate;

    [MenuItem("Window/TileMapControls")]
    public static void ShowWindow()
    {
        TileMapControls example = GetWindow<TileMapControls>("Tile Map Controls");
        Instance = GetWindow<TileMapControls>("Tile Map Controls");
    }

    private void OnGUI()
    {
        Tilemap3DModes[] modes = GetMapModes();
        int activeLayer = GetActiveLayer();

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Dungeon Generator Controls",EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Auto Generate is ", GUILayout.Width(150))) {
            ToggleAutoUpdate();
        }
        GUILayout.Label(" " + autoUpdate);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Levels",EditorStyles.boldLabel);   
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Level 0",GUILayout.Width(150))) {
            ToggleVisability(0);
        }
        if (modes.Length >= 1) GUILayout.Label(" " + modes[0]);
        if (GUILayout.Button(activeLayer == 0 ? "ACTIVE":"---",GUILayout.Width(70))) {
            // Request this layer to become active
            ToggleActive(0);
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Toggle Level -1",GUILayout.Width(150))) {
            ToggleVisability(-1);
        }
        if (modes.Length >= 2) GUILayout.Label(" " + modes[1]);
        if (GUILayout.Button(activeLayer == 1 ? "ACTIVE" : "---", GUILayout.Width(70))) {
            // Request this layer to become active
            ToggleActive(1);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Toggle Level -2",GUILayout.Width(150))) {
            ToggleVisability(-2);
        }
        if (modes.Length >= 3) GUILayout.Label(" " + modes[2]);
        if (GUILayout.Button(activeLayer == 2 ? "ACTIVE" : "---", GUILayout.Width(70))) {
            // Request this layer to become active
            ToggleActive(2);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if(GUILayout.Button("Toggle Level -3",GUILayout.Width(150))) {
            ToggleVisability(-3);
        }
        GUILayout.Label((modes.Length >= 4)?(" " + modes[3]):"DISABLED");
        if (GUILayout.Button(activeLayer == 3 ? "ACTIVE" : "---", GUILayout.Width(70))) {
            // Request this layer to become active
            ToggleActive(3);
        }
        GUILayout.EndHorizontal();
    }

    private int GetActiveLayer()
    {
        Tilemap3D[] tilemaps = GetAllTileMap3D();

        if (GridPaintingState.scenePaintTarget != null && GridPaintingState.scenePaintTarget.TryGetComponent(out Tilemap3D active)) {
            for (int i = 0; i < tilemaps.Length; i++) {
                if (tilemaps[i] == active) {
                    Debug.Log("The active tilemap is number "+i);
                    return i;
                }
            }   
        }
        return 0;
        //Selection.activeGameObject = delayedSelectTileMap.gameObject;
        //GridPaintingState.scenePaintTarget = delayedSelectTileMap.gameObject;
    }

    private void ToggleAutoUpdate()
    {
        if(autoUpdate == AutoUpdateMode.INSTANT) {
            autoUpdate = AutoUpdateMode.OFF;         
        }else
            autoUpdate = (AutoUpdateMode)((int)autoUpdate+1);

        // Send info to all tilemap3d instances so they update themself
        SetAllAutoUpdate(autoUpdate);
    }

    // Set UpdateMode for all Tilemap3ds
    private void SetAllAutoUpdate(AutoUpdateMode mode)
    {
        Tilemap3D[] tilemaps = GetAllTileMap3D();
        foreach (var map in tilemaps)
            map.UpdateMode = mode;
    }


    private Tilemap3D[] GetAllTileMap3D()
    {
        return Resources.FindObjectsOfTypeAll<Tilemap3D>()
            .Where(go =>
                !EditorUtility.IsPersistent(go) &&            // Excludes prefab assets
                go.hideFlags == HideFlags.None)// &&             // Excludes hidden objects like gizmo handles
                                               //go.scene.IsValid())                           // Ensures it's part of a loaded scene
            .OrderByDescending(x => x.transform.position.y)
            .ToArray();
    }
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
    private void ToggleActive(int layer)
    {
        Debug.Log("TOGLLE ACTIVE LAYER "+layer);
        // Set layer to the active one so player can paint on it
        Tilemap3D[] tilemaps = Resources.FindObjectsOfTypeAll<Tilemap3D>()
            .Where(go =>
                !EditorUtility.IsPersistent(go) &&            // Excludes prefab assets
                go.hideFlags == HideFlags.None)// &&             // Excludes hidden objects like gizmo handles
                //go.scene.IsValid())                           // Ensures it's part of a loaded scene
            .ToArray();

        Tilemap3D delayedSelectTileMap = null;


        Debug.Log("Layers found "+tilemaps.Length);

        foreach (var item in tilemaps) {
            Debug.Log("Item position = "+ Mathf.RoundToInt(item.transform.position.y));
            if(Mathf.RoundToInt(item.transform.position.y) == -layer) {
                // Correct layer
                if (item.Mode == Tilemap3DModes.OFF) {
                    item.gameObject.SetActive(true);
                    item.TileMapView();
                }
                delayedSelectTileMap = item;
                Debug.Log("layer "+layer+" set as active");
            }
        }

        // Used to turn on the new tilemap and select it from editor calls
        if (delayedSelectTileMap != null)
        EditorApplication.delayCall += () => {
            Debug.Log("Delayed set as active from ToggleActive");
            Selection.activeGameObject = delayedSelectTileMap.gameObject;
            GridPaintingState.scenePaintTarget = delayedSelectTileMap.gameObject;
            //SceneView.lastActiveSceneView.FrameSelected(); // Optional: focus on it
        };
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
                    item.ObjectView(autoUpdate != AutoUpdateMode.OFF);
                    
                    continue;
                }
                // Hide all
                item.TurnOff();
            }
        }

        // Used to turn on the new tilemap and select it from editor calls
        if (delayedSelectTileMap != null)
        EditorApplication.delayCall += () => {
            Debug.Log("Delayed set as active from ToggleVisability");
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
