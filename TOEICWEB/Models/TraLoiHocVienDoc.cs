using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOEICWEB.Models;

public partial class TraLoiHocVienDoc
{
    [Key]
    public int MaTraLoi { get; set; }

    public int? MaKetQua { get; set; }

    public string? MaCauHoi { get; set; }

    public int? MaDapAnChon { get; set; }

    public bool? DungSai { get; set; }

    public DateTime? NgayTao { get; set; }

    // CỘT ma_nd TRONG DB
    [Column("ma_nd")]
    public string? MaNd { get; set; }

    // NAVIGATION
    public virtual CauHoiDoc? MaCauHoiNavigation { get; set; }
    public virtual DapAnDoc? MaDapAnChonNavigation { get; set; }
    public virtual KetQuaBaiDoc? MaKetQuaNavigation { get; set; }

    // FK RÕ RÀNG
    [ForeignKey("MaNd")]
    public virtual NguoiDung? MaNdNavigation { get; set; }
}