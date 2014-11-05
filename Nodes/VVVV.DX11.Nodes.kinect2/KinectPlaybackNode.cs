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

        [Input("LoadClip", IsBang = true, IsSingle = true)]
        ISpread<bool> FLoadClip;

        [Input("Frame", IsSingle = true)]
        ISpread<int> FFrame;

        [Input("Seek", IsBang = true, IsSingle = true)]
        ISpread<bool> FSeek;

        [Input("Pause", IsBang = true, IsSingle = true)]
        ISpread<bool> FPause;

        [Input("Reset", IsBang = true, IsSingle = true)]
        ISpread<bool> FReset;

        [Output("Duration")]
        ISpread<double> FDuration;

        [Output("CurrentPosition")]
        ISpread<double> FCurrentPosition;

        [Output("CurrentFrame")]
        ISpread<int> FCurrentFrame;

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


        public void Evaluate(int SpreadMax)
        {
           

            if (!init && FLoadClip[0])
            {

                OneArgDelegate playback = new OneArgDelegate(LoadClip);
                playback.BeginInvoke(@FFile[0], null, null);

                init = true;
            }

            //if (init)
            
            // ------------------------------------------------ SEEKING


            if (this.FSeek[0])
            {
                
                //TimeSpan seekTo = new TimeSpan(FSeekTime[0]);
                //  TimeSpan seekTo = new TimeSpan(0,0,FFrame[0]);
                //playback.SeekByRelativeTime();
                // playback.SeekByRelativeTime(seekTo);

                seekCount++;

                if (playback.State == KStudioPlaybackState.Playing)
                {

                    playback.Pause();
                    isPaused = true;

                }

                if (playback.State == KStudioPlaybackState.Paused)
                {

                    TimeSpan seekTo = TimeSpan.FromMilliseconds((1000*FFrame[0])/30);
                    TimeSpan blub = new TimeSpan(seekTo.Days,seekTo.Hours, seekTo.Minutes, seekTo.Seconds, seekTo.Milliseconds);
                    TimeSpan bla = new TimeSpan(0, 0, 0, 0, 21117);

                    //TimeSpan.FromTicks

                    FLogger.Log(LogType.Debug, "Frame to milli " + seekTo);
                    playback.SeekByRelativeTime(seekTo);
                    playback.Resume();

                    //FCurrentFrame[0] = Convert.ToInt16(playback.CurrentRelativeTime.TotalMilliseconds) / 30);

                    //DAZWISCHEN MUSS WAS PASSIEREN
                    //austausch vom Bild, ein Frame warten
                   // playback.PropertyChanged

                    //playback.Pause();
                    //isPaused = false;
                }
                

            }
            else
            { }

            if (this.FPause[0])
            {

                if (playback.State == KStudioPlaybackState.Playing)
                {

                    playback.Pause();
                    isPaused = true;

                }
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

            FLogger.Log(LogType.Debug, "StateChanged to " + playback.State.ToString());
            FState[0] = playback.State.ToString();
            
            if (playback.State == KStudioPlaybackState.Playing)
            {

                //playback.Pause();
                isPaused = true;

            }
        }

        public void Seeked()
        {
            
            FLogger.Log(LogType.Debug, "Seeeeeeeeked ");
        }

        public void Property()
        {
            FLogger.Log(LogType.Debug, " - - - - Property ");

            if (playback.State == KStudioPlaybackState.Playing)
            {

                //playback.Pause();
                isPaused = true;

            }
        }


        public void UpdateOutputs()
        {
            // FLogger.Log(LogType.Debug, "DisposeFunction triggered");
            FDuration[0] = playback.Duration.TotalMilliseconds;
            //FMillisec[0] = playback.CurrentRelativeTime.Milliseconds;
            //FSec[0] = playback.CurrentRelativeTime.Seconds;
            FCurrentPosition[0] = playback.CurrentRelativeTime.TotalMilliseconds;
            FCurrentFrame[0] = Convert.ToInt16(playback.CurrentRelativeTime.TotalMilliseconds)/30 ;
            //FMin[0] = playback.CurrentRelativeTime.Minutes;
           // FState[0] = playback.State.ToString();
            FIsConnected[0] = client.IsServiceConnected;

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
            playback.LoopCount = 3;

            //geht vermutlich...
            playback.StateChanged += (s, e) => StateChanged();
            playback.Looped += (s, e) => LoopEvent();
           // //playback.Seeked += (s, e) => Seeked();
            //playback.Looped += seekEvent;
            playback.PropertyChanged += (s, e) => Property();
            
            playback.Start(); //startpaused eht nicht mit seeking
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

            if (playback.State == KStudioPlaybackState.Error)
            {
                throw new InvalidOperationException("Error: Playback failed!");
                Dispose();
            }

           
        }

        private void LoopEvent()
        {
            FLogger.Log(LogType.Debug, "Loooooop!");
        }

        private void seekEvent(object sender, EventArgs e)
        {
            throw new NotImplementedException();
            FLogger.Log(LogType.Debug, "Seekeeeed");
            //throw new NotImplementedException();
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
            isPaused = false;
            init = false;

            //UpdateOutputs();
            
            playback = null;
            client = null;

            //playback.Dispose();
            // client.Dispose();

        }


    }

}
