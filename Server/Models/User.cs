namespace Server.Models
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string FullName { get; set; }
        public string ContactEmail { get; set; }   // Mail liên hệ
        public string AssignedClass { get; set; }  // Lớp dạy
        public string Subject { get; set; }        // Môn dạy

        public override string ToString()
        {
            return $"{Username}#{FullName}#{Role}#{ContactEmail}#{AssignedClass}#{Subject}";
        }
    }
}
