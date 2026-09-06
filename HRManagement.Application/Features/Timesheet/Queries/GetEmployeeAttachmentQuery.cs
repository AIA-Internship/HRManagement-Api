using HRManagement.Domain.Interfaces;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Queries;
using HRManagement.Domain.Models.Payload.EmployeeDtos.Queries.Dto;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Response.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HRManagement.Application.Queries;
public record GetEmployeeAttachmentsQuery(int Id) : IRequest<ApiResponse<List<EmployeeAttachmentDto>>>;

public class GetEmployeeAttachmentsHandler : IRequestHandler<GetEmployeeAttachmentsQuery, ApiResponse<List<EmployeeAttachmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetEmployeeAttachmentsHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<List<EmployeeAttachmentDto>>> Handle(GetEmployeeAttachmentsQuery request, CancellationToken cancellationToken)
    {
        var attachments = await _context.EmployeeAttachment
            .AsNoTracking()
            .Where(a => a.Id == request.Id && a.IsActive)
            .Select(a => new EmployeeAttachmentDto
            {
                Id = a.Id,
                DocumentType = a.DocumentType,
                FileName = a.FileName,
                FilePath = a.FilePath
            })
            .ToListAsync(cancellationToken);

        return ApiHelperResponse.Success("Attachments retrieved", attachments);
    }
}




