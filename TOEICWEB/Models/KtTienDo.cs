using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class KtTienDo
{
    public string MaBkt { get; set; } = null!;

    public string MaNd { get; set; } = null!;

    public string TenBaiKt { get; set; } = null!;

    public string? FileDePdf { get; set; }

    public string? DapAnPdf { get; set; }

    public string? TrangThai { get; set; }

    public double? DiemSo { get; set; }

    public DateTime? NgayLam { get; set; }

    public virtual ICollection<DanhGiaAi> DanhGiaAis { get; set; } = new List<DanhGiaAi>();

    public virtual NguoiDung MaNdNavigation { get; set; } = null!;
}
