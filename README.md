# ECS Level Of Detail System

A Level of Detail (LOD) system for Unity's Entity Component System (ECS). Automatically converts Unity's standard `LODGroup` components to ECS during subscene baking, enabling distance-based LOD switching for baked entities.

## Features

- **Automatic Conversion**: Standard Unity `LODGroup` components are automatically converted to ECS during baking
- **Distance-Based Switching**: Runtime LOD level selection based on camera distance
- **Multi-Level Support**: Supports up to 5 LOD levels with automatic culling
- **Debug Visualization**: Optional gizmos and on-screen display for LOD distances and current levels
- **Performance Optimized**: Burst-compiled systems for efficient LOD updates

## Installation

### Via Git URL

1. Open Unity Package Manager (Window > Package Manager)
2. Click the `+` button and select "Add package from git URL"
3. Enter the repository URL
4. Click "Add"

### Via Local Package

1. Clone or download this repository
2. Open Unity Package Manager (Window > Package Manager)
3. Click the `+` button and select "Add package from disk"
4. Navigate to the package folder and select `package.json`
5. Click "Open"

## Quick Start

### Basic Usage

1. Create a SubScene in your scene (GameObject > Scene > Sub Scene)
2. Add a GameObject to the SubScene with a `LODGroup` component
3. Configure the LODGroup with your LOD levels and renderers as usual
4. Enter Play mode - LOD switching happens automatically

The `LODGroupBaker` automatically converts the LODGroup during subscene baking. No additional setup required.

### Debug Visualization (Optional)

To visualize LOD distances and see the current LOD level:

1. Add the `LODAuthoring` component to any GameObject that has a `LODGroup`
2. Enable "Show Gizmos" to see LOD distance spheres in Scene view
3. Enable "Show Debug Text" to display the current LOD level

**Note**: `LODAuthoring` is purely for debug visualization and is not required for LOD functionality.

## Requirements

- Unity 2022.3 or later
- Unity Entities package (com.unity.entities)
- A scene with Camera.main set (required for distance calculations)

## License

<img src="https://licensebuttons.net/l/by-nc-sa/3.0/88x31.png"></img>
