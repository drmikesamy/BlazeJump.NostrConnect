using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.EntityFrameworkCore;
using NostrConnect.Maui.Data;
using NostrConnect.Maui.Models;
using NostrConnect.Maui.Services.Identity;
using BlazeJump.Tools.Services.Crypto;
using BlazeJump.Tools.Services.Message;
using BlazeJump.Tools.Enums;
using System.Linq.Expressions;
using LinqExpression = System.Linq.Expressions.Expression;

namespace NostrConnect.Maui.Services.Fhir;

/// <summary>
/// Generic service for CRUD operations on FHIR resources.
/// </summary>
/// <typeparam name="TResource">The FHIR resource type (e.g., Appointment, Observation, Medication)</typeparam>
public class FhirResourceService<TResource> : IFhirResourceService<TResource> where TResource : DomainResource
{
    private readonly IDbContextFactory<NostrDbContext> _contextFactory;
    private readonly INativeIdentityService _identityService;
    private readonly ICryptoService _cryptoService;
    private readonly IMessageService _messageService;
    private readonly FhirJsonSerializer _serializer;
    private readonly FhirJsonParser _parser;
    private readonly string _resourceType;

    public FhirResourceService(
        IDbContextFactory<NostrDbContext> contextFactory,
        INativeIdentityService identityService,
        ICryptoService cryptoService,
        IMessageService messageService)
    {
        _contextFactory = contextFactory;
        _identityService = identityService;
        _cryptoService = cryptoService;
        _messageService = messageService;
        _serializer = new FhirJsonSerializer();
        _parser = new FhirJsonParser();
        
        // Get resource type name at construction time
        _resourceType = GetResourceTypeName();
    }

    public async Task<List<TResource>> GetAllAsync(string? publicKey = null)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var records = await context.LocalResources
            .Where(r => r.FhirType == _resourceType && !r.IsDeleted)
            .OrderByDescending(r => r.LastUpdated)
            .ToListAsync();

        var resources = new List<TResource>();
        foreach (var record in records)
        {
            var resource = ParseResource(record);
            if (resource != null)
            {
                resources.Add(resource);
            }
        }

        return resources;
    }

    public async Task<TResource?> GetByIdAsync(Guid id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var record = await context.LocalResources
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (record == null)
            return null;

        return ParseResource(record);
    }

    public async Task<TResource?> GetByFhirIdAsync(string fhirId)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var records = await context.LocalResources
            .Where(r => r.FhirType == _resourceType && !r.IsDeleted)
            .ToListAsync();

        foreach (var record in records)
        {
            var resource = ParseResource(record);
            if (resource != null && resource.Id == fhirId)
                return resource;
        }

        return null;
    }

    public async Task<(TResource Resource, Guid LocalResourceId)> SaveAsync(
        TResource resource, bool syncToNostr = true)
    {
        var publicKey = _identityService.ActiveUserProfile?.PublicKey;
        if (string.IsNullOrEmpty(publicKey))
            throw new InvalidOperationException("No active user profile");

        // Ensure the resource has an ID
        if (string.IsNullOrEmpty(resource.Id))
            resource.Id = Guid.NewGuid().ToString();

        // Update meta information
        resource.Meta ??= new Meta();
        resource.Meta.LastUpdated = DateTimeOffset.UtcNow;

        var fhirJson = _serializer.SerializeToString(resource);

        using var context = await _contextFactory.CreateDbContextAsync();
        var localResource = new LocalResource
        {
            FhirType = _resourceType,
            Content = fhirJson,
            LastUpdated = DateTime.UtcNow
        };

        context.LocalResources.Add(localResource);
        await context.SaveChangesAsync();

        // Store LocalResource ID in the resource's Meta.VersionId for future reference
        SetLocalResourceId(resource, localResource.Id);

        // Sync to Nostr if requested
        if (syncToNostr)
        {
            var nostrEventId = await SyncToNostrAsync(fhirJson, publicKey);
            localResource.NostrEventId = nostrEventId;
            await context.SaveChangesAsync();
        }

        return (resource, localResource.Id);
    }

    public async Task<TResource> UpdateAsync(
        Guid localResourceId, TResource resource, bool syncToNostr = true)
    {
        var publicKey = _identityService.ActiveUserProfile?.PublicKey;
        if (string.IsNullOrEmpty(publicKey))
            throw new InvalidOperationException("No active user profile");

        using var context = await _contextFactory.CreateDbContextAsync();
        var localResource = await context.LocalResources.FindAsync(localResourceId);
        
        if (localResource == null)
            throw new KeyNotFoundException($"LocalResource with ID {localResourceId} not found");

        // Update meta information
        resource.Meta ??= new Meta();
        resource.Meta.LastUpdated = DateTimeOffset.UtcNow;

        var fhirJson = _serializer.SerializeToString(resource);
        
        localResource.Content = fhirJson;
        localResource.LastUpdated = DateTime.UtcNow;
        
        await context.SaveChangesAsync();

        // Sync to Nostr if requested
        if (syncToNostr)
        {
            var nostrEventId = await SyncToNostrAsync(fhirJson, publicKey);
            localResource.NostrEventId = nostrEventId;
            await context.SaveChangesAsync();
        }

        return resource;
    }

    public async Task<bool> DeleteAsync(Guid id, bool syncToNostr = true)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var localResource = await context.LocalResources.FindAsync(id);
        
        if (localResource == null)
            return false;

        localResource.IsDeleted = true;
        localResource.LastUpdated = DateTime.UtcNow;
        await context.SaveChangesAsync();

        // TODO: Sync deletion to Nostr if needed
        
        return true;
    }

    public async Task<List<(TResource Resource, Guid LocalResourceId)>> QueryAsync(
        Expression<Func<LocalResource, bool>> predicate)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        // Combine the user's predicate with our base filters
        var basePredicate = (Expression<Func<LocalResource, bool>>)(r => 
            r.FhirType == _resourceType && !r.IsDeleted);
        
        var combinedPredicate = CombinePredicates(basePredicate, predicate);
        
        var records = await context.LocalResources
            .Where(combinedPredicate)
            .ToListAsync();

        var results = new List<(TResource Resource, Guid LocalResourceId)>();
        foreach (var record in records)
        {
            var resource = ParseResource(record);
            if (resource != null)
            {
                results.Add((resource, record.Id));
            }
        }

        return results;
    }

    public Guid? GetLocalResourceId(TResource resource)
    {
        if (resource.Meta?.VersionId != null && Guid.TryParse(resource.Meta.VersionId, out var guid))
            return guid;
        
        return null;
    }

    public void SetLocalResourceId(TResource resource, Guid localResourceId)
    {
        resource.Meta ??= new Meta();
        resource.Meta.VersionId = localResourceId.ToString();
    }

    #region Private Helpers

    private string GetResourceTypeName()
    {
        // Use reflection to get the ResourceType from the static property
        var type = typeof(TResource);
        var resourceTypeProperty = type.GetProperty("ResourceType", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        
        if (resourceTypeProperty != null)
        {
            var value = resourceTypeProperty.GetValue(null);
            return value?.ToString() ?? type.Name;
        }

        // Fallback: use class name
        return type.Name;
    }

    private TResource? ParseResource(LocalResource localResource)
    {
        try
        {
            var resource = _parser.Parse<TResource>(localResource.Content);
            
            // Store the LocalResource ID for future operations
            SetLocalResourceId(resource, localResource.Id);
            
            return resource;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing FHIR resource: {ex.Message}");
            return null;
        }
    }

    private async Task<string> SyncToNostrAsync(string fhirJson, string publicKey)
    {
        try
        {
            var encryptedData = await _cryptoService.Nip44Encrypt(fhirJson, publicKey, publicKey);

            var nEvent = _messageService.CreateNEvent(
                publicKey,
                KindEnum.EncryptedDirectMessages,
                encryptedData,
                null,
                null,
                new List<string> { publicKey }
            );

            await _messageService.Send(KindEnum.EncryptedDirectMessages, nEvent, null);
            
            return nEvent.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error syncing to Nostr: {ex.Message}");
            return string.Empty;
        }
    }

    private Expression<Func<T, bool>> CombinePredicates<T>(
        Expression<Func<T, bool>> first,
        Expression<Func<T, bool>> second)
    {
        var parameter = LinqExpression.Parameter(typeof(T));

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return LinqExpression.Lambda<Func<T, bool>>(
            LinqExpression.AndAlso(left!, right!), parameter);
    }

    private class ReplaceExpressionVisitor : ExpressionVisitor
    {
        private readonly LinqExpression _oldValue;
        private readonly LinqExpression _newValue;

        public ReplaceExpressionVisitor(LinqExpression oldValue, LinqExpression newValue)
        {
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public override LinqExpression? Visit(LinqExpression? node)
        {
            return node == _oldValue ? _newValue : base.Visit(node);
        }
    }

    #endregion
}
