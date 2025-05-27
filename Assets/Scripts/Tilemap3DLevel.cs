using UnityEngine;

public class Tilemap3DLevel : MonoBehaviour
{
    [SerializeField] private Tilemap3D floorTilemap3D; 
    [SerializeField] private Tilemap3D itemsTilemap3D;

    public Tilemap3D Tilemap3DFloor => floorTilemap3D;
    public Tilemap3D Tilemap3DItems => itemsTilemap3D;

    public Tilemap3D GetNextTilemap(Tilemap3D active)
    {
        if(active == floorTilemap3D)
            return itemsTilemap3D;
        return floorTilemap3D;
    }

public Tilemap3D SetViewAndMapMode(ViewMode activeViewMode, MapMode activeMapMode)
    {
        // Set the Map and View mode and send back the tilemap to set active
        floorTilemap3D.SetMode(activeViewMode);
        itemsTilemap3D.SetMode(activeViewMode);

        // Fix later - set tio level tilemap for now when any
        switch (activeMapMode) {
            case MapMode.Floor:
                Debug.Log("Activating floor, deactivating items");
                floorTilemap3D.gameObject.SetActive(true);
                itemsTilemap3D.gameObject.SetActive(false);
                return floorTilemap3D;
            case MapMode.Items:
                Debug.Log("Activating items, deactivating floor");
                floorTilemap3D.gameObject.SetActive(false);
                itemsTilemap3D.gameObject.SetActive(true);
                return itemsTilemap3D;
            case MapMode.Any:
                Debug.Log("Activating Both items and floor");
                floorTilemap3D.gameObject.SetActive(true);
                itemsTilemap3D.gameObject.SetActive(true);
                return floorTilemap3D;
            default:
                return floorTilemap3D;
        }
    }

    public void InitiateForGameplay()
    {
        floorTilemap3D.ObjectView();
        itemsTilemap3D.ObjectView();
    }

}
