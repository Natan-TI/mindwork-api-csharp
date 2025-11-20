using System;

namespace MindWork.Api.DTOs.Organization;

public class OrganizationResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
