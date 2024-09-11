using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
    // Serialized Fields \\
    [SerializeField] private int seed = 0;

    // Private Fields \\
    private ConsistantRandom random;

    private FastNoise bumpNoise;
    private FastNoise largeHeightNoise;

    private FastNoise vegitationNoise;

    private FastNoise grassHeightNoise;

    // Private Methods \\
    private void Awake()
    {
        // Random 
        random = new ConsistantRandom(seed);

        // Bump - Terrain
        bumpNoise = new FastNoise(seed);

        bumpNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        bumpNoise.SetFrequency(0.01f);

        // Large - Terrain
        largeHeightNoise = new FastNoise(seed + 1);

        largeHeightNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        largeHeightNoise.SetFrequency(0.0025f);

        // Obstacle
        vegitationNoise = new FastNoise(seed + 2);

        largeHeightNoise.SetNoiseType(FastNoise.NoiseType.SimplexFractal);
        bumpNoise.SetFrequency(0.01f);

        // Grass
        grassHeightNoise = new FastNoise(seed + 3);

        grassHeightNoise.SetNoiseType(FastNoise.NoiseType.Simplex);
        grassHeightNoise.SetFrequency(0.05f);
    }

    // Public Methods \\
    public float GetHeightAtPoint(Vector2 _point)
    {
        float bumpHeight = bumpNoise.GetNoise(_point.x, _point.y) * 10;
        float largeHeight = largeHeightNoise.GetNoise(_point.x, _point.y) * 10;

        return bumpHeight + largeHeight;
    }

    public float GetGrassHeightAtPoint(Vector2 _point)
    {
        float value = (grassHeightNoise.GetNoise(_point.x, _point.y) + 1) / 2f;

        return Mathf.Clamp(value, 0f, 1f);
    }

    public float GetVegitationNoise(Vector2 _point)
    {
        float value = (vegitationNoise.GetNoise(_point.x, _point.y) + 1) * 50;

        return Mathf.Clamp(value, 0f, 100f);
    }

    public float Random(float _lowerRange, float _upperRange)
    {
        float randomDouble = (float) random.NextDouble();

        return Mathf.Lerp(_lowerRange, _upperRange, randomDouble);
    }
}