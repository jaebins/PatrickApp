using SpongebobMottoApp.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpongebobMottoApp
{
    public partial class Form1 : Form
    {
        // 이미지 관련 변수들
        string spritesDir;
        string[] spritesPaths;
        int nowFrameCnt = 0;
        Bitmap[] patickSprites;
        PictureBox mainFrame;
        Graphics g_screen;
        Size newScreenSize;

        // 화면 제어 변수들
        bool isClick = false;
        int sCtrlMode = 0;
        Dictionary<int, Pen> sCtrlMode_color = new Dictionary<int, Pen>();
        Point startMousePos = new Point();
        Size startFrameSize;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadResource();
            InitSet();
        }

        void LoadResource()
        {
            spritesDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\Patrick";
            spritesPaths = Directory.GetFiles(spritesDir);
            patickSprites = new Bitmap[spritesPaths.Length];

            for (int i = 0; i < patickSprites.Length; i++)
            {
                patickSprites[i] = new Bitmap(spritesPaths[i]);
            }

            sCtrlMode_color.Add(1, Pens.Blue);
            sCtrlMode_color.Add(2, Pens.Red);
        }

        void InitSet()
        {
            Size imgSize = patickSprites[0].Size;
            SetClientSizeCore(imgSize.Width, imgSize.Height);
            newScreenSize = imgSize;

            mainFrame = new PictureBox();
            mainFrame.Size = imgSize;
            mainFrame.MouseDown += Event_SetMousePos;
            mainFrame.MouseMove += Event_SetFormSize;
            mainFrame.MouseMove += Event_SetWindowPos;
            mainFrame.MouseUp += Event_CompleteSetFormSize;
            mainFrame.Paint += Event_ChangeMode;
            Controls.Add(mainFrame);

            mainFrame.Image = patickSprites[nowFrameCnt];

            Task t1 = new Task(new Action(PlayFrames));
            t1.Start();
        }

        void Event_SetMousePos(object args, MouseEventArgs e)
        {
            isClick = true;
            
            startMousePos = e.Location;
            startFrameSize = this.Size;
        }

        void Event_SetWindowPos(object args, MouseEventArgs e)
        {
            if (!isClick || sCtrlMode != 1) return;

            Location = new Point(Cursor.Position.X - startMousePos.X, Cursor.Position.Y - startMousePos.Y);
        }

        void Event_SetFormSize(object args, MouseEventArgs e)
        {
            if (!isClick || sCtrlMode != 2) return;

            newScreenSize = new Size(startFrameSize.Width + (e.Location.X - startMousePos.X), startFrameSize.Height + (e.Location.Y - startMousePos.Y));
            if (newScreenSize.Width > 0 && newScreenSize.Height > 0)
            {
                SetClientSizeCore(newScreenSize.Width, newScreenSize.Height);
                mainFrame.Size = newScreenSize;
            }
        }

        void Event_CompleteSetFormSize(object args, MouseEventArgs e)
        {
            isClick = false;

            ResetImg();
        }

        void ResetImg()
        {
            for(int i = 0; i < patickSprites.Length; i++)
            {
                patickSprites[i] = new Bitmap(new Bitmap(spritesPaths[i]), newScreenSize);
            }
            mainFrame.Image = patickSprites[nowFrameCnt];
        }

        void Event_ChangeMode(object args, PaintEventArgs e)
        {
            if (sCtrlMode > 0)
            {
                g_screen = e.Graphics;

                if (isClick) mainFrame.Image = null;
                g_screen.DrawRectangle(sCtrlMode_color[sCtrlMode], new Rectangle(0, 0, Width - 1, Height - 1));
            }
        }

        async void PlayFrames()
        {
            while (true)
            {
                for(nowFrameCnt = 0; nowFrameCnt < patickSprites.Length; nowFrameCnt++)
                {
                    await Task.Delay(100);
                    mainFrame.Image = patickSprites[nowFrameCnt];
                }
            }
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (isClick) return;

            switch(e.KeyCode)
            {
                case Keys.Enter:
                    sCtrlMode = 0;
                    break;
                case Keys.T:
                    sCtrlMode = 1;
                    break;
                case Keys.S:
                    sCtrlMode = 2;
                    break;
                case Keys.Escape:
                    Application.Exit();
                    break;
            }

            ResetImg();
        }
    }
}
