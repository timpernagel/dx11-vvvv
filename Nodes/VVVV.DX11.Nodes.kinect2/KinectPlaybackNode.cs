using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using VVVV.PluginInterfaces.V2;
using VVVV.MSKinect.Lib;
using VVVV.Utils.VMath;
using Microsoft.Kinect;
using Microsoft.Kinect.Tools;

using VVVV.Core.Logging;
//using VVVV.PluginInterfaces.V1;
using System.ComponentModel.Composition;

namespace VVVV.MSKinect.Nodes
{
    [PluginInfo(Name = "Kinect2 Playback",
                Category = "Devices",
                Version = "Microsoft",
                Author = "timpernagel",
                Tags = "DX11",
                Help = "Playback of XEF-Files")]

    public class KinectPlaybackNode : IPluginEvaluate, IDisposable
    {

        [Input("XEF-File", DefaultString = "", StringType = StringType.Filename)]
        ISpread<string> FFile;

        [Input("Play", IsBang = true, IsSingle = true)]
        ISpread<bool> FPlay;

        [Input("Stop", IsBang = true, IsSingle = true)]
        ISpread<bool> FStop;

        [Input("Loop Count", IsSingle = true)]
        ISpread<int> FLoopCount;

        [Input("DoStep", IsBang = true, IsSingle = true)]
        ISpread<bool> FDoStep;

        [Input("Reset", IsBang = true, IsSingle = true)]
        ISpread<bool> FReset;

        [Output("Duration")]
        ISpread<float> FDuration;

        [Output("Min")]
        ISpread<float> FMin;

        [Output("Sec")]
        ISpread<float> FSec;

        [Output("Millisec")]
        ISpread<float> FMillisec;

        [Output("State")]
        ISpread<string> FState;

        [Output("IsPlaying", IsSingle = true)]
        ISpread<bool> FIsPlaying;

        [Import()]
        public ILogger FLogger;

        private KStudioClient client;
        private KStudioPlayback playback;

        private bool isPlaying = false;

        /// <summary> Delegate for placing a job with a single string argument onto the Dispatcher </summary>
        // <param name="arg">string argument</param>
        private delegate void OneArgDelegate(string arg);

        public void Evaluate(int SpreadMax)
        {

            // ------------------------------------------------ STANDARD PLAYBACK

            if (this.FPlay[0] && !FIsPlaying[0])
            {
                FLogger.Log(LogType.Debug, "Start Playback");

                Dispose();

                OneArgDelegate playback = new OneArgDelegate(StandardPlayback);
                playback.BeginInvoke(@FFile[0], null, null);
            }
            else
            {
            }

            if (this.FStop[0])
            {
                FLogger.Log(LogType.Debug, "Stop Playback");

                if (FIsPlaying[0])
                {
                    playback.Stop();
                }
            }
            else
            {
            }

            // ------------------------------------------------ SEEKING

            
            if (this.FDoStep[0])
            {
                if (!isPlaying)
                {
                    OneArgDelegate playback = new OneArgDelegate(SeekedPlayback);
                    playback.BeginInvoke(@FFile[0], null, null);
                }
                else
                {
                    playback.StepOnce();
                }

            }
            else
            { }

            // ------------------------------------------------ RESET


            if (this.FReset[0])
            {
                if (isPlaying)
                {

                    Dispose();

                }
                else { }
            }
            else
            { }

        }
        public void UpdateTimeOutputs()
        {
            // FLogger.Log(LogType.Debug, "DisposeFunction triggered");
            // FDuration[0] = playback.Duration.Milliseconds;
            FMillisec[0] = playback.CurrentRelativeTime.Milliseconds;
            FSec[0] = playback.CurrentRelativeTime.Seconds;
            FMin[0] = playback.CurrentRelativeTime.Minutes;
            FState[0] = playback.State.ToString();

        }

        public void StandardPlayback(string filePath)
        {

            client = KStudio.CreateClient();
            client.ConnectToService();

            playback = client.CreatePlayback(filePath);
            playback.LoopCount = Convert.ToUInt16(FLoopCount[0]);

            playback.Start();
            FIsPlaying[0] = true;
            FDuration[0] = playback.Duration.Milliseconds;

            while (playback.State == KStudioPlaybackState.Playing)
            {
              //  FLogger.Log(LogType.Debug, "IsPlaying");
                // System.Threading.Thread.Sleep(500);
                UpdateTimeOutputs();
            }

            if (playback.State == KStudioPlaybackState.Error)
            {
                throw new InvalidOperationException("Error: Playback failed!");
                Dispose();
            }

            if (playback.State == KStudioPlaybackState.Stopped)
            {
                FLogger.Log(LogType.Debug, "Playback Stopped");

                if (FIsPlaying[0]) 
                {
                    Dispose();
                    FIsPlaying[0] = false; 
                }

            }

            client.DisconnectFromService();
            Dispose();

        }

        public void SeekedPlayback(string filePath)
        {

            client = KStudio.CreateClient();
            client.ConnectToService();

            playback = client.CreatePlayback(filePath);
            playback.StartPaused();
            //playback.StepOnce();

            isPlaying = true;
            FState[0] = playback.State.ToString();

            //TimeSpan seekTo = new TimeSpan(FSeekTime[0]);
            //TimeSpan seekTo = new TimeSpan(0, 0, 1, 0, 0);
            //playback.Mode = KStudioPlaybackMode.TimingDisabled;
            //playback.SeekByRelativeTime(seekTo);
            //FIsPlaying[0] = true;
            //FDuration[0] = playback.Duration.Milliseconds;
           /* 
            while (playback.State == KStudioPlaybackState.Playing)
            {
                UpdateTimeOutputs();
                //  FLogger.Log(LogType.Debug, "IsPlaying");
                // System.Threading.Thread.Sleep(500);
            }*/
            /*
            if (playback.State == KStudioPlaybackState.Error)
            {
                throw new InvalidOperationException("Error: Playback failed!");
                Dispose();
            }

            if (playback.State == KStudioPlaybackState.Stopped)
            {
                FLogger.Log(LogType.Debug, "Playback Stopped");

                if (FIsPlaying[0])
                {
                    Dispose();
                    FIsPlaying[0] = false;
                }

            }
            
            client.DisconnectFromService();
            Dispose();*/

        }

        public void Dispose()
        {
            FLogger.Log(LogType.Debug, "DisposeFunction triggered");
            playback = null;
            client = null;
            //playback.Dispose();
           // client.Dispose();

        }


    }

}
