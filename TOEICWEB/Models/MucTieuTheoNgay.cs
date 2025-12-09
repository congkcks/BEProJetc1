using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class MucTieuTheoNgay
{
    public int MaMucTieu { get; set; }

    public string? MaNd { get; set; }

    public DateOnly NgayMucTieu { get; set; }

    public string LoaiMucTieu { get; set; } = null!;

    public int GiaTriMucTieu { get; set; }

    public int? GiaTriHienTai { get; set; }

    public string? DonVi { get; set; }

    public bool? DaHoanThanh { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual NguoiDung? MaNdNavigation { get; set; }
}
