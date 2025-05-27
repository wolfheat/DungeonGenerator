using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class EnhancedRuleTile : RuleTile<EnhancedRuleTile.Neighbor> {
    public bool customField;
    public TileBase[] solidTiles;
    public TileBase[] hollowTiles;

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Solid = 3;
        public const int Hollow = 4;
        public const int Null = 5;
        public const int NotNull = 6;   
    }

    public override bool RuleMatch(int neighbor, TileBase tile) {
        switch (neighbor) {
            case Neighbor.Solid: return EvaluateSolid(tile);
            case Neighbor.Hollow: return EvaluateHollow(tile);
            case Neighbor.Null: return tile == null;
            case Neighbor.NotNull: return tile != null;
        }
        return base.RuleMatch(neighbor, tile);
    }

    private bool EvaluateSolid(TileBase tile)
    {
        return solidTiles.Contains(tile);
    }

    private bool EvaluateHollow(TileBase tile)
    {
        return hollowTiles.Contains(tile);
    }
}