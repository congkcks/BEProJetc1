using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOEICWEB.Data;
using TOEICWEB.Models;
using TOEICWEB.ViewModels;
using System.Security.Claims;

namespace ToeicWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TienDoHocTapController : ControllerBase
    {
        private readonly SupabaseDbContext _context;

        public TienDoHocTapController(SupabaseDbContext context)
        {
            _context = context;
        }

        // ✅ LẤY TIẾN ĐỘ HỌC TẬP CỦA NGƯỜI DÙNG
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllTienDoHocTap()
        {
            try
            {
                var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNd))
                    return Unauthorized(new { message = "Không tìm thấy mã người dùng trong token!" });

                var tienDos = await _context.TienDoHocTaps
                    .Where(t => t.MaNd == maNd)
                    .Join(_context.BaiHocs,
                        t => t.MaBai,
                        b => b.MaBai,
                        (t, b) => new TienDoHocTapDTO
                        {
                            MaTienDo = t.MaTienDo,
                            MaBai = t.MaBai ?? string.Empty,
                            TenBai = b.TenBai ?? string.Empty,
                            MaLoTrinh = b.MaLoTrinh,
                            TrangThai = t.TrangThai ?? string.Empty,
                            NgayHoanThanh = t.NgayHoanThanh,
                            NgayCapNhat = t.NgayCapNhat,
                            ThoiGianHocPhut = t.ThoiGianHocPhut ?? 0,
                            PhanTramHoanThanh = t.PhanTramHoanThanh ?? 0
                        })
                    .OrderBy(t => t.NgayCapNhat)
                    .ToListAsync();

                if (!tienDos.Any())
                    return Ok(new
                    {
                        message = "Bạn chưa có tiến độ học tập nào",
                        total = 0,
                        data = new List<object>()
                    });

                return Ok(new
                {
                    message = "Danh sách tiến độ học tập",
                    total = tienDos.Count,
                    data = tienDos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy tiến độ học tập!", error = ex.Message });
            }
        }

        // ✅ LẤY TIẾN ĐỘ HỌC TẬP CHO MỘT BÀI HỌC CỤ THỂ
        [Authorize]
        [HttpGet("bai/{maBai}")]
        public async Task<IActionResult> GetTienDoHocTapByBaiHoc(string maBai)
        {
            try
            {
                var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNd))
                    return Unauthorized(new { message = "Không tìm thấy mã người dùng trong token!" });

                var baiHoc = await _context.BaiHocs.FirstOrDefaultAsync(b => b.MaBai == maBai);
                if (baiHoc == null)
                    return NotFound(new { message = "Bài học không tồn tại!" });

                var tienDo = await _context.TienDoHocTaps
                    .Where(t => t.MaNd == maNd && t.MaBai == maBai)
                    .Select(t => new TienDoHocTapDTO
                    {
                        MaTienDo = t.MaTienDo,
                        MaBai = t.MaBai ?? string.Empty,
                        TenBai = baiHoc.TenBai ?? string.Empty,
                        MaLoTrinh = baiHoc.MaLoTrinh,
                        TrangThai = t.TrangThai ?? string.Empty,
                        NgayHoanThanh = t.NgayHoanThanh,
                        NgayCapNhat = t.NgayCapNhat,
                        ThoiGianHocPhut = t.ThoiGianHocPhut ?? 0,
                        PhanTramHoanThanh = t.PhanTramHoanThanh ?? 0
                    })
                    .FirstOrDefaultAsync();

                if (tienDo == null)
                    return Ok(new
                    {
                        message = $"Bạn chưa bắt đầu bài học '{baiHoc.TenBai}'",
                        data = new { }
                    });

                return Ok(new
                {
                    message = $"Tiến độ học tập cho bài học '{baiHoc.TenBai}'",
                    data = tienDo
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy tiến độ học tập!", error = ex.Message });
            }
        }

        // ✅ LẤY TIẾN ĐỘ HỌC TẬP THEO LỘ TRÌNH HỌC TẬP
        [Authorize]
        [HttpGet("lotrinh/{maLoTrinh}")]
        public async Task<IActionResult> GetTienDoHocTapByLoTrinh(string maLoTrinh)
        {
            try
            {
                var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNd))
                    return Unauthorized(new { message = "Không tìm thấy mã người dùng trong token!" });

                var tienDos = await _context.TienDoHocTaps
                    .Join(_context.BaiHocs,
                        t => t.MaBai,
                        b => b.MaBai,
                        (t, b) => new { TienDo = t, BaiHoc = b })
                    .Where(joined => joined.TienDo.MaNd == maNd && joined.BaiHoc.MaLoTrinh == maLoTrinh)
                    .Select(joined => new TienDoHocTapDTO
                    {
                        MaTienDo = joined.TienDo.MaTienDo,
                        MaBai = joined.TienDo.MaBai,
                        TenBai = joined.BaiHoc.TenBai,
                        MaLoTrinh = joined.BaiHoc.MaLoTrinh,
                        TrangThai = joined.TienDo.TrangThai,
                        NgayHoanThanh = joined.TienDo.NgayHoanThanh,
                        NgayCapNhat = joined.TienDo.NgayCapNhat,
                        ThoiGianHocPhut = joined.TienDo.ThoiGianHocPhut ?? 0,
                        PhanTramHoanThanh = joined.TienDo.PhanTramHoanThanh ?? 0
                    })
                    .OrderBy(t => t.NgayCapNhat)
                    .ToListAsync();

                if (!tienDos.Any())
                    return Ok(new
                    {
                        message = $"Bạn chưa có tiến độ học tập nào cho lộ trình '{maLoTrinh}'",
                        total = 0,
                        data = new List<object>()
                    });

                return Ok(new
                {
                    message = $"Tiến độ học tập cho lộ trình '{maLoTrinh}'",
                    total = tienDos.Count,
                    data = tienDos
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy tiến độ học tập theo lộ trình!", error = ex.Message });
            }
        }

        // ✅ CẬP NHẬT TIẾN ĐỘ HỌC TẬP
        [Authorize]
        [HttpPost("update/{maBai}")]
        public async Task<IActionResult> UpdateTienDoHocTap(string maBai, [FromBody] UpdateTienDoHocTapVM model)
        {
            try
            {
                var maNd = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(maNd))
                    return Unauthorized(new { message = "Không tìm thấy mã người dùng trong token!" });

                // Validate input
                if (model.PhanTramHoanThanh.HasValue && (model.PhanTramHoanThanh < 0 || model.PhanTramHoanThanh > 100))
                    return BadRequest(new { message = "Phần trăm hoàn thành phải nằm trong khoảng 0-100!" });
                if (model.ThoiGianHocPhut.HasValue && model.ThoiGianHocPhut < 0)
                    return BadRequest(new { message = "Thời gian học không được âm!" });
                if (!string.IsNullOrEmpty(model.TrangThai) &&
                    !new[] { "Chưa bắt đầu", "Đang học", "Hoàn thành" }.Contains(model.TrangThai))
                    return BadRequest(new { message = "Trạng thái không hợp lệ!" });

                // Kiểm tra bài học
                var baiHoc = await _context.BaiHocs.FirstOrDefaultAsync(b => b.MaBai == maBai);
                if (baiHoc == null)
                    return NotFound(new { message = "Bài học không tồn tại!" });

                // Kiểm tra tiến độ hiện tại
                var tienDo = await _context.TienDoHocTaps
                    .FirstOrDefaultAsync(t => t.MaNd == maNd && t.MaBai == maBai);

                if (tienDo == null)
                {
                    // Tạo mới tiến độ
                    tienDo = new TienDoHocTap
                    {
                        MaNd = maNd,
                        MaBai = maBai,
                        TrangThai = model.TrangThai ?? "Chưa bắt đầu",
                        ThoiGianHocPhut = model.ThoiGianHocPhut ?? 0,
                        PhanTramHoanThanh = model.PhanTramHoanThanh ?? 0,
                        NgayCapNhat = DateTime.Now,
                        NgayHoanThanh = model.TrangThai == "Hoàn thành" ? DateTime.Now : null
                    };
                    _context.TienDoHocTaps.Add(tienDo);
                }
                else
                {
                    // Cập nhật tiến độ
                    tienDo.TrangThai = model.TrangThai ?? tienDo.TrangThai;
                    tienDo.ThoiGianHocPhut = model.ThoiGianHocPhut ?? tienDo.ThoiGianHocPhut;
                    tienDo.PhanTramHoanThanh = model.PhanTramHoanThanh ?? tienDo.PhanTramHoanThanh;
                    tienDo.NgayCapNhat = DateTime.Now;
                    tienDo.NgayHoanThanh = model.TrangThai == "Hoàn thành" ? DateTime.Now : tienDo.NgayHoanThanh;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật tiến độ học tập thành công!",
                    data = new
                    {
                        maTienDo = tienDo.MaTienDo,
                        maBai = tienDo.MaBai,
                        tenBai = baiHoc.TenBai,
                        maLoTrinh = baiHoc.MaLoTrinh,
                        trangThai = tienDo.TrangThai,
                        thoiGianHocPhut = tienDo.ThoiGianHocPhut,
                        phanTramHoanThanh = tienDo.PhanTramHoanThanh,
                        ngayCapNhat = tienDo.NgayCapNhat,
                        ngayHoanThanh = tienDo.NgayHoanThanh
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi cập nhật tiến độ học tập!", error = ex.Message });
            }
        }
    }
}