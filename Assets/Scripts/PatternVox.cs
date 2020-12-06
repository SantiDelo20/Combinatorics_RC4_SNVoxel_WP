using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

/// <summary>
/// PatternType can be refered to by name. These can become your block names to make your code more readible. This enum can also be casted to it's assigned integer values. Only define used block types.
/// </summary>
public enum PatternVoxType { VoxPatternA = 0, VoxPatternB = 1, VoxPatternC = 2 }

public class PatternVoxelManager
{
    /// <summary>
    /// Singleton object of the PatternManager class. Refer to this to access the date inside the object.
    /// </summary>
    public static PatternVoxelManager Instance { get; } = new PatternVoxelManager();

    private static List<PatternVox> _patternsVox;
    /// <summary>
    /// returns a read only list of the patterns defined in the project
    /// </summary>
    public static ICollection<PatternVox> PatternVoxS => new ReadOnlyCollection<PatternVox>(_patternsVox);

    private PatternVoxelManager()
    {
        _patternsVox = new List<PatternVox>();
        List<Voxel> patternVoxA = new List<Voxel>();
        patternVoxA.Add(new Voxel(new Vector3Int(0, 0, 0), new List<Vector3Int>()
        {
            //PosibleDirections in (0,0,0)
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 1, 0)
        }));
        patternVoxA.Add(new Voxel(new Vector3Int(0, 0, 1), new List<Vector3Int>()));
        patternVoxA.Add(new Voxel(new Vector3Int(0, 0, 2), new List<Vector3Int>()));
        patternVoxA.Add(new Voxel(new Vector3Int(1, 0, 2), new List<Vector3Int>()
        {
            new Vector3Int(1, 0, 1),
            new Vector3Int(0, -1, 0),
            new Vector3Int(0, 1, 0)

        }));
        AddPatternVox(patternVoxA, PatternVoxType.VoxPatternA);
    
    }

    public bool AddPatternVox(List<Voxel> voxels, PatternVoxType type)
    {

        //only add valid patterns
        if (voxels == null) return false;
        if (voxels[0].Index != Vector3Int.zero) return false;
        if (_patternsVox.Count(p => p.Type == type) > 0) return false;
        _patternsVox.Add(new PatternVox(new List<Voxel>(voxels), type));
        return true;
    }


    //public static PatternVox GetPatternByType(PatternType type) => PatternVoxS.First(p => p.Type == type);

    //public static PatternVox GetPatternByType(PatternType type) => PatternVoxS.First(p => p.Type == type);




}



public class PatternVox
{
    /// <summary>
    /// The patterns are saved as ReadOnlyCollections rather than list so that once defined, the pattern can never be changed
    /// </summary>
    public ReadOnlyCollection<Voxel> Voxels { get; }
    public PatternVoxType Type { get; }

    /// <summary>
    /// Pattern constructor. The indices will be stored in a ReadOnlyCollection
    /// </summary>
    ///<param name = "indices" > List of indices that define the patter.The indices should always relate to Vector3In(0,0,0) as anchor point</param>
    /// <param name="type">The PatternType of this pattern to add. Each type can only exist once</param>
    public PatternVox(List<Voxel> voxels, PatternVoxType type)
    {
        Voxels = new ReadOnlyCollection<Voxel>(voxels);
        Type = type;
    }
}


