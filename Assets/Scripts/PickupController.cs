using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpController : MonoBehaviour
{
    [SerializeField] private bool showColliderBox = false; 
    [SerializeField] private GameObject colliderShowPrefab; 
    public Mock Mockup { get; set; } = null;

    private LayerMask doorLayerMask;
    private LayerMask wallAndDoorLayerMask;
    private LayerMask stairLayerMasks;
    public List<Collider> Walls { get; set; } = new List<Collider>();

    private WaitForSeconds wait = new WaitForSeconds(0.1f);
    private Vector3 pickupBoxExtents = new Vector3(0.43f, 0.43f, 0.43f);

    private void Start()
    {
        doorLayerMask = LayerMask.GetMask("Door");
        wallAndDoorLayerMask = LayerMask.GetMask("Wall") | doorLayerMask;
        stairLayerMasks = LayerMask.GetMask("StairUp") | LayerMask.GetMask("StairDown");
    }

    private void OnTriggerExit(Collider other)
    {
        if(Walls.Contains(other))
        { 
            Walls.Remove(other); 
        }
        if (Walls.Count == 0)
            ShowEmptyWallUI();
        else {
            ShowLastWallInUI();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!Walls.Contains(other)) {
            Walls.Add(other);
        }
        ShowLastWallInUI(); 
    }

    private void ShowEmptyWallUI() => WallPickupTextManager.Instance.SetText("");
    private void ShowLastWallInUI() => WallPickupTextManager.Instance.SetText(Walls[Walls.Count - 1].name + "" + (Walls.Count));


    internal bool IsTileFree(Vector3 playerPosition, Vector3 target)
    {
        Debug.Log("Trying to find walls at target " + target);

        Vector3 directionVector = playerPosition - target;
        Debug.Log("Direction is " + directionVector);

        Vector3 overlapBoxPosition = target + directionVector * 0.1f;

        // Get the direction the player are and move the overlapBox in that direction
        Collider[] colliders = Physics.OverlapBox(overlapBoxPosition, pickupBoxExtents, Quaternion.identity,wallAndDoorLayerMask);

        if(showColliderBox) {
            GameObject tempBox = Instantiate(colliderShowPrefab);
            tempBox.transform.position = overlapBoxPosition;
        }
        

        if (colliders.Length > 0) {
            Debug.Log("Collider in that direction " + colliders[0].name, colliders[0].gameObject);
            return false;
        }
        return true;
    }

    internal bool IsStair(Vector3 target, out Vector3 newTarget)
    {
        newTarget = target;
        // Get the direction the player are and move the overlapBox in that direction
        Collider[] colliders = Physics.OverlapBox(target, pickupBoxExtents, Quaternion.identity, stairLayerMasks);

        if (colliders.Length > 0) { 
            if (colliders[0].gameObject.layer == LayerMask.NameToLayer("StairUp")) { // Stair UP
                Debug.Log("Stair UP");
                newTarget = target + colliders[0].transform.right * 2f + Vector3.up;
            }
            else { // Stair Down
                Debug.Log("Stair DOWN");
                newTarget = target - colliders[0].transform.right * 2f + Vector3.down;
            }
            return true;
        }        
        return false;
    }
}
