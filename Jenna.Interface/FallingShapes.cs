/////////////////////////////////////////////////////////////////////////
//
// This module contains code to do display falling shapes, and do
// hit testing against a set of segments provided by the Kinect NUI, and
// have shapes react accordingly.
//
// Copyright © Microsoft Corporation.  All rights reserved.  
// This code is licensed under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Research.Kinect.Nui;
using System.Windows.Media.Imaging;
using Jenna.Interface;
using Hyves.Api.Model;

/// <summary>
/// Falling shapes, and intersection hit testing with body segments
/// </summary>
/// 
namespace Jenna.Interface
{
    // For hit testing, a dictionary of BoneData items, keyed off the endpoints
    // of a segment (Bone) is used.  The velocity of these endpoints is estimated
    // and used during hit testing and updating velocity vectors after a hit.

    public struct Bone
    {
        public JointID joint1;
        public JointID joint2;
        public Bone(JointID j1, JointID j2)
        {
            joint1 = j1;
            joint2 = j2;
        }
    }

    public struct Segment
    {
        public double x1;
        public double y1;
        public double x2;
        public double y2;
        public double radius;

        public Segment(double x, double y)
        {
            radius = 1;
            x1 = x2 = x;
            y1 = y2 = y;
        }

        public Segment(double x_1, double y_1, double x_2, double y_2)
        {
            radius = 1;
            x1 = x_1;
            y1 = y_1;
            x2 = x_2;
            y2 = y_2;
        }

        public bool IsCircle()
        {
            return ((x1 == x2) && (y1 == y2));
        }
    }

    public struct BoneData
    {
        public Segment seg;
        public Segment segLast;
        public double xVel;
        public double yVel;
        public double xVel2;
        public double yVel2;
        private const double smoothing = 0.8;
        public DateTime timeLastUpdated;

        public BoneData(Segment s)
        {
            seg = segLast = s;
            xVel = yVel = 0;
            xVel2 = yVel2 = 0;
            timeLastUpdated = DateTime.Now;
        }

        // Update the segment's position and compute a smoothed velocity for the circle or the
        // endpoints of the segment based on  the time it took it to move from the last position
        // to the current one.  The velocity is in pixels per second.

        public void UpdateSegment(Segment s)
        {
            segLast = seg;
            seg = s;

            DateTime cur = DateTime.Now;
            double fMs = cur.Subtract(timeLastUpdated).TotalMilliseconds;
            if (fMs < 10.0)
                fMs = 10.0;
            double fFPS = 1000.0 / fMs;
            timeLastUpdated = cur;

            if (seg.IsCircle())
            {
                xVel = xVel * smoothing + (1.0 - smoothing) * (seg.x1 - segLast.x1) * fFPS;
                yVel = yVel * smoothing + (1.0 - smoothing) * (seg.y1 - segLast.y1) * fFPS;
            }
            else
            {
                xVel = xVel * smoothing + (1.0 - smoothing) * (seg.x1 - segLast.x1) * fFPS;
                yVel = yVel * smoothing + (1.0 - smoothing) * (seg.y1 - segLast.y1) * fFPS;
                xVel2 = xVel2 * smoothing + (1.0 - smoothing) * (seg.x2 - segLast.x2) * fFPS;
                yVel2 = yVel2 * smoothing + (1.0 - smoothing) * (seg.y2 - segLast.y2) * fFPS;
            }
        }

        // Using the velocity calculated above, estimate where the segment is right now.

        public Segment GetEstimatedSegment(DateTime cur)
        {
            Segment estimate = seg;
            double fMs = cur.Subtract(timeLastUpdated).TotalMilliseconds;
            estimate.x1 += fMs * xVel / 1000.0;
            estimate.y1 += fMs * yVel / 1000.0;
            if (seg.IsCircle())
            {
                estimate.x2 = estimate.x1;
                estimate.y2 = estimate.y1;
            }
            else
            {
                estimate.x2 += fMs * xVel2 / 1000.0;
                estimate.y2 += fMs * yVel2 / 1000.0;
            }
            return estimate;
        }
    }

    public enum PolyType
    {
        None = 0x00,
        Triangle = 0x01,
        Square = 0x02,
        Star = 0x04,
        Pentagon = 0x08,
        Hex = 0x10,
        Star7 = 0x20,
        Circle = 0x40,
        Bubble = 0x80,
        All = 0x7f
    }

    public enum HitType
    {
        None = 0x00,
        Hand = 0x01,
        Arm = 0x02,
        Squeezed = 0x04,
        Popped = 0x08
    }

    // BannerText generates a scrolling or still banner of text (along the bottom of the screen).
    // Only one banner exists at a time.  Calling NewBanner() will erase the old one and start the new one.

    public class BannerText
    {
        private static BannerText bannerText = null;
        private Brush brush;
        private Color color;
        private Label label;
        private Rect boundsRect;
        private Rect renderedRect;
        private bool doScroll;
        private double offset = 0;
        private string text;

        public BannerText(string s, Rect rect, bool scroll, Color col)
        {
            text = s;
            boundsRect = rect;
            doScroll = scroll;
            brush = null;
            label = null;
            color = col;
            offset = (doScroll) ? 1.0 : 0.0;
        }

        public static void NewBanner(string s, Rect rect, bool scroll, Color col)
        {
            bannerText = new BannerText(s, rect, scroll, col);
        }

        private Label GetLabel()
        {
            if (brush == null)
                brush = new SolidColorBrush(color);

            if (label == null)
            {
                label = FallingThings.MakeSimpleLabel(text, boundsRect, brush);
                if (doScroll)
                {
                    label.FontSize = Math.Max(20, boundsRect.Height / 30);
                    label.Width = 10000;
                }
                else
                    label.FontSize = Math.Min(Math.Max(10, boundsRect.Width * 2 / text.Length),
                                              Math.Max(10, boundsRect.Height / 20));
                label.VerticalContentAlignment = VerticalAlignment.Bottom;
                label.HorizontalContentAlignment = (doScroll) ? HorizontalAlignment.Left : HorizontalAlignment.Center;
                label.SetValue(Canvas.LeftProperty, offset * boundsRect.Width);
            }

            renderedRect = new Rect(label.RenderSize);

            if (doScroll)
            {
                offset -= 0.0015;
                if (offset * boundsRect.Width < boundsRect.Left - 10000)
                    return null;
                label.SetValue(Canvas.LeftProperty, offset * boundsRect.Width + boundsRect.Left);
            }
            return label;
        }

        public static void UpdateBounds(Rect rect)
        {
            if (bannerText == null)
                return;
            bannerText.boundsRect = rect;
            bannerText.label = null;
        }

        public static void Draw(UIElementCollection children)
        {
            if (bannerText == null)
                return;

            Label text = bannerText.GetLabel();
            if (text == null)
            {
                bannerText = null;
                return;
            }
            children.Add(text);
        }
    }

    // FlyingText creates text that flys out from a given point, and fades as it gets bigger.
    // NewFlyingText() can be called as often as necessary, and there can be many texts flying out at once.

    public class FlyingText
    {
        Point center;
        string text;
        Brush brush;
        double fontSize;
        double fontGrow;
        double alpha;
        Label label;

        public FlyingText(string s, double size, Point ptCenter)
        {
            text = s;
            fontSize = size;
            fontGrow = Math.Sqrt(size) * 0.4;
            center = ptCenter;
            alpha = 1.0;
            label = null;
            brush = null;
        }

        public static void NewFlyingText(double size, Point center, string s)
        {
            flyingTexts.Add(new FlyingText(s, size, center));
        }

        void Advance()
        {
            alpha -= 0.01;
            if (alpha < 0)
                alpha = 0;

            if (brush == null)
                brush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));

            if (label == null)
                label = FallingThings.MakeSimpleLabel(text, new Rect(0, 0, 0, 0), brush);

            brush.Opacity = Math.Pow(alpha, 1.5);
            label.Foreground = brush;
            fontSize += fontGrow;
            label.FontSize = fontSize;
            Rect rRendered = new Rect(label.RenderSize);
            label.SetValue(Canvas.LeftProperty, center.X - rRendered.Width / 2);
            label.SetValue(Canvas.TopProperty, center.Y - rRendered.Height / 2);
        }

        public static void Draw(UIElementCollection children)
        {
            for (int i = 0; i < flyingTexts.Count; i++)
            {
                FlyingText flyout = flyingTexts[i];
                if (flyout.alpha <= 0)
                {
                    flyingTexts.Remove(flyout);
                    i--;
                }
            }

            foreach (var flyout in flyingTexts)
            {
                flyout.Advance();
                children.Add(flyout.label);
            }
        }

        static List<FlyingText> flyingTexts = new List<FlyingText>();
    }



    
}
