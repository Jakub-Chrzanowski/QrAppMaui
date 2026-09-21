using SQLite;

namespace QrAppMaui
{
    public class Checkinentry
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string SerialNumber { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Ignore]
        public string Initials =>
            $"{(FirstName.Length > 0 ? FirstName[0] : '?')}{(LastName.Length > 0 ? LastName[0] : '?')}"
                .ToUpperInvariant();

        [Ignore]
        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}