using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class VideoBaiHoc
{
    public string MaVideo { get; set; } = null!;

    public string? MaBai { get; set; }

    public string TieuDeVideo { get; set; } = null!;

    public string DuongDanVideo { get; set; } = null!;

    public int? ThoiLuongGiay { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual BaiHoc? MaBaiNavigation { get; set; }
}
