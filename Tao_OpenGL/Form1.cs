using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace Tao_OpenGL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            AnT.InitializeContexts();
        }

        private anEngine ProgrammDrawingEngine;



        private void Form1_Load(object sender, EventArgs e)
        {
            // инициализация библиотеки GLUT
            Glut.glutInit();
            // инициализация режима окна
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE | Glut.GLUT_DEPTH);
            // устанавливаем цвет очистки окна
            Gl.glClearColor(255, 255, 255, 1);
            // устанавливаем порт вывода, основываясь на размерах элемента управления AnT
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);
            // устанавливаем проекционную матрицу 
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            // очищаем ее
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0.0, AnT.Width, 0.0, AnT.Height);
            // переходим к объектно-видовой матрице 
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            ProgrammDrawingEngine = new anEngine(AnT.Width, AnT.Height, AnT.Width, AnT.Height);
            RenderTimer.Start();
        }

        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            Drawing();
        }

        // функция рисования
        private void Drawing()
        {
            // очистка буфера цвета и буфера глубины
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            // очищение текущей матрицы
            Gl.glLoadIdentity();
            // установка черного цвета
            Gl.glColor3f(0, 0, 0);
            // визуализация изображения из движка
            ProgrammDrawingEngine.SwapImage();
            // дожидаемся завершения визуализации кадра
            Gl.glFlush();
            // сигнал для обновление элемента, реализующего визуализацию. 
            AnT.Invalidate();
        }

        private void AnT_MouseMove(object sender, MouseEventArgs e)
        {
            //если нажата левая клавиша мыши 
            if (e.Button == MouseButtons.Left)
                ProgrammDrawingEngine.Drawing(e.X, AnT.Height - e.Y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // устанавливаем стандартную кисть 4х4
            ProgrammDrawingEngine.SetStandartBrush(4);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // устанавливаем специальную кисть
            ProgrammDrawingEngine.SetSpecialBrush(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // установить кисть из файла
            ProgrammDrawingEngine.SetBrushFromFile("stars.bmp");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // временное хранение цвета элемента color1
            Color tmp = color1.BackColor;
            // замена:
            color1.BackColor = color2.BackColor;
            color2.BackColor = tmp;
            // передача нового цвета в ядро растрового редактора
            ProgrammDrawingEngine.SetColor(color1.BackColor);
        }

        // функция установки нового цвета с помощью диалогового окна выбора цвета 
        private void color1_MouseClick(object sender, MouseEventArgs e)
        {
            // если цвет успешно выбран
            if (changeColor.ShowDialog() == DialogResult.OK)
                // установить данный цвет
                color1.BackColor = changeColor.Color;
            // и передать его в класс anEngine для установки активным цветом текущего слоя
            ProgrammDrawingEngine.SetColor(color1.BackColor);
        }
    }
}

