# ECS LOD System - Quick Reference

## Component Architecture

```
┌─────────────────────────────────────────────────┐
│  LODGroup GameObject (Prefab)                   │
│  - LODGroup component (Unity built-in)          │
│  - LODDebugAuthoring (optional)                 │
│    └─ LOD0 GameObject (MeshRenderer)            │
│    └─ LOD1 GameObject (MeshRenderer)            │
│    └─ LOD2 GameObject (MeshRenderer)            │
│    └─ LOD3 GameObject (MeshRenderer)            │
└─────────────────────────────────────────────────┘
                    │
                    │ BAKING (LODGroupBaker)
                    ▼
┌─────────────────────────────────────────────────┐
│  Parent Entity (LOD Group)                      │
│  ┌────────────────────────────────────────┐    │
│  │ LODGroupComponent                      │    │
│  │ - ObjectSize                           │    │
│  │ - LODCount                             │    │
│  │ - LOD0Entity → Entity                  │    │
│  │ - LOD1Entity → Entity                  │    │
│  │ - LOD2Entity → Entity                  │    │
│  │ - LOD3Entity → Entity                  │    │
│  │ - LOD4Entity → Entity                  │    │
│  │ - CurrentLOD                           │    │
│  └────────────────────────────────────────┘    │
│  ┌────────────────────────────────────────┐    │
│  │ LODTransitions                         │    │
│  │ - LOD0MaxDistance                      │    │
│  │ - LOD1MaxDistance                      │    │
│  │ - LOD2MaxDistance                      │    │
│  │ - LOD3MaxDistance                      │    │
│  │ - LOD4MaxDistance                      │    │
│  │ - CullDistance                         │    │
│  └────────────────────────────────────────┘    │
│  ┌────────────────────────────────────────┐    │
│  │ DynamicBuffer<LODLevelInfo>            │    │
│  │ [0] LODEntity, MaxDistance, %          │    │
│  │ [1] LODEntity, MaxDistance, %          │    │
│  │ [2] LODEntity, MaxDistance, %          │    │
│  └────────────────────────────────────────┘    │
│  - LocalToWorld (position)                     │
│  - LODDebugInfo (optional)                     │
└─────────────────────────────────────────────────┘
                    │
                    │ References
                    ▼
┌─────────────────────────────────────────────────┐
│  Child Entity (LOD0)                            │
│  ┌────────────────────────────────────────┐    │
│  │ LODChildTag                            │    │
│  │ - LODLevel: 0                          │    │
│  │ - ParentLODGroup → Parent Entity       │    │
│  └────────────────────────────────────────┘    │
│  - MeshRenderer components                     │
│  - LocalToWorld                                │
│  - Disabled (when inactive)                    │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│  Child Entity (LOD1)                            │
│  - LODChildTag (LODLevel: 1)                   │
│  - MeshRenderer components                     │
│  - Disabled ✓                                  │
└─────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────┐
│  Child Entity (LOD2)                            │
│  - LODChildTag (LODLevel: 2)                   │
│  - MeshRenderer components                     │
│  - Disabled ✓                                  │
└─────────────────────────────────────────────────┘
```

## Runtime Flow

```
┌──────────────────────┐
│  LODUpdateSystem     │
│  (every frame)       │
└──────────────────────┘
          │
          ▼
┌──────────────────────────────────────┐
│ 1. Get Camera.main position          │
└──────────────────────────────────────┘
          │
          ▼
┌──────────────────────────────────────┐
│ 2. For each LODGroupComponent:       │
│    - Get entity position             │
│    - Calculate distance to camera    │
└──────────────────────────────────────┘
          │
          ▼
┌──────────────────────────────────────┐
│ 3. Determine LOD level:               │
│    if distance <= LOD0MaxDistance     │
│       → LOD 0                         │
│    else if distance <= LOD1MaxDistance│
│       → LOD 1                         │
│    ... etc                            │
│    else if distance > CullDistance    │
│       → -1 (culled)                   │
└──────────────────────────────────────┘
          │
          ▼
┌──────────────────────────────────────┐
│ 4. If LOD changed:                    │
│    - Add Disabled to old LOD entity   │
│    - Remove Disabled from new LOD     │
│    - Update CurrentLOD                │
└──────────────────────────────────────┘
```

## Distance Calculation Formula

```
Screen Height % → World Distance

Given:
- objectSize: from LODGroup.size
- screenHeightPercent: from LOD.screenRelativeTransitionHeight
- FOV: camera field of view (default 60°)

Calculate:
distance = (objectSize / (2 * tan(FOV/2))) / screenHeightPercent

Example:
- objectSize = 10m
- screenHeightPercent = 0.50 (50%)
- FOV = 60°

distance = (10 / (2 * tan(30°))) / 0.50
        = (10 / (2 * 0.577)) / 0.50
        = (10 / 1.154) / 0.50
        = 8.66 / 0.50
        = 17.32m
```

## LOD Level Decision Tree

```
Distance from Camera
        │
        ▼
   ┌────────────────┐
   │ > CullDistance?│───YES──→ Cull (LOD = -1)
   └────────────────┘
          │ NO
          ▼
   ┌────────────────┐
   │<= LOD0MaxDist? │───YES──→ Show LOD 0
   └────────────────┘
          │ NO
          ▼
   ┌────────────────┐
   │<= LOD1MaxDist? │───YES──→ Show LOD 1
   └────────────────┘
          │ NO
          ▼
   ┌────────────────┐
   │<= LOD2MaxDist? │───YES──→ Show LOD 2
   └────────────────┘
          │ NO
          ▼
   ┌────────────────┐
   │<= LOD3MaxDist? │───YES──→ Show LOD 3
   └────────────────┘
          │ NO
          ▼
        LOD 4 or highest available
```

## Component Queries

### LODUpdateSystem Query

```csharp
Query<
    Entity,                      // Entity reference
    RefRW<LODGroupComponent>,    // Read/Write LOD group data
    RefRO<LocalToWorld>,         // Read position
    RefRO<LODTransitions>        // Read transition distances
>
```

### Debug System Query

```csharp
Query<
    Entity,
    RefRO<LODGroupComponent>,
    RefRO<LODTransitions>,
    RefRO<LocalToWorld>,
    RefRO<LODDebugInfo>
>
```

## Common Modifications

### 1. Change Update Frequency

```csharp
// In LODUpdateSystem.OnUpdate()
private double lastUpdateTime;

if (SystemAPI.Time.ElapsedTime - lastUpdateTime < 0.1) // Update every 0.1s
    return;
lastUpdateTime = SystemAPI.Time.ElapsedTime;
```

### 2. Adjust Distance Multiplier

```csharp
// In LODUpdateJob.DetermineLODLevel()
float adjustedDistance = distance * 0.8f; // Make LOD switch closer
```

### 3. Add LOD Bias

```csharp
public struct LODBias : IComponentData
{
    public float DistanceMultiplier; // 1.0 = normal, 0.5 = half distance
}

// Then multiply distance by this value
float adjustedDistance = distance * lodBias.DistanceMultiplier;
```

### 4. Support More LOD Levels

```csharp
// Add to LODGroupComponent:
public Entity LOD5Entity;
public Entity LOD6Entity;
// ... etc

// Update LODTransitions similarly
```

## Debugging Tips

1. **Enable Gizmos**: Add `LODDebugAuthoring` to see distances
2. **Watch Scene View**: Green = LOD0, Yellow = LOD1, Orange = LOD2, Red = LOD3
3. **Check Entity Inspector**: Verify components are present after baking
4. **Console Logs**: Add debug logs in `DetermineLODLevel()`
5. **Frame Debugger**: Check which meshes are actually rendering

## Performance Metrics

Typical performance (Unity 2022.3, modern CPU):

| LOD Groups | Update Time | FPS Impact |
|-----------|-------------|------------|
| 100       | ~0.1ms      | Negligible |
| 1,000     | ~0.8ms      | < 1%       |
| 10,000    | ~5-8ms      | ~10-15%    |

*Note: With burst compilation and proper job scheduling*

## File Dependencies

```
LODComponents.cs
    ↓ (used by)
LODGroupBaker.cs ←── LODGroup (Unity)
    ↓ (creates)
[Baked Entity Components]
    ↓ (used by)
LODUpdateSystem.cs
LODUpdateSystemBuffered.cs (alternative)
LODDebugSystem.cs (optional)
```
