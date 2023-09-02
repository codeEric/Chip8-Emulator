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
        Chip8.Chip8 chip8;
        byte[] _pixels;

        private Color[] _gfx;
        Texture2D _canvas;
        private Rectangle _scaleSize;

        private DebugUI debugUi;
        bool pKeyPressed = false;

        public MonoChip8()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            chip8 = new Chip8.Chip8();
            debugUi = new DebugUI();
            debugUi.StopButtonClicked += OnStopButtonClicked;
            debugUi.StateButtonClicked += OnStateButtonClicked;
        }

        protected override void Initialize()
        {
            chip8.Initialize("IBM Logo");
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
            debugUi.Load(this);
            debugUi.Init();

        }

        protected override void Update(GameTime gameTime)
        {
            switch (chip8.State)
            {
                case Chip8.Chip8State.Running:
                    KeyboardState state = Keyboard.GetState();
                    #region Inputs
                    if (state.IsKeyDown(Keys.P) && !pKeyPressed)
                    {
                        pKeyPressed = true;
                        debugUi.Show = !debugUi.Show;
                    }
                    if (state.IsKeyUp(Keys.P))
                    {
                        pKeyPressed = false;
                    }
                    if (state.IsKeyDown(Keys.D1))
                    {
                        chip8.Cpu.Keypad[0x0] = 1;
                    }
                    if (state.IsKeyDown(Keys.D2))
                    {
                        chip8.Cpu.Keypad[0x1] = 1;
                    }
                    if (state.IsKeyDown(Keys.D3))
                    {
                        chip8.Cpu.Keypad[0x2] = 1;
                    }
                    if (state.IsKeyDown(Keys.D4))
                    {
                        chip8.Cpu.Keypad[0x3] = 1;
                    }
                    if (state.IsKeyDown(Keys.Q))
                    {
                        chip8.Cpu.Keypad[0x4] = 1;
                    }
                    if (state.IsKeyDown(Keys.W))
                    {
                        chip8.Cpu.Keypad[0x5] = 1;
                    }
                    if (state.IsKeyDown(Keys.E))
                    {
                        chip8.Cpu.Keypad[0x6] = 1;
                    }
                    if (state.IsKeyDown(Keys.R))
                    {
                        chip8.Cpu.Keypad[0x7] = 1;
                    }
                    if (state.IsKeyDown(Keys.A))
                    {
                        chip8.Cpu.Keypad[0x8] = 1;
                    }
                    if (state.IsKeyDown(Keys.S))
                    {
                        chip8.Cpu.Keypad[0x9] = 1;
                    }
                    if (state.IsKeyDown(Keys.D))
                    {
                        chip8.Cpu.Keypad[0xA] = 1;
                    }
                    if (state.IsKeyDown(Keys.F))
                    {
                        chip8.Cpu.Keypad[0xB] = 1;
                    }
                    if (state.IsKeyDown(Keys.Z))
                    {
                        chip8.Cpu.Keypad[0xC] = 1;
                    }
                    if (state.IsKeyDown(Keys.X))
                    {
                        chip8.Cpu.Keypad[0xD] = 1;
                    }
                    if (state.IsKeyDown(Keys.C))
                    {
                        chip8.Cpu.Keypad[0xE] = 1;
                    }
                    if (state.IsKeyDown(Keys.V))
                    {
                        chip8.Cpu.Keypad[0xF] = 1;
                    }
                    #endregion

                    for (int j = 0; j < 8; j++)
                    {
                        chip8.Run();
                        debugUi.Update(chip8.Cpu);
                    }
                    _pixels = chip8.Cpu.Graphics;
                    for (int i = 0; i < _pixels.Length; i++)
                    {
                        _gfx[i] = _pixels[i] == 1 ? Color.White : Color.Black;
                    }

                    if (chip8.Cpu.DelayTimer > 0)
                        chip8.Cpu.DelayTimer--;

                    if (chip8.Cpu.SoundTimer > 0)
                    {
                        if (chip8.Cpu.SoundTimer == 1)
                        {
                            Console.Beep();
                        }
                        chip8.Cpu.SoundTimer--;
                    }
                    Array.Clear(chip8.Cpu.Keypad);
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
            _spriteBatch.Draw(_canvas, new Rectangle(0, 0, 64*12, 32*12), Color.White);
            _spriteBatch.End();

            if(debugUi.Show)
                debugUi.Render();

            base.Draw(gameTime);
        }

        public void OnStopButtonClicked(object sender, RomEventArgs args)
        {
            chip8.Initialize(args.Rom);
        }

        public void OnStateButtonClicked(object sender, StateEventArgs args)
        {
            chip8.State = args.State;
        }
    }
}