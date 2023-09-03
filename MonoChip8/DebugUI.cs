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

            var mainGrid = new Grid
            {
                ColumnSpacing = 3,
                RowSpacing = 3,
            };
            mainGrid.ColumnsProportions.Add(new Proportion
            {
                Type = ProportionType.Pixels,
                Value = 768,
            });
            mainGrid.ColumnsProportions.Add(new Proportion());
            mainGrid.RowsProportions.Add(new Proportion
            {
                Type = ProportionType.Pixels,
                Value = 384,
            });
            mainGrid.RowsProportions.Add(new Proportion());

            var panel = new Panel();
            panel.Width = 768;
            panel.Height = 384;
            panel.GridColumn = 0;
            panel.GridRow = 0;
            mainGrid.Widgets.Add(panel);

            var memoryGrid = new Grid
            {
                RowSpacing = 15,
                ColumnSpacing = 2,
                Margin = new Myra.Graphics2D.Thickness(20, 0, 0, 0)
            };
            for (int i = 0; i < 15; i++)
            {
                memoryGrid.ColumnsProportions.Add(new Proportion());
            }

            for (int i = 0; i < 15; i++)
            {
                var tempLabel = new Label();
                tempLabel.Id = $"memory {i}";
                tempLabel.Text = $"[0x000{Convert.ToString(i, 16).ToUpper()}] = 0000";
                if(i % 7 == 0 && i != 0 && i != 14)
                    tempLabel.TextColor = Microsoft.Xna.Framework.Color.Green;
                tempLabel.GridRow = i;
                tempLabel.GridColumn = 0;
                memoryGrid.Widgets.Add(tempLabel);
            }

            memoryGrid.GridColumn = 1;
            memoryGrid.GridRow = 0;
            mainGrid.Widgets.Add(memoryGrid);

            var registerGrid = new Grid()
            {
                RowSpacing = 4,
                ColumnSpacing = 5,
            };
            registerGrid.GridColumn = 1;
            registerGrid.GridRow = 1;
            mainGrid.Widgets.Add(registerGrid);
            

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
            iLabel.Padding = new Myra.Graphics2D.Thickness(0, 8, 0, 0);
            registerGrid.Widgets.Add(iLabel);

            var opcodeLabel = new Label();
            opcodeLabel.Id = "opcode";
            opcodeLabel.Text = "Op: 0000";
            opcodeLabel.GridRow = 2;
            opcodeLabel.GridColumn = 0;
            opcodeLabel.Padding = new Myra.Graphics2D.Thickness(0, 8, 0, 0);
            registerGrid.Widgets.Add(opcodeLabel);

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

            var bottomLeftGrid = new Grid()
            {
                ColumnSpacing = 2,
                RowSpacing = 3,
            };

            bottomLeftGrid.ColumnsProportions.Add(new Proportion
            {
                Type = ProportionType.Pixels,
                Value = 220,
            });
            bottomLeftGrid.ColumnsProportions.Add(new Proportion());
            bottomLeftGrid.RowsProportions.Add(new Proportion());
            bottomLeftGrid.RowsProportions.Add(new Proportion());
            bottomLeftGrid.RowsProportions.Add(new Proportion());

            bottomLeftGrid.GridColumn = 0;
            bottomLeftGrid.GridRow = 1;
            mainGrid.Widgets.Add(bottomLeftGrid);

            var combo = new ComboBox();
            combo.Items.Add(new ListItem("IBM Logo"));
            combo.Items.Add(new ListItem("Breakout"));
            combo.Items.Add(new ListItem("Random Number Test"));
            combo.Items.Add(new ListItem("Space Invaders"));
            combo.SelectedItem = combo.Items[0];
            combo.GridColumn = 0;
            combo.GridRow = 0;
            bottomLeftGrid.Widgets.Add(combo);

            var loadRomButton = new TextButton();
            loadRomButton.Text = "Load ROM";
            loadRomButton.GridColumn = 1;
            loadRomButton.GridRow = 0;
            bottomLeftGrid.Widgets.Add(loadRomButton);

            loadRomButton.Click += (s, a) =>
            {
                OnStopButtonClicked(new RomEventArgs(combo.SelectedItem.Text));
            };

            var startButton = new TextButton();
            startButton.Text = "Start";
            startButton.GridColumn = 0;
            startButton.GridRow = 1;
            startButton.Margin = new Myra.Graphics2D.Thickness(0, 10, 0, 0);
            bottomLeftGrid.Widgets.Add(startButton);

            startButton.Click += (s, a) =>
            {
                OnStateButtonClicked(new StateEventArgs(Chip8.Chip8State.Running));
            };

            var pauseButton = new TextButton();
            pauseButton.Text = "Pause";
            pauseButton.GridColumn = 1;
            pauseButton.GridRow = 1;
            pauseButton.Margin = new Myra.Graphics2D.Thickness(0, 10, 0, 0);
            bottomLeftGrid.Widgets.Add(pauseButton);

            pauseButton.Click += (s, a) =>
            {
                OnStateButtonClicked(new StateEventArgs(Chip8.Chip8State.Paused));
            };

            _desktop = new Desktop();
            _desktop.Root = mainGrid;
        }

        public void Update(Chip8.CPU cpu, ushort[] memory)
        {
            for (int i = 0; i < 15; i++)
            {
                if (i < 7)
                {
                    ((Label)_desktop.GetWidgetByID($"memory {i}")).Text = $"[0x{Convert.ToString(cpu.Pc - (7 - i), 16).PadLeft(4, '0').ToUpper()}] = 0x{Convert.ToString(memory[cpu.Pc - (7 - i)], 16).PadLeft(4, '0').ToUpper()}";
                }
                else if (i == 7)
                {
                    ((Label)_desktop.GetWidgetByID($"memory {i}")).Text = $"[0x{Convert.ToString(cpu.Pc, 16).PadLeft(4, '0').ToUpper()}] = 0x{Convert.ToString(memory[cpu.Pc - (7 - i)], 16).PadLeft(4, '0').ToUpper()}";
                }
                else
                {
                    ((Label)_desktop.GetWidgetByID($"memory {i}")).Text = $"[0x{Convert.ToString(cpu.Pc + (i - 7), 16).PadLeft(4, '0').ToUpper()}] = 0x{Convert.ToString(memory[cpu.Pc - (7 - i)], 16).PadLeft(4, '0').ToUpper()}";
                }
            }

            ((Label)_desktop.GetWidgetByID("pcLabel")).Text = $"PC: {cpu.Pc}";

            for (int i = 0; i < 16; i++)
            {
                ((Label)_desktop.GetWidgetByID($"v{i} Register")).Text = $"V[0x{Convert.ToString(i, 16).ToUpper()}]: {cpu.V[i]}";
            }

            ((Label)_desktop.GetWidgetByID("opcode")).Text = $"Op: {Convert.ToString(cpu.Opcode.Data, 16).PadLeft(4, '0').ToUpper()}";

        }

        public void Render()
        {
            _desktop.Render();
        }

    }
}
