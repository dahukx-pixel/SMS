using Cafe.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cafe.Infrastructure.Repositories.Interfaces
{
    public interface IMenuItemRepository
    {
        Task AddRangeAsync(IEnumerable<MenuItem> menuItems);
    }
}
