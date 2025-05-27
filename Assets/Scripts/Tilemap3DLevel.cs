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

    public void ActivateFloorTilemap(bool setActive = true) => floorTilemap3D.gameObject.SetActive(setActive);
    public void ActivateItemsTilemap(bool setActive = true) => itemsTilemap3D.gameObject.SetActive(setActive);

    public void SetViewModeForAllMaps(ViewMode activeViewMode)
    {
        // Set the Map and View mode and send back the tilemap to set active
        floorTilemap3D.SetMode(activeViewMode);
        itemsTilemap3D.SetMode(activeViewMode);
    }

    public void InitiateForGameplay()
    {
        floorTilemap3D.ObjectView();
        itemsTilemap3D.ObjectView();
    }

}
