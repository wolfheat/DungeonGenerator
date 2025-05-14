using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

public class Tilemap3D : MonoBehaviour
{

    [SerializeField] private GameObject[] TileObjects;
    [SerializeField] private GameObject[] DoorObjects;

    [SerializeField] private Sprite[] TileSprites;
    [SerializeField] private Sprite[] DoorSprites;

    [SerializeField] private GameObject objectHolder;
    //[SerializeField] private bool autoUpdate = false;

    public GameObject GetHolder => objectHolder;

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

            int spriteIndex = Array.IndexOf(TileSprites, sprite);
            GameObject[] array = TileObjects;

            if (spriteIndex == -1) {
                Debug.Log("Creating door object at "+pos+" sprite: "+sprite.name);
                // Not in 
                spriteIndex = Array.IndexOf(DoorSprites, sprite);
                array = DoorObjects;
                if (spriteIndex == -1) {
                    Debug.Log("Could not find Sprite in any sprite array");
                    return;
                }
            }

            GameObject tileObject = array[spriteIndex];

            GameObject tile = Instantiate(tileObject, objectHolder.transform);
            tile.transform.SetLocalPositionAndRotation(pos + offset, Quaternion.Euler(0, YRotation, 0));
        }
        catch (Exception e) {
            Debug.Log("Could not create Item: "+e.Message);
        }
    }

}
