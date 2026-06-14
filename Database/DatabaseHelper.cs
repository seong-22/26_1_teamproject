using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace BILCAM.Database
{
    public static class DatabaseHelper
    {
        private static string ConnectionString =>
            "Host=aws-1-ap-northeast-1.pooler.supabase.com;" +
            "Port=5432;" +
            "Database=postgres;" +
            "Username=postgres.jqcqmcltynoulrkqzwex;" +
            "Password=kw2026team5!;" +
            "SSL Mode=Require;" +
            "Trust Server Certificate=true;";

        public static void Initialize()
        {
            try
            {
                using (var conn = GetConnection()) { }
            }
            catch (Exception ex)
            {
                throw new Exception("Supabase 연결 실패: " + ex.Message);
            }
        }

        public static NpgsqlConnection GetConnection()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static int ExecuteNonQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static DataTable ExecuteQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                cmd.Parameters.AddRange(parameters);
                var dt = new DataTable();
                new NpgsqlDataAdapter(cmd).Fill(dt);
                return dt;
            }
        }

        public static object ExecuteScalar(string sql, params NpgsqlParameter[] parameters)
        {
            using (var conn = GetConnection())
            using (var cmd = new NpgsqlCommand(sql, conn))
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
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
        //예약 자동 삭제 함수
        public static void DeleteExpiredPendingReservations()
        {
            ExecuteNonQuery(
                "DELETE FROM reservations WHERE status='pending' AND reservationdate::date < CURRENT_DATE");
        }
    }
}
