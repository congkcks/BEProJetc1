namespace TOEICWEB.ViewModels
{
    public class BaiHocDTO
    {
        public string MaBai { get; set; } = string.Empty;
        public string MaLoTrinh { get; set; } = string.Empty;
        public string TenBai { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public int ThoiLuongPhut { get; set; }
        public int SoThuTu { get; set; }
        public DateTime? NgayTao { get; set; }
        public List<VideoBaiHocDTO> Videos { get; set; } = new List<VideoBaiHocDTO>();

        public List<BaiNgheDTO> BaiNghes { get; set; } = new();
        public List<BaiDocDTO> BaiDocs { get; set; } = new();



    }
}