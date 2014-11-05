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

        [Input("LoadClip", IsBang = true, IsSingle = true)]
        ISpread<bool> FLoadClip;

        [Input("DoStep", IsBang = true, IsSingle = true)]
        ISpread<bool> FDoStep;

        [Input("Frame", IsSingle = true)]
        ISpread<int> FFrame;

        [Input("Seek", IsBang = true, IsSingle = true)]
        ISpread<bool> FSeek;

        [Input("Pause", IsBang = true, IsSingle = true)]
        ISpread<bool> FPause;

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

        [Output("IsSeeked")]
        ISpread<bool> FIsSeeked;

        [Output("IsConnected")]
        ISpread<bool> FIsConnected;

        [Import()]
        public ILogger FLogger;

        private KStudioClient client;
        private KStudioPlayback playback;

        private bool isPlaying = false;
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

        public void Evaluate(int SpreadMax)
        {

            if (!init && FLoadClip[0])
            {

                OneArgDelegate playback = new OneArgDelegate(LoadClip);
                playback.BeginInvoke(@FFile[0], null, null);

                init = true;
            }

            
            // ------------------------------------------------ SEEKING


            if (this.FDoStep[0])
            {
                if (!isPlaying)
                {
                    OneArgDelegate playback = new OneArgDelegate(LoadClip);
                    playback.BeginInvoke(@FFile[0], null, null);
                }
                else
                {
                    playback.StepOnce();
                }

            }
            else
            { }

            if (this.FSeek[0])
            {

                //TimeSpan seekTo = new TimeSpan(FSeekTime[0]);
                //  TimeSpan seekTo = new TimeSpan(0,0,FFrame[0]);
                //playback.SeekByRelativeTime();
                // playback.SeekByRelativeTime(seekTo);

                seekCount++;
                playback.Pause();

                TimeSpan seekTo = new TimeSpan(0, 0, 0, 3, 0);
                playback.SeekByRelativeTime(seekTo);
                playback.Resume();
                //FLogger.Log(LogType.Debug, seekTo.ToString());


            }
            else
            { }

            if (this.FPause[0])
            {

                playback.Pause();

            }
            else
            { }

            // ------------------------------------------------ RESET


            if (this.FReset[0])
            {
                if (isPlaying) // das is noch nicht so gut!
               // if (playback.State == KStudioPlaybackState.Playing || playback.State == KStudioPlaybackState.Paused) // das is noch nicht so gut!
                {

                    Dispose();

                }
                else { }
            }
            else
            { }

        }

        public void StateChanged()
        {

            FLogger.Log(LogType.Debug, "StateChanged!");
            FState[0] = playback.State.ToString();
        }

        public void Seeked()
        {

            FLogger.Log(LogType.Debug, "Seeked!");
        }

        public void UpdateOutputs()
        {
            // FLogger.Log(LogType.Debug, "DisposeFunction triggered");
            // FDuration[0] = playback.Duration.Milliseconds;
            FMillisec[0] = playback.CurrentRelativeTime.Milliseconds;
            FSec[0] = playback.CurrentRelativeTime.Seconds;
            FMin[0] = playback.CurrentRelativeTime.Minutes;
           // FState[0] = playback.State.ToString();
            FIsConnected[0] = client.IsServiceConnected;


           // if (playback.Seeked)
                
          //  FIsSeeked[0] = true

        }

        public void Init()
        {
            FLogger.Log(LogType.Debug, "INIT!");
            OneArgDelegate playback = new OneArgDelegate(StandardPlayback);
            playback.BeginInvoke(@FFile[0], null, null);
        }

        public void StandardPlayback(string filePath)
        {

            client = KStudio.CreateClient();
            client.ConnectToService();

            playback = client.CreatePlayback(filePath);
           // playback.LoopCount = Convert.ToUInt16(FLoopCount[0]);


           // TimeSpan seekTo = new TimeSpan(0, 0, 0, 5, 0);

           // playback.AddPausePointByRelativeTime(seekTo);



            playback.Start();
            isPlaying = true;
            FDuration[0] = playback.Duration.Milliseconds;

            while (playback.State == KStudioPlaybackState.Playing)
            {
                //  FLogger.Log(LogType.Debug, "IsPlaying");
                // System.Threading.Thread.Sleep(500);
                UpdateOutputs();
            }

            if (playback.State == KStudioPlaybackState.Error)
            {
                throw new InvalidOperationException("Error: Playback failed!");
                Dispose();
            }

            if (playback.State == KStudioPlaybackState.Stopped)
            {
                FLogger.Log(LogType.Debug, "Playback Stopped");

                if (isPlaying)
                {
                    Dispose();
                    isPlaying = false;
                }

            }

            client.DisconnectFromService();
            Dispose();

        }

        public void LoadClip(string filePath)
        {
            client = KStudio.CreateClient();
            client.ConnectToService();

            playback = client.CreatePlayback(filePath);
            isConnected = true;
            FDuration[0] = playback.Duration.Milliseconds;

            //geht vermutlich...
            playback.StateChanged += (s, e) => FState[0] = playback.State.ToString(); 
            playback.Seeked += (s, e) => Seeked();



           // playback.Mode = KStudioPlaybackMode.TimingDisabled;
           // playback.StartPaused();

            isPlaying = true;

            TimeSpan seekTo = new TimeSpan(0, 0, 0, 0, seekCount);
           // playback.AddPausePointByRelativeTime(seekTo);

            playback.Start();

            FLogger.Log(LogType.Debug, "In " + playback.InPointByRelativeTime.ToString());
            FLogger.Log(LogType.Debug, "Out " + playback.OutPointByRelativeTime.ToString());
            FLogger.Log(LogType.Debug, "Mode " + playback.Mode.ToString());
            FLogger.Log(LogType.Debug, "Pause " + playback.PausePointsByRelativeTime.ToString());
           // FLogger.Log(LogType.Debug, "User " + playback.UserState.ToString()); //buggy
           // FLogger.Log(LogType.Debug, "Flags " + playback.Flags.ToString()); //buggy
            
            while (playback.State == KStudioPlaybackState.Playing)
            {
                // System.Threading.Thread.Sleep(500);
                UpdateOutputs();
            }

            if (playback.State == KStudioPlaybackState.Error)
            {
                throw new InvalidOperationException("Error: Playback failed!");
                Dispose();
            }

           
        }

        public void Dispose()
        {
            FLogger.Log(LogType.Debug, "DisposeFunction triggered");
            if(isConnected)
            {
                client.DisconnectFromService();
            }

            isConnected = false;
            isPlaying = false;
            init = false;

            //UpdateOutputs();
            
            playback = null;
            client = null;

            //playback.Dispose();
            // client.Dispose();

        }


    }

}
