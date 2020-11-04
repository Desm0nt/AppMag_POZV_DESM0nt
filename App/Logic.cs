namespace App
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using Tao.OpenGl;
    using OpenCvSharp;
    using Size = OpenCvSharp.Size;
    using System.Media;
    using System.Globalization;
    using App.Model;
    using MathNet.Numerics.LinearAlgebra.Double;
    using MeshGenerator.Elements;
    using MeshGenerator.Model;
    using MeshGenerator.Modelling.Conditions;
    using MeshGenerator.Modelling.Loads;
    using MeshGenerator.Modelling.Solutions;
    using MeshGenerator.Modelling.Solvers;
    using MeshGenerator.Scene;
    using System.Threading.Tasks;

    public partial class Logic : Form
    {
        private const int DEGREES_OF_FREEDOM = 3;
        private const int VERTEBRA_MATERIAL_ID = 2;
        private const int INNERVERTEBRAL_DISK_MATERIAL_ID = 3;
        private const double STEP_WIDTH = 23.75; // step of the tetraherdral model by width
        private const double STEP_HEIGHT = 20; // step of the tetraherdral model by height

        FeModel model;
        ISolution solution;
        ISolve<SparseMatrix> solver;
        IRepository<string, List<Tetrahedron>> repository;
        //IRepository<string, List<Triangle>> trnglRepository;
        IRepository<string, List<Tetrahedron>> tetrahedralRepository;

        ILoad load;
        IBoundaryCondition conditions;

        double forceValue = 1;



        private int _currentIndexlayer; //Индекс выбранного слоя из списка
        private readonly List<Layer> _layers; //Список слоев
        private FormShow _formShow; //Форма для просмотра двумерных слоев
        private List<MyTriangle> _triangles;
        private float _glZoom; //Текущий зум
        private readonly float _glZoomIncriment; //5.0f; //Инкремент зума
        private readonly float _glRotateAngleIncriment; //Инкремент угла поворота
        private float _rotX, _rotY, _rotZ; //Значение углов поворота вокруг осей X,Y,Z

        public Logic()
        {
            InitializeComponent();
            panelOpenGl.InitializeContexts();
            _currentIndexlayer = -1;
            _glZoom = -1300.0f;
            _glZoomIncriment = 50.0f;
            _glRotateAngleIncriment = 5.0f;
            _layers = new List<Layer>();
            _triangles = new List<MyTriangle>();
        }

        /// <summary>
        /// Загрузка формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logic_Load(object sender, EventArgs e)
        {
            Gl.glShadeModel(Gl.GL_SMOOTH); // Enable Smooth Shading
            Gl.glClearColor(0, 0, 0, 0.5f); // Black Background
            Gl.glClearDepth(1); // Depth Buffer Setup
            Gl.glEnable(Gl.GL_DEPTH_TEST); // Enables Depth Testing
            Gl.glDepthFunc(Gl.GL_LEQUAL); // The Type Of Depth Testing To Do
            Gl.glHint(Gl.GL_PERSPECTIVE_CORRECTION_HINT, Gl.GL_NICEST); // Really Nice Perspective Calculations

            // устанавливаем цвет очистки окна 
            Gl.glClearColor(0.9f, 0.9f, 0.9f, 1);

            // устанавливаем порт вывода, основываясь на размерах элемента управления AnT 
            Gl.glViewport(0, 0, panelOpenGl.Width, panelOpenGl.Height);

            // Reset The Current Viewport
            Gl.glMatrixMode(Gl.GL_PROJECTION); // Select The Projection Matrix
            Gl.glLoadIdentity(); // Reset The Projection Matrix
            //Glu.gluPerspective(45, panelOpenGl.Width / (double)panelOpenGl.Height, 0.1, 100);          // Calculate The Aspect Ratio Of The Window
            Glu.gluPerspective(45, panelOpenGl.Width / (double)panelOpenGl.Height, 0.1, 0);
            Gl.glMatrixMode(Gl.GL_MODELVIEW); // Select The Modelview Matrix
            Gl.glLoadIdentity();
        }

        #region InitMeshGeneration

        /// <summary>
        /// Построение сетки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Text = @"Выполняется построение модели...";
            toolStripProgressBar1.Value = 0;
            Refresh();

            float pw = Convert.ToSingle(_layers[0].Dicom.PixelSpacingValue[0], CultureInfo.InvariantCulture.NumberFormat);
            float ph = Convert.ToSingle(_layers[0].Dicom.PixelSpacingValue[1], CultureInfo.InvariantCulture.NumberFormat);

            Volume volume = new Volume(_layers,pw,ph);
            var depth =_layers[0].Dicom.pixelDepth;
            var space=_layers[0].Dicom.pixeSpace;
            var t = MCCube.getTriangles(volume);

            for(int k=0; k<t.Count-2;k+=3)
                _triangles.Add(new MyTriangle(t[k],t[k+1],t[k+2]));

            SystemSounds.Beep.Play();
            MessageBox.Show("Done!!!");
            toolStripStatusLabel1.Text = @"Реконструкция 3D модели позвонка завершена. Число треугольников:" + _triangles.Count.ToString();


            Stream myStream = new FileStream(@"Stl\Vertebras\st1\L1.stl", FileMode.Create);
            StlFile stl = new StlFile
            {
                SolidName = "my",
                Triangles = _triangles
            };
            stl.Save(myStream, true);
            myStream.Close();



            btn_render_start.Enabled = true;
            btn_render_stop.Enabled = true;
            сохранитьToolStripMenuItem.Enabled = true;
            button2.Enabled = true;
            Refresh();

        }

        #endregion

        #region DICOMHandler

        /// <summary>
        /// Усредняющий фильтр
        /// </summary>
        /// <param name="pixels16">Изображение (список пикселей)</param>
        /// <param name="width">Ширина изображения</param>
        /// <param name="height">Высота изображения</param>
        /// <param name="r">Радиус размытия</param>
        /// <returns></returns>
        private List<ushort> AveragingFilter(List<ushort> pixels16, int width, int height, int r)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int myCount = 0;
                    int color = 0;
                    for (int x = i - r; x < i + r; x++)
                    {
                        for (int y = j - r; y < j + r; y++)
                        {
                            if (x > 0 && x < width && y > 0 && y < height)
                            {
                                color += pixels16[x*width + y];
                                myCount++;
                            }
                        }
                    }
                    pixels16[i*width + j] = (ushort) (color/myCount);
                }
            }
            return pixels16;
        }

        /// <summary>
        /// Получение слоя. Считывание Dicom файла. Применение фильтров к изображению.
        /// </summary>
        /// <param name="file">Имя файла(путь)</param>
        /// <returns></returns>
        private Layer GetLayer(string file)
        {
            //Декодирование Dicom файла
            DicomDecoder dicom = new DicomDecoder {DicomFileName = file};
            Bitmap bitmap = new Bitmap(file +".jpg");

            Mat segmentJpg = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitmap);

            //Усредняющий фильтр
            dicom.Pixels16 = AveragingFilter(dicom.Pixels16Origin, dicom.width, dicom.height, 5);

            //Сегментация
            Mat origin = Mat.Zeros(new Size(dicom.width, dicom.height), MatType.CV_16UC1);
            Mat segment = Mat.Zeros(new Size(dicom.width, dicom.height), MatType.CV_16UC1);
            Mat laplace = Mat.Zeros(new Size(dicom.width, dicom.height), MatType.CV_16UC1);
            for (int w = 0; w < dicom.width; ++w)
            {
                for (int h = 0; h < dicom.height; ++h)
                {
                    segment.Set(w, h, dicom.Pixels16[w*dicom.height + h] >= 32900 ? (ushort) 40000 : (ushort) 10);
                    origin.Set(w, h, dicom.Pixels16[w*dicom.height + h]);
                }
            }

            //Обработка изображения оператором Лапласа
            //Cv2.Laplacian(segment, laplace, MatType.CV_16UC1);

            //Создание слоя
            return new Layer
            {
                Dicom = dicom, //Данные, полученные из Dicom файла
                //InstanceNumber = dicom.InstanceNumber,
                //SeriesNumber = dicom.SeriesNumber,
                Origin = origin, //Оригинальное изображение 
                SegmentMatrix = segment, //Сегментированное изображение
                SegmentMatrixJpg = segmentJpg
                //LaplaceMatrix = laplace //Контурное изображение
            };
        }

      

        /// <summary>
        /// Открытие 1-ого Dicom файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ОткрытьФайлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                if (openFileDialog1.FileName.Length > 0)
                {
                    toolStripStatusLabel1.Text = @"Загрузка КТ изображения " + openFileDialog1.FileName;
                    Refresh();
                    _layers.Add(GetLayer(openFileDialog1.FileName));
                    listBox1.Items.Add(openFileDialog1.FileName);
                }
                openFileDialog1.Dispose();
                groupBox2.Enabled = true;

            }
            toolStripStatusLabel1.Text = @"Загрузка КТ изображения завершена.";
            UpdateListBox();
            Refresh();

        }

        /// <summary>
        /// Открытие папки с Dicom файлами
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ОткрытьПапкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                int currentPosition = 1;
                var length = Directory.GetFiles(folderBrowserDialog1.SelectedPath).Length;
                toolStripStatusLabel1.Text = @"Загрузка КТ изображений...";
                Refresh();
                foreach (string file in Directory.GetFiles(folderBrowserDialog1.SelectedPath))
                {
                    toolStripStatusLabel1.Text = @"Загрузка " + file;
                    toolStripProgressBar1.Value = (int)((currentPosition / (decimal)length) * 100);
                    currentPosition++;
                    _layers.Add(GetLayer(file));
                    UpdateListBox();
                    Refresh();
                }
                toolStripStatusLabel1.Text = @"Загрузка КТ изображений завершена.";
                groupBox2.Enabled = true;
                Refresh();
            }
        }

        /// <summary>
        /// Обработка операций со списком ListBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBox1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (radioButton1.Checked)
            {
                _formShow = new FormShow();
                var drawArea = new Bitmap(_formShow.pictureBox1.Size.Width, _formShow.pictureBox1.Size.Height,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                _formShow.pictureBox1.Image = drawArea;

                int index = listBox1.IndexFromPoint(e.Location);
                _currentIndexlayer = index;
                Graphics g;
                g = Graphics.FromImage(drawArea);

                Brush gBrush = Brushes.Gray;
                Brush wBrush = Brushes.White;

                for (int w = 0; w < _layers[index].Dicom.width; ++w)
                {
                    for (int h = 0; h < _layers[index].Dicom.height; ++h)
                    {
                        g.FillRectangle(
                            _layers[index].Dicom.Pixels16[w * _layers[index].Dicom.height + h] >= 32900 ? gBrush : wBrush,
                            h, w, 1, 1);
                    }
                }

                _formShow.Text = _layers[index].Dicom.DicomFileName;
                _formShow.pictureBox1.Image = drawArea;
                g.Dispose();
                _formShow.Show();
            }
            if (radioButton3.Checked)
            {
                if (_currentIndexlayer == listBox1.SelectedIndex)
                    _currentIndexlayer = -1;
                _layers.RemoveAt(listBox1.SelectedIndex);
                UpdateListBox();
            }
        }

        /// <summary>
        /// Обновление списка ListBox
        /// </summary>
        private void UpdateListBox()
        {
            listBox1.Items.Clear();
            foreach (var layer in _layers)
            {
                listBox1.Items.Add(layer.Dicom.DicomFileName);
            }
        }

        #endregion

        #region Visualization 

        /// <summary>
        /// Отрисовка треугольника
        /// </summary>
        /// <param name="triangle">Треугольник</param>
        private void DrawTriangle(MyTriangle triangle)
        {
            Gl.glColor3f(0.1f, 0.1f, 0.1f);
            Gl.glVertex3f(triangle.Vertex1.X, triangle.Vertex1.Y, triangle.Vertex1.Z);

            Gl.glColor3f(0.55f, 0.55f, 0.55f);
            Gl.glVertex3f(triangle.Vertex2.X, triangle.Vertex2.Y, triangle.Vertex2.Z);

            Gl.glColor3f(0.85f, 0.85f, 0.85f);
            Gl.glVertex3f(triangle.Vertex3.X, triangle.Vertex3.Y, triangle.Vertex3.Z);
        }

        /// <summary>
        /// Метод отрисовки
        /// </summary>
        private void Render2()
        {
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT); // Clear Screen And Depth Buffer
            Gl.glLoadIdentity();
            Gl.glTranslatef(0.0f, 0, _glZoom);
            Gl.glRotatef(_rotX, 1, 0, 0);
            Gl.glRotatef(_rotY, 0, 1, 0);
            Gl.glRotatef(_rotZ, 0, 0, 1);

            //cam.Look();
            //GL_FRONT_AND_BACK

            Gl.glBegin(Gl.GL_LINES);
            Gl.glColor3f(1.0f, 0.0f, 0.0f);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(10000, 0, 0);
            Gl.glColor3f(0.0f, 1.0f, 0.0f);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 10000, 0);
            Gl.glColor3f(0.0f, 0.0f, 1.0f);
            Gl.glVertex3f(0, 0, 0);
            Gl.glVertex3f(0, 0, 10000);
            Gl.glEnd();

            Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);

            Gl.glBegin(Gl.GL_TRIANGLES);
            foreach (var t in _triangles)
            {
                DrawTriangle(t);
            }
            Gl.glEnd();
            Gl.glFlush();
            panelOpenGl.Invalidate();
        }

        /// <summary>
        /// Увеличение зума
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_zoom_plus_Click(object sender, EventArgs e)
        {
            ZoomMethod(true);
        }

        /// <summary>
        /// Уменьшение зума
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_zoom_minus_Click(object sender, EventArgs e)
        {
            ZoomMethod(false);
        }

        /// <summary>
        /// Метод изменения зума
        /// </summary>
        /// <param name="isAdd"></param>
        private void ZoomMethod(bool isAdd)
        {
            if (isAdd)
                _glZoom += _glZoomIncriment;
            else
                _glZoom -= _glZoomIncriment;
        }

        /// <summary>
        /// Отрисовка по таймеру
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RenderTimer_Tick(object sender, EventArgs e)
        {
            //Render();
            Render2();
        }

        /// <summary>
        /// Запуск рендеринга
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_render_start_Click(object sender, EventArgs e)
        {
            btn_zoom_plus.Enabled = true;
            btn_zoom_minus.Enabled = true;
            btn_rotate_plus.Enabled = true;
            btn_rotate_minus.Enabled = true;

            cb_X.Enabled = true;
            cb_Y.Enabled = true;
            cb_Z.Enabled = true;
            renderTimer.Start();

        }

        /// <summary>
        /// Остановка рендеринга
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_render_stop_Click(object sender, EventArgs e)
        {
            btn_zoom_plus.Enabled = false;
            btn_zoom_minus.Enabled = false;
            btn_rotate_plus.Enabled = false;
            btn_rotate_minus.Enabled = false;

            cb_X.Enabled = false;
            cb_Y.Enabled = false;
            cb_Z.Enabled = false;
            renderTimer.Stop();
        }

        /// <summary>
        /// Поворот по часовой стрелке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_rotate_plus_Click(object sender, EventArgs e)
        {
            if (cb_X.Checked)
                _rotX += _glRotateAngleIncriment;
            if (cb_Y.Checked)
                _rotY += _glRotateAngleIncriment;
            if (cb_Z.Checked)
                _rotZ += _glRotateAngleIncriment;
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            //trnglRepository = new StlTriangularRepository<string>();
            tetrahedralRepository = new StlTetrahedralRepository<string>();

            List<List<Triangle>> vertebras = new List<List<Triangle>>();
            List<Triangle> myvertebra = new List<Triangle>();
            myvertebra = ReadVertebra(1, "Vertebras/st1/");

            vertebras.Add(myvertebra);

            //vertebras.Add(ReadVertebra(1, "Vertebras/st2/"));
            //int countVertebras = 5;
            //for (int i = 2; i <= countVertebras; i++)
            //{
            //    vertebras.Add(ReadVertebra(i, "Vertebras/st2/"));
            //}

            List<Triangle> allVertebras = new List<Triangle>();
            vertebras.ForEach(trngls => allVertebras.AddRange(trngls));

            double shiftX = Math.Abs(allVertebras.Min(tngl => tngl.Center.X));
            double shiftY = Math.Abs(allVertebras.Min(tngl => tngl.Center.Y));
            double shiftZ = Math.Abs(allVertebras.Min(tngl => tngl.Center.Z));

            double minX = allVertebras.Min(tngl => tngl.Center.X);
            double minY = allVertebras.Min(tngl => tngl.Center.Y);
            double minZ = allVertebras.Min(tngl => tngl.Center.Z);
            double maxX = allVertebras.Max(tngl => tngl.Center.X);
            double maxY = allVertebras.Max(tngl => tngl.Center.Y);
            double maxZ = allVertebras.Max(tngl => tngl.Center.Z);
            double avX = (minX + maxX) / 2.0;
            double avY = (minY + maxY) / 2.0;
            double lenght = maxX - minX;
            double width = maxY - minY;
            width = (lenght > width) ? lenght : width;
            double height = maxZ - minZ;

            ShiftModel(ref vertebras, shiftX, shiftY, shiftZ);
            //ShiftModel(ref disks, shiftX, shiftY, shiftZ);

            toolStripStatusLabel1.Text = $"Генерация конечно-элементной сетки...";
            this.Refresh();

            FeModel scene = GenerateTetrahedralModel(width, height + STEP_HEIGHT, STEP_WIDTH, STEP_HEIGHT, VERTEBRA_MATERIAL_ID);
            model = GetGeneralModelFromScene(scene, vertebras);
            var aaaaaa = model.Triangles;
            var bbbbbb = model.Tetrahedrons;

            //load = new Force(SelectedSide.TOP, new Node((int)avX, (int)avY, maxZ - 10), forceValue, true, model.Triangles);

            toolStripStatusLabel1.Text = $"Учет граничных условий и внешних нагрузок...";
            this.Refresh();

            //load = new Force(SelectedSide.TOP, new Node((int)avY, (int)avX, maxZ - 10), forceValue, true, model.Triangles);
            //load = new Pressure(SelectedSide.TOP, new Node((int)avX, (int)avY, maxZ - 10), forceValue, true, model.Triangles);
            //load = new ConcentratedForce(SelectedSide.TOP, forceValue, true, model.Nodes, height / 10.0);
            load = new Force(SelectedSide.TOP, forceValue, true, model.Nodes, height / 18.0);
            conditions = new VolumeBoundaryCondition(SelectedSide.BOTTOM, model.Nodes, height / 20.0);
            //conditions = new VolumeBoundaryCondition(SelectedSide.BOTTOM, new Node((int)avX, (int)avY, minZ), model.Triangles);

            int concentratedIndex = load.LoadVectors.FirstOrDefault().Key;
            int step = (int)(STEP_HEIGHT / 4.0);
            Node tmpNode = model.Nodes.FirstOrDefault(nd => nd.GlobalIndex == concentratedIndex);
            List<Node> nearNodes = new List<Node>(model.Nodes.Where(nd =>
                (nd.X > model.Nodes[concentratedIndex].X - step && nd.X < model.Nodes[concentratedIndex].X + step)
                && (nd.Y > model.Nodes[concentratedIndex].Y - step && nd.Y < model.Nodes[concentratedIndex].Y + step)
                && (nd.Z > model.Nodes[concentratedIndex].Z - step && nd.Z < model.Nodes[concentratedIndex].Z + step))
                .ToList());
            nearNodes.Remove(tmpNode);

            concentratedIndex = TrueIndexOfCenter(concentratedIndex, nearNodes, 0);
            if (concentratedIndex != load.LoadVectors.FirstOrDefault().Key)
            {
                LoadVector vector = new LoadVector(load.LoadVectors.FirstOrDefault().Value.Value, VectorDirection.Z);
                load.LoadVectors.Clear();
                load.LoadVectors.Add(concentratedIndex, vector);
            }

            tetrahedralRepository.Create(model.Id + "in", model.Tetrahedrons);
            //trnglRepository.Create(model.Id + "in", model.Triangles);
            //trnglRepository.Create(model.Id + "load", ((Force)load).LoadedTriangles);
            //trnglRepository.Create(model.Id + "fix", ((VolumeBoundaryCondition)conditions).FixedTriangles);



            solver = new StressStrainSparseMatrix(model);
            solution = new StaticMechanicSparseSolution(solver, model);

            var begin = DateTime.Now;
            solution.Solve(TypeOfFixation.RIGID, conditions, load);

            TimeSpan endSolve = DateTime.Now - begin;
            double[] results = solution.Results;

            toolStripStatusLabel1.Text = $"Расчет завершен.";
            this.Refresh();

            using (StreamWriter writer = new StreamWriter("results.txt"))
            {
                writer.WriteLine($"Total time solving SLAE: {endSolve.TotalSeconds} sec.");
                writer.WriteLine();
                double max = solution.Results[2];
                for (int i = 2; i < solution.Results.Length; i += 3)
                {
                    if (Math.Abs(solution.Results[i]) > Math.Abs(max))
                    {
                        max = solution.Results[i];
                    }
                }

                writer.WriteLine($"Max deformation: {max} mm.");
                writer.WriteLine();

                for (int i = 0; i < solution.Results.Length; i++)
                {
                    writer.WriteLine(solution.Results[i]);
                }
            }

            TotalEpure(model.Nodes, solution.Results, "TotalEpureSpine");

            List<Tetrahedron> outList = ApplyResultsToTetrahedrons(results);
            tetrahedralRepository.Create(model.Id + "out", outList);
            tetrahedralRepository.Create2(model.Id + "out2", outList);

            MessageBox.Show($"Total time solving SLAE: {endSolve.TotalSeconds} sec.");

            Process.Start("notepad.exe", "results.txt");
        }

        /// <summary>
        /// Поворот против часовой стрелке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_rotate_minus_Click(object sender, EventArgs e)
        {
            if (cb_X.Checked)
                _rotX -= _glRotateAngleIncriment;
            if (cb_Y.Checked)
                _rotY -= _glRotateAngleIncriment;
            if (cb_Z.Checked)
                _rotZ -= _glRotateAngleIncriment;
        }

        #endregion

        #region FileWriters  

        /// <summary>
        /// Сохранение файлов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void СохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btn_zoom_plus.Enabled = false;
            btn_zoom_minus.Enabled = false;
            btn_rotate_plus.Enabled = false;
            btn_rotate_minus.Enabled = false;
            cb_X.Enabled = false;
            cb_Y.Enabled = false;
            cb_Z.Enabled = false;
            renderTimer.Stop();

            saveFileDialog1 = new SaveFileDialog
            {
                Filter = @"Stl Files (*.stl)|*.stl|Mesh Files (*.mesh)|*.mesh|GMesh Files (*.msh)|*.msh",
                //Filter = @"Stl Files (*.stl)|*.stl|Mesh Files (*.mesh)|*.mesh",
                FilterIndex = 1,
                RestoreDirectory = true
            };


            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Stream myStream = saveFileDialog1.OpenFile();
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        StlFile stl = new StlFile
                        {
                            SolidName = "my",
                            Triangles = _triangles
                        };
                        stl.Save(myStream, false);
                        break;
                    case 2:
                        //MeshWriter(new StreamWriter(myStream));
                        break;
                    case 3:
                        //GMeshWriter(new StreamWriter(myStream));
                        break;
                }
                myStream.Close();
                System.Media.SystemSounds.Beep.Play();
                MessageBox.Show(@"Файл сохранен!");
            }
        }
        #endregion

        private List<Triangle> ApplyResultsToTrianglesList(double[] results)
        {
            List<double> formax = new List<double>();
            List<Triangle> list = new List<Triangle>();
            StreamWriter wt1 = new StreamWriter("test.color");
            double max1 = 0;

            foreach (var item in model.Triangles)
            {
                Triangle tmp = new Triangle(item);

                tmp.Nodes.ForEach(n =>
                {
                    n.X += results[n.GlobalIndex * 3];
                    n.Y += results[n.GlobalIndex * 3 + 1];
                    n.Z += results[n.GlobalIndex * 3 + 2];
                    double z = Math.Abs(solution.Results[n.GlobalIndex * 3 + 2]);
                    n.DefColor = z;
                    formax.Add(z);
                    //double temp = (Math.Abs(solution.Results[n.GlobalIndex * 3]) + Math.Abs(solution.Results[n.GlobalIndex * 3+1]) + Math.Abs(solution.Results[n.GlobalIndex * 3+2]))/3;
                    //wt1.WriteLine("f " + Math.Abs(solution.Results[n.GlobalIndex * 3 + 2]));
                });
                list.Add(tmp);
            }
            max1 = formax.Max();
            wt1.WriteLine($"{max1}");
            wt1.Close();
            return list;
        }

        private List<Tetrahedron> ApplyResultsToTetrahedrons(double[] results)
        {
            List<double> formax = new List<double>();
            List<Tetrahedron> list = new List<Tetrahedron>();
            StreamWriter wt1 = new StreamWriter("test.color");
            double max1 = 0;

            foreach (var item in model.Tetrahedrons)
            {
                Tetrahedron tmp = new Tetrahedron(item);

                tmp.Nodes.ForEach(n =>
                {
                    n.X += results[n.GlobalIndex * 3];
                    n.Y += results[n.GlobalIndex * 3 + 1];
                    n.Z += results[n.GlobalIndex * 3 + 2];
                    double z = Math.Abs(solution.Results[n.GlobalIndex * 3 + 2]);
                    n.DefColor = z;
                    formax.Add(z);
                });
                list.Add(tmp);
            }
            max1 = formax.Max();
            wt1.WriteLine($"{max1}");
            wt1.Close();
            return list;
        }

        private List<Triangle> ReadVertebra(int vertebraNum, string basePath = "")
        {
            IRepository<string, List<Triangle>> repository = new StlTriangularRepository<string>();
            List<Triangle> triangles = repository.Read($"{basePath}L{vertebraNum}");

            return triangles;
        }

        private FeModel GenerateTetrahedralModel(double width, double height, double stepWidth, double stepHeight, int materialId)
        {
            IScene scene = new TetrahedralScene(width, height, stepWidth, stepHeight);
            scene.Initialize();

            FeModel feModel = new FeModel(scene.Nodes, new List<Triangle>(), scene.Tetrahedrons);

            foreach (var node in feModel.Nodes)
            {
                node.IdMaterial = materialId;
            }
            feModel.Tetrahedrons.AsParallel().ForAll(tn =>
            {
                tn.Nodes.ForEach(node => node.IdMaterial = materialId);
            });
            return feModel;
        }

        private void ShiftModel(ref List<List<Triangle>> stlModel, double shiftX, double shiftY, double shiftZ)
        {
            for (int i = 0; i < stlModel.Count; i++)
            {
                for (int j = 0; j < stlModel[i].Count; j++)
                {
                    for (int k = 0; k < stlModel[i][j].Nodes.Count; k++)
                    {
                        stlModel[i][j].Nodes[k].X += shiftX;
                        stlModel[i][j].Nodes[k].Y += shiftY;
                        stlModel[i][j].Nodes[k].Z += shiftZ;
                    }
                }
            }
        }

        private FeModel GetGeneralModelFromScene(FeModel scene, List<List<Triangle>> vertebras)
        {
            FeModel feModel = null;
            List<Node> nodes = new List<Node>();
            List<Tetrahedron> tetrahedrons = new List<Tetrahedron>();

            scene.Tetrahedrons.AsParallel().ForAll(tetrahedron =>
            {
                if (IsInsideStlArea(vertebras, tetrahedron))
                {
                    tetrahedrons.Add(tetrahedron);
                }
            });
            scene.Nodes.ForEach(node =>
            {
                bool isHave = false;
                for (int i = 0; i < tetrahedrons.Count; i++)
                {
                    tetrahedrons[i].Nodes.ForEach(nd =>
                    {
                        if (nd.GlobalIndex == node.GlobalIndex)
                        {
                            nodes.Add(nd);
                            isHave = true;
                        }
                    });
                    if (isHave)
                    {
                        break;
                    }
                }
            });

            tetrahedrons.AsParallel().ForAll(tn =>
            {
                for (int i = 0; i < tn.Nodes.Count; i++)
                {
                    tn.Nodes[i] = nodes.FirstOrDefault(nd => nd.GlobalIndex == tn.Nodes[i].GlobalIndex);
                }
            });

            Parallel.For(0, nodes.Count, counter =>
            {
                nodes[counter].GlobalIndex = counter;
            });

            feModel = new FeModel(nodes, tetrahedrons);

            return feModel;
        }

        private bool IsInsideStlArea(List<List<Triangle>> stlModel, Tetrahedron tetrahedron)
        {
            List<Triangle> xy = new List<Triangle>();
            foreach (var ml in stlModel)
            {
                var trngls = ml.Where(trngl => trngl.IsInTriangleXY(tetrahedron.Center))
                    .OrderBy(trngl => trngl.Center.Z).ToList();
                if (trngls.Count % 2 == 0) xy.AddRange(trngls);
            }

            List<Triangle> xz = new List<Triangle>();
            foreach (var ml in stlModel)
            {
                var trngls = ml.Where(trngl => trngl.IsInTriangleXZ(tetrahedron.Center))
                    .OrderBy(trngl => trngl.Center.Y)
                    .ToList();
                if (trngls.Count % 2 == 0) xz.AddRange(trngls);
            }

            List<Triangle> yz = new List<Triangle>();
            foreach (var ml in stlModel)
            {
                var trngls = ml.Where(trngl => trngl.IsInTriangleYZ(tetrahedron.Center))
                    .OrderBy(trngl => trngl.Center.X)
                    .ToList();
                if (trngls.Count % 2 == 0) yz.AddRange(trngls);
            }
            xy.RemoveAll(trngl => trngl is null);
            xz.RemoveAll(trngl => trngl is null);
            yz.RemoveAll(trngl => trngl is null);

            return IsBetweenTriangles(xy, tetrahedron, Direction.Z)
                && IsBetweenTriangles(xz, tetrahedron, Direction.Y)
                && IsBetweenTriangles(yz, tetrahedron, Direction.X);
        }

        private bool IsBetweenTriangles(List<Triangle> triangles, Tetrahedron tetrahedron, Direction direction)
        {
            for (int i = 0; i < triangles.Count; i += 2)
            {
                switch (direction)
                {
                    case Direction.X:
                        if (tetrahedron.Center.X >= triangles[i].Center.X && tetrahedron.Center.X <= triangles[i + 1].Center.X)
                        {
                            return true;
                        }
                        break;
                    case Direction.Y:
                        if (tetrahedron.Center.Y >= triangles[i].Center.Y && tetrahedron.Center.Y <= triangles[i + 1].Center.Y)
                        {
                            return true;
                        }
                        break;
                    case Direction.Z:
                        if (tetrahedron.Center.Z >= triangles[i].Center.Z && tetrahedron.Center.Z <= triangles[i + 1].Center.Z)
                        {
                            return true;
                        }
                        break;
                    default:
                        throw new Exception("Wrong direction.");
                }
            }
            return false;
        }

        private void TotalEpure(List<Node> epureNodes, double[] results, string outFileName)
        {
            using (StreamWriter writer = new StreamWriter($"{outFileName}.txt"))
            {
                writer.WriteLine("{0, 5}|{1, 10}|{2, 10}|{3, 10}|{4, 10}|{5, 10}|{6, 10}", "Num", "X", "Y", "Z", "DX", "DY", "DZ");
                for (int i = 0; i < epureNodes.Count; i++)
                {
                    writer.Write("{0, 5}|{1, 10:f3}|{2, 10:f3}|{3, 10:f3}", i, epureNodes[i].X, epureNodes[i].Y, epureNodes[i].Z);
                    writer.WriteLine("|{0, 25:f20}|{1, 25:f20}|{2, 25:f20}|", results[i * DEGREES_OF_FREEDOM], results[i * DEGREES_OF_FREEDOM + 1], results[i * DEGREES_OF_FREEDOM + 2]);
                }
            }
        }

        private double Distance(Node first, Node second)
        {
            return Math.Sqrt(Math.Pow(second.X - first.X, 2) + Math.Pow(second.Y - first.Y, 2) + Math.Pow(second.Z - first.Z, 2));
        }

        private int TrueIndexOfCenter(int currentIndex, List<Node> nearNodes, int currentNodeIndex)
        {
            bool isInTetrahedron = false;
            model.Tetrahedrons.AsParallel().ForAll(tetrahedron =>
            {
                tetrahedron.Nodes.ForEach(nd =>
                {
                    if (nd.GlobalIndex == currentIndex)
                    {
                        isInTetrahedron = true;
                    }
                });
            });
            if (!isInTetrahedron)
            {
                if (currentNodeIndex < nearNodes.Count)
                {
                    currentIndex = nearNodes[currentNodeIndex].GlobalIndex;
                    TrueIndexOfCenter(currentIndex, nearNodes, currentNodeIndex + 1);
                }
                else
                {
                    return -1;
                }
            }

            return currentIndex;
        }

    }
}