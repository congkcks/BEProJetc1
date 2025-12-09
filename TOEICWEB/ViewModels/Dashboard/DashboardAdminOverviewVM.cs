using System;

namespace TOEICWEB.ViewModels.Dashboard;

public class DashboardAdminOverviewVM
{
    public int TotalUsers { get; set; }

    public int TotalLessons { get; set; }

    public int ListeningLessons { get; set; }

    public int ReadingLessons { get; set; }

    public int WritingLessons { get; set; }

    public DateTime LastUpdated { get; set; }
}
