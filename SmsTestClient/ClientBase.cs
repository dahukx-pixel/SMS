using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace SmsTestClient
{
    public abstract class ClientBase : IDisposable
    {
        private readonly GrpcChannel _channel;
        private bool _disposed;

        protected GrpcChannel Channel => _channel;

        protected ClientBase(string serverAddress, GrpcChannelOptions? channelOptions = null)
        {
            if (string.IsNullOrWhiteSpace(serverAddress))
                throw new ArgumentException("Server address cannot be null or empty", nameof(serverAddress));

            _channel = GrpcChannel.ForAddress(serverAddress, channelOptions ?? new GrpcChannelOptions());
        }

        /// <summary>
        /// Validates the outcome of an asynchronous operation and throws an exception if the operation did not succeed.
        /// </summary>
        /// <param name="success">A value indicating whether the operation was successful. If <see langword="false"/>, an exception is thrown.</param>
        /// <param name="errorMessage">The error message to include in the exception if the operation failed. If null or whitespace, a default
        /// message is used.</param>
        /// <returns>A completed task if the operation was successful.</returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="success"/> is <see langword="false"/>.</exception>
        protected Task EnsureSuccessAsync(bool success, string errorMessage)
        {
            if (!success)
                throw new InvalidOperationException(
                    !string.IsNullOrWhiteSpace(errorMessage)
                        ? errorMessage
                        : "Operation failed with unknown error");

            return Task.CompletedTask;
        }

        #region Disposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _channel.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ClientBase()
        {
            Dispose(false);
        }
        #endregion
    }
}
