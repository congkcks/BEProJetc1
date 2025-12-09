namespace TOEICWEB.ViewModels.Admin;

public class AdminUserCreateVM
{
    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? SoDienThoai { get; set; }

    public string? VaiTro { get; set; }
}

