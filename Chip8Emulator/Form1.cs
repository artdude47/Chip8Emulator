using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chip8Emulator
{
    public partial class Form1 : Form
    {
        private Chip8 chip8 = new Chip8();
        private Timer emulationTimer;

        public Form1()
        {
            InitializeComponent();

            emulationTimer = new Timer
            {
                Interval = 16 // Roughly 60 Hz
            };
            emulationTimer.Tick += EmulationTimer_Tick;
        }

        private void loadROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CHIP-8 ROM FILES (*.ch8)|*.ch8|All Files (*.*)|*.*",
                Title = "Load CHIP-8 ROM"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    byte[] romData = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                    chip8.LoadROM(romData);
                    emulationTimer.Start();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load ROM: {ex.Message}", "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void closeROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            emulationTimer.Stop();
            chip8.Initialize();
            MessageBox.Show("Emulator reset.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chip8.Initialize();
        }

        private void EmulationTimer_Tick(object sender, EventArgs e)
        {
            chip8.EmulateCycle();
            RenderGraphics();
        }

        private void RenderGraphics()
        {
            int scale = Math.Min(pictureBoxDisplay.Width / 64, pictureBoxDisplay.Height / 32);
            Bitmap bitmap = new Bitmap(64 * scale, 32 * scale);

            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);

                for (int x = 0; x < 64; x++)
                {
                    for (int y = 0; y < 32; y++)
                    {
                        if (chip8.Display[x, y])
                        {
                            g.FillRectangle(Brushes.White, x * scale, y * scale, scale, scale);
                        }
                    }
                }
            }

            pictureBoxDisplay.Image = bitmap;
        }

        private void pictureBoxDisplay_Resize(object sender, EventArgs e)
        {
            RenderGraphics();
        }
    }
}
