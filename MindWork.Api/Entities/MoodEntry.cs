using System;

namespace MindWork.Api.Entities;

public class MoodEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public User User { get; set; } = default!;

    public string Mood { get; set; } = default!;

    public short StressLevel { get; set; }

    public double SleepHours { get; set; }

    public int ScreenTimeMinutes { get; set; }

    public string? Notes { get; set; }

    public string Source { get; set; } = default!;

    public double Confidence { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
