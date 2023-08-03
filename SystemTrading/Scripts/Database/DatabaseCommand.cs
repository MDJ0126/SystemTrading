using MySql.Data.MySqlClient;
using System;

public static class DatabaseCommand
{
    public const string SERVER      = "localhost";
    public const string PORT        = "3306";
    public const string DATABASE    = "systemtrading";
    public const string UID         = "root";
    public const string PWD         = "password";

    private static MySqlConnection GetMySqlConnection()
    {
        return new MySqlConnection($"Server={SERVER};Port={PORT};Database={DATABASE};Uid={UID};Pwd={PWD}");
    }

    public static void Query(string query)
    {
        using (MySqlConnection connection = GetMySqlConnection())
        {
            try
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(query, connection);
                int resultRowCount = command.ExecuteNonQuery();
                if (resultRowCount == 0) Logger.Log("인서트 실패");
            }
            catch (Exception ex)
            {
                Logger.Log("실패");
                Logger.Log(ex.ToString());
            }

        }
    }
}
