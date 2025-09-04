using System.Runtime.InteropServices;

namespace CreoCADBOT
{
    public partial class incomminglb : UserControl
    {
        public incomminglb()
        {
            InitializeComponent();
            Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0,0,Width,Height,5,5));

            /*
            this.BorderStyle = BorderStyle.None;
            this.ClientSize = new Size(width, height);
            this.Size = new Size(Width, Height);
            Region = System.Drawing.Region.FromHrgn(createRoundRect(0,0,Width,Height,30,30));*/
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
        public string Message
        {
            get
            {
                return label1.Text;
            }
            set
            {
                string text = FitTextToWidth(value,label1.Font,556 );
                label1.Text = text;
                this.Size = new System.Drawing.Size(label1.Width,label1.Height+10);
                //Adjesth();
                //AdjustLabelToFit(label1);
            }
        }
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
        public int AdjestHeight
        {
            get
            {
                SizeF uu = this.CreateGraphics().MeasureString(label1.Text,label1.Font,(int)(this.Size.Height*0.8));
                int hei = (int)Math.Ceiling(uu.Height+uu.Height+0.2);
                if(hei < 74)
                {
                    return 74;
                }
                return hei;
            }
        }

        void Adjesth()
        {
            label1.Height = GetH(label1) + 10;

        }/*
        private void AdjustLabelToFit(Label dynamicLabel)
        {
            float fontSize = 12; // Start with a base font size
            dynamicLabel.Font = new Font(dynamicLabel.Font.FontFamily, fontSize);
            SizeF textSize;

            using (Graphics g = dynamicLabel.CreateGraphics())
            {
                do
                {
                    // Measure the text size with the current font size
                    textSize = g.MeasureString(dynamicLabel.Text, new Font(dynamicLabel.Font.FontFamily, fontSize));

                    // If the text fits within the panel, increase font size
                    if (textSize.Width < panel1.Width && textSize.Height < panel1.Height)
                    {
                        fontSize += 0.5f; // Increment font size
                    }
                    else
                    {
                        // If it doesn't fit, shorten the text
                        while (textSize.Width > panel1.Width && dynamicLabel.Text.Length > 3)
                        {
                            dynamicLabel.Text = dynamicLabel.Text.Substring(0, dynamicLabel.Text.Length - 1);
                            textSize = g.MeasureString(dynamicLabel.Text, new Font(dynamicLabel.Font.FontFamily, fontSize));
                        }
                        dynamicLabel.Text += "...";
                        break;
                    }
                } while (true);
            }

            // Set the final font size
            dynamicLabel.Font = new Font(dynamicLabel.Font.FontFamily, fontSize - 0.5f);
        }*/
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
