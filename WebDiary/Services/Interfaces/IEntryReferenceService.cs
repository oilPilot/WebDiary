using WebDiary.DTO;

namespace WebDiary.Services;

public interface IEntryReferenceService
{
    // Create a reference from one entry to another
    Task<EntryReferenceDTO?> CreateReference(CreateEntryReferenceDTO dto, int userId);
    
    // Get all references from a specific entry
    Task<List<EntryReferenceDTO>> GetReferencesFromEntry(int entryId);
    
    // Get all references to a specific entry
    Task<List<EntryReferenceDTO>> GetReferencesTo(int entryId);
    
    // Delete a reference
    Task<bool> DeleteReference(int referenceId, int userId);
}
