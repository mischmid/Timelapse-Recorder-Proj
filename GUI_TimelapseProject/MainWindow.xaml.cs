using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;
using RecordingLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;


namespace GUI_TimelapseProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Members
        const string _progname = "TimeLapseProject";
        private Recorder rec = new Recorder();
        private DispatcherTimer LabelUpdateTimer;
        Stopwatch sw = new Stopwatch();
        RecordingSettings settings;
        


        public MainWindow()
        {
            InitializeComponent();
            LabelUpdateTimer = new DispatcherTimer(); //Initializes timer
            LabelUpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 1); //Sets timer tick to 01 miliseconds 
            LabelUpdateTimer.Tick += LabelUpdaterTimer_Tick;
            settings = new RecordingSettings(new RecordingSettings.Resolution(Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width), (int)slFPS.Value, (int)slBitrate.Value * 1000000, txtPath.Text, (int)(dudInterval.Value * 1000));


   
        }


        //Updates timer label
        private void LabelUpdaterTimer_Tick(object sender, EventArgs e)
        {
            lb_Timer.Content = sw.Elapsed.ToString("hh\\:mm\\:ss\\:ff");
        }





        /// <summary>
        /// Opens an output path dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog(); //New folderbrowser dialog not implemented in wpf, use winforms or 3rd party framework
            fbd.RootFolder = Environment.SpecialFolder.UserProfile;
            fbd.Description = "Select the folder where the output video is saved";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                txtPath.Text = (fbd.SelectedPath);
            }
            

        }



        /// <summary>
        /// Opens an explorer Window showing the selected path
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ViewPath_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(txtPath.Text))
            {
                Process process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = txtPath.Text;
                process.Start();
            }
            else
            {
                System.Windows.MessageBox.Show("Path can't be null", _progname);
            }
            


        }



        /// <summary>
        /// Saves the settings object state as JSON file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            settings = new RecordingSettings(new RecordingSettings.Resolution(Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width), (int)slFPS.Value, (int)slBitrate.Value * 1000000, txtPath.Text, (int)(dudInterval.Value * 1000));
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "JSON file | *.json";
            sfd.DefaultExt = "json";
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK && settings != null)
            {
                serializeTOJSON(sfd.FileName, settings);
            }

            

        }


        /// <summary>
        /// Deserializes file to RecordingSettings
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_LoadSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON file | *.json";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                deserializeFromJSON(ofd.FileName, new JSchemaGenerator().Generate(typeof(RecordingSettings)));
            }
        }




        /// <summary>
        /// Deserializes object from json
        /// </summary>
        /// <param name="path"></param>
        /// <param name="jschem">Schema object to check if deserialized object corresponds to it</param>
        private void deserializeFromJSON(string path, JSchema jschem)
        {

            using (StreamReader file = File.OpenText(path))
            {
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    //Validates if JSON File is correct!
                    JSchemaValidatingReader validatingReader = new JSchemaValidatingReader(reader);
                    validatingReader.Schema = jschem;
                    JsonSerializer serializer = new JsonSerializer();
                    try
                    {
                        settings = serializer.Deserialize<RecordingSettings>(validatingReader);
                        updateGUIValues();
                        System.Windows.MessageBox.Show($"Settings file succesfully loaded!", _progname);

                    }
                    catch (Exception ex) when (ex is JSchemaValidationException || ex is JsonReaderException)
                    {

                        System.Windows.MessageBox.Show($"ERROR: File does not correspond to {_progname} settings format!", _progname);

                    }
                    finally
                    {
                        validatingReader.Close();
                    }
                }
            }

        }


        /// <summary>
        /// Serializes object to json
        /// </summary>
        /// <param name="path"></param>
        /// <param name="obj">Object to serialize</param>
        private void serializeTOJSON<T>(string path, T obj)
        {
             using (StreamWriter file = File.CreateText(path))
            {
                using (JsonTextWriter writer = new JsonTextWriter(file)) {

                    JSchemaValidatingWriter validatingWriter = new JSchemaValidatingWriter(writer);
                    validatingWriter.Schema = new JSchemaGenerator().Generate(typeof(T));
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(validatingWriter, obj);

                }
            }
        }


        /// <summary>
        /// Starts the recording process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStartRecording_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(txtPath.Text))
            {
                //Create a new object of RecordSettings class
                RecordingSettings settings = new RecordingSettings(new RecordingSettings.Resolution(Screen.PrimaryScreen.Bounds.Height, Screen.PrimaryScreen.Bounds.Width), (int)slFPS.Value, (int)slBitrate.Value * 1000000, txtPath.Text, (int)(dudInterval.Value * 1000));
                Thread recordingThread = new Thread(() => rec.startRecording(settings));

                //Hook the Rec_StartRecordSessionEvent to the event
                rec.StartRecordSessionEvent += Rec_StartRecordSessionEvent;

                //Optimize: Foreach loop in method with tags
                // Turn the GUI "DAU-Safe"
                btnStartRecording.IsEnabled = false;
                btnStopRecording.IsEnabled = true;
                slBitrate.IsEnabled = false;
                slFPS.IsEnabled = false;
                btn_OpenFolder.IsEnabled = false;
                dudInterval.IsEnabled = false;
                btn_LoadSettings.IsEnabled = false; 
                btn_SaveSettings.IsEnabled = false;

                lb_Timer.Foreground = Brushes.Red;


                recordingThread.Start();
            }

            else
            {
                System.Windows.MessageBox.Show("Path can't be null", _progname);

            }







        }


        /// <summary>
        /// stops the recording process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            rec.stopRecording();
            rec.StartRecordSessionEvent -= Rec_StartRecordSessionEvent;
            LabelUpdateTimer.Stop();
            sw.Stop();


            btnStartRecording.IsEnabled = true;
            btnStopRecording.IsEnabled = false;
            slBitrate.IsEnabled = true;
            slFPS.IsEnabled = true;
            btn_OpenFolder.IsEnabled = true;
            dudInterval.IsEnabled = true;   
            btn_LoadSettings.IsEnabled = true;
            btn_SaveSettings.IsEnabled = true;

            lb_Timer.Foreground = Brushes.Black;



        }


        /// <summary>
        /// Safely stops recording session when closing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            rec.stopRecording();
            rec.StartRecordSessionEvent -= Rec_StartRecordSessionEvent;
            LabelUpdateTimer.Stop();
            sw.Stop();
        }



        /// <summary>
        /// Starts DispatcherTimer and StopWatch when the RecordSessionEvent is fired
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rec_StartRecordSessionEvent(object sender, EventArgs e)
        {
            LabelUpdateTimer.Start();
            sw.Restart();
        }


        /// <summary>
        /// Updates the values on GUI
        /// </summary>
        private void updateGUIValues()
        {
            txtPath.Text = settings.Path;
            slBitrate.Value = settings.Bitrate / 1000000;
            slFPS.Value = settings.FPS;
            dudInterval.Value = Convert.ToDouble(settings.Interval) / 1000;
        }


    }
}
