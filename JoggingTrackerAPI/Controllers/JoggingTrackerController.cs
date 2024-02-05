using JoggingTrackerAPI.Data;
using JoggingTrackerAPI.Data.Entities;
using JoggingTrackerAPI.Mappers;
using JoggingTrackerAPI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JoggingTrackerAPI.Controllers
{
    [ApiController]
    [Route("jogging-tracker")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class JoggingTrackerController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<UserEntity> _userManager;

        public JoggingTrackerController(AppDbContext dbContext , UserManager<UserEntity> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetJoggingRecord(int id)
        {
            var joggingRecord = await _dbContext.JoggingTrackerEntity.FindAsync(new object[] { id });

            if (joggingRecord == null)
            {
                return NotFound($"Jogging record with ID {id} not found.");
            }

            if (await IsUserAuthorized(joggingRecord.UserId))
            {
                return Ok(joggingRecord.ToResource());
            }
            else
                return Unauthorized();
            
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GetAllJoggingRecords(int? skip,int? take)
        {
            var joggingRecords = _dbContext.JoggingTrackerEntity.ToList().Select(entity => entity.ToResource());
           
            if (User.IsInRole("Admin"))
                return Ok(joggingRecords);
            else {
                var user = await _userManager.FindByNameAsync(User.Identity?.Name);
                return Ok(joggingRecords.Where(x => x.UserId == user.Id).Skip(skip ?? 0).Take(take ?? int.MaxValue)); 
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> CreateJoggingRecord([FromBody] JoggingTrackerModel joggingRecord)
        {
            if (await IsUserAuthorized(joggingRecord.UserId))
            {
                _dbContext.JoggingTrackerEntity.Add(joggingRecord.ToEntity());
                await _dbContext.SaveChangesAsync();
                return Ok(joggingRecord);
            }
            else
               return Unauthorized("You don't have permission to create this record");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateJoggingRecord(int id, [FromBody] JoggingTrackerModel updatedJoggingRecord)
        {
            if (await IsUserAuthorized(updatedJoggingRecord.UserId)) {
                var joggingRecord = await _dbContext.JoggingTrackerEntity.FindAsync(id);

                if (joggingRecord == null)
                {
                    return NotFound($"Jogging record with ID {id} not found.");
                }

                joggingRecord.Date = updatedJoggingRecord.Date;
                joggingRecord.Distance = updatedJoggingRecord.Distance;
                joggingRecord.Time = updatedJoggingRecord.Time;
                joggingRecord.Location = updatedJoggingRecord.Location;

                await _dbContext.SaveChangesAsync();

                return Ok(joggingRecord);
            }else

                return Unauthorized("You don't have permission to Update this record");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteJoggingRecord(int id)
        {
            var joggingRecord = await _dbContext.JoggingTrackerEntity.FindAsync(id);

                if (joggingRecord == null)
            {
                return NotFound($"Jogging record with ID {id} not found.");
            }
            if (await IsUserAuthorized(joggingRecord.UserId))
            {
                _dbContext.JoggingTrackerEntity.Remove(joggingRecord);
                await _dbContext.SaveChangesAsync();

                return NoContent();
            }else
                return Unauthorized("You don't have permission to delete this record");

        }

        [HttpGet("generate-report")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> GenerateWeeklyReport(string userId, [FromQuery] DateTime startDate)
        {
            if(await IsUserAuthorized(userId))
            {
                    var weeklyActivity = _dbContext.JoggingTrackerEntity.ToList()
                        .Select(entity => entity.ToResource())
                        .Where(jogging => jogging.Date >=startDate && userId == jogging.UserId && jogging.Date < startDate.AddDays(7)).ToList();

                return Ok(GenerateWeeklyReports(weeklyActivity));
            }
            else
            {
                return Unauthorized($"You don't have permission to Generate report for this user {userId}");
            }
        }

        private async Task<bool> IsUserAuthorized(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return User.IsInRole("Admin") || (User.IsInRole("User") && user.UserName == User.Identity?.Name);
        }

        private static WeeklyReportResource GenerateWeeklyReports(List<JoggingTrackerResource> weeklyActivity)
        {
            var averageSpeed = 0.0;
            var totalDistance = 0.0;
            var totalHours = 0.0;

            weeklyActivity.ForEach(x =>
            {
                totalDistance += x.Distance;
                totalHours += x.Time.TotalHours;
            });
            if(totalDistance != 0.0 && totalHours != 0.0)
               averageSpeed = totalDistance / totalHours;

            return new WeeklyReportResource {AverageSpeed = averageSpeed,Distance = totalDistance };
        }
    }
}
