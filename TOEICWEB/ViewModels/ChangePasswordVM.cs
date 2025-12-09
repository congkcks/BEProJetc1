using System.Text.Json.Serialization;

namespace TOEICWEB.ViewModels;

public class ChangePasswordVM
{
    [JsonPropertyName("matKhauHienTai")]
    public string? MatKhauHienTai { get; set; }

    [JsonPropertyName("matKhauCu")]
    public string? MatKhauCu { get; set; }

    [JsonPropertyName("currentPassword")]
    public string? CurrentPassword { get; set; }

    [JsonPropertyName("matKhauMoi")]
    public string? MatKhauMoi { get; set; }

    [JsonPropertyName("newPassword")]
    public string? NewPassword { get; set; }

    [JsonPropertyName("xacNhanMatKhauMoi")]
    public string? XacNhanMatKhauMoi { get; set; }

    [JsonPropertyName("confirmPassword")]
    public string? ConfirmPassword { get; set; }
}
