using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class ServiceResult<T>
    {
        public T Result { get; set; }
        public bool IsError { get; set; }
        public string Message { get; set; }
        public Exception Execption { get; set; }
        
    }
}
