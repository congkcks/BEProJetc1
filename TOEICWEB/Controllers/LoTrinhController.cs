using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TOEICWEB.Data;
using TOEICWEB.Models;
using TOEICWEB.ViewModels.LoTrinh;
using TOEICWEB.ViewModels.LoTrinh.Request;
using YourApp.ViewModels.LoTrinh;

namespace TOEICWEB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoTrinhController : ControllerBase
    {
        private readonly SupabaseDbContext _context;

        public LoTrinhController(SupabaseDbContext context)
        {
            _context = context;
        }

        // === Helper: Lấy ma_nd từ JWT (Supabase dùng "sub") ===
        private string? GetMaNd()
        {
            return User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        #region 1. Lấy danh sách lộ trình có sẵn
        [HttpGet("co-san")]
        public async Task<IActionResult> GetLoTrinhCoSan()
        {
            try
            {
                var loTrinhs = await _context.LoTrinhCoSans
                    .Select(lt => new LoTrinhCoSanDto
                    {
                        MaLoTrinh = lt.MaLoTrinh,
                        TenLoTrinh = lt.TenLoTrinh,
                        MoTa = lt.MoTa,
                        ThoiGianDuKien = lt.ThoiGianDuKien,
                        CapDo = lt.CapDo,
                        LoaiLoTrinh = lt.LoaiLoTrinh,
                        MucTieuDiem = lt.MucTieuDiem ?? 0,
                        TongSoBai = (int)lt.TongSoBai,
                        NgayTao = lt.NgayTao ?? DateTime.MinValue,

                        // NEW:
                        KyNangTrongTam = lt.KyNangTrongTam,
                        ChuDeBaiHoc = lt.ChuDeBaiHoc
                    })
                    .OrderBy(lt => lt.CapDo)
                    .ThenBy(lt => lt.NgayTao)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Danh sách lộ trình có sẵn",
                    total = loTrinhs.Count,
                    data = loTrinhs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }
        #endregion

        #region 2. Chi tiết lộ trình
        [HttpGet("co-san/{maLoTrinh}")]
        public async Task<IActionResult> GetLoTrinhCoSanDetail(string maLoTrinh)
        {
            try
            {
                var loTrinh = await _context.LoTrinhCoSans
                    .Where(lt => lt.MaLoTrinh == maLoTrinh)
                    .Select(lt => new LoTrinhCoSanDto
                    {
                        MaLoTrinh = lt.MaLoTrinh,
                        TenLoTrinh = lt.TenLoTrinh,
                        MoTa = lt.MoTa,
                        ThoiGianDuKien = lt.ThoiGianDuKien,
                        CapDo = lt.CapDo,
                        LoaiLoTrinh = lt.LoaiLoTrinh,
                        MucTieuDiem = lt.MucTieuDiem ?? 0,
                        TongSoBai = (int)lt.TongSoBai,
                        NgayTao = lt.NgayTao ?? DateTime.MinValue,

                        // NEW:
                        KyNangTrongTam = lt.KyNangTrongTam,
                        ChuDeBaiHoc = lt.ChuDeBaiHoc
                    })
                    .FirstOrDefaultAsync();

                if (loTrinh == null)
                    return NotFound(new { message = "Không tìm thấy lộ trình" });

                return Ok(new { message = "Chi tiết lộ trình", data = loTrinh });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi server", error = ex.Message });
            }
        }
        #endregion

        #region 3. Đăng ký lộ trình → Tự động tạo lịch học
        [Authorize]
        [HttpPost("dang-ky/{maLoTrinh}")]
        public async Task<IActionResult> DangKyLoTrinh(string maLoTrinh)
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd))
                    return BadRequest(new { message = "Không tìm thấy người dùng" });

                var loTrinh = await _context.LoTrinhCoSans
                    .FirstOrDefaultAsync(lt => lt.MaLoTrinh == maLoTrinh);

                if (loTrinh == null)
                    return NotFound(new { message = "Lộ trình không tồn tại" });

                if (await _context.DangKyLoTrinhs.AnyAsync(dk => dk.MaLoTrinh == maLoTrinh && dk.MaNd == maNd))
                    return BadRequest(new { message = "Đã đăng ký lộ trình này rồi" });

                var dangKy = new DangKyLoTrinh
                {
                    MaLoTrinh = maLoTrinh,
                    MaNd = maNd,
                    NgayDangKy = DateTime.Now,
                    TrangThai = "Đang học"
                };

                _context.DangKyLoTrinhs.Add(dangKy);

                await TaoLichHocTuBaiHoc(maNd, maLoTrinh, dangKy.NgayDangKy ?? DateTime.Now);

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Đăng ký thành công",
                    data = new
                    {
                        maDangKy = dangKy.MaDangKy,
                        tenLoTrinh = loTrinh.TenLoTrinh
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi đăng ký", error = ex.Message });
            }
        }

        private async Task TaoLichHocTuBaiHoc(string maNd, string maLoTrinh, DateTime ngayDangKy)
        {
            var baiHocs = await _context.BaiHocs
                .Where(b => b.MaLoTrinh == maLoTrinh)
                .OrderBy(b => b.SoThuTu)
                .ToListAsync();

            if (!baiHocs.Any()) return;

            var lichHocs = new List<LichHocTap>();
            var ngayHocHienTai = ngayDangKy.Date;
            int tuanHoc = 1;
            bool isFirst = true;

            foreach (var baiHoc in baiHocs)
            {
                var thuTuNgay = (int)ngayHocHienTai.DayOfWeek == 0 ? 7 : (int)ngayHocHienTai.DayOfWeek;

                // 1. Bài lý thuyết
                lichHocs.Add(new LichHocTap
                {
                    MaLich = $"LICH_{maNd}_{baiHoc.MaBai}_LT",
                    MaNd = maNd,
                    MaLoTrinh = maLoTrinh,
                    MaBai = baiHoc.MaBai,
                    TieuDe = $"Lý thuyết: {baiHoc.TenBai}",
                    MoTa = baiHoc.MoTa,
                    LoaiNoiDung = LichHocTap.ContentType.LyThuyet,
                    NgayHoc = DateOnly.FromDateTime(ngayHocHienTai),
                    TrangThai = isFirst ? LichHocTap.Status.DaMoKhoa : LichHocTap.Status.ChuaMoKhoa,
                    DaHoanThanh = false,
                    ThuTuNgay = thuTuNgay,
                    TuanHoc = tuanHoc
                });

                isFirst = false;

                // 2. Bài nghe
                var baiNghe = await _context.BaiNghes.FirstOrDefaultAsync(bn => bn.MaBai == baiHoc.MaBai);
                if (baiNghe != null)
                {
                    lichHocs.Add(new LichHocTap
                    {
                        MaLich = $"LICH_{maNd}_{baiHoc.MaBai}_NGHE",
                        MaNd = maNd,
                        MaLoTrinh = maLoTrinh,
                        MaBai = baiHoc.MaBai,
                        TieuDe = $"Nghe: {baiNghe.TieuDe}",
                        MoTa = $"Luyện nghe - {baiNghe.DoKho}",
                        LoaiNoiDung = LichHocTap.ContentType.Nghe,
                        NgayHoc = DateOnly.FromDateTime(ngayHocHienTai),
                        TrangThai = LichHocTap.Status.ChuaMoKhoa,
                        DaHoanThanh = false,
                        ThuTuNgay = thuTuNgay,
                        TuanHoc = tuanHoc
                    });
                }

                // 3. Bài đọc
                var baiDoc = await _context.BaiDocs.FirstOrDefaultAsync(bd => bd.MaBai == baiHoc.MaBai);
                if (baiDoc != null)
                {
                    lichHocs.Add(new LichHocTap
                    {
                        MaLich = $"LICH_{maNd}_{baiHoc.MaBai}_DOC",
                        MaNd = maNd,
                        MaLoTrinh = maLoTrinh,
                        MaBai = baiHoc.MaBai,
                        TieuDe = $"Đọc: {baiDoc.TieuDe}",
                        MoTa = $"Luyện đọc hiểu - {baiDoc.DoKho}",
                        LoaiNoiDung = LichHocTap.ContentType.Doc,
                        NgayHoc = DateOnly.FromDateTime(ngayHocHienTai),
                        TrangThai = LichHocTap.Status.ChuaMoKhoa,
                        DaHoanThanh = false,
                        ThuTuNgay = thuTuNgay,
                        TuanHoc = tuanHoc
                    });
                }

                ngayHocHienTai = ngayHocHienTai.AddDays(1);
                if (ngayHocHienTai.DayOfWeek == DayOfWeek.Saturday)
                    tuanHoc++;
            }

            _context.LichHocTaps.AddRange(lichHocs);
        }
        #endregion

        #region 4. Lộ trình của tôi
        [Authorize]
        [HttpGet("cua-toi")]
        public async Task<IActionResult> GetLoTrinhCuaToi()
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd))
                    return BadRequest(new { message = "Không tìm thấy người dùng" });

                var dangKyList = await _context.DangKyLoTrinhs
                    .Where(dk => dk.MaNd == maNd)
                    .Join(_context.LoTrinhCoSans,
                        dk => dk.MaLoTrinh,
                        lt => lt.MaLoTrinh,
                        (dk, lt) => new { dk, lt })
                    .ToListAsync();

                if (!dangKyList.Any())
                    return Ok(new { message = "Chưa có lộ trình nào", total = 0, data = new object[0] });

                var maLoTrinhs = dangKyList.Select(x => x.dk.MaLoTrinh).ToList();

                var baiHocs = await _context.BaiHocs
                    .Where(b => maLoTrinhs.Contains(b.MaLoTrinh))
                    .Select(b => new
                    {
                        b.MaLoTrinh,
                        b.MaBai,
                        b.TenBai,
                        b.MoTa,
                        ThoiLuongPhut = b.ThoiLuongPhut ?? 0,
                        b.SoThuTu,
                        b.NgayTao
                    })
                    .OrderBy(b => b.MaLoTrinh)
                    .ThenBy(b => b.SoThuTu)
                    .ToListAsync();

                var lichHoc = await _context.LichHocTaps
                    .Where(l => l.MaNd == maNd && maLoTrinhs.Contains(l.MaLoTrinh))
                    .Select(l => new { l.MaBai, l.TrangThai, l.DaHoanThanh })
                    .ToListAsync();

                var result = dangKyList.Select(dk => new
                {
                    dk.dk.MaDangKy,
                    dk.dk.MaLoTrinh,
                    dk.lt.TenLoTrinh,
                    dk.lt.MoTa,
                    dk.lt.CapDo,
                    dk.lt.ThoiGianDuKien,
                    // NEW: include new fields in user's lo trinh list
                    KyNangTrongTam = dk.lt.KyNangTrongTam,
                    ChuDeBaiHoc = dk.lt.ChuDeBaiHoc,
                    TongSoBai = (int)dk.lt.TongSoBai,
                    NgayDangKy = dk.dk.NgayDangKy ?? DateTime.MinValue,
                    dk.dk.TrangThai,
                    BaiHocs = baiHocs
                        .Where(b => b.MaLoTrinh == dk.dk.MaLoTrinh)
                        .Select(b => new
                        {
                            b.MaBai,
                            b.TenBai,
                            b.MoTa,
                            b.ThoiLuongPhut,
                            b.SoThuTu,
                            b.NgayTao,
                            TrangThai = lichHoc.FirstOrDefault(l => l.MaBai == b.MaBai)?.TrangThai ?? LichHocTap.Status.ChuaMoKhoa,
                            DaHoanThanh = lichHoc.FirstOrDefault(l => l.MaBai == b.MaBai)?.DaHoanThanh ?? false
                        })
                        .ToList()
                })
                .OrderByDescending(x => x.NgayDangKy)
                .ToList();

                return Ok(new { message = "Lộ trình của bạn", total = result.Count, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy lộ trình", error = ex.Message });
            }
        }
        #endregion

        #region 5. Lịch học của tôi
        [Authorize]
        [HttpGet("lich-hoc")]
        public async Task<IActionResult> GetLichHocCuaToi(
            [FromQuery] string? maLoTrinh = null,
            [FromQuery] int page = 1,
            [FromQuery] int size = 20)
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd))
                    return BadRequest(new { message = "Không tìm thấy người dùng" });

                var dangKyList = await _context.DangKyLoTrinhs
                    .Where(dk => dk.MaNd == maNd)
                    .Join(_context.LoTrinhCoSans,
                        dk => dk.MaLoTrinh,
                        lt => lt.MaLoTrinh,
                        (dk, lt) => new { dk, lt })
                    .ToListAsync();

                if (!dangKyList.Any())
                    return NotFound(new { message = "Bạn chưa đăng ký lộ trình nào" });

                if (!string.IsNullOrEmpty(maLoTrinh))
                    dangKyList = dangKyList.Where(x => x.dk.MaLoTrinh == maLoTrinh).ToList();

                if (!dangKyList.Any())
                    return NotFound(new { message = "Không tìm thấy lộ trình đã đăng ký" });

                foreach (var item in dangKyList)
                {
                    var daCoLich = await _context.LichHocTaps
                        .AnyAsync(l => l.MaNd == maNd && l.MaLoTrinh == item.dk.MaLoTrinh);

                    if (!daCoLich)
                    {
                        await TaoLichHocTuBaiHoc(maNd, item.dk.MaLoTrinh, item.dk.NgayDangKy ?? DateTime.Now);
                    }
                }

                await _context.SaveChangesAsync();

                var maLoTrinhs = dangKyList.Select(x => x.dk.MaLoTrinh).ToList();
                var query = _context.LichHocTaps
                    .Where(l => l.MaNd == maNd && maLoTrinhs.Contains(l.MaLoTrinh));

                var total = await query.CountAsync();
                var lich = await query
                    .OrderBy(l => l.NgayHoc)
                    .ThenBy(l => l.ThuTuNgay)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Select(l => new LichHocTapDto
                    {
                        MaLich = l.MaLich,
                        MaLoTrinh = l.MaLoTrinh,
                        MaBai = l.MaBai,
                        TieuDe = l.TieuDe,
                        MoTa = l.MoTa,
                        LoaiNoiDung = l.LoaiNoiDung,
                        NgayHoc = l.NgayHoc.ToDateTime(TimeOnly.MinValue),
                        TrangThai = l.TrangThai,
                        DaHoanThanh = l.DaHoanThanh ?? false,
                        ThuTuNgay = l.ThuTuNgay,
                        TuanHoc = l.TuanHoc
                    })
                    .ToListAsync();

                var ngayBatDau = dangKyList.Min(x => (x.dk.NgayDangKy ?? DateTime.Now).Date);
                var ngayKetThuc = dangKyList.Max(x =>
                    (x.dk.NgayDangKy ?? DateTime.Now).AddDays(ConvertToInt(x.lt.ThoiGianDuKien, 30)).Date);

                return Ok(new
                {
                    message = "Lịch học của bạn",
                    ngayBatDau = ngayBatDau.ToString("yyyy-MM-dd"),
                    ngayKetThuc = ngayKetThuc.ToString("yyyy-MM-dd"),
                    soLoTrinh = dangKyList.Count,
                    danhSachLoTrinh = dangKyList.Select(x => new
                    {
                        x.dk.MaLoTrinh,
                        x.lt.TenLoTrinh,
                        // NEW: include the two new fields in schedule summary
                        KyNangTrongTam = x.lt.KyNangTrongTam,
                        ChuDeBaiHoc = x.lt.ChuDeBaiHoc,
                        ngayBatDau = (x.dk.NgayDangKy ?? DateTime.Now).ToString("yyyy-MM-dd"),
                        ngayKetThuc = (x.dk.NgayDangKy ?? DateTime.Now)
                            .AddDays(ConvertToInt(x.lt.ThoiGianDuKien, 30))
                            .ToString("yyyy-MM-dd")
                    }),
                    total,
                    page,
                    size,
                    totalPages = (int)Math.Ceiling(total / (double)size),
                    data = lich
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy lịch", error = ex.Message });
            }
        }

        private int ConvertToInt(object? value, int defaultValue)
        {
            if (value == null) return defaultValue;
            if (value is int i) return i;
            if (value is string s && int.TryParse(s, out var result)) return result;
            return defaultValue;
        }
        #endregion

        #region 6. Cập nhật tiến độ
        [Authorize]
        [HttpPut("cap-nhat-tien-do/{maBai}")]
        public async Task<IActionResult> CapNhatTienDo(string maBai, [FromBody] CapNhatTienDoRequest req)
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd))
                    return BadRequest(new { message = "Không tìm thấy người dùng" });

                var td = await _context.TienDoHocTaps
                    .FirstOrDefaultAsync(t => t.MaBai == maBai && t.MaNd == maNd);

                if (td == null)
                {
                    td = new TienDoHocTap { MaBai = maBai, MaNd = maNd };
                    _context.TienDoHocTaps.Add(td);
                }

                td.PhanTramHoanThanh = Math.Clamp(req.PhanTramHoanThanh, 0, 100);
                td.ThoiGianHocPhut += req.ThoiGianHocPhut;
                td.TrangThai = td.PhanTramHoanThanh >= 100 ? "Hoàn thành" : "Đang học";
                td.NgayCapNhat = DateTime.Now;

                if (td.PhanTramHoanThanh >= 100)
                    td.NgayHoanThanh = DateTime.Now;

                var lich = await _context.LichHocTaps
                    .FirstOrDefaultAsync(l => l.MaBai == maBai && l.MaNd == maNd && l.LoaiNoiDung == LichHocTap.ContentType.LyThuyet);

                if (lich != null && td.PhanTramHoanThanh >= 100)
                {
                    lich.DaHoanThanh = true;
                    lich.NgayHoanThanh = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật tiến độ thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi cập nhật", error = ex.Message });
            }
        }
        #endregion

        #region 7. Các API phụ
        [Authorize]
        [HttpGet("tien-do/{maLoTrinh}")]
        public async Task<IActionResult> GetTienDoLoTrinh(string maLoTrinh)
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd)) return BadRequest(new { message = "Không tìm thấy người dùng" });

                var dangKy = await _context.DangKyLoTrinhs
                    .FirstOrDefaultAsync(dk => dk.MaLoTrinh == maLoTrinh && dk.MaNd == maNd);
                if (dangKy == null) return NotFound(new { message = "Chưa đăng ký" });

                var loTrinh = await _context.LoTrinhCoSans.FirstOrDefaultAsync(lt => lt.MaLoTrinh == maLoTrinh);
                var tongSoBai = loTrinh?.TongSoBai ?? 0;

                var tienDos = await _context.TienDoHocTaps
                    .Where(td => td.MaNd == maNd && _context.LichHocTaps.Any(l => l.MaBai == td.MaBai && l.MaLoTrinh == maLoTrinh))
                    .Select(td => new TienDoHocTapDto
                    {
                        MaTienDo = td.MaTienDo,
                        MaBai = td.MaBai,
                        TrangThai = td.TrangThai,
                        PhanTramHoanThanh = (int)td.PhanTramHoanThanh,
                        ThoiGianHocPhut = (int)td.ThoiGianHocPhut,
                        NgayHoanThanh = td.NgayHoanThanh,
                        NgayCapNhat = td.NgayCapNhat ?? DateTime.MinValue
                    })
                    .ToListAsync();

                var hoanThanh = tienDos.Count(t => t.TrangThai == "Hoàn thành");
                var phanTram = tongSoBai > 0 ? Math.Round((double)hoanThanh / tongSoBai * 100, 2) : 0;

                return Ok(new
                {
                    message = "Tiến độ lộ trình",
                    data = new
                    {
                        tenLoTrinh = loTrinh?.TenLoTrinh,
                        trangThai = dangKy.TrangThai,
                        tongSoBai,
                        hoanThanh,
                        phanTramHoanThanh = phanTram,
                        danhSach = tienDos
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("cap-nhat-trang-thai/{maLoTrinh}")]
        public async Task<IActionResult> CapNhatTrangThai(string maLoTrinh, [FromBody] TrangThaiRequest req)
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd)) return BadRequest(new { message = "Không tìm thấy người dùng" });

                var dk = await _context.DangKyLoTrinhs
                    .FirstOrDefaultAsync(d => d.MaLoTrinh == maLoTrinh && d.MaNd == maNd);
                if (dk == null) return NotFound(new { message = "Không tìm thấy đăng ký" });

                dk.TrangThai = req.TrangThai;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật trạng thái thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi", error = ex.Message });
            }
        }

        [Authorize]
        [HttpDelete("huy-dang-ky/{maLoTrinh}")]
        public async Task<IActionResult> HuyDangKy(string maLoTrinh)
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd)) return BadRequest(new { message = "Không tìm thấy người dùng" });

                var dk = await _context.DangKyLoTrinhs
                    .FirstOrDefaultAsync(d => d.MaLoTrinh == maLoTrinh && d.MaNd == maNd);
                if (dk == null) return NotFound(new { message = "Không tìm thấy đăng ký" });

                var lichs = await _context.LichHocTaps
                    .Where(l => l.MaLoTrinh == maLoTrinh && l.MaNd == maNd).ToListAsync();

                var maBais = lichs.Select(l => l.MaBai).ToList();
                var tienDos = await _context.TienDoHocTaps
                    .Where(t => maBais.Contains(t.MaBai) && t.MaNd == maNd).ToListAsync();

                _context.LichHocTaps.RemoveRange(lichs);
                _context.TienDoHocTaps.RemoveRange(tienDos);
                _context.DangKyLoTrinhs.Remove(dk);

                await _context.SaveChangesAsync();

                return Ok(new { message = "Hủy đăng ký thành công" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("tong-quan")]
        public async Task<IActionResult> GetTongQuan()
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd)) return BadRequest(new { message = "Không tìm thấy người dùng" });

                var result = await _context.Database
                    .SqlQueryRaw<TongQuanTienDoDto>(
                        "SELECT * FROM fn_thong_ke_hoc_tap(@p0)", maNd)
                    .FirstOrDefaultAsync();

                return Ok(new
                {
                    message = "Tổng quan tiến độ",
                    data = result ?? new TongQuanTienDoDto()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("log-hoat-dong")]
        public async Task<IActionResult> GetLog([FromQuery] int page = 1, [FromQuery] int size = 10)
        {
            try
            {
                var maNd = GetMaNd();
                if (string.IsNullOrEmpty(maNd)) return BadRequest(new { message = "Không tìm thấy người dùng" });

                var logs = await _context.LogHoatDongs
                    .Where(l => l.MaNd == maNd)
                    .OrderByDescending(l => l.ThoiGian)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Select(l => new LogHoatDongDto
                    {
                        MaLog = l.MaLog,
                        LoaiHoatDong = l.LoaiHoatDong,
                        MoTa = l.MoTa,
                        ThoiGian = (DateTime)l.ThoiGian,
                        DuLieuCu = l.DuLieuCu,
                        DuLieuMoi = l.DuLieuMoi
                    })
                    .ToListAsync();

                var total = await _context.LogHoatDongs.CountAsync(l => l.MaNd == maNd);

                return Ok(new
                {
                    message = "Lịch sử hoạt động",
                    total,
                    page,
                    size,
                    data = logs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi", error = ex.Message });
            }
        }
        #endregion
    }
}