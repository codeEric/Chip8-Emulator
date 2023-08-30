
namespace Chip8
{
    public class Chip8
    {
        private ushort[] memory;
        private CPU _cpu;
        public CPU Cpu { get { return _cpu; } }

        private readonly byte[] Font = {
                        0xF0, 0x90, 0x90, 0x90, 0xF0, // 0
                        0x20, 0x60, 0x20, 0x20, 0x70, // 1
                        0xF0, 0x10, 0xF0, 0x80, 0xF0, // 2
                        0xF0, 0x10, 0xF0, 0x10, 0xF0, // 3
                        0x90, 0x90, 0xF0, 0x10, 0x10, // 4
                        0xF0, 0x80, 0xF0, 0x10, 0xF0, // 5
                        0xF0, 0x80, 0xF0, 0x90, 0xF0, // 6
                        0xF0, 0x10, 0x20, 0x40, 0x40, // 7
                        0xF0, 0x90, 0xF0, 0x90, 0xF0, // 8
                        0xF0, 0x90, 0xF0, 0x10, 0xF0, // 9
                        0xF0, 0x90, 0xF0, 0x90, 0x90, // A
                        0xE0, 0x90, 0xE0, 0x90, 0xE0, // B
                        0xF0, 0x80, 0x80, 0x80, 0xF0, // C
                        0xE0, 0x90, 0x90, 0x90, 0xE0, // D
                        0xF0, 0x80, 0xF0, 0x80, 0xF0, // E
                        0xF0, 0x80, 0xF0, 0x80, 0x80  // F
                        };

        public void Initialize()
        {
            memory = new ushort[4096];
            _cpu = new CPU();
            LoadFont();
            LoadRom(memory);
            _cpu.Fetch(memory);
            _cpu.DecodeAndExecute(memory);
        }

        public void Run()
        {
            _cpu.Fetch(memory);
            _cpu.DecodeAndExecute(memory);
        }

        private void LoadFont()
        {
            for (int i = 0; i < Font.Length; i++)
            {
                memory[i] = Font[i];
            }
        }

        public void LoadRom(ushort[] memory)
        {
            string path = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName + @"\Chip8\Roms\IBM_Logo.ch8";
            byte[] romData = File.ReadAllBytes(path);
            for (int i = 0; i < romData.Length; i++)
            {
                memory[0x200 + i] = romData[i];
            }
        }

        public byte[] getGraphics()
        {
            return _cpu.Graphics;
        }
    }
}