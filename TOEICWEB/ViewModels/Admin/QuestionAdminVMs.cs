using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class AdminAnswerRequest
{
    [Required]
    [StringLength(1, ErrorMessage = "Nhãn đáp án chỉ gồm 1 ký tự."), RegularExpression("[A-Za-z]", ErrorMessage = "Nhãn đáp án phải là chữ cái.")]
    public string NhanDapAn { get; set; } = "A";

    [Required]
    [MinLength(1)]
    public string NoiDungDapAn { get; set; } = string.Empty;

    public int? ThuTuHienThi { get; set; }

    public bool LaDapAnDung { get; set; }
}

public class AdminQuestionRequest
{
    [Required]
    public string NoiDungCauHoi { get; set; } = string.Empty;

    public string? GiaiThich { get; set; }

    [Range(1, 100)]
    public int? Diem { get; set; } = 1;

    public int? ThuTuHienThi { get; set; }

    [MinLength(2, ErrorMessage = "Cần ít nhất 2 đáp án."), MaxLength(6, ErrorMessage = "Tối đa 6 đáp án."), Required]
    public List<AdminAnswerRequest> DapAns { get; set; } = new();
}
