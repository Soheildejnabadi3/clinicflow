using Microsoft.AspNetCore.Mvc;
using ClinicFlow.Api.DTOs.Provider;
using ClinicFlow.Api.DTOs.AvailabilitySlot;
using ClinicFlow.Api.Services.Interfaces;

namespace ClinicFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProviderDto>>> GetAll(
        [FromQuery] string? specialty)
    {
        return Ok(await _providerService.GetAllAsync(specialty));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProviderDto>> GetById(int id)
    {
        var provider = await _providerService.GetByIdAsync(id);
        if (provider == null) return NotFound();
        return Ok(provider);
    }

    [HttpPost]
    public async Task<ActionResult<ProviderDto>> Create(
        [FromBody] CreateProviderDto dto)
    {
        var provider = await _providerService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),
            new { id = provider.Id }, provider);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProviderDto>> Update(
        int id, [FromBody] UpdateProviderDto dto)
    {
        var provider = await _providerService.UpdateAsync(id, dto);
        if (provider == null) return NotFound();
        return Ok(provider);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _providerService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/availability")]
    public async Task<ActionResult<List<AvailabilitySlotDto>>> GetAvailability(int id)
    {
        return Ok(await _providerService.GetAvailabilityAsync(id));
    }

    [HttpPost("{id}/availability")]
    public async Task<ActionResult<AvailabilitySlotDto>> AddAvailability(
        int id, [FromBody] CreateAvailabilitySlotDto dto)
    {
        dto.ProviderId = id;
        var slot = await _providerService.AddAvailabilitySlotAsync(dto);
        return Created($"/api/providers/{id}/availability", slot);
    }

    [HttpDelete("availability/{slotId}")]
    public async Task<IActionResult> RemoveAvailability(int slotId)
    {
        var success = await _providerService.RemoveAvailabilitySlotAsync(slotId);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpGet("{id}/available")]
    public async Task<ActionResult<bool>> CheckAvailability(
        int id,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var available = await _providerService.IsAvailableAsync(id, startTime, endTime);
        return Ok(new { available });
    }
}