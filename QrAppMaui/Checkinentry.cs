using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QrAppMaui
{
    internal class Checkinentry
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;

        public string Initials =>
            $"{(FirstName.Length > 0 ? FirstName[0] : '?')}{(LastName.Length > 0 ? LastName[0] : '?')}"
                .ToUpperInvariant();

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}
