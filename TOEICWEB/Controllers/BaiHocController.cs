using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TOEICWEB.Data;
using TOEICWEB.Models;
using TOEICWEB.ViewModels;
using TOEICWEB.ViewModels.Admin;

namespace ToeicWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BaiHocController : ControllerBase
    {
        private readonly SupabaseDbContext _context;

        private record LessonVideo(string? MaVideo, string? MaBai, string? TieuDeVideo, string? DuongDanVideo, int? ThoiLuongGiay, DateTime? NgayTao);
        private record LessonDoc(string? MaBaiDoc, string? MaBai, string? TieuDe, string? DoKho, string? DuongDanFileTxt, DateTime? NgayTao, int SoCauHoi, bool DaHoanThanh);
        private record LessonNghe(string? MaBaiNghe, string? MaBai, string? TieuDe, string? DoKho, string? DuongDanAudio, string? BanGhiAm, DateTime? NgayTao, int SoCauHoi, bool DaHoanThanh);
        private record LessonViet(string? MaBaiViet, string? MaBai, string? TieuDe, string? DeBai, string? BaiMau, int? SoTuToiThieu, int? SoTuToiDa, DateTime? NgayTao, bool DaHoanThanh);

        public BaiHocController(SupabaseDbContext context)
        {
            _context = context;
        }

        private bool CurrentUserIsAdmin()
        {
            var role = User.FindFirst("VaiTro")?.Value;
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildNextId(string? currentId, string prefix)
        {
            var digits = Math.Max(3, (currentId?.Length ?? prefix.Length + 3) - prefix.Length);
            var nextNumber = 1;

            if (!string.IsNullOrWhiteSpace(currentId) && currentId!.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                var numericPart = currentId[prefix.Length..];
                if (int.TryParse(numericPart, out var parsed))
                {
                    nextNumber = parsed + 1;
                }
            }

            return $"{prefix}{nextNumber.ToString($"D{digits}")}";
        }

        private async Task<string> GenerateNextLessonIdAsync()
        {
            var last = await _context.BaiHocs
                .AsNoTracking()
                .Where(b => b.MaBai != null && b.MaBai.StartsWith("BH"))
                .OrderByDescending(b => b.MaBai)
                .Select(b => b.MaBai)
                .FirstOrDefaultAsync();

            return BuildNextId(last, "BH");
        }

        private async Task<string> GenerateNextDocIdAsync()
        {
            var last = await _context.BaiDocs
                .AsNoTracking()
                .Where(b => b.MaBaiDoc != null && b.MaBaiDoc.StartsWith("BD"))
                .OrderByDescending(b => b.MaBaiDoc)
                .Select(b => b.MaBaiDoc)
                .FirstOrDefaultAsync();

            return BuildNextId(last, "BD");
        }

        private async Task<string> GenerateNextListeningIdAsync()
        {
            var last = await _context.BaiNghes
                .AsNoTracking()
                .Where(b => b.MaBaiNghe != null && b.MaBaiNghe.StartsWith("BN"))
                .OrderByDescending(b => b.MaBaiNghe)
                .Select(b => b.MaBaiNghe)
                .FirstOrDefaultAsync();

            return BuildNextId(last, "BN");
        }

        private async Task<string> GenerateNextWritingIdAsync()
        {
            var last = await _context.BaiViets
                .AsNoTracking()
                .Where(b => b.MaBaiViet != null && b.MaBaiViet.StartsWith("BV"))
                .OrderByDescending(b => b.MaBaiViet)
                .Select(b => b.MaBaiViet)
                .FirstOrDefaultAsync();

            return BuildNextId(last, "BV");
        }

        private async Task<int> GetNextLessonOrderAsync(string maLoTrinh)
        {
            var lastOrder = await _context.BaiHocs
                .Where(b => b.MaLoTrinh == maLoTrinh)
                .MaxAsync(b => (int?)b.SoThuTu);

            return (lastOrder ?? 0) + 1;
        }

        private static string NormalizeContentType(string? rawType)
        {
            return (rawType ?? string.Empty).Trim().ToLowerInvariant();
        }

        private async Task<(string type, string id)?> UpsertLessonContentAsync(BaiHoc lesson, LessonContentRequest? contentRequest)
        {
            if (contentRequest == null)
            {
                return null;
            }

            var contentType = NormalizeContentType(contentRequest.Type);
            switch (contentType)
            {
                case "reading":
                    {
                        var payload = contentRequest.Reading ?? new ReadingContentRequest { TieuDe = lesson.TenBai };
                        var existing = await _context.BaiDocs.FirstOrDefaultAsync(b => b.MaBai == lesson.MaBai);
                        if (existing == null)
                        {
                            existing = new BaiDoc
                            {
                                MaBaiDoc = await GenerateNextDocIdAsync(),
                                MaBai = lesson.MaBai,
                                NgayTao = DateTime.UtcNow
                            };
                            _context.BaiDocs.Add(existing);
                        }

                        existing.TieuDe = payload.TieuDe ?? lesson.TenBai;
                        existing.DoKho = payload.DoKho;
                        existing.DuongDanFileTxt = payload.DuongDanFileTxt;
                        existing.NoiDung = payload.NoiDung;

                        return ("reading", existing.MaBaiDoc ?? string.Empty);
                    }
                case "listening":
                    {
                        var payload = contentRequest.Listening ?? new ListeningContentRequest { TieuDe = lesson.TenBai };
                        var existing = await _context.BaiNghes.FirstOrDefaultAsync(b => b.MaBai == lesson.MaBai);
                        if (existing == null)
                        {
                            existing = new BaiNghe
                            {
                                MaBaiNghe = await GenerateNextListeningIdAsync(),
                                MaBai = lesson.MaBai,
                                NgayTao = DateTime.UtcNow
                            };
                            _context.BaiNghes.Add(existing);
                        }

                        existing.TieuDe = payload.TieuDe ?? lesson.TenBai;
                        existing.DoKho = payload.DoKho;
                        existing.DuongDanAudio = payload.DuongDanAudio;
                        existing.BanGhiAm = payload.BanGhiAm;

                        return ("listening", existing.MaBaiNghe ?? string.Empty);
                    }
                case "writing":
                    {
                        var payload = contentRequest.Writing ?? new WritingContentRequest { TieuDe = lesson.TenBai, DeBai = lesson.MoTa ?? lesson.TenBai };
                        var existing = await _context.BaiViets.FirstOrDefaultAsync(b => b.MaBai == lesson.MaBai);
                        if (existing == null)
                        {
                            existing = new BaiViet
                            {
                                MaBaiViet = await GenerateNextWritingIdAsync(),
                                MaBai = lesson.MaBai,
                                NgayTao = DateTime.UtcNow
                            };
                            _context.BaiViets.Add(existing);
                        }

                        existing.TieuDe = payload.TieuDe ?? lesson.TenBai;
                        existing.DeBai = payload.DeBai ?? lesson.MoTa ?? existing.DeBai;
                        existing.BaiMau = payload.BaiMau;
                        existing.SoTuToiThieu = payload.SoTuToiThieu;
                        existing.SoTuToiDa = payload.SoTuToiDa;

                        return ("writing", existing.MaBaiViet ?? string.Empty);
                    }
            }

            return null;
        }

        private async Task<object> BuildLessonSummaryAsync(string maBai)
        {
            var lesson = await _context.BaiHocs
                .Include(b => b.BaiDocs)
                .Include(b => b.BaiNghes)
                .Include(b => b.BaiViets)
                .Include(b => b.MaLoTrinhNavigation)
                .AsNoTracking()
                .FirstAsync(b => b.MaBai == maBai);

            return new
            {
                lesson.MaBai,
                lesson.MaLoTrinh,
                lesson.TenBai,
                lesson.MoTa,
                ThoiLuongPhut = lesson.ThoiLuongPhut ?? 0,
                lesson.SoThuTu,
                lesson.NgayTao,
                CapDo = lesson.MaLoTrinhNavigation?.CapDo,
                TenLoTrinh = lesson.MaLoTrinhNavigation?.TenLoTrinh,
                BaiDocs = lesson.BaiDocs.Select(d => new
                {
                    d.MaBaiDoc,
                    d.TieuDe,
                    d.DoKho,
                    d.DuongDanFileTxt,
                    d.NgayTao
                }).ToList(),
                BaiNghes = lesson.BaiNghes.Select(n => new
                {
                    n.MaBaiNghe,
                    n.TieuDe,
                    n.DoKho,
                    n.DuongDanAudio,
                    n.BanGhiAm,
                    n.NgayTao
                }).ToList(),
                BaiViets = lesson.BaiViets.Select(v => new
                {
                    v.MaBaiViet,
                    v.TieuDe,
                    v.DeBai,
                    v.BaiMau,
                    v.SoTuToiThieu,
                    v.SoTuToiDa,
                    v.NgayTao
                }).ToList()
            };
        }

        private async Task RemoveLessonTreeAsync(BaiHoc lesson)
        {
            var docIds = await _context.BaiDocs
                .Where(d => d.MaBai == lesson.MaBai)
                .Select(d => d.MaBaiDoc)
                .Where(id => id != null)
                .Cast<string>()
                .ToListAsync();

            if (docIds.Count > 0)
            {
                var docQuestionIds = await _context.CauHoiDocs
                    .Where(c => docIds.Contains(c.MaBaiDoc!))
                    .Select(c => c.MaCauHoi)
                    .Where(id => id != null)
                    .Cast<string>()
                    .ToListAsync();

                var docKetQua = _context.KetQuaBaiDocs.Where(k => docIds.Contains(k.MaBaiDoc!));
                var docTraLoi = _context.TraLoiHocVienDocs.Where(t => docQuestionIds.Contains(t.MaCauHoi!));

                _context.DapAnDocs.RemoveRange(_context.DapAnDocs.Where(d => docQuestionIds.Contains(d.MaCauHoi!)));
                _context.CauHoiDocs.RemoveRange(_context.CauHoiDocs.Where(c => docIds.Contains(c.MaBaiDoc!)));
                _context.TraLoiHocVienDocs.RemoveRange(docTraLoi);
                _context.KetQuaBaiDocs.RemoveRange(docKetQua);
                _context.BaiDocs.RemoveRange(_context.BaiDocs.Where(d => docIds.Contains(d.MaBaiDoc!)));
            }

            var ngheIds = await _context.BaiNghes
                .Where(n => n.MaBai == lesson.MaBai)
                .Select(n => n.MaBaiNghe)
                .Where(id => id != null)
                .Cast<string>()
                .ToListAsync();

            if (ngheIds.Count > 0)
            {
                var ngheQuestionIds = await _context.CauHoiNghes
                    .Where(c => ngheIds.Contains(c.MaBaiNghe!))
                    .Select(c => c.MaCauHoi)
                    .Where(id => id != null)
                    .Cast<string>()
                    .ToListAsync();

                _context.DapAnNghes.RemoveRange(_context.DapAnNghes.Where(d => ngheQuestionIds.Contains(d.MaCauHoi!)));
                _context.CauHoiNghes.RemoveRange(_context.CauHoiNghes.Where(c => ngheIds.Contains(c.MaBaiNghe!)));
                _context.TraLoiHocVienNghes.RemoveRange(_context.TraLoiHocVienNghes.Where(t => ngheQuestionIds.Contains(t.MaCauHoi!)));
                _context.KetQuaBaiNghes.RemoveRange(_context.KetQuaBaiNghes.Where(k => ngheIds.Contains(k.MaBaiNghe!)));
                _context.BaiNghes.RemoveRange(_context.BaiNghes.Where(n => ngheIds.Contains(n.MaBaiNghe!)));
            }

            var vietIds = await _context.BaiViets
                .Where(v => v.MaBai == lesson.MaBai)
                .Select(v => v.MaBaiViet)
                .Where(id => id != null)
                .Cast<string>()
                .ToListAsync();

            if (vietIds.Count > 0)
            {
                _context.BaiVietHocViens.RemoveRange(_context.BaiVietHocViens.Where(v => vietIds.Contains(v.MaBaiViet!)));
                _context.BaiViets.RemoveRange(_context.BaiViets.Where(v => vietIds.Contains(v.MaBaiViet!)));
            }

            _context.VideoBaiHocs.RemoveRange(_context.VideoBaiHocs.Where(v => v.MaBai == lesson.MaBai));
            _context.TienDoHocTaps.RemoveRange(_context.TienDoHocTaps.Where(t => t.MaBai == lesson.MaBai));

            _context.BaiHocs.Remove(lesson);
        }

        // ✅ LẤY DANH SÁCH TẤT CẢ BÀI HỌC
        [HttpGet]
        public async Task<IActionResult> GetAllBaiHoc()
        {
            try
            {
                var baiHocs = await _context.BaiHocs
                    .Select(b => new BaiHocDTO
                    {
                        MaBai = b.MaBai,
                        MaLoTrinh = b.MaLoTrinh,
                        TenBai = b.TenBai,
                        MoTa = b.MoTa,
                        ThoiLuongPhut = b.ThoiLuongPhut ?? 0,
                        SoThuTu = b.SoThuTu,
                        NgayTao = b.NgayTao
                    })
                    .OrderBy(b => b.SoThuTu)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Danh sách bài học",
                    total = baiHocs.Count,
                    data = baiHocs
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách bài học!", error = ex.Message });
            }
        }

        // ✅ LẤY CHI TIẾT BÀI HỌC
        [HttpGet("{maBai}")]
        public async Task<IActionResult> GetBaiHocDetail(string maBai)
        {
            try
            {
                // 1. LẤY THÔNG TIN BÀI HỌC CHÍNH
                var baiHoc = await _context.BaiHocs
                    .Where(b => b.MaBai == maBai)
                    .Select(b => new
                    {
                        b.MaBai,
                        b.MaLoTrinh,
                        b.TenBai,
                        b.MoTa,
                        ThoiLuongPhut = b.ThoiLuongPhut ?? 0,
                        b.SoThuTu,
                        b.NgayTao
                    })
                    .FirstOrDefaultAsync();

                if (baiHoc == null)
                    return NotFound(new { message = "Bài học không tồn tại!" });

                // 2. LẤY TẤT CẢ VIDEO LIÊN QUAN
                var videos = await _context.VideoBaiHocs
                    .Where(v => v.MaBai == maBai)
                    .Select(v => new VideoBaiHocDTO
                    {
                        MaVideo = v.MaVideo,
                        TieuDeVideo = v.TieuDeVideo,
                        DuongDanVideo = v.DuongDanVideo,
                        ThoiLuongGiay = v.ThoiLuongGiay,
                        NgayTao = v.NgayTao
                    })
                    .OrderBy(v => v.TieuDeVideo)
                    .ToListAsync();

                // 3. LẤY TẤT CẢ BÀI NGHE LIÊN QUAN
                var baiNghes = await _context.BaiNghes
                    .Where(bn => bn.MaBai == maBai)
                    .Select(bn => new BaiNgheDTO
                    {
                        MaBaiNghe = bn.MaBaiNghe,
                        MaBai = bn.MaBai,
                        TieuDe = bn.TieuDe,
                        DoKho = bn.DoKho,
                        NgayTao = bn.NgayTao,
                        DuongDanAudio = bn.DuongDanAudio,
                        BanGhiAm = bn.BanGhiAm
                    })
                    .OrderBy(bn => bn.TieuDe)
                    .ToListAsync();

                // 4. LẤY TẤT CẢ BÀI ĐỌC LIÊN QUAN
                var baiDocs = await _context.BaiDocs
                    .Where(bd => bd.MaBai == maBai)
                    .Select(bd => new BaiDocDTO
                    {
                        MaBaiDoc = bd.MaBaiDoc,
                        MaBai = bd.MaBai,
                        TieuDe = bd.TieuDe,
                        DoKho = bd.DoKho,
                        NgayTao = bd.NgayTao,
                        DuongDanFileTxt = bd.DuongDanFileTxt
                    })
                    .OrderBy(bd => bd.TieuDe)
                    .ToListAsync();

                var baiViets = await _context.BaiViets
                    .Where(bv => bv.MaBai == maBai)
                    .Select(bv => new
                    {
                        bv.MaBaiViet,
                        bv.MaBai,
                        bv.TieuDe,
                        bv.DeBai,
                        bv.BaiMau,
                        bv.SoTuToiThieu,
                        bv.SoTuToiDa,
                        bv.NgayTao
                    })
                    .OrderBy(bv => bv.TieuDe)
                    .ToListAsync();

                // 5. TRẢ VỀ KẾT QUẢ ĐẦY ĐỦ
                return Ok(new
                {
                    message = "Chi tiết bài học",
                    data = new
                    {
                        baiHoc.MaBai,
                        baiHoc.MaLoTrinh,
                        baiHoc.TenBai,
                        baiHoc.MoTa,
                        baiHoc.ThoiLuongPhut,
                        baiHoc.SoThuTu,
                        baiHoc.NgayTao,
                        Videos = videos,
                        BaiNghes = baiNghes,
                        BaiDocs = baiDocs,
                        BaiViets = baiViets
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy chi tiết bài học!", error = ex.Message });
            }
        }

        // ✅ NEW: LẤY DANH SÁCH BÀI HỌC THEO MÃ LỘ TRÌNH
        [HttpGet("lo-trinh/{maLoTrinh}")]
        public async Task<IActionResult> GetBaiHocByLoTrinh(string maLoTrinh)
        {
            try
            {
                var loTrinh = await _context.LoTrinhCoSans
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l => l.MaLoTrinh == maLoTrinh);

                if (loTrinh == null)
                    return NotFound(new { message = "Không tìm thấy lộ trình" });

                var lessons = await _context.BaiHocs
                    .Where(b => b.MaLoTrinh == maLoTrinh)
                    .OrderBy(b => b.SoThuTu)
                    .Select(b => new
                    {
                        b.MaBai,
                        b.MaLoTrinh,
                        b.TenBai,
                        b.MoTa,
                        ThoiLuongPhut = b.ThoiLuongPhut ?? 0,
                        b.SoThuTu,
                        b.NgayTao
                    })
                    .ToListAsync();

                if (!lessons.Any())
                {
                    return Ok(new
                    {
                        message = "Danh sách bài học theo lộ trình",
                        maLoTrinh = loTrinh.MaLoTrinh,
                        tenLoTrinh = loTrinh.TenLoTrinh,
                        kyNangTrongTam = loTrinh.KyNangTrongTam,
                        chuDeBaiHoc = loTrinh.ChuDeBaiHoc,
                        tongSoBaiHoc = 0,
                        data = new List<object>()
                    });
                }

                var lessonIds = lessons.Select(l => l.MaBai).Where(id => id != null).Cast<string>().ToList();

                var videos = await _context.VideoBaiHocs
                    .Where(v => lessonIds.Contains(v.MaBai))
                    .Select(v => new LessonVideo(
                        v.MaVideo,
                        v.MaBai,
                        v.TieuDeVideo,
                        v.DuongDanVideo,
                        v.ThoiLuongGiay,
                        v.NgayTao))
                    .ToListAsync();

                var docsRaw = await _context.BaiDocs
                    .Where(bd => lessonIds.Contains(bd.MaBai))
                    .Select(bd => new
                    {
                        bd.MaBaiDoc,
                        bd.MaBai,
                        bd.TieuDe,
                        bd.DoKho,
                        bd.DuongDanFileTxt,
                        bd.NgayTao,
                        SoCauHoi = bd.CauHoiDocs.Count
                    })
                    .ToListAsync();

                var ngheRaw = await _context.BaiNghes
                    .Where(bn => lessonIds.Contains(bn.MaBai))
                    .Select(bn => new
                    {
                        bn.MaBaiNghe,
                        bn.MaBai,
                        bn.TieuDe,
                        bn.DoKho,
                        bn.DuongDanAudio,
                        bn.BanGhiAm,
                        bn.NgayTao,
                        SoCauHoi = bn.CauHoiNghes.Count
                    })
                    .ToListAsync();

                var vietRaw = await _context.BaiViets
                    .Where(bv => lessonIds.Contains(bv.MaBai))
                    .Select(bv => new
                    {
                        bv.MaBaiViet,
                        bv.MaBai,
                        bv.TieuDe,
                        bv.DeBai,
                        bv.BaiMau,
                        bv.SoTuToiThieu,
                        bv.SoTuToiDa,
                        bv.NgayTao
                    })
                    .ToListAsync();

                var maNd = User?.Identity?.IsAuthenticated == true
                    ? User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value
                    : null;

                var completedDocIds = new HashSet<string>();
                var completedNgheIds = new HashSet<string>();
                var completedVietIds = new HashSet<string>();

                if (!string.IsNullOrEmpty(maNd))
                {
                    var docIds = docsRaw.Select(d => d.MaBaiDoc).Where(id => id != null).Cast<string>().ToList();
                    if (docIds.Count > 0)
                    {
                        completedDocIds = (await _context.KetQuaBaiDocs
                                .Where(k => k.MaNd == maNd && docIds.Contains(k.MaBaiDoc!))
                                .Select(k => k.MaBaiDoc!)
                                .Distinct()
                                .ToListAsync())
                            .ToHashSet();
                    }

                    var ngheIds = ngheRaw.Select(n => n.MaBaiNghe).Where(id => id != null).Cast<string>().ToList();
                    if (ngheIds.Count > 0)
                    {
                        completedNgheIds = (await _context.KetQuaBaiNghes
                                .Where(k => k.MaNd == maNd && ngheIds.Contains(k.MaBaiNghe!))
                                .Select(k => k.MaBaiNghe!)
                                .Distinct()
                                .ToListAsync())
                            .ToHashSet();
                    }

                    var vietIds = vietRaw.Select(v => v.MaBaiViet).Where(id => id != null).Cast<string>().ToList();
                    if (vietIds.Count > 0)
                    {
                        completedVietIds = (await _context.BaiVietHocViens
                                .Where(k => k.MaNd == maNd && vietIds.Contains(k.MaBaiViet!))
                                .Select(k => k.MaBaiViet!)
                                .Distinct()
                                .ToListAsync())
                            .ToHashSet();
                    }
                }

                var docs = docsRaw
                    .Select(d => new LessonDoc(
                        d.MaBaiDoc,
                        d.MaBai,
                        d.TieuDe,
                        d.DoKho,
                        d.DuongDanFileTxt,
                        d.NgayTao,
                        d.SoCauHoi,
                        d.MaBaiDoc != null && completedDocIds.Contains(d.MaBaiDoc)))
                    .ToList();

                var nghe = ngheRaw
                    .Select(n => new LessonNghe(
                        n.MaBaiNghe,
                        n.MaBai,
                        n.TieuDe,
                        n.DoKho,
                        n.DuongDanAudio,
                        n.BanGhiAm,
                        n.NgayTao,
                        n.SoCauHoi,
                        n.MaBaiNghe != null && completedNgheIds.Contains(n.MaBaiNghe)))
                    .ToList();

                var viet = vietRaw
                    .Select(v => new LessonViet(
                        v.MaBaiViet,
                        v.MaBai,
                        v.TieuDe,
                        v.DeBai,
                        v.BaiMau,
                        v.SoTuToiThieu,
                        v.SoTuToiDa,
                        v.NgayTao,
                        v.MaBaiViet != null && completedVietIds.Contains(v.MaBaiViet)))
                    .ToList();

                var videosByLesson = videos
                    .GroupBy(v => v.MaBai ?? string.Empty)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.TieuDeVideo).ToList());

                var docsByLesson = docs
                    .GroupBy(d => d.MaBai ?? string.Empty)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.TieuDe).ToList());

                var ngheByLesson = nghe
                    .GroupBy(n => n.MaBai ?? string.Empty)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.TieuDe).ToList());

                var vietByLesson = viet
                    .GroupBy(v => v.MaBai ?? string.Empty)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.TieuDe).ToList());

                var result = lessons.Select(lesson =>
                {
                    var key = lesson.MaBai ?? string.Empty;
                    var lessonVideos = videosByLesson.TryGetValue(key, out var vList) ? vList : new List<LessonVideo>();
                    var lessonDocs = docsByLesson.TryGetValue(key, out var dList) ? dList : new List<LessonDoc>();
                    var lessonNghes = ngheByLesson.TryGetValue(key, out var nList) ? nList : new List<LessonNghe>();
                    var lessonViets = vietByLesson.TryGetValue(key, out var wList) ? wList : new List<LessonViet>();

                    var totalContent = lessonDocs.Count + lessonNghes.Count + lessonViets.Count;
                    var completedContent = lessonDocs.Count(d => d.DaHoanThanh) + lessonNghes.Count(n => n.DaHoanThanh) + lessonViets.Count(v => v.DaHoanThanh);
                    var daHoanThanhBaiHoc = totalContent > 0 && completedContent >= totalContent;

                    return new
                    {
                        lesson.MaBai,
                        lesson.MaLoTrinh,
                        lesson.TenBai,
                        lesson.MoTa,
                        lesson.ThoiLuongPhut,
                        lesson.SoThuTu,
                        lesson.NgayTao,
                        Videos = lessonVideos,
                        BaiDocs = lessonDocs,
                        BaiNghes = lessonNghes,
                        BaiViets = lessonViets,
                        DaHoanThanhBaiHoc = daHoanThanhBaiHoc
                    };
                })
                .ToList();

                return Ok(new
                {
                    message = "Danh sách bài học theo lộ trình",
                    maLoTrinh = loTrinh.MaLoTrinh,
                    tenLoTrinh = loTrinh.TenLoTrinh,
                    kyNangTrongTam = loTrinh.KyNangTrongTam,
                    chuDeBaiHoc = loTrinh.ChuDeBaiHoc,
                    tongSoBaiHoc = lessons.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy bài học theo lộ trình!", error = ex.Message });
            }
        }


        // ========================= NEW ENDPOINTS =========================

        // 1) LẤY 1 BÀI HỌC + (BÀI ĐỌC, BÀI NGHE) + TRẠNG THÁI HOÀN THÀNH THEO USER
        [Authorize]
        [HttpGet("{maBai}/bai-doc-nghe")]
        public async Task<IActionResult> GetBaiHocWithDocsNghesAndStatus(string maBai)
        {
            try
            {
                var maNd = User.FindFirst("sub")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(maNd))
                    return Unauthorized(new { message = "Không tìm thấy người dùng" });

                var baiHoc = await _context.BaiHocs
                    .Where(b => b.MaBai == maBai)
                    .Select(b => new
                    {
                        b.MaBai,
                        b.MaLoTrinh,
                        b.TenBai,
                        b.MoTa,
                        ThoiLuongPhut = b.ThoiLuongPhut ?? 0,
                        b.SoThuTu,
                        b.NgayTao
                    })
                    .FirstOrDefaultAsync();

                if (baiHoc == null)
                    return NotFound(new { message = "Bài học không tồn tại!" });

                var baiDocs = await _context.BaiDocs
                    .Where(bd => bd.MaBai == maBai)
                    .Select(bd => new
                    {
                        bd.MaBaiDoc,
                        bd.MaBai,
                        bd.TieuDe,
                        bd.DoKho,
                        bd.DuongDanFileTxt,
                        bd.NgayTao,
                        SoCauHoi = bd.CauHoiDocs.Count,
                        DaHoanThanh = _context.KetQuaBaiDocs
                            .Any(k => k.MaBaiDoc == bd.MaBaiDoc && k.MaNd == maNd)
                    })
                    .OrderBy(d => d.TieuDe)
                    .ToListAsync();

                var baiNghes = await _context.BaiNghes
                    .Where(bn => bn.MaBai == maBai)
                    .Select(bn => new
                    {
                        bn.MaBaiNghe,
                        bn.MaBai,
                        bn.TieuDe,
                        bn.DoKho,
                        bn.DuongDanAudio,
                        bn.BanGhiAm,
                        bn.NgayTao,
                        SoCauHoi = bn.CauHoiNghes.Count,
                        DaHoanThanh = _context.KetQuaBaiNghes
                            .Any(k => k.MaBaiNghe == bn.MaBaiNghe && k.MaNd == maNd)
                    })
                    .OrderBy(n => n.TieuDe)
                    .ToListAsync();

                var baiViets = await _context.BaiViets
                    .Where(bv => bv.MaBai == maBai)
                    .Select(bv => new
                    {
                        bv.MaBaiViet,
                        bv.MaBai,
                        bv.TieuDe,
                        bv.DeBai,
                        bv.BaiMau,
                        bv.SoTuToiThieu,
                        bv.SoTuToiDa,
                        bv.NgayTao,
                        DaHoanThanh = _context.BaiVietHocViens
                            .Any(k => k.MaBaiViet == bv.MaBaiViet && k.MaNd == maNd)
                    })
                    .OrderBy(v => v.TieuDe)
                    .ToListAsync();

                var tongNoiDung = baiDocs.Count + baiNghes.Count + baiViets.Count;
                var daHoanThanh = baiDocs.Count(d => d.DaHoanThanh) + baiNghes.Count(n => n.DaHoanThanh) + baiViets.Count(v => v.DaHoanThanh);
                var daHoanThanhBaiHoc = tongNoiDung > 0 && daHoanThanh == tongNoiDung;

                return Ok(new
                {
                    message = "Chi tiết bài học + trạng thái hoàn thành",
                    data = new
                    {
                        BaiHoc = baiHoc,
                        BaiDocs = baiDocs,
                        BaiNghes = baiNghes,
                        BaiViets = baiViets,
                        TongNoiDung = tongNoiDung,
                        HoanThanh = daHoanThanh,
                        DaHoanThanhBaiHoc = daHoanThanhBaiHoc
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu!", error = ex.Message });
            }
        }

        // 2) LẤY TẤT CẢ BÀI HỌC (BÀI ĐỌC, BÀI NGHE) + TRẠNG THÁI THEO USER
        [Authorize]
        [HttpGet("tat-ca-bai-hoc-noi-dung")]
        public async Task<IActionResult> GetAllLessonsWithDocsNghesAndStatus()
        {
            try
            {
                var maNd = User.FindFirst("sub")?.Value
                    ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(maNd))
                    return Unauthorized(new { message = "Không tìm thấy người dùng" });

                // Lấy tất cả bài học
                var lessons = await _context.BaiHocs
                    .Select(b => new
                    {
                        b.MaBai,
                        b.MaLoTrinh,
                        b.TenBai,
                        b.MoTa,
                        ThoiLuongPhut = b.ThoiLuongPhut ?? 0,
                        b.SoThuTu,
                        b.NgayTao
                    })
                    .OrderBy(b => b.MaLoTrinh)
                    .ThenBy(b => b.SoThuTu)
                    .ToListAsync();

                if (!lessons.Any())
                    return Ok(new { message = "Chưa có bài học nào", total = 0, data = new object[0] });

                var maBaiList = lessons.Select(l => l.MaBai).ToList();

                // Lấy tất cả bài đọc thuộc các bài học + trạng thái hoàn thành theo user
                var allDocs = await _context.BaiDocs
                    .Where(bd => maBaiList.Contains(bd.MaBai))
                    .Select(bd => new
                    {
                        bd.MaBaiDoc,
                        bd.MaBai,
                        bd.TieuDe,
                        bd.DoKho,
                        bd.DuongDanFileTxt,
                        bd.NgayTao,
                        SoCauHoi = bd.CauHoiDocs.Count,
                        DaHoanThanh = _context.KetQuaBaiDocs
                            .Any(k => k.MaBaiDoc == bd.MaBaiDoc && k.MaNd == maNd)
                    })
                    .ToListAsync();

                // Lấy tất cả bài nghe thuộc các bài học + trạng thái hoàn thành theo user
                var allNghes = await _context.BaiNghes
                    .Where(bn => maBaiList.Contains(bn.MaBai))
                    .Select(bn => new
                    {
                        bn.MaBaiNghe,
                        bn.MaBai,
                        bn.TieuDe,
                        bn.DoKho,
                        bn.DuongDanAudio,
                        bn.BanGhiAm,
                        bn.NgayTao,
                        SoCauHoi = bn.CauHoiNghes.Count,
                        DaHoanThanh = _context.KetQuaBaiNghes
                            .Any(k => k.MaBaiNghe == bn.MaBaiNghe && k.MaNd == maNd)
                    })
                    .ToListAsync();

                var allViets = await _context.BaiViets
                    .Where(bv => maBaiList.Contains(bv.MaBai))
                    .Select(bv => new
                    {
                        bv.MaBaiViet,
                        bv.MaBai,
                        bv.TieuDe,
                        bv.DeBai,
                        bv.BaiMau,
                        bv.SoTuToiThieu,
                        bv.SoTuToiDa,
                        bv.NgayTao,
                        DaHoanThanh = _context.BaiVietHocViens
                            .Any(k => k.MaBaiViet == bv.MaBaiViet && k.MaNd == maNd)
                    })
                    .ToListAsync();

                // Gộp theo bài học
                var result = lessons.Select(b =>
                {
                    var docs = allDocs.Where(d => d.MaBai == b.MaBai).OrderBy(d => d.TieuDe).ToList();
                    var nghes = allNghes.Where(n => n.MaBai == b.MaBai).OrderBy(n => n.TieuDe).ToList();
                    var viets = allViets.Where(v => v.MaBai == b.MaBai).OrderBy(v => v.TieuDe).ToList();

                    var tong = docs.Count + nghes.Count + viets.Count;
                    var done = docs.Count(d => d.DaHoanThanh) + nghes.Count(n => n.DaHoanThanh) + viets.Count(v => v.DaHoanThanh);
                    var doneBaiHoc = tong > 0 && done == tong;

                    return new
                    {
                        b.MaBai,
                        b.MaLoTrinh,
                        b.TenBai,
                        b.MoTa,
                        b.ThoiLuongPhut,
                        b.SoThuTu,
                        b.NgayTao,
                        TongNoiDung = tong,
                        HoanThanh = done,
                        DaHoanThanhBaiHoc = doneBaiHoc,
                        BaiDocs = docs,
                        BaiNghes = nghes,
                        BaiViets = viets
                    };
                })
                .OrderBy(x => x.MaLoTrinh)
                .ThenBy(x => x.SoThuTu)
                .ToList();

                return Ok(new
                {
                    message = "Danh sách tất cả bài học kèm bài đọc, bài nghe và trạng thái",
                    total = result.Count,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy dữ liệu!", error = ex.Message });
            }
        }

        [Authorize]
        [HttpPost("admin")]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!CurrentUserIsAdmin())
            {
                return Forbid();
            }

            var loTrinh = await _context.LoTrinhCoSans.FindAsync(request.MaLoTrinh);
            if (loTrinh == null)
            {
                return NotFound(new { message = "Không tìm thấy lộ trình" });
            }

            var lesson = new BaiHoc
            {
                MaBai = await GenerateNextLessonIdAsync(),
                MaLoTrinh = request.MaLoTrinh,
                TenBai = request.TenBai.Trim(),
                MoTa = request.MoTa,
                ThoiLuongPhut = request.ThoiLuongPhut,
                SoThuTu = request.SoThuTu ?? await GetNextLessonOrderAsync(request.MaLoTrinh),
                NgayTao = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            };

            _context.BaiHocs.Add(lesson);
            await UpsertLessonContentAsync(lesson, request.Content);
            await _context.SaveChangesAsync();

            var summary = await BuildLessonSummaryAsync(lesson.MaBai);
            return Ok(new { message = "Tạo bài học thành công", data = summary });
        }

        [Authorize]
        [HttpPut("admin/{maBai}")]
        public async Task<IActionResult> UpdateLesson(string maBai, [FromBody] UpdateLessonRequest request)
        {
            if (!CurrentUserIsAdmin())
            {
                return Forbid();
            }

            var lesson = await _context.BaiHocs.FirstOrDefaultAsync(b => b.MaBai == maBai);
            if (lesson == null)
            {
                return NotFound(new { message = "Bài học không tồn tại" });
            }

            if (!string.IsNullOrWhiteSpace(request.TenBai))
            {
                lesson.TenBai = request.TenBai.Trim();
            }

            if (request.MoTa != null)
            {
                lesson.MoTa = request.MoTa;
            }

            if (request.ThoiLuongPhut.HasValue)
            {
                lesson.ThoiLuongPhut = request.ThoiLuongPhut.Value;
            }

            if (request.SoThuTu.HasValue)
            {
                lesson.SoThuTu = request.SoThuTu.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.MaLoTrinh))
            {
                var loTrinh = await _context.LoTrinhCoSans.FindAsync(request.MaLoTrinh);
                if (loTrinh == null)
                {
                    return NotFound(new { message = "Không tìm thấy lộ trình" });
                }
                lesson.MaLoTrinh = request.MaLoTrinh;
            }

            await UpsertLessonContentAsync(lesson, request.Content);
            await _context.SaveChangesAsync();

            var summary = await BuildLessonSummaryAsync(lesson.MaBai);
            return Ok(new { message = "Cập nhật bài học thành công", data = summary });
        }

        [Authorize]
        [HttpDelete("admin/{maBai}")]
        public async Task<IActionResult> DeleteLesson(string maBai)
        {
            if (!CurrentUserIsAdmin())
            {
                return Forbid();
            }

            var lesson = await _context.BaiHocs.FirstOrDefaultAsync(b => b.MaBai == maBai);
            if (lesson == null)
            {
                return NotFound(new { message = "Bài học không tồn tại" });
            }

            await RemoveLessonTreeAsync(lesson);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xóa bài học" });
        }




    }
}