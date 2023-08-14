using MeetingPlannerConsole.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

internal class MenuController
{
    private static class MenuOptions
    {
        public const int AllMeetings = 1;
        public const int FindMeeting = 2;
        public const int CreateMeeting = 3;
        public const int UpdateMeeting = 4;
        public const int DeleteMeeting = 5;
        public const int Exit = 6;
    }

    private Dictionary<int, string> _mainMenu = new Dictionary<int, string>
    {
        { 1, "Все встречи" },
        { 2, "Найти" },
        { 3, "Создать" },
        { 4, "Изменить" },
        { 5, "Удалить" },
        { 6, "Выход" }
    };

    private List<MeetingModel> _meetingsList;
    private OracleConnection _connection;
    private DBController _dbController;
    private DBConnection _dBCconnection;
    private MeetingScheduler _meetingScheduler;

    public void Init(OracleConnection dBConnection, DBController dBController, List<MeetingModel> meetings, DBConnection dBCconnection, MeetingScheduler meetingScheduler)
    {
        _meetingsList = meetings;
        _connection = dBConnection;
        _dbController = dBController;
        _dBCconnection = dBCconnection;
        _meetingScheduler = meetingScheduler;

        _meetingScheduler.AddMeetings(_meetingsList);
    }

    public void PrintMenu()
    {
        foreach (var item in _mainMenu)
        {
            Console.WriteLine($"{item.Key}. {item.Value}");
        }

        if (_meetingScheduler.NextMeeting != "")
        {
            Console.WriteLine($"Время ближайшей встрчи: {_meetingScheduler.NextMeeting}");
        }

        if (int.TryParse(Console.ReadLine(), out int input))
        {
            HandleMenuOption(input);
        }
        else
        {
            Console.WriteLine("Неверный ввод! Введите числовую опцию из меню.\n");
        }
    }

    private void HandleMenuOption(int input)
    {
        switch (input)
        {
            case MenuOptions.AllMeetings:
                DisplayMeetingList(_meetingsList);
                break;

            case MenuOptions.FindMeeting:
                PrintFindMeeting();
                break;

            case MenuOptions.CreateMeeting:
                InsertMeeting();
                break;

            case MenuOptions.UpdateMeeting:
                UpdateMeeting();
                break;

            case MenuOptions.DeleteMeeting:
                DeleteMeeting();
                break;

            case MenuOptions.Exit:
                _dBCconnection.CloseConnection();
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine("Неверный ввод!\n");
                break;
        }
    }

    private void PrintFindMeeting()
    {
        if (_meetingsList.Count == 0)
        {
            Console.WriteLine("В базе данных нет встреч.\n");
            return;
        }

        Console.WriteLine("Выберите дату для поиска встреч: ");
        var uniqueDates = _meetingsList.Select(meeting => meeting.StartTime.Date.ToString("yyyy/MM/dd")).Distinct().ToList();
        
        for (int i = 0; i < uniqueDates.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {uniqueDates[i]}");
        }

        int userInput = ReadValidIntInput("Введите номер даты или 0 чтобы вернуться назад: ", 0, uniqueDates.Count);
        if (userInput == 0)
        {
            return;
        }

        var selectedDate = uniqueDates[userInput - 1];
        Console.WriteLine("Вы выбрали дату: " + selectedDate);

        List<MeetingModel> meetingsList = _dbController.SelectWhere(_connection, selectedDate);

        DisplayMeetingList(meetingsList);

        if (GetYesNoInput("Для экспорта введите 1. Для выхода в меню - Enter"))
        {
            ExportToFile.ExportMeeting(meetingsList);
        }
    }

    private void DeleteMeeting()
    {
        if (_meetingsList.Count == 0)
        {
            Console.WriteLine("В базе данных нет встреч.\n");
            return;
        }

        DisplayMeetingList(_meetingsList);

        int userInput = ReadValidIntInput("Введите номер встречи для удаления или 0 чтобы вернуться назад: ", 0, _meetingsList.Count);

        if (userInput == 0)
        {
            return;
        }

        if (userInput > 0 && userInput <= _meetingsList.Count)
        {
            var selectedMeeting = _meetingsList[userInput - 1];
            _dbController.Delete(_connection, selectedMeeting.Id);

            Console.WriteLine($"Встреча \"{selectedMeeting.Name}\" удалена\n");

            UpdateList();
        }
        else
        {
            Console.WriteLine("Некорректный номер встречи.\n");
        }
    }

    private void InsertMeeting()
    {
        Console.WriteLine("Нажмите Enter для ввода новой записи или введите 0 чтобы вернуться назад:\n ");
        if (Console.ReadLine() == "0")
        {
            return;
        }

        MeetingModel newMeeting = new MeetingModel();

        newMeeting.Id = _meetingsList.OrderByDescending(item => item.Id).FirstOrDefault().Id + 1;
        newMeeting.Name = ReadInput("Введите имя встречи: ");
        var dates = ValidateDateTimeInput();
        newMeeting.StartTime = dates.Item1;
        newMeeting.EndTime = dates.Item2;
        newMeeting.Description = ReadInput("Введите описание встречи (или нажмите Enter чтобы оставить пустым): ");

        int notificationTimeInput = ReadValidIntInput("Введите время уведомления (в минутах) или 0 чтобы оставить пустым: ", 0, int.MaxValue);
        newMeeting.NotificationTime = (notificationTimeInput == 0) ? newMeeting.StartTime : newMeeting.StartTime.AddMinutes(-notificationTimeInput);

        Console.WriteLine(newMeeting.PrintAllMeetings());

        _dbController.Insert(_connection, newMeeting);

        Console.WriteLine("Встреча добавлена\n");

        UpdateList();
    }

    private void UpdateMeeting()
    {
        if (_meetingsList.Count == 0)
        {
            Console.WriteLine("В базе данных нет встреч.\n");
            return;
        }

        DisplayMeetingList(_meetingsList);

        int userInput = ReadValidIntInput("Введите номер встречи для изменения или 0 чтобы вернуться назад: ", 0, _meetingsList.Count);

        if (userInput == 0)
        {
            return;
        } 

        int id = _meetingsList[userInput - 1].Id;
        MeetingModel currentMeeting = _meetingsList[userInput - 1];

        if (GetYesNoInput("Заменить имя?"))
        {
            currentMeeting.Name = Console.ReadLine();
        }

        if (GetYesNoInput("Заменить время встречи?"))
        {
            var dates = ValidateDateTimeInput();
            currentMeeting.StartTime = dates.Item1;
            currentMeeting.EndTime = dates.Item2;
        }


        if (GetYesNoInput("Заменить описание встречи?"))
        {
            currentMeeting.Description = Console.ReadLine();
        }

        if (GetYesNoInput("Заменить время уведомления?"))
        {
            int notificationTimeInput = ReadValidIntInput("Введите время уведомления (в минутах) или 0 для оставить пустым: ", 0, int.MaxValue);
            currentMeeting.NotificationTime = (notificationTimeInput == 0) ? currentMeeting.StartTime : currentMeeting.StartTime.AddMinutes(-notificationTimeInput);
        }

        Console.WriteLine(currentMeeting.PrintAllMeetings());

        _dbController.Update(_connection, currentMeeting);

        Console.WriteLine($"Встреча добавлена\n");

        UpdateList();
    }

    private void UpdateList()
    {
        _meetingsList.Clear();
        _meetingsList = _dbController.SelectAll(_connection);

        _meetingScheduler.AddMeetings(_meetingsList);
    }

    private string ReadInput(string prompt)
    {
        Console.WriteLine(prompt);
        return Console.ReadLine();
    }

    private bool GetYesNoInput(string prompt)
    {
        Console.WriteLine($"{prompt} 1 - Да | Enter - Нет");
        string input = Console.ReadLine();
        return (input == "1");
    }
   
    private int ReadValidIntInput(string prompt, int minValue, int maxValue)
    {
        Console.WriteLine(prompt);
        int userInput;
        while (!int.TryParse(Console.ReadLine(), out userInput) || userInput < minValue || userInput > maxValue)
        {
            Console.WriteLine($"Некорректный ввод. Пожалуйста, введите значение от {minValue} до {maxValue}: ");
        }
        return userInput;
    }

    private void DisplayMeetingList(List<MeetingModel> meetingModel)
    {
        for (int i = 0; i < meetingModel.Count; i++)
        {
            Console.WriteLine(meetingModel[i].PrintAllMeetings(i));
        }
    }

    public (DateTime Start, DateTime End) ValidateDateTimeInput()
    {
        string format = "yyyy-MM-dd HH:mm:ss";
        DateTime startDateTime;
        DateTime endDateTime;

        while (true)
        {
            Console.WriteLine("Введите дату и время начала встречи в формате год-месяц-день часы:минуты:секунды (2022-02-01 09:30:00):");
            string startInput = Console.ReadLine();

            if (DateTime.TryParseExact(startInput, format, null, System.Globalization.DateTimeStyles.None, out startDateTime) && startDateTime > DateTime.Now)
            {
                Console.WriteLine("Введите дату и время конца встречи в том же формате:");
                string endInput = Console.ReadLine();

                if (DateTime.TryParseExact(endInput, format, null, System.Globalization.DateTimeStyles.None, out endDateTime) && endDateTime > startDateTime)
                {
                    if (IsDateNotIntersecting(startDateTime, endDateTime))
                    {
                        return (startDateTime, endDateTime);
                    }
                    else
                    {
                        Console.WriteLine("Введенные даты пересекаются с существующими встречами.");
                    }
                }
                else
                {
                    Console.WriteLine("Неправильный формат или дата конца встречи меньше или равна дате начала.");
                }
            }
            else
            {
                Console.WriteLine("Неправильный формат или дата начала встречи меньше или равна текущей дате. Пожалуйста, введите корректную дату и время начала:");
            }
        }
    }

    private bool IsDateNotIntersecting(DateTime proposedStart, DateTime proposedEnd)
    {
        foreach (var meeting in _meetingsList)
        {
            if ((proposedStart >= meeting.StartTime && proposedStart <= meeting.EndTime) ||
                (proposedEnd >= meeting.StartTime && proposedEnd <= meeting.EndTime))
            {
                return false; // Даты пересекаются с существующей встречей
            }
        }
        return true; // Даты не пересекаются с ни одной из встреч
    }

}