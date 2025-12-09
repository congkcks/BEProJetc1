using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class CauHoiNghe
{
    public string MaCauHoi { get; set; } = null!;

    public string? MaBaiNghe { get; set; }

    public string NoiDungCauHoi { get; set; } = null!;

    public int? Diem { get; set; }

    public string? GiaiThich { get; set; }

    public int? ThuTuHienThi { get; set; }

    public virtual ICollection<DapAnNghe> DapAnNghes { get; set; } = new List<DapAnNghe>();

    public virtual BaiNghe? MaBaiNgheNavigation { get; set; }

    public virtual ICollection<TraLoiHocVienNghe> TraLoiHocVienNghes { get; set; } = new List<TraLoiHocVienNghe>();
}
