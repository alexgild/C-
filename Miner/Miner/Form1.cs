using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Miner
{
    public partial class Form1 : Form
    {
        private MenuStrip menuStrip1 = new MenuStrip();
        static private int fieldWidth = 10;
        static private int fieldHeight = 8;
        private readonly int distanceBetweenButtons = 25;
        private string mode = "";
        private int bombCount = 10;
        private int openCellsCount = 0;
        private bool firstClick = true;
        private CellButton[,] buttonsField;

        public Form1()
        {
            InitializeComponent();

            ToolStripMenuItem modeItem = new ToolStripMenuItem("Mode");
            ToolStripMenuItem easyModeItem = new ToolStripMenuItem("Easy");
            ToolStripMenuItem normalModeItem = new ToolStripMenuItem("Normal");
            ToolStripMenuItem difficultModeItem = new ToolStripMenuItem("Difficult");

            modeItem.DropDownItems.Add(easyModeItem);
            modeItem.DropDownItems.Add(normalModeItem);
            modeItem.DropDownItems.Add(difficultModeItem);

            menuStrip1.Items.Add(modeItem);
            Controls.Add(menuStrip1);

            easyModeItem.Click += new EventHandler(MenuItemClick);
            normalModeItem.Click += new EventHandler(MenuItemClick);
            difficultModeItem.Click += new EventHandler(MenuItemClick);

            Form1_Load(this, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            GenerateField();
        }

        /** Ganarate field with buttons */
        void GenerateField()
        {
            Random random = new Random();
            buttonsField = new CellButton[fieldWidth, fieldHeight];

            for (int i = 10; (i - 10) < fieldWidth * distanceBetweenButtons; i += distanceBetweenButtons)
            {
                for (int j = 30; (j - 30) < fieldHeight * distanceBetweenButtons; j += distanceBetweenButtons)
                {
                    CellButton button = new CellButton
                    {
                        Location = new Point(i, j),
                        Size = new Size(30, 30)
                    };

                    buttonsField[(i - 10) / distanceBetweenButtons, (j - 30) / distanceBetweenButtons] = button;
                    button.XCoord1 = (i - 10) / distanceBetweenButtons;
                    button.YCoord1 = (j - 30) / distanceBetweenButtons;
                    Controls.Add(button);
                    button.MouseClick += new MouseEventHandler(FieldClick);
                }
            }

            AddBombs(bombCount);
        }

        /** Handle click on menu item */
        void MenuItemClick(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
            mode = menuItem.Text;

            GameSetup();
        }

        /** Setup game parameters */
        void GameSetup()
        {
            switch (mode)
            {
                case "Easy":
                    {
                        fieldWidth = 10;
                        fieldHeight = 8;
                        bombCount = 10;
                        this.ClientSize = new System.Drawing.Size(280, 240);
                        break;
                    }
                case "Normal":
                    {
                        fieldWidth = 18;
                        fieldHeight = 14;
                        bombCount = 40;
                        this.ClientSize = new System.Drawing.Size(480, 390);
                        break;
                    }
                case "Difficult":
                    {
                        fieldWidth = 24;
                        fieldHeight = 20;
                        bombCount = 99;
                        this.ClientSize = new System.Drawing.Size(630, 540);
                        break;
                    }
            }

            Controls.Clear();
            Controls.Add(menuStrip1);

            openCellsCount = 0;
            GenerateField();
            this.CenterToScreen();

            firstClick = true;
        }

        /** Add bombs to the field in random order */
        void AddBombs(int bombsToAdd)
        {
            for (int i = 0; i < bombsToAdd; i++)
            {
                Random random = new Random();
                int x = random.Next(0, fieldWidth);
                int y = random.Next(0, fieldHeight);

                while (buttonsField[x, y].HasBomb)
                {
                    x = random.Next(0, fieldWidth);
                    y = random.Next(0, fieldHeight);
                }

                buttonsField[x, y].HasBomb = true;
            }
        }

        /** Handle click on the field */
        void FieldClick(object sender, MouseEventArgs e)
        {
            CellButton button = (CellButton)sender;

            switch (e.Button)
            {
                case (MouseButtons.Left):
                    {
                        if (button.IsActivated || button.Text == "X")
                        {
                            break;
                        }

                        if (button.HasBomb)
                        {
                            if (firstClick)
                            {
                                button.HasBomb = false;

                                // add new bomb
                                AddBombs(1);
                                EmptyCellClick(button);
                            }
                            else
                            {
                                BombExplode(button);
                            }
                        }
                        else
                        {
                            EmptyCellClick(button);
                            if (button.NeighboursCount == 0)
                            {
                                OpenEmptyCells(button);
                            }
                        }

                        firstClick = false;
                        button.IsActivated = true;

                        break;
                    }
                case MouseButtons.Right:
                    {
                        if (button.IsActivated)
                        {
                            break;
                        }

                        if (button.BackColor != Color.LightPink)
                        {
                            button.BackColor = Color.LightPink;
                            button.Text = "X";
                        }
                        else
                        {
                            button.BackColor = Control.DefaultBackColor;
                            button.UseVisualStyleBackColor = true;
                            button.Text = "";
                        }

                        break;
                    }
            }
        }

        /** Explode all bombs on the field */
        void BombExplode(CellButton button)
        {
            button.Text = "*";

            for (int i = 0; i < fieldWidth; i++)
            {
                for (int j = 0; j < fieldHeight; j++)
                {
                    if (buttonsField[i, j].HasBomb)
                    {
                        buttonsField[i, j].Text = "*";
                    }

                    buttonsField[i, j].Enabled = false;
                }
            }

            if (DialogResult.OK == MessageBox.Show("You loose!"))
            {
                GameSetup();
            }
        }

        /** Handle click on cell without a bomb */
        void EmptyCellClick(CellButton button)
        {
            button.BackColor = Color.DarkGray;
            CountNeighbours(button);

            if (button.NeighboursCount != 0)
            {
                button.Text = button.NeighboursCount.ToString();
            }

            if (!button.IsActivated)
            {
                openCellsCount++;
            }

            button.IsActivated = true;

            if (openCellsCount == fieldHeight * fieldWidth - bombCount)
            {
                for (int i = 0; i < fieldWidth; i++)
                {
                    for (int j = 0; j < fieldHeight; j++)
                    {
                        if (buttonsField[i, j].HasBomb)
                        {
                            buttonsField[i, j].BackColor = Color.LightGreen;
                        }
                    }
                }
                
                if (DialogResult.OK == MessageBox.Show("Congratulations! You win!"))
                {
                    Application.Exit();
                }
            }
        }

        /** Count neighbours around a cell */
        int CountNeighbours(CellButton button)
        { 
            int neighbours = 0;

            for (int i = button.XCoord1 - 1; i <= button.XCoord1 + 1; i++)
            {
                for (int j = button.YCoord1 - 1; j <= button.YCoord1 + 1; j++)
                {
                    if (i >= 0 && i < fieldWidth && j >= 0 && j < fieldHeight)
                    {
                        if (buttonsField[i, j].HasBomb)
                        {
                            neighbours++;
                        }
                    }
                }
            }

            button.NeighboursCount = neighbours;
            return button.NeighboursCount;
        }

        /** Open a cell area with cells wihout numbers */
        void OpenEmptyCells(CellButton button)
        {
            EmptyCellClick(button);

            for (int i = button.XCoord1 - 1; i <= button.XCoord1 + 1; i++)
            {
                for (int j = button.YCoord1 - 1; j <= button.YCoord1 + 1; j++)
                {
                    if (i >= 0 && i < fieldWidth && j >= 0 && j < fieldHeight)
                    {
                        if (!buttonsField[i, j].IsActivated &&
                            CountNeighbours(buttonsField[i, j]) == 0)
                        {
                            OpenEmptyCells(buttonsField[i, j]);
                        }
                        else if (!buttonsField[i, j].HasBomb)
                        {
                            EmptyCellClick(buttonsField[i, j]);
                        }
                    }
                }
            }
        }
    }

    class CellButton : Button
    {
        private bool hasBomb;
        private int neighboursCount = 0;
        private int XCoord;
        private int YCoord;
        private bool isActivated = false;
        public bool HasBomb { get => hasBomb; set => hasBomb = value; }
        public int NeighboursCount { get => neighboursCount; set => neighboursCount = value; }
        public int XCoord1 { get => XCoord; set => XCoord = value; }
        public int YCoord1 { get => YCoord; set => YCoord = value; }
        public bool IsActivated { get => isActivated; set => isActivated = value; }
    }
}
