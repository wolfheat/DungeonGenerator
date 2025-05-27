using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

//public enum ViewMode{OFF,TileMap,Objects }
public enum AutoUpdateMode { OFF, ON, INSTANT, INSTANTFAST }
public enum ViewMode { Objects, TileMap}
public enum MapMode { Floor, Items, Any }


public class Tilemap3D : MonoBehaviour
{

    // If link data is set use that instead of the sprite and objects arrays
    [SerializeField] private Tilemap3DLinksData linkData;

    [SerializeField] private GameObject[] TileObjects;
    [SerializeField] private GameObject[] DoorObjects;

    [SerializeField] private Sprite[] TileSprites;
    [SerializeField] private Sprite[] DoorSprites;

    [SerializeField] private GameObject objectHolder;
    [SerializeField] private GameObject transparentBackground;
    [SerializeField] private Renderer renderer;

    [SerializeField] private bool itemsTilemap = false;
    //[SerializeField] private bool autoUpdate = false;

    public int GetObjectsAmount => objectHolder.transform.childCount;
    public GameObject GetHolder => objectHolder;
    public GameObject GetTransBackground => transparentBackground;

    public ViewMode Mode { get; set; } = ViewMode.Objects;
    public AutoUpdateMode UpdateMode { get; set; } = AutoUpdateMode.OFF;
    public bool HoldsItems => itemsTilemap;



    public void SetMode(ViewMode mode)
    {
        switch (mode) {
            case ViewMode.Objects:
                ObjectView();
                break;
            case ViewMode.TileMap:
                TileMapView();
                break;
        }
    }
    public void TileMapView() 
    {
        Mode = ViewMode.TileMap;
        // Show TileMap
        objectHolder.SetActive(false);
        renderer.enabled = true;
        transparentBackground.SetActive(true);
    }
    
    public void ObjectView(bool autoUpdate = true) 
    {
        if (autoUpdate)
            Generate3DTilesForced();

        Mode = ViewMode.Objects;
        // Show Tile Objects
        renderer.enabled = false;
        transparentBackground.SetActive(false);
        objectHolder.SetActive(true);
    }
        

    private Vector3 offset = new Vector3(0.5f,0f,0.5f);
    Tilemap tilemap;

    private bool needsUpdate = false;

    [ContextMenu("Generate 3D tiles")]
    public void Generate3DTiles()
    {
        //Debug.Log("Use the 2D tilemap to generate 3D tiles on it");

        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        CreateAllObjectsFromSprites();
    }
    

    [ContextMenu("Generate 3D tiles - Forced Mode")]
    public void Generate3DTilesForced()
    {   
        if(tilemap == null)
            tilemap = GetComponent<Tilemap>();

        RemoveAllTiles();

        CreateAllObjectsFromSprites();        
    }

    internal void Generate3DTilesForcedSpecific(List<Vector2Int> changedPositions)
    {
        if (tilemap == null)
            tilemap = GetComponent<Tilemap>();

        Transform[] objects = GetObjectsAt(changedPositions);

        //Debug.Log("Generate Only affected tiles");
        for (int i = 0; i < changedPositions.Count; i++) {
            
            Vector2Int pos = changedPositions[i];

            Debug.Log("Change at: "+pos);
            Sprite sprite = tilemap.GetSprite(new Vector3Int(pos.x,pos.y,0));
            // Find the current object and delete it
            if (objects[i] != null) {
                DestroyImmediate(objects[i].gameObject);
            }

            if (sprite != null) {
                // There is a sprite here get its index = type
                int index = Array.IndexOf(TileSprites,sprite);
                //Debug.Log("Update or create object ID: "+index+" at "+pos);

                // Create an Object of that type
                float rotation = -tilemap.GetTransformMatrix(new Vector3Int(pos.x,pos.y,0)).rotation.eulerAngles.z;

                Vector3Int XZPosition = new Vector3Int(pos.x, 0, pos.y);
                //Debug.Log("Changing tilePosition "+pos+" to dungeon position "+XZPosition);
                CreateObjectFromSprite(sprite, XZPosition, rotation);

            }
        }    

    }

    private Transform[] GetObjectsAt(List<Vector2Int> pos)
    {
        Transform[] objectTransforms = new Transform[pos.Count];

        foreach (Transform child in objectHolder.transform) {
            Vector3 posAdjusted = child.position-offset;
            Vector2Int posInt = new Vector2Int(Mathf.RoundToInt(posAdjusted.x),Mathf.RoundToInt(posAdjusted.z));
            int index = pos.IndexOf(posInt);
            if (index != -1) {
                objectTransforms[index] = child;
            }
        }
        return objectTransforms;
    }

    private void CreateAllObjectsFromSprites()
    {
        // Find all tiles and make objects from them
        TileBase[] allTiles = tilemap.GetTiles<TileBase>();
        Vector3Int[] allTilesPositions = tilemap.GetTilePositions();
        int level = Mathf.RoundToInt(tilemap.transform.localPosition.y);

        foreach (Vector3Int pos in allTilesPositions) {
            Sprite sprite = tilemap.GetSprite(pos);
            if (sprite == null) continue;

            float rotation = -tilemap.GetTransformMatrix(pos).rotation.eulerAngles.z;

            Vector3Int XZPosition = new Vector3Int(pos.x, 0, pos.y);
            //Debug.Log("Changing tilePosition "+pos+" to dungeon position "+XZPosition);
            CreateObjectFromSprite(sprite, XZPosition, rotation);
        } 
    }

    [ContextMenu("Destroy All 3D tiles")]
    private void RemoveAllTiles()
    {
        int childrens = objectHolder.transform.childCount;
        for (int i = childrens - 1; i >= 0; i--) {
            Transform child = objectHolder.transform.GetChild(i);
            if (child == this.transform) {
                Debug.Log("Skipping parent when removing children");
            } 
#if UNITY_EDITOR
            DestroyImmediate(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }
    }

    private void CreateObjectFromSprite(Sprite sprite, Vector3Int pos, float YRotation = 0)
    {
        try {
            // This uses the index the sprite has in the array and finds the corresponding index in the objects array to map the tile
            int spriteIndex = Array.IndexOf(TileSprites, sprite);


            GameObject prefab;

            if (linkData != null) {
                // There is a Scriptable object of tyoe link Data assigned, use that for getting the corresponding objects
                prefab = linkData.GetLinkObject(sprite);
                //Debug.Log("Link object received from sprite: "+sprite.name);
            }
            else {
                prefab = GetIndexCorrespondingObjectPrefab(sprite);
            }

            if (prefab == null) {
                Debug.Log("Could not find an defined object for the sprite: "+sprite.name);
                return;
            }

            GameObject tile = Instantiate(prefab, objectHolder.transform);
            tile.transform.SetLocalPositionAndRotation(pos + offset, Quaternion.Euler(0, YRotation, 0));
        }
        catch (Exception e) {
            Debug.Log("Could not create Item: "+e.Message);
        }
    }

    private GameObject GetIndexCorrespondingObjectPrefab(Sprite sprite)
    {
        // Check if in Tiles array
        int spriteIndex = Array.IndexOf(TileSprites, sprite);

        if (spriteIndex == -1) {
            
            // Not in tiles array check if in Doors Array
            spriteIndex = Array.IndexOf(DoorSprites, sprite);

            if (spriteIndex == -1) {
                // Not in any array
                return null;
            }
            return DoorObjects[spriteIndex];
        }
        return TileObjects[spriteIndex];        
    }

    private void OnGUI()
    {
        Renderer renderer = this.GetComponent<Renderer>();
        // Make sure the level is set to the Z value
        if(renderer != null && renderer.sortingOrder != (int)transform.position.y)
            renderer.sortingOrder = (int)transform.position.y;

    }

    private void OnEnable()
    {
        Debug.Log("Onenable");
        //SetCurrentMode();        
    }

    public void TurnOff()
    {
        gameObject.SetActive(false);
    }

}
