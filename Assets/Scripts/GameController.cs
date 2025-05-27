using System;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    // Find all tileObjects and set them visible

    void Start()
    {
        ActiveteAllTileObjects();                    
    }

    private void ActiveteAllTileObjects()
    {
        Tilemap3DLevel[] tilemapLevels = GetComponentsInChildren<Tilemap3DLevel>(true).ToArray();

        Debug.Log("Found TilemapLevels: "+tilemapLevels.Length);

        foreach (var tilemap in tilemapLevels) {
            tilemap.gameObject.SetActive(true);
            tilemap.InitiateForGameplay();
        } 
    }
}
