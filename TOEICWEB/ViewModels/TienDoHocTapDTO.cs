public class TienDoHocTapDTO
{
    public int MaTienDo { get; set; }
    public string MaBai { get; set; } = string.Empty;
    public string TrangThai { get; set; } = string.Empty;
    public DateTime? NgayHoanThanh { get; set; }
    public DateTime? NgayCapNhat { get; set; }
    public int ThoiGianHocPhut { get; set; }
    public int PhanTramHoanThanh { get; set; }
    public string TenBai { get; internal set; } = string.Empty;
    public string? MaLoTrinh { get; internal set; }
}