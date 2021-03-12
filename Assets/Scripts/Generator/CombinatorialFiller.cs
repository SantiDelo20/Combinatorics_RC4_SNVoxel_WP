using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CombinatorialFiller : MonoBehaviour
{
    #region Public Fields
    public VoxelGrid VoxelGrid { get; private set; }
   
    public List<Voxel> Voxels;
   
    public List<Vector3Int> JointIndex;
    public Vector3Int Index;
    public Vector3Int GridSize;
    public PatternType Type;
    public IEnumerable<Voxel> IsOccupied { get; private set; } //Auto fix
    private bool _showvoid;

    [SerializeField]
    Text _voidRatio;

    float _efficencyFillinstance;
    string _efficencyFillString;
    #endregion

    #region Private Fields

    //private VoxelGrid _grid;
    public Component _selected { get; private set; }
    private float _voxelSize = 0.2f;
    private bool generating = false;
    
    private int _seed = 1;

    private List<float> _efficiencyFill = new List<float>();
    

    #endregion

    #region Iteration Settings
    private int _triesPerIteration = 2000;
    private int _iterations = 240;//Divisible by 4
    private int _tryCounter = 0;
    private int _iterationCounter = 0;

    #endregion

    #region Random Index & Rotation
    //1. Set a first Block / Voxel
    Vector3Int StartRandomIndexXZ()
    {
        // Place a random start at the bottom
        int x = UnityEngine.Random.Range(0, VoxelGrid.GridSize.x);
        int y = 0;
        int z = UnityEngine.Random.Range(0, VoxelGrid.GridSize.z);
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

        VoxelGrid = new VoxelGrid(GridSize, _voxelSize, transform.position);
        Debug.Log(VoxelGrid.GridSize);
        //_grid.DisableOutsideBoundingMesh();

        Debug.Log("Press space to Start Coroutine combinatorial Agg");
        Debug.Log("S for Start Block");
        Debug.Log("D for an after Block");
    }

    void Update()//______________________________________________________________ Loop over the combinatorial logic
    {
        
        DrawVoxels();
        if (_voidRatio != null)
        {
            _voidRatio.text = $"Current Void Ratio: {GetVoidRatio().ToString("F2")}";
        }
        

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

        if (Input.GetKeyDown("t")) VoxelGrid.SetRandomType();
    }

    #endregion
    #region GUI Elements
    /// OnGUI is used to display all the scripted graphic user interface elements in the Unity loop
    private void OnGUI()
    {
        int padding = 30;
        int labelHeight = 20;
        int labelWidth = 500;
        int counter = 0;

        if (generating)
        {
            VoxelGrid.ShowPatternVoxels = GUI.Toggle(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight), VoxelGrid.ShowPatternVoxels, "Show pattern voxels"); //-----------------xx

            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
                $"Grid {VoxelGrid.PlacedBlocks.Count} Blocks added");
            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
                $"Grid {VoxelGrid.JointVoxels.Count} Joints created");
            GUI.Label(new Rect(padding, (padding + labelHeight) * ++counter, labelWidth, labelHeight),
                $"Fill efficency: {_efficencyFillString}");

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
            VoxelGrid.SetRandomType();
            var randomRotation = RandomRotation();
            for (int i = VoxelGrid.JointVoxels.Count - 1; i >= 0; i--)
            {
                var anchor = VoxelGrid.JointVoxels[i].Index;
                bool blockAdded = VoxelGrid.TryAddBlockToGrid(anchor, randomRotation);
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
        foreach (var voxel in VoxelGrid.JointVoxels)
        {
            //Draws the joint voxels

            //Small dot voxel
            Drawing.DrawTransparentCube(((Vector3)voxel.Index * _voxelSize) + transform.position, _voxelSize); //+ transform.position

            //Irradiant Red voxel
            Drawing.DrawCube(((Vector3)voxel.Index * _voxelSize) + transform.position, _voxelSize*0.35f);
        }
        if(_showvoid != false)
        {
            print("showing void");

            foreach (var voxel in VoxelGrid.Voxels)
            {
                
                if (voxel.Status == VoxelState.Available)
                {
                    Drawing.DrawTransparentCubeBig((Vector3)voxel.Index * _voxelSize, _voxelSize);

                }

            }

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
        VoxelGrid.SetRandomType();//RandomPattern
        bool blockAdded = VoxelGrid.TryAddBlockToGrid(StartRandomIndexXZ(), RandomRotation());
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
        VoxelGrid.SetRandomType();//RandomPattern
        var randomRotation = RandomRotation();
        for (int i = VoxelGrid.JointVoxels.Count - 1; i >= 0; i--)
        {
            var anchor = VoxelGrid.JointVoxels[i].Index;
            bool blockAdded = VoxelGrid.TryAddBlockToGrid(anchor, randomRotation);
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
            VoxelGrid.SetRandomType();//RandomPattern
            //_grid.AddBlock(StartRandomIndexXZ(), RandomRotation());
            bool blockAdded = VoxelGrid.TryAddBlockToGrid(StartRandomIndexXZ(), RandomRotation());
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
            _efficiencyFill.Add(VoxelGrid.Efficiency);

            //print(VoxelGrid.Efficiency.ToString());
            _efficencyFillString = VoxelGrid.Efficiency.ToString("F2");
            print(_efficencyFillString);

        }
        Console.WriteLine(":_(");

        //Debug.Log(_efficiencyFill.ToString());
            
    }
    public float GetVoidRatio()
    {
        if (generating)
        {
            _efficencyFillinstance = VoxelGrid.Efficiency;
            return _efficencyFillinstance;
        }
        else
        {
            return 0;
        }
    }
    #endregion

    public void VoidsOn(bool value)
    {
        if (value == true)
        {
            _showvoid = true;
        }
        else
        {
            _showvoid = false;
        }
    }
}
