using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class VTongQuanTienDo
{
    public string? MaNd { get; set; }

    public string? HoTen { get; set; }

    public string? TenLoTrinh { get; set; }

    public long? SoBaiHoanThanh { get; set; }

    public long? TongSoBai { get; set; }

    public decimal? PhanTramHoanThanh { get; set; }

    public long? TongThoiGianHoc { get; set; }
}
