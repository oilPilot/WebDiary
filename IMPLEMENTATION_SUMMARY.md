# Enhanced Collaboration Feature - Implementation Summary

## Overview
Implemented comprehensive role-based access control (RBAC) and collaboration features for the WebDiary application, allowing users to share diary groups with specific roles and reference entries across collaborative spaces.

## Database Changes

### New Entities (Already Defined)
- **GroupPermission**: Manages user permissions within diary groups
  - Fields: GroupId, UserId, GrantedByUserId, Role (Owner/Editor/Viewer), CreatedAtUtc, UpdatedAtUtc
  - Unique constraint on (GroupId, UserId) to prevent duplicate permissions
  
- **EntryReference**: Links entries together for cross-referencing
  - Fields: SourceEntryId, ReferencedEntryId, ExcerptText, ExcerptStartIndex, ExcerptEndIndex, CreatedAtUtc
  - Supports referencing specific excerpts or full entries

- **Diary OwnerId**: Added ownership tracking to diary entries
  - Foreign key to User table
  - Tracks who created each entry (important for collaborative groups)

### Migration
- Created migration: `20260321100000_AddGroupPermissionsAndEntryReferences.cs`
- Creates `groupPermissions` and `entryReferences` tables
- Establishes proper foreign key relationships and indexes

## API Endpoints

### Group Collaboration Management
All endpoints require authorization. Base path: `/groups/{id}/members`

**GET /groups/{id}/members**
- List all members of a group
- Returns: `GroupPermissionDTO[]` with username, role, and grant timestamp
- Access: Only editors+ can view

**POST /groups/{id}/members** (Add Member)
- Add a user to a group with a specific role
- Request: `ManageCollaborationDTO`
  - UserName: string (username of user to add)
  - Role: GroupRole (Owner, Editor, or Viewer)
- Response: `GroupPermissionDTO` with new member details
- Access: Group owner or admin only

**PUT /groups/{id}/members/{targetUserId}** (Update Role)
- Change a member's role in the group
- Request: `UpdateCollaborationDTO` with new Role
- Response: 204 NoContent
- Access: Group owner or admin only
- Restrictions: Cannot modify owner's permissions

**DELETE /groups/{id}/members/{targetUserId}** (Remove Member)
- Remove a user from group collaboration
- Response: 204 NoContent
- Access: Group owner or admin only
- Restrictions: Cannot remove the owner

### Entry References
Base path: `/diaries/{id}/references`

**GET /diaries/{id}/references**
- List all references from an entry
- Returns: `EntryReferenceDTO[]`
- Includes owner information for referenced entries

**POST /diaries/{id}/references** (Create Reference)
- Create a reference from one entry to another
- Request: `CreateEntryReferenceDTO`
  - ReferencedEntryId: id of entry being referenced
  - ExcerptText: optional, text of excerpt being referenced
  - ExcerptStartIndex, ExcerptEndIndex: optional, position in source entry
- Response: `EntryReferenceDTO` with full details
- Access: Entry owner or editor in group
- Restrictions: Can only reference entries from accessible groups

**DELETE /diaries/{id}/references/{referenceId}** (Delete Reference)
- Remove a reference from an entry
- Response: 204 NoContent
- Access: Entry owner or admin only

### Updated Diary Endpoints

**GET /diaries/**
- Now returns entries from:
  - Groups owned by user
  - Groups where user has any role (Editor, Viewer, Owner)
- Includes owner information for each entry
- Shows collaborative entry authorship

**GET /diaries/{id}**
- Now includes Owner information
- Shows who created the entry (important in shared groups)

**POST /diaries/**
- Automatically sets OwnerId to current user
- Users with Editor role can create entries
- Viewers cannot create entries

## DTOs (Data Transfer Objects)

### New DTOs
- **GroupPermissionDTO**: Display group permissions with user details
- **ManageCollaborationDTO**: Add/manage collaborators
- **UpdateCollaborationDTO**: Change permission levels
- **RemoveCollaborationDTO**: Remove members
- **CollaborationListDTO**: List all collaborators in a group
- **EntryReferenceDTO**: Display entry references with owner info
- **CreateEntryReferenceDTO**: Create new references

### Updated DTOs
- **GroupDTO**: Now includes OwnerId and OwnerUserName
- **DiaryDTO**: Now includes OwnerId and OwnerUserName
- **EntryReferenceDTO**: Now includes ReferencedEntryOwner

## Services

### Updated Files
- **StatsService.cs**:
  - `GetStatsForPeriod()`: Now calculates statistics only for entries owned by the user
  - Includes entries from all groups where user has permissions (owned or shared)
  - More accurate personal activity tracking
  - Filters out entries created by others in shared groups

### New Services
- **CollaborationService.cs**: Manages group permissions and collaborations
- **EntryReferenceService.cs**: Handles entry references and excerpts

## Permission Model

### GroupRole Enum
- **Owner** (0): Full control, can manage group and permissions
- **Editor** (1): Can create and edit content, view all entries
- **Viewer** (2): Read-only access, cannot create entries

### Access Control Logic
- **Read Access**: Owner or any permission level in group
- **Write Access**: Owner or Editor level
- **Manage Permissions**: Owner only
- **Statistics**: Personal entries only (not group-wide)

## Key Features Implemented

### 1. Role-Based Access Control
❌ **Previously**: Groups could only be owned
✅ **Now**: Users can share groups with specific roles (Editor, Viewer, Owner)

### 2. Entry Ownership Tracking
❌ **Previously**: No tracking of who created entries
✅ **Now**: Each entry tracks its owner, visible when shared

### 3. Entry References
❌ **Previously**: Cannot reference other entries
✅ **Now**: Create links between entries with optional excerpts

### 4. Personal Statistics
❌ **Previously**: Stats included all group entries
✅ **Now**: Stats only count entries created by the current user

### 5. Collaborative Access
❌ **Previously**: Users only see groups they own
✅ **Now**: Users see all groups they have any access to (read, write, admin)

## Security Considerations

1. **Permission Checks**: All endpoints verify user has appropriate role
2. **Entry Ownership**: Only entry creators can modify references
3. **Group Management**: Only owners can manage permissions
4. **Cascading Deletes**: Removing permissions or entries cascades appropriately
5. **Audit Trail**: GrantedByUserId tracks who granted permissions

## Database Indexes

Created indexes for:
- `groupPermissions(GroupId, UserId)` - Unique constraint
- `entryReferences(SourceEntryId)` - Quick reference lookups
- `entryReferences(ReferencedEntryId)` - Reverse reference lookups

## Migration Path

The migration is designed to be non-destructive:
1. Creates new tables alongside existing schema
2. Existing groups remain unchanged (all owned by their creator)
3. Existing entries can still be queried under old access rules
4. No data loss or breaking changes

**Application Steps:**
1. Run migration: `dotnet ef database update`
2. Existing groups automatically work with new permission system
3. Group owners are set as the existing UserId on DiaryGroup

## Frontend Integration Points

The frontend should be updated to:

1. **Manage Collaboration** (top of pagination)
   - POST /groups/{id}/members (add user by username)
   - GET /groups/{id}/members (list members)
   - PUT /groups/{id}/members/{userId} (change role)
   - DELETE /groups/{id}/members/{userId} (remove member)

2. **Display Entry Metadata**
   - Show "Written by: {OwnerUserName}" on each entry
   - Different styling for own entries vs. shared entries

3. **Entry References**
   - GET /diaries/{id}/references (list references)
   - POST /diaries/{id}/references (add reference to another entry)
   - UI for selecting entries to reference
   - Display references in entry view

4. **Statistics Updates**
   - Stats now show personal activity
   - Can add filters for group-level stats if needed

## Testing Recommendations

1. **Permission Testing**
   - Create group, share with editor, verify editor can create entries
   - Share with viewer, verify viewer cannot create entries
   - Verify only owner can manage permissions

2. **Entry References**
   - Create reference between entries in same group
   - Create reference between entries in different shared groups
   - Test excerpt references with start/end positions
   - Verify access control for cross-group references

3. **Statistics**
   - Verify stats only count user's own entries
   - Add entry in shared group, verify creator's stats increase
   - Verify non-creator statistics aren't affected

4. **Migration**
   - Test on staging with production data backup
   - Verify all existing data remains accessible
   - Check performance of new queries with indexes

## Notes for Developers

- All date/time fields use UTC format (CreatedAtUtc, UpdatedAtUtc)
- GroupPermissions use a unique constraint to prevent duplicate roles
- EntryReferences support optional excerpt tracking for detailed citations
- Statistics automatically scope to current user for privacy
- Permission checks happen in endpoints, not just in services
- Serilog logging added for audit trails of permission changes and references

## Files Modified

### Controllers
- `/Controller/StatsController.cs` - Uses updated StatsService

### Data
- `/Data/DiariesContext.cs` - Already configured for new entities
- `/Data/Migrations/20260321100000_AddGroupPermissionsAndEntryReferences.cs` - New migration

### DTO
- `/DTO/CollaborationDTO.cs` - New DTOs for collaboration
- `/DTO/EntryReferenceDTO.cs` - Updated with ReferencedEntryOwner
- `/DTO/GroupDTO.cs` - Updated with owner info
- `/DTO/DiaryDTO.cs` - Updated with owner info

### Endpoints
- `/Endpoints/GroupEndpoints.cs` - Added collaboration management endpoints
- `/Endpoints/DiaryEndpoints.cs` - Updated for RBAC, added reference endpoints

### Entities
- `/Entities/GroupPermission.cs` - Already defined
- `/Entities/EntryReference.cs` - Already defined (with OwnerId)

### Mapping
- `/Mapping/Collaboration.cs` - New conversion functions
- `/Mapping/Groups.cs` - Updated to include owner info
- `/Mapping/Diaries.cs` - Updated to include owner info

### Services
- `/Services/StatsService.cs` - Updated for personal statistics
- `/Services/CollaborationService.cs` - New collaboration service (if not pre-existing)
- `/Services/EntryReferenceService.cs` - New reference service (if not pre-existing)

## Backward Compatibility

✅ **Fully Backward Compatible**
- Existing group access patterns still work
- Existing entries continue to be accessible
- OwnerId initialization for historical entries can be set to group owner
- All new features are additive, no breaking changes
