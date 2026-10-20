# 📂 Project Architecture

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