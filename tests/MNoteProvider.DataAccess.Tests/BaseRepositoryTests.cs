using FluentAssertions;
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
        sql.Should().EndWith("FROM note WHERE id = @Id;");
    }

    [Test]
    public void SelectAllSql_ForNote_OrdersByLastModification()
    {
        // Act
        var sql = BaseRepository<Note, INote>.SelectAllSql;

        // Assert
        sql.Should().EndWith("FROM note ORDER BY doeom DESC;");
    }

    [Test]
    public void InsertSql_ForNote_TargetsTheTableFromTableAttribute()
    {
        // Act
        var sql = BaseRepository<Note, INote>.InsertSql;

        // Assert
        sql.Should().StartWith("INSERT INTO note");
    }

    [Test]
    public void InsertSql_ForNote_IncludesImmutableColumns()
    {
        // Act
        var sql = BaseRepository<Note, INote>.InsertSql;

        // Assert
        sql.Should().Contain("creationdate");
    }

    [Test]
    public void UpdateSql_ForFolder_ExcludesImmutableColumns()
    {
        // Act
        var sql = BaseRepository<Folder, IFolder>.UpdateSql;

        // Assert
        sql.Should().NotContain("creationdate");
        sql.Should().Contain("name = @Name");
        sql.Should().Contain("WHERE id = @Id");
    }

    [Test]
    public void DeleteSql_ForNote_FiltersOnPrimaryKey()
    {
        // Act
        var sql = BaseRepository<Note, INote>.DeleteSql;

        // Assert
        sql.Should().Be("DELETE FROM note WHERE id = @Id;");
    }

    [Test]
    public void BuildInsertParameters_ForNote_IncludesImmutableColumns()
    {
        // Arrange
        var note = CreateNote();

        // Act
        var parameters = BaseRepository<Note, INote>.BuildInsertParameters(note);

        // Assert
        parameters.ParameterNames.Should().BeEquivalentTo(
            nameof(Note.Id),
            nameof(Note.Name),
            nameof(Note.Content),
            nameof(Note.Description),
            nameof(Note.FolderId),
            nameof(Note.CreationDate),
            nameof(Note.Doeom));
    }

    [Test]
    public void BuildUpdateParameters_ForNote_ExcludesImmutableColumns()
    {
        // Arrange
        var note = CreateNote();

        // Act
        var parameters = BaseRepository<Note, INote>.BuildUpdateParameters(note);

        // Assert
        parameters.ParameterNames.Should().BeEquivalentTo(
            nameof(Note.Id),
            nameof(Note.Name),
            nameof(Note.Content),
            nameof(Note.Description),
            nameof(Note.FolderId),
            nameof(Note.Doeom));
    }

    [Test]
    public void BuildUpdateParameters_ForNote_CarriesTheEntityValues()
    {
        // Arrange
        var note = CreateNote(name: "Besprechungsnotiz");

        // Act
        var parameters = BaseRepository<Note, INote>.BuildUpdateParameters(note);

        // Assert
        parameters.Get<string>(nameof(Note.Name)).Should().Be("Besprechungsnotiz");
        parameters.Get<Guid>(nameof(Note.Id)).Should().Be(note.Id);
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
