using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;
using Server.Models;

namespace Server.Core
{
    public class DatabaseManager
    {
        private readonly string dbFile = "StudentManager.db";
        private readonly string connStr;

        public DatabaseManager()
        {
            connStr = $"Data Source={dbFile}";
        }

        public void Initialize()
        {
            using (var conn = new SqliteConnection(connStr))
            {
                conn.Open();

                // 1. Users Table
                string tblUsers = @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Username TEXT PRIMARY KEY,
                        Password TEXT,
                        Role TEXT,
                        FullName TEXT,
                        ContactEmail TEXT,
                        AssignedClass TEXT,
                        Subject TEXT
                    );";
                new SqliteCommand(tblUsers, conn).ExecuteNonQuery();

                // Migration - add new columns if they don't exist
                try { new SqliteCommand("ALTER TABLE Users ADD COLUMN FullName TEXT", conn).ExecuteNonQuery(); } catch { }
                try { new SqliteCommand("ALTER TABLE Users ADD COLUMN ContactEmail TEXT", conn).ExecuteNonQuery(); } catch { }
                try { new SqliteCommand("ALTER TABLE Users ADD COLUMN AssignedClass TEXT", conn).ExecuteNonQuery(); } catch { }
                try { new SqliteCommand("ALTER TABLE Users ADD COLUMN Subject TEXT", conn).ExecuteNonQuery(); } catch { }

                var checkUserCmd = new SqliteCommand("SELECT COUNT(*) FROM Users", conn);
                if ((long)checkUserCmd.ExecuteScalar() == 0)
                {
                    string initUsers = @"
                        INSERT INTO Users VALUES ('admin@admin.edu.vn', 'admin123', 'ADMIN', 'Quản Trị Viên');
                        INSERT INTO Users VALUES ('gv01@school.edu.vn', '123', 'USER', 'Nguyễn Văn A');";
                    new SqliteCommand(initUsers, conn).ExecuteNonQuery();
                }

                // 2. Students Table
                string tblStudents = @"
                    CREATE TABLE IF NOT EXISTS Students (
                        StudentID TEXT PRIMARY KEY,
                        FullName TEXT,
                        Class TEXT,
                        Phone TEXT,
                        Email TEXT,
                        Subject TEXT
                    );";
                new SqliteCommand(tblStudents, conn).ExecuteNonQuery();

                // Migration: Add columns if they don't exist
                try { new SqliteCommand("ALTER TABLE Students ADD COLUMN Phone TEXT", conn).ExecuteNonQuery(); } catch { }
                try { new SqliteCommand("ALTER TABLE Students ADD COLUMN Email TEXT", conn).ExecuteNonQuery(); } catch { }
                try { new SqliteCommand("ALTER TABLE Students ADD COLUMN Subject TEXT", conn).ExecuteNonQuery(); } catch { }

                var checkStudentCmd = new SqliteCommand("SELECT COUNT(*) FROM Students", conn);
                if ((long)checkStudentCmd.ExecuteScalar() == 0)
                {
                    string initStudents = "INSERT INTO Students VALUES ('SV001', 'Nguyen Van A', 'CNTT', '0909123456', 'nva@email.com', 'Lap Trinh C#')";
                    new SqliteCommand(initStudents, conn).ExecuteNonQuery();
                }
            }
        }

        // --- User Operations ---

        public User Authenticate(string username, string password)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("SELECT Role, FullName FROM Users WHERE Username=@u AND Password=@p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                string fn = r["FullName"]?.ToString();
                return new User { 
                    Username = username, 
                    Role = r["Role"].ToString(), 
                    FullName = string.IsNullOrWhiteSpace(fn) ? username : fn
                };
            }
            return null;
        }

        public List<User> GetAllUsers()
        {
            var list = new List<User>();
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("SELECT Username, FullName, Role, ContactEmail, AssignedClass, Subject FROM Users", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                string fn = r["FullName"]?.ToString();
                list.Add(new User {
                    Username = r["Username"].ToString(),
                    FullName = string.IsNullOrWhiteSpace(fn) ? r["Username"].ToString() : fn,
                    Role = r["Role"].ToString(),
                    ContactEmail = r["ContactEmail"] is DBNull ? "" : r["ContactEmail"].ToString(),
                    AssignedClass = r["AssignedClass"] is DBNull ? "" : r["AssignedClass"].ToString(),
                    Subject = r["Subject"] is DBNull ? "" : r["Subject"].ToString()
                });
            }
            return list;
        }

        public bool CreateUser(string u, string p, string r, string fn, string contactEmail = "", string assignedClass = "", string subject = "")
        {
            try {
                using var conn = new SqliteConnection(connStr);
                conn.Open();
                var cmd = new SqliteCommand("INSERT INTO Users (Username, Password, Role, FullName, ContactEmail, AssignedClass, Subject) VALUES (@u, @p, @r, @fn, @ce, @ac, @sb)", conn);
                cmd.Parameters.AddWithValue("@u", u);
                cmd.Parameters.AddWithValue("@p", p);
                cmd.Parameters.AddWithValue("@r", r);
                cmd.Parameters.AddWithValue("@fn", fn);
                cmd.Parameters.AddWithValue("@ce", contactEmail);
                cmd.Parameters.AddWithValue("@ac", assignedClass);
                cmd.Parameters.AddWithValue("@sb", subject);
                return cmd.ExecuteNonQuery() > 0;
            } catch { return false; }
        }

        public bool DeleteUser(string username)
        {
            if (username == "admin@admin.edu.vn") return false;
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("DELETE FROM Users WHERE Username=@u", conn);
            cmd.Parameters.AddWithValue("@u", username);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateUser(string u, string p, string r, string fn, string contactEmail = null, string assignedClass = null, string subject = null)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            string sql = "UPDATE Users SET Role=@r";
            if (!string.IsNullOrEmpty(p)) sql += ", Password=@p";
            if (!string.IsNullOrEmpty(fn)) sql += ", FullName=@fn";
            if (contactEmail != null) sql += ", ContactEmail=@ce";
            if (assignedClass != null) sql += ", AssignedClass=@ac";
            if (subject != null) sql += ", Subject=@sb";
            sql += " WHERE Username=@u";

            var cmd = new SqliteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@u", u);
            cmd.Parameters.AddWithValue("@r", r);
            if (!string.IsNullOrEmpty(p)) cmd.Parameters.AddWithValue("@p", p);
            if (!string.IsNullOrEmpty(fn)) cmd.Parameters.AddWithValue("@fn", fn);
            if (contactEmail != null) cmd.Parameters.AddWithValue("@ce", contactEmail);
            if (assignedClass != null) cmd.Parameters.AddWithValue("@ac", assignedClass);
            if (subject != null) cmd.Parameters.AddWithValue("@sb", subject);
            return cmd.ExecuteNonQuery() > 0;
        }

        // --- Student Operations ---

        public bool StudentExists(string id)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("SELECT COUNT(*) FROM Students WHERE StudentID=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return (long)cmd.ExecuteScalar() > 0;
        }

        public bool AddStudent(string id, string name, string cls, string phone, string email, string subject)
        {
            try {
                using var conn = new SqliteConnection(connStr);
                conn.Open();
                var cmd = new SqliteCommand("INSERT INTO Students VALUES (@id, @n, @c, @p, @e, @s)", conn);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@c", cls);
                cmd.Parameters.AddWithValue("@p", phone);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@s", subject);
                return cmd.ExecuteNonQuery() > 0;
            } catch { return false; }
        }

        public bool UpdateStudent(string id, string name, string cls, string phone, string email, string subject)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("UPDATE Students SET FullName=@n, Class=@c, Phone=@p, Email=@e, Subject=@s WHERE StudentID=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@c", cls);
            cmd.Parameters.AddWithValue("@p", phone);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@s", subject);
            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteStudent(string id)
        {
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("DELETE FROM Students WHERE StudentID=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public List<Student> SearchStudents(string type, string val)
        {
            var list = new List<Student>();
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            string query = type switch {
                "ID" => "SELECT * FROM Students WHERE StudentID=@v",
                "CLASS" => "SELECT * FROM Students WHERE Class LIKE @v",
                _ => "SELECT * FROM Students WHERE StudentID LIKE @v OR FullName LIKE @v OR Class LIKE @v"
            };
            var cmd = new SqliteCommand(query, conn);
            cmd.Parameters.AddWithValue("@v", type == "ID" ? val : $"%{val}%");
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Student {
                    StudentID = r["StudentID"].ToString(),
                    FullName = r["FullName"].ToString(),
                    Class = r["Class"].ToString(),
                    Phone = r["Phone"] is DBNull ? "" : r["Phone"].ToString(),
                    Email = r["Email"] is DBNull ? "" : r["Email"].ToString(),
                    Subject = r["Subject"] is DBNull ? "" : r["Subject"].ToString()
                });
            }
            return list;
        }

        public List<Student> GetAllStudents()
        {
            var list = new List<Student>();
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("SELECT * FROM Students ORDER BY FullName", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(new Student {
                    StudentID = r["StudentID"].ToString(),
                    FullName = r["FullName"].ToString(),
                    Class = r["Class"].ToString(),
                    Phone = r["Phone"] is DBNull ? "" : r["Phone"].ToString(),
                    Email = r["Email"] is DBNull ? "" : r["Email"].ToString(),
                    Subject = r["Subject"] is DBNull ? "" : r["Subject"].ToString()
                });
            }
            return list;
        }
        public List<string> GetAllClasses()
        {
            var list = new List<string>();
            using var conn = new SqliteConnection(connStr);
            conn.Open();
            var cmd = new SqliteCommand("SELECT DISTINCT Class FROM Students WHERE Class IS NOT NULL AND Class != '' ORDER BY Class", conn);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add(r["Class"].ToString());
            return list;
        }
    }
}
