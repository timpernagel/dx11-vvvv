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
using VVVV.PluginInterfaces.V1;
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
        /*[Input("Index", IsSingle = true)]
        IDiffSpread<int> FInIndex;*/

        

        //[Input("Enable Color", IsSingle = true, DefaultValue = 1)]
        //IDiffSpread<bool> FInEnableColor;

        [Input("XEF-File", DefaultString = "", StringType = StringType.Filename)]
        ISpread<string> FFile;

        [Input("Play", IsBang = true, IsSingle = true)]
        ISpread<bool> FPlay;

        [Input("Loop Count", IsSingle = true)]
        ISpread<int> FLoopCount;

        [Output("Duration")]
        ISpread<float> FDuration;

        [Output("IsPlaying", IsSingle = true)]
        ISpread<bool> FIsPlaying;

		[Import()]
        public ILogger FLogger;


        private bool playbackOnce = false;

       // private KStudioPlayback playback = new KStudioPlayback();


        /// <summary> Delegate for placing a job with no arguments onto the Dispatcher </summary>
       // private delegate void NoArgDelegate();

        /// <summary> Delegate for placing a job with a single string argument onto the Dispatcher </summary>
        // <param name="arg">string argument</param>
        private delegate void OneArgDelegate(string arg);



        public void Evaluate(int SpreadMax)
        {

            if (this.FPlay[0])
           {
               FLogger.Log(LogType.Debug, "Play");
               OneArgDelegate playback = new OneArgDelegate(PlaybackClip);
               playback.BeginInvoke(@FFile[0], null, null);
               playbackOnce = true;
           }
           else { 
           }

                       
        }
         
         public void Dispose()
        {

             FLogger.Log(LogType.Debug, "DisposeFunction triggered");

         }



        public void PlaybackClip(string filePath)
         // public static void PlaybackClip(string filePath, uint loopCount)
         {

            using (KStudioClient client = KStudio.CreateClient())
            {

                client.ConnectToService();

                using (KStudioPlayback playback = client.CreatePlayback(filePath))
                {
                    playback.LoopCount = Convert.ToUInt16(FLoopCount[0]);
                    playback.Start();
                    FDuration[0] = playback.Duration.Milliseconds;

                    while (playback.State == KStudioPlaybackState.Playing)
                    {
                        FIsPlaying[0] = true;
                       // System.Threading.Thread.Sleep(500);
                    }

                    if (playback.State == KStudioPlaybackState.Error)
                    {
                        throw new InvalidOperationException("Error: Playback failed!");
                    }

                    if (playback.State == KStudioPlaybackState.Stopped)
                    {
                        FLogger.Log(LogType.Debug, "Finished Playback");
                        FIsPlaying[0] = false;
                        playback.Dispose();
                    }

                }
               
                client.DisconnectFromService();
                client.Dispose();
            }

        }


    }

}
