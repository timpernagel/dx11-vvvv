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
    [PluginInfo(Name = "Kinect2 Playback StepByStep",
                Category = "Devices",
                Version = "Microsoft",
                Author = "timpernagel",
                Tags = "DX11",
                Help = "Playback of XEF-Files")]

    public class KinectPlaybackStepByStepNode : IPluginEvaluate, IDisposable
    {

        [Input("XEF-File", DefaultString = "", StringType = StringType.Filename)]
        ISpread<string> FFile;

        [Input("LoadClip", IsBang = true, IsSingle = true)]
        ISpread<bool> FLoadClip;
        
        [Input("StepOnce", IsBang = true, IsSingle = true)]
        ISpread<bool> FStepOnce;

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

                OneArgDelegate playback = new OneArgDelegate(KinectPlayback);
                playback.BeginInvoke(@FFile[0], null, null);

                init = true;
            }
                       

            if (this.FStepOnce[0])
            {

                if (playback.State == KStudioPlaybackState.Paused)
                {
                    playback.StepOnce(KStudioEventStreamDataTypeIds.UncompressedColor);
                    UpdateOutputs();
                }
            }
            else
            { }
            
            // ------------------------------------------------ RESET


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

            FLogger.Log(LogType.Debug, "StateChanged to " + playback.State.ToString());
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
            FCurrentFrame[0] = Convert.ToInt16(playback.CurrentRelativeTime.TotalMilliseconds)/30 ;
            FIsConnected[0] = client.IsServiceConnected;

        }
        
        public void KinectPlayback(string filePath)
        {
            client = KStudio.CreateClient();
            client.ConnectToService();

            playback = client.CreatePlayback(filePath);
            isConnected = true;
            FDuration[0] = playback.Duration.Milliseconds;

            playback.StateChanged += (s, e) => StateChanged();
            
            playback.StartPaused(); //startpaused eht nicht mit seeking
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

            if (playback.State == KStudioPlaybackState.Stopped)
            {
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
