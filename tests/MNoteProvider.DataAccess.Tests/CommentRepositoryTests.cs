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
        Assert.That(sql, Is.EqualTo("SELECT * FROM comment WHERE noteid = @NoteId;"));
    }
}
