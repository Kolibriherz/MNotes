using Microsoft.AspNetCore.Http;
using MNoteProvider.Common;


namespace MNoteProvider.Tests;

[TestFixture]
public class ResultExtensionTests
{
   
    [Test]
    public void ToIResulut_Returns400_WhenFailTypeIsBadRequest()
    {
        //Arrange
        var fail = new MNoteProcessFail(Common.Abstractions.Enums.MNotesFailType.BADREQUEST, "Bad request message");

        //Act
        var result = fail.ToIResult();
        var resultWithStatusCode = (IStatusCodeHttpResult)result;

        // Assert
        Assert.That(resultWithStatusCode.StatusCode, Is.EqualTo(400));

    }

    [Test]
    public void ToIResulut_Returns404_WhenFailTypeIsNotFound()
    {
        //Arrange
        var fail = new MNoteProcessFail(Common.Abstractions.Enums.MNotesFailType.NOTFOUND, "Not found.");

        //Act
        var result = fail.ToIResult();
        var resultWithStatusCode = (IStatusCodeHttpResult)result;

        // Assert
        Assert.That(resultWithStatusCode.StatusCode, Is.EqualTo(404));

    }


    [Test]
    public void ToIResulut_Returns500_WhenFailTypeIsProblem()
    {
        //Arrange
        var fail = new MNoteProcessFail(Common.Abstractions.Enums.MNotesFailType.PROBLEM, "Problem");

        //Act
        var result = fail.ToIResult();
        var resultWithStatusCode = (IStatusCodeHttpResult)result;

        // Assert
        Assert.That(resultWithStatusCode.StatusCode, Is.EqualTo(500));

    }
}
