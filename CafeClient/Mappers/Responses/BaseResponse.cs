using System;
using System.Collections.Generic;
using System.Text;

namespace CafeClient.Mappers.Responses
{
    public class BaseResponse
    {
        public string Command { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
