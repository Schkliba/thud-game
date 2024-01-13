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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            bool troll=false;
            bool dwarf=false;
            if ((string)button1.Text == "Člověk") dwarf = true;
            if ((string)button2.Text == "Člověk") troll = true;
            Form deska = new Form2(troll,dwarf);
            deska.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string a;
            a=button1.Text;
            button1.Text = (string)button1.Tag;
            button1.Tag = a; 
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string a;
            a = button2.Text;
            button2.Text = (string)button2.Tag;
            button2.Tag = a;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    class TurnArgs : EventArgs
    {
        public Tah Turn { get; set; }
        public TurnArgs(Tah turn)
        {
            Turn = turn;
        }
    }
    class Pole
    {
        public int x;
        public int y;
        public Pole(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    class Tah
    {
        public Pole start;
        public Pole target;
        public bool surrender;
        public Tah(Pole start, Pole target, bool surrender)
        {
            this.start = start;
            this.target = target;
            this.surrender = surrender;
        }
        public Tah Oposite()
        {
            return new Tah(target,start,surrender);
        }
    }

    abstract class Figura // figura představuje soubor pravidel která platí pro daný typ figury. Obsahuje i konkrétní reprezentaci.
    {
        public Field shape;
        public int value;
        public List<Pole> sebrane;
        public Figura(Field shape, int value)
        {
            this.shape = shape;
            this.value = value;
        }

        abstract public List<Tah> Make_Que(Field[,] mapa, int x, int y, List<Tah> que = default(List<Tah>));
        abstract public Field[,] Move(Field[,] mapa, Tah turn);
        abstract public Field[,] MoveBack(Field[,] mapa, Tah turn, List<Pole> sebrane, Field enemy);

    }
    class Troll :Figura
    {
        int[,] shove;
        public Troll(Field shape, int value) : base(shape, value)
        {

        }
        override public Field[,] MoveBack(Field[,] mapa, Tah turn, List<Pole> sebrane, Field enemy)
        {
            mapa = Move(mapa, turn);
            while (this.sebrane.Any())
            {
                mapa[this.sebrane.First().x, this.sebrane.First().y] = enemy;
                this.sebrane.RemoveAt(0);
            }
            while (sebrane.Any())
            {
                mapa[sebrane.First().x, sebrane.First().y] = enemy;
                sebrane.RemoveAt(0);
            }
            return mapa;
        }
        override public Field[,] Move(Field[,] mapa, Tah turn)
        {
            Pole start = turn.start;
            Pole cil = turn.target;
            sebrane = new List<Pole>();
            if (mapa[start.x, start.y] == shape)
            {
                mapa[start.x, start.y] = Field.Free;
                mapa[cil.x, cil.y] = shape;
                for (int i = -1; i < 2; i++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        if ((i == 0) && (k == 0)) continue;
                        if ((cil.x + i >= 15) || (cil.y + k >= 15) || (cil.x + i < 0) || (cil.y + k < 0)) continue;
                        if ((mapa[cil.x+i, cil.y+k]!=shape)&&(mapa[cil.x+i, cil.y+k] != Field.NaN)){
                            if (mapa[cil.x + i, cil.y + k] != Field.Free) sebrane.Add(new Pole(cil.x + i, cil.y + k));
                            mapa[cil.x + i, cil.y + k] = Field.Free;
                        }
                    }
                } 
                
            }
            return mapa;
        }
        override public List<Tah> Make_Que(Field[,] mapa, int x, int y, List<Tah> que = default(List<Tah>))
        {
            if (que == null) que = new List<Tah>();
            if (mapa[x, y] == shape)
            {
                shove = count_shove(mapa, x, y);
                
                for (int i = -1; i < 2; i++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        int ix = x+i;
                        int iy = y+k;
                        int check = 0;
                        if ((i == 0) && (k == 0)) continue;
                        if ((ix >= 15) || (iy >= 15) || (ix < 0) || (iy < 0)) continue; //hlídání přetečení
                        while ((mapa[ix, iy] == Field.Free)&&(check<shove[1+i,1+k]))
                        {
                            bool trpaslik=false;
                            for (int d1 = -1; d1 < 2; d1++)
                            {
                                for (int d2 = -1; d2 < 2; d2++)
                                {
                                    if ((d1 == 0) && (d2 == 0)) continue;
                                    if ((ix+d1 >= 15) || (iy+d2 >= 15) || (ix+d1 < 0) || (iy+d2 < 0)) continue;
                                    if (mapa[ix + d1, iy + d2] == Field.Dwarf) trpaslik = true;
                                    
                                }
                            }
                            if ((trpaslik)||(check<1)) que.Add(new Tah(new Pole(x, y), new Pole(ix, iy), false));
                            ix += i;
                            iy += k;
                            if ((ix >= 15) || (iy >= 15) || (ix < 0) || (iy < 0)) break;
                            check++;
                        }
                    }
                }

            }
            return que;
        }

        int [,] count_shove(Field[,] mapa, int x, int y)
        {
            shove = new int[3, 3];
            
            for (int i = -1; i < 2; i++)
            {
                for (int k = -1; k < 2; k++)
                {
                    int ix = x;
                    int iy = y;
                    if ((i == 0) && (k == 0)) continue;
                    while (mapa[ix, iy] == shape)
                    {
                        
                        shove[1 + i, 1 + k] += 1;
                        ix -= i;
                        iy -= k;
                        if ((ix >= 15) || (iy >= 15) || (ix < 0) || (iy < 0)) break;
                    }
                }
            }
            return shove;
        }
    }
    class Dwarf: Figura
    {
        int[,] shove;
        public Dwarf(Field shape, int value) : base(shape, value)
        {

        }

        override public Field[,] MoveBack(Field[,] mapa, Tah turn, List<Pole> sebrane, Field enemy)
        {
            mapa = Move(mapa, turn);
            while (this.sebrane.Any())
            {
                mapa[this.sebrane.First().x, this.sebrane.First().y] = enemy;
                this.sebrane.RemoveAt(0);
            }
            while (sebrane.Any())
            {
                mapa[sebrane.First().x, sebrane.First().y] = enemy;
                sebrane.RemoveAt(0);
            }
            return mapa;
        }
        override public Field[,] Move(Field[,] mapa, Tah turn)
        {
            Pole start = turn.start;
            Pole cil = turn.target;
            sebrane = new List<Pole>();
            if (mapa[start.x, start.y] == shape)
            {
                mapa[start.x, start.y] = Field.Free;
                if ((mapa[cil.x, cil.y] != Field.Free) && (mapa[cil.x, cil.y] != Field.NaN)) sebrane.Add(new Pole(cil.x, cil.y));
                mapa[cil.x, cil.y] = shape;
                
            }
            return mapa;
        }
        override public List<Tah> Make_Que(Field[,] mapa, int x, int y, List<Tah> que = default(List<Tah>))
        {
            if (que == null) que = new List<Tah>();
            if (mapa[x, y] == shape)
            {
                
                shove = count_shove(mapa, x, y);
                
                for (int i = -1; i < 2; i++)
                {
                    for (int k = -1; k < 2; k++)
                    {
                        int ix = x+i;
                        int iy = y+k;
                        if ((i == 0) && (k == 0)) continue;
                        if ((ix >= 15) || (iy >= 15) || (ix < 0) || (iy < 0)) continue; //hlídání přetečení
                        
                        while ((mapa[ix, iy] == Field.Free)||(shove[1 + i, 1 + k] > 0)) 
                        {
                            //MessageBox.Show("Bender je bůh!");
                            if ((mapa[ix, iy] == shape) || (mapa[ix, iy] == Field.NaN)) break;
                            que.Add(new Tah(new Pole(x,y),new Pole(ix, iy),false));
                            //MessageBox.Show(ix.ToString()+" "+iy.ToString());
                            if (mapa[ix, iy] != Field.Free) break;
                            ix += i;
                            iy += k;
                            if ((ix >= 15) || (iy >= 15) || (ix < 0) || (iy < 0)) break;
                            shove[1 + i, 1 + k]--;
                        }
                    }
                }
            }
            return que;
        }
        

        int[,] count_shove(Field[,] mapa, int x, int y)
        {
            shove = new int[3, 3];

            for (int i = -1; i < 2; i++)
            {
                for (int k = -1; k < 2; k++)
                {
                    int ix = x;
                    int iy = y;
                    if ((i == 0) && (k == 0)) continue;
                    //if ((ix >= 15) || (iy >= 15) || (ix < 0) || (iy < 0)) continue;
                    while (mapa[ix, iy] == shape)
                    {
                        
                        shove[1 + i, 1 + k] += 1;
                        ix -= i;
                        iy -= k;
                        if ((ix >= 15) || (iy >= 15) || (ix < 0) || (iy < 0)) break;
                    }
                }
            }
            return shove;
        }
    }
    class Deska
    {
        Field[,] mapa;
        public Bitmap deska;
        Graphics painter;
        int tile_size;
        int width;
        int height;
        Color font_color = Color.Gray;
        Color backgroud = Color.DarkGreen;
        public event EventHandler Updating;
        public EventArgs e = null;
        public delegate void EventHandler();

        public Deska(int width, int height, int tile_size)
        {
            this.width = width;
            this.height = height;
            deska = new Bitmap(width, height);
            painter = Graphics.FromImage(deska);
            this.tile_size = tile_size;
        }
        public virtual void OnUpdate()
        {
            EventHandler handler = Updating;
            if (handler != null)
            {
                handler();
            }
        }
        public void ShowText(string text)
        {
            //Pen kontura = new Pen(Color.Gold, 10);
            FontFamily fontFamily = new FontFamily("Arial");
            Font font = new Font(
               fontFamily,
               45,
               FontStyle.Bold,
               GraphicsUnit.Pixel);
            Font font2 = new Font(
               fontFamily,
               46,
               FontStyle.Bold,
               GraphicsUnit.Pixel);
            Brush stetec;
            //stetec = new SolidBrush(backgroud);
            
            stetec = new SolidBrush(font_color);
            painter.DrawString(text, font, stetec, new PointF(width / 6, height / 3));




        }
        public void Update()
        {
            Pen kontura = new Pen(Color.Gold,10);
            Brush stetec = new SolidBrush(backgroud);
            painter.FillRectangle(stetec, 0, 0, width, height);
            painter.DrawRectangle(kontura,0,0,width,height);

            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    if (mapa[x, y] != Field.NaN)
                    {

                        
                        Color barva = Color.White;
                        if (((x % 2 == 0) && (y % 2 == 0)) || ((y % 2) * (x % 2) == 1)) barva = Color.Black;
                        stetec = new SolidBrush(barva);
                        painter.FillRectangle(stetec, x * tile_size, y * tile_size, tile_size, tile_size);
                        if (mapa[x, y] == Field.Troll)
                        {
                            kontura = new Pen(Color.White, 2);
                            stetec = new SolidBrush(Color.Black);
                            painter.FillEllipse(stetec, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                            painter.DrawEllipse(kontura, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                        }
                        else if (mapa[x, y] == Field.Dwarf)
                        {
                            kontura = new Pen(Color.Black, 2);
                            stetec = new SolidBrush(Color.White);
                            painter.FillEllipse(stetec, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                            painter.DrawEllipse(kontura, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                        }

                    }

                }
            }
        }
        public Pole Pos2Tile(int x, int y)
        {
            return new Pole((x/tile_size), (y/tile_size));
        }

        public Bitmap High_Light(Pole tile, Color color, Color dark)
        {
            Pen kontura;
            Color barva = color;
            if (((tile.x % 2 == 0) && (tile.y % 2 == 0)) || ((tile.y % 2) * (tile.x % 2) == 1)) barva = dark;
            Brush stetec = new SolidBrush(barva);
            painter.FillRectangle(stetec,tile.x*tile_size, tile.y*tile_size, tile_size, tile_size);
            if (mapa[tile.x, tile.y] == Field.Troll)
            {
                kontura = new Pen(Color.White, 2);
                stetec = new SolidBrush(Color.Black);
                painter.FillEllipse(stetec, tile.x * tile_size, tile.y * tile_size, tile_size - 2, tile_size - 2);
                painter.DrawEllipse(kontura, tile.x * tile_size, tile.y * tile_size, tile_size - 2, tile_size - 2);
            }
            else if (mapa[tile.x, tile.y] == Field.Dwarf)
            {
                kontura = new Pen(Color.Black, 2);
                stetec = new SolidBrush(Color.White);
                painter.FillEllipse(stetec, tile.x * tile_size, tile.y * tile_size, tile_size - 2, tile_size - 2);
                painter.DrawEllipse(kontura, tile.x * tile_size, tile.y * tile_size, tile_size - 2, tile_size - 2);
            }
            return deska;
        }

        public Bitmap Gen_Bitmp(Field[,] mapa)//generování plátna
        {
            this.mapa = mapa;
            Pen kontura = new Pen(Color.Gold, 10);
            Brush stetec = new SolidBrush(Color.DarkGreen);
            painter.FillRectangle(stetec, 0, 0, width, height);
            painter.DrawRectangle(kontura, 0, 0, width, height);
            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    if (mapa[x, y]!=Field.NaN)
                    {
                        
                        
                        Color barva = Color.White;
                        if (((x % 2 == 0)&&(y % 2==0))||((y % 2)*(x % 2)==1)) barva = Color.Black;
                        stetec = new SolidBrush(barva);
                        painter.FillRectangle(stetec,x*tile_size,y*tile_size, tile_size, tile_size);
                        if (mapa[x, y] == Field.Troll)
                        {
                            kontura = new Pen(Color.White,2);
                            stetec = new SolidBrush(Color.Black);
                            painter.FillEllipse(stetec, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                            painter.DrawEllipse(kontura, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                        }
                        else if (mapa[x, y] == Field.Dwarf)
                        {
                            kontura = new Pen(Color.Black,2);
                            stetec = new SolidBrush(Color.White);
                            painter.FillEllipse(stetec, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                            painter.DrawEllipse(kontura, x * tile_size, y * tile_size, tile_size - 2, tile_size - 2);
                        }

                    }
                    
                }
            }

            return deska;
        }

       
    }

    abstract class Hrac
    {
        protected Field[,] mapa; //odkaz na současnou podobu hry
        public Figura moje; //pravidla pro moje figurky
        public Figura jeho; //pravidla pro nepřátelské figurky
        public bool done; //značí že hráč je hotový s tahem a nebude jej nijak upravovat
        

        public event EventHandler<TurnArgs> EndTurn;
        public TurnArgs e = null;
        public delegate void EventHandler<TurnArgs>(object sender, TurnArgs e);

        public Hrac(Figura moje, Figura jeho)
        {

            this.moje = moje;
            this.jeho = jeho;
        }
        public void AttachMap(Field[,] mapa)
        {
            this.mapa = mapa;
        }
        protected virtual void On_EndTurn(object sender , TurnArgs e)
        {
            EventHandler<TurnArgs> handler = EndTurn;
            //Control.Invoke(new EventHandler<TurnArgs>(EndTurn),new object[]{ sender, e });
            
            handler.Invoke(sender, e);

        }
        
        abstract public void Tah(object sender, DoWorkEventArgs e);
        abstract public bool Surrender();
    }

    class Rozhodci
    {
        Hrac current;
        Hrac waiting;
        Deska deska;
        Field[,] layout;
        bool run;
        BackgroundWorker player_thread;//kontrolní prvek hráčského vlákna
        Tah turn; //vrácený tah
        System.IO.StreamWriter log; 
        public Rozhodci(Deska deska, Hrac troll, Hrac dwarf)
        {
            
            this.deska = deska;
            layout = Lay_Out();
            deska.Gen_Bitmp(layout);
            
            current = dwarf;
            current.EndTurn += this.Tah;
            current.AttachMap(layout);
            
            waiting = troll;
            waiting.EndTurn += this.Tah;
            waiting.AttachMap(layout);
            player_thread = new BackgroundWorker();
            player_thread.DoWork+=current.Tah;
            player_thread.RunWorkerCompleted+=TurnCross;

        }
        public void Start_Game()
        {
            run = true;
            if (current is Computer) deska.ShowText("Přemýšlím dlouho.\n Prosím čekejte...");
            deska.OnUpdate();
            //turn = new Tah(new Pole(0, 0), new Pole(0, 0), false);
            log = new System.IO.StreamWriter("log.txt");
            player_thread.RunWorkerAsync();
        }
        public void Close_Log(object sender, EventArgs e)
        {
            int cur_score = Count_Score(current);
            int waitscore = Count_Score(waiting);
            if (run)
            {
                if (cur_score == waitscore) MessageBox.Show("Remíza");
                else if (cur_score > waitscore) MessageBox.Show("Vyhrál " + current.moje.shape.ToString() + "\n se skóre: " + cur_score);
                else MessageBox.Show("Vyhrál " + waiting.moje.shape.ToString() + "\n se skóre: " + waitscore);
            }
            log.Close();
        }
        static string Make_Map(Field[,] map) //převedení mapy do tisknutelné podoby
        {
            string new_map = "";
            for (int y = 0; y < 15; y++)
            {
                for (int x = 0; x < 15; x++)
                {
                    if (map[x, y] == Field.Troll) new_map += "T";
                    if (map[x, y] == Field.Dwarf) new_map += "D";
                    if (map[x, y] == Field.Free) new_map += "_";
                    if (map[x, y] == Field.NaN) new_map += "X";
                }
                new_map += "\n";
            }

            new_map += "----------------------------\n\n";

            
            return new_map;
        }
        int Count_Score(Hrac hrac)
        {
            int score = 0;
            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    if (layout[x, y] == hrac.moje.shape) score += hrac.moje.value;
                }
            }
            return score;
        }
        void TurnCross(object sender,RunWorkerCompletedEventArgs e)
        {
            if ((run)&&(current.done))
            {
                run = false;
                string line = turn.start.x.ToString()+";"+turn.start.y.ToString()+"->"+ turn.target.x.ToString() + ";" + turn.target.y.ToString();
                if(current is Computer) log.WriteLine(((Computer)current).max_score.ToString()+"\n");
                log.WriteLineAsync(line);
                log.Flush();
                if (turn.surrender)
                {
                    if (waiting.Surrender())
                    {
                        deska.Update();
                        MessageBox.Show("Konec hry!");
                        int cur_score = Count_Score(current);
                        int waitscore = Count_Score(waiting);
                        if (cur_score == waitscore) deska.ShowText("Remíza");
                        else if (cur_score > waitscore) deska.ShowText("Vyhrál " + current.moje.shape.ToString() + "\n se skóre: " + cur_score);
                        else deska.ShowText("Vyhrál " + waiting.moje.shape.ToString() + "\n se skóre: " + waitscore);
                        deska.OnUpdate();
                        run = false;
                        log.Close();
                        return;
                    }
                    run = true;
                    player_thread.RunWorkerAsync();
                    return;
                }
                layout = Move(layout, turn);
                Hrac z = current;
                current = waiting;
                waiting = z;
                
                player_thread.DoWork -= waiting.Tah;
                player_thread.DoWork += current.Tah;
                
                deska.Update();
                deska.High_Light(turn.target, Color.Red, Color.DarkRed);
                if (current is Computer) deska.ShowText("Přemýšlím dlouho. \n Prosím čekejte...");
                deska.OnUpdate();
                run = true;

                player_thread.RunWorkerAsync();
            }
            
        }
        public Hrac Get_Current() //vrací momentálně hrajícího hráče
        {
            return current;
        }
        public void Tah(object sender, TurnArgs e)
        {
            if (e.Turn == null)
            {
                turn = new Tah(new Pole(0, 0), new Pole(0, 0), false);
                run = false;
            }
            else turn = e.Turn;
            if (player_thread.IsBusy) player_thread.CancelAsync();
            else TurnCross(this, null);   

        }
        Field[,] Move(Field[,] mapa, Tah tah)
        {
            mapa = current.moje.Move(mapa, tah);
            return mapa;
        }
        public Field[,] Lay_Out() //vysázení figurek
        {
            Field[,]layout = new Field[15, 15];
            Pole current;
            List<Pole> troll_f = Gen_Trolls();
            List<Pole> dwarf_f = Gen_Dwarf();
            for (int i = 5; i >= 0; i--)
            {
                for (int x = i; x < (15 - i); x++)
                {

                    layout[x, (5 - i)] = Field.Free;
                    layout[x, 6] = Field.Free;
                    if (x !=7)layout[x, 7] = Field.Free;
                    layout[x, 8] = Field.Free;
                    layout[x, (14 - (5 - i))] = Field.Free;
                }
            }
            while (dwarf_f.Any())//vylití trpaslíků
            {
                current = dwarf_f.First();
                dwarf_f.RemoveAt(0);
                layout[current.x, current.y] = Field.Dwarf;
               
            }
            while (troll_f.Any()) //vylití trollů
            {
                current = troll_f.First();
                troll_f.RemoveAt(0);
                layout[current.x, current.y] = Field.Troll;

            }
            return layout;
        }

        List<Pole> Gen_Dwarf() // počáteční rozložení trpaslíků
        {
            List<Pole> list = new List<Pole>(); 
            for (int i = 5; i >= 0; i--)
            {
                for (int x = i; x < (15 - i); x++)
                {
                    list.Add(new Pole(i, (5 - i)));
                    list.Add(new Pole(14-i, (5 - i)));
                    list.Add(new Pole(14-i, 14 - (5 - i)));
                    list.Add(new Pole(i, 14 - (5 - i)));
                }
            }
            list.Add(new Pole(6,0));
            list.Add(new Pole(8,0));
            list.Add(new Pole(0,6));
            list.Add(new Pole(0,8));
            list.Add(new Pole(14, 6));
            list.Add(new Pole(14, 8));
            list.Add(new Pole(8, 14));
            list.Add(new Pole(6, 14));
            return list;
        }
        List<Pole> Gen_Trolls() //počáteční rozložení Trollů
        {
            List<Pole> list = new List<Pole>();
            list.Add(new Pole(6,6));
            list.Add(new Pole(6, 7));
            list.Add(new Pole(6, 8));

            list.Add(new Pole(7, 6));
            list.Add(new Pole(7, 8));

            list.Add(new Pole(8, 6));
            list.Add(new Pole(8, 7));
            list.Add(new Pole(8, 8));
            return list;
        }
               
     }

    
    class Human : Hrac
    {
        Pole selected;
        Pole target;
        //public event TurnHandler EndTurn;
        bool surrender = false;
        public bool select;
        List<Pole> possible;
        public Human( Figura moje, Figura jeho) :base(moje, jeho)
        {
            possible = new List<Pole>();
        }

        public override void Tah(object sender, DoWorkEventArgs e)
        {
            surrender = false;
            done = false;
        }
        public void End_Tah()
        {
            
            if (done)
            {
                Tah turn = new Tah(selected,target,surrender);
                TurnArgs e = new TurnArgs(turn);
                On_EndTurn(this, e);
            }
        }
        public override bool Surrender()
        {
            string message = "Chcete ukončit hru teď?";
            string caption = "Konec hry?";
            MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            DialogResult result;
            result = MessageBox.Show(message, caption, buttons);
            if (result == DialogResult.Yes)
            {
                surrender = true;
                done = true;
                return true;
            }
            else
            {
                surrender = false;
                return false;
            }
        }
        public List<Pole>Select(Pole pole) //stará se o označení potecionální figurky k pohybu, vrací možnosti pohybu
        {
            List<Tah> Tahy = new List<Tah>();
            possible = new List<Pole>();
            Tah tah;
            if (mapa[pole.x, pole.y] == moje.shape)
            {
                done = false;
                selected = pole;
                select = true;
            }
            Tahy= moje.Make_Que(mapa, pole.x, pole.y);
            while (Tahy.Any())
            {
                tah = Tahy.First();
                Tahy.RemoveAt(0);
                possible.Add(tah.target);
            }
            return new List<Pole>(possible);
        }

        public bool Selecting(Pole pole) //zjišťuje jestli je si polem vybíráme nebo ne
        {

            for (int i=0; i<possible.Count; i++)
            {
                if((pole.x==possible.ElementAt(i).x)&& (pole.y == possible.ElementAt(i).y))
                {
                    if (select) return false;
                }
            }
            select = false;
            return true;
            

        }
        public void Move(Pole pole)
        {
            target = pole;
            select = false;
            done = true;
        }

    }

    enum Field {NaN,Free,Troll,Dwarf}

    /*----------------------------------------------------Počítač*/
    class Computer : Hrac
    {
        int max_depth;
        Field[,] dfs_map;
        Tah turn;
        public int max_score;
        public int min_span=655;
        public int max_span=0;

        //public delegate void InvokeHandler<TurnArgs>(object sender, TurnArgs e);

        public Computer(Figura moje, Figura jeho, int max_depth):base(moje,jeho)
        {
            this.max_depth = max_depth;

        }
        override public void Tah(object sender, DoWorkEventArgs i)
        {

            done = false;
            //pak spustíme prohledávání do hloubky ( stačí když každý běh vrátí věci do původního stavu)
            turn = new Tah(new Pole(0, 0), new Pole(0, 0), false);
            turn = DFS_Turn();
                        
            e = new TurnArgs(turn);
            this.done = true;
            On_EndTurn(this,e);
            

        }
        static Field[,] Copy_Map(Field[,] map)
        {
            Field[,] new_map = new Field[15, 15];
            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    new_map[x,y] = map[x, y];
                }
            }
            return new_map;
        }
        List<Pole> Count_Figures(Field[,] mapa,Field figura)
        {
            List<Pole> que = new List<Pole>();
            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    if (mapa[x, y] == figura) que.Add(new Pole(x, y));
                }
            }
            return que;
        }
        public override bool Surrender() //pokud dojde žádost o rezignaci, přijme ji
        {
            return true;
        }
        Tah DFS_Turn() //vrací momentálně nejlepší tah tah;
        {
            
           //
            dfs_map = Copy_Map(mapa);
            List<Pole> Figury = Count_Figures(dfs_map,moje.shape); //všechny figury 
            max_score = -100;
            Pole figurka;
            List<Tah> moznosti= new List<Tah>(); // všechny možné tahy
            List<Pole> taken; //sebrané figury pro návrat
            Tah max_turn; //nejlepší tah
            int alpha = -32;
            int beta = 32;
            int score;
            int min_score = 100;

            max_turn = new Tah(new Pole(0, 0), new Pole(0, 0), true);
            if (!Figury.Any())
            {
                return max_turn;
            }

            while (Figury.Any())//pro všechny figurky shormáždíme všechny tahy... funguje to tak líp s AB pruningem
            {
                figurka= Figury.First();
                Figury.RemoveAt(0);
                
                moznosti = moje.Make_Que(dfs_map, figurka.x, figurka.y,moznosti);
            }
            if (moznosti.Count > max_span) max_span = moznosti.Count;
            if (moznosti.Count < min_span) min_span = moznosti.Count;
            while (moznosti.Any()) // pro všechny tahy
            {               
                Tah turn = moznosti.First();
                moznosti.RemoveAt(0);
                dfs_map = moje.Move(dfs_map, turn);
                taken = moje.sebrane;
               
                //score = -DFS_Score(dfs_map, 0, -1, -(alpha +1), -alpha); //nulové okno
                //if (score > alpha && score < beta)
                //{
                score = -DFS_Score(dfs_map, 0, -1, -beta, -alpha);
                //}

                //if ((score >=4)||(score<=-4)) MessageBox.Show(score.ToString() + "\n" + 1 );
                if (score < min_score) min_score = score;
                if (max_score < score)
                {
                    max_score = score;
                    max_turn = turn;
                }
                if (score > alpha) alpha = score;
                
                dfs_map = moje.MoveBack(dfs_map, turn.Oposite(), taken, jeho.shape);
                if (alpha >= beta) //alefabeta ořezávání
                {
                    break;
                }
            }
            
            return max_turn;
        }
        int DFS_Score(Field[,]board ,int depth, int polarita,int alpha,int beta) // vrací nejlepší skóre nejlepšího tahu
        {
            
            List<Pole> taken = new List<Pole>();

 
            if (depth == max_depth) {
                return polarita * Score(board);
            }

            //cyklus tahů max_score je nejvyšší získané
            List<Pole> Figury = new List<Pole>();
            List<Tah> moznosti = new List<Tah>();
            if (polarita == 1)
            {
                Figury = Count_Figures(board, moje.shape);
            }

            else if (polarita == -1) Figury = Count_Figures(board, jeho.shape); 

            Pole figurka;
            int max_score = -100;
            Tah turning;
            int score=0;
            if (!Figury.Any())
            {
                
                return polarita*Score(board);
            }
            //int this_score;
            while (Figury.Any())//pro všechny figury
            {
                figurka = Figury.First();
                Figury.RemoveAt(0);

                if (polarita==1) moznosti = moje.Make_Que(board, figurka.x, figurka.y,moznosti);
                else if (polarita == -1)  moznosti = jeho.Make_Que(board, figurka.x, figurka.y,moznosti);            
            }
            if (moznosti.Count > max_span) max_span = moznosti.Count;
            if (moznosti.Count < min_span) min_span = moznosti.Count;
            while (moznosti.Any()) // pro všechny možnosti kam s figurou pohnout
            {
                turning = moznosti.First();
                moznosti.RemoveAt(0);
                if (polarita == 1)
                {
                    board = moje.Move(board, turning);
                    taken = moje.sebrane;
                }
                else if (polarita == -1)
                {
                    board = jeho.Move(board, turning);
                    taken = jeho.sebrane;
                }
                //int add=0;
                //if (polarita == 1) add = moje.value;
                //else if (polarita == -1) add = jeho.value;
                //score = -DFS_Score(board, depth + 1, -polarita, -(alpha + 1), -alpha); //nulové okno
                //if (score > alpha && score < beta)
                //{
                score = -DFS_Score(board, depth + 1, -polarita, -beta, -alpha);
                //}

                if (max_score < score)
                {
                    max_score = score;
                }
                if (score > alpha)
                {
                    alpha = score;
                }
                if (polarita == 1) board = moje.MoveBack(board, turning.Oposite(), taken, jeho.shape);
                else if (polarita == -1) board = jeho.MoveBack(board, turning.Oposite(), taken, moje.shape);
                if (alpha >= beta) break;
            }
            return max_score;
        }
        int Score(Field[,]mapa) // skórovací funkce
        {
            int score = 0;
            for (int x = 0; x < 15; x++)
            {
                for (int y = 0; y < 15; y++)
                {
                    if (mapa[x, y] == moje.shape) score+= moje.value;
                    else if (mapa[x, y] == jeho.shape) score -= jeho.value;
                }
            }
            return score;
        }
    }


}
