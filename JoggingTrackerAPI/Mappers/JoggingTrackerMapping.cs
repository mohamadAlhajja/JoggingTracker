using JoggingTrackerAPI.Data.Entities;
using JoggingTrackerAPI.Models;

namespace JoggingTrackerAPI.Mappers
{
    public static class JoggingTrackerMapping
    {
        public static JoggingTrackerEntity ToEntity(this JoggingTrackerModel joggingModel)
        {
            return new JoggingTrackerEntity
            {
                UserId = joggingModel.UserId,
                Time = joggingModel.Time,
                Date = joggingModel.Date,
                Location = joggingModel.Location,
                Distance = joggingModel.Distance,
            };
        }

        public static JoggingTrackerResource ToResource(this JoggingTrackerEntity joggingEntity)
        {
            return new JoggingTrackerResource
            {
                Id = joggingEntity.Id,
                UserId = joggingEntity.UserId,
                Time = joggingEntity.Time,
                Date = joggingEntity.Date,
                Distance = joggingEntity.Distance,
                Location = joggingEntity.Location,
            };
        }
    }
}
