﻿using System;
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
    //public Voxel[,,] Voxels;
    public VoxelGrid VoxelGrid { get; private set; }
    public List<Vector3Int> JointIndex;

    public Vector3Int Index;
    public Vector3Int GridSize;

    //have the patterns variables
    public PatternType Type;

    public IEnumerable<Voxel> IsOccupied { get; private set; } //Auto fix 

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

    private VoxelGrid _grid;

    public Component _selected { get; private set; }

    //Grid generating variables
    private float _voxelSize = 0.2f;
    private int _voxelOffset = 2;
    private BuildingManager _buildingManager;

    private bool generating = false;
    private int _seed = 0;

    private Dictionary<int, float> _efficiencies = new Dictionary<int, float>(); //Undersand dictionary
    private List<int> orderedEfficiencyIndex = new List<int>();


    #endregion

    #region Iteration Settings

    private int _triesPerIteration = 20000;
    private int _iterations = 100;

    private int _tryCounter = 0;
    private int _iterationCounter = 0;
   

    #endregion

    #region Random Index & Rotation

    //1. Set a first Block / Voxel
    //Probably just select a random voxel with Z index 0, Do we mean Z as Y in UNITY?
    //Position a block on this voxel

    Vector3Int StartRandomIndexXZ()
    {
        // Place a random start at the bottom
        int x = UnityEngine.Random.Range(0, _grid.GridSize.x);
        int y = UnityEngine.Random.Range(0, _grid.GridSize.y);
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
    #endregion

    #region Star & Update
    void Start()
    {
        GridSize = new Vector3Int(5, 5, 5);
        VoxelGrid = new VoxelGrid(GridSize, 1f);

        _grid = BManager.CreateVoxelGrid(BoundingMesh.GetGridDimensions(_voxelOffset, _voxelSize), _voxelSize, BoundingMesh.GetOrigin(_voxelOffset, _voxelSize));
        Debug.Log(_grid.GridSize);
        _grid.DisableOutsideBoundingMesh();

        Debug.Log("Press space Start Coroutine combinatorial Agg");
        Debug.Log("S for Start Block");
        Debug.Log("d for an after Block");

    }

    void Update()//___________________________________________________________________________________________________________________________________________________ Loop over the combinatorial logic
    {
        //DrawVoxels();
        

        if (Input.GetKeyDown("s"))
        {
            ManualJumpStart();
        }
        if (Input.GetKeyDown("d"))
        {
            ManualCombinatorialBlock();
        }

        
        if (Input.GetKeyDown("space"))
        {

            //TryAddCombinatorialBlock();

            if (!generating)
            {
                generating = true;

                StartCoroutine(CombinatorialAGG());
            }
            else
            {

                generating = false;

                StopAllCoroutines();
            }
        }
        
        if (Input.GetKeyDown("t")) _grid.SetRandomType();

    }
    #endregion


    #region Display GUI Elements
    /// <summary>
    /// Toggle Voxels & Display efficenccy
    /// </summary>
    ///
    /*
    private void OnGUI()
    {
        int padding = 10;
        int labelHeight = 20;
        int labelWidth = 250;
        int counter = 0;

        if (generating)
        {
            _grid.ShowVoxels = GUI.Toggle(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight), _grid.ShowVoxels, "Show voxels"); //-----------------xx

            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
                $"Grid {_grid.Efficiency} % filled");
            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
                $"Grid {_grid.NumberOfBlocks} Blocks added");
        }
        for (int i = 0; i < Mathf.Min(orderedEfficiencyIndex.Count, 10); i++)
        {
            string text = $"Seed: {orderedEfficiencyIndex[i]} Efficiency: {_efficiencies[orderedEfficiencyIndex[i]]}";
            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
               text);

        }
    }
    */
    
    #endregion

    #region Combinatorial Logic
    //2. Find all the possible next voxels//___________________________________________________________________________________________________________________________ Block GetFlattenedDirectionAxisVoxels
    //loop over all the blocks //Or the VOXELS!!
    //Where possibleDirection contains elements

    //2: List tracker and IEnumerable methods in VoxelGrid CLass

    private bool TryAddCombinatorialBlock() //________Do we implement a for loop that cicles through the placed block finding possible alternatives if the las block fails?
    {

        _tryCounter = 0;

        while (_tryCounter < _triesPerIteration)
        {
            _grid.SetRandomType();
            var randomRotation = RandomRotation();

            for (int i = _grid.JointVoxels.Count - 1; i >= 0; i--)
            {

                var anchor = _grid.JointVoxels[i].Index;

                bool blockAdded = _grid.TryAddBlockToGrid(anchor, randomRotation);
                if (blockAdded)
                {
                    return true;
                }

            }
        }
        Debug.Log("Nope :'(");
        return false;
        
    }

    #endregion

    #region Public methods
    /// <summary>
    /// Drawing not working
    /// </summary>
    public void DrawVoxels() //________Cant use it anywhere else
    {
        //Iterate through all voxles
        foreach (var voxel in VoxelGrid.Voxels)
        {
            //Check over more stuff_?
            //Draw voxel if it is not occupied
            if (voxel.ShowVoxel == true)
            {
                Drawing.DrawTransparentCube(((Vector3)voxel.Index * VoxelGrid.VoxelSize) + transform.position, VoxelGrid.VoxelSize);
            }
        }
    }
    #endregion


    #region Private Methods

    /// <summary>
    /// TestManual StarBlock
    /// </summary>
    /// <returns></returns>
    private bool ManualJumpStart()
    {
        //_tryCounter = 0;
        Debug.Log("Add Start Block Attempt");
        _grid.SetRandomType();//RandomPattern
        bool blockAdded = _grid.TryAddBlockToGrid(StartRandomIndexXZ(), RandomRotation());
        if (blockAdded)
        {
            return true;
        }
        Debug.Log("Nope :'(");
        return false;
    }

    private bool ManualCombinatorialBlock() //________Do we implement a for loop that cicles through the placed block finding possible alternatives if the las block fails?
    {
        Debug.Log("Next Block Attempt");
        _grid.SetRandomType();//RandomPattern
        var randomRotation = RandomRotation();
        for (int i = _grid.JointVoxels.Count - 1; i >= 0; i--)
        {

            var anchor = _grid.JointVoxels[i].Index;

            bool blockAdded = _grid.TryAddBlockToGrid(anchor, randomRotation);
            if (blockAdded)
            {
                //_tryCounter++;
                return true;
            }

        }
        Debug.Log("Nope :'(");
        return false;

    }

    /// <summary>
    /// Methods using VoxelGrid operations, 
    /// </summary>
    private bool AddStartBlock()
    {
        _tryCounter = 0;
        Debug.Log("Add Start Block Attempt");
        while (_tryCounter < _triesPerIteration)
        {
            _grid.SetRandomType();//RandomPattern

            //_grid.AddBlock(StartRandomIndexXZ(), RandomRotation());
            bool blockAdded = _grid.TryAddBlockToGrid(StartRandomIndexXZ(), RandomRotation());
            if (blockAdded)
            {
                _tryCounter ++;
                return true;
            }
        }
        Debug.Log("Nope :'(");
        return false;
    }

    //7. Loop over 2 --> 3 till you place a certain amount of blocks, or no more blocks can be added__________________________________________________________________________
    IEnumerator CombinatorialAGG()
    {
        while (_iterationCounter < _iterations)
        {
            UnityEngine.Random.InitState(_seed++);

            if (_iterationCounter == 0)
            {
                if(AddStartBlock())
                {
                    yield return new WaitForSeconds(0.1f);
                    _iterationCounter++;
                }
                else
                {
                    break;
                }
            }
            else
            {
                if (TryAddCombinatorialBlock())
                {
                    
                    yield return new WaitForSeconds(0.1f);
                }
                _iterationCounter++;
            }
            
        }
        Console.WriteLine(":_(");

        foreach (var value in _efficiencies.Values)
        {
            Debug.Log(value);
        }
    }
    #endregion

}
