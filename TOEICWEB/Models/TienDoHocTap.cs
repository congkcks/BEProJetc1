using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class TienDoHocTap
{
    public int MaTienDo { get; set; }

    public string? MaNd { get; set; }

    public string? MaBai { get; set; }

    public string? TrangThai { get; set; }

    public int? PhanTramHoanThanh { get; set; }

    public int? ThoiGianHocPhut { get; set; }

    public DateTime? NgayHoanThanh { get; set; }

    public DateTime? NgayCapNhat { get; set; }

    public virtual BaiHoc? MaBaiNavigation { get; set; }

    public virtual NguoiDung? MaNdNavigation { get; set; }
}
