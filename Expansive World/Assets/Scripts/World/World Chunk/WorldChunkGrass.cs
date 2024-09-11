using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldChunkGrass : MonoBehaviour
{
    // Serialized Fields \\
    [Header("Grass Mesh & Material")]
    [SerializeField] private Mesh grassMesh;
    [SerializeField] private Material grassMaterial;

    // Private Fields \\
    private List<List<GPUObjectData>> batches = new List<List<GPUObjectData>>();

    private bool isInitiated = false;

    // Public Methods \\
    public void Initiate(GPUObjectData[] _objectDatas)
    {
        int length = _objectDatas.Length;

        List<GPUObjectData> currentBatch = new List<GPUObjectData>();

        int batchIndex = 0;

        for (int i = 0; i < length; i++) 
        {
            currentBatch.Add(_objectDatas[i]);
            batchIndex++;

            if(batchIndex == 1023)
            {
                batches.Add(currentBatch);

                currentBatch = new List<GPUObjectData>();
                
                batchIndex = 0;
            }
        }

        if(currentBatch.Count > 0)
        {
            batches.Add(currentBatch);
        }

        isInitiated = true;
    }

    // Private Methods \\
    private void Update()
    {
        if(!isInitiated) { return; }

        for (int i = 0; i < batches.Count; i++)
        {
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, batches[i].Select((a) => a.matrix).ToList());
        }
    }
}