using MeetingPlannerConsole.Model;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;

namespace MeetingPlannerConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DBConnection dBCconnection = new DBConnection();
            DBController dBController = new DBController();
            MenuController menuController = new MenuController();
            MeetingScheduler scheduler = new MeetingScheduler();

            OracleConnection oracleConnection = dBCconnection.Connection();
            List<MeetingModel> meetingsList = dBController.SelectAll(oracleConnection);

            menuController.Init(oracleConnection, dBController, meetingsList, dBCconnection, scheduler);

            while (true)
            {
                menuController.PrintMenu();
            }
        }
    }
}
