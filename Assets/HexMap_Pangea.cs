using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMap_Pangea : HexMap {

    override public void GenerateMap()
    {
        base.GenerateMap();  // Initialize the hexes

        int targetLandTiles = (int)(NumRows * NumColumns * 0.4f);  // Targeting 40% land coverage
        int landTilesCreated = 0;
        int waterTilesCreated = 0;

        // Start with a central point
        int centerX = NumColumns / 2;
        int centerY = NumRows / 2;

        int borderSize = 4;
        int perlinBorderSize = 2;

        Random.InitState(0);

        // Queue to handle land expansion
        Queue<Hex> frontier = new Queue<Hex>();
        frontier.Enqueue(GetHexAt(centerX, centerY));

        while (landTilesCreated < targetLandTiles && frontier.Count > 0)
        {
            Hex currentHex = frontier.Dequeue();

            if (currentHex.R >= borderSize && currentHex.R < NumRows - borderSize)
            {
                if (currentHex.Elevation < HeightFlat)
                {
                    currentHex.Elevation = HeightFlat;  // Make it land
                    landTilesCreated++;

                    // Randomly pick neighbors to convert to land
                    foreach (Hex neighbor in GetHexesInRange(currentHex, 1))
                    {
                        if (neighbor != null && neighbor.Elevation < HeightFlat && Random.value > 0.3f)
                        {
                            frontier.Enqueue(neighbor);
                        }
                    }
                } else 
                {
                    waterTilesCreated++;
                }
            }
        }

        perlinNoiseGeneration(targetLandTiles, perlinBorderSize);
        findLakeHexes();
        
        // Now make sure all the hex visuals are updated to match the data.
        UpdateHexVisuals();
    }

    void perlinNoiseGeneration(int targetLandTiles, int borderSize) {
        int landTilesCreated = 0;
        int waterTilesCreated = 0;

        // Add lumpiness Perlin Noise?
        float elevationNoiseResolution = 0.02f;
        Vector2 elevationNoiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); 

        float moistureNoiseResolution = 0.12f;
        Vector2 moistureNoiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); 

        float vegetationNoiseResolution = 0.01f;
        Vector2 vegetationNoiseOffset = new Vector2( Random.Range(0f, 1f), Random.Range(0f, 1f) ); 

        float elevationNoiseScale = 2.3f;  // Larger values makes more lakes and islands.
        float moistureNoiseScale = 2f;  // Larger values makes more deserts and marshes.
        float vegetationNoiseScale = 2f;  // Larger values makes more deserts and forrest/jungle.
        

        for (int column = 0; column < NumColumns; column++)
        {
            for (int row = 0; row < NumRows; row++)
            {
                Hex h = GetHexAt(column, row);
                
                if (!IfHexTouchesOnlyWater(h) && row >= borderSize && row < NumRows - borderSize) {
                    // Noise for elevation
                    float e = 
                        Mathf.PerlinNoise( ((float)column/Mathf.Max(NumColumns,NumRows) / elevationNoiseResolution) + elevationNoiseOffset.x, 
                            ((float)row/Mathf.Max(NumColumns,NumRows) / elevationNoiseResolution) + elevationNoiseOffset.y )
                        - 0.3f;
                    h.Elevation += e * elevationNoiseScale;

                    // Noise for moisture
                     float m = 
                    Mathf.PerlinNoise( ((float)column/Mathf.Max(NumColumns,NumRows) / moistureNoiseResolution) + moistureNoiseOffset.x, 
                        ((float)row/Mathf.Max(NumColumns,NumRows) / moistureNoiseResolution) + moistureNoiseOffset.y )
                    - 0.5f;
                    h.Moisture = m * moistureNoiseScale;

                    // Noise for vegetation
                     float v = 
                    Mathf.PerlinNoise( ((float)column/Mathf.Max(NumColumns,NumRows) / vegetationNoiseResolution) + vegetationNoiseOffset.x, 
                        ((float)row/Mathf.Max(NumColumns,NumRows) / vegetationNoiseResolution) + vegetationNoiseOffset.y )
                    - 0.5f;
                    h.Vegetation = v * vegetationNoiseScale;
                }

                if (h.Elevation >= HeightFlat) 
                {
                    landTilesCreated++;
                } else 
                {
                    waterTilesCreated++;
                }
            }
        }

        
        Debug.Log($"Target Land Tiles: {targetLandTiles}, Land Tiles: {landTilesCreated}");
        Debug.Log($"Target Water Tiles: {NumRows * NumColumns - targetLandTiles}, Water Tiles: {waterTilesCreated}");
    }

    void ElevateArea(int q, int r, int range, float centerHeight = 0.8f)
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
}
