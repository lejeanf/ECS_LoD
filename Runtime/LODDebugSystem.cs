using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CustomLOD
{
    /// <summary>
    /// System to display LOD debug information using Debug.DrawLine for runtime visualization
    /// Note: The LODDebugInfo component is added by LODAuthoring.cs
    /// </summary>
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    public partial class LODDebugSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var camera = Camera.main;
            if (camera == null) return;

            float3 cameraPos = camera.transform.position;

            // Use Debug.DrawLine instead of Gizmos since this runs in Update, not OnDrawGizmos
            Entities
                .WithoutBurst()
                .ForEach((
                    Entity entity,
                    in LODGroupComponent lodGroup,
                    in LODTransitions transitions,
                    in LocalToWorld localToWorld,
                    in LODDebugInfo debugInfo) =>
                {
                    // Early exit if both debug features are disabled
                    if (!debugInfo.ShowGizmos && !debugInfo.ShowDebugText)
                        return;

                    float3 position = localToWorld.Position;
                    float distance = math.distance(cameraPos, position);

                    // Draw debug lines for LOD distances (only if ShowGizmos is enabled)
                    if (debugInfo.ShowGizmos)
                    {
                        DrawLODDebugLines(position, transitions, lodGroup.LODCount, lodGroup.CurrentLOD);
                    }

                    // Show debug text using Debug.Log-style text (only if ShowDebugText is enabled)
                    if (debugInfo.ShowDebugText)
                    {
                        DrawDebugText(position, distance, lodGroup.CurrentLOD, lodGroup.LODCount);
                    }
                }).Run();
        }

        private void DrawLODDebugLines(float3 position, LODTransitions transitions, int lodCount, int currentLOD)
        {
            // Color for each LOD level
            Color[] lodColors = new Color[]
            {
                Color.green,        // LOD0
                Color.yellow,       // LOD1
                new Color(1f, 0.5f, 0f), // LOD2 - Orange
                Color.red,          // LOD3
                Color.magenta       // LOD4
            };

            // Draw circles using Debug.DrawLine for each LOD distance
            int segments = 32;

            if (lodCount > 0 && transitions.LOD0MaxDistance > 0)
                DrawCircle(position, transitions.LOD0MaxDistance, lodColors[0], segments);

            if (lodCount > 1 && transitions.LOD1MaxDistance > 0)
                DrawCircle(position, transitions.LOD1MaxDistance, lodColors[1], segments);

            if (lodCount > 2 && transitions.LOD2MaxDistance > 0)
                DrawCircle(position, transitions.LOD2MaxDistance, lodColors[2], segments);

            if (lodCount > 3 && transitions.LOD3MaxDistance > 0)
                DrawCircle(position, transitions.LOD3MaxDistance, lodColors[3], segments);

            if (lodCount > 4 && transitions.LOD4MaxDistance > 0)
                DrawCircle(position, transitions.LOD4MaxDistance, lodColors[4], segments);

            // Draw cull distance in black
            DrawCircle(position, transitions.CullDistance, Color.black, segments);

            // Draw a line from object to camera
            Color distanceLineColor = currentLOD >= 0 && currentLOD < lodColors.Length
                ? lodColors[currentLOD]
                : Color.red;
            Debug.DrawLine(position, Camera.main.transform.position, distanceLineColor);
        }

        private void DrawCircle(float3 center, float radius, Color color, int segments)
        {
            // Draw horizontal circle (XZ plane)
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * math.PI * 2f;
                float angle2 = ((i + 1) / (float)segments) * math.PI * 2f;

                float3 point1 = center + new float3(math.cos(angle1) * radius, 0, math.sin(angle1) * radius);
                float3 point2 = center + new float3(math.cos(angle2) * radius, 0, math.sin(angle2) * radius);

                Debug.DrawLine(point1, point2, color);
            }

            // Draw vertical circle (XY plane)
            for (int i = 0; i < segments; i++)
            {
                float angle1 = (i / (float)segments) * math.PI * 2f;
                float angle2 = ((i + 1) / (float)segments) * math.PI * 2f;

                float3 point1 = center + new float3(math.cos(angle1) * radius, math.sin(angle1) * radius, 0);
                float3 point2 = center + new float3(math.cos(angle2) * radius, math.sin(angle2) * radius, 0);

                Debug.DrawLine(point1, point2, color);
            }
        }

        private void DrawDebugText(float3 position, float distance, int currentLOD, int lodCount)
        {
            // Draw text using Debug.DrawRay to create a visual indicator
            // We can't use Handles.Label from an ECS system, so we use an alternative approach

            Color textColor = currentLOD == -1 ? Color.red : Color.cyan;
            float3 textPosition = position + new float3(0, 2, 0);

            // Draw an upward ray to indicate the object (like an exclamation point)
            Debug.DrawRay(position, new float3(0, 2, 0), textColor);

            // Draw a small cross at the top to make it more visible
            Debug.DrawLine(textPosition + new float3(-0.5f, 0, 0), textPosition + new float3(0.5f, 0, 0), textColor);
            Debug.DrawLine(textPosition + new float3(0, 0, -0.5f), textPosition + new float3(0, 0, 0.5f), textColor);

            // For actual text, we use DebugTextComponent which can be rendered by a separate MonoBehaviour
            // or use the Scene view's Debug.Log with spatial context
            #if UNITY_EDITOR
            string logMessage = currentLOD == -1
                ? $"[LOD Debug] CULLED at {distance:F1}m"
                : $"[LOD Debug] LOD {currentLOD}/{lodCount - 1} at {distance:F1}m";

            // Only log periodically to avoid spam (every 60 frames = ~1 second)
            if (UnityEngine.Time.frameCount % 60 == 0)
            {
                Debug.Log(logMessage, null);
            }
            #endif
        }
    }
}
