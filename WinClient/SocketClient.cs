using System;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace WinClient
{
    public static class SocketClient
    {
        private static TcpClient client;
        private static NetworkStream stream;

        public static bool Connect()
        {
            try
            {
                if (client != null && client.Connected) return true;
                client = new TcpClient("127.0.0.1", 8888);
                stream = client.GetStream();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối tới Server: " + ex.Message);
                return false;
            }
        }

        public static void Send(string msg)
        {
            try 
            {
                if (client == null || !client.Connected) throw new Exception("Chưa kết nối Server");
                byte[] data = Encoding.UTF8.GetBytes(msg);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi tin: " + ex.Message);
            }
        }

        public static string Receive()
        {
            try
            {
                if (client == null || !client.Connected) return null;
                byte[] buffer = new byte[8192]; 
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                return Encoding.UTF8.GetString(buffer, 0, bytesRead);
            }
            catch
            {
                return null;
            }
        }

        public static void Close()
        {
            stream?.Close();
            client?.Close();
        }
    }
}
