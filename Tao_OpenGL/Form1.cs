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

        private void Form1_Load(object sender, EventArgs e)
        {
            // инициализация Glut
            Glut.glutInit();
            Glut.glutInitDisplayMode(Glut.GLUT_RGB | Glut.GLUT_DOUBLE |
            Glut.GLUT_DEPTH);
            // очистка окна
            Gl.glClearColor(255, 255, 255, 1);
            // установка порта вывода в соответствии с размерами элемента AnT
            Gl.glViewport(0, 0, AnT.Width, AnT.Height);
            // настройка проекции
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            // теперь необходимо корректно настроить 2D ортогональную проекцию
            // в зависимости от того, какая сторона больше
            // мы немного варьируем то, как будут сконфигурированы настройки проекции
            if ((float)AnT.Width <= (float)AnT.Height)
            {
                Glu.gluOrtho2D(0.0, 30.0 * (float)AnT.Height / (float)AnT.Width, 0.0, 30.0);
            }
            else
            {
                Glu.gluOrtho2D(0.0, 30.0 * (float)AnT.Width / (float)AnT.Height, 0.0, 30.0);
            }
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            // настройка параметров OpenGL для визуализации
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // очищаем буфер цвета и глубины
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            // очищаем текущую матрицу
            Gl.glLoadIdentity();
            // устанавливаем текущий цвет - красный
            Gl.glColor3f(255, 0, 0);
            // активируем режим рисования линий на основе
            // последовательного соединения всех вершин отрезками
            Gl.glBegin(Gl.GL_LINE_LOOP);
            // первая вершина будет находиться в начале координат
            Gl.glVertex2d(8, 7);
            Gl.glVertex2d(8, 12);
            Gl.glVertex2d(10, 12);
            Gl.glVertex2d(13, 25);
            Gl.glVertex2d(20, 25);
            Gl.glVertex2d(20, 12);
            Gl.glVertex2d(22, 12);
            Gl.glVertex2d(22, 7);
            Gl.glVertex2d(21, 7);
            Gl.glVertex2d(21, 11);
            Gl.glVertex2d(9, 11);
            Gl.glVertex2d(9, 7);
            Gl.glEnd();
            // вторая линия
            Gl.glBegin(Gl.GL_LINE_LOOP);
            Gl.glVertex2d(11, 12);
            Gl.glVertex2d(14, 24);
            Gl.glVertex2d(19, 24);
            Gl.glVertex2d(19, 12);
            Gl.glEnd();
            // дожидаемся конца визуализации кадра
            Gl.glFlush();
            // посылаем сигнал перерисовки элемента AnT.
            AnT.Invalidate();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

    }
}
