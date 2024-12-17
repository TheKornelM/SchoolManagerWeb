using Radzen;

namespace SchoolManagerWeb.Utils;

public class Notifier(NotificationService notificationService)
{
    public required NotificationService NotificationService { get; set; } = notificationService;

    public void ShowError(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Error,
            Summary = "Error",
            Detail = message,
            Duration = 4000,
            Style = "word-break:break-word"
        });
    }

    public void ShowSuccess(string message)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Success",
            Detail = message,
            Duration = 4000,
            Style = "word-break:break-word"
        });
    }
}