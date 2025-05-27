using System;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private Vector3 playModeGridOffset = new Vector3(-0.5f, -0.5f, -0.5f);

    public Vector3 GridOffset => playModeGridOffset;
    // Find all tileObjects and set them visible


    public static GameController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    void Start()
    {
        transform.position = playModeGridOffset;
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
