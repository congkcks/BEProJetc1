using System.ComponentModel.DataAnnotations;

namespace TOEICWEB.ViewModels.Admin;

public class LessonContentRequest
{
    [Required]
    [RegularExpression("^(listening|reading|writing)$", ErrorMessage = "Loại nội dung không hợp lệ.")]
    public string Type { get; set; } = "reading";

    public ListeningContentRequest? Listening { get; set; }
    public ReadingContentRequest? Reading { get; set; }
    public WritingContentRequest? Writing { get; set; }
}

public class ListeningContentRequest
{
    [Required]
    public string TieuDe { get; set; } = string.Empty;
    public string? DoKho { get; set; }
    public string? DuongDanAudio { get; set; }
    public string? BanGhiAm { get; set; }
}

public class ReadingContentRequest
{
    [Required]
    public string TieuDe { get; set; } = string.Empty;
    public string? DoKho { get; set; }
    public string? DuongDanFileTxt { get; set; }
    public string? NoiDung { get; set; }
}

public class WritingContentRequest
{
    [Required]
    public string TieuDe { get; set; } = string.Empty;
    [Required]
    public string DeBai { get; set; } = string.Empty;
    public string? BaiMau { get; set; }
    public int? SoTuToiThieu { get; set; }
    public int? SoTuToiDa { get; set; }
}

public class CreateLessonRequest
{
    [Required]
    public string MaLoTrinh { get; set; } = string.Empty;

    [Required]
    public string TenBai { get; set; } = string.Empty;

    public string? MoTa { get; set; }

    [Range(1, 600)]
    public int? ThoiLuongPhut { get; set; }

    [Range(1, 1000)]
    public int? SoThuTu { get; set; }

    public LessonContentRequest? Content { get; set; }
}

public class UpdateLessonRequest
{
    public string? TenBai { get; set; }
    public string? MoTa { get; set; }

    [Range(1, 600)]
    public int? ThoiLuongPhut { get; set; }

    [Range(1, 1000)]
    public int? SoThuTu { get; set; }

    public string? MaLoTrinh { get; set; }

    public LessonContentRequest? Content { get; set; }
}
