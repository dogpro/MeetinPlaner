using MeetingPlannerConsole.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

internal class MeetingScheduler
{
    private List<DateTime> _meetingsDate = new List<DateTime>();
    public string NextMeeting = "";
    private Timer _timer;
    private List<MeetingModel> _meetingModels = new List<MeetingModel>();
    private MeetingModel _selectedMeeting;

    public MeetingScheduler()
    {
        _timer = new Timer();
        _timer.Elapsed += TimerElapsed;
    }

    public void AddMeetings(List<MeetingModel> meetingsModel)
    {
        if (_meetingModels.Count > 0)
        {
            _meetingModels.Clear();
        }

        _meetingModels = meetingsModel;

        _meetingsDate = _meetingModels.Select(obj => obj.NotificationTime).ToList();
        
        UpdateTimer();
    }

    private void UpdateTimer()
    {
        if (_meetingsDate.Count > 0)
        {
            DateTime currntMeeting = FindMeetingWithClosestDate();
            TimeSpan timeToNextMeeting = currntMeeting - DateTime.Now;

            if (timeToNextMeeting.TotalMilliseconds > 0)
            {
                NextMeeting = _selectedMeeting.StartTime.ToString("dd.MM.yyyy HH:mm:ss");
                _timer.Interval = timeToNextMeeting.TotalMilliseconds;
                _timer.Start();
            }
        }
        else
        {
            _timer.Stop();
        }
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (_meetingsDate.Count > 0 && DateTime.Now >= _meetingsDate[0])
        {
            _meetingsDate.RemoveAt(0);
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine($"Наступило время встречи!\n{_selectedMeeting.PrintAllMeetings()}");
            Console.WriteLine("--------------------------------------------------------------");
            Console.WriteLine("--------------------------------------------------------------");
            UpdateTimer();
        }
    }

    private DateTime FindMeetingWithClosestDate()
    {
        DateTime currentDate = DateTime.Now;
        DateTime closestGreaterDate = DateTime.MaxValue;
        TimeSpan closestDifference = TimeSpan.MaxValue;

        foreach (var obj in _meetingModels)
        {
            TimeSpan difference = obj.NotificationTime - currentDate;

            if (difference > TimeSpan.Zero && difference < closestDifference)
            {
                closestDifference = difference;
                closestGreaterDate = obj.NotificationTime;
                _selectedMeeting = obj;
            }
        }

        return closestGreaterDate;
    }

}