// Controllers/PatientsController.cs
using Microsoft.AspNetCore.Mvc;
using ClinicFlow.Api.DTOs.Patient;
using ClinicFlow.Api.Services.Interfaces;

namespace ClinicFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]  // Route: /api/patients
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    // GET /api/patients
    [HttpGet]
    public async Task<ActionResult<List<PatientDto>>> GetAll([FromQuery] string? search)
    {
        var patients = await _patientService.GetAllAsync(search);
        return Ok(patients);
    }

    // GET /api/patients/5
    [HttpGet("{id}")]
    public async Task<ActionResult<PatientDto>> GetById(int id)
    {
        var patient = await _patientService.GetByIdAsync(id);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    // POST /api/patients
    [HttpPost]
    public async Task<ActionResult<PatientDto>> Create([FromBody] CreatePatientDto dto)
    {
        var patient = await _patientService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = patient.Id }, patient);
    }

    // PUT /api/patients/5
    [HttpPut("{id}")]
    public async Task<ActionResult<PatientDto>> Update(int id, [FromBody] UpdatePatientDto dto)
    {
        var patient = await _patientService.UpdateAsync(id, dto);
        if (patient == null) return NotFound();
        return Ok(patient);
    }

    // DELETE /api/patients/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _patientService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}