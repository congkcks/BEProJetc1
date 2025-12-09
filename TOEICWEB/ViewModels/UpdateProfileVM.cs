using System.Text.Json.Serialization;

namespace TOEICWEB.ViewModels;

public class UpdateProfileVM
{
    [JsonPropertyName("maNguoiDung")]
    public string? MaNguoiDung { get; set; }

    [JsonPropertyName("maND")]
    public string? MaNd { get; set; }

    [JsonPropertyName("ma_nd")]
    public string? MaNdSnake { get; set; }

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("userId")]
    public string? UserId { get; set; }

    [JsonPropertyName("hoTen")]
    public string? HoTen { get; set; }

    [JsonPropertyName("fullName")]
    public string? FullName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("soDienThoai")]
    public string? SoDienThoai { get; set; }

    [JsonPropertyName("phone")]
    public string? Phone { get; set; }

    [JsonPropertyName("anhDaiDien")]
    public string? AnhDaiDien { get; set; }

    [JsonPropertyName("avatarUrl")]
    public string? AvatarUrl { get; set; }

    [JsonPropertyName("avatar")]
    public string? Avatar { get; set; }
}
