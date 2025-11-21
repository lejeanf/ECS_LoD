using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CustomLOD
{
    /// <summary>
    /// Debug component to visualize LOD distances and current LOD level
    /// Add this to your GameObject with LODGroup to see debug info
    /// </summary>
    public struct LODDebugInfo : IComponentData
    {
        public bool ShowGizmos;
        public bool ShowDebugText;
    }

    /// <summary>
    /// Authoring component for LOD debugging
    /// </summary>
    public class LODDebugAuthoring : MonoBehaviour
    {
        public bool ShowGizmos = true;
        public bool ShowDebugText = true;

        class Baker : Baker<LODDebugAuthoring>
        {
            public override void Bake(LODDebugAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new LODDebugInfo
                {
                    ShowGizmos = authoring.ShowGizmos,
                    ShowDebugText = authoring.ShowDebugText
                });
            }
        }
    }

    /// <summary>
    /// System to display LOD debug information
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class LODDebugSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var camera = Camera.main;
            if (camera == null) return;

            float3 cameraPos = camera.transform.position;

            Entities
                .WithoutBurst()
                .ForEach((
                    Entity entity,
                    in LODGroupComponent lodGroup,
                    in LODTransitions transitions,
                    in LocalToWorld localToWorld,
                    in LODDebugInfo debugInfo) =>
                {
                    if (!debugInfo.ShowGizmos && !debugInfo.ShowDebugText)
                        return;

                    float3 position = localToWorld.Position;
                    float distance = math.distance(cameraPos, position);

                    // Draw gizmos
                    if (debugInfo.ShowGizmos)
                    {
                        DrawLODGizmos(position, transitions, lodGroup.LODCount, lodGroup.ObjectSize);
                    }

                    // Show debug text
                    if (debugInfo.ShowDebugText)
                    {
                        ShowDebugText(position, distance, lodGroup.CurrentLOD, lodGroup.LODCount);
                    }
                }).Run();
        }

        private void DrawLODGizmos(float3 position, LODTransitions transitions, int lodCount, float objectSize)
        {
            // Draw spheres at each LOD transition distance
            Color[] lodColors = new Color[]
            {
                Color.green,
                Color.yellow,
                new Color(1f, 0.5f, 0f), // Orange
                Color.red,
                Color.magenta
            };

            if (lodCount > 0 && transitions.LOD0MaxDistance > 0)
            {
                Gizmos.color = lodColors[0];
                Gizmos.DrawWireSphere(position, transitions.LOD0MaxDistance);
            }
            if (lodCount > 1 && transitions.LOD1MaxDistance > 0)
            {
                Gizmos.color = lodColors[1];
                Gizmos.DrawWireSphere(position, transitions.LOD1MaxDistance);
            }
            if (lodCount > 2 && transitions.LOD2MaxDistance > 0)
            {
                Gizmos.color = lodColors[2];
                Gizmos.DrawWireSphere(position, transitions.LOD2MaxDistance);
            }
            if (lodCount > 3 && transitions.LOD3MaxDistance > 0)
            {
                Gizmos.color = lodColors[3];
                Gizmos.DrawWireSphere(position, transitions.LOD3MaxDistance);
            }
            if (lodCount > 4 && transitions.LOD4MaxDistance > 0)
            {
                Gizmos.color = lodColors[4];
                Gizmos.DrawWireSphere(position, transitions.LOD4MaxDistance);
            }

            // Draw cull distance
            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(position, transitions.CullDistance);
        }

        private void ShowDebugText(float3 position, float distance, int currentLOD, int lodCount)
        {
#if UNITY_EDITOR
            var style = new GUIStyle();
            style.normal.textColor = currentLOD == -1 ? Color.red : Color.white;
            style.fontStyle = FontStyle.Bold;
            
            string text = currentLOD == -1 
                ? $"CULLED\nDist: {distance:F1}m" 
                : $"LOD {currentLOD}/{lodCount-1}\nDist: {distance:F1}m";
            
            UnityEditor.Handles.Label(position, text, style);
#endif
        }
    }
}
