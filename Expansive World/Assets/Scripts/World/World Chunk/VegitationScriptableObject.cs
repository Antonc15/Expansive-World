using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewVegitation", menuName = "ScriptableObjects/New Vegitation Object", order = 1)]
public class VegitationScriptableObject : ScriptableObject
{
    // Serialized Fields \\
    [Header("GameObject")]
    [SerializeField] private GameObject prefab;

    [Header("Generation")]
    [SerializeField][Range(0f, 100f)] private float vegitationValueUpper = 100f;
    [SerializeField][Range(0f, 100f)] private float vegitationValueLower = 0f;
    [SerializeField][Range(0f, 360f)] private float maxAngle = 14f;
    [SerializeField][Range(0f, 100f)] private float percentProbability = 0.1f;

    [Header("Rotation")]
    [SerializeField] private bool randomRotationX = false;
    [SerializeField] private bool randomRotationY = false;
    [SerializeField] private bool randomRotationZ = false;
    [SerializeField][Range(0f, 1f)] private float surfaceNormalWeight = 0f;

    [Header("Scaling & Collision")]
    [SerializeField] private float obstacleCheckRadius = 0.5f;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;

    // Public Methods \\
    public bool IsConstructable(float _vegValue)
    {
        return _vegValue <= vegitationValueUpper && _vegValue >= vegitationValueLower;
    }

    public bool DoesFailProbability(float _probabilityvalue)
    {
        return _probabilityvalue > percentProbability;
    }

    public bool DoesFailAngle(Vector3 _normal)
    {
        float dotAngle = Vector3.Dot(_normal, Vector3.up) * 360f;
        return dotAngle < (360f - maxAngle);
    }

    // Public Getters \\
    public float ObstacleCheckRadius
    {
        get
        {
            return obstacleCheckRadius;
        }
    }

    public float SurfaceNormalWeight
    {
        get
        {
            return surfaceNormalWeight;
        }
    }

    public float MaxAngle
    {
        get
        {
            return maxAngle;
        }
    }

    public float MinScale
    {
        get
        {
            return minScale;
        }
    }

    public float MaxScale
    {
        get
        {
            return maxScale;
        }
    }

    public bool RandomRotX
    {
        get
        {
            return randomRotationX;
        }
    }

    public bool RandomRotY
    {
        get
        {
            return randomRotationY;
        }
    }

    public bool RandomRotZ
    {
        get
        {
            return randomRotationZ;
        }
    }

    public GameObject Prefab
    {
        get
        {
            return prefab;
        }
    }
}