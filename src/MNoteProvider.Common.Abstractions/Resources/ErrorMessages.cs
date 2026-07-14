namespace MNoteProvider.Common.Abstractions.Resources;

/// <summary>
/// Provides the centralised error message texts used by providers and repositories,
/// so that wording stays consistent across all layers.
/// </summary>
public static class ErrorMessages
{
    /// <summary>Message used when an attempt is made to move the root folder.</summary>
    public const string RootFolderNotMovable = "The root folder cannot be moved.";

    /// <summary>Message used when an attempt is made to delete the root folder.</summary>
    public const string RootFolderNotDeletable = "The root folder cannot be deleted.";

    /// <summary>Builds the message for a folder move that would create a cycle in the folder hierarchy.</summary>
    /// <param name="folderId">The folder that was requested to be moved.</param>
    /// <param name="parentId">The proposed new parent folder.</param>
    public static string FolderMoveWouldCreateCycle(Guid folderId, Guid parentId) =>
        $"Moving folder [{folderId}] below folder [{parentId}] would create a cycle.";


    /// <summary>Builds the message for a failed database operation.</summary>
    /// <param name="action">The attempted action, e.g. <c>save</c> or <c>update</c>.</param>
    /// <param name="what">The affected entity, e.g. <c>note</c>.</param>
    public static string DatabaseFail(string action, string what) => $"Failed to {action } the {what} in the database.";

    /// <summary>Builds the message for a lookup of an id that does not exist.</summary>
    /// <param name="id">The identifier that was not found.</param>
    public static string EntryDoesNotExist(Guid id) => $"Entry with id [{id}] does not exist in the database.";

    /// <summary>Builds the message for an entity type that lacks its <c>[Table]</c> attribute.</summary>
    /// <param name="tableName">The name of the entity type missing the attribute.</param>
    public static string NoDataBaseTableAttribute(string tableName) => $"{tableName} needs a [Table]-attribute.";

    /// <summary>Builds the message for an entity whose name is empty.</summary>
    /// <param name="entity">The affected entity type.</param>
    public static string NameRequired(string entity) => $"The {entity} name must not be empty.";

    /// <summary>Builds the message for an entity whose name exceeds the allowed length.</summary>
    /// <param name="entity">The affected entity type.</param>
    /// <param name="maxLength">The maximum permitted name length.</param>
    public static string NameTooLong(string entity, int maxLength) => $"The {entity} name must not exceed {maxLength} characters.";


    /// <summary>Builds the message for a note-tag assignment that does not exist.</summary>
    /// <param name="noteId">The note side of the missing assignment.</param>
    /// <param name="tagId">The tag side of the missing assignment.</param>
    public static string AssignmentDoesNotExist(Guid noteId, Guid tagId)=> $"No assignment exists between note [{noteId}] and tag [{tagId}].";

    /// <summary>
    /// Returns an error indicating that an entry of the specified type already exists.
    /// </summary>
    /// <param name="entity">The name of the entity that already exists.</param>
    public static string EntryAlreadyExists(string entity) => $"The {entity} already exists.";


    /// <summary>Builds the message for an insert that conflicts with an already existing id.</summary>
    /// <param name="id">The identifier that already exists.</param>
    public static string EntryAlreadyExists(Guid id) => $"Entry with id {id} already exists.";

    /// <summary>
    /// Returns an error indicating that an entry references an entity that does not exist.
    /// </summary>
    /// <param name="entity">The affected entity type.</param>
    public static string InvalidReference(string entity) => $"The {entity} references an entry that does not exist.";


    /// <summary>
    /// Returns an error indicating that an entry violates a data constraint.
    /// </summary>
    /// <param name="entity">The affected entity type.</param>
    public static string InvalidData(string entity) => $"The {entity} contains invalid data.";



}
