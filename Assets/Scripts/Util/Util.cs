using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis { X, Y, Z };

// new enum axis direction for pattern class?? ------------
public enum AxisDirection { Xmin, Xplus, Ymin, Yplus, Zmin, Zplus };

/// <summary>
/// Static class for all general usefull functions related to this project
/// </summary>
public static class Util
{


    // Task, Make a static dictionary to store the joint logics<---------------------------------------------------xxxx
    public static Dictionary<AxisDirection, Vector3Int> AxisDirectionDic
        = new Dictionary<AxisDirection, Vector3Int>()
        {
            {AxisDirection.Xmin, new Vector3Int(-1,0,0) },
            {AxisDirection.Xplus, new Vector3Int(1,0,0) },
            {AxisDirection.Ymin, new Vector3Int(0,-1,0) },
            {AxisDirection.Yplus, new Vector3Int(0,1,0) },
            {AxisDirection.Zmin, new Vector3Int(0,0,-1) },
            {AxisDirection.Zplus, new Vector3Int(0,0,1) },
        };


    /// <summary>
    /// Extension method to Unities Vector3Int class. Now you can use a Vector3 variable and use the .ToVector3InRound to get the vector rounded to its integer values
    /// </summary>
    /// <param name="v">the Vector3 variable this method is applied to</param>
    /// <returns>the rounded Vector3Int value of the given Vector3</returns>
    public static Vector3Int ToVector3IntRound(this Vector3 v) => new Vector3Int(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y), Mathf.RoundToInt(v.z));

    public static bool TryOrientIndex(Vector3Int localIndex, Vector3Int anchor, Quaternion rotation, VoxelGrid grid, out Vector3Int worldIndex)
    {
        var rotated = rotation * localIndex;
        worldIndex = anchor + rotated.ToVector3IntRound();
        return CheckBounds(worldIndex, grid);
    }

    public static void TryOrientRotation(AxisDirection originalAxis, Quaternion rotation, out Vector3Int newAxis) //NEW-------------------Function!
    {
        newAxis = (rotation * AxisDirectionDic[originalAxis]).ToVector3IntRound();
    }

    /// <summary>
    /// Check if a given index is inside or outside the voxelgrid
    /// </summary>
    /// <param name="index">the index to check</param>
    /// <returns>true for inside, false for outside</returns>
    public static bool CheckBounds(Vector3Int index, VoxelGrid grid)
    {
        if (index.x < 0) return false;
        if (index.y < 0) return false;
        if (index.z < 0) return false;
        if (index.x >= grid.GridSize.x) return false;
        if (index.y >= grid.GridSize.y) return false;
        if (index.z >= grid.GridSize.z) return false;
        return true;
    }

    public static Color RandomColor
    {
        get
        {
            float r = Random.Range(0, 255) / 255f;
            float g = Random.Range(0, 255) / 255f;
            float b = Random.Range(0, 255) / 255f;
            return new Color(r, g, b);
        }
    }
}
