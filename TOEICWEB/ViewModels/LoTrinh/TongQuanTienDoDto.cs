namespace YourApp.ViewModels.LoTrinh
{
    public class TongQuanTienDoDto
    {
        public int TongSoBai { get; set; }
        public int SoBaiHoanThanh { get; set; }
        public int TongThoiGianHocPhut { get; set; }
        public double PhanTramHoanThanh => TongSoBai > 0
            ? Math.Round((double)SoBaiHoanThanh / TongSoBai * 100, 1)
            : 0;
        public int SoLoTrinhDangHoc { get; set; }
        public int SoMucTieuHomNay { get; set; }
        public int SoMucTieuHoanThanh { get; set; }
    }
}