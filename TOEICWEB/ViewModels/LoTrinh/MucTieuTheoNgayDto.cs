public class MucTieuTheoNgayDto
{
    public int MaMucTieu { get; set; }
    public string LoaiMucTieu { get; set; } = string.Empty;
    public int GiaTriMucTieu { get; set; }
    public int GiaTriHienTai { get; set; }
    public string DonVi { get; set; } = string.Empty;
    public bool DaHoanThanh { get; set; }
    public DateTime NgayMucTieu { get; set; }
    public string MaBai { get; set; } = string.Empty;
    public string TieuDe { get; set; } = string.Empty;
    public string MaLoTrinh { get; set; } = string.Empty;
}