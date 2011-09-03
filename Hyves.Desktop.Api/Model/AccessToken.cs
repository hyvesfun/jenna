using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class AccessToken
    {
        public string oauth_token { get; set; }
        public string oauth_token_secret { get; set; }
        public string userid { get; set; }
        public string methods { get; set; }
        public string expiredate { get; set; }
        public string method { get; set; }
        public InfoResponse info { get; set; }
    }
}
