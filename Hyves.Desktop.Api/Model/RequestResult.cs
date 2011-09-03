using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public delegate void HyvesServicesCallback<T>(ServiceResult<T> servicesResult);

    public class RequestResult<T>
    {
        public HyvesServicesCallback<T> Callback { get; set; }
        public string Response { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
        public Exception Execption { get; set; }
    }
}
