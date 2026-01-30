using System;
using System.Collections.Generic;
using System.Text;

namespace Cafe.Domain.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        public string Article { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        /// <summary>
        /// Является ли товар весовым (требует взвешивания).
        /// </summary>
        public bool IsWeighted { get; set; }

        /// <summary>
        /// Полный путь в меню (категория\подкатегория).
        /// </summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>
        /// Список штрихкодов товара.
        /// </summary>
        public List<string> Barcodes { get; set; } = new();
    }
}
