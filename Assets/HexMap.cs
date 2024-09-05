using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GenerateMap();
	}

    public GameObject HexPrefab;

    public Mesh MeshOcean;
    public Mesh MeshLake;
    public Mesh MeshFlat;
    public Mesh MeshHill;
    public Mesh MeshMountain;

    public GameObject ForestPrefab;
    public GameObject JunglePrefab;

    public Material MatOcean;
    public Material MatLake;
    public Material MatPlains;
    public Material MatGrasslands;
    public Material MatMountains;
    public Material MatDesert;

    // Tiles with height above whatever, is a whatever
    [System.NonSerialized] public float HeightMountain = 1f;
    [System.NonSerialized] public float HeightHill = 0.6f;
    [System.NonSerialized] public float HeightFlat = 0.0f;

    [System.NonSerialized] public float MoistureGrasslands = 0f;
    [System.NonSerialized] public float MoisturePlains = -0.5f;
    [System.NonSerialized] public float VegetationJungle = 0.7f;
    [System.NonSerialized] public float VegetationForest = 0.0f;

    [System.NonSerialized] public int NumRows = 30;
    [System.NonSerialized] public int NumColumns = 60;

    // TODO: Link up with the Hex class's version of this
    private Hex[,] hexes;
    private Dictionary<Hex, GameObject> hexToGameObjectMap;

    public Hex GetHexAt(int x, int y)
    {
        if(hexes == null)
        {
            Debug.LogError("Hexes array not yet instantiated.");
            return null;
        }


        x = x % NumColumns;
        if(x < 0)
        {
            x += NumColumns;
        }

        try {
            return hexes[x, y];
        }
        catch
        {
            Debug.LogError("GetHexAt: " + x + "," + y);
            return null;
        }
    }

    virtual public void GenerateMap()
    {
        // Generate a map filled with ocean

        hexes = new Hex[NumColumns, NumRows];
        hexToGameObjectMap = new Dictionary<Hex, GameObject>();

        // Loops through each hex in the hexMap.
        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                // Instantiate a Hex
                Hex h = new Hex( this, column, row );
                h.Elevation = -0.5f;
                h.isLake = false;

                hexes[ column, row ] = h;

                Vector3 pos = h.PositionFromCamera( 
                    Camera.main.transform.position, 
                    NumRows, 
                    NumColumns 
                );


                GameObject hexGO = (GameObject)Instantiate(
                    HexPrefab, 
                    pos,
                    Quaternion.identity,
                    this.transform
                );

                hexToGameObjectMap[h] = hexGO;

                hexGO.name = string.Format("HEX: {0},{1}", column, row);
                hexGO.GetComponent<HexComponent>().Hex = h;
                hexGO.GetComponent<HexComponent>().HexMap = this;

                hexGO.GetComponentInChildren<TextMesh>().text = string.Format("{0},{1}", column, row);
            }
        }

        UpdateHexVisuals();
    }

    public void UpdateHexVisuals()
    {
        // Loops through each hex in the hexMap.
        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex h = hexes[column,row];
                GameObject hexGO = hexToGameObjectMap[h];

                MeshRenderer mr = hexGO.GetComponentInChildren<MeshRenderer>();
                MeshFilter mf = hexGO.GetComponentInChildren<MeshFilter>();

                if(h.Elevation >= HeightFlat && h.Elevation < HeightMountain)
                {
                    if(h.Vegetation >= VegetationJungle && h.Moisture >= MoisturePlains)
                    {
                        // Spawn trees
                        Vector3 p = hexGO.transform.position;
                        if(h.Elevation >= HeightHill)
                        {
                            p.y += 0.25f;
                        }
                        GameObject.Instantiate(JunglePrefab, p, Quaternion.identity, hexGO.transform);
                    }
                    else if(h.Vegetation >= VegetationForest && h.Moisture >= MoisturePlains)
                    {
                        // Spawn trees
                        Vector3 p = hexGO.transform.position;
                        if(h.Elevation >= HeightHill)
                        {
                            p.y += 0.25f;
                        }
                        GameObject.Instantiate(ForestPrefab, p, Quaternion.identity, hexGO.transform);
                    }

                    if(h.Moisture >= MoistureGrasslands)
                    {
                        mr.material = MatGrasslands;
                    }
                    else if(h.Moisture >= MoisturePlains)
                    {
                        mr.material = MatPlains;
                    }
                    else 
                    {
                        mr.material = MatDesert;
                    }
                }

                if(h.Elevation >= HeightMountain)
                {
                    mr.material = MatMountains;
                    mf.mesh = MeshMountain;
                }
                else if(h.Elevation >= HeightHill)
                {
                    mf.mesh = MeshHill;
                }
                else if(h.Elevation >= HeightFlat)
                {
                    mf.mesh = MeshFlat;
                }
                else
                {
                    if (h.isLake) 
                    {
                        mr.material = MatLake;
                        mf.mesh = MeshLake;
                    } else 
                    {
                        mr.material = MatOcean;
                        mf.mesh = MeshOcean;
                    }
                }
            }
        }
    }

    public Hex[] GetHexesInRange(Hex centerHex, int range)
    {
    // Creates a list of hexes in range of the center.
    List<Hex> hexesInRange = new List<Hex>();

    // Loops through every hex within a certain range of the centerHex.
    for (int dx = -range; dx <= range; dx++)
    {
        for (int dy = Mathf.Max(-range, -dx - range); dy <= Mathf.Min(range, -dx + range); dy++)
        {
            int xValue = centerHex.Q + dx;
            int yValue = centerHex.R + dy;

            // Wrap around the x and y values if they go out of bounds
            xValue = (xValue + NumColumns) % NumColumns; // Wrap x within 0 and NumColumns-1
            yValue = (yValue + NumRows) % NumRows; // Wrap y within 0 and NumRows-1

            // Find the hex at the wrapped coordinates
            Hex hex = GetHexAt(xValue, yValue);

            if (hex != null)
            {
                // Add the hex to the array if valid
                hexesInRange.Add(hex);
            }
        }
    }

    return hexesInRange.ToArray();
}

    public bool IfHexTouchesOnlyWater(Hex centerHex) 
    {
        bool onlyTouchesWater = true;

        Hex[] touchingHexes = GetHexesInRange(centerHex, 1);

        foreach(Hex h in touchingHexes)
        {
            if (h.Elevation >= HeightFlat) 
            {
                onlyTouchesWater = false;
            }
        }

        return onlyTouchesWater;
    }

    public bool IfHexTouchesAnyWater(Hex centerHex) 
    {
        bool ifTouchesWater = false;

        Hex[] touchingHexes = GetHexesInRange(centerHex, 1);

        foreach(Hex h in touchingHexes)
        {
            if (h.Elevation < HeightFlat) 
            {
                ifTouchesWater = true;
            }
        }

        return ifTouchesWater;
    }

    public void ElevateArea(int q, int r, int range, float centerHeight = 0.8f)
    {
        Hex centerHex = GetHexAt(q, r);

        Hex[] areaHexes = GetHexesInRange(centerHex, range);

        foreach (Hex h in areaHexes)
        {
            float distance = Hex.Distance(centerHex, h);
            float normalizedDistance = distance / range;

            // Ensure symmetry by normalizing distance within the specified range
            h.Elevation = centerHeight * Mathf.Lerp(1f, 0.25f, Mathf.Pow(normalizedDistance, 2f));
        }
    }

    public void findLakeHexes() 
    {
        // Loops through each hex in the hexMap.
        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                // Gets the hex object at the column and row.
                Hex h = hexes[column, row];
                // Creates a list for hexes that have been check already.
                List<Hex> checkedHexes = new List<Hex>();

                // If the hex isn't on the checked list.
                if (!checkedHexes.Contains(h)) {
                    // If the hex is a water tile.
                    if (h.Elevation < 0) 
                    {
                        // Creates a list for hexes that are lake tiles.
                        List<Hex> lakeHexes = new List<Hex>{h};

                        int lakeSize = 1;

                        // Bool value whether hex touches a water tile.
                        bool bordersWater = IfHexTouchesAnyWater(h);

                        // If the hex border a water tile.
                        if (bordersWater)
                        {
                            // Creates a list of hexes that need to be check.
                            List<Hex> hexesToCheck = new List<Hex> { h };
                            checkedHexes.Add(h);

                            // Whil the list isn't empty and the lake is smaller than 10.
                            // Lakes bigger than 10 tiles are considered inland sea, so use ocean tile.
                            while (hexesToCheck.Count > 0 && lakeSize < 10) 
                            {
                                // Remove the hex from the list of ones needed to be checked.
                                Hex currentHex = hexesToCheck[0];
                                hexesToCheck.RemoveAt(0);

                                // Get all hexes bordering the currentHex.
                                Hex[] neighbors = GetHexesInRange(currentHex, 1);

                                // Loop through each hex the currentHex is bordering.
                                foreach (Hex neighbor in neighbors)
                                {
                                    // If the neiboring hex is water and hasn't been check already.
                                    if (neighbor.Elevation < 0 && !checkedHexes.Contains(neighbor)) {
                                        // Add it to all of the lists.
                                        hexesToCheck.Add(neighbor);
                                        checkedHexes.Add(neighbor);
                                        lakeHexes.Add(neighbor);
                                        lakeSize++;
                                    }
                                }
                            }
                        // Makes sure that it isn't changing an inland sea to lake material.
                        if (lakeSize < 10) 
                        {
                            // Loop through each hex in the lakeHexes list.
                            foreach (Hex lakeHex in lakeHexes)
                            {
                                // Set material for each hex added to lake.
                                lakeHex.isLake = true;
                            }
                        }
                        }
                    }
                }  else {
                    // If the tile is land, add it to checkedHexes because don't want to check it.
                    checkedHexes.Add(h);
                }
            }
        }
    }
}
