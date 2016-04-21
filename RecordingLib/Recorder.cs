using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using AForge.Video.FFMPEG;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;

namespace RecordingLib
{
    public class Recorder
    {

        private bool _isRecording = false;





        /// <summary>
        /// Triggers when a new RecordSession is started
        /// </summary>
        public event EventHandler StartRecordSessionEvent;


        public void OnStartRecordSessionEvent()
        {
            if (StartRecordSessionEvent != null)
                StartRecordSessionEvent(this, new EventArgs()); // TODO add custom event args
        }


        /// <summary>
        /// Starts recording session
        /// </summary>
        /// <param name="rs"></param>
        public void startRecording(RecordingSettings rs)
        {
            _isRecording = true;




            #region stringbuilding
            StringBuilder sb = new StringBuilder();
            sb.Append(rs.Path + "\\");
            sb.Append(Environment.UserName.ToUpper() + "_");
            sb.Append(DateTime.Now.ToString("d_MMM_yyyy_HH_mm_ssff"));
            sb.Append(".avi"); //TODO: Add variable file extension!
            #endregion

            using (VideoFileWriter vfw = new VideoFileWriter())
            {


                vfw.Open(sb.ToString(), rs.RecordingResolution.Width, rs.RecordingResolution.Height, rs.FPS, rs.Codec, rs.Bitrate); //opens a new video stream for recording
                OnStartRecordSessionEvent();


                do
                {
                    //Create a new bitmap
                    var img = new Bitmap(rs.RecordingResolution.Width, rs.RecordingResolution.Height, PixelFormat.Format32bppRgb);//Format24bppRgb

                    // Create a graphics object from the bitmap.
                    var gfxScreenshot = Graphics.FromImage(img);

                    //Take a screenshot of the complete screen area 
                    gfxScreenshot.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);
                    vfw.WriteVideoFrame(img);



                    gfxScreenshot.Dispose();
                    img.Dispose();

                    Thread.Sleep(rs.Interval); // Waits rs.Interval times (creates timelapse effect)

                } while (_isRecording == true);
                vfw.Close();

            }



        }


        public void stopRecording()
        {
            _isRecording = false;
        }







    }
}
