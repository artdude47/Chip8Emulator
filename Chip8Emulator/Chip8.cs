using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8Emulator
{
    class Chip8
    {
        //4KB of memory
        private byte[] memory = new byte[4096];

        //16 general purpose registers
        private byte[] V = new byte[16];

        //Index register and program counter
        private ushort I;
        private ushort pc;

        private Stack<ushort> stack = new Stack<ushort>();

        private byte delayTimer;
        private byte soundTimer;

        // Display is 64x32 pixels
        public bool[,] Display { get; private set; } = new bool[64, 32];

        // 16 key keypad
        public bool[] Keypad { get; private set; } = new bool[16];

        public void Initialize()
        {
            pc = 0x200;
            I = 0;
            delayTimer = 0;
            soundTimer = 0;

            //Clear memory, resgisters, stack and display
            Array.Clear(memory, 0, memory.Length);
            Array.Clear(V, 0, V.Length);
            stack.Clear();
            Display = new bool[64, 32];
        }

        public void LoadROM(byte[] rom)
        {
            if (rom.Length > (memory.Length - 0x200))
                throw new ArgumentException("ROM size exceeds available memory!");

            Array.Copy(rom, 0, memory, 0x200, rom.Length);
        }

    }
}
