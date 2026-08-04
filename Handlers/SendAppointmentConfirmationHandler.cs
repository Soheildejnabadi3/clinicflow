using MediatR;
using ClinicFlow.Api.Events;

namespace ClinicFlow.Api.Handlers;

public class SendAppointmentConfirmationHandler
    : INotificationHandler<AppointmentCreatedEvent>
{
    private readonly ILogger<SendAppointmentConfirmationHandler> _logger;

    public SendAppointmentConfirmationHandler(
        ILogger<SendAppointmentConfirmationHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(
        AppointmentCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // In production: send email/SMS confirmation
        _logger.LogInformation(
            "Sending confirmation for appointment {Id} to patient {PatientId}",
            notification.AppointmentId,
            notification.PatientId);

        // Simulate async work
        await Task.Delay(100, cancellationToken);
    }
}

// Another handler for the same event (fan-out pattern)
public class UpdateProviderScheduleHandler
    : INotificationHandler<AppointmentCreatedEvent>
{
    public async Task Handle(
        AppointmentCreatedEvent notification,
        CancellationToken cancellationToken)
    {
        // Update provider's availability
        // Block the time slot so no double-booking occurs
        await Task.CompletedTask;
    }
}