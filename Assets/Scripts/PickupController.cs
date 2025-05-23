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
    public List<Collider> Walls { get; set; } = new List<Collider>();

    private WaitForSeconds wait = new WaitForSeconds(0.1f);

    private void Start()
    {
        doorLayerMask = LayerMask.GetMask("Door");
        wallAndDoorLayerMask = LayerMask.GetMask("Wall") | doorLayerMask;
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
        Collider[] colliders = Physics.OverlapBox(overlapBoxPosition, new Vector3(0.4f, 0.4f, 0.4f),Quaternion.identity,wallAndDoorLayerMask);

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
}
