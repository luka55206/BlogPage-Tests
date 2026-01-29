using BlogPage.Application.Comments;
using BlogPage.Domain.Entities;
using BlogPage.Domain.Exceptions;
using BlogPage.Persistence.Context;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BlogPage.Tests.Services;

public class CommentServiceTests : IDisposable

{
    private readonly BlogDbContext _context;
    private readonly CommentService _commentService;

    public CommentServiceTests()
    {
        var options = new DbContextOptionsBuilder<BlogDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        
        _context = new BlogDbContext(options);
        _commentService = new CommentService(_context);
        
        SeedTestData();
    }
    
    private void SeedTestData()
    {
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hash"
        };

        var post = new Post
        {
            Id = 1,
            Title = "Test Post",
            Content = "Test Content",
            AuthorId = 1,
            PublishDate = DateTime.UtcNow
        };

        _context.Users.Add(user);
        _context.Posts.Add(post);
        _context.SaveChanges();
    }
    
    [Fact]
    public async Task CreateCommentAsync_WithValidData_ShouldCreateComment()
    {
        var command = new CreateCommentCommand(
            PostId: 1,
            UserId: 1,
            Content: "Test Content");
        
        var result = await _commentService.CreateCommentAsync(command);
        
        result.Should().NotBeNull();
        result.Author.Should().Be("testuser");
        result.Content.Should().Be("Test Content");
    }

    [Fact]
    public async Task CreateCommentAsync_WithInvalidPostId_ShouldThrowNotFoundException()
    {
        var command = new CreateCommentCommand(
            PostId: 999,
            UserId: 1,
            Content: "Test Content");
        
        
        var act = async () => await _commentService.CreateCommentAsync(command);
        
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"Post with id 999 does not exist");
        
    }
    
    [Fact]
    public async Task CreateCommentAsync_ShouldSetCorrectTimestamp()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow;
        var command = new CreateCommentCommand(
            PostId: 1,
            UserId: 1,
            Content: "Test comment"
        );

        // Act
        var result = await _commentService.CreateCommentAsync(command);
        var afterTime = DateTime.UtcNow;

        // Assert
        result.DateCreated.Should().BeAfter(beforeTime.AddSeconds(-1));
        result.DateCreated.Should().BeBefore(afterTime.AddSeconds(1));
    }

    
    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}


