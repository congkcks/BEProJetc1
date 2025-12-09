namespace YourApp.ViewModels.LoTrinh
{
    public class TienDoHocTapDto
    {
        public int MaTienDo { get; set; }
        public string MaBai { get; set; } = string.Empty;
        public string TrangThai { get; set; } = "Chưa bắt đầu";
        public int PhanTramHoanThanh { get; set; }
        public int ThoiGianHocPhut { get; set; }
        public DateTime? NgayHoanThanh { get; set; }
        public DateTime NgayCapNhat { get; set; }
    }
}