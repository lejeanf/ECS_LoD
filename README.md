# Custom ECS LOD System for Unity

A complete Level of Detail (LOD) system for Unity's Entity Component System (ECS) that works with subscenes and baked entities.

## The Problem

Unity's built-in `LODGroup` component is GameObject-based and doesn't automatically work with ECS entities in subscenes. When GameObjects are baked to entities, LOD functionality is lost.

## The Solution

This custom ECS LOD system provides:
- Automatic conversion of LODGroup data during baking
- Runtime LOD switching based on camera distance
- Support for up to 5 LOD levels
- Automatic culling beyond max distance
- Debug visualization tools

## Files Included

1. **LODComponents.cs** - Component data structures
2. **LODGroupBaker.cs** - Converts LODGroup to ECS during baking
3. **LODUpdateSystem.cs** - Runtime system for LOD switching
4. **LODUpdateSystemBuffered.cs** - Alternative buffer-based approach (optional)
5. **LODDebugSystem.cs** - Debug visualization tools

## Installation

1. Create a folder in your project: `Assets/Scripts/CustomLOD/`
2. Copy all `.cs` files into this folder
3. Ensure you have the Unity Entities package installed

## Setup

### For Existing Prefabs with LODGroup

Your prefab already has an `LODGroup` component configured. The baker will automatically convert it:

1. Keep your existing LODGroup setup on the prefab
2. The baker will run automatically during subscene baking
3. LOD functionality will work in Play mode

### Optional: Add Debug Visualization

1. Add the `LODDebugAuthoring` component to your prefab (next to LODGroup)
2. Check "Show Gizmos" to see LOD distance spheres in Scene view
3. Check "Show Debug Text" to see current LOD level in Scene view

## How It Works

### Baking Process

When your subscene is baked:
1. `LODGroupBaker` finds the LODGroup component
2. Extracts LOD levels, transition distances, and renderer references
3. Creates ECS components:
   - `LODGroupComponent` - Main LOD data
   - `LODTransitions` - Distance thresholds
   - `LODLevelInfo` buffer - Flexible LOD storage
   - `LODChildTag` - Tags on child LOD entities
4. Disables all LOD meshes except LOD0

### Runtime Behavior

Every frame, `LODUpdateSystem`:
1. Gets camera position (from Camera.main)
2. Calculates distance to each LOD group entity
3. Determines appropriate LOD level
4. Enables/disables the correct LOD mesh entities
5. Handles culling when beyond max distance

## Distance Calculation

LOD distances are calculated from Unity's screen height percentages:

```
distance = (objectSize / (2 * tan(FOV/2))) / screenHeightPercent
```

Default FOV is 60°. Adjust in `LODGroupBaker.CalculateMaxDistance()` if needed.

## Configuration

### Adjust FOV for Distance Calculation

In `LODGroupBaker.cs`, modify line ~95:

```csharp
float fov = 60f * Mathf.Deg2Rad; // Change this to match your camera FOV
```

### Use Buffer-Based System (Optional)

For more flexibility with varying LOD counts:

1. In `LODUpdateSystemBuffered.cs`, change line 27:
   ```csharp
   state.Enabled = true; // Enable this system
   ```
2. In `LODUpdateSystem.cs`, change line 19:
   ```csharp
   state.Enabled = false; // Disable the default system
   ```

## Troubleshooting

### LODs Not Switching

**Issue**: LOD levels don't change as camera moves

**Solutions**:
1. Verify `Camera.main` exists in your scene
2. Check that subscene is baked (not in live conversion mode)
3. Add `LODDebugAuthoring` to see distances and current LOD
4. Verify LODGroup is on the parent GameObject (not on child renderers)

### Baking Errors

**Issue**: Errors during subscene baking

**Solutions**:
1. Ensure all LOD renderers are children of the LODGroup GameObject
2. Check that LODGroup has valid LOD levels configured
3. Verify all renderer GameObjects exist in the hierarchy

### Performance Issues

**Issue**: Frame rate drops with many LOD objects

**Solutions**:
1. Use fewer LOD groups (combine objects where possible)
2. Consider implementing spatial partitioning (octrees)
3. Update LOD checks less frequently (modify system update rate)
4. Use the buffer-based system for better data locality

### Distance Seems Wrong

**Issue**: LOD switches at incorrect distances

**Solutions**:
1. Verify your camera FOV matches the baker's FOV setting
2. Check the "Object Size" in LODGroup component (should match object bounds)
3. Adjust screen height percentages in Unity's LODGroup
4. Test at runtime with debug visualization enabled

## Performance Optimization

### Reduce Update Frequency

Add a timer to only update every few frames:

```csharp
[BurstCompile]
public partial struct LODUpdateSystem : ISystem
{
    private double lastUpdateTime;
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // Only update every 0.1 seconds
        if (SystemAPI.Time.ElapsedTime - lastUpdateTime < 0.1)
            return;
            
        lastUpdateTime = SystemAPI.Time.ElapsedTime;
        
        // ... rest of update code
    }
}
```

### Use Spatial Partitioning

For hundreds/thousands of LOD groups, consider implementing a spatial hash or octree to only check LODs near the camera.

## Extending the System

### Add Cross-Fade Support

Enable `LODFadeSystem` and implement shader-based cross-fading:

1. Enable both current and next LOD levels
2. Set material properties for fade values
3. Use dithering or alpha blending in shader

### Multiple Camera Support

Modify `GetCameraPosition()` to handle multiple cameras:

```csharp
private float3 GetCameraPosition()
{
    // Find closest camera or use priority system
    var cameras = Camera.allCameras;
    // ... implement camera selection logic
}
```

### Add LOD Bias/Override

Add a component to force specific LOD levels:

```csharp
public struct LODBias : IComponentData
{
    public int ForcedLODLevel; // -1 for automatic
    public float DistanceMultiplier; // Scale distances
}
```

## Known Limitations

1. Maximum 5 LOD levels (can be extended by adding more Entity fields)
2. Requires `Camera.main` to be set
3. Distance calculation assumes perspective camera
4. No built-in support for LOD fade modes
5. Each LOD group can only reference one renderer per LOD level in the main approach

## Example Scene Setup

```
Scene
├── Subscene
│   ├── LOD_Building_Prefab (x50)
│   │   ├── LODGroup component
│   │   ├── LODDebugAuthoring component (optional)
│   │   ├── LOD0 (child GameObject with MeshRenderer)
│   │   ├── LOD1 (child GameObject with MeshRenderer)
│   │   ├── LOD2 (child GameObject with MeshRenderer)
│   │   └── LOD3 (child GameObject with MeshRenderer)
│   └── ... more prefabs
└── Main Camera (tagged as MainCamera)
```

## Credits

Built for Unity Entities 1.0+ and Unity 2022.3+

## License

Free to use and modify for your projects.
