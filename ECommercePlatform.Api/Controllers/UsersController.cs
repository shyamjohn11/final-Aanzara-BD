using System.ComponentModel.DataAnnotations;
using ECommercePlatform.Api.Common;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Common.Security;
using ECommercePlatform.Application.Features.Users.Commands.UpdateUserRole;
using ECommercePlatform.Application.Features.Users.Commands.UpdateUserStatus;
using ECommercePlatform.Application.Features.Users.Dtos;
using ECommercePlatform.Application.Features.Users.Queries.GetUserDetails;
using ECommercePlatform.Domain.Constants;
using ECommercePlatform.Domain.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.Api.Controllers;

/// <summary>
/// Full administration controller for managing user accounts.
/// Supports listing, viewing, creating, updating, and deleting users.
/// Restricted to users with the Admin role.
/// </summary>
[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/admin/users")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly ISender _sender;

    public UsersController(
        IUserService userService,
        IRoleService roleService,   
        ISender sender)
    {
        _userService = userService;
        _roleService = roleService;
        _sender = sender;
    }

    /// <summary>
    /// Returns every registered user account along with their assigned roles, paginated.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserListItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<UserListItemResponse>>> GetAllUsers(
        CancellationToken cancellationToken,
        [FromQuery] int Page = 1,
        [FromQuery] int PageSize = 25)
    {
        var allUsers = await _userService.GetAllUsersAsync(cancellationToken);

        var totalCount = allUsers.Count;
        var skip = (Page - 1) * PageSize;
        var pageUsers = allUsers
            .Skip(skip)
            .Take(PageSize)
            .ToArray();

        var responseList = new List<UserListItemResponse>(pageUsers.Length);

        foreach (var user in pageUsers)
        {
            var userRoles = await _roleService.GetRolesForUserAsync(user.UserId, cancellationToken);
            responseList.Add(new UserListItemResponse(
                user.UserId,
                user.Name,
                user.Email,
                user.Phone,
                user.Status,
                userRoles));
        }

        return new PagedResult<UserListItemResponse>(responseList, Page, PageSize, totalCount);
    }

    /// <summary>
    /// Returns complete details of a specific user by their unique identifier.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(UserDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailsResponse>> GetUserDetails(
        Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserDetailsQuery(userId), cancellationToken);
        return ToResponse(result);
    }

    /// <summary>
    /// Creates a new user account with the provided name, email, and password.
    /// The new user is automatically assigned the Customer role.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserDetailResponse>> CreateNewUser(
        [FromBody] CreateNewUserRequest request, CancellationToken cancellationToken)
    {
        var creationResult = await _userService.CreateNewUserAsync(
            request.Name, request.Email, request.Passphrase, cancellationToken);

        if (creationResult.IsFailure)
        {
            return ToProblem(creationResult.Error!);
        }

        var createdUser = await _userService.FindUserByIdAsync(creationResult.Value, cancellationToken);
        if (createdUser is null)
        {
            return NotFound();
        }

        var userRoles = await _roleService.GetRolesForUserAsync(createdUser.UserId, cancellationToken);
        var response = new UserDetailResponse(
            createdUser.UserId, createdUser.Name, createdUser.Email,
            createdUser.Phone, createdUser.Status, userRoles);

        return CreatedAtAction(nameof(GetUserDetails), new { userId = createdUser.UserId }, response);
    }

    /// <summary>
    /// Updates an existing user's name, email, phone number, and status.
    /// </summary>
    [HttpPut("{userId:guid}")]
    [ProducesResponseType(typeof(UserDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDetailResponse>> UpdateExistingUser(
        Guid userId, [FromBody] UpdateExistingUserRequest request, CancellationToken cancellationToken)
    {
        var updateResult = await _userService.UpdateExistingUserAsync(
            userId, request.Name, request.Email, request.Phone, request.Status, cancellationToken);

        if (updateResult.IsFailure)
        {
            return ToProblem(updateResult.Error!);
        }

        var updatedUser = await _userService.FindUserByIdAsync(userId, cancellationToken);
        if (updatedUser is null)
        {
            return NotFound();
        }

        var userRoles = await _roleService.GetRolesForUserAsync(updatedUser.UserId, cancellationToken);
        var response = new UserDetailResponse(
            updatedUser.UserId, updatedUser.Name, updatedUser.Email,
            updatedUser.Phone, updatedUser.Status, userRoles);

        return Ok(response);
    }

    /// <summary>
    /// Blocks or unblocks a user account by updating their status.
    /// </summary>
    [HttpPatch("{userId:guid}/status")]
    [ProducesResponseType(typeof(UpdateUserStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateUserStatusResponse>> UpdateUserStatus(
        Guid userId, [FromBody] UpdateUserStatusRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateUserStatusCommand(userId, request.Status), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>
    /// Updates a user's role assignment.
    /// </summary>
    [HttpPatch("{userId:guid}/role")]
    [ProducesResponseType(typeof(UpdateUserRoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UpdateUserRoleResponse>> UpdateUserRole(
        Guid userId, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateUserRoleCommand(userId, request.Role), cancellationToken);

        return ToResponse(result);
    }

    /// <summary>
    /// Permanently removes a user account by their unique identifier.
    /// </summary>
    [HttpDelete("{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveUser(Guid userId, CancellationToken cancellationToken)
    {
        var deletionResult = await _userService.DeleteUserAsync(userId, cancellationToken);

        if (deletionResult.IsFailure)
        {
            return ToProblem(deletionResult.Error!);
        }

        return NoContent();
    }

    /// <summary>
    /// Converts a domain error into a consistent ProblemDetails HTTP response.
    /// </summary>
    private ActionResult ToProblem(Error error)
    {
        var statusCode = error.Type.ToStatusCode();
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type.ToTitle(),
            Detail = error.Message,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = $"{Request.Method} {Request.Path}"
        };
        problemDetails.Extensions["code"] = error.Code;
        problemDetails.Extensions["requestId"] = HttpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private ActionResult ToResponse<T>(Result<T> result)
    {
        if (result.IsFailure)
        {
            return ToProblem(result.Error!);
        }

        return Ok(result.Value);
    }
}

/// <summary>
/// Summary view of a user, used in the list-all endpoint.
/// </summary>
public sealed record UserListItemResponse(
    Guid UserId,
    string Name,
    string Email,
    string? Phone,
    string Status,
    IReadOnlyCollection<string> Roles);

/// <summary>
/// Full detail view of a user, used in create and update responses.
/// </summary>
public sealed record UserDetailResponse(
    Guid UserId,
    string Name,
    string Email,
    string? Phone,
    string Status,
    IReadOnlyCollection<string> Roles);

/// <summary>
/// Request payload sent when a new user account is being created.
/// Password must be at least 12 characters and confirmed with matching input.
/// </summary>
public sealed class CreateNewUserRequest
{
    [Required][MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required][EmailAddress][MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required][MinLength(12)]
    public string Passphrase { get; set; } = string.Empty;

    [Required][Compare(nameof(Passphrase))]
    public string ConfirmPassphrase { get; set; } = string.Empty;
}

/// <summary>
/// Request payload sent when an existing user account is being updated.
/// All fields are required except Phone which can be left blank.
/// </summary>
public sealed class UpdateExistingUserRequest
{
    [Required][MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required][EmailAddress][MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    [Required]
    public string Status { get; set; } = string.Empty;
}
