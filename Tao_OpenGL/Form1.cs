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
        // текущий активный слой 
        private int ActiveLayer = 0;
        // счетчик слоев 
        private int LayersCount = 1;
        // счетчик всех создаваемых слоев для генерации имен 
        private int AllLayrsCount = 1;


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

            // добавление элемента, отвечающего за управления главным слоем в объект LayersControl 
            LayersControl.Items.Add("Главный слой", true);
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

        private void button4_Click(object sender, EventArgs e)
        {
            // ластик
            ProgrammDrawingEngine.SetSpecialBrush(1);
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

        private void добавитьСлойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // счетчик созданных слоев 
            LayersCount++;
            // вызываем функцию добавления слоя в движке графического редактора 
            ProgrammDrawingEngine.AddLayer();
            // добавляем слой, генерирую имя "Слой №" в объекте LayersControl. 
            // обязательно после функции ProgrammDrawingEngine.AddLayer();, 
            // иначе произойдет попытка установки активного цвета для еще не существующего цвета 
            int AddingLayerNom = LayersControl.Items.Add("Слой" + LayersCount.ToString(), false);
            // выделяем его 
            LayersControl.SelectedIndex = AddingLayerNom;
            // устанавливаем его как активный 
            ActiveLayer = AddingLayerNom;

        }

        private void удалитьСлойToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // запрашиваем подтверждение действия с помощью messageBox 
            DialogResult res = MessageBox
                .Show("Будет удален текущий активный слой, действительно продолжить?", "Внимание!",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            // если пользователь нажал кнопку "ДА" в окне подтверждения 
            if (res == DialogResult.Yes)
            {
                // если удаляемый слой - начальный 
                if (ActiveLayer == 0)
                {
                    // сообщаем о невозможности удаления 
                    MessageBox.Show("Вы не можете удалить нулевой слой.", "Внимание!",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
                else // иначе
                {
                    // уменьшаем значение счетчика слоев 
                    LayersCount--;
                    // сохраняем номер удаляемого слоя,
                    // т.к. SelectedIndex измениться после операций в LayersControl 
                    int LayerNomForDel = LayersControl.SelectedIndex;
                    // удаляем запись в элементе LayerControl
                    // (с индексом LayersControl.SelectedIndex - текущим выделенным слоем) 
                    LayersControl.Items.RemoveAt(LayerNomForDel);
                    // устанавливаем выделенным слоем нулевой (главный слой) 
                    LayersControl.SelectedIndex = 0;
                    // помечаем активный слой - нулевой 
                    ActiveLayer = 0;
                    // помечаем галочкой нулевой слой 
                    LayersControl.SetItemCheckState(0, CheckState.Checked);
                    // вызываем функцию удаления слоя в движке программы 
                    ProgrammDrawingEngine.RemoveLayer(LayerNomForDel);
                }
            }
        }

        private void LayersControl_SelectedValueChanged(object sender, EventArgs e)
        {
            // если отметили новый слой, необходимо снять галочку выделения со старого 
            if (LayersControl.SelectedIndex != ActiveLayer)
            {
                // если выделенный индекс является корректным
                // (больше либо равен нулю и входит в диапазон элементов) 
                if (LayersControl.SelectedIndex != -1 && ActiveLayer < LayersControl.Items.Count)
                {
                    // снимаем галочку с предыдущего активного слоя
                    LayersControl.SetItemCheckState(ActiveLayer, CheckState.Unchecked);
                    // сохраняем новый индекс выделенного элемента 
                    ActiveLayer = LayersControl.SelectedIndex;
                    // помечаем галочкой новый активный слой 
                    LayersControl.SetItemCheckState(LayersControl.SelectedIndex, CheckState.Checked);
                    // посылаем сигнал движку программы об изменении активного слоя 
                    ProgrammDrawingEngine.SetActiveLayerNom(ActiveLayer);
                }
            }
        }

        // дублирование создания слоя
        private void button10_Click(object sender, EventArgs e)
        {
            добавитьСлойToolStripMenuItem_Click(sender, e);
        }

        // дублирование удаления слоя
        private void button11_Click(object sender, EventArgs e)
        {
            удалитьСлойToolStripMenuItem_Click(sender, e);
        }

        private void карандашToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }

        private void кистьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button3_Click(sender, e);
        }

        private void ластикToolStripMenuItem_Click(object sender, EventArgs e)
        {
            button4_Click(sender, e);
        }

        private void новыйРисунокToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // вызываем диалог подтверждения 
            DialogResult result = MessageBox.Show(
                "В данный момент проект уже начат, сохранить изменения перед закрытием проекта?",
                "Внимание!",
                MessageBoxButtons.YesNoCancel);
            // если отказ пользователя
            // в зависимости от результата нажатия кнопки пользователем в окне MessageBox 
            switch (result)
            {
                case DialogResult.No:
                    {
                        // просто создаем чистый проект 
                        ProgrammDrawingEngine = new anEngine(AnT.Width, AnT.Height, AnT.Width, AnT.Height);
                        // очищаем информацию о добавляемых ранее слоях 
                        LayersControl.Items.Clear();
                        // по новой инициализируем нулевой слой: 
                        // текущий активный слой 
                        ActiveLayer = 0;
                        // счетчик слоев 
                        LayersCount = 1;
                        // счетчик всех создаваемых слоев для генерации имен 
                        AllLayrsCount = 1;
                        // добавление элемента, отвечающего за управления главным слоем в объект LayersControl 
                        LayersControl.Items.Add("Главный слой", true);
                        break;
                    }
                case DialogResult.Cancel:
                    {
                        // возвращаемся 
                        return;
                    }
                case DialogResult.Yes:
                    {
                        // открываем окно сохранения файла, и если имя файла указано
                        // и DialogResult вернуло сигнал об успешном нажатии кнопки ОК 
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            // получаем результирующее изображение слоя 
                            Bitmap ToSave = ProgrammDrawingEngine.GetFinalImage();
                            // сохраняем, используя имя файла, указанное в диалоговом окне сохранения файла 
                            ToSave.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            // сохранили - начинаем новый проект: 
                            // создаем новый объект "движка" программы 
                            ProgrammDrawingEngine = new anEngine(AnT.Width, AnT.Height, AnT.Width, AnT.Height);
                            // очищаем информацию о добавляемых ранее слоях 
                            LayersControl.Items.Clear();
                            // по новой инициализируем нулевой слой: 
                            // текущий активный слой 
                            ActiveLayer = 0;
                            // счетчик слоев 
                            LayersCount = 1;
                            // счетчик всех создаваемых слоев для генерации имен 
                            AllLayrsCount = 1;
                            // добавление элемента, отвечающего за управления главным слоем в объект LayersControl 
                            LayersControl.Items.Add("Главный слой", true);
                        }
                        else
                        {
                            // если сохранение не завершилось нормально (скорее всего пользователь закрыл окно сохранения файла 
                            // возвращаемся в проект 
                            return;
                        }
                        break;
                    }
            }
        }

        private void загрузитьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // вызываем диалог подтверждения 
            DialogResult reslt = MessageBox.Show(
                "В данный момент проект уже начат, сохранить изменения перед закрытием проекта?",
                "Внимание!", MessageBoxButtons.YesNoCancel);
            // если отказ пользователя 
            switch (reslt)
            {
                case DialogResult.No:
                    {
                        // просто создаем проект, подгружая изображения 
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            // проверяем существование файла 
                            if (System.IO.File.Exists(openFileDialog1.FileName))
                            {
                                // загружаем изображение в экземпляр класса Bitmap 
                                Bitmap ToLoad = new Bitmap(openFileDialog1.FileName);
                                // если размер изображения не корректен
                                if (ToLoad.Width > AnT.Width || ToLoad.Height > AnT.Height)
                                {
                                    // сообщаем пользователю об ошибке 
                                    MessageBox.Show(
                                        "Извините, но размер изображения превышает размеры области рисования",
                                        "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                    // возвращаемся к функции 
                                    return;
                                }
                                // если размер был меньше области редактирования программы 
                                // создаем новый экземпляр класса anEngine 
                                ProgrammDrawingEngine = new anEngine(AnT.Width, AnT.Height, AnT.Width, AnT.Height);
                                // копируем изображение в нижний левый угол рабочей области 
                                ProgrammDrawingEngine.SetImageToMainLayer(ToLoad);
                                // очищаем информацию о добавляемых ранее слоях 
                                LayersControl.Items.Clear();
                                // по новой инициализируем нулевой слой:
                                // текущий активный слой 
                                ActiveLayer = 0;
                                // счетчик слоев 
                                LayersCount = 1;
                                // счетчик всех создаваемых слоев для генерации имен 
                                AllLayrsCount = 1;
                                // добавление элемента, отвечающего за управления главным слоем в объект LayersControl 
                                LayersControl.Items.Add("Главный слой", true);
                            }
                        }
                        break;
                    }
                case DialogResult.Cancel:
                    {
                        // возвращаемся
                        return;
                    }
                case DialogResult.Yes:
                    {
                        // открываем окно сохранения файла, и если имя файла указано
                        // и DialogResult вернуло сигнал об успешном нажатии кнопки ОК 
                        if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            // получаем результирующее изображение слоя 
                            Bitmap ToSave = ProgrammDrawingEngine.GetFinalImage();
                            // сохраняем, используя имя файла, указанное в диалоговом окне сохранения файла 
                            ToSave.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                            // сохранили - начинаем новый проект: // просто создаем проект, подгружая изображения 
                            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                            {
                                // проверяем существование файла 
                                if (System.IO.File.Exists(openFileDialog1.FileName))
                                {
                                    // загружаем изображение в экземпляр класса Bitmap
                                    Bitmap ToLoad = new Bitmap(openFileDialog1.FileName);
                                    // если размер изображения не корректен 
                                    if (ToLoad.Width > AnT.Width || ToLoad.Height > AnT.Height)
                                    {
                                        // сообщаем пользователю об ошибке 
                                        MessageBox.Show("Извините, но размер изображения превышает размеры области рисования",
                                            "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                                        // возвращаемся и функции 
                                        return;
                                    }
                                    // если размер был меньше области редактирования программы 
                                    // создаем новый экземпляр класса anEngine 
                                    ProgrammDrawingEngine = new anEngine(AnT.Width, AnT.Height, AnT.Width, AnT.Height);
                                    // копируем изображение в нижний левый угол рабочей области 
                                    ProgrammDrawingEngine.SetImageToMainLayer(ToLoad);
                                    // очищаем информацию о добавляемых ранее слоях 
                                    LayersControl.Items.Clear();
                                    // по новой инициализируем нулевой слой: // текущий активный слой 
                                    ActiveLayer = 0;
                                    // счетчик слоев 
                                    LayersCount = 1;
                                    // счетчик всех создаваемых слоев для генерации имен 
                                    AllLayrsCount = 1;
                                    // добавление элемента, отвечающего за управления главным слоем в объект LayersControl 
                                    LayersControl.Items.Add("Главный слой", true);
                                }
                            }
                            break;
                        }
                        else
                        {
                            return; 
                        }
                    }
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // открываем окно сохранения файла, и если имя файла указано и DialogResult вернуло сигнал об успешном нажатии кнопки ОК 
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) { 
                // получаем результирующее изображение слоя 
                Bitmap ToSave = ProgrammDrawingEngine.GetFinalImage(); 
                // сохраняем используя имя файла указанное в диалоговом окне сохранения файла 
                ToSave.Save(saveFileDialog1.FileName, System.Drawing.Imaging.ImageFormat.Jpeg); }
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}