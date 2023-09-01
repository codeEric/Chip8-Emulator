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
        private byte sp;
        private Opcode opcode;
        private byte[] _keypad;
        public byte[] Keypad { set { _keypad = value; } get { return _keypad; } }
        private byte _delayTimer;
        public byte DelayTimer { set { _delayTimer = value; } get { return _delayTimer; } }
        private byte _soundTimer;
        public byte SoundTimer { set { _soundTimer = value; } get { return _soundTimer; } }

        public CPU()
        {
            _graphics = new byte[2048]; // Display 64x32 pixels
            ClearDisplay();
            pc = StartingLocation;
            V = new byte[16];
            sp = 0;
            stack = new ushort[16];
            _keypad = new byte[16];
            _delayTimer = 0;
            _delayTimer = 0;
        }

        private void DebugMessage<T>(T message)
        {

            System.Diagnostics.Debug.WriteLine(message?.ToString());
        }

        private string ConvertToHex(ushort data)
        {
            return Convert.ToString(data, 16).PadLeft(4, '0').ToUpper();
        }

        private void ClearDisplay()
        {
            Array.Clear(_graphics, 0, _graphics.Length);
        }

        private void Draw(ushort[] memory, byte X, byte Y, byte N)
        {
            byte x = (byte)(V[X] & 0x3F);
            byte y = (byte)(V[Y] & 0x1F);
            ushort spriteRowData;
            int idx;
            V[0xF] = 0;

            for (int spriteRow = 0; spriteRow < N; spriteRow++)
            {
                spriteRowData = (ushort)(memory[I + spriteRow]);
                for (int spriteBit = 0; spriteBit < 8; spriteBit++)
                {
                    idx = (x + spriteBit) + ((y + spriteRow) * 64);
                    if (idx > 2047) break;
                    if ((spriteRowData & (1 << 7 - spriteBit)) != 0)
                    {
                        if(_graphics[idx] == 1)
                            V[0xF] = 1;
                        _graphics[idx] ^= 1;
                    }

                }

            }
        }

        public void Fetch(ushort[] memory)
        {
            opcode = new Opcode((ushort)(memory[pc] << 8 | memory[pc+1]));
            ushort op = (ushort)(memory[pc] << 8 | memory[pc + 1]);
            //DebugMessage($"Current Opcode: {ConvertToHex(opcode.Data)}");
            pc += 2;
        }

        public void DecodeAndExecute(ushort[] memory)
        {
            switch(opcode.Type)
            {
                case 0x0 when opcode.N == 0x0:
                    ClearDisplay();
                    break;
                case 0x0 when opcode.N == 0xE:
                    pc = stack[--sp];
                    break;
                case 0x1:
                    pc = opcode.NNN;
                    break;
                case 0x2:
                    stack[sp++] = pc;
                    pc = opcode.NNN;
                    break;
                case 0x3:
                    if (V[opcode.X] == opcode.NN) 
                        pc += 2;
                    break;
                case 0x4:
                    if (V[opcode.X] != opcode.NN)
                        pc += 2; 
                    break;
                case 0x5:
                    if (V[opcode.X] == V[opcode.Y])
                        pc += 2;
                    break;
                case 0x6:
                    V[opcode.X] = opcode.NN;
                    break;
                case 0x7:
                    V[opcode.X] += opcode.NN;
                    break;
                case 0x8 when opcode.N == 0x0:
                    V[opcode.X] = V[opcode.Y];
                    break;
                case 0x8 when opcode.N == 0x1:
                    V[opcode.X] |= V[opcode.Y];
                    break;
                case 0x8 when opcode.N == 0x2:
                    V[opcode.X] &= V[opcode.Y];
                    break;
                case 0x8 when opcode.N == 0x3:
                    V[opcode.X] ^= V[opcode.Y];
                    break;
                case 0x8 when opcode.N == 0x4:
                    V[opcode.X] += V[opcode.Y];
                    if (0xFF - V[opcode.X] < V[opcode.Y])
                        V[0xF] = 1;
                    else
                        V[0xF] = 0;
                    break;
                case 0x8 when opcode.N == 0x5:
                    if (V[opcode.Y] > V[opcode.X])
                        V[0xF] = 0;
                    else
                        V[0xF] = 1;
                    V[opcode.X] -= V[opcode.Y];
                    break;
                case 0x8 when opcode.N == 0x6:
                    V[0xF] = (byte)(V[opcode.X] & 0x1);
                    V[opcode.X] >>= 1;
                    break;
                case 0x8 when opcode.N == 0x7:
                    if (V[opcode.Y] < V[opcode.X])
                        V[0xF] = 0;
                    else
                        V[0xF] = 1;
                    V[opcode.X] = (byte)(V[opcode.Y] - V[opcode.X]);
                    break;
                case 0x8 when opcode.N == 0xE:
                    V[0xF] = (byte)((V[opcode.X] & 0x80) >> 7);
                    V[opcode.X] <<= 0x1;
                    break;
                case 0x9:
                    if (V[opcode.X] != V[opcode.Y])
                        pc += 2;
                    break;
                case 0xA:
                    I = opcode.NNN;
                    break;
                case 0xB:
                    pc = (ushort)(V[0x0] + opcode.NNN);
                    break;
                case 0xC:
                    Random rand = new Random();
                    V[opcode.X] = (byte)(rand.Next(0, 256) & opcode.NN);
                    break;
                case 0xD:
                    Draw(memory, opcode.X, opcode.Y, opcode.N);
                    break;
                case 0xE when opcode.NN == 0x9E:
                    if (_keypad[V[opcode.X]] == 1)
                        pc += 2;
                    break;
                case 0xE when opcode.NN == 0xA1:
                    if(_keypad[V[opcode.X]] == 0)
                        pc += 2;
                    break;
                case 0xF when opcode.NN == 0x07:
                    V[opcode.X] = _delayTimer;
                    break;
                case 0xF when opcode.NN == 0x0A:
                    var activeKeypad = Array.FindIndex(_keypad, keypad => keypad == 1);
                    if (activeKeypad >= 0)
                    {
                        V[opcode.X] = (byte)activeKeypad;
                    }
                    else
                    {
                        pc -= 2;
                    }
                    break;
                case 0xF when opcode.NN == 0x15:
                    _delayTimer = V[opcode.X];
                    break;
                case 0xF when opcode.NN == 0x18:
                    _soundTimer = V[opcode.X];
                    break;
                case 0xF when opcode.NN == 0x1E:
                    I += V[opcode.X];
                    break;
                case 0xF when opcode.NN == 0x29:
                    I = (ushort)(0x5 * V[opcode.X]);
                    break;
                case 0xF when opcode.NN == 0x33:
                    memory[I] = (ushort)(V[opcode.X] / 100);
                    memory[I + 1] = (ushort)(V[opcode.X] / 10 % 10);
                    memory[I + 2] = (ushort)(V[opcode.X] % 10);
                    break;
                case 0xF when opcode.NN == 0x55:
                    for(int i = 0; i <= opcode.X; i++)
                    {
                        memory[I + i] = V[i];
                    }
                    break;
                case 0xF when opcode.NN == 0x65:
                    for (int i = 0; i <= opcode.X; i++)
                    {
                        V[i] = (byte)(memory[I + i]);
                    }
                    break;
                default:
                    DebugMessage($"Unrecognized opcode: [{ConvertToHex(opcode.Data)}]");
                    break;
            }
        }
    }
}
