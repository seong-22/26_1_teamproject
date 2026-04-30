using System;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BILCAM.Database
{
    public static class DatabaseHelper
    {
        private static string _dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bilcam.db");
        private static string ConnectionString => $"Data Source={_dbPath};Version=3;";

        public static void Initialize()
        {
            bool isNew = !File.Exists(_dbPath);
            using (var conn = new SQLiteConnection(ConnectionString))
            {
                conn.Open();
                if (isNew) CreateSchema(conn);
            }
            if (isNew) SeedData();
        }

        private static void CreateSchema(SQLiteConnection conn)
        {
            string sql = @"
                CREATE TABLE IF NOT EXISTS Users (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId TEXT NOT NULL UNIQUE,
                    PasswordHash TEXT NOT NULL,
                    Name TEXT NOT NULL,
                    Role TEXT NOT NULL DEFAULT 'student'
                );
                CREATE TABLE IF NOT EXISTS Resources (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Category TEXT NOT NULL,
                    Location TEXT NOT NULL,
                    IsAvailable INTEGER NOT NULL DEFAULT 1
                );
                CREATE TABLE IF NOT EXISTS Reservations (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    UserId TEXT NOT NULL,
                    ResourceId INTEGER NOT NULL,
                    ReservationDate TEXT NOT NULL,
                    StartTime TEXT NOT NULL,
                    EndTime TEXT NOT NULL,
                    Status TEXT NOT NULL DEFAULT 'pending',
                    CreatedAt TEXT NOT NULL,
                    FOREIGN KEY (ResourceId) REFERENCES Resources(Id)
                );";
            using (var cmd = new SQLiteCommand(sql, conn))
                cmd.ExecuteNonQuery();
        }

        private static void SeedData()
        {
            // Seed users
            ExecuteNonQuery($"INSERT INTO Users (UserId, PasswordHash, Name, Role) VALUES ('student', '{HashPassword("1234")}', '오성이', 'student')");
            ExecuteNonQuery($"INSERT INTO Users (UserId, PasswordHash, Name, Role) VALUES ('admin', '{HashPassword("1234")}', '관리자', 'admin')");

            // Seed resources
            string[] resources = {
                "('강의실 101호', 'classroom', '새빛관 1층', 1)",
                "('강의실 102호', 'classroom', '새빛관 1층', 1)",
                "('강의실 103호', 'classroom', '새빛관 1층', 1)",
                "('강의실 104호', 'classroom', '새빛관 1층', 1)",
                "('강의실 201호', 'classroom', '새빛관 2층', 0)",
                "('강의실 202호', 'classroom', '새빛관 2층', 0)",
                "('강의실 203호', 'classroom', '새빛관 2층', 0)",
                "('강의실 204호', 'classroom', '새빛관 2층', 0)",
                "('노트북 #01', 'laptop', '인융대 학생회실', 1)",
                "('노트북 #02', 'laptop', '인융대 학생회실', 1)",
                "('노트북 #03', 'laptop', '인융대 학생회실', 0)",
                "('우산 대여 (1번)', 'umbrella', '인융대 학생회실', 1)",
                "('우산 대여 (2번)', 'umbrella', '인융대 학생회실', 0)",
            };
            foreach (var r in resources)
                ExecuteNonQuery($"INSERT INTO Resources (Name, Category, Location, IsAvailable) VALUES {r}");

            // Seed reservations
            string today = DateTime.Today.ToString("yyyy-MM-dd");
            string tomorrow = DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");
            ExecuteNonQuery($"INSERT INTO Reservations (UserId, ResourceId, ReservationDate, StartTime, EndTime, Status, CreatedAt) VALUES ('student', 1, '{today}', '13:00', '14:00', 'approved', '{DateTime.Now}')");
            ExecuteNonQuery($"INSERT INTO Reservations (UserId, ResourceId, ReservationDate, StartTime, EndTime, Status, CreatedAt) VALUES ('student', 4, '{tomorrow}', '10:00', '11:00', 'pending', '{DateTime.Now}')");
        }

        public static SQLiteConnection GetConnection()
        {
            var conn = new SQLiteConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static int ExecuteNonQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static DataTable ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                var dt = new DataTable();
                new SQLiteDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static object ExecuteScalar(string sql, params SQLiteParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new SQLiteCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteScalar();
            }
        }

        public static string HashPassword(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
    }
}
