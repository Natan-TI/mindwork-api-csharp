using System;

namespace MindWork.Api.DTOs.User;

public class UserCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public Guid OrganizationId { get; set; }
}
