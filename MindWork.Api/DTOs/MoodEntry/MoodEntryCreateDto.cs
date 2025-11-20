using System;
using MindWork.Api.Enums;

namespace MindWork.Api.DTOs.MoodEntry;

public class MoodEntryCreateDto
{
    public Guid UserId { get; set; }

    public MoodState Mood { get; set; }

    public short? StressLevel { get; set; }

    public double? SleepHours { get; set; }

    public int? ScreenTimeMinutes { get; set; }

    public string? Notes { get; set; }

    public DataSourceType Source { get; set; } = DataSourceType.Manual;

    public double? Confidence { get; set; }
}
