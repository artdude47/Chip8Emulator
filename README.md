#CHIP-8 Emulator
A simple CHIP-8 emulator written in C# using WinForms, intended to act as a portfolio project

## Descritpion

The CHIP-8 Emulator allows you to load and play classic CHIP-8 ROMs. It's a lightweight interpreter that replicates the functionality of the original CHIP-8 virtual machine, often used in retro games from the 1970s.

This project demonstrates my skills in:
- Low-level system emulation
- C# programming and WinForms UI development
- Real-time input handling
- Debugging and problem-solving

  ## Features

- Emulates the full CHIP-8 instruction set.
- Support for keyboard input mapped to the CHIP-8 keypad.
- Dynamic rendering of the 64x32 display.
- Built-in tone generation for the CHIP-8 sound timer.
- Menu options to load ROMs, reset, and control emulation speed.
- Pause and resume emulation.
- Adjustable speed with a 2x toggle.

## Getting Started

### Prerequisites

- Windows OS
- Visual Studio 2019 or later
- .NET Framework 4.7.2 or later

### Installation

1. Clone the repository: git clone https://github.com/artdude47/Chip8Emulator.git
2. Open the project in Visual Studio
3. Build and run the solution

## Running the Emulator
1. Launch the emulator
2. Load a CHIP-8 ROM by selecting File > Load ROM from the menu. (Find some ROMs here: https://github.com/Timendus/chip8-test-suite)
3. Use the keyboard to interact with the ROM

Keyboard Controls

CHIP-8 Key	Keyboard Key
1	           1
2	           2
3            3
C	           4
4	           Q
5	           W
6            E
D	           R
7	           A
8	           S
9	           D
E	           F
A	           Z
0	           X
B	           C
F	           V
