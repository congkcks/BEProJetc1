namespace YourApp.ViewModels.LoTrinh
{
    public class DangKyLoTrinhDto
    {
        public int MaDangKy { get; set; }
        public string MaLoTrinh { get; set; } = string.Empty;
        public string TenLoTrinh { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? CapDo { get; set; }
        public int TongSoBai { get; set; }
        public DateTime NgayDangKy { get; set; }
        public string TrangThai { get; set; } = "Đang học";
    }
}