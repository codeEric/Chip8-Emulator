using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8
{

    public readonly struct Opcode
    {
        public ushort Data { get; }
        public byte Type { get; }
        public byte X { get; }
        public byte Y { get; }
        public byte N { get; }
        public byte NN { get; }
        public ushort NNN { get; }

        public Opcode(ushort data)
        {
            Data = data;
            Type = (byte)(data >> 12);
            X = (byte)((data & 0x0F00) >> 8);
            Y = (byte)((data & 0x00F0) >> 4);
            N = (byte)(data & 0x000F);
            NN = (byte)(data & 0x00FF);
            NNN = (ushort)(data & 0x0FFF);
        }
    }

    public class CPU
    {
        private const ushort StartingLocation = 0x200;
        private byte[] _graphics;
        public byte[] Graphics { get { return _graphics; }}
        private ushort pc;
        private ushort I;
        private byte[] V;
        private ushort[] stack;
        private Opcode opcode;
        private byte delayTimer;
        private byte soundTimer;
        
        public CPU()
        {
            _graphics = new byte[2048]; // Display 64x32 pixels
            ClearDisplay();
            pc = StartingLocation;
            V = new byte[16];
            stack = new ushort[16];
        }

        private void ClearDisplay()
        {
            Array.Clear(_graphics, 0, _graphics.Length);
        }

        private void Draw(ushort[] memory, byte X, byte Y, byte N)
        {
            byte x = (byte)(V[X] & 0x3F);
            byte y = (byte)(V[Y] & 0x1F);
            byte spriteRowData;
            bool spritePixel;
            int idx;
            V[0xF] = 0;

            for(int spriteRow = 0; spriteRow < N; spriteRow++)
            {
                spriteRowData = (byte)(memory[I + spriteRow]);
                for(int spriteBit = 0; spriteBit < 8; spriteBit++)
                {
                    spritePixel = (spriteRowData & (1 << 7 - spriteBit)) != 0;
                    idx = (x + spriteBit) + ((y + spriteRow) * 64);
                    if(!spritePixel && _graphics[idx] == 1)
                    {
                        _graphics[idx] = 0;
                        V[0xF] = 1;
                    }
                    else if(spritePixel && _graphics[idx] == 0)
                    {
                        _graphics[idx] = 1;
                    }

                }

            }

        }

        public void Fetch(ushort[] memory)
        {
            opcode = new Opcode((ushort)(memory[pc] << 8 | memory[pc+1]));
            ushort op = (ushort)(memory[pc] << 8 | memory[pc + 1]);
            pc += 2;
        }

        public void DecodeAndExecute(ushort[] memory)
        {
            switch(opcode.Type)
            {
                case 0x0 when opcode.N == 0x0:
                    ClearDisplay();
                    break;

                case 0x1:
                    pc = opcode.NNN;
                    break;
                case 0x6:
                    V[opcode.X] = opcode.NN;
                    break;
                case 0x7:
                    V[opcode.X] += opcode.NN;
                    break;
                case 0xA:
                    I = opcode.NNN;
                    break;
                case 0xD:
                    Draw(memory, opcode.X, opcode.Y, opcode.N);
                    break;
                default:
                    Console.Error.WriteLine($"Unrecognized opcode: [{Convert.ToString(opcode.Data, 16).PadLeft(4, '0').ToUpper()}]");
                    break;
            }
        }
    }
}
