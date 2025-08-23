using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.DTOs
{
    public class RefreshTokenRequestDTO
    {
        public Guid UserId { get; set; }
        public required string RefreshToken { get; set; }
    }
}
