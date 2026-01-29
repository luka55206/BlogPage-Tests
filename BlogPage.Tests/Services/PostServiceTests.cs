using BlogPage.Application.Posts;
using BlogPage.Domain.Entities;
using BlogPage.Domain.Exceptions;
using BlogPage.Persistence.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BlogPage.Tests.Services;

public class PostServiceTests : IDisposable
{
    private readonly BlogDbContext _context;
    private readonly PostService _postService;

    public PostServiceTests()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BlogDbContext(options);
        _postService = new PostService(_context);
        
        SeedTestData();
    }

    private void SeedTestData()
    {
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash",
            Role = "User"
        };
        
        _context.Users.Add(user);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CreatePostAync_WithValidData_ShouldCreatePost()
    {
        var command = new CreatePostCommand(
            AuthorId: 1,
            Title: "Test Post",
            Content: "This is a test post with enough content to pass validation requirements.",
            Tags: new List<string> { "test", "unit-test" }
            );
        
        var result = await _postService.CreatePostAsync(command);
        
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Post");
        result.Author.Should().Be("testuser");
        result.Tags.Should().HaveCount(2);
        result.Tags.Should().Contain("test");
        result.Tags.Should().Contain("unit-test");
    }   


    [Fact]
    public async Task CreatePostAsync_WithDuplicateTags_ShouldDeduplicateTags()
    {
        var command = new CreatePostCommand(
            AuthorId: 1,
            Title: "Test Post",
            Content: "This is a test post with enough content to pass validation requirements.",
            Tags: new List<string> { "test", "Test", "TEST" }
        );
        
        var result = await _postService.CreatePostAsync(command);
        
        result.Should().NotBeNull();
        result.Tags.Should().HaveCount(1);

    }

    [Fact]
    public async Task CreatePostAsync_WithExistingTags_ShouldReuseTag()
    {
        var existingTag  = new Tag { Name = "existing" };
        _context.Add(existingTag);
        await _context.SaveChangesAsync();
        
        var command = new CreatePostCommand(
            AuthorId: 1,
            Title: "Test Post",
            Content: "This is a test post with enough content to pass validation requirements.",
            Tags: new List<string> { "existing", "new" }
        );
        var result = await _postService.CreatePostAsync(command);
       
        var tagsInDb = await _context.Tags.CountAsync();
        tagsInDb.Should().Be(2);
        result.Tags.Should().Contain("existing");
        result.Tags.Should().Contain("new");
        
    }
    [Fact]
    public async Task CreatePostAsync_WithWhitespaceTags_ShouldTrimAndClean()
    {
        // Arrange
        var command = new CreatePostCommand(
            AuthorId: 1,
            Title: "Test Post",
            Content: "This is a test post with enough content to pass validation requirements.",
            Tags: new List<string> { "  test  ", "DOTNET", "" }
        );

        // Act
        var result = await _postService.CreatePostAsync(command);

        // Assert
        result.Tags.Should().HaveCount(2);
        result.Tags.Should().Contain("test");
        result.Tags.Should().Contain("dotnet");
        result.Tags.Should().NotContain("");
    }
    
    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}