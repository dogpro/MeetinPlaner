using System;

namespace MeetingPlannerConsole.Model
{
    internal class MeetingModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Description { get; set; }
        public DateTime NotificationTime { get; set; }

        #region Constructors
        public MeetingModel(int id, string name, DateTime startTime, DateTime endTime, string description, DateTime notificationTime)
        {
            Id = id;
            Name = name;
            StartTime = startTime;
            EndTime = endTime;
            Description = description;
            NotificationTime = notificationTime;
        }

        public MeetingModel() { }
        #endregion
        
        public string PrintAllMeetings(int id = 0)
        { 
            return $"№{id + 1} {Name}\n Начало: {StartTime}\n Конец: {EndTime}\n Описаие: {Description}\n Уведомить в: {NotificationTime}\n";
        }

    }
}
