using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class DanhGiaAi
{
    public string MaDgai { get; set; } = null!;

    public string MaNd { get; set; } = null!;

    public string MaBkt { get; set; } = null!;

    public int? TongCauHoi { get; set; }

    public int? SoCauDung { get; set; }

    public double? DiemSo { get; set; }

    public double? XepHangPhanTram { get; set; }

    public string? DiemManh { get; set; }

    public string? DiemYeu { get; set; }

    public string? GoiYCaiThien { get; set; }

    public double? DuDoanDiem { get; set; }

    public DateTime? NgayDanhGia { get; set; }

    public virtual KtTienDo MaBktNavigation { get; set; } = null!;

    public virtual NguoiDung MaNdNavigation { get; set; } = null!;
}
