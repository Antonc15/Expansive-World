using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
    // Serialized Fields \\
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject chunkPrefab;

    [Header("Dependents")]
    [SerializeField] private WorldMap worldMap;

    [Header("Settings")]
    [SerializeField] private float chunkSize = 32f;
    [SerializeField] private int chunkPadding = 0;

    // Private Fields \\
    private Vector2Int lastBottomLeft;
    private Vector2Int lastTopRight;

    private Dictionary<Vector2Int, WorldChunk> loadedChunks = new Dictionary<Vector2Int, WorldChunk>();

    // Private Methods -- Start \\
    private void Start()
    {
        Vector2Int[] bounds = GetScreenBounds();

        Vector2Int bottomLeft = bounds[0];
        Vector2Int topRight = bounds[1];

        LoadChunks(bottomLeft, topRight);

        lastBottomLeft = bottomLeft;
        lastTopRight = topRight;
    }

    private void LoadChunks(Vector2Int _bottomLeft, Vector2Int _topRight)
    {
        Vector2Int[] chunks = GetCoordsFromBounds(_bottomLeft, _topRight).ToArray();

        for (int i = 0; i < chunks.Length; i++)
        {
            ConstructChunk(chunks[i]);
        }
    }

    // Private Methods -- Update \\
    private void Update()
    {
        Vector2Int[] bounds = GetScreenBounds();

        Vector2Int bottomLeft = bounds[0];
        Vector2Int topRight = bounds[1];

        if (bottomLeft != lastBottomLeft || topRight != lastTopRight)
        {
            UpdateChunks(bottomLeft, topRight);
        }

        lastBottomLeft = bottomLeft;
        lastTopRight = topRight;
    }

    private void UpdateChunks(Vector2Int _bottomLeft, Vector2Int _topRight)
    {
        List<Vector2Int> oldCoords = loadedChunks.Keys.ToList();
        List<Vector2Int> newCoords = GetCoordsFromBounds(_bottomLeft, _topRight);

        List<Vector2Int> sharedCoords = new List<Vector2Int>();

        // Populate Shared-Coords list
        for (int i = 0; i < newCoords.Count; i++)
        {
            Vector2Int coord = newCoords[i];

            if (oldCoords.Contains(coord))
            {
                sharedCoords.Add(coord);
            }
        }

        // Remove Shared-Coords from Both Lists
        for (int i = 0; i < sharedCoords.Count; i++)
        {
            oldCoords.Remove(sharedCoords[i]);
            newCoords.Remove(sharedCoords[i]);
        }

        // Destroy remaining coordinates in old-coords.
        for (int i = 0; i < oldCoords.Count; i++)
        {
            DestructChunk(oldCoords[i]);
        }

        // Construct remaining coordinates in new-coords.
        for (int i = 0; i < newCoords.Count; i++)
        {
            ConstructChunk(newCoords[i]);
        }
    }

    private List<Vector2Int> GetCoordsFromBounds(Vector2Int _bottomLeft, Vector2Int _topRight)
    {
        List<Vector2Int> coords = new List<Vector2Int>();

        int left = _bottomLeft.x;
        int right = _topRight.x;
        int bottom = _bottomLeft.y;
        int top = _topRight.y;

        for (int x = left - chunkPadding; x <= right + chunkPadding; x++)
        {
            for (int y = bottom - chunkPadding; y <= top + chunkPadding; y++)
            {
                coords.Add(new Vector2Int(x, y));
            }
        }

        return coords;
    }

    private Vector2Int ScreenPointToChunk(int _x, int _y)
    {
        Ray ray = cam.ScreenPointToRay(new Vector2(_x, _y));
        Plane plane = new Plane(Vector3.up, 0f);

        Vector3 point = Vector3.zero;

        if (plane.Raycast(ray, out float intersect))
        {
            point = ray.GetPoint(intersect);
        }

        int x = Mathf.RoundToInt(point.x / chunkSize);
        int y = Mathf.RoundToInt(point.z / chunkSize);

        return new Vector2Int(x, y);
    }

    private Vector2Int[] GetScreenBounds()
    {
        Vector2Int[] checkPoints = new Vector2Int[4];

        checkPoints[0] = ScreenPointToChunk(0, 0); // Bottom - Left
        checkPoints[1] = ScreenPointToChunk(0, Screen.height - 1); // Top - Left
        checkPoints[2] = ScreenPointToChunk(Screen.width - 1, Screen.height - 1); // Top - Right
        checkPoints[3] = ScreenPointToChunk(Screen.width - 1, 0); // Bottom - Right

        int maxX = int.MinValue;
        int minX = int.MaxValue;
        int maxY = int.MinValue;
        int minY = int.MaxValue;

        for (int i = 0; i < checkPoints.Length; i++)
        {
            int x = checkPoints[i].x;
            int y = checkPoints[i].y;

            if (x < minX) { minX = x; }
            if (x > maxX) { maxX = x; }

            if (y < minY) { minY = y; }
            if (y > maxY) { maxY = y; }
        }

        return new Vector2Int[2] { new Vector2Int(minX, minY), new Vector2Int(maxX, maxY) };
    }

    // Private Methods -- Chunks \\
    private void ConstructChunk(Vector2Int _chunkCoordinate)
    {
        Vector3 worldPos = new Vector3(_chunkCoordinate.x, 0, _chunkCoordinate.y) * chunkSize;

        GameObject newChunk = Instantiate(chunkPrefab, worldPos, Quaternion.identity, transform);
        WorldChunk newWorldChunk = newChunk.GetComponent<WorldChunk>();

        loadedChunks.Add(_chunkCoordinate, newWorldChunk);
        newWorldChunk.Construct(worldMap);
    }

    private void DestructChunk(Vector2Int _chunkCoordinate)
    {
        if(!loadedChunks.TryGetValue(_chunkCoordinate, out WorldChunk chunk)) { return; }

        loadedChunks.Remove(_chunkCoordinate);
        chunk.Destruct();
    }
}