using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BlockState { Valid = 0, Intersecting = 1, OutOfBounds = 1, Placed = 2 }
public class Block  //Block is an assembly of a Pattern Def. + achor point + rotation + a grind?
{
    public List<Voxel> Voxels;
    public List<Voxel> JointVoxels;
    public List<Vector3Int> JointIndex;
    public List<Block> PlacedBlocks = new List<Block>();//____________________Does it interfier with the same list in voxelGrid?

    public PatternType Type;
    private Pattern _pattern => PatternManager.GetPatternByType(Type);
    private VoxelGrid _grid;
    private GameObject _goBlock;

    public Vector3Int Anchor;
    public Quaternion Rotation;
    private bool _placed = false;
    /// <summary>
    /// Get the current state of the block. Can be Valid, Intersecting, OutOfBound or Placed
    /// </summary>
    public BlockState State
    {
        get
        {
            if (_placed) return BlockState.Placed;
            if (Voxels.Count < _pattern.PatternVoxels.Count) return BlockState.OutOfBounds;
            if (Voxels.Count(v => v.Status != VoxelState.Available) > 0) return BlockState.Intersecting;
            return BlockState.Valid;
        }
    }
    /// <summary>
    /// Block constructor. Will create block starting at an anchor with a certain rotation of a given type.
    /// </summary>
    /// <param name="type">The block type</param>
    /// <param name="anchor">The index where the block needs to be instantiated</param>
    /// <param name="rotation">The rotation the blocks needs to be instantiated in</param>
    public Block(PatternType type, Vector3Int anchor, Quaternion rotation, VoxelGrid grid)
    {
        Type = type;
        Anchor = anchor;
        Rotation = rotation;
        _grid = grid;


        PositionPattern();

    }

    /// <summary>
    /// Add all the relevant voxels to the block according to it's anchor point, pattern and rotation //public Voxel(Vector3Int index, List<Vector3Int> possibleDirections)
    /// </summary>
    /*public void PositionPattern()
    {
        Voxels = new List<Voxel>();
        foreach (var voxel in _pattern.PatternVoxels)
        {
            if (Util.TryOrientIndex(voxel.Index, Anchor, Rotation, _grid, out var newIndex))
            {
                Voxels.Add(_grid.Voxels[newIndex.x, newIndex.y, newIndex.z]);
            }
        }
    }*/

    //2. /Non indexable directory of open placement slots_________________________________________________________________________________________________________________________<-Input here the placed blocks to keep track of the availabe slots
    public IEnumerable<Voxel> GetFlattenedDirectionAxisVoxels
    {
        get
        {
            
            var lastVoxel = Voxels.Last();
            var possibleIndex = lastVoxel.PossibleDirections;
            foreach (var direction in possibleIndex)
            {
                
                Vector3Int index = lastVoxel.Index + Util.AxisDirectionDic[direction];
                bool isInside = Util.CheckBounds(index, _grid);
                /*
                if (index.x > 0 && index.x < _grid.GridSize.x)
                {
                    if (index.y > 0 && index.y < _grid.GridSize.y)
                    {
                        if (index.z > 0 && index.y < _grid.GridSize.z)
                        {
                            yield return _grid.Voxels[index.x, index.y, index.z];
                        }
                    }
                    
                }
                */
                if (isInside == true)
                {
                    yield return _grid.Voxels[index.x, index.y, index.z];
                }
            }
        }
    }
    //2. /Create a list of JointVoxels that keeps track of the open placement slots_________________________________________________________________________________________________________________________<-Input here the placed blocks to keep track of the availabe slots
    public void CreateJointVoxels()
    {
        //Voxels = new List<Voxel>();
        JointVoxels = new List<Voxel>();
        JointIndex = new List<Vector3Int>();


        //public Voxel(Vector3Int index, List<AxisDirection> possibleDirections) //Filter the voxels that contain PossibleDirections

        List<Voxel> connectionVoxels = Voxels.Where(AxisDirection...EXISts?);

        foreach (var voxel in connectionVoxels)
        {
            //Vector3Int connectionIndex = voxel.Index + Util.AxisDirectionDic[direction];
            List<Vector3Int> possibleIndexList = connectionVoxels.PossibleDirections;
            foreach (var index in possibleIndexList)
            {
                //bool isInside = Util.CheckBounds(possibleIndex.Last(), _grid); //(voxel.Index, _grid);

                bool isInside = Util.CheckBounds(index, _grid);
                if (isInside == true)//If the voxel within bounds
                {
                    if (voxel.Status == 0)//If the voxel is not in a placedBlock
                    {

                        JointVoxels.Add(voxel);
                        JointIndex.Add(index);

                    }

                }

                else
                {
                    JointVoxels.Remove(voxel);
                    JointIndex.Add(index);
                }
            }
            

        }

    }

    //3.Loop over possible directions elements
    //____________________________________________________________________________________________________________This maybe has to go somewhere else! possibly next to the flaten dirVoxels

    /// <summary>
    /// Similar to TryAddCurrentBlocksToGrid() in voxel grid, but only trying to add one block at a time
    /// </summary>
    public bool ABlockAtATime()
    {
        var lastBlock = _grid.PlacedBlocks[_grid.PlacedBlocks.Count - 1];
        //var lastVoxel = lastBlock.Voxels[lastBlock.Voxels.Count - 1];

        //Does a backtrack function make sense?
        foreach (var voxel in JointVoxels)
        {
            int directoryLenght = JointVoxels.Count;
            int backTracker = 0;

            if (Util.TryOrientIndex(voxel.Index, Anchor, Rotation, _grid, out var newIndex))
            {
                
                var pastDirectory = JointVoxels[JointVoxels.Count - backTracker];
                //var pastDirectory = JointVoxels[JointVoxels.Count - i];

                Util.TryOrientRotation(pastDirectory.PossibleDirections[backTracker], Rotation, out var newAxis);
                pastDirectory.PossibleDirections[backTracker] = Util.AxisDirectionDic.First(d => d.Value == newAxis).Key;

                //Check if the axis is valid
                

                if (_grid.PlacedBlocks[backTracker].ActivateVoxels(out var newBlock))
                {
                    PlacedBlocks.Add(newBlock);
                    break;
                }
                
            }

            backTracker++;
        }
        return true;
    }

    ///<summary>
    ///New Possition pattern according to its possible directions
    ///<summary>
    public void PositionPattern() 
    {
        Voxels = new List<Voxel>();
        foreach (var voxel in _pattern.PatternVoxels)
        {
            if (Util.TryOrientIndex(voxel.Index, Anchor, Rotation, _grid, out var newIndex))
            {
                Voxel curVoxel = _grid.Voxels[newIndex.x, newIndex.y, newIndex.z];
                Voxels.Add(curVoxel);
                curVoxel.PossibleDirections = new List<AxisDirection>(voxel.PossibleDirections);
                for (int i = 0; i < curVoxel.PossibleDirections.Count; i++)
                {
                    Util.TryOrientRotation(curVoxel.PossibleDirections[i], Rotation, out var newAxis);
                    curVoxel.PossibleDirections[i] = Util.AxisDirectionDic.First(d=>d.Value == newAxis).Key;
                }
            }
        }
    }

    /// <summary>
    /// Try to activate all the voxels in the block. This method will always return false if the block is not in a valid state.
    /// </summary>
    /// <returns>Returns true if it managed to activate all the voxels in the grid</returns>
    public bool ActivateVoxels(out Block result)
    {
        result = null;
        if (State != BlockState.Valid)
        {
            Debug.LogWarning("Block can't be placed");
            return false;
        }
        Color randomCol = Util.RandomColor;

        foreach (var voxel in Voxels)
        {
            voxel.Status = VoxelState.Alive;
            //Drawing.DrawTransparentCube(((Vector3)voxel.Index * VoxelGrid.VoxelSize) + transform.position, 1f);
            voxel.SetColor(randomCol);
            
        }
        CreateGOBlock();
        result = this;
        _placed = true;
        return true;
    }


    public void CreateGOBlock()
    {
        _goBlock = GameObject.Instantiate(_grid.GOPatternPrefabs[Type], _grid.GetVoxelByIndex(Anchor).Centre, Rotation);
    }

    /// <summary>
    /// Remove the block from the grid
    /// </summary>
    public void DeactivateVoxels()
    {

        foreach (var voxel in Voxels)
            voxel.Status = VoxelState.Available;

    }

    /// <summary>
    /// Completely remove the block
    /// </summary>
    public void DestroyBlock()
    {
        DeactivateVoxels();
        if (_goBlock != null) GameObject.Destroy(_goBlock);
    }


  
}
