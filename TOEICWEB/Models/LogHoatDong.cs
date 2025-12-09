using System;
using System.Collections.Generic;

namespace TOEICWEB.Models;

public partial class LogHoatDong
{
    public int MaLog { get; set; }

    public string? MaNd { get; set; }

    public string? LoaiHoatDong { get; set; }

    public string? MoTa { get; set; }

    public string? DuLieuCu { get; set; }

    public string? DuLieuMoi { get; set; }

    public DateTime? ThoiGian { get; set; }
}
