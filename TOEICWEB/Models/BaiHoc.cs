using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class BaiHoc
{
    public string MaBai { get; set; } = null!;

    public string? MaLoTrinh { get; set; }

    public string TenBai { get; set; } = null!;

    public int SoThuTu { get; set; }

    public string? MoTa { get; set; }

    public int? ThoiLuongPhut { get; set; }

    public DateTime? NgayTao { get; set; }

    public virtual ICollection<BaiDoc> BaiDocs { get; set; } = new List<BaiDoc>();

    public virtual ICollection<BaiNghe> BaiNghes { get; set; } = new List<BaiNghe>();

    public virtual ICollection<BaiViet> BaiViets { get; set; } = new List<BaiViet>();

    public virtual ICollection<LichHocTap> LichHocTaps { get; set; } = new List<LichHocTap>();

    public virtual LoTrinhCoSan? MaLoTrinhNavigation { get; set; }

    public virtual ICollection<TienDoHocTap> TienDoHocTaps { get; set; } = new List<TienDoHocTap>();

    public virtual ICollection<VideoBaiHoc> VideoBaiHocs { get; set; } = new List<VideoBaiHoc>();
}
