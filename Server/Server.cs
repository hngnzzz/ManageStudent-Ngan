using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using Server.Core;
using Server.Models;

namespace Server
{
    class Program
    {
        private static readonly DatabaseManager db = new DatabaseManager();
        private static readonly List<TcpClient> clients = new List<TcpClient>();
        private const int Port = 8888;

        public enum LogType { INFO, ERROR, SUCCESS, CMD, DATA }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            db.Initialize();

            TcpListener listener = new TcpListener(IPAddress.Any, Port);
            listener.Start();

            Log($"SERVER started on port {Port}", LogType.SUCCESS);
            Log($"Database: {Path.GetFullPath("StudentManager.db")}");

            while (true)
            {
                try
                {
                    TcpClient client = listener.AcceptTcpClient();
                    lock (clients) clients.Add(client);
                    
                    Thread clientThread = new Thread(HandleClient);
                    clientThread.IsBackground = true;
                    clientThread.Start(client);
                }
                catch (Exception ex)
                {
                    Log($"Accept Error: {ex.Message}", LogType.ERROR);
                }
            }
        }

        private static void HandleClient(object obj)
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();
            string clientEndPoint = client.Client.RemoteEndPoint.ToString();

            Log($"Client connected: {clientEndPoint}", LogType.INFO);

            try
            {
                while (true)
                {
                    string request = Receive(stream);
                    if (string.IsNullOrEmpty(request)) break;

                    Log($"Request from {clientEndPoint}: {request}", LogType.CMD);
                    ProcessCommand(request, stream);
                }
            }
            catch (Exception ex)
            {
                Log($"Session Error ({clientEndPoint}): {ex.Message}", LogType.ERROR);
            }
            finally
            {
                lock (clients) clients.Remove(client);
                client.Close();
                Log($"Client disconnected: {clientEndPoint}", LogType.INFO);
            }
        }

        private static void ProcessCommand(string request, NetworkStream stream)
        {
            string[] p = request.Split('|');
            if (p.Length == 0) return;

            string cmd = p[0].Trim();

            switch (cmd)
            {
                case "LOGIN":
                    var user = db.Authenticate(p[1], p[2]);
                    if (user != null) Send(stream, $"LOGIN_SUCCESS|{user.Role}|{user.FullName}");
                    else Send(stream, "LOGIN_FAIL");
                    break;

                case "LIST":
                    var students = db.GetAllStudents();
                    Send(stream, "LIST_RES|" + string.Join(";", students) + ";");
                    break;

                case "ADD":
                    string p4_add = p.Length > 4 ? p[4] : "";
                    string p5_add = p.Length > 5 ? p[5] : "";
                    string p6_add = p.Length > 6 ? p[6] : "";
                    if (db.AddStudent(p[1], p[2], p[3], p4_add, p5_add, p6_add))
                    {
                        Send(stream, "ADD_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "EXISTS");
                    break;

                case "UPDATE":
                    string p4_up = p.Length > 4 ? p[4] : "";
                    string p5_up = p.Length > 5 ? p[5] : "";
                    string p6_up = p.Length > 6 ? p[6] : "";
                    if (db.UpdateStudent(p[1], p[2], p[3], p4_up, p5_up, p6_up))
                    {
                        Send(stream, "UPDATE_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "STUDENT_NOT_FOUND");
                    break;

                case "DELETE":
                    if (db.DeleteStudent(p[1]))
                    {
                        Send(stream, "DELETE_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "STUDENT_NOT_FOUND");
                    break;

                case "SEARCH":
                    var results = db.SearchStudents(p[1], p[2]);
                    if (results.Count > 0) Send(stream, "LIST_RES|" + string.Join(";", results) + ";");
                    else Send(stream, "STUDENT_NOT_FOUND");
                    break;

                case "CHAT":
                    Broadcast($"CHAT|{p[1]}|{p[2]}");
                    break;

                case "LIST_USERS":
                    var users = db.GetAllUsers();
                    Send(stream, "LIST_USERS_RES|" + string.Join(";", users) + ";");
                    break;

                case "LIST_CLASSES":
                    var classes = db.GetAllClasses();
                    Send(stream, "LIST_CLASSES_RES|" + string.Join(";", classes) + ";");
                    break;

                case "CREATE_USER":
                    string fn = p.Length > 4 ? p[4] : "Giáo Viên";
                    string ce = p.Length > 5 ? p[5] : "";
                    string ac = p.Length > 6 ? p[6] : "";
                    string sb = p.Length > 7 ? p[7] : "";
                    if (db.CreateUser(p[1], p[2], p[3], fn, ce, ac, sb)) 
                    {
                        Send(stream, "CREATE_USER_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "CREATE_USER_FAIL");
                    break;

                case "UPDATE_USER":
                    string ufn = p.Length > 4 ? p[4] : "";
                    string uce = p.Length > 5 ? p[5] : null;
                    string uac = p.Length > 6 ? p[6] : null;
                    string usb = p.Length > 7 ? p[7] : null;
                    if (db.UpdateUser(p[1], p[2], p[3], ufn, uce, uac, usb)) 
                    {
                        Send(stream, "UPDATE_USER_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "UPDATE_USER_FAIL");
                    break;

                case "DELETE_USER":
                    if (db.DeleteUser(p[1])) 
                    {
                        Send(stream, "DELETE_USER_SUCCESS");
                        Broadcast("REFRESH");
                    }
                    else Send(stream, "DELETE_USER_FAIL");
                    break;

                default:
                    Send(stream, "INVALID_COMMAND");
                    break;
            }
        }

        private static void Broadcast(string msg)
        {
            byte[] data = Encoding.UTF8.GetBytes(msg);
            lock (clients)
            {
                foreach (var c in clients.ToArray())
                {
                    try { c.GetStream().Write(data, 0, data.Length); }
                    catch { /* Connection closed */ }
                }
            }
        }

        private static void Send(NetworkStream s, string msg)
        {
            try {
                byte[] data = Encoding.UTF8.GetBytes(msg);
                s.Write(data, 0, data.Length);
            } catch { }
        }

        private static string Receive(NetworkStream s)
        {
            byte[] b = new byte[8192];
            try
            {
                int n = s.Read(b, 0, b.Length);
                return n > 0 ? Encoding.UTF8.GetString(b, 0, n) : null;
            }
            catch { return null; }
        }

        public static void Log(string msg, LogType type = LogType.INFO)
        {
            string ts = DateTime.Now.ToString("HH:mm:ss");
            Console.Write($"[{ts}] ");
            switch (type)
            {
                case LogType.INFO: Console.ForegroundColor = ConsoleColor.Gray; break;
                case LogType.ERROR: Console.ForegroundColor = ConsoleColor.Red; break;
                case LogType.SUCCESS: Console.ForegroundColor = ConsoleColor.Green; break;
                case LogType.CMD: Console.ForegroundColor = ConsoleColor.Cyan; break;
                case LogType.DATA: Console.ForegroundColor = ConsoleColor.Yellow; break;
            }
            Console.Write($"[{type}] ");
            Console.ResetColor();
            Console.WriteLine(msg);
        }
    }
}
