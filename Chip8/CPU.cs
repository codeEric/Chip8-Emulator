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
        private const ushort StartingLocation = 0x200; // Program starting memory location
        private byte[] _graphics;                      // Display 64x32 pixels
        public byte[] Graphics { get { return _graphics; }}
        private ushort _pc;
        public ushort Pc { get { return _pc; } }       // Program counter
        private ushort I;                              // Index register
        private byte[] _v;                             // Variable registers numbered 0 through F hexadecimal
        public byte[] V { get { return _v; }}
        private ushort[] stack;                        // A stack which is used to call subroutines
        private byte sp;
        private Opcode _opcode;                        // Operation code
        public Opcode Opcode { get { return _opcode; }}
        private byte[] _keypad;                        // Hexadecimal keypad, it has 16 keys labelled 0 through F
        /* 
        Original keypad    =>       Modern keypad
        1   2	3	C      =>       1	2	3	4
        4	5	6	D      =>       Q	W	E	R
        7	8	9	E      =>       A	S	D	F
        A	0	B	F      =>       Z	X	C	V
        */
        public byte[] Keypad { set { _keypad = value; } get { return _keypad; } }
        private byte _delayTimer;                      // Delay timer which is decremented at a rate of 60 Hz until it reaches 0
        public byte DelayTimer { set { _delayTimer = value; } get { return _delayTimer; } }
        private byte _soundTimer;                      // Sound timer hich functions like the delay timer, but which also gives off a beeping sound as long as it’s not 0
        public byte SoundTimer { set { _soundTimer = value; } get { return _soundTimer; } }

        public CPU()
        {
            _graphics = new byte[2048];
            ClearDisplay();
            _pc = StartingLocation;
            _v = new byte[16];
            sp = 0;
            stack = new ushort[16];
            _keypad = new byte[16];
            _delayTimer = 0;
            _delayTimer = 0;
        }

        #region Helpers
        private void DebugMessage<T>(T message)
        {

            System.Diagnostics.Debug.WriteLine(message?.ToString());
        }

        private string ConvertToHex(ushort data)
        {
            return Convert.ToString(data, 16).PadLeft(4, '0').ToUpper();
        }
        #endregion
        private void ClearDisplay()
        {
            Array.Clear(_graphics, 0, _graphics.Length);
        }

        private void Draw(ushort[] memory, byte X, byte Y, byte N)
        {
            byte x = (byte)(_v[X] & 0x3F);
            byte y = (byte)(_v[Y] & 0x1F);
            ushort spriteRowData;
            int idx;
            _v[0xF] = 0;

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
                            _v[0xF] = 1;
                        _graphics[idx] ^= 1;
                    }

                }

            }
        }

        public void Fetch(ushort[] memory)
        {
            _opcode = new Opcode((ushort)(memory[_pc] << 8 | memory[_pc+1]));
            _pc += 2;
        }

        public void DecodeAndExecute(ushort[] memory)
        {
            switch(_opcode.Type)
            {
                case 0x0 when _opcode.N == 0x0:   // Opcode - 00E0 - Clears the screen 
                    ClearDisplay();
                    break;
                case 0x0 when _opcode.N == 0xE:   // Opcode - 00EE - Returns from subroutine
                    _pc = stack[--sp];
                    break;
                case 0x1:                         // Opcode - 1NNN - Jumps to address NNN
                    _pc = _opcode.NNN;
                    break;
                case 0x2:                         // Opcode - 2NNN - Calls subroutine at NNN
                    stack[sp++] = _pc;
                    _pc = _opcode.NNN;
                    break;
                case 0x3:                         // Opcode - 3XNN - Skips the next instruction if VX equals NN
                    if (_v[_opcode.X] == _opcode.NN) 
                        _pc += 2;
                    break;
                case 0x4:                         // Opcode - 4XNN - Skips the next instruction if VX does not equal NN
                    if (_v[_opcode.X] != _opcode.NN)
                        _pc += 2; 
                    break;
                case 0x5:                         // Opcode - 5XY0 - Skips the next instruction if VX equals VY
                    if (_v[_opcode.X] == _v[_opcode.Y])
                        _pc += 2;
                    break;
                case 0x6:                         // Opcode - 6XNN - Sets VX to NN
                    _v[_opcode.X] = _opcode.NN;
                    break;
                case 0x7:                         // Opcode - 7XNN - Adds NN to VX
                    _v[_opcode.X] += _opcode.NN;
                    break;
                case 0x8 when _opcode.N == 0x0:   // Opcode - 8XY0 - Sets VX to the value of VY.
                    _v[_opcode.X] = _v[_opcode.Y];
                    break;
                case 0x8 when _opcode.N == 0x1:   // Opcode - 8XY1 - Sets VX to VX or VY (bitwise OR operation)
                    _v[_opcode.X] |= _v[_opcode.Y];
                    break;
                case 0x8 when _opcode.N == 0x2:   // Opcode - 8XY2 - Sets VX to VX and VY (bitwise AND operation)
                    _v[_opcode.X] &= _v[_opcode.Y];
                    break;
                case 0x8 when _opcode.N == 0x3:   // Opcode - 8XY3 - Sets VX to VX xor VY
                    _v[_opcode.X] ^= _v[_opcode.Y];
                    break;
                case 0x8 when _opcode.N == 0x4:   // Opcode - 8XY4 - Adds VY to VX. VF is set to 1 when there's a carry, and to 0 when there is not
                    _v[_opcode.X] += _v[_opcode.Y];
                    if (0xFF - _v[_opcode.X] < _v[_opcode.Y])
                        _v[0xF] = 1;
                    else
                        _v[0xF] = 0;
                    break;
                case 0x8 when _opcode.N == 0x5:   // Opcode - 8XY5 - VY is subtracted from VX. VF is set to 0 when there's a borrow, and 1 when there is not
                    if (_v[_opcode.Y] > _v[_opcode.X])
                        _v[0xF] = 0;
                    else
                        _v[0xF] = 1;
                    _v[_opcode.X] -= _v[_opcode.Y];
                    break;
                case 0x8 when _opcode.N == 0x6:   // Opcode - 8XY6 - Stores the least significant bit of VX in VF and then shifts VX to the right by 1
                    _v[0xF] = (byte)(_v[_opcode.X] & 0x1);
                    _v[_opcode.X] >>= 1;
                    break;
                case 0x8 when _opcode.N == 0x7:   // Opcode - 8XY7 - Sets VX to VY minus VX. VF is set to 0 when there's a borrow, and 1 when there is not
                    if (_v[_opcode.Y] < _v[_opcode.X])
                        _v[0xF] = 0;
                    else
                        _v[0xF] = 1;
                    _v[_opcode.X] = (byte)(_v[_opcode.Y] - _v[_opcode.X]);
                    break;
                case 0x8 when _opcode.N == 0xE:   // Opcode - 8XYE - Stores the most significant bit of VX in VF and then shifts VX to the left by 1
                    _v[0xF] = (byte)((_v[_opcode.X] & 0x80) >> 7);
                    _v[_opcode.X] <<= 0x1;
                    break;
                case 0x9:                         // Opcode - 9XY0 - Skips the next instruction if VX does not equal VY
                    if (_v[_opcode.X] != _v[_opcode.Y])
                        _pc += 2;
                    break;
                case 0xA:                         // Opcode - ANNN - Sets I to the address NNN
                    I = _opcode.NNN;
                    break;
                case 0xB:                         // Opcode - BNNN - Jumps to the address NNN plus V0 
                    _pc = (ushort)(_v[0x0] + _opcode.NNN);
                    break;
                case 0xC:                         // Opcode - CXNN - Sets VX to the result of a bitwise and operation on a random number (0 to 255) and NN
                    Random rand = new Random();
                    _v[_opcode.X] = (byte)(rand.Next(0, 256) & _opcode.NN);
                    break;
                case 0xD:                         // Opcode - DXYN - Draws a sprite at coordinate (VX, VY) that has a width of 8 pixels and a height of N pixels.
                    Draw(memory, _opcode.X, _opcode.Y, _opcode.N);
                    break;
                case 0xE when _opcode.NN == 0x9E: // Opcode - EX9E - Skips the next instruction if the key stored in VX is pressed
                    if (_keypad[_v[_opcode.X]] == 1)
                        _pc += 2;
                    break;
                case 0xE when _opcode.NN == 0xA1: // Opcode - EXA1 - Skips the next instruction if the key stored in VX is not pressed
                    if(_keypad[_v[_opcode.X]] == 0)
                        _pc += 2;
                    break;
                case 0xF when _opcode.NN == 0x07: // Opcode - FX07 - Sets VX to the value of the delay timer
                    _v[_opcode.X] = _delayTimer;
                    break;
                case 0xF when _opcode.NN == 0x0A: // Opcode - FX0A - A key press is awaited, and then stored in VX (blocking operation, all instruction halted until next key event)
                    var activeKeypad = Array.FindIndex(_keypad, keypad => keypad == 1);
                    if (activeKeypad >= 0)
                    {
                        _v[_opcode.X] = (byte)activeKeypad;
                    }
                    else
                    {
                        _pc -= 2;
                    }
                    break;
                case 0xF when _opcode.NN == 0x15: // Opcode - FX15 - Sets the delay timer to VX
                    _delayTimer = _v[_opcode.X];
                    break;
                case 0xF when _opcode.NN == 0x18: // Opcode - FX18 - Sets the sound timer to VX
                    _soundTimer = _v[_opcode.X];
                    break;
                case 0xF when _opcode.NN == 0x1E: // Opcode - FX1E - Adds VX to I
                    I += _v[_opcode.X];
                    break;
                case 0xF when _opcode.NN == 0x29: // Opcode - FX29 - Sets I to the location of the sprite for the character in VX
                    I = (ushort)(Chip8.CharacterSize * _v[_opcode.X]);
                    break;
                case 0xF when _opcode.NN == 0x33: // Opcode - FX33 - Stores the binary-coded decimal representation of VX,
                                                  // with the hundreds digit in memory at location in I,
                                                  // the tens digit at location I+1,
                                                  // and the ones digit at location I+2
                    memory[I]     = (ushort)(_v[_opcode.X] / 100);
                    memory[I + 1] = (ushort)(_v[_opcode.X] / 10 % 10);
                    memory[I + 2] = (ushort)(_v[_opcode.X] % 10);
                    break;
                case 0xF when _opcode.NN == 0x55: // Opcode - FX55 - Stores from V0 to VX (including VX) in memory, starting at address I
                    for(int i = 0; i <= _opcode.X; i++)
                    {
                        memory[I + i] = _v[i];
                    }
                    break;
                case 0xF when _opcode.NN == 0x65: // Opcode - FX65 - Fills from V0 to VX (including VX) with values from memory, starting at address I
                    for (int i = 0; i <= _opcode.X; i++)
                    {
                        _v[i] = (byte)(memory[I + i]);
                    }
                    break;
                default:
                    DebugMessage($"Unrecognized opcode: [{ConvertToHex(_opcode.Data)}]");
                    break;
            }
        }
    }
}
