using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Midi;

namespace MidiAnalyzer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string path = @".\..\..\midi";
            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);

                foreach (var file in di.GetFiles())
                {
                    if (file.Extension.ToLowerInvariant() == ".mid" ||
                        file.Extension.ToLowerInvariant() == ".midi")
                    {
                        Console.WriteLine(file.Name);
                        bool strictMode = true;
                        MidiFile mf = new MidiFile(file.FullName, strictMode);

                        Console.WriteLine("Format {0}, Tracks {1}, Delta Ticks Per Quarter Note {2}",
                            mf.FileFormat, mf.Tracks, mf.DeltaTicksPerQuarterNote);
                    }
                }
            }
        }
    }
}
