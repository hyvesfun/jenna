using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class MediaToken
    {
        public string token { get; set; }
        public string ip { get; set; }
        public InfoResponse info { get; set; }

        // non hyves response properties
        public MediaTokenStatus MediaTokenStatus { get; set; }
        public HyvesUploadRequest HyvesUploadRequest { get; set; }
    }
}
