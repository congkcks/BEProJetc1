using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class BaiViet
{
    public string MaBaiViet { get; set; } = null!;

    public string? MaBai { get; set; }

    public string TieuDe { get; set; } = null!;

    public string DeBai { get; set; } = null!;

    public int? SoTuToiThieu { get; set; }

    public int? SoTuToiDa { get; set; }

    public string? BaiMau { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<BaiVietHocVien> BaiVietHocViens { get; set; } = new List<BaiVietHocVien>();

    public virtual BaiHoc? MaBaiNavigation { get; set; }
}
