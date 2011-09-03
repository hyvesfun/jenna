using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class Album
    {
        public string albumid { get; set; }
        public string title { get; set; }
        public int mediacount { get; set; }
        public string userid { get; set; }
        public string url { get; set; }
        public string visibility { get; set; }
        public string printability { get; set; }
        public string lastupdate { get; set; }
    }
}
