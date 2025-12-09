using System.Text.Json.Serialization;

namespace TOEICWEB.ViewModels;

public class UpdateSelfVM
{
    // Identification (implicit from token, allow explicit for future checks)
    [JsonPropertyName("maNd")] public string? MaNd { get; set; }
    [JsonPropertyName("maNguoiDung")] public string? MaNguoiDung { get; set; }
    [JsonPropertyName("userId")] public string? UserId { get; set; }

    // Profile fields
    [JsonPropertyName("hoTen")] public string? HoTen { get; set; }
    [JsonPropertyName("fullName")] public string? FullName { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("email")] public string? Email { get; set; }
    [JsonPropertyName("soDienThoai")] public string? SoDienThoai { get; set; }
    [JsonPropertyName("phone")] public string? Phone { get; set; }
    [JsonPropertyName("anhDaiDien")] public string? AnhDaiDien { get; set; }
    [JsonPropertyName("avatarUrl")] public string? AvatarUrl { get; set; }
    [JsonPropertyName("avatar")] public string? Avatar { get; set; }

    // Password change fields (optional)
    [JsonPropertyName("matKhauHienTai")] public string? MatKhauHienTai { get; set; }
    [JsonPropertyName("matKhauCu")] public string? MatKhauCu { get; set; }
    [JsonPropertyName("currentPassword")] public string? CurrentPassword { get; set; }
    [JsonPropertyName("matKhauMoi")] public string? MatKhauMoi { get; set; }
    [JsonPropertyName("newPassword")] public string? NewPassword { get; set; }
    [JsonPropertyName("xacNhanMatKhauMoi")] public string? XacNhanMatKhauMoi { get; set; }
    [JsonPropertyName("confirmPassword")] public string? ConfirmPassword { get; set; }
}
