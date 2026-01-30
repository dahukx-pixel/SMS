using Cafe.Domain.Models;
using CafeClient.Mappers;
using CafeClient.Mappers.Requests;
using CafeClient.Mappers.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace CafeClient
{
    public class ClientCafe : ClientBase
    {
        public ClientCafe(string endpointUrl,
                          string username,
                          string password,
                          HttpClient? httpClient = null) : base(endpointUrl, username, password, httpClient)
        {
        }

        public async Task<List<MenuItem>> GetMenuAsync(bool withPrice = true, CancellationToken cancellationToken = default)
        {
            var requestParams = new GetMenuRequest { WithPrice = withPrice };

            var response = await SendRequestAsync<GetMenuRequest, GetMenuResponse>("GetMenu", requestParams, cancellationToken);

            return response.Data?.MenuItems ?? new List<MenuItem>();
        }


        public async Task SendOrderAsync(string orderId, List<OrderItem> items, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order ID cannot be null or empty", nameof(orderId));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new ArgumentException("Order must contain at least one item", nameof(items));

            var requestParams = new SendOrderRequest
            {
                OrderId = orderId,
                MenuItems = items
            };

            await SendRequestAsync<SendOrderRequest, BaseResponse>("SendOrder", requestParams, cancellationToken);
        }
    }
}
