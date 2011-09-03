using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class InfoResponse
    {
        public int timestamp_difference { get; set; }
        public bool secure_connection { get; set; }
        public int running_milliseconds { get; set; }
        public string serverinfo { get; set; }
    }
}
