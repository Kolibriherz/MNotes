namespace MNoteProvider.Common.Tests;

public class MNotesRoutesTest
{
    private const string BaseUrl = "https://example.org/api/";

    [TestCase(MNotesRoutes.Endpoints.NoteEndpoints.GetAll, "https://example.org/api/note/getall")]
    [TestCase(MNotesRoutes.Endpoints.FolderEndpoints.GetAll, "https://example.org/api/folder/getall")]
    [TestCase(MNotesRoutes.Endpoints.CommentEndpoints.GetAllByNote, "https://example.org/api/comment/getallbynote")]
    [TestCase(MNotesRoutes.Endpoints.TagEndpoints.GetAll, "https://example.org/api/tag/getall")]
    [TestCase(MNotesRoutes.Endpoints.NoteTagAssignmentEndpoints.GetAll, "https://example.org/api/notetagassignment/getall")]
    [TestCase(MNotesRoutes.Hubs.Name, "https://example.org/api/hubs")]
    public void RouteCombination_WithTrailingSlash_ProducesCorrectUrl( string route, string expected)
    {
        // Arrange
        var baseUri = new Uri(BaseUrl);

        // Act
        var result = new Uri(baseUri, route);

        // Assert
        Assert.That(result.ToString(), Is.EqualTo(expected));
    }
}

