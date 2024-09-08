using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// The Hex class defines the grid position, world space position, size, 
/// neighbours, etc... of a Hex Tile. However, it does NOT interact with
/// Unity directly in any way.
/// </summary>
public class Hex {

    public Hex(HexMap hexMap, int q, int r)
    {
        this.HexMap = hexMap;

        this.Q = q;
        this.R = r;
        this.S = -(q + r);
    }

    // Q + R + S = 0
    // S = -(Q + R)

    public readonly int Q;  // Column
    public readonly int R;  // Row
    public readonly int S;

    // Data for map generation and maybe in-game effects
    public float Elevation;
    public float Moisture;
    public float Vegetation;

    // Marks a hex as being a lake tile if elevation < flat
    public bool isLake;

    HashSet<Unit> units;


    public readonly HexMap HexMap;

    static readonly float WIDTH_MULTIPLIER = Mathf.Sqrt(3) / 2;

    float radius = 1f;

    /// <summary>
    /// Returns the world-space position of this hex
    /// </summary>
    public Vector3 Position()
    {
        return new Vector3(
            HexHorizontalSpacing() * (this.Q + this.R/2f),
            0,
            HexVerticalSpacing() * this.R
        );
    }

    public float HexHeight()
    {
        return radius * 2;
    }

    public float HexWidth()
    {
        return WIDTH_MULTIPLIER * HexHeight();
    }

    public float HexVerticalSpacing()
    {
        return HexHeight() * 0.75f;
    }

    public float HexHorizontalSpacing()
    {
        return HexWidth();
    }

    public Vector3 PositionFromCamera() 
    {
        return HexMap.GetHexPosition(this);
    }

    public Vector3 PositionFromCamera(Vector3 cameraPosition, float NumRows, float NumColumns)
    {
        float mapHeight = NumRows * HexVerticalSpacing();
        float mapWidth  = NumColumns * HexHorizontalSpacing();

        Vector3 position = Position();


        float howManyWidthsFromCamera = (position.x - cameraPosition.x) / mapWidth;

        // We want howManyWidthsFromCamera to be between -0.5 to 0.5
        if(howManyWidthsFromCamera > 0)
            howManyWidthsFromCamera += 0.5f;
        else
            howManyWidthsFromCamera -= 0.5f;

        int howManyWidthToFix = (int)howManyWidthsFromCamera;

        position.x -= howManyWidthToFix * mapWidth;
        

        return position;
    }

    public static float Distance(Hex a, Hex b)
    {
        // Calculate the differences in Q, R, and S
        int dQ = Mathf.Abs(a.Q - b.Q);
        int dR = Mathf.Abs(a.R - b.R);
        int dS = Mathf.Abs(a.S - b.S);

        // Handle wrapping for Q
        if (dQ > a.HexMap.NumColumns / 2)
        {
            dQ = a.HexMap.NumColumns - dQ;
        }

        // The hex distance is the maximum of the differences
        return Mathf.Max(dQ, dR, dS);
    }

    public void AddUnit(Unit unit) 
    {
        if (units == null) 
        {
            units = new HashSet<Unit>();
        }

        units.Add(unit);
    }

    public void RemoveUnit(Unit unit) 
    {
        if (units != null) 
        {
            units.Remove(unit);
        }
    }

    public Unit[] Units() {
        return units.ToArray();
    }
}