using System;
using System.Collections.Generic;
using System.Text;

namespace Hyves.Api.Model
{
    public class RequestToken
    {
        public string oauth_token { get; set; }
        public string oauth_token_secret { get; set; }
        public string method { get; set; }
        public InfoResponse info { get; set; }
    }
}
