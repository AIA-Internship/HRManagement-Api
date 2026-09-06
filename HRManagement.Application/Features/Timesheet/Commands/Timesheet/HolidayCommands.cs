using HRManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using HRManagement.Application.Interfaces;
using HRManagement.Domain.Models.Payload.TimesheetDtos.Commands.Dto;
using HRManagement.Domain.Models.Response.Shared;
using HRManagement.Domain.Models.Tables;

namespace HRManagement.Application.Commands.Timesheet
{
    public class BulkUpsertHolidaysCommand : IRequest<ApiResponse<string>>
    {
        public BulkUpsertHolidaysDto Dto { get; set; } = new();
        public int ActionerId { get; set; }
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
                .ToListAsync(cancellationToken);

            var processedHolidays = new HashSet<TimesheetHoliday>();

            // 1. Process Updates & Adds
            foreach (var holidayDto in holidaysToUpsert)
            {
                // Find by ID first, or fallback to matching Date (to catch soft-deleted or existing dates)
                var existing = existingHolidays.FirstOrDefault(p => (holidayDto.Id.HasValue && p.Id == holidayDto.Id) || p.HolidayDate.Date == holidayDto.HolidayDate.Date);
                
                if (existing != null)
                {
                    // Update and Undelete if necessary
                    existing.UpdateDetails(holidayDto.HolidayDate, holidayDto.HolidayName, holidayDto.Description, 0);
                    if (existing.IsDeleted) existing.SetDelete(0); // Undelete logic (assuming SetDelete toggles or there's a way. Let's set IsDeleted manually or via domain method. Usually IsDeleted = false is enough, but I'll use standard assignment)
                    existing.IsDeleted = false;
                    processedHolidays.Add(existing);
                }
                else
                {
                    // Add new safely
                    var newHoliday = new TimesheetHoliday(holidayDto.HolidayDate, holidayDto.HolidayName, holidayDto.Description, 0);
                    await _dbContext.TimesheetHolidays.AddAsync(newHoliday, cancellationToken);
                    existingHolidays.Add(newHoliday); // Prevent duplicates in the same payload
                    processedHolidays.Add(newHoliday);
                }
            }

            // 2. Process Deletions (Soft Delete)
            // Delete those that are currently active but were not processed in the payload
            var holidaysToDelete = existingHolidays.Where(p => !p.IsDeleted && !processedHolidays.Contains(p)).ToList();

            foreach (var holiday in holidaysToDelete)
            {
                // We mark it as deleted so it doesn't show up but remains in history
                holiday.SetDelete(0);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return ApiResponse<string>.Success("Holidays updated successfully.");
        }
    }

    public class DeleteHolidayCommand : IRequest<ApiResponse<string>>
    {
        public int Id { get; set; }
        public int ActionerId { get; set; }
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
            
            if (holiday == null) return ApiHelperResponse.Failed<string>("Holiday not found.");

            holiday.SetDelete(0);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ApiResponse<string>.Success("Holiday deleted successfully.");
        }
    }
}













