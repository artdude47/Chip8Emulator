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

        public Form1()
        {
            InitializeComponent();
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
                    MessageBox.Show("ROM loaded successfully!", "Success", MessageBoxButtons.OK);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to load ROM: {ex.Message}", "Error", MessageBoxButtons.OK);
                }
            }
        }

        private void closeROMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Close ROM clicked!");
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chip8.Initialize();
        }
    }
}
