using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using static AxisDirection;

/// <summary>
/// PatternType can be refered to by name. These can become your block names to make your code more readible. This enum can also be casted to it's assigned integer values. Only define used block types.
/// </summary>
public enum PatternType { PatternA = 0, PatternB = 1, PatternC = 2 }

/// <summary>
/// The pattern manager is a singleton class. This means there is only one instance of the PatternManager class in the entire project and it can be refered to anywhere withing the project
/// </summary>
public class PatternManager
{
    /// <summary>
    /// Singleton object of the PatternManager class. Refer to this to access the date inside the object.
    /// </summary>
    public static PatternManager Instance { get; } = new PatternManager();

    private static List<Pattern> _patterns;
    /// <summary>
    /// returns a read only list of the patterns defined in the project
    /// </summary>
    public static ICollection<Pattern> Patterns => new ReadOnlyCollection<Pattern>(_patterns);

    /// <summary>
    /// private constructor. All initial patterns will be defined in here
    /// </summary>
    private PatternManager()
    {
        _patterns = new List<Pattern>();

        #region patternA
        List<Voxel> patternA = new List<Voxel>();
        patternA.Add(new Voxel(new Vector3Int(0, 0, 0), new List<AxisDirection>()
        {
            //PosibleDirections in (0,0,0)
            Xplus,
            Ymin,
            Yplus

        }));
        patternA.Add(new Voxel(new Vector3Int(0, 0, 1), new List<AxisDirection>()));
        patternA.Add(new Voxel(new Vector3Int(0, 0, 2), new List<AxisDirection>()));
        patternA.Add(new Voxel(new Vector3Int(1, 0, 2), new List<AxisDirection>()
        {

            //PosibleDirections in (1,0,2)
            Xplus,
            Ymin,
            Yplus
        }));
        AddPattern(patternA, PatternType.PatternA);
        #endregion

        #region PatternB
        List<Voxel> patternB = new List<Voxel>();
        patternB.Add(new Voxel(new Vector3Int(0, 0, 0), new List<AxisDirection>()
        {
            //PosibleDirections in (0,0,0)
            Xplus,
            Ymin,

        }));
        patternB.Add(new Voxel(new Vector3Int(0, 0, 1), new List<AxisDirection>()));
        patternB.Add(new Voxel(new Vector3Int(0, 0, 2), new List<AxisDirection>()));
        patternB.Add(new Voxel(new Vector3Int(1, 0, 2), new List<AxisDirection>()));
        patternB.Add(new Voxel(new Vector3Int(2, 0, 2), new List<AxisDirection>()));
        patternB.Add(new Voxel(new Vector3Int(0, 0, 3), new List<AxisDirection>()));
        patternB.Add(new Voxel(new Vector3Int(0, 1, 3), new List<AxisDirection>()));
        patternB.Add(new Voxel(new Vector3Int(0, 2, 3), new List<AxisDirection>()
        {
            Xplus,
            Ymin,
            Yplus

        }));
        AddPattern(patternB, PatternType.PatternB);
        #endregion

        #region PatternC
        List<Voxel> patternC = new List<Voxel>();
        patternC.Add(new Voxel(new Vector3Int(0, 0, 0), new List<AxisDirection>()
        {
            //PosibleDirections in (0,0,0)
            Xplus,
            Ymin,
            Yplus
        }));
        patternC.Add(new Voxel(new Vector3Int(0, 0, 1), new List<AxisDirection>()));
        patternC.Add(new Voxel(new Vector3Int(0, 0, 2), new List<AxisDirection>()));
        patternC.Add(new Voxel(new Vector3Int(0, 0, 3), new List<AxisDirection>()));
        patternC.Add(new Voxel(new Vector3Int(0, 1, 3), new List<AxisDirection>()));
        patternC.Add(new Voxel(new Vector3Int(0, 2, 3), new List<AxisDirection>()));
        patternC.Add(new Voxel(new Vector3Int(0, 2, 2), new List<AxisDirection>()));
        patternC.Add(new Voxel(new Vector3Int(0, 2, 1), new List<AxisDirection>()
        {
            Xplus,
            Ymin,
            Yplus

        }));
        AddPattern(patternC, PatternType.PatternC);
        #endregion

    }
    public bool AddPattern(List<Voxel> voxels, PatternType type)
    {

        //only add valid patterns
        if (voxels == null) return false;
        if (voxels[0].Index != Vector3Int.zero) return false;
        if (_patterns.Count(p => p.Type == type) > 0) return false;
        _patterns.Add(new Pattern(new List<Voxel>(voxels), type));
        return true;
    }

    /// <summary>
    /// Return the pattern linked to its type
    /// </summary>
    /// <param name="type">The type to look for</param>
    /// <returns>The pattern linked to the type. Will return null if the type is never defined</returns>
     public static Pattern GetPatternByType(PatternType type) => Patterns.First(p => p.Type == type);
}

/// <summary>
/// The pattern that defines a block. Object of this class should only be made in the PatternManager
/// </summary>
public class Pattern
{
    /// <summary>
    /// The patterns are saved as ReadOnlyCollections rather than list so that once defined, the pattern can never be changed
    /// </summary>
    public ReadOnlyCollection<Voxel> PatternVoxels { get; }
    public PatternType Type { get; }

    /// <summary>
    /// Pattern constructor. The indices will be stored in a ReadOnlyCollection
    /// </summary>
    ///<param name = "indices" > List of indices that define the patter.The indices should always relate to Vector3In(0,0,0) as anchor point</param>
    /// <param name="type">The PatternType of this pattern to add. Each type can only exist once</param>
    public Pattern(List<Voxel> voxels, PatternType type)
    {
        PatternVoxels = new ReadOnlyCollection<Voxel>(voxels);
        Type = type;
    }
}

