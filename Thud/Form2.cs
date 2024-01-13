using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Thud
{

    public partial class Form2 : Form
    {
        

        Rozhodci game;
        Deska board;
        public Form2(bool troll_t, bool dwarf_t)
        {
            InitializeComponent();
            this.Height = 710;
            Width = 850;
            label1.Text = "Loading..";
            board = new Deska(600, 600, 40);
            board.Updating += Refresh;
            Troll troll_g = new Troll(Field.Troll, 4);
            Dwarf dwarf_g = new Dwarf(Field.Dwarf, 1);
            Hrac troll = new Human(troll_g, dwarf_g);
            Hrac dwarf = new Human(dwarf_g, troll_g);
            if (!troll_t) troll = new Computer(troll_g, dwarf_g, 3);
            if (!dwarf_t) dwarf = new Computer(dwarf_g, troll_g, 3);
            this.game = new Rozhodci(board, troll, dwarf);
            this.FormClosing+=game.Close_Log;
            pictureBox1.Size = new Size(610, 610);
            pictureBox1.Top = 40;
            pictureBox1.Left = 20;
            button1.Left = 640;
            button1.Top = 40;
            button1.Size= new Size(120,600);
            button1.Text = "Skončit hru?";
            //genrevání podkladu

            pictureBox1.Image = board.deska;
            label1.Text = "Loaded";

            

        }
        
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            
            Hrac current = game.Get_Current();

            PictureBox sending = (PictureBox) sender;
            
            if (current is Human)
            {
                
                Human human;
                human = (Human) current;
                List<Pole> highlight;
                board.Update();
                int x = this.PointToClient(Cursor.Position).X; 
                x -= sending.Left;
                int y = this.PointToClient(Cursor.Position).Y - sending.Top;
                Pole select = board.Pos2Tile(x, y);
                if ((select.x >= 15) || (select.y >= 15) || (select.x < 0) || (select.y < 0)) return;
                if (human.Selecting(select))
                {
                    
                    
                    //human.Select(select);
                    highlight =human.Select(board.Pos2Tile(x, y));
                   
                    while (highlight.Any())
                    {
                        
                        Pole cur = highlight.First();
                        
                        board.High_Light(cur,Color.Blue,Color.DarkBlue);
                        pictureBox1.Image = board.deska;
                        highlight.RemoveAt(0);
                    }
                }
                else //if (!human.done)
                {
                    //human.done = true;
                    human.Move(select);
                    human.End_Tah();
                }
            }         
            Refresh();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hrac current = game.Get_Current();

            if (current is Human)
            {
                Human human;
                human = (Human)current;
                human.Surrender();
                human.End_Tah();
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {
           
        }

        private void Form2_Shown(object sender, EventArgs e)
        {
            Refresh();
            game.Start_Game();
            Refresh();
        }

        private void Form2_Validated(object sender, EventArgs e)
        {
            Refresh();
            game.Start_Game();
            Refresh();
        }
    }
}
