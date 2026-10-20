using JOT.Core.Rendering.Cameras;

namespace JOT.Core.Rendering.Renderers;

/// <summary>
/// Defines a rendering backend.
/// </summary>
public interface IRenderer
{
    /// <summary>
    /// Initializes the renderer.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Begins rendering a frame.
    /// </summary>
    void BeginFrame();

    /// <summary>
    /// Renders the specified camera view.
    /// </summary>
    /// <param name="camera">
    /// Camera used for rendering.
    /// </param>
    void Render(ICamera camera);

    /// <summary>
    /// Ends rendering a frame.
    /// </summary>
    void EndFrame();

    /// <summary>
    /// Releases renderer resources.
    /// </summary>
    void Shutdown();
}