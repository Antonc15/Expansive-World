using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainMeshData
{
    // Private Fields \\
    private Vector3[] vertices;
    private Vector2[] uvs;
    private int[] triangles;

    private int triangleIndex = 0;

    private Vector3[] borderVertices;
    private int[] borderTriangles;

    private int borderTriangleIndex = 0;

    // Initializer \\
    public TerrainMeshData(int _edgeVerts)
    {
        vertices = new Vector3[_edgeVerts * _edgeVerts];
        uvs = new Vector2[_edgeVerts * _edgeVerts];
        triangles = new int[(_edgeVerts - 1) * (_edgeVerts - 1) * 6];

        borderVertices = new Vector3[(_edgeVerts * 4) + 4];
        borderTriangles = new int[24 * _edgeVerts];
    }

    // Private Methods \\
    private Vector3[] CalculateNormals()
    {
        Vector3[] vertNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;

        for (int i = 0; i < triangleCount; i++)
        {
            int triIndex = i * 3;

            int vertIndexA = triangles[triIndex];
            int vertIndexB = triangles[triIndex + 1];
            int vertIndexC = triangles[triIndex + 2];

            Vector3 triNormal = NormalFromIndices(vertIndexA, vertIndexB, vertIndexC);

            vertNormals[vertIndexA] += triNormal;
            vertNormals[vertIndexB] += triNormal;
            vertNormals[vertIndexC] += triNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++)
        {
            int triIndex = i * 3;

            int vertIndexA = borderTriangles[triIndex];
            int vertIndexB = borderTriangles[triIndex + 1];
            int vertIndexC = borderTriangles[triIndex + 2];

            Vector3 triNormal = NormalFromIndices(vertIndexA, vertIndexB, vertIndexC);

            if(vertIndexA >= 0)
            {
                vertNormals[vertIndexA] += triNormal;
            }

            if (vertIndexB >= 0)
            {
                vertNormals[vertIndexB] += triNormal;
            }

            if (vertIndexC >= 0)
            {
                vertNormals[vertIndexC] += triNormal;
            }
        }

        for (int i = 0; i < vertNormals.Length; i++) 
        {
            vertNormals[i].Normalize();
        }

        return vertNormals;
    }

    private Vector3 NormalFromIndices(int _indexA, int _indexB, int _indexC)
    {
        Vector3 pointA = (_indexA < 0) ? borderVertices[-_indexA - 1] : vertices[_indexA];
        Vector3 pointB = (_indexB < 0) ? borderVertices[-_indexB - 1] : vertices[_indexB];
        Vector3 pointC = (_indexC < 0) ? borderVertices[-_indexC - 1] : vertices[_indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    // Public Methods \\
    public void AddVertex(Vector3 _vertexPos, Vector2 _vertexUV, int _index)
    {
        if(_index < 0)
        {
            borderVertices[-_index - 1] = _vertexPos;
            return;
        }

        vertices[_index] = _vertexPos;
        uvs[_index] = _vertexUV;
    }

    public void AddTriangle(int _indexA, int _indexB, int _indexC)
    {
        if (_indexA < 0 || _indexB < 0 || _indexC < 0)
        {
            borderTriangles[borderTriangleIndex] = _indexA;
            borderTriangles[borderTriangleIndex + 1] = _indexB;
            borderTriangles[borderTriangleIndex + 2] = _indexC;

            borderTriangleIndex += 3;

            return;
        }

        triangles[triangleIndex] = _indexA;
        triangles[triangleIndex + 1] = _indexB;
        triangles[triangleIndex + 2] = _indexC;

        triangleIndex += 3;
    }

    public Mesh GenerateMesh()
    {
        Mesh terrainMesh = new Mesh();

        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.uv = uvs;

        terrainMesh.normals = CalculateNormals();

        return terrainMesh;
    }
}