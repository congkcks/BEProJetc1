using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using TOEICWEB.Data;
using TOEICWEB.Models;
using TOEICWEB.ViewModels;
using System.Security.Claims;

namespace ToeicWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SupabaseDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(SupabaseDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        // 🔹 Tạo JWT Token
        private string GenerateJwtToken(NguoiDung user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "your-secret-key-here-minimum-32-characters-long-for-security"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.MaNd),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.HoTen),
                new Claim("VaiTro", user.VaiTro ?? "User")
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"] ?? "toeic-app",
                audience: _configuration["Jwt:Audience"] ?? "toeic-users",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    // ✅ ĐĂNG KÝ NGƯỜI DÙNG
    [HttpPost("register")]
    [HttpPost("dang-ky")]
        public async Task<IActionResult> Register([FromBody] RegisterVM model)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.MatKhau) ||
                    string.IsNullOrWhiteSpace(model.HoTen))
                    return BadRequest(new { message = "Email, mật khẩu và họ tên là bắt buộc!" });

                // Kiểm tra định dạng email
                if (!model.Email.Contains("@"))
                    return BadRequest(new { message = "Email không hợp lệ!" });

                // Kiểm tra độ dài mật khẩu
                if (model.MatKhau.Length < 6)
                    return BadRequest(new { message = "Mật khẩu phải ít nhất 6 ký tự!" });

                // Kiểm tra email đã tồn tại
                if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
                    return BadRequest(new { message = "Email đã tồn tại trong hệ thống!" });

                // Tạo mã ND mới
                var lastUser = await _context.NguoiDungs
                    .Where(u => u.MaNd.StartsWith("ND"))
                    .OrderByDescending(u => u.MaNd)
                    .FirstOrDefaultAsync();

                string newMaNd;
                if (lastUser == null)
                    newMaNd = "ND001";
                else
                {
                    int soCu = int.Parse(lastUser.MaNd.Substring(2));
                    newMaNd = "ND" + (soCu + 1).ToString("D3");
                }

                // Tạo người dùng mới (Không hash, trigger sẽ hash bằng MD5)
                var nguoiDung = new NguoiDung
                {
                    MaNd = newMaNd,
                    Email = model.Email,
                    MatKhau = model.MatKhau, // Gửi plain text, trigger sẽ hash
                    HoTen = model.HoTen,
                    VaiTro = "User", // Mặc định là User
                    NgayDangKy = DateTime.Now,
                    AnhDaiDien = null,
                    LanDangNhapCuoi = null
                };

                _context.NguoiDungs.Add(nguoiDung);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Đăng ký thành công!",
                    ma_nd = newMaNd,
                    email = model.Email,
                    ho_ten = model.HoTen
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đăng ký!", error = ex.Message });
            }
        }

    // ✅ ĐĂNG NHẬP
    [HttpPost("login")]
    [HttpPost("dang-nhap")]
        public async Task<IActionResult> Login([FromBody] LoginVM model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.MatKhau))
                    return BadRequest(new { message = "Email và mật khẩu là bắt buộc!" });

                var user = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                    return Unauthorized(new { message = "Sai email hoặc mật khẩu!" });

                string inputPasswordHash = GetMD5Hash(model.MatKhau);

                // Debug: In ra để kiểm tra
                Console.WriteLine($"Email: {model.Email}");
                Console.WriteLine($"Input MD5: {inputPasswordHash}");
                Console.WriteLine($"DB MD5: {user.MatKhau}");
                Console.WriteLine($"Match: {user.MatKhau == inputPasswordHash}");

                if (user.MatKhau != inputPasswordHash)
                    return Unauthorized(new { message = "Sai email hoặc mật khẩu!" });

                var token = GenerateJwtToken(user);

                return Ok(new
                {
                    message = "Đăng nhập thành công!",
                    token = token,
                    user = new
                    {
                        ma_nd = user.MaNd,
                        ho_ten = user.HoTen,
                        email = user.Email,
                        vai_tro = user.VaiTro,
                        ngay_dang_ky = user.NgayDangKy,
                        anh_dai_dien = user.AnhDaiDien,
                        lan_dang_nhap_cuoi = user.LanDangNhapCuoi
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi đăng nhập!", error = ex.Message });
            }
        }

        // 🔹 Tính MD5 Hash (khớp với trigger database)
        private string GetMD5Hash(string password)
        {
            using var md5 = System.Security.Cryptography.MD5.Create();
            var bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // ✅ ADMIN: ĐĂNG KÝ ADMIN MỚI (Chỉ admin hiện tại có quyền)
        [Authorize]
        [HttpPost("admin/register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterVM model)
        {
            try
            {
                // Kiểm tra quyền admin
                var currentUserRole = User.FindFirst("VaiTro")?.Value;
                if (currentUserRole != "Admin")
                    return Forbid();

                // Validate input
                if (string.IsNullOrWhiteSpace(model.Email) ||
                    string.IsNullOrWhiteSpace(model.MatKhau) ||
                    string.IsNullOrWhiteSpace(model.HoTen))
                    return BadRequest(new { message = "Email, mật khẩu và họ tên là bắt buộc!" });

                if (model.MatKhau.Length < 6)
                    return BadRequest(new { message = "Mật khẩu phải ít nhất 6 ký tự!" });

                if (await _context.NguoiDungs.AnyAsync(u => u.Email == model.Email))
                    return BadRequest(new { message = "Email đã tồn tại!" });

                // Tạo mã ADM mới
                var lastAdmin = await _context.NguoiDungs
                    .Where(u => u.MaNd.StartsWith("ADM"))
                    .OrderByDescending(u => u.MaNd)
                    .FirstOrDefaultAsync();

                string newMaAdm;
                if (lastAdmin == null)
                    newMaAdm = "ADM001";
                else
                {
                    int soCu = int.Parse(lastAdmin.MaNd.Substring(3));
                    newMaAdm = "ADM" + (soCu + 1).ToString("D3");
                }

                var admin = new NguoiDung
                {
                    MaNd = newMaAdm,
                    Email = model.Email,
                    MatKhau = model.MatKhau, // Gửi plain text, trigger sẽ hash
                    HoTen = model.HoTen,
                    VaiTro = "Admin",
                    NgayDangKy = DateTime.Now,
                    AnhDaiDien = null,
                    LanDangNhapCuoi = null
                };

                _context.NguoiDungs.Add(admin);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Tạo admin mới thành công!",
                    ma_adm = newMaAdm,
                    email = model.Email,
                    ho_ten = model.HoTen
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tạo admin!", error = ex.Message });
            }
        }

        // ✅ ADMIN: KHÓA/MỞ KHÓA TÀI KHOẢN
       
        [Authorize]
        [HttpGet("admin/users")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var currentUserRole = User.FindFirst("VaiTro")?.Value;
                if (currentUserRole != "Admin")
                    return Forbid();

                var users = await _context.NguoiDungs
                    .Select(u => new
                    {
                        u.MaNd,
                        u.Email,
                        u.HoTen,
                        u.VaiTro,
                        u.NgayDangKy,
                        u.AnhDaiDien,
                        u.LanDangNhapCuoi
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Danh sách người dùng",
                    total = users.Count,
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi lấy danh sách!", error = ex.Message });
            }
        }

        // ✅ TEST KẾT NỐI DATABASE
        [HttpGet("testdb")]
        public async Task<IActionResult> TestDB()
        {
            try
            {
                var count = await _context.NguoiDungs.CountAsync();
                return Ok(new
                {
                    message = "✅ Kết nối thành công!",
                    total_users = count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "❌ Kết nối thất bại!",
                    error = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }

        // ✅ ĐỔI MẬT KHẨU (Hỗ trợ các đường dẫn /Auth/change-password và /Auth/doi-mat-khau)
        [Authorize]
        [HttpPost("doi-mat-khau")]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordVM model)
        {
            try
            {
                if (model == null)
                    return BadRequest(new { message = "Thiếu dữ liệu." });

                string FirstNonEmpty(params string?[] values)
                {
                    foreach (var v in values)
                    {
                        if (!string.IsNullOrWhiteSpace(v)) return v!.Trim();
                    }
                    return string.Empty;
                }

                var currentPassword = FirstNonEmpty(model.MatKhauHienTai, model.MatKhauCu, model.CurrentPassword);
                var newPassword = FirstNonEmpty(model.MatKhauMoi, model.NewPassword);
                var confirmPassword = FirstNonEmpty(model.XacNhanMatKhauMoi, model.ConfirmPassword, model.MatKhauMoi, model.NewPassword);

                if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                {
                    return BadRequest(new { message = "Mat khau hien tai va mat khau moi la bat buoc." });
                }

                if (!string.Equals(newPassword, confirmPassword, StringComparison.Ordinal))
                {
                    return BadRequest(new { message = "Mat khau xac nhan khong khop." });
                }

                if (newPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mat khau moi phai it nhat 6 ky tu." });
                }

                var maNdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrWhiteSpace(maNdFromToken))
                {
                    return Unauthorized(new { message = "Phien lam viec khong hop le." });
                }

                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNd == maNdFromToken);
                if (user == null)
                {
                    return NotFound(new { message = "Nguoi dung khong ton tai!" });
                }

                var currentHash = GetMD5Hash(currentPassword);
                if (!string.Equals(user.MatKhau, currentHash, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { message = "Mat khau hien tai khong chinh xac." });
                }

                if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
                {
                    return BadRequest(new { message = "Mat khau moi phai khac mat khau hien tai." });
                }

                // DB trigger trg_ma_hoa_mat_khau hashes passwords, so store raw value here to avoid double hashing.
                user.MatKhau = newPassword;
                await _context.SaveChangesAsync();
                await _context.Entry(user).ReloadAsync();

                return Ok(new { message = "Doi mat khau thanh cong!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Loi khi doi mat khau!", error = ex.Message });
            }
    }

        // ✅ KIỂM TRA TOKEN & TRẢ VỀ CLAIMS (Debug hỗ trợ lỗi 401 Dashboard)
        [Authorize]
        [HttpGet("validate")]
        public IActionResult ValidateToken()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var nameId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst("VaiTro")?.Value;
            return Ok(new
            {
                message = "Token hợp lệ",
                nameId,
                email,
                role,
                claims
            });
        }
    }
}