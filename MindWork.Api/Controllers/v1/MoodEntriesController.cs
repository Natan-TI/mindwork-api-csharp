using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindWork.Api.Data;
using MindWork.Api.DTOs.MoodEntry;
using MindWork.Api.Entities;
using MindWork.Api.Enums;
using Microsoft.AspNetCore.Authorization;


namespace MindWork.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/mood-entries")]
[Authorize]
public class MoodEntriesController : ControllerBase
{
    private readonly MindWorkDbContext _db;

    public MoodEntriesController(MindWorkDbContext db)
    {
        _db = db;
    }

    // Método auxiliar para mapear entidade -> DTO
    private static MoodEntryResponseDto MapToResponse(MoodEntry entity)
    {
        return new MoodEntryResponseDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Mood = Enum.Parse<MoodState>(entity.Mood),
            StressLevel = entity.StressLevel,
            SleepHours = entity.SleepHours,
            ScreenTimeMinutes = entity.ScreenTimeMinutes,
            Notes = entity.Notes,
            Source = Enum.Parse<DataSourceType>(entity.Source),
            Confidence = entity.Confidence,
            CreatedAt = entity.CreatedAt
        };
    }

    // GET /api/v1/mood-entries
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MoodEntryResponseDto>>> GetAll()
    {
        var entries = await _db.MoodEntries
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var response = entries.Select(MapToResponse);
        return Ok(response);
    }

    // GET /api/v1/mood-entries/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MoodEntryResponseDto>> GetById(Guid id)
    {
        var entity = await _db.MoodEntries.FindAsync(id);

        if (entity == null)
            return NotFound();

        return Ok(MapToResponse(entity));
    }

    // GET /api/v1/mood-entries/by-user/{userId}
    // Lista todas as mood-entries de um usuário específico
    [HttpGet("by-user/{userId:guid}")]
    public async Task<ActionResult<IEnumerable<MoodEntryResponseDto>>> GetByUser(Guid userId)
    {
        var entries = await _db.MoodEntries
            .Where(e => e.UserId == userId)
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var response = entries.Select(MapToResponse);
        return Ok(response);
    }

    // GET /api/v1/mood-entries/by-organization/{organizationId}
    // Lista todas as mood-entries de todos os usuários de uma organização
    [HttpGet("by-organization/{organizationId:guid}")]
    public async Task<ActionResult<IEnumerable<MoodEntryResponseDto>>> GetByOrganization(Guid organizationId)
    {
        var entries = await _db.MoodEntries
            .Include(e => e.User)
            .Where(e => e.User.OrganizationId == organizationId)
            .AsNoTracking()
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var response = entries.Select(MapToResponse);
        return Ok(response);
    }

    // POST /api/v1/mood-entries
    [HttpPost]
    public async Task<ActionResult<MoodEntryResponseDto>> Create([FromBody] MoodEntryCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Garante que o usuário existe
        var userExists = await _db.Users.AnyAsync(u => u.Id == dto.UserId);
        if (!userExists)
        {
            ModelState.AddModelError(nameof(dto.UserId), "User not found.");
            return ValidationProblem(ModelState);
        }

        var entity = new MoodEntry
        {
            UserId = dto.UserId,
            Mood = dto.Mood.ToString(),
            StressLevel = dto.StressLevel ?? 0,
            SleepHours = dto.SleepHours ?? 0,
            ScreenTimeMinutes = dto.ScreenTimeMinutes ?? 0,
            Notes = dto.Notes,
            Source = dto.Source.ToString(),
            Confidence = dto.Confidence ?? 0.95,
            CreatedAt = DateTime.UtcNow
        };

        _db.MoodEntries.Add(entity);
        await _db.SaveChangesAsync();

        var response = MapToResponse(entity);

        return CreatedAtAction(
            nameof(GetById),
            new { id = entity.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" },
            response
        );
    }

    // Sem PUT / DELETE de propósito
}
