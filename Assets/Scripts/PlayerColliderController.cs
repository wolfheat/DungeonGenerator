using UnityEngine;

public class PlayerColliderController : MonoBehaviour
{
    [SerializeField] PlayerController playerController;

    public static bool IsPlayerInRegainArea = false;

    LayerMask itemsLayerMask;

    private void Start()
    {
        itemsLayerMask = LayerMask.GetMask("Items", "ItemsSeeThrough");
    }

    private void OnTriggerExit(Collider other)
    {
    }

    private void OnTriggerEnter(Collider other)
    {
       
    }

}
