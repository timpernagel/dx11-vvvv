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

using System.Threading;
using VVVV.DX11.Nodes.Kinect2;

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

        //[Input("LoadClip", IsBang = true, IsSingle = true)]
        //ISpread<bool> FLoadClip;

        [Input("Play", IsBang = true, IsSingle = true)]
        ISpread<bool> FPlay;

        [Input("Pause", IsBang = true, IsSingle = true)]
        ISpread<bool> FPause;

        [Input("Stop", IsBang = true, IsSingle = true)]
        ISpread<bool> FStop;

        [Input("LoopCount", IsSingle = true)]
        ISpread<uint> FLoopCount;

        [Input("StepOnce", IsBang = true, IsSingle = true)]
        ISpread<bool> FStepOnce;

        [Input("Seek", IsBang = true, IsSingle = true)]
        ISpread<bool> FSeek;

        [Input("SeekTime", IsSingle = true)]
        ISpread<double> FSeekTime;

        [Input("Reset", IsBang = true, IsSingle = true)]
        ISpread<bool> FReset;

        [Output("Duration")]
        ISpread<double> FDuration;

        [Output("CurrentPosition")]
        ISpread<double> FCurrentPosition;

        [Output("CurrentFrame")]
        ISpread<int> FCurrentFrame;

        [Output("CurrentTick")]
        ISpread<int> FCurrentTick;

        [Output("State")]
        ISpread<string> FState;

        [Output("IsSeeked")]
        ISpread<bool> FIsSeeked;

        [Output("IsConnected")]
        ISpread<bool> FIsConnected;

        [Import()]
        public ILogger FLogger;

        private KStudioClient client;
        private KStudioPlayback playback;

        private bool isPlaying = false;
        private bool isPaused = false;
        private bool init = false;
        private bool isConnected = false;
        private int seekCount = 1;

        //public delegate void EventHandler();
        //public static event EventHandler _show;

        /// <summary> Delegate for placing a job with no arguments onto the Dispatcher </summary>
        private delegate void NoArgDelegate();
        /// <summary> Delegate for placing a job with a single string argument onto the Dispatcher </summary>
        // <param name="arg">string argument</param>
        private delegate void OneArgDelegate(string arg);
        /// <summary> Delegate for placing a job with a single string argument onto the Dispatcher </summary>
        // <param name="arg">string argument</param>
        private delegate void TwoArgDelegate(string arg,string arg2);


        public void Evaluate(int SpreadMax)
        {

            //if (!init && FLoadClip[0])
            //{

            //    OneArgDelegate playback = new OneArgDelegate(KinectPlayback);
            //    playback.BeginInvoke(@FFile[0], null, null);

            //    init = true;
            //}


            if (this.FStepOnce[0])
            {
                if (!init)
                {
                    TwoArgDelegate playback = new TwoArgDelegate(KinectPlayback);
                    playback.BeginInvoke(@FFile[0], "step", null, null);
                    init = true;
                }
                else
                {
                    if (playback.State == KStudioPlaybackState.Paused)
                    {
                        //playback.StepOnce(KStudioEventStreamDataTypeIds.Ir);
                        //playback.StepOnce(KStudioEventStreamDataTypeIds.Depth);
                        playback.StepOnce(KStudioEventStreamDataTypeIds.UncompressedColor);
                        UpdateOutputs();
                    }
                }

            }
            else
            { }

            if (this.FPlay[0])
            {

                if (!init)
                {
                    TwoArgDelegate playback = new TwoArgDelegate(KinectPlayback);
                    playback.BeginInvoke(@FFile[0], "play", null, null);
                    init = true;
                }
                else
                {


                    if (playback.State == KStudioPlaybackState.Paused)
                    {
                        playback.Resume();
                    }

                    if (playback.State == KStudioPlaybackState.Stopped)
                    {
                        playback.Start();
                    }


                }

            }
            else
            { }

            if (this.FPause[0])
            {
                if (playback.State == KStudioPlaybackState.Playing)
                {
                    playback.Pause();
                }
            }
            else
            { }

            if (this.FStop[0])
            {

                if (playback.State == KStudioPlaybackState.Playing || playback.State == KStudioPlaybackState.Paused)
                {

                    playback.Stop();
                    Dispose();

                }
                else { }
            }
            else
            { }

            if (this.FSeek[0])
            {
                if (!init)
                {
                    TwoArgDelegate playback = new TwoArgDelegate(KinectPlayback);
                    playback.BeginInvoke(@FFile[0], "seek", null, null);
                    init = true;
                }
                else { 

                if (playback.State == KStudioPlaybackState.Playing)
                {
                    playback.Pause();
                }

                TimeSpan curTime = new TimeSpan(Convert.ToInt64(FSeekTime[0]) * 10000000);
              
                //TimeSpan curTime = new TimeSpan(100000000);
                playback.SeekByRelativeTime(curTime);

                playback.Resume();
                }
            }
            else
            { }


            if (this.FReset[0])
            {
                // if (isPlaying) // das is noch nicht so gut!
                // if (playback.State == KStudioPlaybackState.Playing || playback.State == KStudioPlaybackState.Paused) // das is noch nicht so gut!
                //{

                Dispose();

                // }
                //else { }
            }
            else
            { }


        }

        public void StateChanged()
        {
            UpdateOutputs();
            //  FLogger.Log(LogType.Debug, "StateChanged to " + playback.State.ToString());
            FState[0] = playback.State.ToString();

            if (playback.State == KStudioPlaybackState.Playing)
            {

                //playback.Pause();
                isPaused = true;

            }
        }

        public void UpdateOutputs()
        {
            FDuration[0] = playback.Duration.TotalMilliseconds;
            FCurrentPosition[0] = playback.CurrentRelativeTime.TotalMilliseconds;
            FCurrentFrame[0] = Convert.ToInt32(playback.CurrentRelativeTime.TotalMilliseconds) / 30;
            FCurrentTick[0] = Convert.ToInt32(playback.CurrentRelativeTime.Ticks);
            FIsConnected[0] = client.IsServiceConnected;

        }

        public void KinectPlayback(string filePath,string mode)
        {
            client = KStudio.CreateClient();
            client.ConnectToService();

            playback = client.CreatePlayback(filePath);
            isConnected = true;
            FDuration[0] = playback.Duration.Milliseconds;
            playback.LoopCount = FLoopCount[0];

            playback.StateChanged += (s, e) => StateChanged();

            if (mode == "play")
                playback.Start();

            if (mode == "step")
                playback.StartPaused();

            if (mode == "seek")
                playback.Start(); 
            
            //startpaused eht nicht mit seeking
            // playback.StartPaused(); //startpaused eht nicht mit seeking
            isPlaying = true;

            FLogger.Log(LogType.Debug, "In " + playback.InPointByRelativeTime.ToString());
            FLogger.Log(LogType.Debug, "Out " + playback.OutPointByRelativeTime.ToString());
            FLogger.Log(LogType.Debug, "Mode " + playback.Mode.ToString());
            FLogger.Log(LogType.Debug, "Pause " + playback.PausePointsByRelativeTime.ToString());


            while (playback.State == KStudioPlaybackState.Playing)
            {
                //System.Threading.Thread.Sleep(500);
                UpdateOutputs();
            }

            while (playback.State == KStudioPlaybackState.Paused)
            {
                //System.Threading.Thread.Sleep(500);
                UpdateOutputs();
            }

            if (playback.State == KStudioPlaybackState.Error)
            {
                throw new InvalidOperationException("Error: Playback failed!");
                Dispose();
            }

            if (playback.State == KStudioPlaybackState.Stopped)
            {
                Dispose();
            }


        }

        public void Dispose()
        {
            FLogger.Log(LogType.Debug, "DisposeFunction triggered");
            if (isConnected)
            {
                client.DisconnectFromService();
            }

            isConnected = false;
            isPlaying = false;
            isPaused = false;
            init = false;

            //UpdateOutputs();

            FDuration[0] = 0;
            FCurrentPosition[0] = 0;
            FCurrentFrame[0] = 0;
            FIsConnected[0] = false;

            playback = null;
            client = null;

            //playback.Dispose();
            // client.Dispose();

        }


    }

}
