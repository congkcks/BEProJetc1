using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class BaiVietHocVien
{
    public int MaBaiVietHv { get; set; }

    public string? MaNd { get; set; }

    public string? MaBaiViet { get; set; }

    public string NoiDung { get; set; } = null!;

    public int? SoTu { get; set; }

    public int? Diem { get; set; }

    public string? NhanXet { get; set; }

    public DateTime? NgayNop { get; set; }

    public virtual BaiViet? MaBaiVietNavigation { get; set; }

    public virtual NguoiDung? MaNdNavigation { get; set; }
}
