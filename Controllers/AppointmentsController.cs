using Microsoft.AspNetCore.Mvc;
using ClinicFlow.Api.DTOs.Appointment;
using ClinicFlow.Api.Services.Interfaces;

namespace ClinicFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AppointmentDto>>> GetAll(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var appointments = await _appointmentService.GetAllAsync(fromDate, toDate);
        return Ok(appointments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDto>> GetById(int id)
    {
        var appointment = await _appointmentService.GetByIdAsync(id);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    [HttpGet("patient/{patientId}")]
    public async Task<ActionResult<List<AppointmentDto>>> GetByPatient(int patientId)
    {
        return Ok(await _appointmentService.GetByPatientIdAsync(patientId));
    }

    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<List<AppointmentDto>>> GetByProvider(
        int providerId, [FromQuery] DateTime? date)
    {
        return Ok(await _appointmentService.GetByProviderIdAsync(providerId, date));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboard()
    {
        return Ok(await _appointmentService.GetDashboardStatsAsync());
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDto>> Create(
        [FromBody] CreateAppointmentDto dto)
    {
        var appointment = await _appointmentService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById),
            new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AppointmentDto>> Update(
        int id, [FromBody] UpdateAppointmentDto dto)
    {
        var appointment = await _appointmentService.UpdateAsync(id, dto);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    [HttpPost("{id}/cancel")]
    public async Task<ActionResult<AppointmentDto>> Cancel(
        int id, [FromBody] CancelRequest request)
    {
        var appointment = await _appointmentService.CancelAsync(id, request.Reason);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<AppointmentDto>> Complete(int id)
    {
        var appointment = await _appointmentService.CompleteAsync(id);
        if (appointment == null) return NotFound();
        return Ok(appointment);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _appointmentService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}

public record CancelRequest(string Reason);