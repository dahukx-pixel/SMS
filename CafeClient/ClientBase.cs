using CafeClient.Mappers.Responses;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CafeClient
{
    public abstract class ClientBase
    {
        protected readonly HttpClient HttpClient;
        protected readonly string EndpointUrl;
        protected readonly string AuthHeader;

        private readonly JsonSerializerOptions _jsonOptions;

        protected ClientBase(string endpointUrl, string username, string password, HttpClient? httpClient = null)
        {
            EndpointUrl = endpointUrl;
            
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty", nameof(username));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            HttpClient = httpClient ?? new HttpClient();
            AuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        protected async Task<TResponse> SendRequestAsync<TRequest, TResponse>(string command, 
                                                                              TRequest commandParameters,
                                                                              CancellationToken cancellationToken = default) where TResponse : BaseResponse
        {
            var requestPayload = new
            {
                Command = command,
                CommandParameters = commandParameters
            };

            var request = CreateRequestMessage(requestPayload);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            var result = JsonSerializer.Deserialize<TResponse>(responseBody, _jsonOptions);

            if (result == null)
                throw new InvalidOperationException("Failed to deserialize server response.");

            if (!result.Success)
                throw new InvalidOperationException(
                    !string.IsNullOrWhiteSpace(result.ErrorMessage)
                        ? result.ErrorMessage
                        : $"Request '{command}' failed with unknown error.");

            return result;
        }

        private HttpRequestMessage CreateRequestMessage(object requestPayload)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, EndpointUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", AuthHeader);

            var json = JsonSerializer.Serialize(requestPayload, _jsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return request;
        }
    }
}
