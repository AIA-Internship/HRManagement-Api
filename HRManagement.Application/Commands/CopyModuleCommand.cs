using CSharpFunctionalExtensions;
using HRManagement.Api.Domain.Interfaces;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Table.ELearningModels;
using HRManagement.Api.Domain.Models.Table.ELearningModels.ELearningDto;
using HRManagement.Api.Repositories.Base;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HRManagement.Api.Application.Commands.ELearningCommands
{
    public class CopyModuleCommand(CopyModuleDto dto) : IRequest<Result<ApiResponse>>
    {
        public CopyModuleDto Dto { get; set; } = dto;
    }

    internal class CopyModuleHandler : IRequestHandler<CopyModuleCommand, Result<ApiResponse>>
    {
        private readonly ILogger<CopyModuleHandler> _logger;
        private readonly SqlDbContext _context;

        public CopyModuleHandler(SqlDbContext context, ILogger<CopyModuleHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Result<ApiResponse>> Handle(CopyModuleCommand request, CancellationToken ct)
        {
            _logger.LogTrace("Executing handler for request : {request}", nameof(CopyModuleHandler));

            using var transaction = await _context.Database.BeginTransactionAsync(ct);
            try
            {
                var sourceModule = await _context.ELearningModules
                    .FirstOrDefaultAsync(m => m.ModuleId == request.Dto.sourceModuleId && !m.IsDeleted, ct);

                if (sourceModule == null)
                    return ApiHelperResponse.Failed("Source module not found.");

                var clonedModule = new ModuleModel
                {
                    BatchId = request.Dto.targetBatchId,
                    ModuleTitle = sourceModule.ModuleTitle,
                    ModuleDescription = sourceModule.ModuleDescription,
                    TargetRole = sourceModule.TargetRole,
                    IsPriority = sourceModule.IsPriority,
                    DueDate = sourceModule.DueDate,
                    IsDeleted = false,
                    CreatedBy = request.Dto.currentUserId.ToString(),
                    CreatedUtcDate = DateTime.UtcNow
                };

                _context.ELearningModules.Add(clonedModule);
                await _context.SaveChangesAsync(ct);

                var sourceContents = await _context.ELearningModuleContents
                    .Where(c => c.ModuleId == request.Dto.sourceModuleId && !c.IsDeleted)
                    .ToListAsync(ct);

                if (sourceContents.Any())
                {
                    foreach (var content in sourceContents)
                    {
                        var clonedContent = new ModuleContentModel
                        {
                            ModuleId = clonedModule.ModuleId,
                            ContentTitle = content.ContentTitle,
                            ContentType = content.ContentType,
                            ContentUrl = content.ContentUrl,
                            SortOrder = content.SortOrder,
                            IsDeleted = false,
                            CreatedBy = request.Dto.currentUserId.ToString(),
                            CreatedUtcDate = DateTime.UtcNow
                        };
                        _context.ELearningModuleContents.Add(clonedContent);
                    }
                    await _context.SaveChangesAsync(ct);
                }

                await transaction.CommitAsync(ct);
                return ApiHelperResponse.Success("Module copied successfully to the new batch!");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(ct);
                _logger.LogError(ex, "Error copying module id {sourceId}", request.Dto.sourceModuleId);
                return ApiHelperResponse.Failed(ex.Message);
            }
        }
    }
}