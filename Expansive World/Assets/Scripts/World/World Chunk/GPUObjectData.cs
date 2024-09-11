using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUObjectData
{
    public Vector3 pos;
    public Quaternion rot;
    public Vector3 scale;

    public Matrix4x4 matrix
    {
        get
        {
            return Matrix4x4.TRS(pos, rot, scale);
        }
    }

    public GPUObjectData(Vector3 _pos, Quaternion _rot, Vector3 _scale)
    {
        pos = _pos;
        rot = _rot;
        scale = _scale;
    }
}