using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HRManagement.Api.Application.Interfaces;
using HRManagement.Api.Application.TimesheetDtos.Commands.Dto;
using HRManagement.Api.Domain.Models.Response.Shared;
using HRManagement.Api.Domain.Models.Tables;

namespace HRManagement.Api.Application.Commands.Timesheet
{
    public class BulkUpsertHolidaysCommand : IRequest<ApiResponse<string>>
    {
        public BulkUpsertHolidaysDto Dto { get; set; } = new();
        public long ActionerId { get; set; }
    }

    public class BulkUpsertHolidaysCommandHandler : IRequestHandler<BulkUpsertHolidaysCommand, ApiResponse<string>>
    {
        private readonly IApplicationDbContext _dbContext;

        public BulkUpsertHolidaysCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<string>> Handle(BulkUpsertHolidaysCommand request, CancellationToken cancellationToken)
        {
            var holidaysToUpsert = request.Dto.Holidays;
            var existingHolidays = await _dbContext.TimesheetHolidays
                .Where(p => !p.IsDeleted)
                .ToListAsync(cancellationToken);

            // 1. Process Updates & Adds
            foreach (var holidayDto in holidaysToUpsert)
            {
                if (holidayDto.Id.HasValue && holidayDto.Id > 0)
                {
                    // Update
                    var existing = existingHolidays.FirstOrDefault(p => p.Id == holidayDto.Id);
                    if (existing != null)
                    {
                        existing.UpdateDetails(holidayDto.HolidayDate, holidayDto.HolidayName, holidayDto.Description, request.ActionerId);
                    }
                }
                else
                {
                    // Add new (check for date duplicate first)
                    bool alreadyExists = existingHolidays.Any(p => p.HolidayDate.Date == holidayDto.HolidayDate.Date);
                    if (!alreadyExists)
                    {
                        var newHoliday = new TimesheetHoliday(holidayDto.HolidayDate, holidayDto.HolidayName, holidayDto.Description, request.ActionerId);
                        await _dbContext.TimesheetHolidays.AddAsync(newHoliday, cancellationToken);
                    }
                }
            }

            // 2. Process Deletions (Soft Delete)
            var incomingIds = holidaysToUpsert.Where(h => h.Id.HasValue).Select(h => h.Id!.Value).ToList();
            var holidaysToDelete = existingHolidays.Where(p => !incomingIds.Contains(p.Id)).ToList();

            foreach (var holiday in holidaysToDelete)
            {
                // We mark it as deleted so it doesn't show up but remains in history
                holiday.MarkAsDeleted(request.ActionerId);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ApiResponse<string>.Success("Holidays updated successfully.");
        }
    }

    public class DeleteHolidayCommand : IRequest<ApiResponse<string>>
    {
        public int Id { get; set; }
        public long ActionerId { get; set; }
    }

    public class DeleteHolidayCommandHandler : IRequestHandler<DeleteHolidayCommand, ApiResponse<string>>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeleteHolidayCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ApiResponse<string>> Handle(DeleteHolidayCommand request, CancellationToken cancellationToken)
        {
            var holiday = await _dbContext.TimesheetHolidays
                .FirstOrDefaultAsync(h => h.Id == request.Id && !h.IsDeleted, cancellationToken);
            
            if (holiday == null) return ApiResponse<string>.Failed("Holiday not found.");

            holiday.MarkAsDeleted(request.ActionerId);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ApiResponse<string>.Success("Holiday deleted successfully.");
        }
    }
}

