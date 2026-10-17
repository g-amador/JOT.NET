# 🎮 JOT.NET

<p align="center">
  <strong>A Modular Multi-purpose Game Engine for .NET</strong>
</p>

<p align="center">
  Infrastructure • Core • Toolkits • Framework
</p>

---

JOT.NET is a modern C#/.NET reimplementation of the original Java-based JOT (Just One Thing) engine. 

Designed for experimentation, education, prototyping, and Massively Multiplayer Online Games (MMOGs), it utilizes a modular architecture where subsystems can be extended or replaced independently.

---

## 🚧 Project Status

JOT.NET is currently **experimental and not production ready**. Current priorities include:

- Establishing the engine architecture
- Implementing the Infrastructure layer
- Building the desktop client foundation
- Building the browser client foundation
- Defining the Core engine systems
- Preparing the networking architecture for MMO support

---

## 🎯 Design Goals

JOT.NET is designed around the following objectives:

✅ Modular architecture with replaceable subsystems
✅ MMO-first design and browser client support
✅ Educational value and clean dependency flow
✅ Long-term maintainability within the modern .NET ecosystem

---

## 📑 <a name="toc">Table of Contents</a>

1. [📜 Origins](#origins)
2. [🏗️ Architecture](#architecture)
3. [🌐 MMO & Browser Strategy](#mmo-and-browser-strategy)
4. [🧩 Layer Responsibilities](#layer-responsibilities)
5. [📂 Project Organization](#project-organization)
6. [🛠️ Technology Stack](#technology-stack)
7. [⚙️ Installation](#installation)
8. [🚀 Usage](#usage)
9. [🧪 Templates](#templates)
10. [🤝 Contributing](#contributing)
11. [📄 License](#license)
12. [📬 Contact](#contact)

---

## 📜 <a name="origins">Origins</a>

JOT.NET modernizes the original JOT vision—demonstrating that engines can be collections of isolated components rather than monolithic systems. While the conceptual architecture remains faithful to the original research, the technology stack has been updated for the .NET ecosystem.

GitHub Repository:  
Presentation: https://goo.gl/i9eJFy
Research Paper: https://goo.gl/sGBVQT

| Original JOT (Java) | JOT.NET (.NET) |
|---------------------|----------------|
| JOGL / JogAmp	| Silk.NET |
| Apache Commons Math | System.Numerics |
| xmlbeans 2.6.0 | AssimpNet |
| JGroups | DotNetty |
| NetBeans | VS / VS Code / Rider |
| Maven	| .NET SDK / NuGet / CPM |
| Java Desktop Client | Desktop + Browser Clients |

[Back to Table of Contents](#toc)

---

## 🏗️ <a name="architecture">Architecture</a>

JOT.NET employs a strict layered architecture where dependencies only flow downward:

`Framework` → `Toolkits` → `Core` → `Infrastructure`

### Core Principles

- Replaceability: Subsystems can be swapped without affecting the rest of the engine.
- No Circularity: Lower layers must never be aware of higher layers; cross-layer shortcuts are forbidden.
- Infrastructure as Adapter: This layer isolates external libraries (Windowing, Graphics, Networking) from engine logic.
- Minimal Core: Contains only essential functionality (Math, Resource abstractions, Rendering basics). A basic app can run on `Infrastructure + Core` alone.
- Optional Toolkits: Advanced features (AI, ECS, Physics) are provided as optional extensions.
- Productivity Framework: Reduces boilerplate (Scene/Asset management) without being mandatory.

[Back to Table of Contents](#toc)

---

## 🌐 <a name="mmo-and-browser-strategy">MMO & Browser Strategy </a>

MMO-First: Networking is a first-class concern, not an add-on. The engine is built for client/server separation, state synchronization, and scalability from the ground up.

Browser Support: To maximize accessibility and reuse, browser clients are implemented via Blazor WebAssembly, WebSockets, and MudBlazor.

[Back to Table of Contents](#toc)

---

## 🧩 <a name="layer-responsibilities">Layer Responsibilities</a>

| Layer	| Responsibility | Example Projects |
|-------|----------------|------------------|
| Infrastructure | External adapters & platform specifics | `JOT.Infrastructure.Silk`, `.Assimp`, `.DotNetty` |
| Core | Universal engine requirements | `JOT.Core.Math`, `.IO`, `.Rendering`, `.Physics` |
| Toolkits | Optional advanced functionality | `JOT.Toolkit.AI`, `.ECS`, `.Networking`, `.Serialization` |
| Framework	| High-level boilerplate reduction | `JOT.Framework.Application`, `.GUI`, `.Managers` |

[Back to Table of Contents](#toc)

---

## 📂 <a name="project-organization">Project Organization</a>

```text
src/
├── JOT.NET.slnx
├── assets
├── client-browser/
├── client-desktop/
├── demos/
├── engine/
│   ├── infrastructure/
│   ├── core/
│   ├── toolkits/
│   └── framework/
└── server/
```

[Back to Table of Contents](#toc)

---

## 🛠️ <a name="technology-stack">Technology Stack</a>

| Purpose | Technology |
|----------|------------|
| Desktop Client | Silk.NET |
| Browser Client | Blazor WebAssembly |
| Browser UI | MudBlazor (planned) |
| Mathematics | System.Numerics |
| Model Loading | AssimpNet |
| Networking | DotNetty |
| Serialization | Protocol Buffers |
| Package Management | Central Package Management (CPM) |

[Back to Table of Contents](#toc)

---

## ⚙️ <a name="installation">Installation</a>

### Prerequisites

- .NET SDK 10.0+
- Git
- IDE: Visual Studio 2026 or VS Code

### Setup 

```bash
git clone https://github.com/g-amador/JOT.NET.git
cd JOT.NET
dotnet restore
dotnet build
```

### Verification

```bash
dotnet run --project src/demos/templates/CoreTemplate
```

[Back to Table of Contents](#toc)

---

## 🚀 <a name="usage">Usage</a>

To launch a project:

```bash
dotnet run --project src/demos/templates/CoreTemplate
# PR
dotnet run --project src/demos/templates/FullTemplate
```

[Back to Table of Contents](#toc)

---

## 🧪 <a name="templates">Templates</a>

- Core Template: Demonstrates the minimum viable engine setup (`Infrastructure + Core`).
- Full Template: Demonstrates the complete stack (`Infrastructure + Core + Toolkits + Framework`).

[Back to Table of Contents](#toc)

---

## 🤝 <a name="contributing">Contributing</a>

Contributions are welcome, including:

- Bug reports
- Feature requests
- Documentation improvements
- Examples and demos
- Engine components and toolkits

When contributing, please respect the layered architecture and keep Core minimal.

[Back to Table of Contents](#toc)

---

## 📄 <a name="license">License</a>

JOT.NET is released under the **MIT License**.

See the `LICENSE` file for details.

[Back to Table of Contents](#toc)

---

## 📬 <a name="contact">Contact</a>

If you have questions, suggestions, feedback, or would like to share a project built with JOT.NET, feel free to get in touch:

📧 **g.n.p.amador@gmail.com**

If you extend JOT.NET, create templates, demos, games, or ports based on it, I would be delighted to hear about it.

[Back to Table of Contents](#toc)

---