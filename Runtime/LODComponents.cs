using Unity.Entities;

namespace CustomLOD
{
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

    public struct LODTransitions : IComponentData
    {
        public float LOD0MaxDistance;
        public float LOD1MaxDistance;
        public float LOD2MaxDistance;
        public float LOD3MaxDistance;
        public float LOD4MaxDistance;
        public float CullDistance; 
    }

    /// <summary>
    /// Tag component for LOD child entities. Currently unused but kept for future extensibility.
    ///
    /// NOTE: Due to Unity ECS baking ownership rules, this component cannot be added during baking
    /// from the LODGroupBaker (parent baker cannot modify child entities from other GameObjects).
    ///
    /// To use this component in the future:
    /// 1. Create a separate LODChildAuthoring MonoBehaviour component
    /// 2. Add that component to child LOD renderer GameObjects
    /// 3. Create a baker for LODChildAuthoring that adds this LODChildTag to its own entity
    ///
    /// This follows Unity's "Baker Principles" where each GameObject's baker only modifies its own entity.
    /// </summary>
    public struct LODChildTag : IComponentData
    {
        public int LODLevel;
        public Entity ParentLODGroup;
    }

    public struct LODLevelInfo : IBufferElementData
    {
        public Entity LODEntity;
        public float MaxDistance;
        public float ScreenHeightPercent;
    }
}
