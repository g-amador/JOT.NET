You are helping me continue JOT.NET, a modern C#/.NET reimplementation of the original Java-based JOT (Just One Thing) Modular Multi-purpose Game Engine.

The project is experimental and not production-ready.

Always inspect the actual repository before making assumptions. The code is the source of truth.

# PROJECT GITHUB REPO
https://github.com/g-amador/JOT.NET/tree/V1.0

# PROJECT ORGANIZATION
Directory structure:
```text
JOT.NET/
├── AI.md                           # Prompts for expanding the game
├── ARCHITECTURE.md                 # Detailed project architecture & directory structure
├── LICENSE                         # License details
├── README.md                       # Main project documentation
└── src/
    ├── Directory.Packages.props
    ├── JOT.NET.slnx
    ├── client-browser/
    │   └── JOT.Client.Browser/
    │       ├── _Imports.razor
    │       ├── App.razor
    │       ├── JOT.Client.Browser.csproj
    │       ├── Program.cs
    │       ├── Layout/
    │       │   ├── MainLayout.razor
    │       │   ├── MainLayout.razor.css
    │       │   ├── NavMenu.razor
    │       │   └── NavMenu.razor.css
    │       ├── Pages/
    │       │   ├── Counter.razor
    │       │   ├── Home.razor
    │       │   ├── NotFound.razor
    │       │   └── Weather.razor
    │       ├── Properties/
    │       │   └── launchSettings.json
    │       └── wwwroot/
    │           ├── index.html
    │           ├── css/
    │           │   └── app.css
    │           └── sample-data/
    │               └── weather.json
    ├── client-desktop/
    │   └── JOT.Client.Desktop/
    │       ├── JOT.Client.Desktop.csproj
    │       └── Program.cs
    ├── demos/
    │   └── templates/
    │       ├── CoreTemplate/
    │       │   ├── CoreTemplate.csproj
    │       │   └── Program.cs
    │       └── FullTemplate/
    │           ├── FullTemplate.csproj
    │           └── Program.cs
    ├── engine/
    │   ├── core/
    │   │   ├── JOT.Core.IO/
    │   │   │   └── JOT.Core.IO.csproj
    │   │   ├── JOT.Core.Math/
    │   │   │   ├── JOT.Core.Math.csproj
    │   │   │   ├── Geometry/
    │   │   │   │   ├── AxisAlignedBoundingBox.cs
    │   │   │   │   ├── BoundingSphere.cs
    │   │   │   │   ├── OrientedBoundingBox.cs
    │   │   │   │   ├── Plane.cs
    │   │   │   │   ├── Ray.cs
    │   │   │   │   └── Intersections/
    │   │   │   │       ├── BoxIntersections.cs
    │   │   │   │       ├── RayIntersections.cs
    │   │   │   │       └── SphereIntersections.cs
    │   │   │   └── Transformations/
    │   │   │       └── Transform.cs
    │   │   ├── JOT.Core.Physics/
    │   │   │   └── JOT.Core.Physics.csproj
    │   │   ├── JOT.Core.Rendering/
    │   │   │   ├── JOT.Core.Rendering.csproj
    │   │   │   ├── Cameras/
    │   │   │   │   ├── Camera.cs
    │   │   │   │   ├── ICamera.cs
    │   │   │   │   ├── OrthographicCamera.cs
    │   │   │   │   └── PerspectiveCamera.cs
    │   │   │   ├── Materials/
    │   │   │   │   └── IMaterial.cs
    │   │   │   ├── Meshes/
    │   │   │   │   └── IMesh.cs
    │   │   │   ├── Renderers/
    │   │   │   │   └── IRenderer.cs
    │   │   │   └── Textures/
    │   │   │       └── ITexture.cs
    │   │   └── JOT.Core.Util/
    │   │       └── JOT.Core.Util.csproj
    │   ├── framework/
    │   │   ├── JOT.Framework.Application/
    │   │   │   └── JOT.Framework.Application.csproj
    │   │   ├── JOT.Framework.GUI/
    │   │   │   └── JOT.Framework.GUI.csproj
    │   │   └── JOT.Framework.Managers/
    │   │       └── JOT.Framework.Managers.csproj
    │   ├── infrastructure/
    │   │   ├── JOT.Infrastructure.Assimp/
    │   │   │   └── JOT.Infrastructure.Assimp.csproj
    │   │   ├── JOT.Infrastructure.DotNetty/
    │   │   │   └── JOT.Infrastructure.DotNetty.csproj
    │   │   └── JOT.Infrastructure.Silk/
    │   │       ├── JOT.Infrastructure.Silk.csproj
    │   │       ├── Graphics/
    │   │       │   ├── IGraphicsContext.cs
    │   │       │   └── SilkGraphicsContext.cs
    │   │       ├── Input/
    │   │       │   ├── IInputContext.cs
    │   │       │   ├── IKeyboard.cs
    │   │       │   ├── KeyCode.cs
    │   │       │   ├── SilkInputContext.cs
    │   │       │   └── SilkKeyboard.cs
    │   │       ├── Timing/
    │   │       │   ├── ITimer.cs
    │   │       │   └── SilkTimer.cs
    │   │       └── Windowing/
    │   │           ├── IWindow.cs
    │   │           └── SilkWindow.cs
    │   └── toolkits/
    │       ├── JOT.Toolkit.AI/
    │       │   └── JOT.Toolkit.AI.csproj
    │       ├── JOT.Toolkit.ECS/
    │       │   └── JOT.Toolkit.ECS.csproj
    │       ├── JOT.Toolkit.Geometry/
    │       │   └── JOT.Toolkit.Geometry.csproj
    │       ├── JOT.Toolkit.Networking/
    │       │   └── JOT.Toolkit.Networking.csproj
    │       ├── JOT.Toolkit.Physics/
    │       │   └── JOT.Toolkit.Physics.csproj
    │       └── JOT.Toolkit.Serialization/
    │           └── JOT.Toolkit.Serialization.csproj
    └── server/
        └── JOT.Server/
            ├── JOT.Server.csproj
            └── Program.cs
```

# Goals
- Modular architecture with replaceable subsystems
- Clear layer boundaries
- Downward-only dependencies
- MMO-oriented long-term design
- Educational value and maintainability
- Desktop-first, browser later
- Incremental validation of the foundation

# PRIMARY OBJECTIVE

Build a clean, modular, educational, and maintainable game engine foundation that:

- Preserves clear architectural boundaries.
- Supports replaceable subsystems.
- Validates each layer incrementally.
- Remains suitable for future MMO-oriented development.
- Supports desktop clients first.
- Supports browser clients later.
- Prioritizes correctness and architecture over feature breadth.

# ARCHITECTURE

Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure

## Rules

- A layer may depend only on itself or lower layers.
- No circular dependencies.
- No cross-layer shortcuts.
- Lower layers must not know about higher layers.
- Third-party libraries belong in Infrastructure.
- Platform-specific code belongs in Infrastructure.
- Keep Core independent of platform and graphics APIs.

## Contracts vs Implementations
- Core should own engine-facing abstractions.
- Infrastructure should own platform-specific implementations.
- Core must not depend on concrete Infrastructure implementations.
- Infrastructure must not depend on higher layers.

Avoid adding abstractions without a clear immediate need.


# LAYER RESPONSABILITIES

## Infrastructure

Adapters and integrations:
- Windowing
- Input
- Graphics contexts
- Timing
- Asset importing
- Networking integrations
- External libraries

## Core

Minimal engine concepts:
- Math
- Rendering abstractions
- Input abstractions
- Resource abstractions
- Basic collision concepts
- Utilities

## Toolkits

Optional advanced systems:
- ECS
- AI
- Networking
- Serialization
- Geometry
- Physics

## Framework

Optional higher-level features:
- Application lifecycle
- Scenes
- Assets
- State management
- GUI

# ROADMAP

Roadmap progress:
✅ Phase 1 - Infrastructure
🔄 Phase 2 - Core (Active)
⬜ Phase 3 - Toolkits
⬜ Phase 4 - Framework
⬜ Phase 5 - MMO Prototype
⬜ Phase 6 - Browser Client

# CURRENT STATUS

## Phase 1 — JOT.Infrastructure.Silk

Status: COMPLETED

Phase 1 completion criteria have been satisfied:

1. Application opens a window.
2. Graphics initialize successfully.
3. Background color is displayed.
4. Keyboard input is processed.
5. Escape closes the application.
6. Application exits cleanly.

Build and runtime validation have been performed.

## Phase 2 — Core

Status: ACTIVE PHASE

All new development should prioritize the active phase unless:
- Fixing a bug.
- Correcting an architectural issue.
- Completing a dependency required by the active phase.

Current Core work consists of two areas:

### JOT.Core.Math

Use System.Numerics directly:
1. Vector2
2. Vector3
3. Vector4
4. Quaternion
5. Matrix4x4

Do not create replacement vector or matrix types.

Potential engine concepts:
- Transform
- Ray
- Plane
- BoundingBox (Object aligned and axis aligned)
- BoundingSphere
- Frustum

Only add types that provide clear engine value.

### JOT.Core.Rendering

Create API-independent abstractions such as:
1. IRenderer
2. ICamera
3. IMesh
4. ITexture
5. IMaterial

Do not expose OpenGL, DirectX, Vulkan, or Silk.NET details in Core.

Rendering implementations belong in Infrastructure.

Math and Rendering should remain independent of platform, graphics API, and third-party rendering libraries.

# CORE TEMPLATE VALIDATION

The purpose of CoreTemplate is to prove the architecture end-to-end.

Success criteria:
1. Open a window.
2. Display a background color.
3. Close with Escape.

Nothing more.

# EXPLICIT NON-GOALS

Do not implement:
- AI
- ECS
- Networking
- Physics
- Framework features
- Browser features
- MMO systems

until the current milestone is validated.

# WORKING STYLE

Before making changes:
1. Audit the repository.
2. Inspect project references and dependencies.
3. Identify implemented, incomplete, and inconsistent areas.
4. Report architecture or build issues.

Then:
1. Propose the smallest useful next step.
2. Explain why it belongs in the current phase.
3. Implement incrementally.
4. Avoid broad refactors unless requested.

When an architectural violation is found:
1. Describe it.
2. Explain why it violates the architecture.
3. Propose the smallest correction.

# BUILD & VALIDATION

Before claiming completion:
- Build affected projects.
- Report warnings and errors.
- State what was built.
- State what was executed.
- State what could not be executed and why.

Do not claim runtime validation if it was not performed.

# CODING GUIDELINES

- Follow existing repository conventions.
- Prefer modern idiomatic C#.
- Favor simplicity over extensibility.
- Avoid premature abstraction.
- Keep dependencies minimal.
- Preserve architectural boundaries.
- Do not invent repository details.
- Ask questions only when a decision genuinely blocks progress.

This version is about half the size, but retains nearly all of the architectural guidance that will actually influence an AI agent's behavior.