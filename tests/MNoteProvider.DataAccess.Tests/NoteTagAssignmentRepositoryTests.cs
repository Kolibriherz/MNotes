using MNoteProvider.DataAccess.Repositories;

namespace MNoteProvider.DataAccess.Tests;

[TestFixture]
public class NoteTagAssignmentRepositoryTests
{
    [Test]
    public void DeleteAssignmentSql_FiltersOnNoteAndTag()
    {

        //Act
        var sql = NoteTagAssignmentRepository.DeleteAssignmentSql;

        //Assert
        Assert.That(sql, Is.EqualTo("DELETE FROM notetagassignment WHERE noteid = @NoteId AND tagid = @TagId;"));
    }


}



