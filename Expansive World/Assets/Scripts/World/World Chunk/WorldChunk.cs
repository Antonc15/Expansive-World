using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    // Serialized Fields \\
    [Header("Terrain Mesh")]
    [SerializeField] private MeshFilter terrainMeshFilter;
    [SerializeField] private MeshCollider terrainMeshCollider;

    [Header("Terrain Mesh Settings")]
    [SerializeField] private int terrainMeshResolution = 12;
    [SerializeField] private float terrainMeshSize = 32f;

    [Header("Vegitation")]
    [SerializeField] private WorldChunkVegitation vegitationChunkComponent;

    [Header("Vegitation Settings")]
    [SerializeField] private int vegitationPointDensity = 1000;
    [SerializeField] private float vegitationRaycastHeight = 100f;

    [Header("Grass Components")]
    [SerializeField] private WorldChunkGrass grassChunkComponent;

    [Header("Grass Settings - Performance")]
    [SerializeField] private int grassResolution = 16;
    [SerializeField] private float grassMaxAngle = 14f;
    [SerializeField] private float grassRaycastHeight = 100f;

    [Header("Grass Settings - Collision Check")]
    [SerializeField] private LayerMask grassCheckIgnoreLayers;
    [SerializeField] private float grassOccupationRadius = 0.2f;

    [Header("Grass Settings - Visual")]
    [SerializeField] private float grassRandomDisplacement = 0.3f;
    [SerializeField] private float grassScale = 2f;
    [SerializeField] private float grassHeightMin = 0.9f;
    [SerializeField] private float grassHeightMax = 1.2f;

    [Header("Universal")]
    [SerializeField] private LayerMask terrainMeshLayer;

    // Private Fields \\
    private WorldMap worldMap;

    // Private Methods \\
    private Mesh GenerateTerrainMesh()
    {
        int vertexResolution = terrainMeshResolution + 1;
        int borderedResolution = vertexResolution + 2;

        TerrainMeshData meshData = new TerrainMeshData(vertexResolution);

        int[,] vertIndices = GenerateVertIndices(borderedResolution);
        Vector2[,] vertUVs = GenerateVertUVs(vertexResolution, borderedResolution);
        Vector3[,] vertPositions = GenerateVertPositions(vertexResolution, borderedResolution);

        for (int x = 0; x < borderedResolution; x++)
        {
            for (int y = 0; y < borderedResolution; y++)
            {
                meshData.AddVertex(vertPositions[x, y], vertUVs[x, y], vertIndices[x, y]);

                if (x < borderedResolution - 1 && y < borderedResolution - 1)
                {
                    int a = vertIndices[x, y];
                    int b = vertIndices[x, y + 1];
                    int c = vertIndices[x + 1, y + 1];
                    int d = vertIndices[x + 1, y];

                    meshData.AddTriangle(a, b, c);
                    meshData.AddTriangle(c, d, a);
                }
            }
        }

        return meshData.GenerateMesh();
    }

    private int[,] GenerateVertIndices(int _res)
    {
        int[,] vertIndices = new int[_res, _res];

        int meshIndex = 0;
        int borderIndex = -1;

        for (int x = 0; x < _res; x++)
        {
            for (int y = 0; y < _res; y++)
            {
                // If the [x,y] is within the border, assign a border-index; otherwise assign mesh index.
                if (x == 0 || x == _res - 1 || y == 0 || y == _res - 1)
                {
                    vertIndices[x, y] = borderIndex;
                    borderIndex--;

                    continue;
                }

                vertIndices[x, y] = meshIndex;
                meshIndex++;
            }
        }

        return vertIndices;
    }

    private Vector3[,] GenerateVertPositions(int _vertRes, int _borderRes)
    {
        Vector3[,] vertPositions = new Vector3[_borderRes, _borderRes];

        float vertGap = terrainMeshSize / (_vertRes - 1f);
        float posOffset = (-terrainMeshSize / 2f) - vertGap;

        Vector3 bottomLeftPos = new Vector3(posOffset, 0f, posOffset);
        Vector2 bottomLeftMap = new Vector2(posOffset, posOffset) + new Vector2(transform.position.x, transform.position.z);

        for (int x = 0; x < _borderRes; x++)
        {
            for (int y = 0; y < _borderRes; y++)
            {
                float height = worldMap.GetHeightAtPoint(new Vector2(x * vertGap, y * vertGap) + bottomLeftMap);
                vertPositions[x, y] = bottomLeftPos + new Vector3(x * vertGap, height, y * vertGap);
            }
        }

        return vertPositions;
    }

    private Vector2[,] GenerateVertUVs(int _vertRes, int _borderRes)
    {
        Vector2[,] vertUVs = new Vector2[_borderRes, _borderRes];

        float uvGap = 1f / (_vertRes - 1f);

        for (int x = 0; x < _borderRes; x++)
        {
            for (int y = 0; y < _borderRes; y++)
            {
                vertUVs[x,y] = new Vector2((x - 1) * uvGap, (y - 1) * uvGap);
            }
        }

        return vertUVs;
    }

    private VegitationData[] GenerateVegitationData()
    {
        List<VegitationData> data = new List<VegitationData>();

        float posOffset = -terrainMeshSize / 2f;
        Vector3 bottomLeftPos = new Vector3(posOffset + transform.position.x, vegitationRaycastHeight, posOffset + transform.position.z);

        for (int i = 0; i < vegitationPointDensity; i++)
        {
            Vector3 randomPos = new Vector3(worldMap.Random(0f, 1f), 0, worldMap.Random(0f, 1f)) * terrainMeshSize;
            Vector3 raycastPos = randomPos + bottomLeftPos;

            RaycastHit hit;

            if(Physics.Raycast(raycastPos, Vector3.down, out hit))
            {
                VegitationData newVegData = new VegitationData();

                newVegData.position = hit.point;
                newVegData.normal = hit.normal;

                newVegData.vegitationValue = worldMap.GetVegitationNoise(new Vector2(hit.point.x, hit.point.z));

                data.Add(newVegData);

                continue;
            }
        }

        return data.ToArray();
    }

    private GPUObjectData[] GenerateGrassObjectData()
    {
        List<GPUObjectData> grassData = new List<GPUObjectData>();

        float objectGap = terrainMeshSize / (grassResolution - 1f);
        float posOffset = -terrainMeshSize / 2f;

        Vector3 bottomLeftPos = new Vector3(posOffset + transform.position.x, grassRaycastHeight, posOffset + transform.position.z);

        for (int x = 0; x < grassResolution; x++)
        {
            for (int y = 0; y < grassResolution; y++)
            {
                Vector3 randomVec = new Vector3(worldMap.Random(-1f, 1f), 0, worldMap.Random(-1f, 1f)) * grassRandomDisplacement;
                Vector3 raycastPos = bottomLeftPos + randomVec + new Vector3(x * objectGap, 0, y * objectGap);

                RaycastHit hit;

                if(!Physics.Raycast(raycastPos, Vector3.down, out hit, Mathf.Infinity, terrainMeshLayer)) { continue; }

                if (IsGrassAngleDenied(hit.normal) || IsGrassPointOccupied(hit.point)) { continue; }

                float t = worldMap.GetGrassHeightAtPoint(new Vector2(hit.point.x, hit.point.z));
                float grassHeight = Mathf.Lerp(grassHeightMin, grassHeightMax, t);

                Vector3 scale = new Vector3(grassScale, grassHeight, grassScale);

                Quaternion surfaceNormal = Quaternion.LookRotation(hit.normal);
                Quaternion faceUpConversion = Quaternion.FromToRotation(Vector3.forward, Vector3.down);
                Quaternion randomAngle = Quaternion.Euler(0, worldMap.Random(0f, 360f), 0);

                Quaternion rot = surfaceNormal * faceUpConversion * randomAngle;

                grassData.Add(new GPUObjectData(hit.point, rot, scale));
            }
        }

        return grassData.ToArray();
    }

    private bool IsGrassAngleDenied(Vector3 _normal)
    {
        float dotAngle = Vector3.Dot(_normal, Vector3.up) * 360f;

        return dotAngle < (360f - grassMaxAngle);
    }

    private bool IsGrassPointOccupied(Vector3 _point)
    {
        return Physics.CheckSphere(_point, grassOccupationRadius, grassCheckIgnoreLayers);
    }

    // Private Coroutines \\
    private IEnumerator GenerateGrassLate()
    {
        yield return new WaitForFixedUpdate();

        GPUObjectData[] grassData = GenerateGrassObjectData();
        grassChunkComponent.Initiate(grassData);
    }

    // Public Methods \\
    public void Construct(WorldMap _map)
    {
        worldMap = _map;

        // Terrain mesh
        Mesh terrainMesh = GenerateTerrainMesh();

        terrainMeshFilter.sharedMesh = terrainMesh;
        terrainMeshCollider.sharedMesh = terrainMesh;

        // Vegitation
        VegitationData[] vegData = GenerateVegitationData();
        vegitationChunkComponent.ConstructVegitation(worldMap, vegData);

        // Grass -- Coroutine wait until fixed update to allow obstacle instantiations to take affect.
        StartCoroutine(GenerateGrassLate());
    }

    public void Destruct()
    {
        Destroy(gameObject);
    }
}