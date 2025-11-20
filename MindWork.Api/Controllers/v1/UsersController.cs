using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindWork.Api.Data;
using MindWork.Api.DTOs.User;
using MindWork.Api.Entities;
using Microsoft.AspNetCore.Authorization;


namespace MindWork.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly MindWorkDbContext _db;

    public UsersController(MindWorkDbContext db)
    {
        _db = db;
    }

    // GET /api/v1/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll()
    {
        var users = await _db.Users
            .AsNoTracking()
            .ToListAsync();

        var response = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            OrganizationId = u.OrganizationId,
            CreatedAt = u.CreatedAt
        });

        return Ok(response);
    }

    // GET /api/v1/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id)
    {
        var u = await _db.Users.FindAsync(id);

        if (u == null)
            return NotFound();

        var response = new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            OrganizationId = u.OrganizationId,
            CreatedAt = u.CreatedAt
        };

        return Ok(response);
    }

    // GET /api/v1/users/by-organization/{organizationId}
    // Lista todos os usuários de uma organização
    [HttpGet("by-organization/{organizationId:guid}")]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetByOrganization(Guid organizationId)
    {
        var users = await _db.Users
            .Where(u => u.OrganizationId == organizationId)
            .AsNoTracking()
            .ToListAsync();

        var response = users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            OrganizationId = u.OrganizationId,
            CreatedAt = u.CreatedAt
        });

        return Ok(response);
    }

    // POST /api/v1/users
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] UserCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var orgExists = await _db.Organizations
            .AnyAsync(o => o.Id == dto.OrganizationId);

        if (!orgExists)
        {
            ModelState.AddModelError(nameof(dto.OrganizationId), "Organization not found.");
            return ValidationProblem(ModelState);
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = dto.Password,
            OrganizationId = dto.OrganizationId,
            Role = "Employee"
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var response = new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            OrganizationId = user.OrganizationId,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" },
            response
        );
    }

    // PUT /api/v1/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var user = await _db.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        var orgExists = await _db.Organizations
            .AnyAsync(o => o.Id == dto.OrganizationId);

        if (!orgExists)
        {
            ModelState.AddModelError(nameof(dto.OrganizationId), "Organization not found.");
            return ValidationProblem(ModelState);
        }

        user.Name = dto.Name;
        user.Email = dto.Email;
        user.Password = dto.Password;
        user.OrganizationId = dto.OrganizationId;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/v1/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await _db.Users.FindAsync(id);

        if (user == null)
            return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
