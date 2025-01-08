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

        public ushort FetchOpcode()
        {
            // Combine two bites for 16-bit opcode
            return (ushort)((memory[pc] << 8) | memory[pc + 1]);
        }

        public void DecodeAndExecute(ushort opcode)
        {
            // Extract specific info from opcode for decoding
            ushort nnn = (ushort)(opcode & 0x0FFF); // Lowest 12 bits
            byte nn = (byte)(opcode & 0x00FF);      // Lowest 8 bits
            byte n = (byte)(opcode & 0x000F);       // Lowest 4 bits
            byte x = (byte)((opcode & 0x0F00) >> 8); // 2nd nibble (register X)
            byte y = (byte)((opcode & 0x00F0) >> 4); // 3rd nibble (register Y)

            // Decode and execute the opcode
            switch (opcode & 0xF000)
            {
                case 0x0000:
                    switch (opcode)
                    {
                        case 0x00E0: // Clear the display
                            Array.Clear(Display, 0, Display.Length);
                            pc += 2; // Move to next instruction
                            break;

                        case 0x00EE: // Return from subroutine
                            if (stack.Count == 0)
                                throw new InvalidOperationException("Stack underflow during 00EE.");
                            pc = stack.Pop(); // Restore PC from stack
                            break;

                        default:
                            throw new NotImplementedException($"Unknown 0x00 opcode: 0x{opcode:X4}");
                    }
                    break;

                case 0x1000: // Jump to address NNN
                    pc = nnn;
                    break;

                case 0x2000: // Call subroutine at NNN
                    stack.Push((ushort)(pc + 2)); // Save the address of the next instruction
                    pc = nnn;           // Jump to subroutine
                    break;

                case 0x3000: // Skip next instruction if VX == NN
                    if (V[x] == nn)
                        pc += 4; // Skip next instruction
                    else
                        pc += 2; // Move to next instruction
                    break;

                case 0x4000: // Skip next instruction if VX != NN
                    if (V[x] != nn)
                        pc += 4; // Skip next instruction
                    else
                        pc += 2; // Move to next instruction
                    break;

                case 0x6000: // Set VX to NN
                    V[x] = nn;
                    pc += 2;
                    break;

                case 0x7000: // Add NN to VX
                    V[x] += nn;
                    pc += 2;
                    break;

                case 0xA000: // Set I to NNN
                    I = nnn;
                    pc += 2;
                    break;

                case 0xD000: // Draw a sprite at (VX, VY) with height N
                    {
                        byte posX = V[x];
                        byte posY = V[y];
                        V[0xF] = 0; // Reset collision flag

                        for (int row = 0; row < n; row++)
                        {
                            byte spriteByte = memory[I + row];
                            for (int col = 0; col < 8; col++)
                            {
                                byte spritePixel = (byte)((spriteByte >> (7 - col)) & 0x1);
                                int displayX = (posX + col) % 64; // Wrap horizontally
                                int displayY = (posY + row) % 32; // Wrap vertically

                                if (spritePixel == 1 && Display[displayX, displayY])
                                {
                                    V[0xF] = 1; // Set VF if a pixel is erased
                                }

                                Display[displayX, displayY] ^= spritePixel == 1;
                            }
                        }

                        pc += 2; // Move to the next instruction
                    }
                    break;

                case 0xF000: // FxNN opcodes
                    switch (nn)
                    {
                        case 0x1E: // Add VX to I
                            I += V[x];
                            if (I > 0xFFF) V[0xF] = 1; // Set VF if overflow occurs
                            pc += 2;
                            break;

                        default:
                            throw new NotImplementedException($"Unknown FxNN opcode: 0x{opcode:X4}");
                    }
                    break;

                default:
                    throw new NotImplementedException($"Unknown opcode: 0x{opcode:X4}");
            }
        }


        public void EmulateCycle()
        {
            ushort opcode = FetchOpcode();
            Console.WriteLine($"PC: {pc:X4}, Opcode: {opcode:X4}, I: {I:X4}");
            DecodeAndExecute(opcode);

            Console.WriteLine($"PC after execution: {pc:X4}");


            if (delayTimer > 0) delayTimer--;
            if (soundTimer > 0) soundTimer--;

        }

    }
}
