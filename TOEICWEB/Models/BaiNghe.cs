using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class BaiNghe
{
    public string MaBaiNghe { get; set; } = null!;

    public string? MaBai { get; set; }

    public string TieuDe { get; set; } = null!;

    public string DuongDanAudio { get; set; } = null!;

    public string? BanGhiAm { get; set; }

    public string? DoKho { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<CauHoiNghe> CauHoiNghes { get; set; } = new List<CauHoiNghe>();

    public virtual ICollection<KetQuaBaiNghe> KetQuaBaiNghes { get; set; } = new List<KetQuaBaiNghe>();

    public virtual BaiHoc? MaBaiNavigation { get; set; }
}
