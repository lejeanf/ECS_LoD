using Unity.Entities;
using UnityEngine;

namespace CustomLOD
{
    /// <summary>
    /// Debug component to visualize LOD distances and current LOD level
    /// </summary>
    public struct LODDebugInfo : IComponentData
    {
        public bool ShowGizmos;
        public bool ShowDebugText;
    }

    /// <summary>
    /// Add this component to any GameObject with LODGroup to enable debug visualization
    /// </summary>
    public class LODAuthoring : MonoBehaviour
    {
        [Header("Debug Visualization")]
        [Tooltip("Show LOD distance spheres in Scene view")]
        public bool ShowGizmos = true;

        [Tooltip("Show current LOD level text in Scene view")]
        public bool ShowDebugText = true;

        // Internal reference to the entity (set during baking)
        [HideInInspector]
        public Unity.Entities.Entity Entity;

        class Baker : Baker<LODAuthoring>
        {
            public override void Bake(LODAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // Store the entity reference back to the authoring component
                // This allows runtime systems to sync values
                authoring.Entity = entity;

                AddComponent(entity, new LODDebugInfo
                {
                    ShowGizmos = authoring.ShowGizmos,
                    ShowDebugText = authoring.ShowDebugText
                });
            }
        }
    }
}