using MeetingPlannerConsole.Model;
using System;
using System.Collections.Generic;
using System.IO;

internal class ExportToFile
{
    private static string _filePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\Meetings.txt";

    public static void ExportMeeting(List<MeetingModel> meetingsList)
    {
        using (FileStream fileStream = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
        {
            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                foreach (var meeting in meetingsList)
                {
                    string line = meeting.PrintAllMeetings(meeting.Id);
                    writer.WriteLine(line);
                }

                Console.WriteLine($"Файл сохранен. Путь до файла: {_filePath}"); 
            }
        }
    }
}
