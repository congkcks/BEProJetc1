public class CauHoiDocDTO
{
    public string MaCauHoi { get; set; } = string.Empty;             // ma_cau_hoi
    public string NoiDungCauHoi { get; set; } = string.Empty;        // noi_dung_cau_hoi
    public string GiaiThich { get; set; } = string.Empty;            // giai_thich
    public int Diem { get; set; }                    // diem (default 1)
    public int? ThuTuHienThi { get; set; }           // thu_tu_hien_thi
}