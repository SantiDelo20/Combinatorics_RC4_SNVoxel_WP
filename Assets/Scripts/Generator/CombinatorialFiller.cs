using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public class CombinatorialFiller : MonoBehaviour
{
    #region Public Fields
   
    public List<Voxel> Voxels;
   
    public List<Vector3Int> JointIndex;
    public Vector3Int Index;
    public Vector3Int GridSize;
    //have the patterns variables
    public PatternType Type;
    public IEnumerable<Voxel> IsOccupied { get; private set; } //Auto fix 
    
    #endregion

    #region Private Fields

    private VoxelGrid _grid;
    public Component _selected { get; private set; }
    private float _voxelSize = 0.2f;
    private bool generating = false;
    private int _seed = 1;

    private Dictionary<int, float> _efficiencies = new Dictionary<int, float>();
    private List<int> orderedEfficiencyIndex = new List<int>();

    #endregion

    #region Iteration Settings
    private int _triesPerIteration = 2000;
    private int _iterations = 50;
    private int _tryCounter = 0;
    private int _iterationCounter = 0;

    #endregion

    #region Random Index & Rotation
    //1. Set a first Block / Voxel
    Vector3Int StartRandomIndexXZ()
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
    #endregion

    #region Star & Update

    void Start()
    {
        GridSize = new Vector3Int(50, 50, 50);
        
        _grid = new VoxelGrid(GridSize, _voxelSize, transform.position);
        Debug.Log(_grid.GridSize);
        //_grid.DisableOutsideBoundingMesh();

        Debug.Log("Press space to Start Coroutine combinatorial Agg");
        Debug.Log("S for Start Block");
        Debug.Log("D for an after Block");

    }
    void Update()//___________________________________________________________________________________________________________________________________________________ Loop over the combinatorial logic
    {
        DrawVoxels();

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
    #region GUI Elements
    /// OnGUI is used to display all the scripted graphic user interface elements in the Unity loop
    private void OnGUI()
    {
        int padding = 30;
        int labelHeight = 60;
        int labelWidth = 500;
        int counter = 0;

        if (generating)
        {
            _grid.ShowVoxels = GUI.Toggle(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight), _grid.ShowVoxels, "Show voxels"); //-----------------xx

            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
                $"Grid {_grid.PlacedBlocks.Count} Blocks added");
            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
                $"Grid {_grid.JointVoxels.Count} Joints created");

            //Fill
            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
               $"Fill: {_efficiencies[0]}");
        }
        for (int i = 0; i < Mathf.Min(orderedEfficiencyIndex.Count, 10); i++)
        {
            string text = $"Seed: {orderedEfficiencyIndex[i]} Efficiency: {_efficiencies[orderedEfficiencyIndex[i]]}";
            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
               text);

        }
    }

    #endregion

    #region Combinatorial Logic
    /// <summary>
    /// Combinatorial block method, ads a random block per iteration, will try upto the set value of _triesPerIteration
    /// </summary>
    /// <returns></returns>
    private bool TryAddCombinatorialBlock() 
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
    public void DrawVoxels()
    {
        //Iterate through all voxles
        foreach (var voxel in _grid.JointVoxels)
        {
            //Draws the joint voxels

            //Small dot voxel
            Drawing.DrawTransparentCube(((Vector3)voxel.Index * _voxelSize) + transform.position, _voxelSize); //+ transform.position

            //Irradiant Red voxel
            Drawing.DrawCube(((Vector3)voxel.Index * _voxelSize) + transform.position, _voxelSize*0.35f);
        }
    }

    /// <summary>
    /// Draw voxel if it is not occupied, NOT WORKING
    /// </summary>
    public void DrawVoidGrid()
    {
       
        foreach (var voxel in _grid.Voxels)
        {
            Drawing.DrawTransparentCubeBig((Vector3)voxel.Index * _voxelSize, _voxelSize);
        }
        
    }

    /// <summary>
    /// Start Button call function
    /// </summary>
    public void StartAg()
    {
        if (!generating)
        {
            generating = true;
            StartCoroutine(CombinatorialAGG());
        }
    }
    /// <summary>
    /// Pause button call function
    /// </summary>
    public void StopAg()
    {
        generating = false;
        StopAllCoroutines();
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
    private bool ManualCombinatorialBlock() //DebugTool
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
                _tryCounter++;
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
                if (AddStartBlock())
                {
                    yield return new WaitForSeconds(0.2f);
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
                    yield return new WaitForSeconds(0.2f);
                }

                

                _iterationCounter++;
            }

            //Keeping track of the efficency
            _efficiencies.Add(_seed, _grid.Efficiency);

        }
        Console.WriteLine(":_(");

        //List seeds ordered by efficency
        //orderedEfficiencyIndex = _efficiencies.Keys.OrderByDescending(k => _efficiencies[k]).Take(11).ToList();
        //if (orderedEfficiencyIndex.Count == 11)
        //    _efficiencies.Remove(orderedEfficiencyIndex[10]); //cutting out the list to save memory

        foreach (var value in _efficiencies.Values)
        {
            Debug.Log(value);
        }
    }
    #endregion
}
