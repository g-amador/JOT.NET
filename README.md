# 🎮 JOT.NET

<p align="center">
  <strong>A Modular Multi-purpose Game Engine for .NET</strong>
</p>

<p align="center">
  Infrastructure • Core • Toolkits • Framework
</p>

---

JOT.NET is a modern C#/.NET reimplementation of the original Java-based JOT (Just One Thing) Modular Multi-purpose Game Engine.

Designed for experimentation, education, prototyping, and massively multiplayer online games (MMOGs), JOT.NET provides a modular architecture where engine subsystems can be understood, extended, replaced, or rewritten independently.

The project aims to modernize the original engine while preserving its strongest architectural ideas: clear layer separation, replaceable subsystems, and an MMO-oriented design philosophy.

---

## 🚧 Project Status

JOT.NET is currently under active development.

Current priorities:

- Establishing the engine architecture
- Implementing the Infrastructure layer
- Building the desktop client foundation
- Building the browser client foundation
- Defining the Core engine systems
- Preparing the networking architecture for MMO support

At this stage the project should be considered **experimental and not production ready**.

---

## 🎯 Design Goals

JOT.NET is designed around the following objectives:

- ✅ Modular architecture
- ✅ Replaceable subsystems
- ✅ MMO-first design
- ✅ Browser client support
- ✅ Educational value
- ✅ Clean dependency flow
- ✅ Modern .NET ecosystem
- ✅ Long-term maintainability

Every feature added to the engine should support one or more of these goals.

---

### 🧩 Layered Architecture

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

Each layer depends only on itself or lower layers.

### ♻️ Replaceable Components

Major subsystems are designed to be replaced or reimplemented without affecting the rest of the engine.

### 🌐 MMO-Oriented

Networking is treated as a first-class concern rather than an afterthought.

### 🖥️ Multi-Client Strategy

JOT.NET targets both:

- Desktop applications
- Browser-based clients

through a shared engine architecture.

---

## 📚 Table of Contents

1. #-origins
2. #️-architecture-overview
3. #-core-principles
4. #-mmo-first-philosophy
5. #-browser-client-strategy
6. #-layer-responsibilities
7. #-project-organization
7. #️-installation
8. #-usage
9. #-templates-and-examples
10. #-Contributing
11. #-License
12. #-contact--usage-notice

---

## 📜 Origins

JOT.NET is a modern C#/.NET reimplementation of the original Java-based JOT (Just One Thing) Modular Multi-purpose Game Engine.

The original JOT project was created as a modular game engine for experimentation, education, research, and massively multiplayer online games (MMOGs). Its primary objective was to demonstrate that game engines can be designed as collections of replaceable, well-isolated components rather than as monolithic systems.

JOT.NET preserves the architectural vision of the original project while modernizing the technology stack, development workflow, and target platforms.

The result is a new engine built around contemporary .NET technologies while remaining faithful to the principles of modularity, extensibility, and educational value that inspired the original work.

### 🔗 Original Project

- GitHub Repository: <https://github.com/g-amador/JOT>
- Original Engine: JOT – A Modular Multi-purpose Game Engine
- Research Paper: JOT – A Modular Multi-purpose Minimalistic Massively Multiplayer Online Game Engine

### ✅ What Carries Forward

JOT.NET preserves the most important ideas from the original engine:

- Layered architecture
- Modular design
- Replaceable subsystems
- Clear separation between Core, Toolkits, and Framework
- Research and educational focus
- MMO-oriented design philosophy

### 🔄 Modernization

JOT.NET adopts a modern .NET ecosystem while retaining the conceptual architecture of the original project.

| Original JOT (Java) | JOT.NET (.NET) |
|---------------------|----------------|
| JOGL / JogAmp | Silk.NET |
| Apache Commons Math | System.Numerics |
| Assimp Java Bindings | AssimpNet |
| JGroups | DotNetty |
| NetBeans | Visual Studio, VS Code, Rider |
| Maven | .NET SDK, NuGet, Central Package Management |
| Java Desktop Client | Desktop + Browser Clients |

### 🎯 Why a Reimplementation Instead of a Direct Port?

The goal of JOT.NET is not simply to translate Java code into C#.

Instead, the project uses the original architecture as a foundation while taking advantage of modern .NET tooling, improved package management, cross-platform support, browser technologies, and contemporary software engineering practices.

This approach allows the engine to evolve independently while preserving the ideas that made the original JOT unique.

#-table-of-contents

---

## 🏗️ Architecture Overview

JOT.NET follows a layered architecture:

Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure

Dependencies always flow downward.

- No circular dependencies
- No cross-layer shortcuts
- External libraries are isolated in Infrastructure

#-table-of-contents

---

## 📖 Core Principles

The architecture of JOT.NET is guided by a small set of fundamental principles.

These principles are intended to remain stable even as technologies, libraries, and implementation details evolve over time.

---

### ♻️ Everything Is Replaceable

Subsystems should be replaceable with alternative implementations without affecting the rest of the engine.

---

### ⬇️ Dependencies Flow Downward

Dependencies must always flow toward lower layers.

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

The following rules apply:

- A layer may depend only on itself or lower layers.
- Circular dependencies are forbidden.
- Cross-layer shortcuts are forbidden.
- Lower layers must never be aware of higher layers.

These rules help prevent architectural drift as the project grows.

---

### 🔌 Infrastructure Is an Adapter Layer

Infrastructure exists to integrate external technologies and platform-specific functionality.

Infrastructure is responsible for:

- Window creation
- Graphics contexts
- Audio contexts
- Model importing
- Networking adapters
- Third-party library integration

Infrastructure is **not** responsible for:

- Game logic
- Scene management
- AI
- Gameplay systems
- Framework services

Infrastructure adapts external technologies to the engine, but should contain as little engine-specific behavior as possible.

---

### ⚙️ Core Must Remain Minimal

The Core layer contains the minimum functionality required to classify JOT.NET as a game engine.

Core responsibilities include:

- Mathematics
- Resource abstractions
- Rendering abstractions
- Basic collision detection
- Input abstractions
- Utility systems

A complete application should be buildable using only:

```text
Infrastructure + Core
```

If a feature is not universally required by most games, it probably does not belong in Core.

---

### 🧰 Toolkits Are Optional

Toolkits extend the capabilities of the engine without increasing the complexity of Core.

Examples include:

- Artificial Intelligence
- Networking
- Serialization
- ECS
- Geometry generation
- Advanced physics systems

The absence of a toolkit must never prevent the engine from functioning.

Toolkits exist to provide advanced functionality while keeping Core small and focused.

---

### 🏛️ Framework Exists to Reduce Boilerplate

The Framework layer exists to simplify application development.

Typical framework responsibilities include:

- Scene management
- Asset management
- State management
- GUI integration
- Application lifecycle management

The Framework should improve productivity without becoming mandatory.

Developers should be free to work directly with Core when greater control or educational value is desired.

---

### 🎓 Educational Value Matters

JOT.NET is intended to be more than a game engine.

It is also intended to be:

- A learning platform
- A research platform
- A teaching tool
- A reference implementation of modular engine architecture

Whenever reasonable, the architecture should favor clarity and maintainability over unnecessary complexity.

The engine should remain approachable to students, hobbyists, researchers, and professionals alike.

---

### 🚀 Modern Technologies, Stable Principles

Technologies will evolve over time.

Libraries may be replaced.

Platforms may change.

The architectural principles should remain stable.

Current technologies include:

- Silk.NET
- AssimpNet
- System.Numerics
- DotNetty
- Blazor WebAssembly
- MudBlazor (planned)
- Protocol Buffers

Future technologies may replace some of these choices, but the core principles of modularity, replaceability, educational value, and clean dependency flow should remain unchanged.

#-table-of-contents

---

## 🌐 MMO-First Philosophy

Networking is not an afterthought in JOT.NET.

Unlike many game engines that begin as single-player frameworks and later add networking capabilities, JOT.NET is designed with multiplayer and MMO architectures in mind from the start.

Key design considerations include:

- Client/server separation
- State synchronization
- Replication
- Serialization
- Scalability
- Distributed systems

This philosophy influences the design of the Core, Toolkits, and Framework layers even when building single-player applications.

### 🏗️ Designed for Growth

JOT.NET aims to support a wide range of applications, from local prototypes to persistent online worlds.

Long-term goals include:

- Dedicated game servers
- Browser-based clients
- Shared simulation systems
- MMO-oriented networking toolkits

### 🎓 Educational Value

MMO architectures combine several challenging areas of software engineering, including networking, serialization, synchronization, and distributed systems.

By exposing these concepts through a modular architecture, JOT.NET serves both as a game engine and as a platform for learning and experimentation.

### 🚀 Single-Player Friendly

Being MMO-first does not mean being MMO-only.

Single-player games remain first-class citizens and can be developed using exactly the same architecture and tools.

#-table-of-contents

---

## 🌍 Browser Client Strategy

JOT.NET aims to support both desktop and browser-based applications through a shared engine architecture.

The browser client is built using:

- Blazor WebAssembly
- MudBlazor (planned)
- WebSockets

The objective is to maximize code reuse between desktop and browser clients while remaining lightweight, accessible, and multiplayer-friendly.

Browser support is considered a strategic goal of the project and an important step toward making JOT.NET available across a wider range of platforms.

#-table-of-contents

---

## 🧩 Layer Responsibilities

JOT.NET is organized into four layers, each with a clearly defined responsibility.

```text
Framework
    ↓
Toolkits
    ↓
Core
    ↓
Infrastructure
```

### 🔌 Infrastructure

Provides adapters for external technologies and platform-specific functionality.

Examples:

- Windowing and input
- Rendering backends
- Model importing
- Networking integrations

Projects:

- JOT.Infrastructure.Silk
- JOT.Infrastructure.Assimp
- JOT.Infrastructure.Numerics
- JOT.Infrastructure.DotNetty

### ⚙️ Core

Provides the minimum functionality required by all applications built with JOT.NET.

Examples:

- Mathematics
- Resource abstractions
- Rendering abstractions
- Basic collision detection
- Common utilities

Projects:

- JOT.Core.Math
- JOT.Core.IO
- JOT.Core.Util
- JOT.Core.Physics
- JOT.Core.Rendering

### 🧰 Toolkits

Provide optional, advanced functionality that extends the Core layer.

Examples:

- Artificial Intelligence
- Networking
- Serialization
- ECS
- Geometry generation
- Advanced physics

Projects:

- JOT.Toolkit.AI
- JOT.Toolkit.ECS
- JOT.Toolkit.Geometry
- JOT.Toolkit.Networking
- JOT.Toolkit.Physics
- JOT.Toolkit.Serialization

### 🏛️ Framework

Provides high-level systems intended to reduce development boilerplate.

Examples:

- Scene management
- Asset management
- State management
- GUI integration

Projects:

- JOT.Framework.Application
- JOT.Framework.GUI
- JOT.Framework.Managers

The Framework is optional. Applications can be built using only the Infrastructure and Core layers or can leverage Toolkits and Framework for increased productivity.

#-table-of-contents

---

## ⚙️ Installation

### Prerequisites

JOT.NET targets the modern .NET ecosystem and supports Windows, Linux, and macOS.

Required software:

- .NET SDK 9.0 or newer
- Git
- A modern IDE:
  - Visual Studio 2022
  - Visual Studio Code
  - JetBrains Rider

The desktop client additionally requires graphics hardware capable of running Silk.NET-based applications.

### Clone the Repository

```bash
git clone https://github.com/g-amador/JOT.NET.git
cd JOT.NET
```

### Restore Dependencies

```bash
dotnet restore
```

### Build the Solution

```bash
dotnet build
```

### Verify the Installation

Run one of the template projects:

```bash
dotnet run --project src/demos/templates/CoreTemplate
```

If the template builds and runs successfully, your environment is correctly configured.

#-table-of-contents

---

## 🚀 Usage

Run:

dotnet run --project src/demos/templates/CoreTemplate

or

dotnet run --project src/demos/templates/FullTemplate
`

#-table-of-contents

---

## 🧪 Templates and Examples

JOT.NET provides two official templates that demonstrate different levels of engine usage.

### ⚙️ Core Template

Uses only:

```text
Infrastructure + Core
```

This template demonstrates the minimum functionality required to build applications with JOT.NET.

### 🏛️ Full Template

Uses:

```text
Infrastructure + Core + Toolkits + Framework
```

This template demonstrates how the higher layers reduce development effort and application boilerplate.

### 🚀 Examples

Additional examples will showcase individual engine features, such as:

- Rendering
- Physics
- AI
- Networking
- MMO-oriented systems

#-table-of-contents

---

## 🤝 Contributing

Contributions are welcome, including:

- Bug reports
- Feature requests
- Documentation improvements
- Examples and demos
- Engine components and toolkits

When contributing, please respect the layered architecture and keep Core minimal.

A dedicated `CONTRIBUTING.md` guide may be provided as the project matures.

#-table-of-contents

---

## 📄 License

JOT.NET is released under the **MIT License**.

See the `LICENSE` file for details.

#-table-of-contents

---

## 📬 Contact & Usage Notice

JOT.NET is released under the MIT License.

If you have questions, suggestions, feedback, or would like to share a project built with JOT.NET, feel free to get in touch:

📧 **g.n.p.amador@gmail.com**

If you extend JOT.NET, create templates, demos, games, or ports based on it, I would be delighted to hear about it.

#-table-of-contents

---