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
    //public Voxel[,,] Voxels;
    public VoxelGrid VoxelGrid { get; private set; }
    
    
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

    private Voxel _endPatternVoxel;
    private VoxelGrid _grid;
    private Pattern _pattern => PatternManager.GetPatternByType(Type);

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

    private int _triesPerIteration = 20
        ;
    private int _iterations = 100;

    private int _tryCounter = 0;
    private int _iterationCounter = 0;
   

    #endregion

    #region Start Voxel Deff

    //Research a GUI if everything works miraculously

    //1. Set a first Block / Voxel
    //Probably just select a random voxel with Z index 0, Do we mean Z as Y in UNITY?
    //Position a block on this voxel

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

    //1.1 Place a random patern and get its next possible voxels
    
    void Start()
    {
        GridSize = new Vector3Int(5, 5, 5);
        VoxelGrid = new VoxelGrid(GridSize, 1f);

        _grid = BManager.CreateVoxelGrid(BoundingMesh.GetGridDimensions(_voxelOffset, _voxelSize), _voxelSize, BoundingMesh.GetOrigin(_voxelOffset, _voxelSize));
        Debug.Log(_grid.GridSize);
        _grid.DisableOutsideBoundingMesh();

        //Display the voxels before start
        // Cycle through the VoxelGrid and create components
        /*for (int x = 0; x < _gridSize.x; x++)
        {
            for (int y = 0; y < _gridSize.y; y++)
            {
                for (int z = 0; z < _gridSize.z; z++)
                {
                    // Get the Voxel
                    var voxel = VoxelGrid.Voxels[x, y, z];
                    voxel.Status = VoxelState.Alive;

                }
            }
        }*/
        
    }

    void Update()//___________________________________________________________________________________________________________________________________________________ Loop over the combinatorial logic
    {
        DrawVoxels();
        if (Input.GetKeyDown("c"))
        {
  

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
    /*private void OnGUI()
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

    //2: List tracker and IEnumerable methods in Block CLass

    //Summary.
    //We have  somme Voxel Join listing that is close to be "done"... Its unclear if it has to be in VoxelGrid or in Block
    //The method for Adding a block per step is sort of done... but in the wrong class //public bool ABlockAtATime() is in block should be moved to VoxelGrid
    //Try Add Random block, needs to work with ABlockAtATime()
    //if this gose to plan... the coroutine should work... But we need to ad a start block and then aggregate from that.


    //Normalize

    private void CombinatorialStepLogic() //___________________________________________________________________________________________________________________________
    {
        //_grid.PurgeAllBlocks();

        _tryCounter = 0;
        while (_tryCounter < _triesPerIteration)
        {
            var lastBlock = _grid.PlacedBlocks[_grid.PlacedBlocks.Count - 1];
            var lastVoxel = lastBlock.Voxels[lastBlock.Voxels.Count - 1];
            ABlockAtATime();
            //TryAddCombinatorialBlock();
            _tryCounter++;
        }

        //Keeping track of the efficency
        _efficiencies.Add(_seed, _grid.Efficiency);

        //List seeds ordered by efficency
        orderedEfficiencyIndex = _efficiencies.Keys.OrderByDescending(k => _efficiencies[k]).Take(11).ToList();
        if (orderedEfficiencyIndex.Count == 11)
            _efficiencies.Remove(orderedEfficiencyIndex[10]);


    }
    #endregion

    #region Public methods

    #endregion
    public void DrawVoxels() //________Cant use it anywhere else
    {
        //Iterate through all voxles
        foreach (var voxel in VoxelGrid.Voxels)
        {
            //Check over more stuff_?
            //Draw voxel if it is not occupied
            //_goVoxel.SetActive(value);
            if (voxel.ShowVoxel == true)
            {
                Drawing.DrawTransparentCube(((Vector3)voxel.Index * VoxelGrid.VoxelSize) + transform.position, VoxelGrid.VoxelSize);
            }
        }
    }

    #region Private Methods

    /// <summary>
    /// From Util, Drawing Class
    /// </summary>


    //Create the method to select component by clicking
    /// <summary>
    /// Select a component and assign Agent position with mouse click
    /// </summary>
    private void SelectComponent()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Transform objectHit = hit.transform;

            if (objectHit.CompareTag("Component"))
            {
                //Assign clicked component to the selected variable
                _selected = objectHit.GetComponent<Component>();

                /*
                // 75 Set the position of the agent at the clicked voxel
                var pos = objectHit.transform.localPosition;
                Vector3Int posInt = new Vector3Int((int)pos.x, (int)pos.y, (int)pos.z);
                _agent.GoToVoxel(posInt);
                */
            }
        }
        else
        {
            _selected = null;
        }
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

    private bool TryAddCombinatorialBlock() //Use this as a start block?
    {
        _grid.SetRandomType(); //Upgrade this fuction in Voxel grid, to output the axis data of the random placement
        _grid.AddBlock(StartRandomIndexXZ(), RandomRotation());
        bool blockAdded = _grid.TryAddBlockToGrid();
       
        return blockAdded;
    }

    //7. Loop over 2 --> 3 till you place a certain amount of blocks, or no more blocks can be added__________________________________________________________________________
    IEnumerator CombinatorialAGG()
    {
        while (_iterationCounter < _iterations)
        {
            UnityEngine.Random.seed = _seed++;
            CombinatorialStepLogic();
            _iterationCounter++;
            yield return new WaitForSeconds(0.1f);
        }

        foreach (var value in _efficiencies.Values)
        {
            Debug.Log(value);
        }
    }
    #endregion

}
