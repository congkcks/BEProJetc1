using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class BaiDoc
{
    public string MaBaiDoc { get; set; } = null!;

    public string? MaBai { get; set; }

    public string TieuDe { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    public string? DuongDanFileTxt { get; set; }

    public string? DoKho { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<CauHoiDoc> CauHoiDocs { get; set; } = new List<CauHoiDoc>();

    public virtual ICollection<KetQuaBaiDoc> KetQuaBaiDocs { get; set; } = new List<KetQuaBaiDoc>();

    public virtual BaiHoc? MaBaiNavigation { get; set; }
}
