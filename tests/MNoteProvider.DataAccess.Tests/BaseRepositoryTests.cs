using MNoteProvider.DataAccess.Repositories;
using MNoteProvider.Domain;
using MNoteProvider.Domain.Abstractions;

namespace MNoteProvider.DataAccess.Tests;

[TestFixture]
public class BaseRepositoryTests
{
    [Test]
    public void SelectByIdSql_ForNote_FiltersOnPrimaryKey()
    {
        // Act
        var sql = BaseRepository<Note, INote>.SelectByIdSql;

        // Assert
        Assert.That(sql, Does.EndWith("FROM note WHERE id = @Id;"));
    }

    [Test]
    public void SelectAllSql_ForNote_OrdersByLastModification()
    {
        // Act
        var sql = BaseRepository<Note, INote>.SelectAllSql;

        // Assert
        Assert.That(sql, Does.EndWith("FROM note ORDER BY doeom DESC;"));
    }

    [Test]
    public void InsertSql_ForNote_TargetsTheTableFromTableAttribute()
    {
        // Act
        var sql = BaseRepository<Note, INote>.InsertSql;

        // Assert
        Assert.That(sql, Does.StartWith("INSERT INTO note"));
    }

    [Test]
    public void InsertSql_ForNote_IncludesImmutableColumns()
    {
        // Act
        var sql = BaseRepository<Note, INote>.InsertSql;

        // Assert
        Assert.That(sql, Does.Contain("creationdate"));
    }

    [Test]
    public void UpdateSql_ForFolder_ExcludesImmutableColumns()
    {
        // Act
        var sql = BaseRepository<Folder, IFolder>.UpdateSql;

        // Assert
        Assert.That(sql, Does.Not.Contain("creationdate"));
        Assert.That(sql, Does.Contain("name = @Name"));
        Assert.That(sql, Does.Contain("WHERE id = @Id"));
    }

    [Test]
    public void DeleteSql_ForNote_FiltersOnPrimaryKey()
    {
        // Act
        var sql = BaseRepository<Note, INote>.DeleteSql;

        // Assert
        Assert.That(sql, Is.EqualTo("DELETE FROM note WHERE id = @Id;"));
    }

    [Test]
    public void BuildInsertParameters_ForNote_IncludesImmutableColumns()
    {
        // Arrange
        var note = CreateNote();

        // Act
        var parameters = BaseRepository<Note, INote>.BuildInsertParameters(note);

        // Assert
        Assert.That(parameters.ParameterNames, Is.EquivalentTo(new[]
        {
        nameof(Note.Id),
        nameof(Note.Name),
        nameof(Note.Content),
        nameof(Note.Description),
        nameof(Note.FolderId),
        nameof(Note.CreationDate),
        nameof(Note.Doeom)
        }));
    }

    [Test]
    public void BuildUpdateParameters_ForNote_ExcludesImmutableColumns()
    {
        // Arrange
        var note = CreateNote();

        // Act
        var parameters = BaseRepository<Note, INote>.BuildUpdateParameters(note);


        //Assert
        Assert.That(parameters.ParameterNames, Is.EquivalentTo(new[]
        {
                        nameof(Note.Id),
            nameof(Note.Name),
            nameof(Note.Content),
            nameof(Note.Description),
            nameof(Note.FolderId),
            nameof(Note.Doeom)
        }));
    }

    [Test]
    public void BuildUpdateParameters_ForNote_CarriesTheEntityValues()
    {
        // Arrange
        var note = CreateNote(name: "Besprechungsnotiz");

        // Act
        var parameters = BaseRepository<Note, INote>.BuildUpdateParameters(note);

        // Assert
        Assert.That(parameters.Get<string>(nameof(Note.Name)), Is.EqualTo("Besprechungsnotiz"));
        Assert.That(parameters.ParameterNames, Does.Contain(nameof(Note.Id)));
    }

    private static Note CreateNote(
        string name = "Testnotiz",
        Guid? id = null,
        Guid? folderId = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Content = "Inhalt",
            Description = "Beschreibung",
            FolderId = folderId ?? Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            Doeom = DateTime.UtcNow
        };
}
