namespace YourApp.ViewModels.LoTrinh
{
    public class LogHoatDongDto
    {
        public int MaLog { get; set; }
        public string LoaiHoatDong { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public DateTime ThoiGian { get; set; }
        public object? DuLieuCu { get; set; }
        public object? DuLieuMoi { get; set; }
    }
}