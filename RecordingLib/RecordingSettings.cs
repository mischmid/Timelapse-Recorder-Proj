using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Video.FFMPEG;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RecordingLib
{
    [Serializable]
    public class RecordingSettings
    {
        /// <summary>
        /// Resolution struct, not the resolution setting
        /// </summary>
        /// 
        [Serializable]
        public struct Resolution
        {
            private readonly int _height;
            private readonly int _width;


            public Resolution(int height, int width)
            {
                this._height = height;
                this._width = width;
            }

            public int Height
            {
                get { return _height; }
            }
            public int Width
            {
                get { return _width; }
            }




        }



        //TODO:
        //a nested enum OutputVideoFile
        //        enum ImageFormat
        //        int bitrate - in Mbps(* 1000000)
        //        string path

        //        struct Resolution
        //                ImageHeight
        //                ImageWidth

        

        private readonly string _path;
        private readonly int _bitrate; //in Kbps
        private readonly int _fps;
        private readonly VideoCodec _codec;
        private readonly Resolution _res;
        private readonly int _interval;


        /// <summary>
        /// Creates an object of RecordingSettings to use with RecordingLib. This object holds all important information required for a recording process.
        /// </summary>
        /// <param name="res">Describes the resolution of the output video</param>
        /// <param name="fps">Describes the frames per second of the output video</param>
        /// <param name="codec">Describes the output video format.</param>
        /// <param name="bitrate">Describes the bitrate of the output video. In Kbps</param>
        /// <param name="path">Describes the output path</param>
        /// <param name="interval">Describes the interval between frame shots (In Milliseconds)</param>
        /// 


        public RecordingSettings(Resolution res, int fps, /*VideoCodec codec,*/ int bitrate, string path, int interval = 0)
        {
            this._res = res;
            this._fps = fps;
            this._codec = VideoCodec.MPEG4;
            this._bitrate = (int)bitrate; //convert to kbps
            this._path = path;
            this._interval = interval; //convert to milliseconds
        }

        /// <summary>
        /// The selected resolution settings for recording
        /// </summary>
        public Resolution RecordingResolution
        {
            get { return _res; }
        }


        /// <summary>
        /// Output video format
        /// </summary>
        public VideoCodec Codec
        {
            get { return _codec; }
        }


        /// <summary>
        /// Gets the output path of the video.
        /// </summary>
        public string Path
        {
            get { return _path; }
        }


        /// <summary>
        /// Gets selected recording bitrate in Kbps
        /// </summary>
        /// <remarks>in Kbps</remarks>
        public int Bitrate
        {
            get { return _bitrate; }
        }

        /// <summary>
        /// Gets FPS
        /// </summary>
        public int FPS
        {
            get { return _fps; }
        }

        /// <summary>
        /// Gets the interval between frame shots in Milliseconds
        /// </summary>
        public int Interval
        {
            get { return _interval; }
        }





    }
}
