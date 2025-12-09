namespace TOEICWEB.ViewModels.Dashboard;

public class DashboardSummaryVM
{
    public int StreakDays { get; set; }

    public int PredictedScore { get; set; }

    public int CompletedLessons { get; set; }

    public int TotalLessons { get; set; }

    public double StudyTimeHours { get; set; }
}
