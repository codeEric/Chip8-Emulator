using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;

namespace MonoChip8
{
    public class MonoChip8 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Chip8.Chip8 _chip8;
        private byte[] _pixels;

        private Color[] _gfx;
        private Texture2D _canvas;
        private const int PixelSize = 12;
        private const int DisplayWidth = 64;
        private const int DisplayHeight = 32;

        private DebugUI _debugUi;
        private bool _pKeyPressed;

        public MonoChip8()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            _chip8 = new Chip8.Chip8();
            _debugUi = new DebugUI();
            _debugUi.StopButtonClicked += OnStopButtonClicked;
            _debugUi.StateButtonClicked += OnStateButtonClicked;
            _pKeyPressed = false;
        }

        protected override void Initialize()
        {
            _chip8.Initialize("IBM Logo");
            _canvas = new Texture2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color);
            _gfx = new Color[2048];
            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 640;
            _graphics.ApplyChanges();

            IsMouseVisible = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _debugUi.Load(this);
            _debugUi.Init();

        }

        protected override void Update(GameTime gameTime)
        {
            switch (_chip8.State)
            {
                case Chip8.Chip8State.Running:
                    KeyboardState state = Keyboard.GetState();
                    #region Inputs
                    if (state.IsKeyDown(Keys.P) && !_pKeyPressed)
                    {
                        _pKeyPressed = true;
                        _debugUi.Show = !_debugUi.Show;
                    }
                    if (state.IsKeyUp(Keys.P))
                    {
                        _pKeyPressed = false;
                    }
                    if (state.IsKeyDown(Keys.D1))
                    {
                        _chip8.Cpu.Keypad[0x0] = 1;
                    }
                    if (state.IsKeyDown(Keys.D2))
                    {
                        _chip8.Cpu.Keypad[0x1] = 1;
                    }
                    if (state.IsKeyDown(Keys.D3))
                    {
                        _chip8.Cpu.Keypad[0x2] = 1;
                    }
                    if (state.IsKeyDown(Keys.D4))
                    {
                        _chip8.Cpu.Keypad[0x3] = 1;
                    }
                    if (state.IsKeyDown(Keys.Q))
                    {
                        _chip8.Cpu.Keypad[0x4] = 1;
                    }
                    if (state.IsKeyDown(Keys.W))
                    {
                        _chip8.Cpu.Keypad[0x5] = 1;
                    }
                    if (state.IsKeyDown(Keys.E))
                    {
                        _chip8.Cpu.Keypad[0x6] = 1;
                    }
                    if (state.IsKeyDown(Keys.R))
                    {
                        _chip8.Cpu.Keypad[0x7] = 1;
                    }
                    if (state.IsKeyDown(Keys.A))
                    {
                        _chip8.Cpu.Keypad[0x8] = 1;
                    }
                    if (state.IsKeyDown(Keys.S))
                    {
                        _chip8.Cpu.Keypad[0x9] = 1;
                    }
                    if (state.IsKeyDown(Keys.D))
                    {
                        _chip8.Cpu.Keypad[0xA] = 1;
                    }
                    if (state.IsKeyDown(Keys.F))
                    {
                        _chip8.Cpu.Keypad[0xB] = 1;
                    }
                    if (state.IsKeyDown(Keys.Z))
                    {
                        _chip8.Cpu.Keypad[0xC] = 1;
                    }
                    if (state.IsKeyDown(Keys.X))
                    {
                        _chip8.Cpu.Keypad[0xD] = 1;
                    }
                    if (state.IsKeyDown(Keys.C))
                    {
                        _chip8.Cpu.Keypad[0xE] = 1;
                    }
                    if (state.IsKeyDown(Keys.V))
                    {
                        _chip8.Cpu.Keypad[0xF] = 1;
                    }
                    #endregion

                    for (int j = 0; j < 8; j++)
                    {
                        _chip8.CompleteCycle();
                        _debugUi.Update(_chip8.Cpu, _chip8.Memory);
                    }
                    _pixels = _chip8.Cpu.Graphics;
                    for (int i = 0; i < _pixels.Length; i++)
                    {
                        _gfx[i] = _pixels[i] == 1 ? Color.White : Color.Black;
                    }

                    if (_chip8.Cpu.DelayTimer > 0)
                        _chip8.Cpu.DelayTimer--;

                    if (_chip8.Cpu.SoundTimer > 0)
                    {
                        if (_chip8.Cpu.SoundTimer == 1)
                        {
                            Console.Beep();
                        }
                        _chip8.Cpu.SoundTimer--;
                    }
                    Array.Clear(_chip8.Cpu.Keypad);
                    break;
                case Chip8.Chip8State.Paused:
                    break;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.Textures[0] = null;

            _canvas.SetData(_gfx, 0, 2048);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap);
            _spriteBatch.Draw(_canvas, new Rectangle(0, 0, DisplayWidth * PixelSize, DisplayHeight * PixelSize), Color.White);
            _spriteBatch.End();

            if(_debugUi.Show)
                _debugUi.Render();

            base.Draw(gameTime);
        }

        public void OnStopButtonClicked(object sender, RomEventArgs args)
        {
            _chip8.Initialize(args.Rom);
        }

        public void OnStateButtonClicked(object sender, StateEventArgs args)
        {
            _chip8.State = args.State;
        }
    }
}