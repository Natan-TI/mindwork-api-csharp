using System;

namespace MindWork.Api.DTOs.User;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public Guid OrganizationId { get; set; }

    public DateTime CreatedAt { get; set; }
}
