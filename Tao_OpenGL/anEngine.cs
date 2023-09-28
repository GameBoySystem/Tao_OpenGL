using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Platform.Windows;

namespace Tao_OpenGL
{
    public class anBrush
    {
        //объект Bitmap, в котором мы будем хранить битовую карту нашей кисти. 
        // этот объект выбран из-за удобства, а также из-за того,
        // что в будущем мы добавим возможность загрузки кистей
        // из графических файлов с расширением bmp. 
        public Bitmap myBrush;

        // флаг, сигнализирующий о том, что установленная кисть является стеркой 
        private bool IsErase = false;
        // функция, которая будет использоваться для получения информации
        // о том, является ли данная кисть стеркой. 
        public bool IsBrushErase() { return IsErase; }

        // стандартная(квадратная) кисть, с указанием масштаба
        // и флагом закраски углов
        public anBrush(int Value, bool Special)
        {
            if (!Special)
            {
                myBrush = new Bitmap(Value, Value);
                for (int ax = 0; ax < Value; ax++) for (int bx = 0; bx < Value; bx++)
                        myBrush.SetPixel(0, 0, Color.Black);
                IsErase = false;
            }

            else
            {
                // здесь мы будем размещать предустановленные кисти
                // созданная нами ранее кисть в виде перекрестия двух линий будет кистью по умолчанию
                // на тот случай, если задан не описанный номер кисти
                switch (Value)
                {
                    default:
                        {
                            // создаем плоскость 4х4 пикселя 
                            myBrush = new Bitmap(4, 4);
                            // заполняем все пиксели красным цветом
                            // (все пиксели красного цвета мы будем считать не значимыми, 
                            // а черного – значимыми при рисования кистью 
                            // для установки пискеля, как видно из кода, используется функция SetPixel. 
                            for (int ax = 0; ax < 4; ax++)
                                for (int bx = 0; bx < 4; bx++)
                                    myBrush.SetPixel(ax, bx, Color.Red);
                            myBrush.SetPixel(0, 1, Color.Black);
                            myBrush.SetPixel(0, 2, Color.Black);
                            myBrush.SetPixel(1, 0, Color.Black);
                            myBrush.SetPixel(1, 1, Color.Black);
                            myBrush.SetPixel(1, 2, Color.Black);
                            myBrush.SetPixel(1, 3, Color.Black);
                            myBrush.SetPixel(2, 0, Color.Black);
                            myBrush.SetPixel(2, 1, Color.Black);
                            myBrush.SetPixel(2, 2, Color.Black);
                            myBrush.SetPixel(2, 3, Color.Black);
                            myBrush.SetPixel(3, 1, Color.Black);
                            myBrush.SetPixel(3, 2, Color.Black);

                            IsErase = false;

                            break;
                        }
                    case 1:
                        {
                            myBrush = new Bitmap(5, 5);
                            for (int ax = 0; ax < Value; ax++) for (int bx = 0; bx < Value; bx++)
                                    myBrush.SetPixel(0, 0, Color.Black);
                            IsErase = true;
                            break;
                        }
                }
            }
        }
        // второй конструктор будет позволять загружать кисть из стороннего файла 
        public anBrush(string FromFile)
        {
            string path = Directory.GetCurrentDirectory();
            path += "\\" + FromFile;
            myBrush = new Bitmap(path);
        }
    }

    // ===================================================================================================
    // ===================================================================================================
    public class anLayer
    {
        // размеры экранной области 
        public int Width, Heigth;
        // массив, представляющий область рисунка (координаты пикселя и его цвет), 
        private int[,,] DrawPlace;
        // который будет хранить растровые данные для данного слоя 
        public int[,,] GetDrawingPlace() { return DrawPlace; }
        // флаг видимости слоя: true - видимый, false - невидимый 
        private bool isVisible;
        // текущий установленный цвет 
        private Color ActiveColor;
        // конструктор класса, в качестве входных параметров 
        // мы получаем размеры изображения, чтобы создать в памяти массив, 
        // который будет хранить растровые данные для данного слоя 
        public anLayer(int s_W, int s_H)
        {
            // запоминаем значения размеров рисунка 
            Width = s_W; Heigth = s_H;
            // создаем в памяти массив, соответствующий размерам рисунка 
            // каждая точка на плоскости массива будет иметь 3 составляющие цвета 
            // + 4 ячейка - флаг, о том что данный пиксель пуст (или полностью прозрачен) 
            DrawPlace = new int[Width, Heigth, 4];
            // проходим по всей плоскости и устанавливаем всем точкам флаг, 
            // сигнализирующий, что они прозрачны 
            for (int ax = 0; ax < Width; ax++)
            {
                for (int bx = 0; bx < Heigth; bx++)
                {
                    // флаг прозрачности точки в координатах ax,bx. 
                    DrawPlace[ax, bx, 3] = 1;
                }
            }
            // устанавливаем флаг видимости слоя (по умолчанию создаваемый слой всегда видимый) 
            isVisible = true;
            // текущим активным цветом устанавливаем черный 
            // в следующей главе мы реализуем функции установки цветов из оболочки. 
            ActiveColor = Color.Black;
        }
        // функция установки режима видимости слоя 
        public void SetVisibility(bool visiblityState)
        {
            isVisible = visiblityState;
        }
        // функция получения текущего состояния видимости слоя 
        public bool GetVisibility() { return isVisible; }
        // функция рисования 
        // получает в качестве параметров кисть для рисования и координаты, 
        // где сейчас необходимо перерисовать пиксели заданной кистью 
        public void Draw(anBrush BR, int x, int y)
        {
            // определяем позиция старта рисования 
            int real_pos_draw_start_x = x - BR.myBrush.Width / 2;
            int real_pos_draw_start_y = y - BR.myBrush.Height / 2;
            // корректируем ее для не выхода за границы массива 
            // проверка на отрицательные значения (граница "справа")
            if (real_pos_draw_start_x < 0) real_pos_draw_start_x = 0;
            if (real_pos_draw_start_y < 0) real_pos_draw_start_y = 0;
            // проверки на выход за границу "справа"
            int boundary_x = real_pos_draw_start_x + BR.myBrush.Width;
            int boundary_y = real_pos_draw_start_y + BR.myBrush.Height;
            if (boundary_x > Width) boundary_x = Width;
            if (boundary_y > Heigth) boundary_y = Heigth;
            // счетчик пройденных строк и столбцов массива, представляющий собой маску кисти
            int count_x = 0, count_y = 0;
            // цикл по области с учетом смещения кисти и коррекции для невыхода за границы массива 
            for (int ax = real_pos_draw_start_x; ax < boundary_x; ax++, count_x++)
            {
                count_y = 0;
                for (int bx = real_pos_draw_start_y; bx < boundary_y; bx++, count_y++)
                {
                    if (BR.IsBrushErase())
                    {
                        // данная кисть - стерка. 
                        // помечаем данный пиксель как не закрашенный 
                        // получаем текущий цвет пикселя маски 
                        Color ret = BR.myBrush.GetPixel(count_x, count_y);
                        // цвет не красный 
                        if (!(ret.R == 255 && ret.G == 0 && ret.B == 0))
                        {
                            // заполняем данный пиксель соответствующим из маски,
                            // используя активный цвет 
                            DrawPlace[ax, bx, 3] = 1;
                        }
                    }
                    else
                    {
                        // получаем текущий цвет пикселя маски 
                        Color ret = BR.myBrush.GetPixel(count_x, count_y);
                        // цвет не красный 
                        if (!(ret.R == 255 && ret.G == 0 && ret.B == 0))
                        {
                            // заполняем данный пиксель соответствующим из маски, используя активный цвет 
                            DrawPlace[ax, bx, 0] = ActiveColor.R;
                            Console.WriteLine(DrawPlace[ax, bx, 0]);
                            Console.WriteLine(DrawPlace[ax, bx, 1]);
                            Console.WriteLine(DrawPlace[ax, bx, 2]);
                            DrawPlace[ax, bx, 1] = ActiveColor.G;
                            DrawPlace[ax, bx, 2] = ActiveColor.B;
                            DrawPlace[ax, bx, 3] = 0;
                        }
                    }
                }
            }
        }
        // функция визуализации слоя 
        public void RenderImage()
        {
            // данную функцию мы улучшим в следующих частях,
            // для того чтобы получить более быструю визуализацию, 
            // но пока она будет выглядеть следующим образом 
            // активируем режим рисования точек 
            Gl.glBegin(Gl.GL_POINTS);
            // проходим по всем точкам рисунка 
            for (int ax = 0; ax < Width; ax++)
            {
                for (int bx = 0; bx < Heigth; bx++)
                {
                    // если точка в координатах ax,bx не помечена флагом "прозрачная", 
                    if (DrawPlace[ax, bx, 3] != 1)
                    {
                        // устанавливаем заданный в ней цвет 
                        Gl.glColor3f(
                            (float)(Convert.ToDouble(DrawPlace[ax, bx, 0]) / 255),
                            (float)(Convert.ToDouble(DrawPlace[ax, bx, 1]) / 255),
                            (float)(Convert.ToDouble(DrawPlace[ax, bx, 2]) / 255));
                        // и выводим ее на экран 
                        Gl.glVertex2i(ax, bx);
                    }
                }
            }
            // завершаем режим рисования 
            Gl.glEnd();
        }
        // установка текущего цвета для рисования в слое
        public void SetColor(Color NewColor)
        {
            ActiveColor = NewColor;
        }
        // получение текущего активного цвета
        public Color GetColor()
        {
            return ActiveColor;
        }
    }

    // ===================================================================================================
    // ===================================================================================================
    // класс, реализующий "ядро" нашего растрового редактора. 
    public class anEngine
    {
        // размеры изображения
        private int picture_size_x, picture_size_y;
        // положение полос прокрутки будет использовано в будущем 
        private int scroll_x, scroll_y;
        // размер оконной части (объекта AnT) 
        private int screen_width, screen_height;
        // номер активного слоя
        private int ActiveLayerNom;
        // массив слоев
        private ArrayList Layers = new ArrayList();
        // стандартная кисть
        private anBrush standartBrush;
        // последний установленный цвет
        private Color LastColorInUse;
        // конструктор класса
        public anEngine(int size_x, int size_y, int screen_w, int screen_h)
        {
            // при инициализации экземпляра класса сохраним настройки
            // размеров элементов и изображения в локальных переменных
            picture_size_x = size_x;
            picture_size_y = size_y;
            screen_width = screen_w;
            screen_height = screen_h;
            // полосы прокрутки у нас пока отсутствуют, поэтому просто обнулим значение переменных
            scroll_x = 0;
            scroll_y = 0;
            // добавим новый слой для работы, пока он будет единственным 
            Layers.Add(new anLayer(picture_size_x, picture_size_y));
            // номер активного слоя - 0
            ActiveLayerNom = 0;
            // и создадим стандартную кисть
            standartBrush = new anBrush(3, false);
        }
        // функция для установки номера активного слоя 
        public void SetActiveLayerNom(int nom)
        {
            // новый активный слой получает установленный активный цвет для предыдущего активного слоя 
            ((anLayer)Layers[nom]).SetColor(((anLayer)Layers[ActiveLayerNom]).GetColor());
            // установка номера активного слоя
            ActiveLayerNom = nom;
        }
        //установка видимости / невидимости слоя
        public void SetWisibilityLayerNom(int nom, bool visible)
        {

        }
        // рисование текущей кистью
        public void Drawing(int x, int y)
        {
            // транслируем координаты, в которых проходит рисование, стандартной кистью 
            ((anLayer)Layers[ActiveLayerNom]).Draw(standartBrush, x, y);
        }
        // визуализация
        public void SwapImage()
        {
            // вызываем функцию визуализации в нашем слое для всех существующих слоев 
            for (int ax = 0; ax < Layers.Count; ax++)
                ((anLayer)Layers[ax]).RenderImage();
        }
        // функция установки стандартной кисти, передается только размер 
        public void SetStandartBrush(int SizeB)
        {
            standartBrush = new anBrush(SizeB, false);
        }
        // функция установки специальной кисти 
        public void SetSpecialBrush(int Nom)
        {
            standartBrush = new anBrush(Nom, true);
        }
        // установка кисти из файла 
        public void SetBrushFromFile(string FileName)
        {
            standartBrush = new anBrush(FileName);
        }
        // функция установки активного цвета 
        public void SetColor(Color NewColor)
        {
            ((anLayer)Layers[ActiveLayerNom]).SetColor(NewColor);
            LastColorInUse = NewColor;
        }
        // функция добавления слоя 
        public void AddLayer()
        {
            // добавляем слой в массив слоев ArrayList 
            int AddingLayer = Layers.Add(new anLayer(picture_size_x, picture_size_y));
            // устанавливаем его активным 
            SetActiveLayerNom(AddingLayer);
        }
        // функция удаления слоев 
        public void RemoveLayer(int nom)
        {
            // если номер корректен (в диапазоне добавленных в ArrayList 
            if (nom < Layers.Count && nom >= 0)
            {
                // делаем активным слой 0 
                SetActiveLayerNom(0);

                // очищаем дисплейный список данного слоя 
                //((anLayer)Layers[nom]).ClearList();

                // удаляем запись о слое 
                Layers.RemoveAt(nom);
            }
        }
        // получение финального изображения
        public Bitmap GetFinalImage()
        {
            // заготовка результирующего изображения 
            Bitmap resaultBitmap = new Bitmap(picture_size_x, picture_size_y);
            // данное решение также не является оптимальным по быстродействию,
            // но при этом является самым простым способом решения задачи 
            for (int ax = 0; ax < Layers.Count; ax++)
            {
                // получаем массив пикселей данного слоя 
                int[,,] tmp_layer_data = ((anLayer)Layers[ax]).GetDrawingPlace();
                // пройдем двумя циклами по информации о пикселях данного слоя
                for (int a = 0; a < picture_size_x; a++)
                {
                    for (int b = 0; b < picture_size_y; b++)
                    {
                        // в случае, если пиксель не помечен как "прозрачный", 
                        if (tmp_layer_data[a, b, 3] != 1)
                        {
                            // устанавливаем данный пиксель на результирующее изображение
                            resaultBitmap.SetPixel(a, b, Color.FromArgb(tmp_layer_data[a, b, 0], tmp_layer_data[a, b, 1], tmp_layer_data[a, b, 2]));
                        }
                        else
                        {
                            if (ax == 0) // нулевой слой - необходимо закрасить белым отсутствующие пиксели
                            {
                                // закрашиваем белым цветом
                                resaultBitmap.SetPixel(a, b, Color.FromArgb(255, 255, 255));
                            }
                        }
                    }
                }

            }
            // поворачиваем изображение для корректного отображения 
            resaultBitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
            // возвращаем результат 
            return resaultBitmap;
        }
        // получение изображения для главного слоя 
        public void SetImageToMainLayer(Bitmap layer)
        {
            // поворачиваем изображение (чтобы оно корректно отображалось в области редактирования). 
            layer.RotateFlip(RotateFlipType.Rotate180FlipX);
            // проходим 2-мя циклами по всем пикселям изображения, загруженного в класс Bitmap 
            // получая цвет пикселя, устанавливаем его в текущий слой с помощью функции Drawing 
            // данный алгоритм является крайне медленным, но при этом и крайне простым. 
            // оптимальным решением здесь будет написание собственного загрузчика файлов изображений 
            // что даст возможность без "посредников" получать массив значений пикселей изображений 
            // но данная задача является на много более сложной, а для обучения мы идем более легкими путями 
            for (int ax = 0; ax < layer.Width; ax++)
            {
                for (int bx = 0; bx < layer.Height; bx++)
                {
                    // получение цвета пикселя изображения 
                    SetColor(layer.GetPixel(ax, bx));
                    // отрисовка данного пикселя в слое 
                    Drawing(ax, bx);
                }
            }
        }
    }
}
