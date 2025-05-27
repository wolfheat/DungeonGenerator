using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Tilemaps;
using System;


public class TileMapControls : EditorWindow
{
    public static AutoUpdateMode autoUpdate = AutoUpdateMode.OFF;
    public static ViewMode activeViewMode = ViewMode.Objects;
    //public static MapMode activeMapMode = MapMode.Floor;
    public static bool toggleState = true;

    public static TileMapControls Instance;
    //public static AutoUpdateMode ActiveAutoUpdateMode => Instance.autoUpdate;

    [MenuItem("Window/TileMapControls")]
    public static void ShowWindow()
    {
        TileMapControls example = GetWindow<TileMapControls>("Tile Map Controls");
        Instance = GetWindow<TileMapControls>("Tile Map Controls");
    }

    private void ForceUpdate()
    {
        Tilemap3D activeLayer = GetActiveLayer();
        activeLayer.Generate3DTilesForced();
    }

    // GUI Settings STYLE
    private GUIStyle CenterLabelStyle()
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    }

    private int[][] GetTotalObjects(Tilemap3DLevel[] tilemapsLevels)
    {
        int[][] ans = new int[tilemapsLevels.Length][];

        for (int i = 0; i < tilemapsLevels.Length; i++) {
            ans[i] = new int[2];
            ans[i][0] = tilemapsLevels[i].Tilemap3DFloor.GetObjectsAmount;
            ans[i][1] = tilemapsLevels[i].Tilemap3DItems.GetObjectsAmount;
        }
        return ans;
    }

    private Tilemap3D GetActiveLayer()
    {
        Tilemap3D[] tilemaps = GetAllTileMap3D();

        if (GridPaintingState.scenePaintTarget != null && GridPaintingState.scenePaintTarget.TryGetComponent(out Tilemap3D active)) {
            for (int i = 0; i < tilemaps.Length; i++) {
                if (tilemaps[i] == active) {
                    return active;
                }
            }   
        }
        return null;
    }

    private int[] GetActiveLayerIndex() => GetActiveLayerIndex(GetAllTilemapLevels());

    private int[] GetActiveLayerIndex(Tilemap3DLevel[] tilemap3DLevels)
    {        
        if (GridPaintingState.scenePaintTarget != null && GridPaintingState.scenePaintTarget.TryGetComponent(out Tilemap3D active)) {
            for (int i = 0; i < tilemap3DLevels.Length; i++) {
                if (tilemap3DLevels[i].Tilemap3DFloor == active) {
                    return new int[] { i, 0 };
                }
                if (tilemap3DLevels[i].Tilemap3DItems == active) {
                    return new int[] { i, 1 };
                }
            }   
        }
        return new int[] {0,0};
    }
    
    private bool[][] GetActiveLayersBools(Tilemap3DLevel[] tilemap3DLevels)
    {
        bool[][] ans = new bool[tilemap3DLevels.Length][];
        for (int i = 0; i < tilemap3DLevels.Length; i++) {
            ans[i] = new bool[2];
            ans[i][0] = tilemap3DLevels[i].Tilemap3DFloor.gameObject.activeSelf;
            ans[i][1] = tilemap3DLevels[i].Tilemap3DItems.gameObject.activeSelf;
        }
        return ans;
    }

    private void ToggleViewMode()
    {
        Debug.Log("Toggel View mode from "+activeViewMode);

        int enumLength = Enum.GetValues(typeof(ViewMode)).Length;
        int nextMode = ((int)activeViewMode + 1) % enumLength;
        activeViewMode = (ViewMode)nextMode;

        // Send info to all tilemap3d instances so they update themself
        SetViewMode(activeViewMode);
    }

    private void ToggleAutoUpdate()
    {
        int enumLength = Enum.GetValues(typeof(AutoUpdateMode)).Length;
        int nextMode = ((int)autoUpdate + 1) % enumLength;
        autoUpdate = (AutoUpdateMode)nextMode;

        // Send info to all tilemap3d instances so they update themself
        SetAllAutoUpdate(autoUpdate);
    }

    // Set UpdateMode for all Tilemap3ds
    private void SetViewMode(ViewMode mode)
    {
        Debug.Log("Set view Mode to "+mode);
        int[] active = GetActiveLayerIndex();
        UpdateAllObjectsAndTilemapsView();
        //ToggleActive(-active[0]);
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

    private Tilemap3DLevel[] GetAllTilemapLevels()
    {
        return Resources.FindObjectsOfTypeAll<Tilemap3DLevel>()
            .Where(go =>
                !EditorUtility.IsPersistent(go) &&            // Excludes prefab assets
                go.hideFlags == HideFlags.None)
            // &&             // Excludes hidden objects like gizmo handles
                                               //go.scene.IsValid())                           // Ensures it's part of a loaded scene
            .OrderByDescending(x => x.transform.position.y).ToArray();
    }

    private ViewMode[] GetNonItemsMapModes()
    {
        return Resources.FindObjectsOfTypeAll<Tilemap3D>()
            .Where(go =>
                !EditorUtility.IsPersistent(go) &&            // Excludes prefab assets
                go.hideFlags == HideFlags.None &&
                go.HoldsItems)
            // &&             // Excludes hidden objects like gizmo handles
                                               //go.scene.IsValid())                           // Ensures it's part of a loaded scene
            .OrderByDescending(x => x.transform.position.y)
            .Select(x => x.Mode).ToArray();
    }


    private void UpdateAllObjectsAndTilemapsView()
    {
        // Settings for viewMode or MapMode has changed 
        // Update all these visuals to reflect this

        Tilemap3DLevel[] levels = GetAllTilemapLevels();

        foreach (var level in levels) 
            level.SetViewModeForAllMaps(activeViewMode);

        // Force another gui update
        Repaint();
    }

    private void ToggleActive(int layer, int mapType)
    {
        
        Debug.Log("Layer: ["+layer+","+mapType+"] toggle active!    ViewMode = " + activeViewMode);

        Tilemap3DLevel[] levels = GetAllTilemapLevels();

        // To begin with make just the selected layer show??
                
        // Defines what tilemap to set as active
        Tilemap3D delayedSelectTileMap = null;

        foreach (var level in levels) {
            //Debug.Log("Item position = "+ Mathf.RoundToInt(level.transform.position.y));
            if(Mathf.RoundToInt(level.transform.position.y) == -layer) {
                // Correct layer - activate it if inactive
                if (!level.gameObject.activeSelf) {
                    Debug.Log("ToggleActive - Enable Level");
                    level.gameObject.SetActive(true);
                }

                if (mapType == 0) {
                    if (!level.Tilemap3DFloor.gameObject.activeSelf) {
                        Debug.Log("ToggleActive - Floor enable");
                        level.ActivateFloorTilemap();
                    }
                    delayedSelectTileMap = level.Tilemap3DFloor;
                }
                else {
                    if (!level.Tilemap3DItems.gameObject.activeSelf) {
                        Debug.Log("ToggleActive - Items enable");
                        level.ActivateItemsTilemap();
                    }
                    delayedSelectTileMap = level.Tilemap3DItems;
                }
            }
        }

        // Used to turn on the new tilemap and select it from editor calls
        if (delayedSelectTileMap != null)
            EditorApplication.delayCall += () => {
                Debug.Log("Delayed set as active from ToggleActive");
                //Selection.activeGameObject = delayedSelectTileMap.gameObject;
                GridPaintingState.scenePaintTarget = delayedSelectTileMap.gameObject;
                //SceneView.lastActiveSceneView.FrameSelected(); // Optional: focus on it
            };
    }


    // Toggles whats visible for a certain layer
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
                if (!item.gameObject.activeSelf) {
                    item.gameObject.SetActive(true);
                    item.TileMapView();
                    delayedSelectTileMap = item; // Used to turn on the new tilemap and select it from editor calls
                    continue;
                }
                else if (item.Mode == ViewMode.TileMap) {
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


    private void OnGUI()
    {
        SetAllAutoUpdate(autoUpdate);

        // Get all the levels
        Tilemap3DLevel[] tilemap3DLevels = GetAllTilemapLevels();

        // Get Main Level and Items separately


        ViewMode[] modes = GetNonItemsMapModes();

        // Check that this is doing what it should
        int[][] objectsAmount = GetTotalObjects(tilemap3DLevels);

        int[] activeTilemap = GetActiveLayerIndex(tilemap3DLevels);

        bool[][] activeLayerBools = GetActiveLayersBools(tilemap3DLevels);

        //int activeLayer = GetActiveLayerIndex();

        Paint_GUIHeader();

        Paint_ModesButtons();

        // Header Row
        GUILayout.BeginHorizontal();
        GUILayout.Label("", CenterLabelStyle(), GUILayout.Width(15));
        GUILayout.Label("Level", CenterLabelStyle(), GUILayout.Width(150));
        GUILayout.Label("Objects Amt", CenterLabelStyle(), GUILayout.Width(100));
        GUILayout.Label("Floor", CenterLabelStyle(), GUILayout.Width(70));
        GUILayout.Label("Items", CenterLabelStyle(), GUILayout.Width(130));
        GUILayout.EndHorizontal();

        // Settings View

        // Object/Tilemaps - Level/Items

        // NEW BUTTON SYSTEM

        // BUTTON <- activates this layer, objects or tile view set on top, and Items or base level 


        // Data Rows
        for (int i = 0; i < objectsAmount.Length; i++) {
            GUILayout.BeginHorizontal();


            bool oldToggleState = tilemap3DLevels[i].gameObject.activeSelf;
            // Check Box
            bool newToggleState = GUILayout.Toggle(oldToggleState, "", GUILayout.Width(15));

            tilemap3DLevels[i].gameObject.SetActive(newToggleState);

            // Button Part
            if (GUILayout.Button("Toggle Level " + (-i), GUILayout.Width(150))) {
                ToggleVisability(-i);
            }

            // Mode Part
            //if (modes.Length >= 1) GUILayout.Label("" + modes[i], CenterLabelStyle(), GUILayout.Width(100));

            // Objects Amount Part
            if (modes.Length >= 1) GUILayout.Label(objectsAmount[i][0] + " + " + objectsAmount[i][1], CenterLabelStyle(), GUILayout.Width(100));


            // Floor and Items
            // Floor
            if (GUILayout.Button(activeTilemap[0] == i && activeTilemap[1] == 0 ? "[FLOOR]" : (activeLayerBools[i][0] ? "floor":""), GUILayout.Width(70))) 
            {            
                // Request this layer to become active
                ToggleActive(i,0);
            }


            // Spacer
            GUILayout.Label("", CenterLabelStyle(), GUILayout.Width(10));

            // Checkbox here
            bool oldItemState = tilemap3DLevels[i].Tilemap3DItems.gameObject.activeSelf;

            // Check Box
            bool itemState = GUILayout.Toggle(oldItemState, "", GUILayout.Width(15));

            tilemap3DLevels[i].Tilemap3DItems.gameObject.SetActive(itemState);

            // Items
            if (GUILayout.Button(activeTilemap[0] == i && activeTilemap[1] == 1 ? "[ITEMS]" : (activeLayerBools[i][1] ? "items" : ""), GUILayout.Width(70))) {
                // Request this layer to become active
                ToggleActive(i,1);
            }

            GUILayout.EndHorizontal();
        }

    }

    private void Paint_ModesButtons()
    {
        // Update Mode toggle
        GUILayout.BeginHorizontal();


        GUILayout.Label("Auto mode: ", GUILayout.Width(80));
        if (GUILayout.Button("" + autoUpdate, GUILayout.Width(100))) {
            ToggleAutoUpdate();
        }

        GUILayout.FlexibleSpace();

        // Force Update Button
        if (GUILayout.Button("Force Update", GUILayout.Width(100))) {
            ForceUpdate();
        }

        GUILayout.EndHorizontal();

        // Object/Tilemaps - Level/Items
        
        // View Mode toggle
        GUILayout.BeginHorizontal();
        GUILayout.Label("View mode: ", GUILayout.Width(80));
        if (GUILayout.Button(""+activeViewMode, GUILayout.Width(100))) {
            ToggleViewMode();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();
        //GUILayout.Label("Map mode: ", GUILayout.Width(80));
        //
        //// Map Mode 
        //if (GUILayout.Button("" + activeMapMode, GUILayout.Width(100))) {
        //    ToggleMapMode();
        //}
        //GUILayout.FlexibleSpace();
        //GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }

    private static void Paint_GUIHeader()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Dungeon Generator Controls", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }
}
