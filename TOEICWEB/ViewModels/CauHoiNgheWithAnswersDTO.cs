public class CauHoiNgheWithAnswersDTO
{
    public string MaCauHoi { get; set; } = string.Empty;
    public string NoiDungCauHoi { get; set; } = string.Empty;
    public string GiaiThich { get; set; } = string.Empty;
    public int Diem { get; set; }
    public int ThuTuHienThi { get; set; }
    public List<DapAnNgheDTO> DapAns { get; set; } = new List<DapAnNgheDTO>();
}