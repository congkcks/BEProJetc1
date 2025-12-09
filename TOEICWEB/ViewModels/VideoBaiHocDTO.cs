namespace TOEICWEB.ViewModels
{
    public class VideoBaiHocDTO
    {
        public string MaVideo { get; set; } = string.Empty;
        public string TieuDeVideo { get; set; } = string.Empty;
        public string DuongDanVideo { get; set; } = string.Empty;
        public int? ThoiLuongGiay { get; set; }
        public DateTime? NgayTao { get; set; }
    }
}
