using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class LoTrinhCoSan
{
    public string MaLoTrinh { get; set; } = null!;

    public string TenLoTrinh { get; set; } = null!;

    public string? MoTa { get; set; }

    public int? MucTieuDiem { get; set; }

    public int? TongSoBai { get; set; }

    public string? ThoiGianDuKien { get; set; }

    public string? CapDo { get; set; }

    public string? LoaiLoTrinh { get; set; }

    public DateTime? NgayTao { get; set; }

    public string? KyNangTrongTam { get; set; }

    public string? ChuDeBaiHoc { get; set; }

    public virtual ICollection<BaiHoc> BaiHocs { get; set; } = new List<BaiHoc>();

    public virtual ICollection<DangKyLoTrinh> DangKyLoTrinhs { get; set; } = new List<DangKyLoTrinh>();

    public virtual ICollection<LichHocTap> LichHocTaps { get; set; } = new List<LichHocTap>();
}
