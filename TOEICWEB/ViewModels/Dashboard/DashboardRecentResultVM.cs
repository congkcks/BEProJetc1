using System.Text.Json.Serialization;

namespace TOEICWEB.ViewModels.Dashboard;

public class DashboardRecentResultVM
{
    public string Exercise { get; set; } = string.Empty;

    public int Score { get; set; }

    [JsonIgnore]
    public DateTime CompletedAt { get; set; }
}
