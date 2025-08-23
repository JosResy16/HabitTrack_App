using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HabitTracker.Application.Common.Interfaces
{
    public interface IUserContextService
    {
        Guid GetCurrentUserId();
    }
}
