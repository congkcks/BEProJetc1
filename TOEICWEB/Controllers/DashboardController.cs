using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TOEICWEB.Data;
using TOEICWEB.ViewModels.Dashboard;

namespace ToeicWeb.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly SupabaseDbContext _context;

    public DashboardController(SupabaseDbContext context)
    {
        _context = context;
    }

    private sealed record ProgressSnapshot(
        string? LessonId,
        string? Status,
        int CompletionPercent,
        int StudyMinutes,
        DateTime? UpdatedAt,
        DateTime? CompletedAt
    );

    private (string TargetId, IActionResult? Error) ResolveTargetUser(string? providedId)
    {
        var maNdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(maNdFromToken))
        {
            return (string.Empty, Unauthorized(new { message = "Phiên làm việc không hợp lệ." }));
        }

        if (string.IsNullOrWhiteSpace(providedId) || string.Equals(providedId, maNdFromToken, StringComparison.OrdinalIgnoreCase))
        {
            return (maNdFromToken, null);
        }

        var role = User.FindFirst("VaiTro")?.Value;
        if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return (string.Empty, Forbid());
        }

        return (providedId, null);
    }

    private static int CalculateStreak(IEnumerable<DateTime> timestamps)
    {
        var uniqueDates = timestamps
            .Select(t => DateOnly.FromDateTime(t.ToUniversalTime().Date))
            .ToHashSet();

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var streak = 0;
        var cursor = today;

        while (uniqueDates.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    private async Task<List<ProgressSnapshot>> LoadProgressSnapshotsAsync(string maNd)
    {
        return await _context.TienDoHocTaps
            .Where(t => t.MaNd == maNd)
            .Select(t => new ProgressSnapshot(
                t.MaBai,
                t.TrangThai,
                t.PhanTramHoanThanh ?? 0,
                t.ThoiGianHocPhut ?? 0,
                t.NgayCapNhat,
                t.NgayHoanThanh
            ))
            .ToListAsync();
    }

    private static bool IsLessonCompleted(ProgressSnapshot snapshot)
    {
        if (snapshot is null)
        {
            return false;
        }

        if (snapshot.CompletionPercent >= 100)
        {
            return true;
        }

        return string.Equals(snapshot.Status, "Hoàn thành", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(snapshot.Status, "Hoan thanh", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(snapshot.Status, "Completed", StringComparison.OrdinalIgnoreCase);
    }

    private static int CalculateScore(int? score, int? maxScore, decimal? percentage)
    {
        if (percentage.HasValue)
        {
            var value = (int)Math.Round(percentage.Value);
            return Math.Clamp(value, 0, 100);
        }

        if (score.HasValue && maxScore.HasValue && maxScore.Value > 0)
        {
            var value = (int)Math.Round(score.Value * 100d / maxScore.Value);
            return Math.Clamp(value, 0, 100);
        }

        if (score.HasValue)
        {
            return Math.Clamp(score.Value, 0, 100);
        }

        return 0;
    }

    private static string NormalizeStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "Pending";
        }

        return status.Trim().ToLowerInvariant() switch
        {
            "hoàn thành" or "hoan thanh" or "completed" => "Completed",
            "đang học" or "dang hoc" or "đang học tập" or "dang hoc tap" or "inprogress" or "in-progress" => "InProgress",
            "đã mở khóa" or "da mo khoa" or "đã mở khoá" or "da mo khoá" => "InProgress",
            _ => "Pending"
        };
    }

    private static int ToPercent(int completed, int total)
    {
        if (total <= 0)
        {
            return 0;
        }

        var ratio = completed * 100d / total;
        return Math.Clamp((int)Math.Round(ratio), 0, 100);
    }

    private static string ToIso8601(DateTime value)
    {
        return value.ToUniversalTime().ToString("o");
    }

    [HttpGet("summary")]
    [HttpGet("summary/{maNd}")]
    public async Task<IActionResult> GetSummary(string? maNd = null)
    {
        var (target, error) = ResolveTargetUser(maNd);
        if (error != null)
        {
            return error;
        }

        var snapshots = await LoadProgressSnapshotsAsync(target);
        var completedLessonIds = snapshots
            .Where(IsLessonCompleted)
            .Select(p => p.LessonId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .ToList();

        var studyMinutes = snapshots.Sum(p => p.StudyMinutes);
        var streakDays = CalculateStreak(snapshots
            .Where(p => p.UpdatedAt.HasValue)
            .Select(p => p.UpdatedAt!.Value));

        var predictedScore = await _context.DanhGiaAis
            .Where(d => d.MaNd == target)
            .OrderByDescending(d => d.NgayDanhGia ?? DateTime.MinValue)
            .Select(d => d.DuDoanDiem)
            .FirstOrDefaultAsync() ?? 0;

        var totalLessons = await _context.BaiHocs.CountAsync();

        var summary = new DashboardSummaryVM
        {
            StreakDays = streakDays,
            PredictedScore = (int)Math.Round(predictedScore),
            CompletedLessons = completedLessonIds.Count,
            TotalLessons = totalLessons,
            StudyTimeHours = Math.Round(studyMinutes / 60d, 1)
        };

        return Ok(summary);
    }

    [AllowAnonymous]
    [HttpGet("admin-overview")]
    public async Task<IActionResult> GetAdminOverview()
    {
        var totalUsers = await _context.NguoiDungs.CountAsync();
        var listeningLessons = await _context.BaiNghes.CountAsync();
        var readingLessons = await _context.BaiDocs.CountAsync();
        var writingLessons = await _context.BaiViets.CountAsync();
        var totalLessons = listeningLessons + readingLessons + writingLessons;

        var overview = new DashboardAdminOverviewVM
        {
            TotalUsers = totalUsers,
            TotalLessons = totalLessons,
            ListeningLessons = listeningLessons,
            ReadingLessons = readingLessons,
            WritingLessons = writingLessons,
            LastUpdated = DateTime.UtcNow
        };

        return Ok(overview);
    }

    [HttpGet("progress")]
    [HttpGet("progress/{maNd}")]
    public async Task<IActionResult> GetProgress(string? maNd = null)
    {
        var (target, error) = ResolveTargetUser(maNd);
        if (error != null)
        {
            return error;
        }

        var totalListening = await _context.BaiNghes.CountAsync();
        var completedListening = await _context.KetQuaBaiNghes
            .Where(k => k.MaNd == target && k.MaBaiNghe != null)
            .Select(k => k.MaBaiNghe!)
            .Distinct()
            .CountAsync();

        var totalReading = await _context.BaiDocs.CountAsync();
        var completedReading = await _context.KetQuaBaiDocs
            .Where(k => k.MaNd == target && k.MaBaiDoc != null)
            .Select(k => k.MaBaiDoc!)
            .Distinct()
            .CountAsync();

        var totalWriting = await _context.BaiViets.CountAsync();
        var completedWriting = await _context.BaiVietHocViens
            .Where(b => b.MaNd == target && b.MaBaiViet != null)
            .Select(b => b.MaBaiViet!)
            .Distinct()
            .CountAsync();

        var listeningPercent = ToPercent(completedListening, totalListening);
        var readingPercent = ToPercent(completedReading, totalReading);
        var writingPercent = ToPercent(completedWriting, totalWriting);

        var latestAssessment = await _context.DanhGiaAis
            .Where(d => d.MaNd == target)
            .OrderByDescending(d => d.NgayDanhGia ?? DateTime.MinValue)
            .FirstOrDefaultAsync();

        var speakingPercent = latestAssessment?.XepHangPhanTram.HasValue == true
            ? Math.Clamp((int)Math.Round(latestAssessment.XepHangPhanTram!.Value), 0, 100)
            : (new[] { listeningPercent, readingPercent, writingPercent }.Where(v => v > 0).DefaultIfEmpty(0).Sum() /
               Math.Max(1, new[] { listeningPercent, readingPercent, writingPercent }.Count(v => v > 0)));

        var progress = new DashboardProgressVM
        {
            Listening = listeningPercent,
            Reading = readingPercent,
            Writing = writingPercent,
            Speaking = speakingPercent
        };

        return Ok(progress);
    }

    [HttpGet("today")]
    [HttpGet("today/{maNd}")]
    public async Task<IActionResult> GetToday(string? maNd = null)
    {
        var (target, error) = ResolveTargetUser(maNd);
        if (error != null)
        {
            return error;
        }

        var snapshots = await LoadProgressSnapshotsAsync(target);
        var progressByLesson = snapshots
            .Where(s => !string.IsNullOrWhiteSpace(s.LessonId))
            .GroupBy(s => s.LessonId!)
            .ToDictionary(
                g => g.Key,
                g => g
                    .OrderByDescending(s => s.UpdatedAt ?? DateTime.MinValue)
                    .First()
            );

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var eightAm = new TimeOnly(8, 0);

        var schedules = await _context.LichHocTaps
            .Where(l => l.MaNd == target && l.NgayHoc == today)
            .OrderBy(l => l.ThuTuNgay ?? 0)
            .ThenBy(l => l.MaLich)
            .Select(l => new
            {
                l.MaLich,
                l.MaBai,
                l.TieuDe,
                l.MoTa,
                l.TrangThai,
                l.LoaiNoiDung,
                l.NgayHoc
            })
            .ToListAsync();

        var plans = schedules.Select(item =>
        {
            progressByLesson.TryGetValue(item.MaBai ?? string.Empty, out var snapshot);
            var start = item.NgayHoc.ToDateTime(eightAm);
            var end = start.AddMinutes(60);
            var progressPercent = snapshot?.CompletionPercent ?? 0;

            return new DashboardPlanItemVM
            {
                PlanId = item.MaLich,
                LessonId = item.MaBai,
                Title = string.IsNullOrWhiteSpace(item.TieuDe) ? item.MaBai ?? "Bài học" : item.TieuDe,
                Description = item.MoTa,
                Status = NormalizeStatus(item.TrangThai),
                StartTime = ToIso8601(start),
                EndTime = ToIso8601(end),
                ContentType = item.LoaiNoiDung,
                ProgressPercent = Math.Clamp(progressPercent, 0, 100)
            };
        }).ToList();

        return Ok(plans);
    }

    [HttpGet("recent-results")]
    [HttpGet("recent-results/{maNd}")]
    public async Task<IActionResult> GetRecentResults(string? maNd = null)
    {
        var (target, error) = ResolveTargetUser(maNd);
        if (error != null)
        {
            return error;
        }

        var readingResults = await _context.KetQuaBaiDocs
            .Where(k => k.MaNd == target && k.NgayNop.HasValue)
            .Include(k => k.MaBaiDocNavigation)
            .OrderByDescending(k => k.NgayNop)
            .Take(10)
            .Select(k => new DashboardRecentResultVM
            {
                Exercise = k.MaBaiDocNavigation != null && !string.IsNullOrWhiteSpace(k.MaBaiDocNavigation.TieuDe)
                    ? k.MaBaiDocNavigation.TieuDe
                    : k.MaBaiDoc ?? "Bài đọc",
                Score = CalculateScore(k.Diem, k.DiemToiDa, k.PhanTram),
                CompletedAt = k.NgayNop!.Value
            })
            .ToListAsync();

        var listeningResults = await _context.KetQuaBaiNghes
            .Where(k => k.MaNd == target && k.NgayNop.HasValue)
            .Include(k => k.MaBaiNgheNavigation)
            .OrderByDescending(k => k.NgayNop)
            .Take(10)
            .Select(k => new DashboardRecentResultVM
            {
                Exercise = k.MaBaiNgheNavigation != null && !string.IsNullOrWhiteSpace(k.MaBaiNgheNavigation.TieuDe)
                    ? k.MaBaiNgheNavigation.TieuDe!
                    : k.MaBaiNghe ?? "Bài nghe",
                Score = CalculateScore(k.Diem, k.DiemToiDa, k.PhanTram),
                CompletedAt = k.NgayNop!.Value
            })
            .ToListAsync();

        var combined = readingResults
            .Concat(listeningResults)
            .OrderByDescending(r => r.CompletedAt)
            .Take(8)
            .Select(r => new { exercise = r.Exercise, score = r.Score })
            .ToList();

        return Ok(combined);
    }

    [HttpGet("badges")]
    [HttpGet("badges/{maNd}")]
    public async Task<IActionResult> GetBadges(string? maNd = null)
    {
        var (target, error) = ResolveTargetUser(maNd);
        if (error != null)
        {
            return error;
        }

        var snapshots = await LoadProgressSnapshotsAsync(target);
        var completedItems = snapshots.Where(IsLessonCompleted).ToList();
        var completedLessonCount = completedItems
            .Select(p => p.LessonId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .Count();

        var studyMinutes = snapshots.Sum(s => s.StudyMinutes);
        var streakDays = CalculateStreak(snapshots
            .Where(s => s.UpdatedAt.HasValue)
            .Select(s => s.UpdatedAt!.Value));

        var latestAssessment = await _context.DanhGiaAis
            .Where(d => d.MaNd == target)
            .OrderByDescending(d => d.NgayDanhGia ?? DateTime.MinValue)
            .FirstOrDefaultAsync();

        var badges = new List<DashboardBadgeVM>();

        var firstCompletionAt = completedItems
            .Select(s => s.CompletedAt ?? s.UpdatedAt)
            .Where(dt => dt.HasValue)
            .OrderBy(dt => dt)
            .FirstOrDefault()?.ToUniversalTime() ?? DateTime.UtcNow;

        if (completedLessonCount >= 1)
        {
            badges.Add(new DashboardBadgeVM
            {
                BadgeId = "lesson-first",
                BadgeName = "Bài học đầu tiên",
                Description = "Hoàn thành bài học đầu tiên",
                AwardedAt = firstCompletionAt
            });
        }

        if (completedLessonCount >= 5)
        {
            badges.Add(new DashboardBadgeVM
            {
                BadgeId = "lesson-five",
                BadgeName = "Chinh phục 5 bài học",
                Description = "Hoàn thành ít nhất 5 bài học",
                AwardedAt = firstCompletionAt
            });
        }

        if (streakDays >= 3)
        {
            badges.Add(new DashboardBadgeVM
            {
                BadgeId = "streak-3",
                BadgeName = "Chuỗi 3 ngày",
                Description = "Duy trì học liên tục 3 ngày",
                AwardedAt = DateTime.UtcNow
            });
        }

        if (studyMinutes >= 600)
        {
            badges.Add(new DashboardBadgeVM
            {
                BadgeId = "study-10h",
                BadgeName = "10 giờ học tập",
                Description = "Tích lũy ít nhất 10 giờ học",
                AwardedAt = DateTime.UtcNow
            });
        }

        if (latestAssessment?.DuDoanDiem.HasValue == true && latestAssessment.DuDoanDiem.Value >= 700)
        {
            badges.Add(new DashboardBadgeVM
            {
                BadgeId = "score-700",
                BadgeName = "Mục tiêu 700+",
                Description = "Điểm dự đoán đạt 700 trở lên",
                AwardedAt = (latestAssessment.NgayDanhGia ?? DateTime.UtcNow).ToUniversalTime()
            });
        }

        return Ok(badges);
    }
}
