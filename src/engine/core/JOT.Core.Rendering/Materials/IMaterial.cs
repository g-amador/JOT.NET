using System.Numerics;

namespace JOT.Core.Rendering.Materials;

/// <summary>
/// Represents the visual appearance of a mesh.
/// </summary>
public interface IMaterial
{
    /// <summary>
    /// Gets the material name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets the base color.
    /// </summary>
    Vector4 BaseColor { get; set; }
}