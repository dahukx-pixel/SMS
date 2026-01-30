using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Sms.Test;
using System;
using System.Collections.Generic;
using System.Text;

namespace SmsTestClient
{
    public class CafeClient : ClientBase
    {
        private readonly SmsTestService.SmsTestServiceClient _grpcClient;

        public CafeClient(string serverAddress, 
                          GrpcChannelOptions? channelOptions = null) : base(serverAddress, channelOptions)
        {
            _grpcClient = new SmsTestService.SmsTestServiceClient(Channel);
        }

        /// <summary>
        /// Asynchronously retrieves the list of available menu items from the remote service.
        /// </summary>
        /// <param name="withPrice">Specifies whether to include price information for each menu item. Set to <see langword="true"/> to include
        /// prices; otherwise, price details will be omitted.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of <see cref="MenuItem"/>
        /// objects representing the available menu items.</returns>
        public async Task<List<MenuItem>> GetMenuAsync(bool withPrice = true,
                                                       CancellationToken cancellationToken = default)
        {
            var request = new BoolValue { Value = withPrice };

            var response = await _grpcClient.GetMenuAsync(request, cancellationToken: cancellationToken);

            await EnsureSuccessAsync(response.Success, response.ErrorMessage);

            return response.MenuItems.ToList();
        }

        /// <summary>
        /// Sends an order asynchronously to the remote service for processing.
        /// </summary>
        /// <param name="orderId">The unique identifier for the order to be sent. Cannot be null, empty, or whitespace.</param>
        /// <param name="items">The list of items included in the order. Must contain at least one item. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the send operation.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="orderId"/> is null, empty, or whitespace, or if <paramref name="items"/> contains
        /// no items.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> is null.</exception>
        public async Task SendOrderAsync(string orderId, 
                                         List<OrderItem> items,
                                         CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("Order ID cannot be null or empty", nameof(orderId));

            if (items == null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
                throw new ArgumentException("Order must contain at least one item", nameof(items));

            var order = new Order
            {
                Id = orderId
            };

            foreach (var item in items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    Id = item.Id,
                    Quantity = item.Quantity
                });
            }

            var response = await _grpcClient.SendOrderAsync(order, cancellationToken: cancellationToken);

            await EnsureSuccessAsync(response.Success, response.ErrorMessage);
        }
    }
}
