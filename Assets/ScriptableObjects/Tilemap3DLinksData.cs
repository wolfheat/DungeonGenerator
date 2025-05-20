using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "/3DData/LinkData" ,menuName = "Tilemap3DLinkData")]
public class Tilemap3DLinksData : ScriptableObject 
{
    public List<Tilemap3DLink> links;

    internal GameObject GetLinkObject(Sprite sprite)
    {
        // Use this to get the correcponding object using a sprite
        foreach (var link in links) {
            if (link.sprite == sprite) {
                return link.gameObject;
            }
        }
        return null;
    }
}
