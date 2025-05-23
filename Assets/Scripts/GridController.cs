using UnityEngine;

public class GridController : MonoBehaviour
{
    private Vector3 playModeGridOffset = new Vector3 (-0.5f, -0.5f, -0.5f);
    
    void Start()
    {
        transform.position = playModeGridOffset;    
    }
}
