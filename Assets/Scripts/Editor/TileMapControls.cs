using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.Tilemaps;
using System;
using System.Collections.Generic;
using Codice.Client.BaseCommands;


public class TileMapControls : EditorWindow
{
    public static AutoUpdateMode autoUpdate = AutoUpdateMode.OFF;
    public static ViewMode activeViewMode = ViewMode.Objects;
    public static MapMode activeMapMode = MapMode.Floor;

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

    private int[] GetTotalObjects()
    {
        Tilemap3D[] tilemaps = GetAllTileMap3D();
        List<int> amt = new();
        for (int i = 0; i < tilemaps.Length; i++) {
            Tilemap3D tileMap = tilemaps[i];
            if (tileMap.HoldsItems)
                continue;
            amt.Add(tileMap.GetHolder.transform.childCount);
        }
        return amt.ToArray();
    }

    private Tilemap3D GetActiveLayer()
    {
        Tilemap3D[] tilemaps = GetAllTileMap3D();

        if (GridPaintingState.scenePaintTarget != null && GridPaintingState.scenePaintTarget.TryGetComponent(out Tilemap3D active)) {
            for (int i = 0; i < tilemaps.Length; i++) {
                if (tilemaps[i] == active) {
                    //Debug.Log("The active tilemap is number "+i); 
                    return active;
                }
            }   
        }
        return null;
        //Selection.activeGameObject = delayedSelectTileMap.gameObject;
        //GridPaintingState.scenePaintTarget = delayedSelectTileMap.gameObject;
    }
    
    private int GetActiveLayerIndex()
    {
        Tilemap3D[] tilemaps = GetAllTileMap3D();

        if (GridPaintingState.scenePaintTarget != null && GridPaintingState.scenePaintTarget.TryGetComponent(out Tilemap3D active)) {
            for (int i = 0; i < tilemaps.Length; i++) {
                if (tilemaps[i] == active) {
                    //Debug.Log("The active tilemap is number "+i); 
                    return i;
                }
            }   
        }
        return 0;
        //Selection.activeGameObject = delayedSelectTileMap.gameObject;
        //GridPaintingState.scenePaintTarget = delayedSelectTileMap.gameObject;
    }

    // Toggle modes
    private void ToggleMapMode()
    {
        Debug.Log("Toggel Map mode from "+activeMapMode);

        int enumLength = Enum.GetValues(typeof(MapMode)).Length;
        int nextMode = ((int)activeMapMode + 1) % enumLength;
        activeMapMode = (MapMode)nextMode;

        // Send info to all tilemap3d instances so they update themself
        SetMapMode(activeMapMode);
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


    private void SetMapMode(MapMode activeMapMode)
    {
        Debug.Log("Set map Mode to "+activeMapMode);

    }

    // Set UpdateMode for all Tilemap3ds
    private void SetViewMode(ViewMode mode)
    {
        Debug.Log("Set view Mode to "+mode);
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
    private void ToggleActive(int layer)
    {
        Debug.Log("Set layer "+layer+" as active! view type = "+activeViewMode+" and map type = "+activeMapMode);

        Tilemap3DLevel[] levels = GetAllTilemapLevels();

        // To begin with make just the selected layer show??

        
        // Defines what tilemap to set as active
        Tilemap3D delayedSelectTileMap = null;


        foreach (var item in levels) {
            Debug.Log("Item position = "+ Mathf.RoundToInt(item.transform.position.y));
            if(Mathf.RoundToInt(item.transform.position.y) == -layer) {
                if(!item.gameObject.activeSelf)
                    item.gameObject.SetActive(true);
                delayedSelectTileMap = item.SetViewAndMapMode(activeViewMode, activeMapMode);
                Debug.Log("layer "+layer+" set as active");
            }
            else {
                // Hide it if not the active one
                item.gameObject.SetActive(false);
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
        int[] objectsAmount = GetTotalObjects();

        int activeLayer = GetActiveLayerIndex();

        Paint_GUIHeader();

        Paint_ModesButtons();



        // Header Row
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level", CenterLabelStyle(), GUILayout.Width(150));
        GUILayout.Label("Active Mode", CenterLabelStyle(), GUILayout.Width(100));
        GUILayout.Label("Objects", CenterLabelStyle(), GUILayout.Width(100));
        GUILayout.Label("Active", CenterLabelStyle(), GUILayout.Width(70));
        GUILayout.EndHorizontal();


        // Settings View

        // Object/Tilemaps - Level/Items


        // NEW BUTTON SYSTEM

        // BUTTON <- activates this layer, objects or tile view set on top, and Items or base level 


        // Data Rows
        for (int i = 0; i < objectsAmount.Length; i++) {
            GUILayout.BeginHorizontal();

            // Button Part
            if (GUILayout.Button("Toggle Level " + (-i), GUILayout.Width(150))) {
                ToggleVisability(-i);
            }

            // Mode Part
            if (modes.Length >= 1) GUILayout.Label("" + modes[i], CenterLabelStyle(), GUILayout.Width(100));

            // Objects Part
            if (modes.Length >= 1) GUILayout.Label("" + objectsAmount[i], CenterLabelStyle(), GUILayout.Width(100));

            // Active part
            if (GUILayout.Button(activeLayer == i ? "ACTIVE" : "---", GUILayout.Width(70))) {
                // Request this layer to become active
                ToggleActive(i);
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

        GUILayout.BeginHorizontal();
        GUILayout.Label("Map mode: ", GUILayout.Width(80));
        
        // Map Mode 
        if (GUILayout.Button("" + activeMapMode, GUILayout.Width(100))) {
            ToggleMapMode();
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
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
