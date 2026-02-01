using Cafe.Domain.Models;
using Cafe.Infrastructure.Db;
using Cafe.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cafe.Infrastructure.Repositories
{
    public class MenuItemRepository : IMenuItemRepository
    {
        private readonly CafeDataBase _cafeDataBase;

        public MenuItemRepository()
        {
            _cafeDataBase = new CafeDataBase();
        }

        public async Task AddRangeAsync(IEnumerable<MenuItem> menuItems)
        {
            try
            {
                _cafeDataBase.MenuItems.AddRange(menuItems);
                await _cafeDataBase.SaveChangesAsync();
            }
            catch
            {
                //log
            }
        }
    }
}
