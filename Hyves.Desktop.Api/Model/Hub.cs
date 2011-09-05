using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class Hub
    {
        public string  hubid  { get; set; }
        public string  hubvisible  { get; set; }
        public string  hubtype  { get; set; }
        public string  title  { get; set; }
        public string  description  { get; set; }
        public string  mediaid  { get; set; }
        public string  hubcategoryid  { get; set; }
        public string  userscount  { get; set; }
        public string  url  { get; set; }
        public Media  profilepicture  { get; set; }
        public string created { get; set; }
    }
}
