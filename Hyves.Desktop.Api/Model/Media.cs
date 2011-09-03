using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Emgu.CV.Structure;

namespace Hyves.Api.Model
{

    [Serializable]
    public class MediaIcon
    {
        public int width { get; set; }
        public int height { get; set; }
        public string src { get; set; }
        public Image image { get; set; }
    }
    [Serializable]
    public class Media
    {
        public string mediaid { get; set; }
        public string userid { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string mediatype { get; set; }
        public MediaIcon icon_small { get; set; }
        public MediaIcon icon_medium { get; set; }
        public MediaIcon icon_large { get; set; }
        public MediaIcon icon_extralarge { get; set; }
        public MediaIcon image { get; set; }
        public MediaIcon image_fullscreen { get; set; }
        public MediaIcon square_large { get; set; }
        public MediaIcon square_extralarge { get; set; }
        public string url { get; set; }
        public string created { get; set; }

        // No hyves property

        [NonSerialized]
        private MCvAvgComp[][] faces;
        public MCvAvgComp[][] Faces
        {
            get { return faces; }
            set { faces = value; }
        }
    }
}
