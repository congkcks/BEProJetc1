namespace YourApp.ViewModels.LoTrinh
{
    public class LoTrinhCoSanDto
    {
        public string MaLoTrinh { get; set; } = string.Empty;
        public string TenLoTrinh { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string? ThoiGianDuKien { get; set; }
        public string? CapDo { get; set; }
        public string? LoaiLoTrinh { get; set; }
        public int? MucTieuDiem { get; set; }
        public int TongSoBai { get; set; }
        public DateTime NgayTao { get; set; }

        // NEW: expose the two new text fields to clients
        public string? KyNangTrongTam { get; set; }
        public string? ChuDeBaiHoc { get; set; }
    }
}