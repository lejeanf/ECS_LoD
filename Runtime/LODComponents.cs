using Unity.Entities;
using Unity.Mathematics;

namespace CustomLOD
{
    /// <summary>
    /// Main LOD component - attached to the root entity with LOD group
    /// </summary>
    public struct LODGroupComponent : IComponentData
    {
        public float ObjectSize;           // Size of the object (from LODGroup)
        public int LODCount;               // Number of LOD levels
        public Entity LOD0Entity;          // Entity reference for LOD 0
        public Entity LOD1Entity;          // Entity reference for LOD 1
        public Entity LOD2Entity;          // Entity reference for LOD 2
        public Entity LOD3Entity;          // Entity reference for LOD 3
        public Entity LOD4Entity;          // Entity reference for LOD 4
        public int CurrentLOD;             // Currently active LOD level (-1 = culled)
    }

    /// <summary>
    /// Stores the screen height percentages for each LOD transition
    /// </summary>
    public struct LODTransitions : IComponentData
    {
        public float LOD0MaxDistance;      // Max distance for LOD 0 (based on screen height %)
        public float LOD1MaxDistance;      // Max distance for LOD 1
        public float LOD2MaxDistance;      // Max distance for LOD 2
        public float LOD3MaxDistance;      // Max distance for LOD 3
        public float LOD4MaxDistance;      // Max distance for LOD 4
        public float CullDistance;         // Distance beyond which object is culled
    }

    /// <summary>
    /// Tag component for LOD child entities
    /// </summary>
    public struct LODChildTag : IComponentData
    {
        public int LODLevel;               // Which LOD level this entity represents
        public Entity ParentLODGroup;      // Reference back to the parent LOD group
    }

    /// <summary>
    /// Buffer element to store multiple LOD level info (alternative approach)
    /// </summary>
    public struct LODLevelInfo : IBufferElementData
    {
        public Entity LODEntity;           // Entity for this LOD level
        public float MaxDistance;          // Max distance for this LOD
        public float ScreenHeightPercent;  // Original screen height percentage
    }
}
