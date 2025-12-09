namespace TOEICWEB.ViewModels.Dashboard;

public class DashboardPlanItemVM
{
    public string PlanId { get; set; } = string.Empty;

    public string? LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Status { get; set; } = "Pending";

    public string StartTime { get; set; } = string.Empty;

    public string EndTime { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public int ProgressPercent { get; set; }
}
