using FluentAssertions;
using MNoteProvider.DataAccess.Repositories;

namespace MNoteProvider.DataAccess.Tests;

[TestFixture]
public class CommentRepositoryTests
{
    [Test]
    public void SelectAllByNoteIdSql_FiltersOnNote()
    {
        //Act
        var sql = CommentRepository.SelectAllByNoteIdSql;

        //Assert
        sql.Should().Be("SELECT * FROM comment WHERE noteid = @NoteId;");
    }
}
