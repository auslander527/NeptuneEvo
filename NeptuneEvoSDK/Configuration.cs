using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace Redage.SDK
{
    public class Config
    {
        private Dictionary<string, object> configs;
        private string Category;
        private string DBCONN =
            "Data Source=Settings.db;Version=3;";

        /// <summary>
        /// Инициализация. Требуется во всех местах, где будут использоваться конфиги.
        /// </summary>
        /// <param name="category_">Идентификатор группы конфигов</param>
        public Config(string category_)
        {
            configs = new Dictionary<string, object>();
            Category = category_;

            using (SQLiteConnection connection = new SQLiteConnection())
            {
                connection.ConnectionString = DBCONN;
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    //Создать, если нету
                    command.CommandText = $"CREATE TABLE IF NOT EXISTS '{Category}' " +
                        "(Param TEXT NOT NULL UNIQUE, Value TEXT, PRIMARY KEY(Param))";
                    command.ExecuteNonQuery();
                    //Получить
                    command.CommandText = $"SELECT * FROM '{Category}'";

                    SQLiteDataReader reader = command.ExecuteReader();
                    DataTable table = new DataTable();

                    table.Load(reader);

                    foreach (DataRow row in table.Rows)
                    {
                        configs.Add(row["Param"].ToString(), row["Value"]);
                        Console.WriteLine($"Loaded config: {Category} {row["Param"].ToString()} {row["Value"]}");
                    }
                }
            }
        }
        /// <summary>
        /// Устанавливает значение в конфиг или создает новый
        /// </summary>
        /// <param name="param">Название параметра конфигурации</param>
        /// <param name="value">Значение параметра</param>
        public object Set(string param, object value)
        {
            if (configs.ContainsKey(param))
            {
                configs[param] = value;
                using (SQLiteConnection connection = new SQLiteConnection())
                {
                    connection.ConnectionString = DBCONN;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = $"UPDATE '{Category}' SET 'Value'='{value.ToString()}' WHERE 'Param'='{param}'";
                        command.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                configs.Add(param, value);
                using (SQLiteConnection connection = new SQLiteConnection())
                {
                    connection.ConnectionString = DBCONN;
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText = $"INSERT INTO '{Category}'('Param','Value') VALUES ('{param}','{value.ToString()}')";
                        command.ExecuteNonQuery();
                    }
                }
            }
            return value;
        }
        /// <summary>
        /// Получает значение параметра по названию. Вернет объект или Null.
        /// </summary>
        /// <param name="param">Название параметра конфигурации</param>
        /// <returns>Возвращает объект для дальнейшей конвертации. Если параметр не найден, вернет null.</returns>
        public object Get(string param)
        {
            if (configs.ContainsKey(param))
                return configs[param];
            return null;
        }
        /// <summary>
        /// Пытается получить значение по названию параметра. Если параметр не найден, создает новый.
        /// </summary>
        /// <typeparam name="T">Тип, который вернется</typeparam>
        /// <param name="param">Название параметра конфигурации</param>
        /// <param name="_default">Значение по умолчанию для данного типа</param>
        /// <returns>Возвращает найденый объект с указанным типом (T)</returns>
        public T TryGet<T>(string param, object _default)
        {
            if (!configs.ContainsKey(param))
            {
                Set(param, _default);
                return (T)Convert.ChangeType(configs[param], typeof(T));
            }
            else return (T)Convert.ChangeType(configs[param], typeof(T));
        }
    }
}
