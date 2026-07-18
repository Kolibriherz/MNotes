using System.Net;
using System.Net.Http.Json;
using System.Text;
using MNoteProvider.Common;
using MNoteProvider.Common.Abstractions.Enums;

namespace MNoteProvider.ClientService.Tests;

[TestFixture]
public class MNoteClientServiceTests
{
    private const string BaseUrl = "https://example.org/api/";
    private const string MediaType = "application/json";


    [Test]
    public async Task GetAllNotes_WhenSuccessButNull_ReturnsEmptyResponseFail()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null", Encoding.UTF8, MediaType)
        };
        var httpClient = CreateHttpClient(response, out var _);
        var client = new MNoteClientService(httpClient);

        var result = await client.GetAllNotes(CancellationToken.None);

        Assert.That(result.TryPickT1(out var fail, out _), Is.True);
        Assert.That(fail.FailType, Is.EqualTo(MNotesFailType.PROBLEM));
        Assert.That(fail.Message, Is.EqualTo("Server returned an empty response while calling " +
            "note/getall" +
            " in GetAllNotes"));
    }

    [Test]
    public async Task GetAllFolders_WhenServerReturnsEmptyJsonArray_ReturnsEmptyArraySuccessfully()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("[]", Encoding.UTF8, MediaType)
        };
        var httpClient = CreateHttpClient(response, out var _);
        var client = new MNoteClientService(httpClient);

        // Act
        var result = await client.GetAllFolders(CancellationToken.None);

        // Assert
        Assert.That(result.TryPickT0(out var emptyArray, out _), Is.True);
        Assert.That(emptyArray, Is.Empty);
    }

    [Test]
    public async Task GetAllTags_WhenFailBodyMalformed_ReturnsGenericErrorWithJsonExceptionMessage()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("{invalid", Encoding.UTF8, MediaType)
        };
        var httpClient = CreateHttpClient(response, out var _);
        var client = new MNoteClientService(httpClient);

        // Act
        var result = await client.GetAllTags(CancellationToken.None);

        Assert.That(result.TryPickT1(out var fail, out _), Is.True);
        Assert.That(fail.FailType, Is.EqualTo(MNotesFailType.PROBLEM));
        Assert.That(fail.Message, Does.StartWith("Error while calling " + MNotesRoutes.Endpoints.TagEndpoints.GetAll + " in GetAllTags:"));
        Assert.That(fail.Message, Does.Contain("invalid"));
    }


    [Test]
    public async Task GetAllCommentsByNote_WhenServerReturnsErrorFailType_ReturnsErrorFromBody()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var serverFail = new MNoteProcessFail(MNotesFailType.BADREQUEST, "The specified note does not exist.");

        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(serverFail)
        };

        var httpClient = CreateHttpClient(response, out var _);
        var client = new MNoteClientService(httpClient);

        // Act
        var result = await client.GetAllCommentsByNote(noteId, CancellationToken.None);

        // Assert
        Assert.That(result.TryPickT1(out var fail, out _), Is.True);
        Assert.That(fail.FailType, Is.EqualTo(MNotesFailType.BADREQUEST));
        Assert.That(fail.Message, Is.EqualTo("The specified note does not exist."));
    }


    private class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? CapturedRequest { get; private set; }

        public FakeHttpMessageHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequest = request;
            return Task.FromResult(_response);
        }
    }

    private HttpClient CreateHttpClient(HttpResponseMessage response, out FakeHttpMessageHandler handler)
    {
        handler = new FakeHttpMessageHandler(response);
        return new HttpClient(handler) { BaseAddress = new Uri(BaseUrl) };
    }

}



