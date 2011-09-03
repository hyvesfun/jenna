using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;

namespace Hyves.Api
{
    public class Util
    {
        public static Image GetImageFromUrl(string url)
        {
            HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            Image image = Image.FromStream(httpWebResponse.GetResponseStream());
            httpWebResponse.Close();
            return image;
        }
    }
}
