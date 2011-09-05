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


namespace Jenna.Interface
{
    // FallingThings is the main class to draw and maintain positions of falling shapes.  It also does hit testing
    // and appropriate bouncing.
    public class FallingThings
    {

        struct PolyDef
        {
            public int numSides;
            public int skip;
        }

        Dictionary<PolyType, PolyDef> PolyDefs = new Dictionary<PolyType, PolyDef>()
        {
            {PolyType.Triangle, new PolyDef()   {numSides = 3, skip = 1}},
            {PolyType.Star, new PolyDef()       {numSides = 5, skip = 2}},
            {PolyType.Pentagon, new PolyDef()   {numSides = 5, skip = 1}},
            {PolyType.Square, new PolyDef()     {numSides = 4, skip = 1}},
            {PolyType.Hex, new PolyDef()        {numSides = 6, skip = 1}},
            {PolyType.Star7, new PolyDef()      {numSides = 7, skip = 3}},
            {PolyType.Circle, new PolyDef()     {numSides = 1, skip = 1}},
            {PolyType.Bubble, new PolyDef()     {numSides = 0, skip = 1}}
        };

        public enum ThingState
        {
            Falling = 0,
            Bouncing = 1,
            Dissolving = 2,
            Remove = 3
        }

        public enum GameMode
        {
            Off = 0,
            Solo = 1,
            TwoPlayer = 2
        }

        // The Thing struct represents a single object that is flying through the air, and
        // all of its properties.

        private class Thing
        {
            public Point center;
            public double size;
            public double theta;
            public double spinRate;
            public double yVelocity;
            public double xVelocity;
            public PolyType shape;
            public Color color;
            public Brush brush;
            public Brush brush2;
            public Brush brushPulse;
            public double dissolve;
            public ThingState state;
            public DateTime timeLastHit;
            public double avgTimeBetweenHits;
            public int touchedBy;               // Last player to touch this thing
            public int hotness;                 // Score level
            public int flashCount;
            public User friend;

            // Hit testing between this thing and a single segment.  If hit, the center point on
            // the segment being hit is returned, along with the spot on the line from 0 to 1 if
            // a line segment was hit.

            public Thing()
            {
                if (MainWindow.friendList.Count > 0)
                {
                    Random r = new Random();
                    int random = r.Next(0, MainWindow.friendList.Count - 1);
                    this.friend = MainWindow.friendList[random];
                }
            }

            public bool Hit(Segment seg, ref Point ptHitCenter, ref double lineHitLocation)
            {
                double minDxSquared = size + seg.radius;
                minDxSquared *= minDxSquared;

                // See if falling thing hit this body segment
                if (seg.IsCircle())
                {
                    if (SquaredDistance(center.X, center.Y, seg.x1, seg.y1) <= minDxSquared)
                    {
                        ptHitCenter.X = seg.x1;
                        ptHitCenter.Y = seg.y1;
                        lineHitLocation = 0;
                        return true;
                    }
                }
                else
                {
                    double sqrLineSize = SquaredDistance(seg.x1, seg.y1, seg.x2, seg.y2);
                    if (sqrLineSize < 0.5)  // if less than 1/2 pixel apart, just check dx to an endpoint
                    {
                        return (SquaredDistance(center.X, center.Y, seg.x1, seg.y1) < minDxSquared) ? true : false;
                    }
                    else
                    {   // Find dx from center to line
                        double u = ((center.X - seg.x1) * (seg.x2 - seg.x1) + (center.Y - seg.y1) * (seg.y2 - seg.y1)) / sqrLineSize;
                        if ((u >= 0) && (u <= 1.0))
                        {   // Tangent within line endpoints, see if we're close enough
                            double xIntersect = seg.x1 + (seg.x2 - seg.x1) * u;
                            double yIntersect = seg.y1 + (seg.y2 - seg.y1) * u;

                            if (SquaredDistance(center.X, center.Y,
                                xIntersect, yIntersect) < minDxSquared)
                            {
                                lineHitLocation = u;
                                ptHitCenter.X = xIntersect;
                                ptHitCenter.Y = yIntersect; ;
                                return true;
                            }
                        }
                        else
                        {
                            // See how close we are to an endpoint
                            if (u < 0)
                            {
                                if (SquaredDistance(center.X, center.Y, seg.x1, seg.y1) < minDxSquared)
                                {
                                    lineHitLocation = 0;
                                    ptHitCenter.X = seg.x1;
                                    ptHitCenter.Y = seg.y1;
                                    return true;
                                }
                            }
                            else
                            {
                                if (SquaredDistance(center.X, center.Y, seg.x2, seg.y2) < minDxSquared)
                                {
                                    lineHitLocation = 1;
                                    ptHitCenter.X = seg.x2;
                                    ptHitCenter.Y = seg.y2;
                                    return true;
                                }
                            }
                        }
                    }
                    return false;
                }
                return false;
            }

            // Change our velocity based on the object's velocity, our velocity, and where we hit.

            public void BounceOff(double x1, double y1, double otherSize, double fXv, double fYv)
            {
                double fX0 = center.X;
                double fY0 = center.Y;
                double fXV0 = xVelocity - fXv;
                double fYV0 = yVelocity - fYv;
                double dist = otherSize + size;
                double fDx = Math.Sqrt((x1 - fX0) * (x1 - fX0) + (y1 - fY0) * (y1 - fY0));
                double A, B, C;
                double xdif = x1 - fX0;
                double ydif = y1 - fY0;
                double newvx1 = 0;
                double newvy1 = 0;

                fX0 = x1 - xdif / fDx * dist;
                fY0 = y1 - ydif / fDx * dist;
                xdif = x1 - fX0;
                ydif = y1 - fY0;

                double Bsq = dist * dist;
                B = dist;
                double Asq = fXV0 * fXV0 + fYV0 * fYV0;
                A = Math.Sqrt(Asq);
                if (A > 0.000001)	// if moving much at all...
                {
                    double cx = fX0 + fXV0;
                    double cy = fY0 + fYV0;
                    double Csq = (x1 - cx) * (x1 - cx) + (y1 - cy) * (y1 - cy);
                    C = Math.Sqrt(Csq);
                    double tt = Asq + Bsq - Csq;
                    double bb = 2 * A * B;
                    double power = A * (tt / bb);
                    newvx1 -= 2 * (xdif / dist * power);
                    newvy1 -= 2 * (ydif / dist * power);
                }

                xVelocity += newvx1;
                yVelocity += newvy1;
                center.X = fX0;
                center.Y = fY0;
            }

            public bool HasFriend { get { return friend != null; } }
        }

        private List<Thing> things = new List<Thing>();
        private const double DissolveTime = 0.4;
        private int maxThings = 0;
        private Rect sceneRect;
        private Random rnd = new Random();
        private double targetFrameRate = 60;
        private double dropRate = 2.0;
        private double shapeSize = 1.0;
        private double baseShapeSize = 20;
        private GameMode gameMode = GameMode.Off;
        private const double BaseGravity = 0.017;
        private double gravity = BaseGravity;
        private double gravityFactor = 1.0;
        private const double baseAirFriction = 0.994;
        private double airFriction = baseAirFriction;
        private int intraFrames = 1;
        private int frameCount = 0;
        private bool doRandomColors = true;
        private double expandingRate = 1.0;
        private Color baseColor = Color.FromRgb(0, 0, 0);
        private PolyType polyTypes = PolyType.All;
        private Dictionary<int, int> scores = new Dictionary<int, int>();
        private DateTime gameStartTime;

        public FallingThings(int maxThings, double framerate, int intraFrames)
        {
            this.maxThings = maxThings;
            this.intraFrames = intraFrames;
            this.targetFrameRate = framerate * intraFrames;
            SetGravity(gravityFactor);
            sceneRect.X = sceneRect.Y = 0;
            sceneRect.Width = sceneRect.Height = 100;
            shapeSize = sceneRect.Height * baseShapeSize / 1000.0;
            expandingRate = Math.Exp(Math.Log(6.0) / (targetFrameRate * DissolveTime));
        }

        public void SetFramerate(double actualFramerate)
        {
            targetFrameRate = actualFramerate * intraFrames;
            expandingRate = Math.Exp(Math.Log(6.0) / (targetFrameRate * DissolveTime));
            if (gravityFactor != 0)
                SetGravity(gravityFactor);
        }

        public void SetBoundaries(Rect r)
        {
            sceneRect = r;
            shapeSize = r.Height * baseShapeSize / 1000.0;
        }

        public void SetDropRate(double f)
        {
            dropRate = f;
        }

        public void SetSize(double f)
        {
            baseShapeSize = f;
            shapeSize = sceneRect.Height * baseShapeSize / 1000.0;
        }

        public void SetShapesColor(Color color, bool doRandom)
        {
            doRandomColors = doRandom;
            baseColor = color;
        }

        public void Reset()
        {
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if ((thing.state == ThingState.Bouncing) || (thing.state == ThingState.Falling))
                {
                    thing.state = ThingState.Dissolving;
                    thing.dissolve = 0;
                    things[i] = thing;
                }
            }
            gameStartTime = DateTime.Now;
            scores.Clear();
        }

        public void SetGameMode(GameMode mode)
        {
            gameMode = mode;
            gameStartTime = DateTime.Now;
            scores.Clear();
        }

        public void SetGravity(double f)
        {
            gravityFactor = f;
            gravity = f * BaseGravity / targetFrameRate / Math.Sqrt(targetFrameRate) / Math.Sqrt((double)intraFrames);
            airFriction = (f == 0) ? 0.997 : Math.Exp(Math.Log(1.0 - (1.0 - baseAirFriction) / f) / intraFrames);

            if (f == 0)  // Stop all movement as well!
            {
                for (int i = 0; i < things.Count; i++)
                {
                    Thing thing = things[i];
                    thing.xVelocity = thing.yVelocity = 0;
                    things[i] = thing;
                }
            }
        }

        public void SetPolies(PolyType polies)
        {
            polyTypes = polies;
        }

        private void AddToScore(int player, int points, Point center)
        {
            if (scores.ContainsKey(player))
                scores[player] = scores[player] + points;
            else
                scores.Add(player, points);
            FlyingText.NewFlyingText(sceneRect.Width / 300, center, "+" + points);
        }

        private static double SquaredDistance(double x1, double y1, double x2, double y2)
        {
            return ((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
        }

        public HitType LookForHits(Dictionary<Bone, BoneData> segments, int playerId)
        {
            DateTime cur = DateTime.Now;
            HitType allHits = HitType.None;

            // Zero out score if necessary
            if (!scores.ContainsKey(playerId))
                scores.Add(playerId, 0);

            foreach (var pair in segments)
            {
                for (int i = 0; i < things.Count; i++)
                {
                    HitType hit = HitType.None;
                    Thing thing = things[i];
                    switch (thing.state)
                    {
                        case ThingState.Bouncing:
                        case ThingState.Falling:
                            {
                                var ptHitCenter = new Point(0, 0);
                                double lineHitLocation = 0;
                                Segment seg = pair.Value.GetEstimatedSegment(cur);
                                if (thing.Hit(seg, ref ptHitCenter, ref lineHitLocation))
                                {
                                    double fMs = 1000;
                                    if (thing.timeLastHit != DateTime.MinValue)
                                    {
                                        fMs = cur.Subtract(thing.timeLastHit).TotalMilliseconds;
                                        thing.avgTimeBetweenHits = thing.avgTimeBetweenHits * 0.8 + 0.2 * fMs;
                                    }
                                    thing.timeLastHit = cur;

                                    // Bounce off head and hands
                                    if (seg.IsCircle())
                                    {
                                        // Bounce off of hand/head/foot
                                        thing.BounceOff(ptHitCenter.X, ptHitCenter.Y, seg.radius,
                                            pair.Value.xVel / targetFrameRate, pair.Value.yVel / targetFrameRate);

                                        if (fMs > 100.0)
                                            hit |= HitType.Hand;
                                    }
                                    else  // Bonce off line segment
                                    {
                                        double xVel = pair.Value.xVel * (1.0 - lineHitLocation) + pair.Value.xVel2 * lineHitLocation;
                                        double yVel = pair.Value.yVel * (1.0 - lineHitLocation) + pair.Value.yVel2 * lineHitLocation;

                                        thing.BounceOff(ptHitCenter.X, ptHitCenter.Y, seg.radius,
                                            xVel / targetFrameRate, yVel / targetFrameRate);

                                        if (fMs > 100.0)
                                            hit |= HitType.Arm;
                                    }

                                    if (gameMode == GameMode.TwoPlayer)
                                    {
                                        if (thing.state == ThingState.Falling)
                                        {
                                            thing.state = ThingState.Bouncing;
                                            thing.touchedBy = playerId;
                                            thing.hotness = 1;
                                            thing.flashCount = 0;
                                        }
                                        else if (thing.state == ThingState.Bouncing)
                                        {
                                            if (thing.touchedBy != playerId)
                                            {
                                                if (seg.IsCircle())
                                                {
                                                    thing.touchedBy = playerId;
                                                    thing.hotness = Math.Min(thing.hotness + 1, 4);
                                                }
                                                else
                                                {
                                                    hit |= HitType.Popped;
                                                    AddToScore(thing.touchedBy, 5 << (thing.hotness - 1), thing.center);
                                                }
                                            }
                                        }
                                    }
                                    else if (gameMode == GameMode.Solo)
                                    {
                                        if (seg.IsCircle())
                                        {
                                            if (thing.state == ThingState.Falling)
                                            {
                                                thing.state = ThingState.Bouncing;
                                                thing.touchedBy = playerId;
                                                thing.hotness = 1;
                                                thing.flashCount = 0;
                                            }
                                            else if ((thing.state == ThingState.Bouncing) && (fMs > 100.0))
                                            {
                                                hit |= HitType.Popped;
                                                AddToScore(thing.touchedBy,
                                                            (pair.Key.joint1 == JointID.FootLeft || pair.Key.joint1 == JointID.FootRight) ? 10 : 5,
                                                            thing.center);
                                                thing.touchedBy = playerId;
                                            }
                                        }
                                    }

                                    things[i] = thing;

                                    if (thing.avgTimeBetweenHits < 8)
                                    {
                                        hit |= HitType.Popped | HitType.Squeezed;
                                        if (gameMode != GameMode.Off)
                                            AddToScore(playerId, 1, thing.center);
                                    }
                                }
                            }
                            break;
                    }

                    if ((hit & HitType.Popped) != 0)
                    {
                        thing.state = ThingState.Dissolving;
                        thing.dissolve = 0;
                        thing.xVelocity = thing.yVelocity = 0;
                        thing.spinRate = thing.spinRate * 6 + 0.2;
                        things[i] = thing;
                    }
                    allHits |= hit;
                }
            }
            return allHits;
        }

        private void DropNewThing(PolyType newShape, double newSize, Color newColor)
        {
            // Only drop within the center "square" area 
            double fDropWidth = (sceneRect.Bottom - sceneRect.Top);
            if (fDropWidth > sceneRect.Right - sceneRect.Left)
                fDropWidth = sceneRect.Right - sceneRect.Left;

            var newThing = new Thing()
            {
                size = newSize,
                yVelocity = (0.5 * rnd.NextDouble() - 0.25) / targetFrameRate,
                xVelocity = 0,
                shape = newShape,
                center = new Point(rnd.NextDouble() * fDropWidth + (sceneRect.Left + sceneRect.Right - fDropWidth) / 2, sceneRect.Top - newSize),
                spinRate = (rnd.NextDouble() * 12.0 - 6.0) * 2.0 * Math.PI / targetFrameRate / 4.0,
                theta = 0,
                timeLastHit = DateTime.MinValue,
                avgTimeBetweenHits = 100,
                color = newColor,
                brush = null,
                brush2 = null,
                brushPulse = null,
                dissolve = 0,
                state = ThingState.Falling,
                touchedBy = 0,
                hotness = 0,
                flashCount = 0
            };

            things.Add(newThing);
        }

        private Shape makeSimpleShape(int numSides, int skip, double size, double spin, Point center, Brush brush,
            Brush brushStroke, double strokeThickness, double opacity)
        {
            if (numSides <= 1)
            {
                var circle = new Ellipse();
                circle.Width = size * 2;
                circle.Height = size * 2;
                circle.Stroke = brushStroke;
                if (circle.Stroke != null)
                    circle.Stroke.Opacity = opacity;
                circle.StrokeThickness = strokeThickness * ((numSides == 1) ? 1 : 2);
                circle.Fill = (numSides == 1) ? brush : null;
                circle.SetValue(Canvas.LeftProperty, center.X - size);
                circle.SetValue(Canvas.TopProperty, center.Y - size);
                return circle;
            }
            else
            {
                var points = new PointCollection(numSides + 2);
                double theta = spin;
                for (int i = 0; i <= numSides + 1; ++i)
                {
                    points.Add(new Point(Math.Cos(theta) * size + center.X, Math.Sin(theta) * size + center.Y));
                    theta = theta + 2.0 * Math.PI * skip / numSides;
                }

                var polyline = new Polyline();
                polyline.Points = points;
                polyline.Stroke = brushStroke;
                if (polyline.Stroke != null)
                    polyline.Stroke.Opacity = opacity;
                polyline.Fill = brush;
                polyline.FillRule = FillRule.Nonzero;
                polyline.StrokeThickness = strokeThickness;
                return polyline;
            }
        }

        public static Label MakeSimpleLabel(string text, Rect bounds, Brush brush)
        {
            Label label = new Label();
            label.Content = text;
            if (bounds.Width != 0)
            {
                label.SetValue(Canvas.LeftProperty, bounds.Left);
                label.SetValue(Canvas.TopProperty, bounds.Top);
                label.Width = bounds.Width;
                label.Height = bounds.Height;
            }
            label.Foreground = brush;
            label.FontFamily = new FontFamily("Arial");
            label.FontWeight = FontWeight.FromOpenTypeWeight(600);
            label.FontStyle = FontStyles.Normal;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.VerticalAlignment = VerticalAlignment.Center;
            return label;
        }

        public void AdvanceFrame()
        {
            // Move all things by one step, accounting for gravity
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                thing.center.Offset(thing.xVelocity, thing.yVelocity);
                thing.yVelocity += gravity * sceneRect.Height;
                thing.yVelocity *= airFriction;
                thing.xVelocity *= airFriction;
                thing.theta += thing.spinRate;

                // bounce off walls
                if ((thing.center.X - thing.size < 0) || (thing.center.X + thing.size > sceneRect.Width))
                {
                    thing.xVelocity = -thing.xVelocity;
                    thing.center.X += thing.xVelocity;
                }

                // Then get rid of one if any that fall off the bottom
                if (thing.center.Y - thing.size > sceneRect.Bottom)
                    thing.state = ThingState.Remove;

                // Get rid of after dissolving.
                if (thing.state == ThingState.Dissolving)
                {
                    thing.dissolve += 1 / (targetFrameRate * DissolveTime);
                    thing.size *= expandingRate;
                    if (thing.dissolve >= 1.0)
                        thing.state = ThingState.Remove;
                }
                things[i] = thing;
            }

            // Then remove any that should go away now
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing.state == ThingState.Remove)
                {
                    things.Remove(thing);
                    i--;
                }
            }

            // Create any new things to drop based on dropRate
            if ((things.Count < maxThings) && (rnd.NextDouble() < dropRate / targetFrameRate) && (polyTypes != PolyType.None))
            {
                PolyType[] alltypes = { PolyType.Triangle, PolyType.Square, PolyType.Star, 
                                        PolyType.Pentagon, PolyType.Hex, PolyType.Star7,
                                        PolyType.Circle, PolyType.Bubble};
                byte r = baseColor.R;
                byte g = baseColor.G;
                byte b = baseColor.B;

                if (doRandomColors)
                {
                    r = (byte)(rnd.Next(215) + 40);
                    g = (byte)(rnd.Next(215) + 40);
                    b = (byte)(rnd.Next(215) + 40);
                }
                else
                {
                    r = (byte)(Math.Min(255.0, (double)baseColor.R * (0.7 + rnd.NextDouble() * 0.7)));
                    g = (byte)(Math.Min(255.0, (double)baseColor.G * (0.7 + rnd.NextDouble() * 0.7)));
                    b = (byte)(Math.Min(255.0, (double)baseColor.B * (0.7 + rnd.NextDouble() * 0.7)));
                }

                PolyType tryType = PolyType.None;
                do
                {
                    tryType = alltypes[rnd.Next(alltypes.Length)];
                } while ((polyTypes & tryType) == 0);

                DropNewThing(PolyType.Circle, shapeSize, Color.FromRgb(r, g, b));
            }
        }

        public void DrawFrame(UIElementCollection children)
        {
            frameCount++;
            Random r = new Random();
            // Draw all shapes in the scene
            for (int i = 0; i < things.Count; i++)
            {
                Thing thing = things[i];
                if (thing.brush == null)
                {
                    thing.brush = new SolidColorBrush(thing.color);
                    double factor = 0.4 + ((double)thing.color.R + thing.color.G + thing.color.B) / 1600;
                    thing.brush2 = new SolidColorBrush(Color.FromRgb((byte)(255 - (255 - thing.color.R) * factor),
                                                                     (byte)(255 - (255 - thing.color.G) * factor),
                                                                     (byte)(255 - (255 - thing.color.B) * factor)));
                    thing.brushPulse = new SolidColorBrush(Color.FromRgb(255, 255, 255));


                    if (thing.HasFriend)
                    {
                        ImageBrush berriesBrush = new ImageBrush();
                        berriesBrush.ImageSource = new BitmapImage(new Uri(thing.friend.profilepicture.icon_medium.src, UriKind.Absolute));
                        thing.brush = berriesBrush;
                    }
                }

                if (thing.state == ThingState.Bouncing)  // Pulsate edges
                {
                    double alpha = (Math.Cos(0.15 * (thing.flashCount++) * thing.hotness) * 0.5 + 0.5);

                    children.Add(makeSimpleShape(PolyDefs[thing.shape].numSides, PolyDefs[thing.shape].skip,
                        thing.size, thing.theta, thing.center, thing.brush,
                        thing.brushPulse, thing.size * 0.1, alpha));
                    things[i] = thing;
                }
                else
                {
                    if (thing.state == ThingState.Dissolving)
                        thing.brush.Opacity = 1.0 - thing.dissolve * thing.dissolve;

                    children.Add(makeSimpleShape(PolyDefs[thing.shape].numSides, PolyDefs[thing.shape].skip,
                        thing.size, thing.theta, thing.center, thing.brush,
                        (thing.state == ThingState.Dissolving) ? null : thing.brush2, 1, 1));
                }
            }

            // Show scores
            if (scores.Count != 0)
            {
                int i = 0;
                foreach (var score in scores)
                {
                    Label label = MakeSimpleLabel(score.Value.ToString(),
                        new Rect((0.02 + i * 0.6) * sceneRect.Width, 0.01 * sceneRect.Height,
                                 0.4 * sceneRect.Width, 0.3 * sceneRect.Height),
                        new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)));
                    label.FontSize = Math.Min(sceneRect.Width / 12, sceneRect.Height / 12);
                    children.Add(label);
                    i++;
                }
            }

            // Show game timer
            if (gameMode != GameMode.Off)
            {
                TimeSpan span = DateTime.Now.Subtract(gameStartTime);
                string text = span.Minutes.ToString() + ":" + span.Seconds.ToString("00");

                Label timeText = MakeSimpleLabel(text,
                    new Rect(0.1 * sceneRect.Width, 0.25 * sceneRect.Height, 0.89 * sceneRect.Width, 0.72 * sceneRect.Height),
                    new SolidColorBrush(Color.FromArgb(160, 255, 255, 255)));
                timeText.FontSize = sceneRect.Height / 16;
                timeText.HorizontalContentAlignment = HorizontalAlignment.Right;
                timeText.VerticalContentAlignment = VerticalAlignment.Bottom;
                children.Add(timeText);
            }
        }
    }
}
