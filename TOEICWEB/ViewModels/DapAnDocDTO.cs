public class DapAnDocDTO
{
    public int MaDapAn { get; set; }                 // ma_dap_an
    public string MaCauHoi { get; set; } = string.Empty;             // ma_cau_hoi
    public string NhanDapAn { get; set; } = string.Empty;            // nhan_dap_an (A, B, C, D)
    public string NoiDungDapAn { get; set; } = string.Empty;         // noi_dung_dap_an
    public int? ThuTuHienThi { get; set; }           // thu_tu_hien_thi
    public bool LaDapAnDung { get; set; }            // la_dap_an_dung
}