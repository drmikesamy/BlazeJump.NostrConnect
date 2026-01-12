using Hl7.Fhir.Model;
using System.Linq.Expressions;

namespace NostrConnect.Maui.Services.Fhir;

/// <summary>
/// Generic service for CRUD operations on FHIR resources stored in LocalResources table.
/// </summary>
/// <typeparam name="TResource">The FHIR resource type (e.g., Appointment, Observation, Medication)</typeparam>
public interface IFhirResourceService<TResource> where TResource : DomainResource
{
    /// <summary>
    /// Gets all resources of the specified FHIR type that are not deleted.
    /// </summary>
    /// <param name="publicKey">Optional public key to filter resources by user</param>
    /// <returns>List of FHIR resources</returns>
    Task<List<TResource>> GetAllAsync(string? publicKey = null);

    /// <summary>
    /// Gets a specific resource by its LocalResource ID.
    /// </summary>
    /// <param name="id">The LocalResource GUID</param>
    /// <returns>The FHIR resource or null if not found</returns>
    Task<TResource?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets a specific resource by its FHIR resource ID.
    /// </summary>
    /// <param name="fhirId">The FHIR resource ID</param>
    /// <returns>The FHIR resource or null if not found</returns>
    Task<TResource?> GetByFhirIdAsync(string fhirId);

    /// <summary>
    /// Saves a new FHIR resource to the database.
    /// </summary>
    /// <param name="resource">The FHIR resource to save</param>
    /// <param name="syncToNostr">Whether to sync the resource to Nostr</param>
    /// <returns>The saved resource with its LocalResource ID in Meta.VersionId</returns>
    Task<(TResource Resource, Guid LocalResourceId)> SaveAsync(TResource resource, bool syncToNostr = true);

    /// <summary>
    /// Updates an existing FHIR resource.
    /// </summary>
    /// <param name="localResourceId">The LocalResource GUID</param>
    /// <param name="resource">The updated FHIR resource</param>
    /// <param name="syncToNostr">Whether to sync the update to Nostr</param>
    /// <returns>The updated resource</returns>
    Task<TResource> UpdateAsync(Guid localResourceId, TResource resource, bool syncToNostr = true);

    /// <summary>
    /// Soft deletes a resource by its LocalResource ID.
    /// </summary>
    /// <param name="id">The LocalResource GUID</param>
    /// <param name="syncToNostr">Whether to sync the deletion to Nostr</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeleteAsync(Guid id, bool syncToNostr = true);

    /// <summary>
    /// Queries resources using a custom predicate on the LocalResource table.
    /// </summary>
    /// <param name="predicate">LINQ expression to filter LocalResources</param>
    /// <returns>List of matching FHIR resources with their LocalResource IDs</returns>
    Task<List<(TResource Resource, Guid LocalResourceId)>> QueryAsync(
        Expression<Func<Models.LocalResource, bool>> predicate);

    /// <summary>
    /// Gets the LocalResource ID for a FHIR resource (stored in Meta.VersionId).
    /// </summary>
    Guid? GetLocalResourceId(TResource resource);

    /// <summary>
    /// Sets the LocalResource ID on a FHIR resource (stored in Meta.VersionId).
    /// </summary>
    void SetLocalResourceId(TResource resource, Guid localResourceId);
}
