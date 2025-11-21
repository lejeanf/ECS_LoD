using Unity.Entities;
using UnityEngine;

namespace CustomLOD
{
    /// <summary>
    /// Syncs LODAuthoring MonoBehaviour values to LODDebugInfo entity components at runtime
    /// This allows you to toggle debug visualization during Play mode
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class LODDebugSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            // Find all LODAuthoring MonoBehaviours and sync their values to entities
            var allLODAuthoring = GameObject.FindObjectsByType<LODAuthoring>(FindObjectsSortMode.None);

            foreach (var lodAuthoring in allLODAuthoring)
            {
                // Use the stored entity reference from the authoring component
                var entity = lodAuthoring.Entity;

                // Verify the entity is valid and has the debug component
                if (entity != Entity.Null && EntityManager.Exists(entity) && EntityManager.HasComponent<LODDebugInfo>(entity))
                {
                    // Get current component data
                    var currentDebugInfo = EntityManager.GetComponentData<LODDebugInfo>(entity);

                    // Only update if values changed (to avoid unnecessary writes)
                    if (currentDebugInfo.ShowGizmos != lodAuthoring.ShowGizmos ||
                        currentDebugInfo.ShowDebugText != lodAuthoring.ShowDebugText)
                    {
                        EntityManager.SetComponentData(entity, new LODDebugInfo
                        {
                            ShowGizmos = lodAuthoring.ShowGizmos,
                            ShowDebugText = lodAuthoring.ShowDebugText
                        });
                    }
                }
            }
        }
    }
}
