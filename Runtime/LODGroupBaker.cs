using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace CustomLOD
{
    public class LODGroupBaker : Baker<LODGroup>
    {
        public override void Bake(LODGroup authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            // Get LOD information from the LODGroup
            var lods = authoring.GetLODs();
            if (lods.Length == 0)
                return;

            // Add main LOD component
            var lodComponent = new LODGroupComponent
            {
                ObjectSize = authoring.size,
                LODCount = lods.Length,
                CurrentLOD = 0,
                LOD0Entity = Entity.Null,
                LOD1Entity = Entity.Null,
                LOD2Entity = Entity.Null,
                LOD3Entity = Entity.Null,
                LOD4Entity = Entity.Null
            };

            // Add transitions component
            var transitions = new LODTransitions();

            // Create a dynamic buffer for LOD level info (more flexible approach)
            var lodBuffer = AddBuffer<LODLevelInfo>(entity);

            // Process each LOD level
            for (int i = 0; i < lods.Length && i < 5; i++)
            {
                var lod = lods[i];
                
                // Calculate max distance based on screen height percentage
                // This is a rough approximation - adjust FOV and multiplier as needed
                float screenHeightPercent = lod.screenRelativeTransitionHeight;
                float maxDistance = CalculateMaxDistance(authoring.size, screenHeightPercent);

                // Store in transitions component
                switch (i)
                {
                    case 0:
                        transitions.LOD0MaxDistance = maxDistance;
                        break;
                    case 1:
                        transitions.LOD1MaxDistance = maxDistance;
                        break;
                    case 2:
                        transitions.LOD2MaxDistance = maxDistance;
                        break;
                    case 3:
                        transitions.LOD3MaxDistance = maxDistance;
                        break;
                    case 4:
                        transitions.LOD4MaxDistance = maxDistance;
                        break;
                }

                // Process renderers in this LOD level
                if (lod.renderers != null && lod.renderers.Length > 0)
                {
                    // Find the first renderer's GameObject
                    var lodGameObject = lod.renderers[0]?.gameObject;
                    if (lodGameObject != null)
                    {
                        // Get entity for this LOD's GameObject
                        var lodEntity = GetEntity(lodGameObject, TransformUsageFlags.Dynamic);
                        
                        // Store reference in main component
                        switch (i)
                        {
                            case 0:
                                lodComponent.LOD0Entity = lodEntity;
                                break;
                            case 1:
                                lodComponent.LOD1Entity = lodEntity;
                                break;
                            case 2:
                                lodComponent.LOD2Entity = lodEntity;
                                break;
                            case 3:
                                lodComponent.LOD3Entity = lodEntity;
                                break;
                            case 4:
                                lodComponent.LOD4Entity = lodEntity;
                                break;
                        }

                        // Add to buffer
                        lodBuffer.Add(new LODLevelInfo
                        {
                            LODEntity = lodEntity,
                            MaxDistance = maxDistance,
                            ScreenHeightPercent = screenHeightPercent
                        });

                        // Add tag to LOD child entity
                        AddComponent(lodEntity, new LODChildTag
                        {
                            LODLevel = i,
                            ParentLODGroup = entity
                        });

                        // Initially disable all LODs except LOD 0
                        if (i != 0)
                        {
                            AddComponent(lodEntity, new Disabled());
                        }
                    }
                }
                // Set cull distance (last LOD's max distance)
                transitions.CullDistance = i == 0 ? transitions.LOD0MaxDistance :
                    i == 1 ? transitions.LOD1MaxDistance :
                    i == 2 ? transitions.LOD2MaxDistance :
                    i == 3 ? transitions.LOD3MaxDistance :
                    transitions.LOD4MaxDistance;
            }

            // Add components to entity
            AddComponent(entity, lodComponent);
            AddComponent(entity, transitions);
        }
        private float CalculateMaxDistance(float objectSize, float screenHeightPercent)
        {
            if (screenHeightPercent <= 0.0001f)
                return 10000f; // Very far

            // Assuming 60 degree FOV (common default)
            float fov = 60f * Mathf.Deg2Rad;
            float tanHalfFOV = Mathf.Tan(fov / 2f);
            
            // Calculate distance where object would occupy screenHeightPercent of screen
            float distance = (objectSize / (2f * tanHalfFOV)) / screenHeightPercent;
            
            return distance;
        }
    }
}
