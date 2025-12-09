using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using TOEICWEB.Data;
using TOEICWEB.Models;
using TOEICWEB.ViewModels;
using TOEICWEB.ViewModels.Admin;

namespace ToeicWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaiDocController : ControllerBase
    {
        private readonly SupabaseDbContext _context;

        public BaiDocController(SupabaseDbContext context)
        {
            _context = context;
        }

        private bool CurrentUserIsAdmin()
        {
            var role = User.FindFirst("VaiTro")?.Value;
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static string GenerateQuestionId(string prefix)
        {
            var normalizedPrefix = (prefix ?? "CH").Length > 10
                ? prefix[..10]
                : prefix;
            var remaining = Math.Max(1, 10 - normalizedPrefix.Length);
            var suffix = Guid.NewGuid().ToString("N").Substring(0, remaining).ToUpperInvariant();
            return normalizedPrefix + suffix;
        }

        private async Task<int> GetNextDocQuestionOrderAsync(string maBaiDoc)
        {
            var maxOrder = await _context.CauHoiDocs
                .Where(c => c.MaBaiDoc == maBaiDoc)
                .MaxAsync(c => (int?)c.ThuTuHienThi);

            return (maxOrder ?? 0) + 1;
        }

        private async Task<CauHoiDocWithAnswersDTO?> BuildDocQuestionDtoAsync(string maCauHoi)
        {
            return await _context.CauHoiDocs
                .Where(c => c.MaCauHoi == maCauHoi)
                .Select(c => new CauHoiDocWithAnswersDTO
                {
                    MaCauHoi = c.MaCauHoi!,
                    NoiDungCauHoi = c.NoiDungCauHoi,
                    GiaiThich = c.GiaiThich ?? string.Empty,
                    Diem = c.Diem ?? 1,
                    ThuTuHienThi = c.ThuTuHienThi,
                    DapAns = c.DapAnDocs
                        .OrderBy(d => d.ThuTuHienThi)
                        .Select(d => new DapAnDocDTO
                        {
                            MaDapAn = d.MaDapAn,
                            MaCauHoi = d.MaCauHoi ?? string.Empty,
                            NhanDapAn = d.NhanDapAn.HasValue ? d.NhanDapAn.Value.ToString() : string.Empty,
                            NoiDungDapAn = d.NoiDungDapAn,
                            ThuTuHienThi = d.ThuTuHienThi,
                            LaDapAnDung = d.LaDapAnDung ?? false
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        // LẤY DANH SÁCH TẤT CẢ BÀI ĐỌC
        [HttpGet]
        public async Task<IActionResult> GetAllBaiDoc()
        {
            try
            {
                var baiDocs = await _context.BaiDocs
                    .Select(b => new BaiDocDTO
                    {
                        MaBaiDoc = b.MaBaiDoc,
                        MaBai = b.MaBai,
                        TieuDe = b.TieuDe,
                        DoKho = b.DoKho,
                        NgayTao = b.NgayTao,
                        DuongDanFileTxt = b.DuongDanFileTxt
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Danh sách bài đọc",
                    total = baiDocs.Count,
                    data = baiDocs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách!", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("{maBaiDoc}/admin/cau-hoi")]
        public async Task<IActionResult> CreateDocQuestion(string maBaiDoc, [FromBody] AdminQuestionRequest request)
        {
            if (!CurrentUserIsAdmin())
                return Forbid();

            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var baiDoc = await _context.BaiDocs.FirstOrDefaultAsync(b => b.MaBaiDoc == maBaiDoc);
            if (baiDoc == null)
                return NotFound(new { message = "Bài đọc không tồn tại" });

            var answers = request.DapAns
                ?.Where(a => !string.IsNullOrWhiteSpace(a.NoiDungDapAn))
                .ToList();

            if (answers == null || answers.Count < 2)
                return BadRequest(new { message = "Cần ít nhất 2 đáp án hợp lệ" });

            if (!answers.Any(a => a.LaDapAnDung))
                return BadRequest(new { message = "Phải chọn ít nhất một đáp án đúng" });

            var maCauHoi = GenerateQuestionId("CHD");
            var thuTu = request.ThuTuHienThi ?? await GetNextDocQuestionOrderAsync(maBaiDoc);

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var cauHoi = new CauHoiDoc
                {
                    MaCauHoi = maCauHoi,
                    MaBaiDoc = maBaiDoc,
                    NoiDungCauHoi = request.NoiDungCauHoi.Trim(),
                    GiaiThich = string.IsNullOrWhiteSpace(request.GiaiThich) ? null : request.GiaiThich,
                    Diem = request.Diem ?? 1,
                    ThuTuHienThi = thuTu
                };

                _context.CauHoiDocs.Add(cauHoi);
                await _context.SaveChangesAsync();

                foreach (var (answer, index) in answers.Select((item, idx) => (item, idx)))
                {
                    var label = !string.IsNullOrWhiteSpace(answer.NhanDapAn)
                        ? char.ToUpperInvariant(answer.NhanDapAn.Trim()[0])
                        : (char)('A' + index);

                    var dapAn = new DapAnDoc
                    {
                        MaCauHoi = maCauHoi,
                        NoiDungDapAn = answer.NoiDungDapAn.Trim(),
                        NhanDapAn = label,
                        ThuTuHienThi = answer.ThuTuHienThi ?? index + 1,
                        LaDapAnDung = answer.LaDapAnDung
                    };
                    _context.DapAnDocs.Add(dapAn);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Không thể tạo câu hỏi", error = ex.Message });
            }

            var dto = await BuildDocQuestionDtoAsync(maCauHoi);
            return Ok(new { message = "Đã tạo câu hỏi", data = dto });
        }

        [Authorize]
        [HttpDelete("{maBaiDoc}/admin/cau-hoi/{maCauHoi}")]
        public async Task<IActionResult> DeleteDocQuestion(string maBaiDoc, string maCauHoi)
        {
            if (!CurrentUserIsAdmin())
                return Forbid();

            var question = await _context.CauHoiDocs
                .Include(c => c.DapAnDocs)
                .FirstOrDefaultAsync(c => c.MaCauHoi == maCauHoi && c.MaBaiDoc == maBaiDoc);

            if (question == null)
                return NotFound(new { message = "Không tìm thấy câu hỏi" });

            var studentAnswers = await _context.TraLoiHocVienDocs
                .Where(t => t.MaCauHoi == maCauHoi)
                .ToListAsync();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (studentAnswers.Any())
                {
                    _context.TraLoiHocVienDocs.RemoveRange(studentAnswers);
                }

                if (question.DapAnDocs.Any())
                {
                    _context.DapAnDocs.RemoveRange(question.DapAnDocs);
                }

                _context.CauHoiDocs.Remove(question);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Không thể xóa câu hỏi", error = ex.Message });
            }

            return NoContent();
        }

        // LẤY CHI TIẾT BÀI ĐỌC (Nội dung + Câu hỏi + Đáp án)
        [HttpGet("{maBaiDoc}")]
        public async Task<IActionResult> GetBaiDocDetail(string maBaiDoc)
        {
            try
            {
                var baiDoc = await _context.BaiDocs
                    .FirstOrDefaultAsync(b => b.MaBaiDoc == maBaiDoc);

                if (baiDoc == null)
                    return NotFound(new { message = "Bài đọc không tồn tại!" });

                var cauHois = await _context.CauHoiDocs
                    .Where(c => c.MaBaiDoc == maBaiDoc)
                    .Select(c => new CauHoiDocDTO
                    {
                        MaCauHoi = c.MaCauHoi,
                        NoiDungCauHoi = c.NoiDungCauHoi,
                        GiaiThich = c.GiaiThich,
                        Diem = c.Diem ?? 1,
                        ThuTuHienThi = c.ThuTuHienThi
                    })
                    .OrderBy(c => c.ThuTuHienThi)
                    .ToListAsync();

                var cauHoisWithAnswers = new List<CauHoiDocWithAnswersDTO>();
                foreach (var cauHoi in cauHois)
                {
                    var dapAns = await _context.DapAnDocs
                        .Where(d => d.MaCauHoi == cauHoi.MaCauHoi)
                        .Select(d => new DapAnDocDTO
                        {
                            MaDapAn = d.MaDapAn,
                            MaCauHoi = d.MaCauHoi,
                            NhanDapAn = d.NhanDapAn.ToString(),
                            NoiDungDapAn = d.NoiDungDapAn,
                            ThuTuHienThi = d.ThuTuHienThi,
                            LaDapAnDung = d.LaDapAnDung ?? false
                        })
                        .OrderBy(d => d.ThuTuHienThi)
                        .ToListAsync();

                    cauHoisWithAnswers.Add(new CauHoiDocWithAnswersDTO
                    {
                        MaCauHoi = cauHoi.MaCauHoi,
                        NoiDungCauHoi = cauHoi.NoiDungCauHoi,
                        GiaiThich = cauHoi.GiaiThich,
                        Diem = cauHoi.Diem,
                        ThuTuHienThi = cauHoi.ThuTuHienThi,
                        DapAns = dapAns
                    });
                }

                return Ok(new
                {
                    maBaiDoc = baiDoc.MaBaiDoc,
                    maBai = baiDoc.MaBai,
                    tieuDe = baiDoc.TieuDe,
                    doKho = baiDoc.DoKho,
                    noiDung = baiDoc.NoiDung,
                    duongDanFileTxt = baiDoc.DuongDanFileTxt,
                    ngayTao = baiDoc.NgayTao,
                    tongCauHoi = cauHoisWithAnswers.Count,
                    cauHois = cauHoisWithAnswers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy chi tiết!", error = ex.Message });
            }
        }

        // LẤY DANH SÁCH BÀI ĐỌC THEO MÃ LỘ TRÌNH
        [HttpGet("lo-trinh/{maLoTrinh}")]
        public async Task<IActionResult> GetBaiDocByLoTrinh(string maLoTrinh)
        {
            try
            {
                var loTrinh = await _context.LoTrinhCoSans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.MaLoTrinh == maLoTrinh);

                if (loTrinh == null)
                    return NotFound(new { message = "Không tìm thấy lộ trình" });

                var baiDocs = await _context.BaiDocs
                    .Where(bd => bd.MaBaiNavigation != null && bd.MaBaiNavigation.MaLoTrinh == maLoTrinh)
                    .Select(bd => new
                    {
                        bd.MaBaiDoc,
                        bd.MaBai,
                        TenBai = bd.MaBaiNavigation.TenBai,
                        SoThuTu = bd.MaBaiNavigation.SoThuTu,
                        TieuDe = bd.TieuDe,
                        DoKho = bd.DoKho,
                        DuongDanFileTxt = bd.DuongDanFileTxt,
                        NgayTao = bd.NgayTao,
                        SoCauHoi = bd.CauHoiDocs.Count
                    })
                    .OrderBy(x => x.SoThuTu)
                    .ThenBy(x => x.TieuDe)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Danh sách bài đọc theo lộ trình",
                    maLoTrinh = loTrinh.MaLoTrinh,
                    tenLoTrinh = loTrinh.TenLoTrinh,
                    loaiLoTrinh = loTrinh.LoaiLoTrinh,
                    // Nếu có các cột này trong model:
                    kyNangTrongTam = (loTrinh as dynamic)?.KyNangTrongTam,
                    chuDeBaiHoc = (loTrinh as dynamic)?.ChuDeBaiHoc,
                    total = baiDocs.Count,
                    data = baiDocs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy bài đọc theo lộ trình!", error = ex.Message });
            }
        }

        // NỘP BÀI ĐỌC - ĐÃ FIX + TỐI ƯU + TRANSACTION
        [Authorize]
        [HttpPost("submit/{maBaiDoc}")]
        public async Task<IActionResult> SubmitBaiDoc(string maBaiDoc, [FromBody] SubmitBaiDocVM model)
        {
            if (model?.TraLois == null || !model.TraLois.Any())
                return BadRequest(new { message = "Dữ liệu trả lời không hợp lệ!" });

            var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(maNd))
                return Unauthorized(new { message = "Không tìm thấy thông tin người dùng!" });

            try
            {
                var baiDoc = await _context.BaiDocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.MaBaiDoc == maBaiDoc);

                if (baiDoc == null)
                    return NotFound(new { message = "Bài đọc không tồn tại!" });

                var cauHoiDapAnDung = await _context.CauHoiDocs
                    .Where(c => c.MaBaiDoc == maBaiDoc)
                    .Select(c => new
                    {
                        c.MaCauHoi,
                        Diem = c.Diem ?? 1,
                        DapAnDung = _context.DapAnDocs
                            .Where(d => d.MaCauHoi == c.MaCauHoi && d.LaDapAnDung == true)
                            .Select(d => d.MaDapAn)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                if (!cauHoiDapAnDung.Any())
                    return BadRequest(new { message = "Bài đọc không có câu hỏi!" });

                var traLoiEntities = new List<TraLoiHocVienDoc>();
                var chiTietKetQua = new List<object>();
                int diem = 0, diemToiDa = 0;

                foreach (var ch in cauHoiDapAnDung)
                {
                    diemToiDa += ch.Diem;
                    var traLoi = model.TraLois.FirstOrDefault(t => t.MaCauHoi == ch.MaCauHoi);
                    var maDapAnChon = traLoi?.MaDapAn;
                    var dungSai = maDapAnChon.HasValue && maDapAnChon == ch.DapAnDung;

                    if (dungSai) diem += ch.Diem;

                    chiTietKetQua.Add(new
                    {
                        maCauHoi = ch.MaCauHoi,
                        maDapAnChon,
                        dapAnDung = ch.DapAnDung,
                        dungSai,
                        diemCauHoi = ch.Diem
                    });

                    traLoiEntities.Add(new TraLoiHocVienDoc
                    {
                        MaNd = maNd,
                        MaCauHoi = ch.MaCauHoi,
                        MaDapAnChon = maDapAnChon,
                        DungSai = dungSai,
                        NgayTao = DateTime.Now
                    });
                }

                var phanTram = diemToiDa > 0 ? Math.Round((double)diem / diemToiDa * 100, 2) : 0;

                var lanLamThu = await _context.KetQuaBaiDocs
                    .CountAsync(k => k.MaBaiDoc == maBaiDoc && k.MaNd == maNd) + 1;

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.TraLoiHocVienDocs.AddRangeAsync(traLoiEntities);

                    var ketQua = new KetQuaBaiDoc
                    {
                        MaBaiDoc = maBaiDoc,
                        MaNd = maNd,
                        Diem = diem,
                        DiemToiDa = diemToiDa,
                        PhanTram = Convert.ToDecimal(phanTram),
                        ThoiGianLamGiay = model.ThoiGianLamGiay,
                        LanLamThu = lanLamThu,
                        NgayNop = DateTime.Now
                    };
                    _context.KetQuaBaiDocs.Add(ketQua);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Lỗi khi lưu vào DB: " + ex.Message);
                }

                return Ok(new
                {
                    message = "Nộp bài thành công!",
                    maBaiDoc,
                    diem,
                    diemToiDa,
                    phanTram,
                    thoiGianLamGiay = model.ThoiGianLamGiay,
                    lanLamThu,
                    tongCauHoi = cauHoiDapAnDung.Count,
                    soCauDung = chiTietKetQua.Count(x => (bool)x.GetType().GetProperty("dungSai").GetValue(x)),
                    chiTiet = chiTietKetQua
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Lỗi khi nộp bài!",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // LẤY TẤT CẢ LỊCH SỬ BÀI ĐỌC CỦA NGƯỜI DÙNG
        [Authorize]
        [HttpGet("history")]
        public async Task<IActionResult> GetAllBaiDocHistory()
        {
            try
            {
                var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNd))
                    return BadRequest(new { message = "Không tìm thấy thông tin người dùng" });

                var ketQuas = await _context.KetQuaBaiDocs
                    .Where(k => k.MaNd == maNd)
                    .Include(k => k.MaBaiDocNavigation)
                    .Select(k => new
                    {
                        maBaiDoc = k.MaBaiDoc,
                        tieuDe = k.MaBaiDocNavigation.TieuDe,
                        doKho = k.MaBaiDocNavigation.DoKho,
                        diem = k.Diem ?? 0,
                        diemToiDa = k.DiemToiDa ?? 1,
                        phanTram = k.PhanTram,
                        thoiGianLamGiay = k.ThoiGianLamGiay ?? 0,
                        thoiGianLamPhut = (k.ThoiGianLamGiay ?? 0) / 60,
                        lanLamThu = k.LanLamThu ?? 1,
                        ngayNop = k.NgayNop,
                        ngayNopFormatted = k.NgayNop.HasValue
                            ? k.NgayNop.Value.ToString("dd/MM/yyyy HH:mm")
                            : ""
                    })
                    .OrderByDescending(k => k.ngayNop)
                    .ToListAsync();

                if (!ketQuas.Any())
                {
                    return Ok(new
                    {
                        message = "Bạn chưa làm bài đọc nào",
                        total = 0,
                        data = new List<object>()
                    });
                }

                return Ok(new
                {
                    message = "Lịch sử bài đọc",
                    total = ketQuas.Count,
                    data = ketQuas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi!", error = ex.Message });
            }
        }

        // LẤY LỊCH SỬ BÀI ĐỌC CỤ THỂ
        [Authorize]
        [HttpGet("history/{maBaiDoc}")]
        public async Task<IActionResult> GetBaiDocHistory(string maBaiDoc)
        {
            try
            {
                var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNd))
                    return BadRequest(new { message = "Không tìm thấy người dùng" });

                var baiDoc = await _context.BaiDocs.FirstOrDefaultAsync(b => b.MaBaiDoc == maBaiDoc);
                if (baiDoc == null)
                    return NotFound(new { message = "Bài đọc không tồn tại" });

                var ketQuas = await _context.KetQuaBaiDocs
                    .Where(k => k.MaBaiDoc == maBaiDoc && k.MaNd == maNd)
                    .Select(k => new
                    {
                        tieuDe = baiDoc.TieuDe,
                        doKho = baiDoc.DoKho,
                        diem = k.Diem ?? 0,
                        diemToiDa = k.DiemToiDa ?? 1,
                        phanTram = k.PhanTram,
                        thoiGianLamGiay = k.ThoiGianLamGiay ?? 0,
                        thoiGianLamPhut = (k.ThoiGianLamGiay ?? 0) / 60,
                        lanLamThu = k.LanLamThu ?? 1,
                        ngayNop = k.NgayNop,
                        ngayNopFormatted = k.NgayNop.HasValue
                            ? k.NgayNop.Value.ToString("dd/MM/yyyy HH:mm")
                            : ""
                    })
                    .OrderByDescending(k => k.ngayNop)
                    .ToListAsync();

                if (!ketQuas.Any())
                {
                    return Ok(new
                    {
                        message = $"Bạn chưa làm bài đọc '{baiDoc.TieuDe}'",
                        total = 0,
                        data = new List<object>()
                    });
                }

                return Ok(new
                {
                    message = $"Lịch sử bài đọc '{baiDoc.TieuDe}'",
                    total = ketQuas.Count,
                    data = ketQuas
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi!", error = ex.Message });
            }
        }

        // THỐNG KÊ TỔNG HỢP
        [Authorize]
        [HttpGet("history/stats/summary")]
        public async Task<IActionResult> GetBaiDocHistoryStats()
        {
            try
            {
                var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNd))
                    return BadRequest(new { message = "Không tìm thấy người dùng" });

                var ketQuas = await _context.KetQuaBaiDocs
                    .Where(k => k.MaNd == maNd)
                    .Include(k => k.MaBaiDocNavigation)
                    .ToListAsync();

                if (!ketQuas.Any())
                {
                    return Ok(new
                    {
                        message = "Bạn chưa làm bài đọc nào",
                        tongBaiDaLam = 0,
                        diemTrungBinh = 0,
                        thoiGianHocTongCong = 0,
                        data = new List<object>()
                    });
                }

                var diemTrungBinh = Math.Round(
                    ketQuas.Average(k => (double)(k.Diem ?? 0) / (double)(k.DiemToiDa ?? 1) * 100), 2);

                var thoiGianTongCong = ketQuas.Sum(k => k.ThoiGianLamGiay ?? 0);

                var detail = ketQuas.Select(k => new
                {
                    maBaiDoc = k.MaBaiDoc,
                    tieuDe = k.MaBaiDocNavigation.TieuDe,
                    doKho = k.MaBaiDocNavigation.DoKho,
                    diem = k.Diem ?? 0,
                    diemToiDa = k.DiemToiDa ?? 1,
                    phanTram = k.PhanTram,
                    thoiGianLamPhut = (k.ThoiGianLamGiay ?? 0) / 60,
                    lanLamThu = k.LanLamThu ?? 1,
                    ngayNopFormatted = k.NgayNop.HasValue
                        ? k.NgayNop.Value.ToString("dd/MM/yyyy HH:mm")
                        : ""
                })
                .OrderByDescending(k => k.ngayNopFormatted)
                .ToList();

                return Ok(new
                {
                    message = "Thống kê lịch sử bài đọc",
                    tongBaiDaLam = ketQuas.Count,
                    diemTrungBinh,
                    thoiGianHocPhut = thoiGianTongCong / 60,
                    data = detail
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi!", error = ex.Message });
            }
        }
    }
}