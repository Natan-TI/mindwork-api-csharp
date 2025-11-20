using System;
using System.Collections.Generic;

namespace MindWork.Api.Entities;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // One-to-many
    public List<User> Users { get; set; } = new();
}
