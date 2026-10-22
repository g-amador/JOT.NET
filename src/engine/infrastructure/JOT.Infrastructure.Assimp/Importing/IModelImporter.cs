namespace JOT.Infrastructure.Assimp.Importing;

/// <summary>
/// Imports model files into engine-ready geometry and materials.
///
/// This is the asset glue contract for third-party model loading
/// libraries; it deals in plain data types only.
/// </summary>
public interface IModelImporter : IDisposable
{
    /// <summary>
    /// Imports the model at the specified path.
    /// </summary>
    /// <param name="filePath">Path of the model file.</param>
    /// <returns>The imported model.</returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the model file does not exist.
    /// </exception>
    ImportedModel Import(string filePath);
}
