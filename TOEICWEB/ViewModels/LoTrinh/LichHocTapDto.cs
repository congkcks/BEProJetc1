namespace YourApp.ViewModels.LoTrinh
{
    public class LichHocTapDto
    {
        public string MaLich { get; set; } = string.Empty;
        public string MaLoTrinh { get; set; } = string.Empty;
        public string? MaBai { get; set; }
        public string TieuDe { get; set; } = string.Empty;
        public string? MoTa { get; set; }
        public string LoaiNoiDung { get; set; } = string.Empty;
        public DateTime NgayHoc { get; set; }
        public string TrangThai { get; set; } = "Chưa mở khóa";
        public bool DaHoanThanh { get; set; }
        public int? ThuTuNgay { get; set; }
        public int? TuanHoc { get; set; }
        public string? MaNd { get; set; }

    }
}