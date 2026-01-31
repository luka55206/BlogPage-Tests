using System.Net;
using System.Net.Http.Json;
using BlogPage.Application.Users;
using FluentAssertions;

namespace BlogPage.Tests.Integration;

public class UserEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public UserEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var user = await response.Content.ReadFromJsonAsync<UserDto>();
        user.Should().NotBeNull();
        user!.Username.Should().Be("newuser");
        user.Email.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Email = "not-an-email",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterUserRequest
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "weak"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/users/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // [Fact]
    // public async Task Login_WithValidCredentials_ShouldReturnToken()
    // {
    //     // Arrange - Register first
    //     var registerRequest = new RegisterUserRequest
    //     {
    //         Username = "logintest",
    //         Email = "login@example.com",
    //         Password = "Password123!"
    //     };
    //
    //     await _client.PostAsJsonAsync("/users/register", registerRequest);
    //
    //     var loginRequest = new Microsoft.AspNetCore.Identity.Data.LoginRequest(
    //         "login@example.com",
    //         "Password123!"
    //     );
    //
    //     // Act
    //     var response = await _client.PostAsJsonAsync("/users/login", loginRequest);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.OK);
    //     var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
    //     result.Should().NotBeNull();
    //     result!.token.Should().NotBeNullOrEmpty();
    // }
    //
    // [Fact]
    // public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    // {
    //     // Arrange
    //     var registerRequest = new RegisterUserRequest
    //     {
    //         Username = "wrongpass",
    //         Email = "wrongpass@example.com",
    //         Password = "Password123!"
    //     };
    //
    //     await _client.PostAsJsonAsync("/users/register", registerRequest);
    //
    //     var loginRequest = new Microsoft.AspNetCore.Identity.Data.LoginRequest(
    //         "wrongpass@example.com",
    //         "WrongPassword!"
    //     );
    //
    //     // Act
    //     var response = await _client.PostAsJsonAsync("/users/login", loginRequest);
    //
    //     // Assert
    //     response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    // }
}

public record LoginResponse(string token);