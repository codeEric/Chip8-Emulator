using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Myra;
using Myra.Graphics2D.UI;

namespace MonoChip8
{

    public class RomEventArgs : EventArgs
    {
        private readonly string rom;
        public string Rom
        {
            get { return this.rom; }
        }

        public RomEventArgs(string rom)
        {
            this.rom = rom;
        }
    }

    public class StateEventArgs : EventArgs
    {
        private readonly Chip8.Chip8State state;
        public Chip8.Chip8State State
        {
            get { return this.state; }
        }

        public StateEventArgs(Chip8.Chip8State state)
        {
            this.state = state;
        }
    }

    internal class DebugUI
    {

        private Desktop _desktop;

        public Desktop Desktop { get { return _desktop; } }
        public bool Show { get; set; }

        public event EventHandler<RomEventArgs> StopButtonClicked;
        public event EventHandler<StateEventArgs> StateButtonClicked;
        protected virtual void OnStopButtonClicked(RomEventArgs e)
        {
            StopButtonClicked?.Invoke(this, e);
        }

        protected virtual void OnStateButtonClicked(StateEventArgs e)
        {
            StateButtonClicked?.Invoke(this, e);
        }

        public void Load(MonoChip8 monoChip8)
        {
            MyraEnvironment.Game = monoChip8;
            _desktop = new Desktop();
            Show = false;
        }

        public void Init()
        {

            var grid = new Grid
            {
                ColumnSpacing = 3,
                RowSpacing = 3,
            };
            grid.ColumnsProportions.Add(new Proportion
            {
                Type = ProportionType.Pixels,
                Value = 768,
            });
            grid.ColumnsProportions.Add(new Proportion());
            grid.RowsProportions.Add(new Proportion
            {
                Type = ProportionType.Pixels,
                Value = 384,
            });
            grid.RowsProportions.Add(new Proportion());

            var panel = new Panel();
            panel.Width = 768;
            panel.Height = 384;
            panel.GridColumn = 0;
            panel.GridRow = 0;
            grid.Widgets.Add(panel);

            var infoGrid = new Grid
            {
                RowSpacing = 18,
                ColumnSpacing = 2,
                Margin = new Myra.Graphics2D.Thickness(20, 0, 0, 0)
            };
            infoGrid.ColumnsProportions.Add(new Proportion
            {
                Type = ProportionType.Pixels,
                Value = 90,
            });
            for (int i = 0; i < 17; i++)
            {
                infoGrid.ColumnsProportions.Add(new Proportion());
            }

            infoGrid.GridColumn = 1;
            infoGrid.GridRow = 0;
            grid.Widgets.Add(infoGrid);

            var registerGrid = new Grid()
            {
                RowSpacing = 4,
                ColumnSpacing = 5,
            };
            registerGrid.GridColumn = 1;
            registerGrid.GridRow = 1;
            grid.Widgets.Add(registerGrid);
            

            var pcLabel = new Label();
            pcLabel.Id = "pcLabel";
            pcLabel.Text = $"PC: 0000";
            pcLabel.GridRow = 0;
            pcLabel.GridColumn = 0;
            pcLabel.Padding = new Myra.Graphics2D.Thickness(0, 8, 0, 0);
            registerGrid.Widgets.Add(pcLabel);

            var iLabel = new Label();
            iLabel.Id = "iRegister";
            iLabel.Text = "I: 0000";
            iLabel.GridRow = 1;
            iLabel.GridColumn = 0;
            pcLabel.Padding = new Myra.Graphics2D.Thickness(0, 8, 0, 0);
            registerGrid.Widgets.Add(iLabel);


            for (int i = 0; i < 16; i++)
            {
                var tempLabel = new Label();
                tempLabel.Id = $"v{i} Register";
                tempLabel.Text = $"V[0x{Convert.ToString(i, 16).ToUpper()}]: 0000";
                tempLabel.GridRow = (i % 4);
                tempLabel.GridColumn = i / 4 + 1;
                tempLabel.Padding = new Myra.Graphics2D.Thickness(8);
                registerGrid.Widgets.Add(tempLabel);
            }

            var bottomGrid = new Grid()
            {
                ColumnSpacing = 2,
                RowSpacing = 3,
            };

            bottomGrid.ColumnsProportions.Add(new Proportion
            {
                Type = ProportionType.Pixels,
                Value = 220,
            });
            bottomGrid.ColumnsProportions.Add(new Proportion());
            bottomGrid.RowsProportions.Add(new Proportion());
            bottomGrid.RowsProportions.Add(new Proportion());
            bottomGrid.RowsProportions.Add(new Proportion());

            bottomGrid.GridColumn = 0;
            bottomGrid.GridRow = 1;
            grid.Widgets.Add(bottomGrid);

            var combo = new ComboBox();
            combo.Items.Add(new ListItem("IBM Logo"));
            combo.Items.Add(new ListItem("Breakout"));
            combo.Items.Add(new ListItem("Random Number Test"));
            combo.Items.Add(new ListItem("Space Invaders"));
            combo.SelectedItem = combo.Items[0];
            combo.GridColumn = 0;
            combo.GridRow = 0;
            bottomGrid.Widgets.Add(combo);

            var loadRom = new TextButton();
            loadRom.Text = "Load ROM";
            loadRom.GridColumn = 1;
            loadRom.GridRow = 0;
            bottomGrid.Widgets.Add(loadRom);

            loadRom.Click += (s, a) =>
            {
                OnStopButtonClicked(new RomEventArgs(combo.SelectedItem.Text));
            };

            var startButton = new TextButton();
            startButton.Text = "Start";
            startButton.GridColumn = 0;
            startButton.GridRow = 1;
            startButton.Margin = new Myra.Graphics2D.Thickness(0, 10, 0, 0);
            bottomGrid.Widgets.Add(startButton);

            startButton.Click += (s, a) =>
            {
                OnStateButtonClicked(new StateEventArgs(Chip8.Chip8State.Running));
            };

            var pauseButton = new TextButton();
            pauseButton.Text = "Pause";
            pauseButton.GridColumn = 1;
            pauseButton.GridRow = 1;
            pauseButton.Margin = new Myra.Graphics2D.Thickness(0, 10, 0, 0);
            bottomGrid.Widgets.Add(pauseButton);

            pauseButton.Click += (s, a) =>
            {
                OnStateButtonClicked(new StateEventArgs(Chip8.Chip8State.Paused));
            };

            _desktop = new Desktop();
            _desktop.Root = grid;
        }

        public void Update(Chip8.CPU cpu)
        {
            ((Label)_desktop.GetWidgetByID("pcLabel")).Text = $"PC: {cpu.Pc}";

            for (int i = 0; i < 16; i++)
            {
                ((Label)_desktop.GetWidgetByID($"v{i} Register")).Text = $"V[0x{Convert.ToString(i, 16).ToUpper()}]: {cpu.V[i]}";
            }

        }

        public void Render()
        {
            _desktop.Render();
        }

    }
}
