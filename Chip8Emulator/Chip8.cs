using System;
using System.Collections.Generic;
using System.Threading;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

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
        public ushort pressedKey = 255;

        //Sound
        private WaveOutEvent waveOut;
        private SignalGenerator signalGenerator;

        public void Initialize()
        {
            pc = 0x200;
            I = 0;
            delayTimer = 0;
            soundTimer = 0;
            InitializeSound();

            //Clear memory, resgisters, stack and display
            Array.Clear(memory, 0, memory.Length);
            Array.Clear(V, 0, V.Length);
            stack.Clear();
            Display = new bool[64, 32];
        }

        public void InitializeSound()
        {
            signalGenerator = new SignalGenerator()
            {
                Gain = 0.5,
                Frequency = 440,
                Type = SignalGeneratorType.Sin
            };

            waveOut = new WaveOutEvent();
            waveOut.Init(signalGenerator);
        }

        public void PlayBeep()
        {
            waveOut.Play();
        }

        public void StopBeep()
        {
            waveOut.Stop();
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
                            IncreasePC(2);
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
                        IncreasePC(4);
                    else
                        IncreasePC(2);
                    break;

                case 0x4000: // Skip next instruction if VX != NN
                    if (V[x] != nn)
                        IncreasePC(4);
                    else
                        IncreasePC(2);
                    break;

                case 0x5000: // Skip the next instruction iv VX == VY
                    if (V[x] == V[y])
                        IncreasePC(4);
                    else
                        IncreasePC(2);
                    break;

                case 0x6000: // Set VX to NN
                    V[x] = nn;
                    IncreasePC(2);
                    break;

                case 0x7000: // Add NN to VX
                    V[x] += nn;
                    IncreasePC(2);
                    break;

                case 0x8000: // 8xNN
                    switch (n)
                    {
                        case 0x0: // Set VX = VY
                            V[x] = V[y];
                            IncreasePC(2);
                            break;

                        case 0x1: // Set VX = VX OR VY
                            V[x] |= V[y];
                            IncreasePC(2);
                            break;

                        case 0x2: // Set VX = VX AND VY
                            V[x] &= V[y];
                            IncreasePC(2);
                            break;

                        case 0x3: // Set VX = VX XOR VY
                            V[x] ^= V[y];
                            IncreasePC(2);
                            break;

                        case 0x4: // Set VX = VX + VY, VF = Carry
                            ushort sum = (ushort)(V[x] + V[y]);
                            V[x] = (byte)(sum & 0xFF);
                            V[0xF] = (byte)((sum > 255) ? 1 : 0);
                            IncreasePC(2);
                            break;

                        case 0x5: // Set VX = VX - VY, VF = NOT Borrow
                            byte tempX = V[x];
                            byte tempY = V[y];
                            V[x] -= V[y];
                            V[0xF] = (byte)((tempX >= tempY) ? 1 : 0);
                            IncreasePC(2);
                            break;

                        case 0x6: // SHR VX {, VY}
                            tempX = V[x];
                            V[x] >>= 1;
                            V[0xF] = (byte)(tempX & 0x01);
                            IncreasePC(2);
                            break;

                        case 0x7: // SUBN VX, VY
                            V[x] = (byte)(V[y] - V[x]);
                            V[0xF] = (byte)((V[y] >= V[x]) ? 1 : 0);
                            IncreasePC(2);
                            break;

                        case 0xE: // SHL VX {, VY}
                            tempX = V[x];
                            V[x] <<= 1;
                            V[0xF] = (byte)((tempX & 0x80) >> 7);
                            IncreasePC(2);
                            break;

                        default:
                            throw new NotImplementedException($"Unknown 8xy_ opcode: 0x{opcode:X4}");
                    }
                    break;

                case 0x9000:
                    if (V[x] != V[y])
                        IncreasePC(4);
                    else
                        IncreasePC(2);
                    break;

                case 0xA000: // Set I to NNN
                    I = nnn;
                    IncreasePC(2);
                    break;

                case 0xB000:
                    pc = (ushort)(V[0] + nnn);
                    break;

                case 0xC000:
                    Random random = new Random();
                    byte randomByte = (byte)random.Next(0, 256);
                    V[x] = (byte)(randomByte & nn);
                    IncreasePC(2);
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

                        IncreasePC(2);
                    }
                    break;

                case 0xE000:
                    switch (nn)
                    {
                        case 0xA1: //Skip next instruction if key in VX is not pressed
                            if (!Keypad[V[x]])
                                IncreasePC(4);
                            else
                                IncreasePC(2);
                            break;

                        case 0x9E: // Skip next instruction if key in VX is pressed
                            if (Keypad[V[x]])
                                IncreasePC(4);
                            else
                                IncreasePC(2);
                            break;
                    }
                    break;

                case 0xF000: // FxNN opcodes
                    switch (nn)
                    {
                        case 0x07: // VX = delay timer
                            V[x] = delayTimer;
                            IncreasePC(2);
                            break;

                        case 0x0A: // Fx0A: Wait for a key release and store it in VX
                            for (int i = 0; i < Keypad.Length; i++)
                            {
                                // Check if the key is pressed
                                if (Keypad[i] && pressedKey == 255)
                                {
                                    pressedKey = (ushort)i;
                                    Console.WriteLine($"Key {i:X} pressed");
                                    pc -= 2;
                                    return;
                                }
                                else
                                {
                                    if (pressedKey != 255)
                                    {
                                        if (!Keypad[pressedKey]) //Key has been lifted
                                        {
                                            Console.WriteLine("Key lifted");
                                            V[x] = (byte)pressedKey;
                                            pressedKey = 255;
                                            IncreasePC(2);
                                            return;
                                        }
                                    }
                                }
                            }

                            // No key release detected; remain halted
                            Console.WriteLine("Waiting for key release...");
                            break;


                        case 0x15: // delay timer = VX
                            delayTimer = V[x];
                            IncreasePC(2);
                            break;

                        case 0x18: // sound timer = VX
                            soundTimer = V[x];
                            IncreasePC(2);
                            break;

                        case 0x1E: // Add VX to I
                            I += V[x];
                            if (I > 0xFFF) V[0xF] = 1; // Set VF if overflow occurs
                            IncreasePC(2);
                            break;

                        case 0x33: // Store BCD representation of VX in memory locations I, I+1, I+2
                            byte value = V[x];
                            memory[I] = (byte)(value / 100);
                            memory[I + 1] = (byte)((value / 10) % 10);
                            memory[I + 2] = (byte)(value % 10);
                            IncreasePC(2);
                            break;

                        case 0x55: //Store V0 through VX in memory starting at I
                            for (int i = 0; i <= x; i++)
                            {
                                memory[I + i] = V[i];
                            }
                            IncreasePC(2);
                            break;

                        case 0x65: // Read V0 through Vx from memory starting at I
                            for (int i = 0; i <= x; i++)
                            {
                                V[i] = memory[I + i];
                            }
                            IncreasePC(2);
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
            DecodeAndExecute(opcode);

            
        }

        public void SimulateTimers()
        {
            if (delayTimer > 0)
            {
                delayTimer--;
            }
            if (soundTimer > 0)
            {
                soundTimer--;
                
                if (soundTimer > 0)
                {
                    PlayBeep();
                } else
                {
                    StopBeep();
                }
            } else
            {
                StopBeep();
            }
        }

        private void IncreasePC(int amount)
        {
            pc += (ushort)amount;
        }
    }
}
