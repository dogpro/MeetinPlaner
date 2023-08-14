using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class DBConnection
{
    private OracleConnection connection;

    #region Connection
    public OracleConnection Connection()
    {
        connection = new OracleConnection();
        connection.ConnectionString = "DATA SOURCE=127.0.0.1:1521/orcl;DBA PRIVILEGE=SYSDBA;TNS_ADMIN=C:\\Users\\mrdru\\Oracle\\network\\admin;PERSIST SECURITY INFO=True;USER ID=SYS;PASSWORD=123;";
        connection.Open();

#if DEBUG
        Console.WriteLine("Connected to Oracle" + connection.ServerVersion);
#endif
        return connection;
    }

    public void CloseConnection()
    {
        connection.Close();
        connection.Dispose();
#if DEBUG
        Console.WriteLine("Connection close");
#endif
    }
    #endregion
}
