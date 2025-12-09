using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOEICWEB.Data;
using TOEICWEB.ViewModels.Dashboard;

namespace ToeicWeb.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ThongKeController : ControllerBase
{
    private readonly SupabaseDbContext _context;

    public ThongKeController(SupabaseDbContext context)
    {
        _context = context;
    }

    private async Task<(int totalUsers, int listening, int reading, int writing)> LoadCoreMetricsAsync()
    {
        var totalUsersTask = _context.NguoiDungs.CountAsync();
        var listeningTask = _context.BaiNghes.CountAsync();
        var readingTask = _context.BaiDocs.CountAsync();
        var writingTask = _context.BaiViets.CountAsync();

        await Task.WhenAll(totalUsersTask, listeningTask, readingTask, writingTask);

        return (await totalUsersTask, await listeningTask, await readingTask, await writingTask);
    }

    [HttpGet("admin-overview")]
    public async Task<IActionResult> GetAdminOverview()
    {
        var (totalUsers, listening, reading, writing) = await LoadCoreMetricsAsync();
        var overview = new DashboardAdminOverviewVM
        {
            TotalUsers = totalUsers,
            ListeningLessons = listening,
            ReadingLessons = reading,
            WritingLessons = writing,
            TotalLessons = listening + reading + writing,
            LastUpdated = DateTime.UtcNow
        };

        return Ok(overview);
    }

    [HttpGet("users/count")]
    public async Task<IActionResult> GetUserCount()
    {
        var totalUsers = await _context.NguoiDungs.CountAsync();
        var newToday = await _context.NguoiDungs
            .CountAsync(u => u.NgayDangKy.HasValue && u.NgayDangKy.Value.Date == DateTime.UtcNow.Date);

        return Ok(new { count = totalUsers, today = newToday });
    }

    [HttpGet("lessons/count")]
    public async Task<IActionResult> GetLessonCount()
    {
        var (_, listening, reading, writing) = await LoadCoreMetricsAsync();
        var total = listening + reading + writing;

        return Ok(new
        {
            totalLessons = total,
            listeningLessons = listening,
            readingLessons = reading,
            writingLessons = writing,
            lastUpdated = DateTime.UtcNow
        });
    }
}
