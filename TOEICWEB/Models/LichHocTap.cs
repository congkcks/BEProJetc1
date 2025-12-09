using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TOEICWEB.ViewModels.LoTrinh;
using YourApp.ViewModels.LoTrinh;

namespace TOEICWEB.Models
{
    [Table("_lich_hoc_tap")]
    public partial class LichHocTap
    {
        [Key]
        [Column("_ma_lich")]
        [StringLength(50)]
        public string MaLich { get; set; } = null!;

        [Column("_ma_nd")]
        [StringLength(30)]
        public string? MaNd { get; set; }

        [Column("_ma_lo_trinh")]
        [StringLength(30)]
        public string? MaLoTrinh { get; set; }

        [Column("_ngay_hoc")]
        public DateOnly NgayHoc { get; set; }

        [Column("_thu_tu_ngay")]
        public int? ThuTuNgay { get; set; }

        [Column("_tuan_hoc")]
        public int? TuanHoc { get; set; }

        [Column("_tieu_de")]
        [StringLength(200)]
        public string TieuDe { get; set; } = null!;

        [Column("_mo_ta")]
        [StringLength(500)]
        public string? MoTa { get; set; }

        [Column("_loai_noi_dung")]
        [StringLength(20)]
        public string LoaiNoiDung { get; set; } = ContentType.LyThuyet;

        [Column("_ma_bai")]
        [StringLength(30)]
        public string? MaBai { get; set; }

        [Column("_trang_thai")]
        [StringLength(20)]
        public string TrangThai { get; set; } = Status.ChuaMoKhoa;

        [Column("_da_hoan_thanh")]
        public bool? DaHoanThanh { get; set; } = false;

        [Column("_ngay_hoan_thanh")]
        public DateTime? NgayHoanThanh { get; set; }

        // Navigation
        public virtual BaiHoc? MaBaiNavigation { get; set; }
        public virtual LoTrinhCoSan? MaLoTrinhNavigation { get; set; }
        public virtual NguoiDung? MaNdNavigation { get; set; }


        // ================================
        // NESTED: HẰNG SỐ TRẠNG THÁI
        // ================================
        public static class Status
        {
            public const string ChuaMoKhoa = "Chưa mở khóa";
            public const string DaMoKhoa = "Đã mở khóa";
            public const string DangHoc = "Đang học";
            public const string HoanThanh = "Hoàn thành";
        }

        // ================================
        // NESTED: HẰNG SỐ LOẠI NỘI DUNG
        // ================================
        public static class ContentType
        {
            public const string LyThuyet = "Lý thuyết";
            public const string Nghe = "Nghe";
            public const string Doc = "Đọc";
        }


        // ================================
        // DTO → ENTITY CONVERSION
        // ================================
        public static implicit operator LichHocTap(LichHocTapDto dto)
        {
            return new LichHocTap
            {
                MaLich = dto.MaLich,
                MaNd = dto.MaNd,
                MaLoTrinh = dto.MaLoTrinh,
                MaBai = dto.MaBai,
                TieuDe = dto.TieuDe,
                MoTa = dto.MoTa,
                LoaiNoiDung = dto.LoaiNoiDung,
                NgayHoc = DateOnly.FromDateTime(dto.NgayHoc),
                TrangThai = dto.TrangThai,
                DaHoanThanh = dto.DaHoanThanh,
                ThuTuNgay = dto.ThuTuNgay,
                TuanHoc = dto.TuanHoc
            };
        }
    }
}