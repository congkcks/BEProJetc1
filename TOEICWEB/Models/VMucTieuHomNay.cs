using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class VMucTieuHomNay
{
    public string? MaNd { get; set; }

    public string? HoTen { get; set; }

    public string? LoaiMucTieu { get; set; }

    public int? GiaTriMucTieu { get; set; }

    public int? GiaTriHienTai { get; set; }

    public string? DonVi { get; set; }

    public decimal? PhanTramHoanThanh { get; set; }

    public bool? DaHoanThanh { get; set; }
}
