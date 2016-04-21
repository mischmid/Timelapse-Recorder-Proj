using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RecordingLib;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using System.IO;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Linq;

namespace TestConsole
{
    class Program
    {

        static void Main(string[] args)
        {
            RecordingSettings settings = new RecordingSettings((new RecordingSettings.Resolution(Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width)), 25, 8, "C:\\Users\\Michael\\Videos", 100);

            Recorder rec = new Recorder();

            Console.WriteLine("Start Recording | Enter");
            Console.ReadLine();
            Thread thread = new Thread(() => rec.startRecording(settings));

            thread.Start();

            Console.WriteLine("Stop Recording | Enter");
            Console.ReadLine();
            rec.stopRecording();
            Console.WriteLine("Finished recording!");





            Console.ReadKey();



        }




    }
}
