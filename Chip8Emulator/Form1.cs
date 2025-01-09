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

        private bool isPaused = false;
        private bool isFastSpeed = false;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
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
            RenderGraphics();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chip8.Initialize();
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
        }

        private void EmulationTimer_Tick(object sender, EventArgs e)
        {
            chip8.EmulateCycle();
            chip8.SimulateTimers();
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

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.D1: chip8.Keypad[0x1] = true; break;
                case Keys.D2: chip8.Keypad[0x2] = true; break;
                case Keys.D3: chip8.Keypad[0x3] = true; break;
                case Keys.D4: chip8.Keypad[0xC] = true; break;

                case Keys.Q: chip8.Keypad[0x4] = true; break;
                case Keys.W: chip8.Keypad[0x5] = true; break;
                case Keys.E: chip8.Keypad[0x6] = true; break;
                case Keys.R: chip8.Keypad[0xD] = true; break;

                case Keys.A: chip8.Keypad[0x7] = true; break;
                case Keys.S: chip8.Keypad[0x8] = true; break;
                case Keys.D: chip8.Keypad[0x9] = true; break;
                case Keys.F: chip8.Keypad[0xE] = true; break;

                case Keys.Z: chip8.Keypad[0xA] = true; break;
                case Keys.X: chip8.Keypad[0x0] = true; break;
                case Keys.C: chip8.Keypad[0xB] = true; break;
                case Keys.V: chip8.Keypad[0xF] = true; break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // Reset the key state when released
            switch (e.KeyCode)
            {
                case Keys.D1: chip8.Keypad[0x1] = false; break;
                case Keys.D2: chip8.Keypad[0x2] = false; break;
                case Keys.D3: chip8.Keypad[0x3] = false; break;
                case Keys.D4: chip8.Keypad[0xC] = false; break;

                case Keys.Q: chip8.Keypad[0x4] = false; break;
                case Keys.W: chip8.Keypad[0x5] = false; break;
                case Keys.E: chip8.Keypad[0x6] = false; break;
                case Keys.R: chip8.Keypad[0xD] = false; break;

                case Keys.A: chip8.Keypad[0x7] = false; break;
                case Keys.S: chip8.Keypad[0x8] = false; break;
                case Keys.D: chip8.Keypad[0x9] = false; break;
                case Keys.F: chip8.Keypad[0xE] = false; break;

                case Keys.Z: chip8.Keypad[0xA] = false; break;
                case Keys.X: chip8.Keypad[0x0] = false; break;
                case Keys.C: chip8.Keypad[0xB] = false; break;
                case Keys.V: chip8.Keypad[0xF] = false; break;
            }
        }

        private void pauseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isPaused)
            {
                emulationTimer.Start();
                isPaused = false;
            }
            else
            {
                emulationTimer.Stop();
                isPaused = true;
            }
        }

        private void xSpeedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isFastSpeed)
            {
                emulationTimer.Interval = 16;
                isFastSpeed = false;
            }
            else
            {
                emulationTimer.Interval = 8;
                isFastSpeed = true;
            }
        }
    }
}
