using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum VoxelState { Dead = 0, Alive = 1, Available = 2 }
public class Voxel
{
    #region Public fields
    public Vector3Int Index;
    public List<Face> Faces = new List<Face>(6);
    //NewDirFunctionality-------------------------------------------XXXX
    public Vector3Int RefIndex;
    public List<AxisDirection> PossibleDirections;
    public bool IsOccupied;
    public bool IsOrigin;

    #endregion
    #region Private fields
    private GameObject _goVoxel;
    private VoxelState _voxelStatus;
    private VoxelGrid _grid;
    private bool _showVoxel;
    private List<Corner> _corners;
    #endregion
    #region Public accessors
    public List<Corner> Corners
    {
        get
        {
            if (_corners == null)
            {
                _corners = new List<Corner>();
                _corners.Add(_grid.Corners[Index.x, Index.y, Index.z]);
                _corners.Add(_grid.Corners[Index.x + 1, Index.y, Index.z]);
                _corners.Add(_grid.Corners[Index.x, Index.y + 1, Index.z]);
                _corners.Add(_grid.Corners[Index.x, Index.y, Index.z + 1]);
                _corners.Add(_grid.Corners[Index.x + 1, Index.y + 1, Index.z]);
                _corners.Add(_grid.Corners[Index.x, Index.y + 1, Index.z + 1]);
                _corners.Add(_grid.Corners[Index.x + 1, Index.y, Index.z + 1]);
                _corners.Add(_grid.Corners[Index.x + 1, Index.y + 1, Index.z + 1]);
            }
            return _corners;
        }
    }
    public bool ShowVoxel
    {
        get
        {
            return _showVoxel;
        }
        set
        {
            _showVoxel = value;
            if (!value)
                _goVoxel.SetActive(value);
            else
                _goVoxel.SetActive(Status == VoxelState.Alive);
        }
    }
    /// <summary>
    /// Get the centre point of the voxel in worldspace
    /// </summary>
    public Vector3 Centre => _grid.Origin + (Vector3)Index * _grid.VoxelSize + Vector3.one * 0.5f * _grid.VoxelSize;  //------------------X
    /// <summary>
    /// Get and set the status of the voxel. When setting the status, the linked gameobject will be enable or disabled depending on the state.
    /// </summary>
    public VoxelState Status
    {
        get
        {
            return _voxelStatus;
        }
        set
        {
            _goVoxel?.SetActive(value == VoxelState.Alive && _showVoxel);
            //if(_goVoxel!=null) _goVoxel.SetActive(value == VoxelState.Alive && _showVoxel); This does the same as the line above
            _voxelStatus = value;
        }
    }
    #endregion

    #region Constructors
    /// <summary>
    /// Voxel constructor. Will construct a voxel object and a Gameobject linked to this to the voxelgrid.
    /// </summary>
    /// <param name="index">index of the voxel</param>
    /// <param name="goVoxel">prefab of the voxel gameobject</param>
    public Voxel(Vector3Int index, GameObject goVoxel, VoxelGrid grid)
    {
        _grid = grid;
        Index = index;
        _goVoxel = GameObject.Instantiate(goVoxel, Centre, Quaternion.identity);
        _goVoxel.GetComponent<VoxelTrigger>().TriggerVoxel = this;
        _goVoxel.transform.localScale = Vector3.one * _grid.VoxelSize * 0.95f;
        Status = VoxelState.Available;
    }
    /// <summary>
    /// Voxel constructor based on possibleDirenctions Rule deffinition
    /// </summary>
    /// <param name="index"></param>
    /// <param name="possibleDirections"></param>
    public Voxel(Vector3Int index, List<AxisDirection> possibleDirections)// GameObject goVoxel, VoxelGrid grid) //public Voxel(Vector3Int index, List<Vector3Int> possibleDirections, VoxelGrid voxelGrid) or?
    {
        Index = index;
        Status = VoxelState.Available;
        PossibleDirections = possibleDirections; //Possible directions new function in util with enum and dictionary
        //Status = VoxelState.Available; //Duplicate
    }
    #endregion
    #region Public methods
    public void SetColor(Color color)
    {
        _goVoxel.GetComponent<MeshRenderer>().material.color = color;
        // 15 Iterate through all voxles

    }
   
    public override int GetHashCode()
    {
        return Index.GetHashCode();
    }

    #endregion


}