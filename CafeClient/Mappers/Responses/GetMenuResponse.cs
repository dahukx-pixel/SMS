using CafeClient.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CafeClient.Mappers.Responses
{
    public class GetMenuResponse : BaseResponse
    {
        public MenuData? Data { get; set; }
    }
}
