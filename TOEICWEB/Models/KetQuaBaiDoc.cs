using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class KetQuaBaiDoc
{
    public int MaKetQua { get; set; }

    public string? MaNd { get; set; }

    public string? MaBaiDoc { get; set; }

    public int? Diem { get; set; }

    public int? DiemToiDa { get; set; }

    public decimal? PhanTram { get; set; }

    public int? ThoiGianLamGiay { get; set; }

    public int? LanLamThu { get; set; }

    public DateTime? NgayNop { get; set; }

    public virtual BaiDoc? MaBaiDocNavigation { get; set; }

    public virtual NguoiDung? MaNdNavigation { get; set; }

    public virtual ICollection<TraLoiHocVienDoc> TraLoiHocVienDocs { get; set; } = new List<TraLoiHocVienDoc>();
}
