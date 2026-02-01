using Cafe.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CafeClient.Mappers.Requests
{
    public class SendOrderRequest
    {
        public string OrderId { get; set; } = string.Empty;
        public List<OrderItem> MenuItems { get; set; } = new();
    }
}
