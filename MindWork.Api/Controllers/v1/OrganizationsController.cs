using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MindWork.Api.Data;
using MindWork.Api.DTOs.Organization;
using MindWork.Api.Entities;
using Microsoft.AspNetCore.Authorization;

namespace MindWork.Api.Controllers.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/organizations")]
[Authorize]
public class OrganizationsController : ControllerBase
{
    private readonly MindWorkDbContext _db;

    public OrganizationsController(MindWorkDbContext db)
    {
        _db = db;
    }

    // GET /api/v1/organizations
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrganizationResponseDto>>> GetAll()
    {
        var orgs = await _db.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .ToListAsync();

        var response = orgs.Select(o => new OrganizationResponseDto
        {
            Id = o.Id,
            Name = o.Name,
            CreatedAt = o.CreatedAt
        });

        return Ok(response);
    }

    // GET /api/v1/organizations/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrganizationResponseDto>> GetById(Guid id)
    {
        var org = await _db.Organizations.FindAsync(id);

        if (org == null)
            return NotFound();

        var response = new OrganizationResponseDto
        {
            Id = org.Id,
            Name = org.Name,
            CreatedAt = org.CreatedAt
        };

        return Ok(response);
    }

    // POST /api/v1/organizations
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<OrganizationResponseDto>> Create([FromBody] OrganizationCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var org = new Organization
        {
            Name = dto.Name
        };

        _db.Organizations.Add(org);
        await _db.SaveChangesAsync();

        var response = new OrganizationResponseDto
        {
            Id = org.Id,
            Name = org.Name,
            CreatedAt = org.CreatedAt
        };

        return CreatedAtAction(
            nameof(GetById),
            new { id = org.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" },
            response
        );
    }

    // PUT /api/v1/organizations/{id}
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] OrganizationCreateDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var org = await _db.Organizations.FindAsync(id);

        if (org == null)
            return NotFound();

        org.Name = dto.Name;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    // DELETE /api/v1/organizations/{id}
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var org = await _db.Organizations.FindAsync(id);

        if (org == null)
            return NotFound();

        var hasUsers = await _db.Users.AnyAsync(u => u.OrganizationId == id);
        if (hasUsers)
        {
            return BadRequest("Cannot delete organization with associated users.");
        }

        _db.Organizations.Remove(org);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
