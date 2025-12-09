using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TOEICWEB.Models;

public partial class NguoiDung
{


    public string MaNd { get; set; } = null!;  // để tự sinh trong backend

    public string HoTen { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string? AnhDaiDien { get; set; }

    public string? SoDienThoai { get; set; }

    public string? VaiTro { get; set; }
    public DateTime? NgayDangKy { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public virtual ICollection<BaiVietHocVien> BaiVietHocViens { get; set; } = new List<BaiVietHocVien>();

    public virtual ICollection<DangKyLoTrinh> DangKyLoTrinhs { get; set; } = new List<DangKyLoTrinh>();

    public virtual ICollection<DanhGiaAi> DanhGiaAis { get; set; } = new List<DanhGiaAi>();

    public virtual ICollection<KetQuaBaiDoc> KetQuaBaiDocs { get; set; } = new List<KetQuaBaiDoc>();

    public virtual ICollection<KetQuaBaiNghe> KetQuaBaiNghes { get; set; } = new List<KetQuaBaiNghe>();

    public virtual ICollection<KtTienDo> KtTienDos { get; set; } = new List<KtTienDo>();

    public virtual ICollection<LichHocTap> LichHocTaps { get; set; } = new List<LichHocTap>();

    public virtual ICollection<MucTieuTheoNgay> MucTieuTheoNgays { get; set; } = new List<MucTieuTheoNgay>();

    public virtual ICollection<TienDoHocTap> TienDoHocTaps { get; set; } = new List<TienDoHocTap>();
    public virtual ICollection<TraLoiHocVienDoc> TraLoiHocVienDocs { get; set; }
        = new List<TraLoiHocVienDoc>();

    // 🔹 Liên kết 1-nhiều: 1 người dùng có thể có nhiều kết quả bài nghe
    public virtual ICollection<TraLoiHocVienNghe> TraLoiHocVienNghes { get; set; }
        = new List<TraLoiHocVienNghe>();

}
