using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utilities
{
    public static class TilemapExtensions
    {
        public static Vector3Int[] GetTilePositions(this Tilemap tilemap)
        {
            List<Vector3Int> tilePositions = new List<Vector3Int>();
            for (int y = tilemap.origin.y; y < (tilemap.origin.y + tilemap.size.y); y++) {
                for (int x = tilemap.origin.x; x < (tilemap.origin.x + tilemap.size.x); x++) {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile<TileBase>(pos);
                    if (tile != null) {
                        tilePositions.Add(pos);
                    }
                }
            }
            return tilePositions.ToArray();
        }
        public static T[] GetTiles<T>(this Tilemap tilemap) where T : TileBase
        {
            List<T> tiles = new List<T>();

            for (int y = tilemap.origin.y; y < (tilemap.origin.y + tilemap.size.y); y++) {
                for (int x = tilemap.origin.x; x < (tilemap.origin.x + tilemap.size.x); x++) {
                    T tile = tilemap.GetTile<T>(new Vector3Int(x, y, 0));
                    if (tile != null) {
                        tiles.Add(tile);
                    }
                }
            }
            return tiles.ToArray();
        }
}
}
