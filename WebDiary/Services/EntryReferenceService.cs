using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Core;
using WebDiary.Data;
using WebDiary.DTO;
using WebDiary.Entities;

namespace WebDiary.Services;

public class EntryReferenceService : IEntryReferenceService
{
    private readonly DiariesContext dbContext;
    private readonly Serilog.ILogger logger;

    public EntryReferenceService(DiariesContext dbContext)
    {
        this.dbContext = dbContext;
        logger = Log.ForContext<EntryReferenceService>();
    }

    public async Task<EntryReferenceDTO?> CreateReference(CreateEntryReferenceDTO dto, int userId)
    {
        try
        {
            var sourceEntry = await dbContext.diaries
                .Include(d => d.Group)
                .FirstOrDefaultAsync(d => d.Id == dto.SourceEntryId);

            if (sourceEntry == null)
                return null;

            // Check if user can edit the source entry (is owner or editor)
            if (sourceEntry.OwnerId != userId)
            {
                var canEdit = sourceEntry.Group != null && 
                    (sourceEntry.Group.UserId == userId || 
                    await dbContext.groupPermissions.AnyAsync(p => 
                        p.GroupId == sourceEntry.GroupId && 
                        p.UserId == userId && 
                        p.Role == GroupRole.Editor));

                if (!canEdit)
                    return null;
            }

            var referencedEntry = await dbContext.diaries.FindAsync(dto.ReferencedEntryId);
            if (referencedEntry == null)
                return null;

            // Entries should be in the same group or accessible group
            if (sourceEntry.GroupId != referencedEntry.GroupId)
                return null;

            var reference = new EntryReference
            {
                SourceEntryId = dto.SourceEntryId,
                ReferencedEntryId = dto.ReferencedEntryId,
                ExcerptText = dto.ExcerptText,
                ExcerptStartIndex = dto.ExcerptStartIndex,
                ExcerptEndIndex = dto.ExcerptEndIndex,
                CreatedAtUtc = DateTime.UtcNow
            };

            dbContext.entryReferences.Add(reference);
            await dbContext.SaveChangesAsync();

            return new EntryReferenceDTO
            {
                Id = reference.Id,
                SourceEntryId = reference.SourceEntryId,
                ReferencedEntryId = reference.ReferencedEntryId,
                ExcerptText = reference.ExcerptText,
                ExcerptStartIndex = reference.ExcerptStartIndex,
                ExcerptEndIndex = reference.ExcerptEndIndex,
                CreatedAtUtc = reference.CreatedAtUtc
            };
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating entry reference from {SourceId} to {ReferencedId}", 
                dto.SourceEntryId, dto.ReferencedEntryId);
            return null;
        }
    }

    public async Task<List<EntryReferenceDTO>> GetReferencesFromEntry(int entryId)
    {
        try
        {
            var references = await dbContext.entryReferences
                .Where(r => r.SourceEntryId == entryId)
                .Select(r => new EntryReferenceDTO
                {
                    Id = r.Id,
                    SourceEntryId = r.SourceEntryId,
                    ReferencedEntryId = r.ReferencedEntryId,
                    ExcerptText = r.ExcerptText,
                    ExcerptStartIndex = r.ExcerptStartIndex,
                    ExcerptEndIndex = r.ExcerptEndIndex,
                    CreatedAtUtc = r.CreatedAtUtc
                })
                .ToListAsync();

            return references;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting references from entry {EntryId}", entryId);
            return new List<EntryReferenceDTO>();
        }
    }

    public async Task<List<EntryReferenceDTO>> GetReferencesTo(int entryId)
    {
        try
        {
            var references = await dbContext.entryReferences
                .Where(r => r.ReferencedEntryId == entryId)
                .Select(r => new EntryReferenceDTO
                {
                    Id = r.Id,
                    SourceEntryId = r.SourceEntryId,
                    ReferencedEntryId = r.ReferencedEntryId,
                    ExcerptText = r.ExcerptText,
                    ExcerptStartIndex = r.ExcerptStartIndex,
                    ExcerptEndIndex = r.ExcerptEndIndex,
                    CreatedAtUtc = r.CreatedAtUtc
                })
                .ToListAsync();

            return references;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting references to entry {EntryId}", entryId);
            return new List<EntryReferenceDTO>();
        }
    }

    public async Task<bool> DeleteReference(int referenceId, int userId)
    {
        try
        {
            var reference = await dbContext.entryReferences
                .Include(r => r.SourceEntry)
                .FirstOrDefaultAsync(r => r.Id == referenceId);

            if (reference == null)
                return false;

            // Only the owner of the source entry or group owner can delete
            if (reference.SourceEntry?.OwnerId != userId && 
                reference.SourceEntry?.Group?.UserId != userId)
            {
                return false;
            }

            dbContext.entryReferences.Remove(reference);
            await dbContext.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting entry reference {ReferenceId}", referenceId);
            return false;
        }
    }
}
