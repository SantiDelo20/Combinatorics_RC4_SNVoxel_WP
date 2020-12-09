using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombinatorialFiller : MonoBehaviour
{

    #region Public Fields
    //list of Blocks
    //list of Voxels, this can replace the blocks
    public List<Voxel> Voxels;
    public Vector3Int Index;
    public Vector3Int GridSize;

    //have the patterns variables
    public PatternType Type;

    public BuildingManager BManager
    {
        get
        {
            if (_buildingManager == null)
            {
                GameObject manager = GameObject.Find("Manager");
                _buildingManager = manager.GetComponent<BuildingManager>();
            }
            return _buildingManager;
        }
    }


    #endregion

    #region Private Fields

    //have the grid variables
    private VoxelGrid _grid;
    //have the patterns variables
    private Pattern _pattern => PatternManager.GetPatternByType(Type);

    //Grid generating variables
    private float _voxelSize = 0.2f;
    private int _voxelOffset = 2;
    private BuildingManager _buildingManager;


    #endregion

    #region Start Voxel Deff

    //Research a GUI if everything works miraculously

    //1. Set a first Block / Voxel
    //Probably just select a random voxel with Z index 0, Do we mean Z as Y in UNITY?
    //Position a block on this voxel

    Vector3Int StartRandomIndex()
    {
        // Place a random start at the bottom
        int x = UnityEngine.Random.Range(0, _grid.GridSize.x);
        int y = 0;
        int z = UnityEngine.Random.Range(0, _grid.GridSize.z);

        return new Vector3Int(x, y, z);
    }

    Quaternion RandomRotation()
    {
        int x = UnityEngine.Random.Range(0, 4) * 90;
        int y = UnityEngine.Random.Range(0, 4) * 90;
        int z = UnityEngine.Random.Range(0, 4) * 90;
        return Quaternion.Euler(x, y, z);
    }

    //1.1 Place a random patern and get its next possible voxels
    
    void Start()
    {
        _grid = BManager.CreateVoxelGrid(BoundingMesh.GetGridDimensions(_voxelOffset, _voxelSize), _voxelSize, BoundingMesh.GetOrigin(_voxelOffset, _voxelSize));
        Debug.Log(_grid.GridSize);
        _grid.DisableOutsideBoundingMesh();
        
    }

    
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            TryAddRandomBlock();
            //StartCoroutine(CombinatorialFiller())
        }
        else
        {
            Console.WriteLine("press space to start");
        }
    }
    #endregion

    #region Public methods
    //2. Find all the possible next voxels
    //loop over all the blocks //Or the VOXELS!!
    //Where possibleDirection contains elements
    public IEnumerable<Voxel> NextVoxels() //how do we just look at the first/last random block/voxel placed??--->
    {
        for (int x = 0; x < GridSize.x; x++)
            for (int y = 0; y < GridSize.y; y++)
                for (int z = 0; z < GridSize.z; z++)
                {
                    //yield return Voxels[index, possibleDirections];
                    //Block state?
                    
                }
    }


    //3.Loop over possible directions elements
    //Get neighbour voxels of these elements in the direction
    //Use the Face Class? Neighbour Faces -> Get the Index/Centers ->
    //In Voxel Class -> IEnumerable<Voxel> GetFaceNeighbours()

    //RC4_M1_C3
    //foreach (GraphVoxel voxel in Voxels)
    //    {
    //        //Get the neighbours of this voxel
    //        var neighbours = voxel.GetFaceNeighbours().Select(n => (GraphVoxel)n);



    //4.Check if index of neighbour voxel is within grid (theres a Util function for that)

    //4.1Check if neighbour voxel is still available

    //_neighbourVoxel.CheckBounds();
    //if true voxel.add

    //5.Add the neighbour voxel to the list of possible direction
    //neihgbour voxels need to be unique ==> Look into hashset

    //6. Try adding a block on a random neighbourvoxel until the next block is built

    //6.1 Neighbourvoxel class?

    //7. Loop over 2 --> 3 till you place a certain amount of blocks, or no more blocks can be added

    #endregion

    #region Private Methods


    private IEnumerable<Voxel> GetPathVoxels()
    {
        foreach (Voxel voxel in CombinatorialFiller.Voxels)
        {
            if (voxel.IsPath)
            {
                yield return voxel;
            }
        }

        //HashSet from RC4_M1_C3
        //class provides high-performance set operations.
        //A set is a collection that contains no duplicate elements, and whose elements are in no particular order.

        //for (int i = 1; i < _targets.Count; i++)
        //{
        //    var start = _targets[i - 1];
        //    var shortest = graph.ShortestPathsDijkstra(e => 1.0, start);
        //    var end = _targets[i];
        //    if (shortest(end, out var endPath))
        //    {
        //        var endPathVoxels = new HashSet<GraphVoxel>(endPath.SelectMany(e => new[] { e.Source, e.Target }));
        //        foreach (var pathVoxel in endPathVoxels)
        //        {
        //            pathVoxel.SetAsPath();

        //            //84 Yield return after setting voxel as path
        //            yield return new WaitForSeconds(0.1f);
        //        }
        //}
    }

    /// <summary>
    /// Methods using VoxelGrid operations, 
    /// </summary>
    private void BlockTest()
    {
        var anchor = new Vector3Int(2, 8, 0);
        var rotation = Quaternion.Euler(0, 0, -90);
        _grid.AddBlock(anchor, rotation);
        _grid.TryAddCurrentBlocksToGrid();
    }

    private bool TryAddRandomBlock()
    {
        _grid.SetRandomType();
        _grid.AddBlock(StartRandomIndex(), RandomRotation());
        bool blockAdded = _grid.TryAddCurrentBlocksToGrid();
        _grid.PurgeUnplacedBlocks();
        return blockAdded;
    }

    #endregion
    
}
