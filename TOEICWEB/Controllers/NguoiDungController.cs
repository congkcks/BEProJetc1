using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TOEICWEB.Data;
using TOEICWEB.Models;
using TOEICWEB.ViewModels;
using TOEICWEB.ViewModels.Admin;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ToeicWeb.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NguoiDungController : ControllerBase
{
    private readonly SupabaseDbContext _context;

    public NguoiDungController(SupabaseDbContext context)
    {
        _context = context;
    }

    private bool CurrentUserIsAdmin()
    {
        var role = User.FindFirst("VaiTro")?.Value;
        return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> GenerateNextUserIdAsync(string? role = null)
    {
        var prefix = "ND";
        if (!string.IsNullOrWhiteSpace(role) && string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            prefix = "ADM";
        }

        var lastUser = await _context.NguoiDungs
            .AsNoTracking()
            .Where(u => u.MaNd.StartsWith(prefix))
            .OrderByDescending(u => u.MaNd)
            .FirstOrDefaultAsync();

        var nextNumber = 1;
        if (lastUser != null)
        {
            var numericPart = lastUser.MaNd.Length > prefix.Length
                ? lastUser.MaNd[prefix.Length..]
                : "0";

            if (int.TryParse(numericPart, out var currentNumber))
            {
                nextNumber = currentNumber + 1;
            }
        }

        while (true)
        {
            var candidate = $"{prefix}{nextNumber:D3}";
            var exists = await _context.NguoiDungs.AnyAsync(u => u.MaNd == candidate);
            if (!exists)
            {
                return candidate;
            }

            nextNumber++;
        }
    }

    private static string ComputeMd5(string input)
    {
        using var md5 = MD5.Create();
        var data = Encoding.UTF8.GetBytes(input ?? string.Empty);
        var hashBytes = md5.ComputeHash(data);
        var builder = new StringBuilder(hashBytes.Length * 2);
        foreach (var b in hashBytes)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }

    // Preflight handlers to avoid 401 on CORS OPTIONS requests
    [AllowAnonymous]
    [HttpOptions("profile")]
    [HttpOptions("cap-nhat")]
    public IActionResult Options()
    {
        Response.Headers.Append("Allow", "OPTIONS, GET, PUT, PATCH, POST");
        return Ok();
    }

    [Authorize]
    [HttpGet("next-id")]
    public async Task<IActionResult> GetNextUserId([FromQuery] string? role = null)
    {
        if (!CurrentUserIsAdmin())
        {
            return Forbid();
        }

        var nextId = await GenerateNextUserIdAsync(role);
        return Ok(new { maNd = nextId });
    }

    [Authorize]
    [HttpGet]
    [HttpGet("danh-sach")]
    public async Task<IActionResult> GetUsers([FromQuery] string? search = null)
    {
        if (!CurrentUserIsAdmin())
        {
            return Forbid();
        }

        var query = _context.NguoiDungs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim().ToLowerInvariant();
            query = query.Where(u =>
                (u.HoTen != null && u.HoTen.ToLower().Contains(keyword)) ||
                (u.Email != null && u.Email.ToLower().Contains(keyword)) ||
                (u.MaNd != null && u.MaNd.ToLower().Contains(keyword)) ||
                (u.SoDienThoai != null && u.SoDienThoai.ToLower().Contains(keyword)) ||
                (u.VaiTro != null && u.VaiTro.ToLower().Contains(keyword)));
        }

        var users = await query
            .OrderByDescending(u => u.NgayDangKy ?? DateTime.MinValue)
            .ThenBy(u => u.MaNd)
            .Select(u => new AdminUserVM
            {
                MaNd = u.MaNd,
                Email = u.Email,
                HoTen = u.HoTen,
                MatKhau = u.MatKhau,
                VaiTro = u.VaiTro,
                SoDienThoai = u.SoDienThoai,
                NgayDangKy = u.NgayDangKy,
                LanDangNhapCuoi = u.LanDangNhapCuoi,
                AnhDaiDien = u.AnhDaiDien
            })
            .ToListAsync();

        return Ok(new
        {
            message = "Danh sách người dùng",
            total = users.Count,
            data = users
        });
    }

    [Authorize]
    [HttpGet("{maNd}")]
    public async Task<IActionResult> GetUserById([FromRoute] string maNd)
    {
        if (!CurrentUserIsAdmin())
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(maNd))
        {
            return BadRequest(new { message = "Ma nguoi dung khong hop le." });
        }

        var user = await _context.NguoiDungs.AsNoTracking()
            .Where(u => u.MaNd == maNd)
            .Select(u => new AdminUserVM
            {
                MaNd = u.MaNd,
                Email = u.Email,
                HoTen = u.HoTen,
                MatKhau = u.MatKhau,
                VaiTro = u.VaiTro,
                SoDienThoai = u.SoDienThoai,
                NgayDangKy = u.NgayDangKy,
                LanDangNhapCuoi = u.LanDangNhapCuoi,
                AnhDaiDien = u.AnhDaiDien
            })
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound(new { message = "Khong tim thay nguoi dung." });
        }

        return Ok(user);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] AdminUserCreateVM model)
    {
        if (!CurrentUserIsAdmin())
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var hoTen = model.HoTen?.Trim();
        var email = model.Email?.Trim();
        var matKhau = model.MatKhau?.Trim();
        var soDienThoai = model.SoDienThoai?.Trim();
        var vaiTro = string.IsNullOrWhiteSpace(model.VaiTro) ? "User" : model.VaiTro!.Trim();

        if (string.IsNullOrWhiteSpace(hoTen) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(matKhau))
        {
            return BadRequest(new { message = "Ho ten, email va mat khau la bat buoc." });
        }

        if (matKhau.Length < 6)
        {
            return BadRequest(new { message = "Mat khau phai it nhat 6 ky tu." });
        }

        if (await _context.NguoiDungs.AnyAsync(u => u.Email == email))
        {
            return Conflict(new { message = "Email da ton tai trong he thong." });
        }

        var maNd = await GenerateNextUserIdAsync(vaiTro);

        var user = new NguoiDung
        {
            MaNd = maNd,
            HoTen = hoTen,
            Email = email,
            MatKhau = matKhau,
            VaiTro = vaiTro,
            SoDienThoai = string.IsNullOrWhiteSpace(soDienThoai) ? null : soDienThoai,
            NgayDangKy = DateTime.UtcNow,
            LanDangNhapCuoi = null,
            AnhDaiDien = null
        };

        _context.NguoiDungs.Add(user);
        await _context.SaveChangesAsync();

        var result = new AdminUserVM
        {
            MaNd = user.MaNd,
            Email = user.Email,
            HoTen = user.HoTen,
            MatKhau = user.MatKhau,
            VaiTro = user.VaiTro,
            SoDienThoai = user.SoDienThoai,
            NgayDangKy = user.NgayDangKy,
            LanDangNhapCuoi = user.LanDangNhapCuoi,
            AnhDaiDien = user.AnhDaiDien
        };

        return CreatedAtAction(nameof(GetUserById), new { maNd = user.MaNd }, result);
    }

    [Authorize]
    [HttpPut("{maNd}")]
    public async Task<IActionResult> UpdateUser([FromRoute] string maNd, [FromBody] AdminUserUpdateVM model)
    {
        if (!CurrentUserIsAdmin())
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(maNd))
        {
            return BadRequest(new { message = "Ma nguoi dung khong hop le." });
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNd == maNd);
        if (user == null)
        {
            return NotFound(new { message = "Khong tim thay nguoi dung." });
        }

        var updatedEmail = model.Email?.Trim();
        if (!string.IsNullOrWhiteSpace(updatedEmail) && !string.Equals(updatedEmail, user.Email, StringComparison.OrdinalIgnoreCase))
        {
            var exists = await _context.NguoiDungs.AnyAsync(u => u.Email == updatedEmail && u.MaNd != maNd);
            if (exists)
            {
                return Conflict(new { message = "Email da ton tai trong he thong." });
            }
            user.Email = updatedEmail;
        }

        var updatedName = model.HoTen?.Trim();
        if (!string.IsNullOrWhiteSpace(updatedName))
        {
            user.HoTen = updatedName;
        }

        var updatedRole = model.VaiTro?.Trim();
        if (!string.IsNullOrWhiteSpace(updatedRole))
        {
            user.VaiTro = updatedRole;
        }

        var updatedPhone = model.SoDienThoai?.Trim();
        if (model.SoDienThoai != null)
        {
            user.SoDienThoai = string.IsNullOrWhiteSpace(updatedPhone) ? null : updatedPhone;
        }

        if (!string.IsNullOrWhiteSpace(model.MatKhau))
        {
            if (model.MatKhau.Length < 6)
            {
                return BadRequest(new { message = "Mat khau phai it nhat 6 ky tu." });
            }
            user.MatKhau = model.MatKhau.Trim();
        }

        await _context.SaveChangesAsync();

        var result = new AdminUserVM
        {
            MaNd = user.MaNd,
            Email = user.Email,
            HoTen = user.HoTen,
            MatKhau = user.MatKhau,
            VaiTro = user.VaiTro,
            SoDienThoai = user.SoDienThoai,
            NgayDangKy = user.NgayDangKy,
            LanDangNhapCuoi = user.LanDangNhapCuoi,
            AnhDaiDien = user.AnhDaiDien
        };

        return Ok(result);
    }

    [Authorize]
    [HttpDelete("{maNd}")]
    public async Task<IActionResult> DeleteUser([FromRoute] string maNd)
    {
        if (!CurrentUserIsAdmin())
        {
            return Forbid();
        }

        if (string.IsNullOrWhiteSpace(maNd))
        {
            return BadRequest(new { message = "Ma nguoi dung khong hop le." });
        }

        var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNd == maNd);
        if (user == null)
        {
            return NotFound(new { message = "Khong tim thay nguoi dung." });
        }

        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrWhiteSpace(currentUserId) &&
            string.Equals(currentUserId, maNd, StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "Khong the xoa tai khoan cua chinh ban." });
        }

        _context.NguoiDungs.Remove(user);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            return Conflict(new { message = "Khong the xoa nguoi dung do dang duoc tham chieu.", error = ex.Message });
        }

        return NoContent();
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value!.Trim();
            }
        }
        return null;
    }

    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile([FromQuery] string? maNguoiDung = null)
    {
        try
        {
            var maNdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(maNdFromToken))
            {
                return Unauthorized(new { message = "Phiên làm việc không hợp lệ." });
            }

            var requestedMaNd = !string.IsNullOrWhiteSpace(maNguoiDung) ? maNguoiDung : maNdFromToken;
            if (!string.Equals(requestedMaNd, maNdFromToken, StringComparison.OrdinalIgnoreCase))
            {
                var currentRole = User.FindFirst("VaiTro")?.Value;
                if (!string.Equals(currentRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            // Tải các trường bằng EF (đã bỏ map SoDienThoai để tránh lỗi 42703)
            var user = await _context.NguoiDungs.AsNoTracking().FirstOrDefaultAsync(u => u.MaNd == requestedMaNd);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại!" });
            }

            // Đọc riêng số điện thoại bằng SQL thô để truy cập cột so_dien_thoai (có chữ hoa, cần trích dẫn)
            string? soDienThoai = null;
            var conn = _context.Database.GetDbConnection();
            var wasClosed = conn.State != ConnectionState.Open;
            try
            {
                if (wasClosed) await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT so_dien_thoai FROM nguoi_dung WHERE ma_nd = @ma LIMIT 1";
                var pMa = cmd.CreateParameter();
                pMa.ParameterName = "@ma";
                pMa.Value = requestedMaNd!;
                cmd.Parameters.Add(pMa);

                var result = await cmd.ExecuteScalarAsync();
                soDienThoai = result == DBNull.Value ? null : result?.ToString();
            }
            finally
            {
                if (wasClosed && conn.State == ConnectionState.Open)
                    await conn.CloseAsync();
            }

            return Ok(new
            {
                ma_nd = user.MaNd,
                email = user.Email,
                ho_ten = user.HoTen,
                so_dien_thoai = soDienThoai,
                vai_tro = user.VaiTro,
                ngay_dang_ky = user.NgayDangKy,
                anh_dai_dien = user.AnhDaiDien,
                lan_dang_nhap_cuoi = user.LanDangNhapCuoi
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi lấy profile!", error = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("cap-nhat")]
    [HttpPost("cap-nhat")]
    [HttpPatch("cap-nhat")]
    [HttpPut("profile")]
    [HttpPatch("profile")]
    public async Task<IActionResult> CapNhatThongTin([FromBody] UpdateProfileVM model)
    {
        try
        {
            var maNdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(maNdFromToken))
            {
                return Unauthorized(new { message = "Phiên làm việc không hợp lệ." });
            }

            var requestedMaNd = FirstNonEmpty(
                model.MaNguoiDung,
                model.MaNd,
                model.MaNdSnake,
                model.Id,
                model.UserId) ?? maNdFromToken;

            if (!string.Equals(requestedMaNd, maNdFromToken, StringComparison.OrdinalIgnoreCase))
            {
                var currentRole = User.FindFirst("VaiTro")?.Value;
                if (!string.Equals(currentRole, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return Forbid();
                }
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNd == requestedMaNd);
            if (user == null)
            {
                return NotFound(new { message = "Người dùng không tồn tại!" });
            }

            var newName = FirstNonEmpty(model.HoTen, model.FullName, model.Name);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                user.HoTen = newName;
            }

            var newEmail = FirstNonEmpty(model.Email);
            if (!string.IsNullOrWhiteSpace(newEmail) &&
                !string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (!newEmail!.Contains("@"))
                {
                    return BadRequest(new { message = "Email không hợp lệ." });
                }

                var emailExists = await _context.NguoiDungs
                    .AnyAsync(u => u.Email == newEmail && u.MaNd != user.MaNd);
                if (emailExists)
                {
                    return BadRequest(new { message = "Email đã tồn tại trong hệ thống." });
                }

                user.Email = newEmail;
            }

            var newPhone = FirstNonEmpty(model.SoDienThoai, model.Phone);
            // Cập nhật số điện thoại qua SQL để nhắm đúng cột so_dien_thoai
            if (newPhone != null || model.SoDienThoai != null || model.Phone != null)
            {
                var conn = _context.Database.GetDbConnection();
                var wasClosed = conn.State != ConnectionState.Open;
                try
                {
                    if (wasClosed) await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE nguoi_dung SET so_dien_thoai = @phone WHERE ma_nd = @ma";

                    var pPhone = cmd.CreateParameter();
                    pPhone.ParameterName = "@phone";
                    pPhone.Value = string.IsNullOrWhiteSpace(newPhone) ? DBNull.Value : newPhone!;
                    cmd.Parameters.Add(pPhone);

                    var pMa = cmd.CreateParameter();
                    pMa.ParameterName = "@ma";
                    pMa.Value = user.MaNd;
                    cmd.Parameters.Add(pMa);

                    await cmd.ExecuteNonQueryAsync();
                }
                finally
                {
                    if (wasClosed && conn.State == ConnectionState.Open)
                        await conn.CloseAsync();
                }
            }

            var newAvatar = FirstNonEmpty(model.AnhDaiDien, model.AvatarUrl, model.Avatar);
            if (newAvatar != null || model.AnhDaiDien != null || model.AvatarUrl != null || model.Avatar != null)
            {
                user.AnhDaiDien = string.IsNullOrWhiteSpace(newAvatar) ? null : newAvatar;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thông tin thành công!",
                user = new
                {
                    ma_nd = user.MaNd,
                    email = user.Email,
                    ho_ten = user.HoTen,
                    so_dien_thoai = newPhone,
                    vai_tro = user.VaiTro,
                    anh_dai_dien = user.AnhDaiDien
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật!", error = ex.Message });
        }
    }

    [Authorize]
    [HttpPost("doi-mat-khau")]
    [HttpPost("change-password")]
    public async Task<IActionResult> DoiMatKhau([FromBody] ChangePasswordVM model)
    {
        try
        {
            if (model == null)
            {
                return BadRequest(new { message = "Thieu du lieu." });
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

            var currentHash = ComputeMd5(currentPassword);
            if (!string.Equals(user.MatKhau, currentHash, StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = "Mat khau hien tai khong chinh xac." });
            }

            if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
            {
                return BadRequest(new { message = "Mat khau moi phai khac mat khau hien tai." });
            }

            // DB trigger trg_ma_hoa_mat_khau hashes passwords; use raw value to avoid double hashing.
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

    /// <summary>
    /// Cập nhật thông tin cá nhân (self-service) và tùy chọn đổi mật khẩu trong một request.
    /// PUT api/NguoiDung/me
    /// </summary>
    [Authorize]
    [HttpPut("me")]
    public async Task<IActionResult> UpdateSelf([FromBody] UpdateSelfVM model)
    {
        try
        {
            var maNdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(maNdFromToken))
            {
                return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNd == maNdFromToken);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy người dùng." });
            }

            // Cập nhật họ tên
            var newName = FirstNonEmpty(model.HoTen, model.FullName, model.Name);
            if (!string.IsNullOrWhiteSpace(newName))
            {
                user.HoTen = newName.Trim();
            }

            // Cập nhật email (kiểm tra trùng và định dạng)
            var newEmail = FirstNonEmpty(model.Email);
            if (!string.IsNullOrWhiteSpace(newEmail) && !string.Equals(newEmail, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (!newEmail.Contains("@"))
                {
                    return BadRequest(new { message = "Email không hợp lệ." });
                }
                var exists = await _context.NguoiDungs.AnyAsync(u => u.Email == newEmail && u.MaNd != user.MaNd);
                if (exists)
                {
                    return Conflict(new { message = "Email đã tồn tại." });
                }
                user.Email = newEmail.Trim();
            }

            // Cập nhật số điện thoại (cột so_dien_thoai)
            var newPhone = FirstNonEmpty(model.SoDienThoai, model.Phone);
            if (newPhone != null || model.SoDienThoai != null || model.Phone != null)
            {
                var conn = _context.Database.GetDbConnection();
                var wasClosed = conn.State != ConnectionState.Open;
                try
                {
                    if (wasClosed) await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "UPDATE nguoi_dung SET so_dien_thoai = @phone WHERE ma_nd = @ma";
                    var pPhone = cmd.CreateParameter();
                    pPhone.ParameterName = "@phone";
                    pPhone.Value = string.IsNullOrWhiteSpace(newPhone) ? DBNull.Value : newPhone!;
                    cmd.Parameters.Add(pPhone);
                    var pMa = cmd.CreateParameter();
                    pMa.ParameterName = "@ma";
                    pMa.Value = user.MaNd;
                    cmd.Parameters.Add(pMa);
                    await cmd.ExecuteNonQueryAsync();
                }
                finally
                {
                    if (wasClosed && conn.State == ConnectionState.Open)
                        await conn.CloseAsync();
                }
            }

            // Cập nhật avatar
            var newAvatar = FirstNonEmpty(model.AnhDaiDien, model.AvatarUrl, model.Avatar);
            if (newAvatar != null || model.AnhDaiDien != null || model.AvatarUrl != null || model.Avatar != null)
            {
                user.AnhDaiDien = string.IsNullOrWhiteSpace(newAvatar) ? null : newAvatar;
            }

            // Đổi mật khẩu nếu có đủ tham số
            var currentPassword = FirstNonEmpty(model.MatKhauHienTai, model.MatKhauCu, model.CurrentPassword);
            var newPassword = FirstNonEmpty(model.MatKhauMoi, model.NewPassword);
            var confirmPassword = FirstNonEmpty(model.XacNhanMatKhauMoi, model.ConfirmPassword);
            if (!string.IsNullOrWhiteSpace(currentPassword) || !string.IsNullOrWhiteSpace(newPassword) || !string.IsNullOrWhiteSpace(confirmPassword))
            {
                if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    return BadRequest(new { message = "Thiếu tham số đổi mật khẩu." });
                }
                if (!string.Equals(newPassword, confirmPassword))
                {
                    return BadRequest(new { message = "Xác nhận mật khẩu không khớp." });
                }
                var currentHash = ComputeMd5(currentPassword);
                if (!string.Equals(user.MatKhau, currentHash, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { message = "Mật khẩu hiện tại không chính xác." });
                }
                if (newPassword.Length < 6)
                {
                    return BadRequest(new { message = "Mật khẩu mới phải ít nhất 6 ký tự." });
                }
                if (string.Equals(currentPassword, newPassword, StringComparison.Ordinal))
                {
                    return BadRequest(new { message = "Mật khẩu mới phải khác mật khẩu hiện tại." });
                }
                user.MatKhau = newPassword.Trim();
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Cập nhật thành công.",
                user = new
                {
                    ma_nd = user.MaNd,
                    email = user.Email,
                    ho_ten = user.HoTen,
                    so_dien_thoai = newPhone,
                    vai_tro = user.VaiTro,
                    anh_dai_dien = user.AnhDaiDien
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin.", error = ex.Message });
        }
    }
}

