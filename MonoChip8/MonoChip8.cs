using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


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

        public MonoChip8()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            chip8 = new Chip8.Chip8();
        }

        protected override void Initialize()
        {
            chip8.Initialize();
            _canvas = new Texture2D(GraphicsDevice, 64, 32, false, SurfaceFormat.Color);
            _gfx = new Color[2048];
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.PreferredBackBufferHeight = 512;
            _graphics.ApplyChanges();
            _scaleSize = GraphicsDevice.PresentationParameters.Bounds;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            chip8.Run();
            _pixels = chip8.Cpu.Graphics;
            for(int i = 0; i < _pixels.Length; i++)
            {
                _gfx[i] = _pixels[i] == 1 ? Color.White: Color.Black;
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.Textures[0] = null;

            _canvas.SetData(_gfx, 0, 2048);

            _spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointWrap);
            _spriteBatch.Draw(_canvas, new Rectangle(0, 0, _scaleSize.Width, _scaleSize.Height), Color.White);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}