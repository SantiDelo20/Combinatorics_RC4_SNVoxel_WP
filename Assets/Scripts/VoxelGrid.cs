﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
//using System;
public class VoxelGrid
{
    #region Public fields
    public Voxel[,,] Voxels;
    public Block[,,] Blocks;
    public Vector3Int GridSize;
    public readonly float VoxelSize;
    public Corner[,,] Corners;
    public Face[][,,] Faces = new Face[3][,,];
    public Edge[][,,] Edges = new Edge[3][,,];
    public Vector3 Origin;
    public Vector3 Corner;

    //List of placed blocks
    public List<Voxel> JointVoxels = new List<Voxel>();
    public List<Block> PlacedBlocks = new List<Block>();
    #endregion

    #region private fields
    private bool _showVoxels = false;
    private GameObject _goVoxelPrefab;
    private List<Block> _blocks = new List<Block>();
    private PatternType _currentPattern = PatternType.PatternA;
    private List<Block> _currentBlocks => _blocks.Where(b => b.State != BlockState.Placed).ToList();//___List of the current placed blocks with its available directiction Voxels
    #endregion

    #region Public dynamic getters

    public bool ShowPatternVoxels
    {
        get
        {
            return _showVoxels;
        }
        set
        {
            foreach (var voxel in FlattenedVoxels)
            {
                voxel.ShowVoxel = value;
            }
            _showVoxels = value;
        }
    }
    /// <summary>
    /// Counts the number of blocks placed in the voxelgrid
    /// </summary>
    public int NumberOfBlocks => _blocks.Count(b => b.State == BlockState.Placed);
    /// <summary>
    /// what percentage of the available grid has been filled up
    /// </summary>
    public float Efficiency
    {
        get
        {
            //if we don't cast this value to a float, it always returns 0 as it is rounding down to integer values
            return (float)FlattenedVoxels.Count(v => v.Status == VoxelState.Alive) / FlattenedVoxels.Where(v => v.Status != VoxelState.Dead).Count() * 100;
        }
    }

    private Dictionary<PatternType, GameObject> _goPatternPrefabs;
    public Dictionary<PatternType, GameObject> GOPatternPrefabs
    {
        get
        {
            if (_goPatternPrefabs == null)
            {
                _goPatternPrefabs = new Dictionary<PatternType, GameObject>();
                _goPatternPrefabs.Add(PatternType.PatternA, Resources.Load("Prefabs/Part_PatL") as GameObject);
                _goPatternPrefabs.Add(PatternType.PatternB, Resources.Load("Prefabs/Part_PatT") as GameObject);
                _goPatternPrefabs.Add(PatternType.PatternC, Resources.Load("Prefabs/Part_PatC") as GameObject);

            }
            return _goPatternPrefabs;
        }
    }
    /// <summary>
    /// Return the voxels in a flat list rather than a threedimensional array
    /// </summary>
    public IEnumerable<Voxel> FlattenedVoxels
    {
        get
        {
            for (int x = 0; x < GridSize.x; x++)
                for (int y = 0; y < GridSize.y; y++)
                    for (int z = 0; z < GridSize.z; z++)
                        yield return Voxels[x, y, z];
        }
    }
    public Voxel GetVoxelByIndex(Vector3Int index) => Voxels[index.x, index.y, index.z];
    /// <summary>
    /// Return all blocks that are not allready place in the grid
    /// </summary>

    #endregion

    #region constructor
    /// <summary>
    /// Constructor for the voxelgrid object. To be called in the Building manager. Origin set to 0,0,0
    /// </summary>
    /// <param name="gridDimensions">The dimensions of the grid</param>
    /// <param name="voxelSize">The size of one voxel</param>
    public VoxelGrid(Vector3Int gridDimensions, float voxelSize)
    {
        GridSize = gridDimensions;
        _goVoxelPrefab = Resources.Load("Prefabs/VoxelCube") as GameObject;
        VoxelSize = voxelSize;
        Origin = Vector3.zero;
        CreateVoxelGrid();
    }

    /// <summary>
    /// Constructor for the voxelgrid object. To be called in the Building manager
    /// </summary>
    /// <param name="gridDimensions">The dimensions of the grid</param>
    /// <param name="voxelSize">The size of one voxel</param>
    public VoxelGrid(Vector3Int gridDimensions, float voxelSize, Vector3 origin)
    {
        GridSize = gridDimensions;
        _goVoxelPrefab = Resources.Load("Prefabs/VoxelCube") as GameObject;
        VoxelSize = voxelSize;
        Origin = origin;
        CreateVoxelGrid();
    }

    /// <summary>
    /// Generate the voxelgrid from public Voxel(Vector3Int index, List<Vector3Int> possibleDirections)
    /// </summary>
    private void CreateVoxelGrid()
    {
        Voxels = new Voxel[GridSize.x, GridSize.y, GridSize.z];
        for (int x = 0; x < GridSize.x; x++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int z = 0; z < GridSize.z; z++)
                {
                    //Voxels[x, y, z] = new Voxel(new Vector3Int(x, y, z), _goVoxelPrefab, this);
                    Voxels[x, y, z] = new Voxel(new Vector3Int(x, y, z), _goVoxelPrefab, this);
                }
            }
        }
        MakeFaces();
        MakeCorners();
        MakeEdges();
    }
    #endregion
    #region Grid elements constructors

    /// <summary>
    /// Creates the Faces of each <see cref="Voxel"/>
    /// </summary>
    private void MakeFaces()
    {
        // make faces
        Faces[0] = new Face[GridSize.x + 1, GridSize.y, GridSize.z];
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[0][x, y, z] = new Face(x, y, z, Axis.X, this);
                }
        Faces[1] = new Face[GridSize.x, GridSize.y + 1, GridSize.z];
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Faces[1][x, y, z] = new Face(x, y, z, Axis.Y, this);
                }
        Faces[2] = new Face[GridSize.x, GridSize.y, GridSize.z + 1];
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Faces[2][x, y, z] = new Face(x, y, z, Axis.Z, this);
                }
    }
    /// <summary>
    /// Creates the Corners of each Voxel
    /// </summary>
    private void MakeCorners()
    {
        Corner = new Vector3(Origin.x - VoxelSize / 2, Origin.y - VoxelSize / 2, Origin.z - VoxelSize / 2);
        Corners = new Corner[GridSize.x + 1, GridSize.y + 1, GridSize.z + 1];
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Corners[x, y, z] = new Corner(new Vector3Int(x, y, z), this);
                }
    }
    /// <summary>
    /// Creates the Edges of each Voxel
    /// </summary>
    private void MakeEdges()
    {
        Edges[2] = new Edge[GridSize.x + 1, GridSize.y + 1, GridSize.z];
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    Edges[2][x, y, z] = new Edge(x, y, z, Axis.Z, this);
                }
        Edges[0] = new Edge[GridSize.x, GridSize.y + 1, GridSize.z + 1];
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[0][x, y, z] = new Edge(x, y, z, Axis.X, this);
                }
        Edges[1] = new Edge[GridSize.x + 1, GridSize.y, GridSize.z + 1];
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    Edges[1][x, y, z] = new Edge(x, y, z, Axis.Y, this);
                }
    }
    #endregion

    #region Block functionality
    /// <summary>
    /// Temporary add a block to the grid. To confirm the block at it's current position, use the TryAddCurrentBlocksToGrid function
    /// </summary>
    /// <param name="anchor">The voxel where the pattern will start building from index(0,0,0) in the pattern</param>
    /// <param name="rotation">The rotation for the current block. This will be rounded to the nearest x,y or z axis</param>
    public void AddBlock(Vector3Int anchor, Quaternion rotation) => _blocks.Add(new Block(_currentPattern, anchor, rotation, this));
    /// <summary>
    /// Similar To TryAddCurrentBlocksToGrid() but trying to place only One Block
    /// </summary>
    /// <returns></returns>
    public bool TryAddBlockToGrid(Vector3Int anchor, Quaternion rotation)//_____________________________________________________________
    {
        Debug.LogWarning("TryAddBlockToGrid");

        Block newBlock = new Block(_currentPattern, anchor, rotation, this);
        if (newBlock.ActivateVoxels(out var successBlock))
        {
            PlacedBlocks.Add(successBlock);
            UpdateJoints();
            return true;
        }
        return false;
    }
    /// <summary>
    /// Try to add the blocks that are currently pending to the grids
    /// </summary>
    /// <returns>true if the function managed to place all the current blocks. False in all other cases</returns>
    public bool TryAddCurrentBlocksToGrid()//____________________________________________________________________
    {
        if (_currentBlocks == null || _currentBlocks.Count == 0)
        {
            Debug.LogWarning("No blocks to add");
            return false;
        }
        if (_currentBlocks.Count(b => b.State != BlockState.Valid) > 0)
        {
            //if we use $ in front of ", variables can be added inline between {} when defining a string
            Debug.LogWarning($"{_currentBlocks.Count(b => b.State != BlockState.Valid)} blocks could not be place because their position is not valid");
            return false;
        }
        int counter = 0;
        //Keep adding blocks to the grid untill all the pending blocks are added_Combinatorial FillerClass
        while (_currentBlocks.Count > 0)
        {
            //Keep track of the blocks
            _currentBlocks.First().ActivateVoxelsLegacy();
            UpdateJoints();
            counter++;
        }
        //Debug.Log($"Added {counter} blocks to the grid");
        return true;
    }
    //2. /Create a list of JointVoxels that keeps track of the open placement slots______________________________
    public void UpdateJoints()
    {
        JointVoxels = new List<Voxel>();
        //var connection = FlattenedVoxels.Where(v => v.PossibleDirections.Count > 0);
        var placedVoxels = PlacedBlocks.SelectMany(b => b.Voxels);
        foreach (var voxel in placedVoxels)
        {
            if (voxel.PossibleDirections != null && voxel.PossibleDirections.Count > 0)
            {
                var directions = voxel.PossibleDirections;
                foreach (var direction in directions)
                {
                    Vector3Int index = voxel.Index + Util.AxisDirectionDic[direction];

                    bool isInside = Util.CheckBounds(index, this); //grid thingy is this the VoxelGrid It self
                    if (isInside)//If the voxel within bounds
                    {
                        Voxel neighbour = Voxels[index.x, index.y, index.z];
                        
                        if (neighbour.Status == VoxelState.Available) 
                        {
                            JointVoxels.Add(neighbour);//If the voxel is not in a placedBlock
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Remove all pending blocks from the grid
    /// </summary>
    public void PurgeUnplacedBlocks()
    {
        _blocks.RemoveAll(b => b.State != BlockState.Placed);
    }
    public void PurgeAllBlocks()
    {
        foreach (var block in _blocks)
        {
            block.DestroyBlock();
        }
        _blocks = new List<Block>();
    }

    /// <summary>
    /// Set a random PatternType based on all the possible patterns in te PatternType Enum.
    /// </summary>
    public void SetRandomType()
    {
        PatternType[] values = System.Enum.GetValues(typeof(PatternType)).Cast<PatternType>().ToArray(); //collection of pattern types
        _currentPattern = (PatternType)values[Random.Range(0, values.Length)]; //Up scalable for more Patterns.
    }
    #endregion

    #region Grid operations
    /// <summary>
    /// Get the Faces of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the faces</returns>
    public IEnumerable<Face> GetFaces()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Faces[n].GetLength(0);
            int ySize = Faces[n].GetLength(1);
            int zSize = Faces[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Faces[n][x, y, z];
                    }
        }
    }

    /// <summary>
    /// Get the Voxels of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Voxels</returns>
    public IEnumerable<Voxel> GetVoxels()
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    yield return Voxels[x, y, z];
                }
    }

    /// <summary>
    /// Get the Corners of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the Corners</returns>
    public IEnumerable<Corner> GetCorners()
    {
        for (int x = 0; x < GridSize.x + 1; x++)
            for (int y = 0; y < GridSize.y + 1; y++)
                for (int z = 0; z < GridSize.z + 1; z++)
                {
                    yield return Corners[x, y, z];
                }
    }

    /// <summary>
    /// Get the Edges of the <see cref="VoxelGrid"/>
    /// </summary>
    /// <returns>All the edges</returns>
    public IEnumerable<Edge> GetEdges()
    {
        for (int n = 0; n < 3; n++)
        {
            int xSize = Edges[n].GetLength(0);
            int ySize = Edges[n].GetLength(1);
            int zSize = Edges[n].GetLength(2);

            for (int x = 0; x < xSize; x++)
                for (int y = 0; y < ySize; y++)
                    for (int z = 0; z < zSize; z++)
                    {
                        yield return Edges[n][x, y, z];
                    }
        }
    }

    public void DisableInsideBoundingMesh()
    {
        foreach (var voxel in GetVoxels())
        {
            if (BoundingMesh.IsInsideCentre(voxel)) voxel.Status = VoxelState.Dead;
        }
    }

    public void DisableOutsideBoundingMesh()
    {
        foreach (var voxel in GetVoxels())
        {
            if (!BoundingMesh.IsInsideCentre(voxel)) voxel.Status = VoxelState.Dead;
        }
    }

    #endregion

}
