using System;

namespace TOEICWEB.ViewModels.Admin;

public class AdminUserVM
{
    public string MaNd { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string? MatKhau { get; set; }

    public string? VaiTro { get; set; }

    public string? SoDienThoai { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public string? AnhDaiDien { get; set; }
}
