using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class CauHoiDoc
{
    public string MaCauHoi { get; set; } = null!;

    public string? MaBaiDoc { get; set; }

    public string NoiDungCauHoi { get; set; } = null!;

    public int? Diem { get; set; }

    public string? GiaiThich { get; set; }

    public int? ThuTuHienThi { get; set; }

    public virtual ICollection<DapAnDoc> DapAnDocs { get; set; } = new List<DapAnDoc>();

    public virtual BaiDoc? MaBaiDocNavigation { get; set; }

    public virtual ICollection<TraLoiHocVienDoc> TraLoiHocVienDocs { get; set; } = new List<TraLoiHocVienDoc>();
}
