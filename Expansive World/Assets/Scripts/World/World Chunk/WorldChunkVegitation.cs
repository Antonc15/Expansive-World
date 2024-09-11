using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WorldChunkVegitation : MonoBehaviour
{
    // Serialized Fields \\
    [Header("Components")]
    [SerializeField] private Transform containerTransform;

    [Header("Settings")]
    [SerializeField] private LayerMask obstacleIgnoreLayer;
    [SerializeField] private VegitationScriptableObject[] vegitationSelection = new VegitationScriptableObject[1];

    // Private Fields \\
    private WorldMap worldMap;
    
    private List<Tuple<Vector3, float>> pointRadiusPairs = new List<Tuple<Vector3, float>>();

    // Public Methods \\
    public void ConstructVegitation(WorldMap _worldMap, VegitationData[] _vegDatas)
    {
        worldMap = _worldMap;

        int length = _vegDatas.Length;

        for (int i = 0; i < length; i++)
        {
            TryBuildVegitation(_vegDatas[i]);
        }
    }

    // Private Methods \\
    private void TryBuildVegitation(VegitationData _vegData)
    {
        List<VegitationScriptableObject> validVegitations = new List<VegitationScriptableObject>();

        float vegValue = _vegData.vegitationValue;

        for (int i = 0; i < vegitationSelection.Length; i++)
        {
            VegitationScriptableObject vso = vegitationSelection[i];

            if (vso.IsConstructable(vegValue))
            {
                validVegitations.Add(vso);
            }
        }

        for (int i = 0; i < validVegitations.Count; i++) 
        {
            float probabilityValue = worldMap.Random(0f, 100f);

            VegitationScriptableObject vso = validVegitations[i];

            if (vso.DoesFailProbability(probabilityValue)) { continue; }
            if (vso.DoesFailAngle(_vegData.normal)) { continue; }

            float objectScale = worldMap.Random(vso.MinScale, vso.MaxScale);
            float vegRadius = vso.ObstacleCheckRadius * objectScale;

            if (IsObstaclePointOccupied(_vegData.position, vegRadius)) { continue; }

            AddPointRadiusPair(_vegData.position, vegRadius);

            float randX = (vso.RandomRotX) ? worldMap.Random(0f, 360f) : 0f;
            float randY = (vso.RandomRotY) ? worldMap.Random(0f, 360f) : 0f;
            float randZ = (vso.RandomRotZ) ? worldMap.Random(0f, 360f) : 0f;

            Quaternion randomRot = Quaternion.Euler(randX, randY, randZ);

            Quaternion normalRot = Quaternion.LookRotation(_vegData.normal) * Quaternion.FromToRotation(Vector3.forward, Vector3.down);
            Quaternion objectRot = Quaternion.Slerp(Quaternion.identity, normalRot, vso.SurfaceNormalWeight);

            GameObject newVegObject = Instantiate(vso.Prefab, _vegData.position, objectRot * randomRot, containerTransform);

            newVegObject.transform.localScale = Vector3.one * objectScale;

            return;
        }
    }

    private bool IsObstaclePointOccupied(Vector3 _point, float _radius)
    {
        bool checkSphereCollision = Physics.CheckSphere(_point, _radius, ~obstacleIgnoreLayer);
        bool distanceCollision = false;

        for (int i = 0; i < pointRadiusPairs.Count; i++)
        {
            Tuple<Vector3, float> prp = pointRadiusPairs[i];

            float minDistance = _radius + prp.Item2;

            if (Vector3.Distance(_point, prp.Item1) <= minDistance)
            {
                distanceCollision = true;
                break;
            }
        }

        return checkSphereCollision || distanceCollision;
    }

    private void AddPointRadiusPair(Vector3 _point, float _radius)
    {
        Tuple<Vector3, float> prp = new (_point, _radius); 
        pointRadiusPairs.Add(prp);
    }
}