/////////////////////////////////////////////////////////////////////////
//
// This module provides sample code used to demonstrate the use
// of the KinectAudioSource for speech recognition in a game setting.
//
// Copyright © Microsoft Corporation.  All rights reserved.  
// This code is licensed under the terms of the 
// Microsoft Kinect for Windows SDK (Beta) from Microsoft Research 
// License Agreement: http://research.microsoft.com/KinectSDK-ToU
//
/////////////////////////////////////////////////////////////////////////
/*
 * IMPORTANT: This sample requires the following components to be installed:
 * 
 * Speech Platform Runtime (v10.2) x86. Even on x64 platforms the x86 needs to be used because the MSR Kinect SDK runtime is x86
 * http://www.microsoft.com/downloads/en/details.aspx?FamilyID=bb0f72cb-b86b-46d1-bf06-665895a313c7
 *
 * Kinect English Language Pack: MSKinectLangPack_enUS.msi 
 * http://go.microsoft.com/fwlink/?LinkId=220942
 * 
 * Speech Platform SDK (v10.2) 
 * http://www.microsoft.com/downloads/en/details.aspx?FamilyID=1b1604d3-4f66-4241-9a21-90a294a5c9a4&displaylang=en
 * */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Research.Kinect.Audio;
using Microsoft.Speech.AudioFormat;
using Microsoft.Speech.Recognition;

namespace Jenna.Interface
{
    public class Recognizer
    {
        public enum Verbs
        {
            None = 0,
            Colorize,
            DoShapes,
            ShapesAndColors,
            Reset,
            Pause,
            Resume,
            Up,
            Down,
            Picture
        };

        struct WhatSaid
        {
            public Verbs verb;
            public PolyType shape;
            public Color color;
        }

        Dictionary<string, WhatSaid> GameplayPhrases = new Dictionary<string, WhatSaid>()
        {
            {"Move Up", new WhatSaid()        {verb=Verbs.Up}},
            {"Move Down", new WhatSaid()        {verb=Verbs.Down}},
            {"Picture", new WhatSaid()        {verb=Verbs.Picture}},
            {"Take Photo", new WhatSaid()        {verb=Verbs.Picture}},
            {"Photo", new WhatSaid()        {verb=Verbs.Picture}},
            {"Take Picture", new WhatSaid()        {verb=Verbs.Picture}},

        };

        Dictionary<string, WhatSaid> SinglePhrases = new Dictionary<string, WhatSaid>()
        {
            {"Reset", new WhatSaid()            {verb=Verbs.Reset}},
            {"Clear", new WhatSaid()            {verb=Verbs.Reset}},
            {"Fuck", new WhatSaid()             {verb=Verbs.Reset}},
            {"Stop", new WhatSaid()             {verb=Verbs.Pause}},
            {"Pause Game", new WhatSaid()       {verb=Verbs.Pause}},
            {"Freeze", new WhatSaid()           {verb=Verbs.Pause}},
            {"Unfreeze", new WhatSaid()         {verb=Verbs.Resume}},
            {"Resume", new WhatSaid()           {verb=Verbs.Resume}},
            {"Continue", new WhatSaid()         {verb=Verbs.Resume}},
            {"Play", new WhatSaid()             {verb=Verbs.Resume}},
            {"Start", new WhatSaid()            {verb=Verbs.Resume}},
            {"Go", new WhatSaid()               {verb=Verbs.Resume}},
        };

        public class SaidSomethingArgs : EventArgs
        {
            public Verbs Verb { get; set; }
            public PolyType Shape { get; set; }
            public Color RGBColor { get; set; }
            public string Phrase { get; set; }
            public string Matched {get; set; }
        }
        
        public event EventHandler<SaidSomethingArgs> SaidSomething;

        private KinectAudioSource kinectSource;
        private SpeechRecognitionEngine sre;
        private const string RecognizerId = "SR_MS_en-US_Kinect_10.0";
        private bool paused = false;
        private bool valid = false;

        public Recognizer()
        {
            RecognizerInfo ri = SpeechRecognitionEngine.InstalledRecognizers().Where(r => r.Id == RecognizerId).FirstOrDefault();
            if (ri == null)
                return;
                
            sre = new SpeechRecognitionEngine(ri.Id);

            // Build a simple grammar of shapes, colors, and some simple program control
            var single = new Choices();
            foreach (var phrase in SinglePhrases)
                single.Add(phrase.Key);
            
            var gameplay = new Choices();
            foreach (var phrase in GameplayPhrases)
                gameplay.Add(phrase.Key);

            var objectChoices = new Choices();
            objectChoices.Add(gameplay);

            var actionGrammar = new GrammarBuilder();
            actionGrammar.AppendWildcard();
            actionGrammar.Append(objectChoices);

            var allChoices = new Choices();
            allChoices.Add(actionGrammar);
            allChoices.Add(single);
            
            var gb = new GrammarBuilder();
            gb.Append(allChoices);

            var g = new Grammar(gb);
            sre.LoadGrammar(g);
            sre.SpeechRecognized += sre_SpeechRecognized;
            sre.SpeechHypothesized += sre_SpeechHypothesized;
            sre.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(sre_SpeechRecognitionRejected);

            var t = new Thread(StartDMO);
            t.Start();

            valid = true;
	    }

        public bool IsValid()
        {
            return valid;
        }

        private void StartDMO()
        {
            kinectSource = new KinectAudioSource();
            kinectSource.SystemMode = SystemMode.OptibeamArrayOnly;
            kinectSource.FeatureMode = true;
            kinectSource.AutomaticGainControl = false;
            kinectSource.MicArrayMode = MicArrayMode.MicArrayAdaptiveBeam;
            var kinectStream = kinectSource.Start();
            sre.SetInputToAudioStream(kinectStream, new SpeechAudioFormatInfo(
                                                  EncodingFormat.Pcm, 16000, 16, 1,
                                                  32000, 2, null));
            sre.RecognizeAsync(RecognizeMode.Multiple);
        }

        public void Stop()
        {
            if (sre != null)
            {
                sre.RecognizeAsyncCancel();
                sre.RecognizeAsyncStop();
                kinectSource.Dispose();
            }
        }

        void sre_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            var said = new SaidSomethingArgs();
            said.Verb = Verbs.None;
            said.Matched = "?";
            SaidSomething(new object(), said);
            Console.WriteLine("\nSpeech Rejected");
        }

        void sre_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            Console.Write("\rSpeech Hypothesized: \t{0}", e.Result.Text);
        }

        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            Console.Write("\rSpeech Recognized: \t{0}", e.Result.Text);
            
            if (SaidSomething == null)
                return;
            
            var said = new SaidSomethingArgs();
            said.RGBColor = Color.FromRgb(0, 0, 0);
            said.Shape = 0;
            said.Verb = 0;
            said.Phrase = e.Result.Text;

            // First check for color, in case both color _and_ shape were both spoken
            bool foundColor = false;
            
            // Look for a match in the order of the lists below, first match wins.
            List<Dictionary<string, WhatSaid>> allDicts = new List<Dictionary<string, WhatSaid>>()
                { GameplayPhrases, SinglePhrases };

            bool found = false;
            for (int i = 0; i < allDicts.Count && !found; ++i)
            {
                foreach (var phrase in allDicts[i])
                {
                    if (e.Result.Text.Contains(phrase.Key))
                    {
                        said.Verb = phrase.Value.verb;
                        said.Shape = phrase.Value.shape;
                        if ((said.Verb == Verbs.DoShapes) && (foundColor))
                        {
                            said.Verb = Verbs.ShapesAndColors;
                            said.Matched += " " + phrase.Key;
                        }
                        else
                        {
                            said.Matched = phrase.Key;
                            said.RGBColor = phrase.Value.color;
                        }
                        found = true;
                        break;
                    }
                }
            }

            if (!found)
                return;

            if (paused) // Only accept restart or reset
            {
                if ((said.Verb != Verbs.Resume) && (said.Verb != Verbs.Reset))
                    return;
                paused = false;
            }
            else
            {
                if (said.Verb == Verbs.Resume)
                    return;
            }
            
            if (said.Verb == Verbs.Pause)
                paused = true;

            SaidSomething(new object(), said);
        }
    }
}
