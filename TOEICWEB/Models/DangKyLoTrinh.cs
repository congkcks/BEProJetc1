using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class DangKyLoTrinh
{
    public int MaDangKy { get; set; }

    public string? MaNd { get; set; }

    public string? MaLoTrinh { get; set; }

    public DateTime? NgayDangKy { get; set; }

    public string? TrangThai { get; set; }

    public virtual LoTrinhCoSan? MaLoTrinhNavigation { get; set; }

    public virtual NguoiDung? MaNdNavigation { get; set; }
}
