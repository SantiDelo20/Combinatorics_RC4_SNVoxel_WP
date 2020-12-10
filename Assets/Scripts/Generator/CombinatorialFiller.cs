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

    private Vector3 _normalizedTargetIndex;

    #endregion

    #region Iteration Settings

    private int _triesPerIteration = 25000;
    private int _iterations = 100;

    private int _tryCounter = 0;
    private int _iterationCounter = 0;
    private List<Voxel> _availableNeighbours;

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

    
    void Update()
    {
        DrawVoxels();//would be nice

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
    //2. Find all the possible next voxels
    //loop over all the blocks //Or the VOXELS!!
    //Where possibleDirection contains elements
    //how do we just look at the first/last random block/voxel placed??--->
    //Flattern Voxels

    //2. /GetTheListOf Axis//-- input this to public void PossibleDirectionsNeighbours()
    public IEnumerable<Voxel> FlattenedAxisVoxels
    {
        
        get
        {
            //public Voxel(Vector3Int index, List<AxisDirection> possibleDirections)
            foreach (var Voxel in _pattern.PatternVoxels)
            {
                //PatternType.get AxisDirection;
                //Voxel curVoxel = _grid.Voxels;
                //Voxels.Add(curVoxel);
                curVoxel.PossibleDirections = new List<AxisDirection>(voxel.PossibleDirections);
                
            }
            //A list of axis
        }
    }

    //Normalize

    private void CombinatorialLogic()
    {
        TryAddRandomBlock();
        //PossibleDirectionsNeighbours(); //1 feed the possible directions to de try add random
        //TryAddRandomNeighbour(PossibleDirectionsNeighbours()); //2 can we do Step one of logic in here?

        _grid.PurgeAllBlocks();

        _tryCounter = 0;
        while (_tryCounter < _triesPerIteration)
        {
            TryAddRandomNeighbour();
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

    //3.Loop over possible directions elements
    //Get neighbour voxels of these elements in the direction
    public void PossibleDirectionsNeighbours()
    {
        
        //We can unballance the index choice with this, and favour directions giving them more chances
        _normalizedTargetIndex = new Vector3(
            _endPatternVoxel.Index.x / _grid.GridSize.x - 1,
            _endPatternVoxel.Index.y / _grid.GridSize.y - 1,
            _endPatternVoxel.Index.z / _grid.GridSize.z - 1);

        Util.CheckBounds(_endPatternVoxel,_grid);

        //Ideally in the possible directions function we should input only the list we have of public List<AxisDirection> PossibleDirectionsArray;
        var neighbours = _endPatternVoxel.GetFaceNeighboursArray();
        for (int i = 0; i < neighbours.Length; i++)
        {
            if (neighbours != null)
            {
                //If neighbour voxel is occupied
                if (neighbours != IsOccupied)
                {
                    //RemoveTheIndex
                }
                // If neighbour voxel is not occupied
                else
                {
                    if (CheckBounds == true)
                    {
                        //Create a List off available neighbours

                        _availableNeighbours = new List<Voxel>();
                        //GoToRandomNeighbour();
                        //From the normalized __normalizedTargetIndex choose a random REMAINING index?
                    }
                    Console.WriteLine("is available!");
                    
                }

            }
            //If neighbour voxel does not exist
            else Console.WriteLine("Reached a no return!");
            //Restart
        }

    }

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



    /*private IEnumerable<Voxel> GetPathVoxels()
    {
        foreach (Voxel voxel in CombinatorialFiller.Voxels)
        {
            if (voxel.IsPath)
            {
                yield return voxel;
            }
        }
    }*/

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


    /// <summary>
    /// From Util, Drawing Class
    /// </summary>
    private void DrawVoxels()
    {
        // 15 Iterate through all voxles
        foreach (var voxel in VoxelGrid.Voxels)
        {
            // 16 Draw voxel if it is not occupied
            if (!voxel.IsOccupied)
            {
                Drawing.DrawTransparentCube(((Vector3)voxel.Index * VoxelGrid.VoxelSize) + transform.position, VoxelGrid.VoxelSize);
            }
        }
    }

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

    private bool TryAddRandomBlock() //Use this as a start block?
    {
        _grid.SetRandomType(); //Upgrade this fuction in Voxel grid, to output the axis data of the random placement
        _grid.AddBlock(StartRandomIndexXZ(), RandomRotation());
        bool blockAdded = _grid.TryAddCurrentBlocksToGrid();
        _grid.PurgeUnplacedBlocks();
        return blockAdded;
    }

    private bool TryAddRandomNeighbour()
    {
        Voxels = new List<Voxel>();
       
       //_grid = new List<AxisDirection>(voxel.PossibleDirections);
       // _grid.AddBlock(); In the Neighbours constraints
        bool blockAdded = _grid.TryAddCurrentBlocksToGrid();
        _grid.PurgeUnplacedBlocks();
        return blockAdded;
    }

    #endregion

}
