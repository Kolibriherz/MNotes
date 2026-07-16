namespace MNoteProvider.Common;
/// <summary>
/// Provides shared route constants for MNoteProvider HTTP endpoints and SignalR hubs.
/// </summary>
public static class MNotesRoutes
{
    private const string Note = "note";
    private const string Folder = "folder";
    private const string Comment = "comment";
    private const string Tag = "tag";
    private const string NoteTagAssignment = "notetagassignment";

    /// <summary>
    /// HTTP endpoint routes.
    /// </summary>
    public static class Endpoints
    {
        /// <summary>Route of the availability probe.</summary>
        public const string IsAvailable = "isavailable";

        /// <summary>Routes of the note endpoints.</summary>
        public static class NoteEndpoints
        {
            /// <summary>Route for getting all notes.</summary>
            public const string GetAll = $"{Note}/getall";
            /// <summary>Route for creating a note.</summary>
            public const string Create = $"{Note}/create";
            /// <summary>Route for updating a note.</summary>
            public const string Update = $"{Note}/update";
            /// <summary>Route for deleting a note.</summary>
            public const string Delete = $"{Note}/delete";
            /// <summary>Route for loading the previous version of a note.</summary>
            public const string LoadPreviousVersion = $"{Note}/loadpreviousversion";
            /// <summary>Route for getting the full update history of a note.</summary>
            public const string GetHistory = $"{Note}/gethistory";
        }

        /// <summary>Routes of the folder endpoints.</summary>
        public static class FolderEndpoints
        {
            /// <summary>Route for getting all folders.</summary>
            public const string GetAll = $"{Folder}/getall";
            /// <summary>Route for creating a folder.</summary>
            public const string Create = $"{Folder}/create";
            /// <summary>Route for updating a folder.</summary>
            public const string Update = $"{Folder}/update";
            /// <summary>Route for deleting a folder.</summary>
            public const string Delete = $"{Folder}/delete";
        }
        /// <summary>Routes of the comment endpoints.</summary>
        public static class CommentEndpoints
        {
            /// <summary>Route for getting all comments of a note.</summary>
            public const string GetAllByNote = $"{Comment}/getallbynote";
            /// <summary>Route for creating a comment.</summary>
            public const string Create = $"{Comment}/create";
            /// <summary>Route for updating a comment.</summary>
            public const string Update = $"{Comment}/update";
            /// <summary>Route for deleting a comment.</summary>
            public const string Delete = $"{Comment}/delete";
        }
        /// <summary>Routes of the tag endpoints.</summary>
        public static class TagEndpoints
        {
            /// <summary>Route for getting all tags.</summary>
            public const string GetAll = $"{Tag}/getall";
            /// <summary>Route for creating a tag.</summary>
            public const string Create = $"{Tag}/create";
            /// <summary>Route for deleting a tag.</summary>
            public const string Delete = $"{Tag}/delete";
        }
        /// <summary>Routes of the note-tag assignment endpoints.</summary>
        public static class NoteTagAssignmentEndpoints
        {
            /// <summary>Route for getting all note-tag assignments.</summary>
            public const string GetAll = $"{NoteTagAssignment}/getall";
            /// <summary>Route for assigning a tag to a note.</summary>
            public const string Assign = $"{NoteTagAssignment}/assign";
            /// <summary>Route for removing a tag assignment from a note.</summary>
            public const string Unassign = $"{NoteTagAssignment}/unassign";
        }
    }

    /// <summary>
    /// SignalR hubs
    /// </summary>
    public static class Hubs
    {
        /// <summary>Base route of the SignalR hubs.</summary>
        public const string Name = "hubs";
        /// <summary>Names of the hub methods pushed to connected clients.</summary>
        public static class MethodNames
        {
            /// <summary>Hub method invoked when a note was created.</summary>
            public const string NoteCreated = "NoteCreated";
            /// <summary>Hub method invoked when a note was updated.</summary>
            public const string NoteUpdated = "NoteUpdated";
            /// <summary>Hub method invoked when a note was deleted.</summary>
            public const string NoteDeleted = "NoteDeleted";
        }
    }

}

