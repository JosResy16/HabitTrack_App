using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Configuration
{
    public class AppSettings
    {
        public string? Token { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
    }
}
