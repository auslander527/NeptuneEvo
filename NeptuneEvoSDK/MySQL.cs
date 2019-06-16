using System;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Data;
using System.Threading.Tasks;

namespace Redage.SDK
{
    public static class MySQL
    {
        private static Config config = new Config("MySQL");
        private static nLog Log = new nLog("MySQL");

        private static string Connection = null;

        public static bool Debug = false;

        public static void Init()
        {
            if (Connection is string) return;
            Connection = 
                $"Host={config.TryGet<string>("Server", "127.0.0.1")};" +
                $"User={config.TryGet<string>("User", "")};" +
                $"Password={config.TryGet<string>("Password", "")};" +
                $"Database={config.TryGet<string>("DataBase", "")};" +
                $"{config.TryGet<string>("SSL", "SslMode=None;")}";
        }

        /// <summary>
        /// Тест соединения с базой
        /// </summary>
        /// <returns>True - если все хорошо</returns>
        public static bool Test()
        {
            Log.Debug("Testing connection...");
            try
            {
                using(MySqlConnection conn = new MySqlConnection(Connection))
                {
                    conn.Open();
                    Log.Debug("Connection is successful!", nLog.Type.Success);
                    conn.Close();
                }
                return true;
            }
            catch (ArgumentException ae)
            {
                Log.Write($"Сonnection string contains an error\n{ae.ToString()}", nLog.Type.Error);
                return false;
            }
            catch (MySqlException me)
            {
                switch (me.Number)
                {
                    case 1042:
                        Log.Write("Unable to connect to any of the specified MySQL hosts", nLog.Type.Error);
                        break;
                    case 0:
                        Log.Write("Access denied", nLog.Type.Error);
                        break;
                    default:
                        Log.Write($"({me.Number}) {me.Message}", nLog.Type.Error);
                        break;
                }
                return false;
            }
        }

        /// <summary>
        /// Выполнить запрос без ответа
        /// </summary>
        /// <param name="command">Передаем заранее составленную команду</param>
        public static void Query(MySqlCommand command)
        {
            try
            {
                if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    connection.Open();

                    command.Connection = connection;

                    command.ExecuteNonQuery();
                }
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        /// <summary>
        /// Выполнить запрос без ответа
        /// </summary>
        /// <param name="command">Передаем команду в виде строки</param>
        public static void Query(string command)
        {
            using(MySqlCommand cmd = new MySqlCommand(command))
            {
                Query(cmd);
            }
        }
        /// <summary>
        /// Выполнить запрос без ответа
        /// </summary>
        /// <param name="command">Передаем заранее составленную команду</param>
        public static async Task QueryAsync(MySqlCommand command)
        {
            try
            {
                if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    await connection.OpenAsync();

                    command.Connection = connection;

                    await command.ExecuteNonQueryAsync();
                }
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        /// <summary>
        /// Выполнить запрос без ответа
        /// </summary>
        /// <param name="command">Передаем команду в виде строки</param>
        public static async Task QueryAsync(string command)
        {
            try
            {
                if (Debug) Log.Debug("Query to DB:\n" + command);
                using (MySqlConnection connection = new MySqlConnection(Connection))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = connection;
                        cmd.CommandText = command;

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e) { Log.Write(e.ToString(), nLog.Type.Error); }
        }
        /// <summary>
        /// Отправить запрос и считать ответ
        /// </summary>
        /// <param name="command">Передаем заранее составленную команду</param>
        /// <returns>Ответ базы данных в формате таблицы</returns>
        public static DataTable QueryRead(MySqlCommand command)
        {
            if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
            using (MySqlConnection connection = new MySqlConnection(Connection))
            {
                connection.Open();

                command.Connection = connection;

                DbDataReader reader = command.ExecuteReader();
                DataTable result = new DataTable();
                result.Load(reader);

                return result;
            }
        }
        /// <summary>
        /// Отправить запрос и считать ответ
        /// </summary>
        /// <param name="command">Передаем команду в виде строки</param>
        /// <returns>Ответ базы данных в формате таблицы</returns>
        public static DataTable QueryRead(string command)
        {
            using(MySqlCommand cmd = new MySqlCommand(command))
            {
                return QueryRead(cmd);
            }
        }
        /// <summary>
        /// Асинхронная версия Read
        /// </summary>
        /// <param name="command">Передаем заранее составленную команду</param>
        /// <returns>Ответ базы данных в формате таблицы</returns>
        public static async Task<DataTable> QueryReadAsync(MySqlCommand command)
        {
            if (Debug) Log.Debug("Query to DB:\n" + command.CommandText);
            using (MySqlConnection connection = new MySqlConnection(Connection))
            {
                await connection.OpenAsync();

                command.Connection = connection;

                DbDataReader reader = await command.ExecuteReaderAsync();
                DataTable result = new DataTable();
                result.Load(reader);

                return result;
            }
        }
        /// <summary>
        /// Асинхронная версия Read
        /// </summary>
        /// <param name="command">Передаем заранее составленную команду</param>
        /// <returns>Ответ базы данных в формате таблицы</returns>
        public static async Task<DataTable> QueryReadAsync(string command)
        {
            using(MySqlCommand cmd = new MySqlCommand(command))
            {
                return await QueryReadAsync(cmd);
            }
        }
        
        public static string ConvertTime(DateTime dateTime)
        {
            return dateTime.ToString("s");
        }
    }
}