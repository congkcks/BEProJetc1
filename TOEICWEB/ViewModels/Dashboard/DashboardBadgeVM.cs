namespace TOEICWEB.ViewModels.Dashboard;

public class DashboardBadgeVM
{
    public string BadgeId { get; set; } = string.Empty;

    public string BadgeName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime AwardedAt { get; set; }
}
