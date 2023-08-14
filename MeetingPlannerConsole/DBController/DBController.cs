using MeetingPlannerConsole.Model;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class DBController
{
    public List<MeetingModel> SelectAll(OracleConnection connection)
    {
        string sqlExpression = "SELECT * FROM MEETINGS_TABLE";

        List<MeetingModel> meetingList = new List<MeetingModel>();

        using (OracleCommand command = new OracleCommand())
        {
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlExpression;
            OracleDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                MeetingModel meeting = new MeetingModel(Convert.ToInt32(reader[0]), reader[1].ToString(), Convert.ToDateTime(reader[2]), Convert.ToDateTime(reader[3]),
                                            reader[4].ToString(), Convert.ToDateTime(reader[5]));

                meetingList.Add(meeting);
            }
        }

        return meetingList;
    }

    public List<MeetingModel> SelectWhere(OracleConnection connection, string selectedDate)
    {
        string sqlExpression = $"SELECT* FROM MEETINGS_TABLE WHERE TRUNC(START_TIME) = TO_DATE('{selectedDate}', 'YYYY-MM-DD')";

        List<MeetingModel> meetingList = new List<MeetingModel>();

        using (OracleCommand command = new OracleCommand())
        {
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlExpression;
            OracleDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                MeetingModel meeting = new MeetingModel(Convert.ToInt32(reader[0]), reader[1].ToString(), Convert.ToDateTime(reader[2]), Convert.ToDateTime(reader[3]),
                                            reader[4].ToString(), Convert.ToDateTime(reader[5]));

                meetingList.Add(meeting);
            }
        }

        return meetingList;
    }

    public void Insert(OracleConnection connection, MeetingModel meeting)
    {

        string startDateToString = String.Format("{0:yyyy-MM-dd HH-mm-ss}", meeting.StartTime);
        string endDateToString = String.Format("{0:yyyy-MM-dd HH-mm-ss}", meeting.EndTime);
        string notificationDateToString = String.Format("{0:yyyy-MM-dd HH-mm-ss}", meeting.NotificationTime);

        string sqlExpression = $"INSERT INTO MEETINGS_TABLE (ID, NAME, START_TIME, END_TIME, DESCRIPTION, NOTIFICATION_TIME, IS_NOTIFICATION) " +
            $"VALUES ({meeting.Id}, '{meeting.Name}', TO_TIMESTAMP('{startDateToString}', 'YYYY-MM-DD HH24:MI:SS'), TO_TIMESTAMP('{endDateToString}', 'YYYY-MM-DD HH24:MI:SS'), " +
            $"'{meeting.Description}', TO_TIMESTAMP('{notificationDateToString}', 'YYYY-MM-DD HH24:MI:SS'), 0)";

        ExecuteNonQuery(connection, sqlExpression);
    }

    public void Delete(OracleConnection connection, int id)
    {
        string sqlExpression = $"DELETE FROM MEETINGS_TABLE WHERE ID = {id}";

        ExecuteNonQuery(connection, sqlExpression);
    }

    public void Update(OracleConnection connection, MeetingModel meeting)
    {
        string startDateToString = String.Format("{0:yyyy-MM-dd HH-mm-ss}", meeting.StartTime);
        string endDateToString = String.Format("{0:yyyy-MM-dd HH-mm-ss}", meeting.EndTime);
        string notificationDateToString = String.Format("{0:yyyy-MM-dd HH-mm-ss}", meeting.NotificationTime);

        Console.WriteLine( startDateToString);

        string sqlExpression = $"UPDATE MEETINGS_TABLE SET NAME = '{meeting.Name}', START_TIME = TO_TIMESTAMP('{startDateToString}', 'YYYY-MM-DD HH24:MI:SS')," +
            $" END_TIME = TO_TIMESTAMP('{endDateToString}', 'YYYY-MM-DD HH24:MI:SS'), DESCRIPTION = '{meeting.Description}', " +
            $"NOTIFICATION_TIME = TO_TIMESTAMP('{notificationDateToString}', 'YYYY-MM-DD HH24:MI:SS'), IS_NOTIFICATION = 0 WHERE ID = {meeting.Id}";

        ExecuteNonQuery(connection, sqlExpression);
    }

    private void ExecuteNonQuery(OracleConnection connection, string sqlExpression)
    {
        using (OracleCommand command = new OracleCommand())
        {
            command.Connection = connection;
            command.CommandType = CommandType.Text;
            command.CommandText = sqlExpression;

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Произошла ошибка при выполнении SQL-запроса: " + ex.Message);
            }
        }
    }
}
