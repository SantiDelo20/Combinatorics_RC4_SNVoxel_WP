using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombinatorialFiller : MonoBehaviour
{

    #region Public Fields
    //list of Blocks
    //list of Voxels, this can replace the blocks
    public List<Voxel> Voxels;

    //have the patterns variables
    public PatternType Type;


    #endregion

    #region Private Fields

    //have the grid variables
    private VoxelGrid _grid;
    //have the patterns variables
    private Pattern _pattern => PatternManager.GetPatternByType(Type);

    #endregion

    #region Start Voxel Deff

    //1. Set a first Block / Voxel
    //Probably just select a random voxel with Z index 0, Do we mean Z as Y in UNITY?
    //Position a block on this voxel

    Vector3Int StartRandomIndex()
    {
        // Place a random start at the bottom
        int x = Random.Range(0, _grid.GridSize.x);
        int y = 0;
        int z = Random.Range(0, _grid.GridSize.z);

        return new Vector3Int(x, y, z);
    }

    Quaternion RandomRotation()
    {
        int x = Random.Range(0, 4) * 90;
        int y = Random.Range(0, 4) * 90;
        int z = Random.Range(0, 4) * 90;
        return Quaternion.Euler(x, y, z);
    }

    #endregion


    //1. Set a first block
    //Probably just select a random voxel with Z index 0
    //Position a block on this voxel

    //2. Find all the possible next voxels
    //loop over all the blocks //Or the VOXELS!!
    //Where possibleDirection contains elements

    //Loop over possible directions elements
    //Get neighbour voxels of these elements in the direction
    //Check if index of neighbour voxel is withing grid (theres a Util function for that)
    //Check if neighbour voxel is still available
    //Add the neighbour voxel to the list of possible direction
    //neihgbour voxels need to be unique ==> Look into hashset

    //3. Try adding a block on a random neighbourvoxel until the next block is built

    //3.1 Neighbourvoxel class?

    //4. Loop over 2 --> 3 till you place a certain amount of blocks, or no more blocks can be added
    #region Public methods

    #endregion

    #region Private Methods

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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
