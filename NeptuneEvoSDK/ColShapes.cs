using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;

namespace Redage.SDK
{
    public class ColShapeData
    {
        /// <summary>
        /// StringID телепорта в базе данных и ключ к dictionary
        /// </summary>
        public string SID { get; set; }
        /// <summary>
        /// Доступ для фракций, 0 = для всех фракций и гражданских
        /// </summary>
        public byte Fraction { get; set; }
        /// <summary>
        /// Доступ для рангов с определённого ранга и выше
        /// </summary>
        public byte Rank { get; set; }
        /// <summary>
        /// Позиция X Y Z откуда идёт первый телепорт
        /// </summary>
        public Vector3 FromPos { get; set; }
        /// <summary>
        /// Номер Dimension откуда идёт первый телепорт
        /// </summary>
        public uint FromDim { get; set; }
        /// <summary>
        /// Позиция X Y Z куда телепортирует первый телепорт и, если что, позиция второго телепорта, если Revers = 1
        /// </summary>
        public Vector3 ToPos { get; set; }
        /// <summary>
        /// Номер Dimension в который телепортируется игрок, и, если что, номер dimension с которого действует второй маркер, если Revers = 1
        /// </summary>
        public uint ToDim { get; set; }
        /// <summary>
        /// false - телепорт только со стороны FromPos на ToPos, если true, то 2 телепорта, один с FromPos до ToPos, второй с ToPos до FromPos
        /// </summary>
        public bool Revers { get; set; }
        /// <summary>
        /// false- телепорт не действует на машину, если true - действует.
        /// </summary>
        public bool ForVeh { get; set; }
        /// <summary>
        /// false - отключить обработку нажатия на кнопку E на первой точке, true - включить
        /// </summary>
        public bool Interact { get; set; }
        public ColShape[] Tp { get; set; } = new ColShape[2] { null, null };
    }
    /// <summary>
    /// Новый контроллер колшейпов
    /// Принцип сохранения и загрузки точек аналогичен SDK.Configuration.
    /// [RemoteEvent("ColShapeInteract")]
    /// Подписка на событие взаимодействия игрока с чекпоинтом SID
    /// </summary>
    public class ColShapes : Script, IEnumerable
    {
        private Dictionary<string, ColShapeData> arr;
        private string Category;
        private bool Sqlite;

        /// <summary>
        /// Получаем список всех колшейпов из базы данных
        /// </summary>
        /// <param name="sqlite_">Работа с данными из SQLite или MySQL</param>
        /// <param name="category_">Категория. Null = Общая для всего мода.</param>
        public ColShapes(bool sqlite_ = false, string category_ = "Main")
        {
            arr = new Dictionary<string, ColShapeData>();
            Category = category_;
            Sqlite = sqlite_;

            if (sqlite_) LoadSQLite();
            else LoadMySQL();
        }

        private void LoadMySQL()
        {
            #region SQL
            MySqlCommand command = new MySqlCommand();
            command.CommandText =
$"CREATE TABLE IF NOT EXISTS `ColShapes_{Category}` (" +
  "`SID` TINYTEXT NOT NULL COLLATE 'utf8_bin'," +
  "`Fraction` tinyint(3) NOT NULL DEFAULT '0'," +
  "`Rank` tinyint(2) NOT NULL DEFAULT '0'," +
  "`FPosX` float NOT NULL DEFAULT '0'," +
  "`FPosY` float NOT NULL DEFAULT '0'," +
  "`FPosZ` float NOT NULL DEFAULT '0'," +
  "`FPosDim` float NOT NULL DEFAULT '0'," +
  "`TPosX` float NOT NULL DEFAULT '0'," +
  "`TPosY` float NOT NULL DEFAULT '0'," +
  "`TPosZ` float NOT NULL DEFAULT '0'," +
  "`TPosDim` float NOT NULL DEFAULT '0'," +
  "`Revers` tinyint(1) NOT NULL DEFAULT '0'," +
  "`ForVeh` tinyint(1) NOT NULL DEFAULT '0'," +
  "`Interact` tinyint(1) NOT NULL DEFAULT '0'," +
"PRIMARY KEY (`SID`(64))" +
") ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;" +
"SELECT * FROM `ColShapes`";
            #endregion

            DataTable table = MySQL.QueryRead(command);
            if (table is null) return;

            foreach(DataRow row in table.Rows)
            {
                LoadFromDB(row);
            }
        }

        private void LoadSQLite()
        {
            using (SQLiteConnection connection = new SQLiteConnection())
            {
                connection.ConnectionString =
                    "Data Source=ColShapes.db;Version=3;";
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    #region SQL
                    command.CommandText =
                        "'CREATE TABLE 'Main' (" +
                        "'SID'	TEXT NOT NULL UNIQUE," +
                        "'Fraction'	INTEGER NOT NULL DEFAULT 0," +
                        "'Rank'	INTEGER NOT NULL DEFAULT 0," +
                        "'FPosX'	REAL NOT NULL DEFAULT 0," +
                        "'FPosY'	REAL NOT NULL DEFAULT 0," +
                        "'FPosZ'	REAL NOT NULL DEFAULT 0," +
                        "'FPosDim'	REAL NOT NULL DEFAULT 0," +
                        "'TPosX'	REAL NOT NULL DEFAULT 0," +
                        "'TPosY'	REAL NOT NULL DEFAULT 0," +
                        "'TPosZ'	REAL NOT NULL DEFAULT 0," +
                        "'TPosDim'	REAL NOT NULL DEFAULT 0," +
                        "'Revers'	INTEGER NOT NULL DEFAULT 0," +
                        "'ForVeh'	INTEGER NOT NULL DEFAULT 0," +
                        "'Interact'	INTEGER NOT NULL DEFAULT 0," +
                        "PRIMARY KEY('SID'));" +
                        $"SELECT * FROM '{Category}';";
                    #endregion

                    SQLiteDataReader reader = command.ExecuteReader();
                    DataTable table = new DataTable();

                    table.Load(reader);

                    foreach (DataRow row in table.Rows)
                    {
                        LoadFromDB(row);
                    }
                }
            }
        }

        private void LoadFromDB(DataRow row)
        {
            ColShapeData tp = new ColShapeData()
            {
                SID = Convert.ToString(row[0]),
                Fraction = Convert.ToByte(row[1]),
                Rank = Convert.ToByte(row[2]),
                FromPos = new Vector3((float)row[3], (float)row[4], (float)row[5]),
                FromDim = Convert.ToUInt32(row[6]),
                ToPos = new Vector3((float)row[7], (float)row[8], (float)row[9]),
                ToDim = Convert.ToUInt32(row[10]),
                Revers = Convert.ToBoolean(row[11]),
                ForVeh = Convert.ToBoolean(row[12]),
                Interact = Convert.ToBoolean(row[13])
            };
            tp.Tp[0] = NAPI.ColShape.CreateSphereColShape(tp.FromPos, 3f, tp.FromDim);
            if (tp.Revers) tp.Tp[1] = NAPI.ColShape.CreateSphereColShape(tp.ToPos, 3f, tp.ToDim);
            if (tp.Interact)
            {
                tp.Tp[0].OnEntityEnterColShape += (ColShape c, Client p) =>
                {
                    p.SetData("ColShapeInteract", tp.SID);
                };
                tp.Tp[0].OnEntityExitColShape += (ColShape c, Client p) =>
                {
                    p.ResetData("ColShapeInteract");
                };
            }

            arr.Add(tp.SID, tp);
        }

        /// <summary>
        /// Перезагрузка всех точек из базы данных 
        /// </summary>
        public void Reload()
        {
            arr.Clear();

            if (Sqlite) LoadSQLite();
            else LoadMySQL();
        }

        /// <summary>
        /// Проверить доступ к телепорту
        /// </summary>
        /// <param name="player">Игрок, которого проверяем</param>
        /// <param name="sid">StringID телепорта</param>
        /// <returns>True - Доступ есть</returns>
        public bool CheckTeleportAccess(Client player, string sid)
        {
            if (arr[sid].Fraction != 0 && arr[sid].Fraction == Utils.GetCharacter(player).FractionID && arr[sid].Rank >= Utils.GetCharacter(player).FractionLVL) return false;
            return true;
        }

        /// <summary>
        /// Сохранить данные телепорта в БД
        /// </summary>
        /// <param name="sid">StringID телепорта</param>
        /// <returns>True - если данные были обновлены</returns>
        public bool Save(string sid)
        {
            if (arr.ContainsKey(sid))
            {
                ColShapeData data = arr[sid];
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "UPDATE `ColShapes` SET Fraction=@fr,Rank=@ra,FPosX=@fX,FPosY=@fY,FPosZ=@fZ,FPosDim=@fDim," +
                        "TPosX=@tX,TPosY=@tY,TPosZ=@tZ,TPosDim=@tDim,Revers=@rev,ForVeh=@veh,Interact=@itr WHERE ID=@id LIMIT 1"
                };
                cmd.Parameters.AddWithValue("@fr", data.Fraction);
                cmd.Parameters.AddWithValue("@ra", data.Rank);
                cmd.Parameters.AddWithValue("@fX", data.FromPos.X);
                cmd.Parameters.AddWithValue("@fY", data.FromPos.Y);
                cmd.Parameters.AddWithValue("@fZ", data.FromPos.Z);
                cmd.Parameters.AddWithValue("@fDim", data.FromDim);
                cmd.Parameters.AddWithValue("@tX", data.ToPos.X);
                cmd.Parameters.AddWithValue("@tY", data.ToPos.Y);
                cmd.Parameters.AddWithValue("@tZ", data.ToPos.Z);
                cmd.Parameters.AddWithValue("@tDim", data.ToDim);
                cmd.Parameters.AddWithValue("@rev", data.Revers);
                cmd.Parameters.AddWithValue("@veh", data.ForVeh);
                cmd.Parameters.AddWithValue("@itr", data.Interact);
                cmd.Parameters.AddWithValue("@id", data.SID);
                MySQL.Query(cmd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Создать новую точку
        /// </summary>
        /// <param name="data">Данные точки</param>
        /// <returns>True - если точка была создана</returns>
        public bool Create(ColShapeData data)
        {
            if (!arr.ContainsKey(data.SID))
            {
                MySqlCommand cmd = new MySqlCommand
                {
                    CommandText = "INSERT INTO `test`.`colshapes`(`ID`,`Fraction`,`Rank`,`FPosX`,`FPosY`,`FPosZ`,`FPosDim`,`TPosX`,`TPosY`,`TPosZ`,`TPosDim`,`Revers`,`ForVeh`,`Interact`) " +
                    "VALUES (@id,@fr,@ra,@fX,@fY,@fZ,@fDim,@tX,@tY,@tZ,@tDim,@rev,@veh,@itr);"
                };
                cmd.Parameters.AddWithValue("@fr", data.Fraction);
                cmd.Parameters.AddWithValue("@ra", data.Rank);
                cmd.Parameters.AddWithValue("@fX", data.FromPos.X);
                cmd.Parameters.AddWithValue("@fY", data.FromPos.Y);
                cmd.Parameters.AddWithValue("@fZ", data.FromPos.Z);
                cmd.Parameters.AddWithValue("@fDim", data.FromDim);
                cmd.Parameters.AddWithValue("@tX", data.ToPos.X);
                cmd.Parameters.AddWithValue("@tY", data.ToPos.Y);
                cmd.Parameters.AddWithValue("@tZ", data.ToPos.Z);
                cmd.Parameters.AddWithValue("@tDim", data.ToDim);
                cmd.Parameters.AddWithValue("@rev", data.Revers);
                cmd.Parameters.AddWithValue("@veh", data.ForVeh);
                cmd.Parameters.AddWithValue("@itr", data.Interact);
                cmd.Parameters.AddWithValue("@id", data.SID);
                MySQL.Query(cmd);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Вернет список данных всех точек
        /// </summary>
        public IEnumerator GetEnumerator()
        {
            return ((IEnumerable)arr).GetEnumerator();
        }

        /// <summary>
        /// Получить/установить ColShapeData по StringID
        /// </summary>
        /// <param name="sid">StringId Точки</param>
        public ColShapeData this[string sid]
        {
            get
            {
                return arr[sid];
            }
            set
            {
                arr[sid] = value;
            }
        }
    }
}
