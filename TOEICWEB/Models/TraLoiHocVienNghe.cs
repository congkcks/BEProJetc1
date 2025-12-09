using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOEICWEB.Models
{
    [Table("tra_loi_hoc_vien_nghe")]  // Tên bảng trong DB
    public partial class TraLoiHocVienNghe
    {
        [Key]
        [Column("ma_tra_loi")]
        public int MaTraLoi { get; set; }

        [Column("ma_ket_qua")]
        public int? MaKetQua { get; set; }

        [Column("ma_cau_hoi")]
        public string? MaCauHoi { get; set; }

        [Column("ma_dap_an_chon")]
        public int? MaDapAnChon { get; set; }

        [Column("dung_sai")]
        public bool? DungSai { get; set; }

        [Column("ngay_tao")]
        public DateTime? NgayTao { get; set; }

        // Foreign Key: Mã người dùng
        [Column("ma_nd")]
        public string? MaNd { get; set; }

        // Navigation Properties
        public virtual CauHoiNghe? MaCauHoiNavigation { get; set; }
        public virtual DapAnNghe? MaDapAnChonNavigation { get; set; }
        public virtual KetQuaBaiNghe? MaKetQuaNavigation { get; set; }
        public virtual NguoiDung? MaNdNavigation { get; set; }
    }
}