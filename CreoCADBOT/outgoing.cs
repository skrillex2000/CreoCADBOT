using ElevenLabs.TextToSpeech;
using ElevenLabs;
using NAudio.Wave;
using System.Text;
using System.Runtime.InteropServices;
using System.Data;

namespace CreoCADBOT
{
    public partial class OutGoing : UserControl
    {
        private DataGridView dataGridView;
        public OutGoing()
        {
            InitializeComponent();

            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 10, 10));

        }
        public string Message
        {
            get
            {
                return label1.Text;
            }
            set
            {
                label1.Visible = true;
                string text = FitTextToWidth(value, label1.Font, 556);
                label1.Text = text;
                this.Size = new System.Drawing.Size(label1.Width, label1.Height+10);
              //   test(value);
                //Adjesth(); 
            }
        }
        public DataTable DataGridView
        {
            set
            {
                dataGridView = new DataGridView
                {
                  //  Dock = DockStyle.Top, // Fill the form
                   // AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, // Adjust column width
                    AllowUserToAddRows = false, // Disable row addition by user
                    ReadOnly = true // Set to read-only for display
                };
                label1.Visible = false;
                dataGridView.RowHeadersVisible = false;                
                dataGridView.DataSource = value;  
                this.Controls.Add(dataGridView);

                this.Size = new Size(dataGridView.Width, dataGridView.Height+10);
               // Size rectSi = dataGridView.Size;
                //dataGridView.Size = new System.Drawing.Size(rectSi.Width, 52);
            }
           
        }
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
            (
            int nLeftRect,
            int nTopRect,
            int nRightRect,
            int nBottomRect,
            int nWidthEllipse,
            int nHeighthEllipse
            );
        private string FitTextToWidth(string text, Font font, int maxWidth)
        {
            string[] words = text.Split(' ');
            string result = string.Empty;
            string line = string.Empty;

            using (Graphics g = this.CreateGraphics())
            {
                foreach (string word in words)
                {
                    string testLine = line.Length > 0 ? line + " " + word : word;
                    SizeF textSize = g.MeasureString(testLine, font);

                    if (textSize.Width > maxWidth)
                    {
                        // Add the current line to the result with a line break
                        result += (line + "\n");
                        line = word; // Start a new line with the current word
                        //label1.Width += 10;
                    }
                    else
                    {
                        line = testLine;
                    }
                }

                // Add the last line
                if (line.Length > 0)
                {
                    result += line;
                }
            }

            return result;
        }
        private void AdjustLabelHeight(Label label, Panel panel)
        {
            // Set the maximum width to the panel's width
            int maxWidth = panel.Width;

            // Measure the required size of the text
            Size proposedSize = new Size(maxWidth, int.MaxValue);
            Size textSize = TextRenderer.MeasureText(label.Text, label.Font, proposedSize, TextFormatFlags.WordBreak);

            // Adjust the label's size
            label.Width = maxWidth; // Keep the width fixed
            label.Height = textSize.Height; // Adjust the height dynamically
        }
        async void test(string message)
        {
            var api = new ElevenLabsClient("sk_be19405a9e2366eb27ac396f9cc2b68a2e2054dbc033aec9");
            var text = message;
            var voice = (await api.VoicesEndpoint.GetAllVoicesAsync()).FirstOrDefault();
            var request = new TextToSpeechRequest(voice, text);
            var voiceClip = await api.TextToSpeechEndpoint.TextToSpeechAsync(request);
            File.WriteAllBytesAsync($"Message.mp3", voiceClip.ClipData.ToArray());
            PlayAudio($"Message.mp3");
        }
        static void PlayAudio(string filePath)
        {
            try
            {
                using (var audioFile = new AudioFileReader(filePath))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.Play();

                    // Wait until playback stops
                    while (outputDevice.PlaybackState == PlaybackState.Playing)
                    {
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing audio: {ex.Message}");
            }
        }
        void Adjesth()
        {
            label1.Height = GetH(label1) + 10;

        }

        int GetH(Label label)
        {
            using (Graphics g = label.CreateGraphics())
            {
                SizeF size = g.MeasureString(label.Text, label.Font, 495);
                return (int)Math.Ceiling(size.Height);
            } 
        }

        public string Message2 { get; set; }
    }
}
