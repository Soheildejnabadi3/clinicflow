using MediatR;

namespace ClinicFlow.Api.Events;

public record AppointmentCreatedEvent(
    int AppointmentId,
    int PatientId,
    int ProviderId,
    DateTime StartTime
) : INotification;

public record AppointmentCancelledEvent(
    int AppointmentId,
    int PatientId,
    int ProviderId,
    DateTime StartTime,
    string Reason
) : INotification;