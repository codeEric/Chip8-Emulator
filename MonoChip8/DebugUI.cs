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

    internal class DebugUI
    {

        private Desktop _desktop;

        public Desktop Desktop { get { return _desktop; } }
        public bool Show { get; set; }

        public event EventHandler<RomEventArgs> StopButtonClicked;
        protected virtual void OnStopButtonClicked(RomEventArgs e)
        {
            StopButtonClicked?.Invoke(this, e);
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

            var pcLabel = new Label();
            pcLabel.Id = "pcLabel";
            pcLabel.Text = $"PC: 0000";
            pcLabel.GridRow = 0;
            infoGrid.Widgets.Add(pcLabel);

            var iLabel = new Label();
            iLabel.Id = "iRegister";
            iLabel.Text = "I: 0000";
            iLabel.GridRow = 0;
            iLabel.GridColumn = 1;
            infoGrid.Widgets.Add(iLabel);

            for (int i = 0; i < 16; i++)
            {
                var tempLabel = new Label();
                tempLabel.Id = $"v{i} Register";
                tempLabel.Text = $"V[0x{Convert.ToString(i, 16).ToUpper()}]: 0000";
                tempLabel.GridRow = (i % 8) + 1;
                tempLabel.GridColumn = i / 8;
                infoGrid.Widgets.Add(tempLabel);
            }

            var bottomGrid = new Grid()
            {
                ColumnSpacing = 2,
                RowSpacing = 2,
            };

            grid.ColumnsProportions.Add(new Proportion());
            grid.ColumnsProportions.Add(new Proportion());
            grid.RowsProportions.Add(new Proportion());
            grid.RowsProportions.Add(new Proportion());

            bottomGrid.GridColumn = 0;
            bottomGrid.GridRow = 1;
            grid.Widgets.Add(bottomGrid);

            var combo = new ComboBox();
            combo.Items.Add(new ListItem("Breakout"));
            combo.Items.Add(new ListItem("IBM Logo"));
            combo.Items.Add(new ListItem("Random Number Test"));
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
