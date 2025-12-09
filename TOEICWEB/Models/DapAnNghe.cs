using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class DapAnNghe
{
    public int MaDapAn { get; set; }

    public string? MaCauHoi { get; set; }

    public string NoiDungDapAn { get; set; } = null!;

    public char? NhanDapAn { get; set; }

    public bool? LaDapAnDung { get; set; }

    public int? ThuTuHienThi { get; set; }

    public virtual CauHoiNghe? MaCauHoiNavigation { get; set; }

    public virtual ICollection<TraLoiHocVienNghe> TraLoiHocVienNghes { get; set; } = new List<TraLoiHocVienNghe>();
}
