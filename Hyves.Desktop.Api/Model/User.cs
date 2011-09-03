using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hyves.Api.Model
{
    public class Birthday 
    {
        public string year { get; set; }
        public string month { get; set; }
        public string day { get; set; }
        public string age { get; set; }
    }
    public class User : IComparable
    {
        public string userid { get; set; }
        public string displayname { get; set; }
        public string nickname { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string gender { get; set; }
        public Birthday birthday { get; set; }
        public string friendscount { get; set; }
        public string url { get; set; }
        public string mediaid { get; set; }
        public string countryid { get; set; }
        public string cityid { get; set; }
        public string created { get; set; }
        public string languagelocale { get; set; }
        public Media profilepicture { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1}", firstname, lastname);
        }

        public int CompareTo(object obj)
        {
            return string.Compare(this.ToString(), obj.ToString());
        }
    }
}
