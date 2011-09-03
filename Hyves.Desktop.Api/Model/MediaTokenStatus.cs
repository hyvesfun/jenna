using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class MediTokenState
    {
        public string expected_endtime { get; set; }
        public string starttime { get; set; }
        public string endtime { get; set; }
    }

    public class MediaUploaderStatusDone 
    {
        public string mediaid { get; set; }
    }

    public class MediaUploadError
    {
        public string errornumber { get; set; }
        public string errormessage { get; set; }
    }

    public class MediaTokenStatus
    {
        public MediTokenState storing { get; set; }
        public MediTokenState upload { get; set; }
        public MediTokenState rendering { get; set; }
        public MediaUploaderStatusDone done { get; set; }
        public MediTokenState renderqueue { get; set; }

        public string currentstate { get; set; }
        public MediaUploadError error { get; set; }
    }

}
