namespace HRManagement.Api.Application.EmployeeDtos.Commands.Dto;

public class UpdateEmploymentInfoRequestDto
{
    /// <summary>
    /// Status Kepegawaian
    /// </summary>
    /// <example>1</example>
    public int? EmploymentStatus { get; set; }

    /// <summary>
    /// Tanggal mulai bekerja
    /// </summary>
    /// <example>2024-01-01</example>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Tipe Kepegawaian
    /// </summary>
    /// <example>1</example>
    public int? EmploymentType { get; set; }

    /// <summary>
    /// Nama Departemen
    /// </summary>
    /// <example>Information Technology</example>
    public string Department { get; set; } = string.Empty;

    /// <summary>
    /// Posisi/Jabatan
    /// </summary>
    /// <example>Software Engineer</example>
    public string Position { get; set; } = string.Empty;

    /// <summary>
    /// Nama Atasan Langsung
    /// </summary>
    /// <example>John Doe</example>
    public string SupervisorName { get; set; } = string.Empty;

    /// <example>E150529</example>
    public string EmployeeDisplayId { get; set; } = string.Empty;
}
