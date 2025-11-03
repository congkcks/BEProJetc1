using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TOEICWEB.Models;

namespace TOEICWEB.Data;

public partial class SupabaseDbContext : DbContext
{
    public SupabaseDbContext()
    {
    }

    public SupabaseDbContext(DbContextOptions<SupabaseDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiDoc> BaiDocs { get; set; }

    public virtual DbSet<BaiHoc> BaiHocs { get; set; }

    public virtual DbSet<BaiNghe> BaiNghes { get; set; }

    public virtual DbSet<BaiViet> BaiViets { get; set; }

    public virtual DbSet<BaiVietHocVien> BaiVietHocViens { get; set; }

    public virtual DbSet<CauHoiDoc> CauHoiDocs { get; set; }

    public virtual DbSet<CauHoiNghe> CauHoiNghes { get; set; }

    public virtual DbSet<DangKyLoTrinh> DangKyLoTrinhs { get; set; }

    public virtual DbSet<DanhGiaAi> DanhGiaAis { get; set; }

    public virtual DbSet<DapAnDoc> DapAnDocs { get; set; }

    public virtual DbSet<DapAnNghe> DapAnNghes { get; set; }

    public virtual DbSet<KetQuaBaiDoc> KetQuaBaiDocs { get; set; }

    public virtual DbSet<KetQuaBaiNghe> KetQuaBaiNghes { get; set; }

    public virtual DbSet<KtTienDo> KtTienDos { get; set; }

    public virtual DbSet<LichHocTap> LichHocTaps { get; set; }

    public virtual DbSet<LoTrinhCoSan> LoTrinhCoSans { get; set; }

    public virtual DbSet<LogHoatDong> LogHoatDongs { get; set; }

    public virtual DbSet<MucTieuTheoNgay> MucTieuTheoNgays { get; set; }

    public virtual DbSet<NguoiDung> NguoiDungs { get; set; }

    public virtual DbSet<TienDoHocTap> TienDoHocTaps { get; set; }

    public virtual DbSet<TraLoiHocVienDoc> TraLoiHocVienDocs { get; set; }

    public virtual DbSet<TraLoiHocVienNghe> TraLoiHocVienNghes { get; set; }

    public virtual DbSet<VLichHocHomNay> VLichHocHomNays { get; set; }

    public virtual DbSet<VMucTieuHomNay> VMucTieuHomNays { get; set; }

    public virtual DbSet<VTongQuanTienDo> VTongQuanTienDos { get; set; }

    public virtual DbSet<VideoBaiHoc> VideoBaiHocs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=DefaultConnection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("graphql", "pg_graphql")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<BaiDoc>(entity =>
        {
            entity.HasKey(e => e.MaBaiDoc).HasName("bai_doc_pkey");

            entity.ToTable("bai_doc");

            entity.Property(e => e.MaBaiDoc)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_doc");
            entity.Property(e => e.DoKho)
                .HasMaxLength(20)
                .HasColumnName("do_kho");
            entity.Property(e => e.DuongDanFileTxt)
                .HasMaxLength(500)
                .HasColumnName("duong_dan_file_txt");
            entity.Property(e => e.MaBai)
                .HasMaxLength(10)
                .HasColumnName("ma_bai");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.NoiDung).HasColumnName("noi_dung");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");

            entity.HasOne(d => d.MaBaiNavigation).WithMany(p => p.BaiDocs)
                .HasForeignKey(d => d.MaBai)
                .HasConstraintName("bai_doc_ma_bai_fkey");
        });

        modelBuilder.Entity<BaiHoc>(entity =>
        {
            entity.HasKey(e => e.MaBai).HasName("bai_hoc_pkey");

            entity.ToTable("bai_hoc");

            entity.HasIndex(e => e.MaLoTrinh, "idx_bai_hoc_lo_trinh");

            entity.Property(e => e.MaBai)
                .HasMaxLength(10)
                .HasColumnName("ma_bai");
            entity.Property(e => e.MaLoTrinh)
                .HasMaxLength(10)
                .HasColumnName("ma_lo_trinh");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.SoThuTu).HasColumnName("so_thu_tu");
            entity.Property(e => e.TenBai)
                .HasMaxLength(200)
                .HasColumnName("ten_bai");
            entity.Property(e => e.ThoiLuongPhut).HasColumnName("thoi_luong_phut");

            entity.HasOne(d => d.MaLoTrinhNavigation).WithMany(p => p.BaiHocs)
                .HasForeignKey(d => d.MaLoTrinh)
                .HasConstraintName("bai_hoc_ma_lo_trinh_fkey");
        });

        modelBuilder.Entity<BaiNghe>(entity =>
        {
            entity.HasKey(e => e.MaBaiNghe).HasName("bai_nghe_pkey");

            entity.ToTable("bai_nghe");

            entity.Property(e => e.MaBaiNghe)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_nghe");
            entity.Property(e => e.BanGhiAm).HasColumnName("ban_ghi_am");
            entity.Property(e => e.DoKho)
                .HasMaxLength(20)
                .HasColumnName("do_kho");
            entity.Property(e => e.DuongDanAudio)
                .HasMaxLength(500)
                .HasColumnName("duong_dan_audio");
            entity.Property(e => e.MaBai)
                .HasMaxLength(10)
                .HasColumnName("ma_bai");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");

            entity.HasOne(d => d.MaBaiNavigation).WithMany(p => p.BaiNghes)
                .HasForeignKey(d => d.MaBai)
                .HasConstraintName("bai_nghe_ma_bai_fkey");
        });

        modelBuilder.Entity<BaiViet>(entity =>
        {
            entity.HasKey(e => e.MaBaiViet).HasName("bai_viet_pkey");

            entity.ToTable("bai_viet");

            entity.Property(e => e.MaBaiViet)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_viet");
            entity.Property(e => e.BaiMau).HasColumnName("bai_mau");
            entity.Property(e => e.DeBai).HasColumnName("de_bai");
            entity.Property(e => e.MaBai)
                .HasMaxLength(10)
                .HasColumnName("ma_bai");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.SoTuToiDa).HasColumnName("so_tu_toi_da");
            entity.Property(e => e.SoTuToiThieu).HasColumnName("so_tu_toi_thieu");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");

            entity.HasOne(d => d.MaBaiNavigation).WithMany(p => p.BaiViets)
                .HasForeignKey(d => d.MaBai)
                .HasConstraintName("bai_viet_ma_bai_fkey");
        });

        modelBuilder.Entity<BaiVietHocVien>(entity =>
        {
            entity.HasKey(e => e.MaBaiVietHv).HasName("bai_viet_hoc_vien_pkey");

            entity.ToTable("bai_viet_hoc_vien");

            entity.Property(e => e.MaBaiVietHv).HasColumnName("ma_bai_viet_hv");
            entity.Property(e => e.Diem).HasColumnName("diem");
            entity.Property(e => e.MaBaiViet)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_viet");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_nop");
            entity.Property(e => e.NhanXet).HasColumnName("nhan_xet");
            entity.Property(e => e.NoiDung).HasColumnName("noi_dung");
            entity.Property(e => e.SoTu).HasColumnName("so_tu");

            entity.HasOne(d => d.MaBaiVietNavigation).WithMany(p => p.BaiVietHocViens)
                .HasForeignKey(d => d.MaBaiViet)
                .HasConstraintName("bai_viet_hoc_vien_ma_bai_viet_fkey");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.BaiVietHocViens)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("bai_viet_hoc_vien_ma_nd_fkey");
        });

        modelBuilder.Entity<CauHoiDoc>(entity =>
        {
            entity.HasKey(e => e.MaCauHoi).HasName("cau_hoi_doc_pkey");

            entity.ToTable("cau_hoi_doc");

            entity.HasIndex(e => e.MaBaiDoc, "idx_cau_hoi_doc");

            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(10)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.Diem)
                .HasDefaultValue(1)
                .HasColumnName("diem");
            entity.Property(e => e.GiaiThich).HasColumnName("giai_thich");
            entity.Property(e => e.MaBaiDoc)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_doc");
            entity.Property(e => e.NoiDungCauHoi).HasColumnName("noi_dung_cau_hoi");
            entity.Property(e => e.ThuTuHienThi).HasColumnName("thu_tu_hien_thi");

            entity.HasOne(d => d.MaBaiDocNavigation).WithMany(p => p.CauHoiDocs)
                .HasForeignKey(d => d.MaBaiDoc)
                .HasConstraintName("cau_hoi_doc_ma_bai_doc_fkey");
        });

        modelBuilder.Entity<CauHoiNghe>(entity =>
        {
            entity.HasKey(e => e.MaCauHoi).HasName("cau_hoi_nghe_pkey");

            entity.ToTable("cau_hoi_nghe");

            entity.HasIndex(e => e.MaBaiNghe, "idx_cau_hoi_nghe");

            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(10)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.Diem)
                .HasDefaultValue(1)
                .HasColumnName("diem");
            entity.Property(e => e.GiaiThich).HasColumnName("giai_thich");
            entity.Property(e => e.MaBaiNghe)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_nghe");
            entity.Property(e => e.NoiDungCauHoi).HasColumnName("noi_dung_cau_hoi");
            entity.Property(e => e.ThuTuHienThi).HasColumnName("thu_tu_hien_thi");

            entity.HasOne(d => d.MaBaiNgheNavigation).WithMany(p => p.CauHoiNghes)
                .HasForeignKey(d => d.MaBaiNghe)
                .HasConstraintName("cau_hoi_nghe_ma_bai_nghe_fkey");
        });

        modelBuilder.Entity<DangKyLoTrinh>(entity =>
        {
            entity.HasKey(e => e.MaDangKy).HasName("dang_ky_lo_trinh_pkey");

            entity.ToTable("dang_ky_lo_trinh");

            entity.HasIndex(e => new { e.MaNd, e.MaLoTrinh }, "dang_ky_lo_trinh_ma_nd_ma_lo_trinh_key").IsUnique();

            entity.Property(e => e.MaDangKy).HasColumnName("ma_dang_ky");
            entity.Property(e => e.MaLoTrinh)
                .HasMaxLength(10)
                .HasColumnName("ma_lo_trinh");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_dang_ky");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Đang học'::character varying")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.MaLoTrinhNavigation).WithMany(p => p.DangKyLoTrinhs)
                .HasForeignKey(d => d.MaLoTrinh)
                .HasConstraintName("dang_ky_lo_trinh_ma_lo_trinh_fkey");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.DangKyLoTrinhs)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("dang_ky_lo_trinh_ma_nd_fkey");
        });

        modelBuilder.Entity<DanhGiaAi>(entity =>
        {
            entity.HasKey(e => e.MaDgai).HasName("danh_gia_ai_pkey");

            entity.ToTable("danh_gia_ai");

            entity.Property(e => e.MaDgai)
                .HasMaxLength(20)
                .HasColumnName("ma_dgai");
            entity.Property(e => e.DiemManh)
                .HasMaxLength(255)
                .HasColumnName("diem_manh");
            entity.Property(e => e.DiemSo).HasColumnName("diem_so");
            entity.Property(e => e.DiemYeu)
                .HasMaxLength(255)
                .HasColumnName("diem_yeu");
            entity.Property(e => e.DuDoanDiem).HasColumnName("du_doan_diem");
            entity.Property(e => e.GoiYCaiThien)
                .HasMaxLength(500)
                .HasColumnName("goi_y_cai_thien");
            entity.Property(e => e.MaBkt)
                .HasMaxLength(20)
                .HasColumnName("ma_bkt");
            entity.Property(e => e.MaNd)
                .HasMaxLength(50)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayDanhGia)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_danh_gia");
            entity.Property(e => e.SoCauDung).HasColumnName("so_cau_dung");
            entity.Property(e => e.TongCauHoi).HasColumnName("tong_cau_hoi");
            entity.Property(e => e.XepHangPhanTram).HasColumnName("xep_hang_phan_tram");

            entity.HasOne(d => d.MaBktNavigation).WithMany(p => p.DanhGiaAis)
                .HasForeignKey(d => d.MaBkt)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("danh_gia_ai_ma_bkt_fkey");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.DanhGiaAis)
                .HasForeignKey(d => d.MaNd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("danh_gia_ai_ma_nd_fkey");
        });

        modelBuilder.Entity<DapAnDoc>(entity =>
        {
            entity.HasKey(e => e.MaDapAn).HasName("dap_an_doc_pkey");

            entity.ToTable("dap_an_doc");

            entity.Property(e => e.MaDapAn).HasColumnName("ma_dap_an");
            entity.Property(e => e.LaDapAnDung)
                .HasDefaultValue(false)
                .HasColumnName("la_dap_an_dung");
            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(10)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.NhanDapAn)
                .HasMaxLength(1)
                .HasColumnName("nhan_dap_an");
            entity.Property(e => e.NoiDungDapAn).HasColumnName("noi_dung_dap_an");
            entity.Property(e => e.ThuTuHienThi).HasColumnName("thu_tu_hien_thi");

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.DapAnDocs)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("dap_an_doc_ma_cau_hoi_fkey");
        });

        modelBuilder.Entity<DapAnNghe>(entity =>
        {
            entity.HasKey(e => e.MaDapAn).HasName("dap_an_nghe_pkey");

            entity.ToTable("dap_an_nghe");

            entity.Property(e => e.MaDapAn).HasColumnName("ma_dap_an");
            entity.Property(e => e.LaDapAnDung)
                .HasDefaultValue(false)
                .HasColumnName("la_dap_an_dung");
            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(10)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.NhanDapAn)
                .HasMaxLength(1)
                .HasColumnName("nhan_dap_an");
            entity.Property(e => e.NoiDungDapAn).HasColumnName("noi_dung_dap_an");
            entity.Property(e => e.ThuTuHienThi).HasColumnName("thu_tu_hien_thi");

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.DapAnNghes)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("dap_an_nghe_ma_cau_hoi_fkey");
        });

        modelBuilder.Entity<KetQuaBaiDoc>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("ket_qua_bai_doc_pkey");

            entity.ToTable("ket_qua_bai_doc");

            entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
            entity.Property(e => e.Diem).HasColumnName("diem");
            entity.Property(e => e.DiemToiDa).HasColumnName("diem_toi_da");
            entity.Property(e => e.LanLamThu)
                .HasDefaultValue(1)
                .HasColumnName("lan_lam_thu");
            entity.Property(e => e.MaBaiDoc)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_doc");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_nop");
            entity.Property(e => e.PhanTram)
                .HasPrecision(5, 2)
                .HasColumnName("phan_tram");
            entity.Property(e => e.ThoiGianLamGiay).HasColumnName("thoi_gian_lam_giay");

            entity.HasOne(d => d.MaBaiDocNavigation).WithMany(p => p.KetQuaBaiDocs)
                .HasForeignKey(d => d.MaBaiDoc)
                .HasConstraintName("ket_qua_bai_doc_ma_bai_doc_fkey");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.KetQuaBaiDocs)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("ket_qua_bai_doc_ma_nd_fkey");
        });

        modelBuilder.Entity<KetQuaBaiNghe>(entity =>
        {
            entity.HasKey(e => e.MaKetQua).HasName("ket_qua_bai_nghe_pkey");

            entity.ToTable("ket_qua_bai_nghe");

            entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
            entity.Property(e => e.Diem).HasColumnName("diem");
            entity.Property(e => e.DiemToiDa).HasColumnName("diem_toi_da");
            entity.Property(e => e.LanLamThu)
                .HasDefaultValue(1)
                .HasColumnName("lan_lam_thu");
            entity.Property(e => e.MaBaiNghe)
                .HasMaxLength(10)
                .HasColumnName("ma_bai_nghe");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayNop)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_nop");
            entity.Property(e => e.PhanTram)
                .HasPrecision(5, 2)
                .HasColumnName("phan_tram");
            entity.Property(e => e.ThoiGianLamGiay).HasColumnName("thoi_gian_lam_giay");

            entity.HasOne(d => d.MaBaiNgheNavigation).WithMany(p => p.KetQuaBaiNghes)
                .HasForeignKey(d => d.MaBaiNghe)
                .HasConstraintName("ket_qua_bai_nghe_ma_bai_nghe_fkey");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.KetQuaBaiNghes)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("ket_qua_bai_nghe_ma_nd_fkey");
        });

        modelBuilder.Entity<KtTienDo>(entity =>
        {
            entity.HasKey(e => e.MaBkt).HasName("kt_tien_do_pkey");

            entity.ToTable("kt_tien_do");

            entity.Property(e => e.MaBkt)
                .HasMaxLength(20)
                .HasColumnName("ma_bkt");
            entity.Property(e => e.DapAnPdf)
                .HasMaxLength(255)
                .HasColumnName("dap_an_pdf");
            entity.Property(e => e.DiemSo).HasColumnName("diem_so");
            entity.Property(e => e.FileDePdf)
                .HasMaxLength(255)
                .HasColumnName("file_de_pdf");
            entity.Property(e => e.MaNd)
                .HasMaxLength(50)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayLam)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_lam");
            entity.Property(e => e.TenBaiKt)
                .HasMaxLength(255)
                .HasColumnName("ten_bai_kt");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chưa làm'::character varying")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.KtTienDos)
                .HasForeignKey(d => d.MaNd)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("kt_tien_do_ma_nd_fkey");
        });

        modelBuilder.Entity<LichHocTap>(entity =>
        {
            entity.HasKey(e => e.MaLich).HasName("lich_hoc_tap_pkey");

            entity.ToTable("lich_hoc_tap");

            entity.HasIndex(e => e.MaNd, "idx_lich_hoc_nguoi_dung");

            entity.Property(e => e.MaLich)
                .HasMaxLength(10)
                .HasColumnName("ma_lich");
            entity.Property(e => e.DaHoanThanh)
                .HasDefaultValue(false)
                .HasColumnName("da_hoan_thanh");
            entity.Property(e => e.LoaiNoiDung)
                .HasMaxLength(50)
                .HasColumnName("loai_noi_dung");
            entity.Property(e => e.MaBai)
                .HasMaxLength(10)
                .HasColumnName("ma_bai");
            entity.Property(e => e.MaLoTrinh)
                .HasMaxLength(10)
                .HasColumnName("ma_lo_trinh");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.NgayHoanThanh)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_hoan_thanh");
            entity.Property(e => e.NgayHoc).HasColumnName("ngay_hoc");
            entity.Property(e => e.ThuTuNgay).HasColumnName("thu_tu_ngay");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chưa mở khóa'::character varying")
                .HasColumnName("trang_thai");
            entity.Property(e => e.TuanHoc).HasColumnName("tuan_hoc");

            entity.HasOne(d => d.MaBaiNavigation).WithMany(p => p.LichHocTaps)
                .HasForeignKey(d => d.MaBai)
                .HasConstraintName("lich_hoc_tap_ma_bai_fkey");

            entity.HasOne(d => d.MaLoTrinhNavigation).WithMany(p => p.LichHocTaps)
                .HasForeignKey(d => d.MaLoTrinh)
                .HasConstraintName("lich_hoc_tap_ma_lo_trinh_fkey");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.LichHocTaps)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("lich_hoc_tap_ma_nd_fkey");
        });

        modelBuilder.Entity<LoTrinhCoSan>(entity =>
        {
            entity.HasKey(e => e.MaLoTrinh).HasName("lo_trinh_co_san_pkey");

            entity.ToTable("lo_trinh_co_san");

            entity.Property(e => e.MaLoTrinh)
                .HasMaxLength(10)
                .HasColumnName("ma_lo_trinh");
            entity.Property(e => e.CapDo)
                .HasMaxLength(20)
                .HasColumnName("cap_do");
            entity.Property(e => e.LoaiLoTrinh)
                .HasMaxLength(20)
                .HasDefaultValueSql("'Chung'::character varying")
                .HasColumnName("loai_lo_trinh");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.MucTieuDiem).HasColumnName("muc_tieu_diem");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.TenLoTrinh)
                .HasMaxLength(100)
                .HasColumnName("ten_lo_trinh");
            entity.Property(e => e.ThoiGianDuKien)
                .HasMaxLength(50)
                .HasColumnName("thoi_gian_du_kien");
            entity.Property(e => e.TongSoBai).HasColumnName("tong_so_bai");

            // NEW: map the two new text columns
            entity.Property(e => e.KyNangTrongTam)
                .HasColumnType("text")
                .HasColumnName("ky_nang_trong_tam");
            entity.Property(e => e.ChuDeBaiHoc)
                .HasColumnType("text")
                .HasColumnName("chu_de_bai_hoc");
        });

        modelBuilder.Entity<LogHoatDong>(entity =>
        {
            entity.HasKey(e => e.MaLog).HasName("log_hoat_dong_pkey");

            entity.ToTable("log_hoat_dong");

            entity.Property(e => e.MaLog).HasColumnName("ma_log");
            entity.Property(e => e.DuLieuCu)
                .HasColumnType("jsonb")
                .HasColumnName("du_lieu_cu");
            entity.Property(e => e.DuLieuMoi)
                .HasColumnType("jsonb")
                .HasColumnName("du_lieu_moi");
            entity.Property(e => e.LoaiHoatDong)
                .HasMaxLength(50)
                .HasColumnName("loai_hoat_dong");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.MoTa).HasColumnName("mo_ta");
            entity.Property(e => e.ThoiGian)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("thoi_gian");
        });

        modelBuilder.Entity<MucTieuTheoNgay>(entity =>
        {
            entity.HasKey(e => e.MaMucTieu).HasName("muc_tieu_theo_ngay_pkey");

            entity.ToTable("muc_tieu_theo_ngay");

            entity.HasIndex(e => new { e.MaNd, e.NgayMucTieu }, "idx_muc_tieu_nguoi_dung_ngay");

            entity.HasIndex(e => new { e.MaNd, e.NgayMucTieu, e.LoaiMucTieu }, "muc_tieu_theo_ngay_ma_nd_ngay_muc_tieu_loai_muc_tieu_key").IsUnique();

            entity.Property(e => e.MaMucTieu).HasColumnName("ma_muc_tieu");
            entity.Property(e => e.DaHoanThanh)
                .HasDefaultValue(false)
                .HasColumnName("da_hoan_thanh");
            entity.Property(e => e.DonVi)
                .HasMaxLength(20)
                .HasColumnName("don_vi");
            entity.Property(e => e.GiaTriHienTai)
                .HasDefaultValue(0)
                .HasColumnName("gia_tri_hien_tai");
            entity.Property(e => e.GiaTriMucTieu).HasColumnName("gia_tri_muc_tieu");
            entity.Property(e => e.LoaiMucTieu)
                .HasMaxLength(50)
                .HasColumnName("loai_muc_tieu");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayMucTieu).HasColumnName("ngay_muc_tieu");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.MucTieuTheoNgays)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("muc_tieu_theo_ngay_ma_nd_fkey");
        });

        modelBuilder.Entity<NguoiDung>(entity =>
        {
            entity.HasKey(e => e.MaNd).HasName("nguoi_dung_pkey");

            entity.ToTable("nguoi_dung");

            entity.HasIndex(e => e.Email, "nguoi_dung_email_key").IsUnique();

            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.AnhDaiDien)
                .HasMaxLength(255)
                .HasColumnName("anh_dai_dien");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.LanDangNhapCuoi)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lan_dang_nhap_cuoi");
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .HasColumnName("mat_khau");
            entity.Property(e => e.NgayDangKy)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_dang_ky");
            entity.Property(e => e.VaiTro)
                .HasMaxLength(20)
                .HasDefaultValueSql("'User'::character varying")
                .HasColumnName("vai_tro");
        });

        modelBuilder.Entity<TienDoHocTap>(entity =>
        {
            entity.HasKey(e => e.MaTienDo).HasName("tien_do_hoc_tap_pkey");

            entity.ToTable("tien_do_hoc_tap");

            entity.HasIndex(e => e.MaBai, "idx_tien_do_bai_hoc");

            entity.HasIndex(e => e.MaNd, "idx_tien_do_nguoi_dung");

            entity.HasIndex(e => new { e.MaNd, e.MaBai }, "tien_do_hoc_tap_ma_nd_ma_bai_key").IsUnique();

            entity.Property(e => e.MaTienDo).HasColumnName("ma_tien_do");
            entity.Property(e => e.MaBai)
                .HasMaxLength(10)
                .HasColumnName("ma_bai");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.NgayCapNhat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_cap_nhat");
            entity.Property(e => e.NgayHoanThanh)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_hoan_thanh");
            entity.Property(e => e.PhanTramHoanThanh)
                .HasDefaultValue(0)
                .HasColumnName("phan_tram_hoan_thanh");
            entity.Property(e => e.ThoiGianHocPhut)
                .HasDefaultValue(0)
                .HasColumnName("thoi_gian_hoc_phut");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chưa bắt đầu'::character varying")
                .HasColumnName("trang_thai");

            entity.HasOne(d => d.MaBaiNavigation).WithMany(p => p.TienDoHocTaps)
                .HasForeignKey(d => d.MaBai)
                .HasConstraintName("tien_do_hoc_tap_ma_bai_fkey");

            entity.HasOne(d => d.MaNdNavigation).WithMany(p => p.TienDoHocTaps)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("tien_do_hoc_tap_ma_nd_fkey");
        });

        modelBuilder.Entity<TraLoiHocVienDoc>(entity =>
        {
            entity.HasKey(e => e.MaTraLoi).HasName("tra_loi_hoc_vien_doc_pkey");

            entity.ToTable("tra_loi_hoc_vien_doc");

            entity.Property(e => e.MaTraLoi).HasColumnName("ma_tra_loi");
            entity.Property(e => e.DungSai).HasColumnName("dung_sai");
            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(10)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.MaDapAnChon).HasColumnName("ma_dap_an_chon");
            entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.TraLoiHocVienDocs)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("tra_loi_hoc_vien_doc_ma_cau_hoi_fkey");

            entity.HasOne(d => d.MaDapAnChonNavigation).WithMany(p => p.TraLoiHocVienDocs)
                .HasForeignKey(d => d.MaDapAnChon)
                .HasConstraintName("tra_loi_hoc_vien_doc_ma_dap_an_chon_fkey");

            entity.HasOne(d => d.MaKetQuaNavigation).WithMany(p => p.TraLoiHocVienDocs)
                .HasForeignKey(d => d.MaKetQua)
                .HasConstraintName("tra_loi_hoc_vien_doc_ma_ket_qua_fkey");
        });

        modelBuilder.Entity<TraLoiHocVienNghe>(entity =>
        {
            entity.HasKey(e => e.MaTraLoi).HasName("tra_loi_hoc_vien_nghe_pkey");

            entity.ToTable("tra_loi_hoc_vien_nghe");

            entity.Property(e => e.MaTraLoi).HasColumnName("ma_tra_loi");
            entity.Property(e => e.DungSai).HasColumnName("dung_sai");
            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(10)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.MaDapAnChon).HasColumnName("ma_dap_an_chon");
            entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");

            entity.HasOne(d => d.MaCauHoiNavigation).WithMany(p => p.TraLoiHocVienNghes)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("tra_loi_hoc_vien_nghe_ma_cau_hoi_fkey");

            entity.HasOne(d => d.MaDapAnChonNavigation).WithMany(p => p.TraLoiHocVienNghes)
                .HasForeignKey(d => d.MaDapAnChon)
                .HasConstraintName("tra_loi_hoc_vien_nghe_ma_dap_an_chon_fkey");

            entity.HasOne(d => d.MaKetQuaNavigation).WithMany(p => p.TraLoiHocVienNghes)
                .HasForeignKey(d => d.MaKetQua)
                .HasConstraintName("tra_loi_hoc_vien_nghe_ma_ket_qua_fkey");
        });

        modelBuilder.Entity<VLichHocHomNay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_lich_hoc_hom_nay");

            entity.Property(e => e.DaHoanThanh).HasColumnName("da_hoan_thanh");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.LoaiNoiDung)
                .HasMaxLength(50)
                .HasColumnName("loai_noi_dung");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.TenBai)
                .HasMaxLength(200)
                .HasColumnName("ten_bai");
            entity.Property(e => e.TenLoTrinh)
                .HasMaxLength(100)
                .HasColumnName("ten_lo_trinh");
            entity.Property(e => e.TieuDe)
                .HasMaxLength(200)
                .HasColumnName("tieu_de");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(50)
                .HasColumnName("trang_thai");
        });

        modelBuilder.Entity<VMucTieuHomNay>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_muc_tieu_hom_nay");

            entity.Property(e => e.DaHoanThanh).HasColumnName("da_hoan_thanh");
            entity.Property(e => e.DonVi)
                .HasMaxLength(20)
                .HasColumnName("don_vi");
            entity.Property(e => e.GiaTriHienTai).HasColumnName("gia_tri_hien_tai");
            entity.Property(e => e.GiaTriMucTieu).HasColumnName("gia_tri_muc_tieu");
            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.LoaiMucTieu)
                .HasMaxLength(50)
                .HasColumnName("loai_muc_tieu");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.PhanTramHoanThanh).HasColumnName("phan_tram_hoan_thanh");
        });

        modelBuilder.Entity<VTongQuanTienDo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_tong_quan_tien_do");

            entity.Property(e => e.HoTen)
                .HasMaxLength(100)
                .HasColumnName("ho_ten");
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");
            entity.Property(e => e.PhanTramHoanThanh).HasColumnName("phan_tram_hoan_thanh");
            entity.Property(e => e.SoBaiHoanThanh).HasColumnName("so_bai_hoan_thanh");
            entity.Property(e => e.TenLoTrinh)
                .HasMaxLength(100)
                .HasColumnName("ten_lo_trinh");
            entity.Property(e => e.TongSoBai).HasColumnName("tong_so_bai");
            entity.Property(e => e.TongThoiGianHoc).HasColumnName("tong_thoi_gian_hoc");
        });
        modelBuilder.Entity<TraLoiHocVienDoc>(entity =>
        {
            entity.HasKey(e => e.MaTraLoi).HasName("tra_loi_hoc_vien_doc_pkey");
            entity.ToTable("tra_loi_hoc_vien_doc");

            entity.Property(e => e.MaTraLoi).HasColumnName("ma_tra_loi");
            entity.Property(e => e.DungSai).HasColumnName("dung_sai");
            entity.Property(e => e.MaCauHoi)
                .HasMaxLength(10)
                .HasColumnName("ma_cau_hoi");
            entity.Property(e => e.MaDapAnChon).HasColumnName("ma_dap_an_chon");
            entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");

            // THÊM CỘT ma_nd
            entity.Property(e => e.MaNd)
                .HasMaxLength(10)
                .HasColumnName("ma_nd");

            // CÁC FK
            entity.HasOne(d => d.MaCauHoiNavigation)
                .WithMany(p => p.TraLoiHocVienDocs)
                .HasForeignKey(d => d.MaCauHoi)
                .HasConstraintName("tra_loi_hoc_vien_doc_ma_cau_hoi_fkey");

            entity.HasOne(d => d.MaDapAnChonNavigation)
                .WithMany(p => p.TraLoiHocVienDocs)
                .HasForeignKey(d => d.MaDapAnChon)
                .HasConstraintName("tra_loi_hoc_vien_doc_ma_dap_an_chon_fkey");

            entity.HasOne(d => d.MaKetQuaNavigation)
                .WithMany(p => p.TraLoiHocVienDocs)
                .HasForeignKey(d => d.MaKetQua)
                .HasConstraintName("tra_loi_hoc_vien_doc_ma_ket_qua_fkey");

            // FK CHO MA_ND → NGƯỜI DÙNG
            entity.HasOne(d => d.MaNdNavigation)
                .WithMany(p => p.TraLoiHocVienDocs)
                .HasForeignKey(d => d.MaNd)
                .HasConstraintName("tra_loi_hoc_vien_doc_ma_nd_fkey");
        });
        modelBuilder.Entity<VideoBaiHoc>(entity =>
        {
            entity.HasKey(e => e.MaVideo).HasName("video_bai_hoc_pkey");

            entity.ToTable("video_bai_hoc");

            entity.Property(e => e.MaVideo)
                .HasMaxLength(10)
                .HasColumnName("ma_video");
            entity.Property(e => e.DuongDanVideo)
                .HasMaxLength(500)
                .HasColumnName("duong_dan_video");
            entity.Property(e => e.MaBai)
                .HasMaxLength(10)
                .HasColumnName("ma_bai");
            entity.Property(e => e.NgayTao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngay_tao");
            entity.Property(e => e.ThoiLuongGiay).HasColumnName("thoi_luong_giay");
            entity.Property(e => e.TieuDeVideo)
                .HasMaxLength(200)
                .HasColumnName("tieu_de_video");

            entity.HasOne(d => d.MaBaiNavigation).WithMany(p => p.VideoBaiHocs)
                .HasForeignKey(d => d.MaBai)
                .HasConstraintName("video_bai_hoc_ma_bai_fkey");
        });
        modelBuilder.Entity<TraLoiHocVienDoc>(entity =>
{
    entity.HasKey(e => e.MaTraLoi).HasName("tra_loi_hoc_vien_doc_pkey");
    entity.ToTable("tra_loi_hoc_vien_doc");

    entity.Property(e => e.MaTraLoi).HasColumnName("ma_tra_loi");
    entity.Property(e => e.DungSai).HasColumnName("dung_sai");
    entity.Property(e => e.MaCauHoi)
        .HasMaxLength(10)
        .HasColumnName("ma_cau_hoi");
    entity.Property(e => e.MaDapAnChon).HasColumnName("ma_dap_an_chon");
    entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
    entity.Property(e => e.NgayTao)
        .HasDefaultValueSql("CURRENT_TIMESTAMP")
        .HasColumnType("timestamp without time zone")
        .HasColumnName("ngay_tao");

    // THÊM CỘT ma_nd
    entity.Property(e => e.MaNd)
        .HasMaxLength(10)
        .HasColumnName("ma_nd");

    // CÁC FK
    entity.HasOne(d => d.MaCauHoiNavigation)
        .WithMany(p => p.TraLoiHocVienDocs)
        .HasForeignKey(d => d.MaCauHoi)
        .HasConstraintName("tra_loi_hoc_vien_doc_ma_cau_hoi_fkey");

    entity.HasOne(d => d.MaDapAnChonNavigation)
        .WithMany(p => p.TraLoiHocVienDocs)
        .HasForeignKey(d => d.MaDapAnChon)
        .HasConstraintName("tra_loi_hoc_vien_doc_ma_dap_an_chon_fkey");

    entity.HasOne(d => d.MaKetQuaNavigation)
        .WithMany(p => p.TraLoiHocVienDocs)
        .HasForeignKey(d => d.MaKetQua)
        .HasConstraintName("tra_loi_hoc_vien_doc_ma_ket_qua_fkey");

    // FK CHO MA_ND → NGƯỜI DÙNG
    entity.HasOne(d => d.MaNdNavigation)
        .WithMany(p => p.TraLoiHocVienDocs)
        .HasForeignKey(d => d.MaNd)
        .HasConstraintName("tra_loi_hoc_vien_doc_ma_nd_fkey");
});
        modelBuilder.Entity<TraLoiHocVienNghe>(entity =>
        {
            entity.ToTable("tra_loi_hoc_vien_nghe");

            entity.HasKey(e => e.MaTraLoi);

            entity.Property(e => e.MaTraLoi).HasColumnName("ma_tra_loi");
            entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
            entity.Property(e => e.MaCauHoi).HasColumnName("ma_cau_hoi");
            entity.Property(e => e.MaDapAnChon).HasColumnName("ma_dap_an_chon");
            entity.Property(e => e.DungSai).HasColumnName("dung_sai");
            entity.Property(e => e.NgayTao).HasColumnName("ngay_tao");
            entity.Property(e => e.MaNd).HasColumnName("ma_nd");

            // Quan hệ với NguoiDung
            entity.HasOne(d => d.MaNdNavigation)
                  .WithMany(p => p.TraLoiHocVienNghes)  // Nếu có ICollection trong NguoiDung
                  .HasForeignKey(d => d.MaNd)
                  .HasConstraintName("fk_tra_loi_hoc_vien_nghe_nguoi_dung");
        });
        modelBuilder.Entity<KetQuaBaiNghe>(entity =>
        {
            entity.ToTable("ket_qua_bai_nghe");

            entity.HasKey(e => e.MaKetQua);

            entity.Property(e => e.MaKetQua).HasColumnName("ma_ket_qua");
            entity.Property(e => e.MaNd).HasColumnName("ma_nd");
            entity.Property(e => e.MaBaiNghe).HasColumnName("ma_bai_nghe");
            entity.Property(e => e.Diem).HasColumnName("diem");
            entity.Property(e => e.DiemToiDa).HasColumnName("diem_toi_da");
            entity.Property(e => e.PhanTram).HasColumnName("phan_tram");
            entity.Property(e => e.ThoiGianLamGiay).HasColumnName("thoi_gian_lam_giay");
            entity.Property(e => e.LanLamThu).HasColumnName("lan_lam_thu");
            entity.Property(e => e.NgayNop).HasColumnName("ngay_nop");

            // Quan hệ với BaiNghe
            entity.HasOne(d => d.MaBaiNgheNavigation)
                  .WithMany(p => p.KetQuaBaiNghes)
                  .HasForeignKey(d => d.MaBaiNghe)
                  .HasConstraintName("fk_ket_qua_bai_nghe_bai_nghe");

            // Quan hệ với NguoiDung
            entity.HasOne(d => d.MaNdNavigation)
                  .WithMany(p => p.KetQuaBaiNghes)
                  .HasForeignKey(d => d.MaNd)
                  .HasConstraintName("fk_ket_qua_bai_nghe_nguoi_dung");

            // Quan hệ 1-n: KetQua → TraLoiHocVienNghe
            entity.HasMany(d => d.TraLoiHocVienNghes)
                  .WithOne(p => p.MaKetQuaNavigation)
                  .HasForeignKey(p => p.MaKetQua)
                  .HasConstraintName("fk_tra_loi_nghe_ket_qua");
        });
        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
