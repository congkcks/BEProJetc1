using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOEICWEB.Models
{
    [Table("ket_qua_bai_nghe")]
    public partial class KetQuaBaiNghe
    {
        [Key]
        [Column("ma_ket_qua")]
        public int MaKetQua { get; set; }

        [Column("ma_nd")]
        public string? MaNd { get; set; }

        [Column("ma_bai_nghe")]
        public string? MaBaiNghe { get; set; }

        [Column("diem")]
        public int? Diem { get; set; }

        [Column("diem_toi_da")]
        public int? DiemToiDa { get; set; }

        [Column("phan_tram")]
        public decimal? PhanTram { get; set; }

        [Column("thoi_gian_lam_giay")]
        public int? ThoiGianLamGiay { get; set; }

        [Column("lan_lam_thu")]
        public int? LanLamThu { get; set; }

        [Column("ngay_nop")]
        public DateTime? NgayNop { get; set; }

        // Navigation Properties
        public virtual BaiNghe? MaBaiNgheNavigation { get; set; }
        public virtual NguoiDung? MaNdNavigation { get; set; }

        // One-to-many: 1 kết quả → nhiều câu trả lời
        public virtual ICollection<TraLoiHocVienNghe> TraLoiHocVienNghes { get; set; } = new List<TraLoiHocVienNghe>();
    }
}