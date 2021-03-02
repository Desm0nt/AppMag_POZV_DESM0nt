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
    using System.Drawing.Imaging;
    using STL_Tools;
    using OpenTK.Graphics.OpenGL;
    using BatuGL;
    using Mouse_Orbit;


    public partial class Logic : Form
    {
        private const int DEGREES_OF_FREEDOM = 3;
        private const int VERTEBRA_MATERIAL_ID = 2;
        private const int INNERVERTEBRAL_DISK_MATERIAL_ID = 3;
        private const double STEP_WIDTH = 23.75; // step of the tetraherdral model by width
        private const double STEP_HEIGHT = 30; // step of the tetraherdral model by height

        string lastname = "";
        string lastname_fullpath = "";

        FeModel model;
        ISolution solution;
        ISolve<SparseMatrix> solver;
        //IRepository<string, List<Triangle>> trnglRepository;
        IRepository<string, List<Tetrahedron>> tetrahedralRepository;
        IRepository2<string, List<MeshGenerator.Elements.Triangle>, List<Node>> trinagleRepository;

        ILoad load;
        IBoundaryCondition conditions;

        double forceValue = 10;

        bool monitorLoaded = false;
        bool moveForm = false;
        int moveOffsetX = 0;
        int moveOffsetY = 0;
        Batu_GL.VAO_TRIANGLES modelVAO = null; // 3d model vertex array object
        private Orbiter orb;
        Vector3 minPos = new Vector3();
        Vector3 maxPos = new Vector3();

        private int _currentIndexlayer; //Индекс выбранного слоя из списка
        private readonly List<Layer> _layers; //Список слоев
        private FormShow _formShow; //Форма для просмотра двумерных слоев
        private List<MyTriangle> _triangles;

        public Logic()
        {
            InitializeComponent();
            //panelOpenGl.InitializeContexts();
            _currentIndexlayer = -1;
            _layers = new List<Layer>();
            _triangles = new List<MyTriangle>();
            orb = new Orbiter();
            GL_Monitor1.MouseDown += orb.Control_MouseDownEvent;
            GL_Monitor1.MouseUp += orb.Control_MouseUpEvent;
            GL_Monitor1.MouseWheel += orb.Control_MouseWheelEvent;
            GL_Monitor1.KeyPress += orb.Control_KeyPress_Event;
            button2.Enabled = true;
        }

        /// <summary>
        /// Загрузка формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Logic_Load(object sender, EventArgs e)
        {
            Batu_GL.Configure(GL_Monitor1, Batu_GL.Ortho_Mode.CENTER);
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

            Volume volume = new Volume(_layers, pw, ph);
            var depth = _layers[0].Dicom.pixelDepth;
            var space = _layers[0].Dicom.pixeSpace;
            var t = MCCube.getTriangles(volume);
            int koeff = 18;
            for (int k = 0; k < t.Count - 2; k += 3)
            {
                var trg = new MyTriangle(t[k], t[k + 1], t[k + 2]);
                Vertex vrt1 = new Vertex(t[k].X * koeff, t[k].Z * koeff * (-1), t[k].Y * koeff);
                Vertex vrt2 = new Vertex(t[k + 1].X * koeff, t[k + 1].Z * koeff * (-1), t[k + 1].Y * koeff);
                Vertex vrt3 = new Vertex(t[k + 2].X * koeff, t[k + 2].Z * koeff * (-1), t[k + 2].Y * koeff);
                var trg2 = new MyTriangle(vrt1, vrt2, vrt3);
                _triangles.Add(trg2);
            }

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
                                color += pixels16[x * width + y];
                                myCount++;
                            }
                        }
                    }
                    pixels16[i * width + j] = (ushort)(color / myCount);
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
            DicomDecoder dicom = new DicomDecoder { DicomFileName = file };

            //Усредняющий фильтр
            dicom.Pixels16 = AveragingFilter(dicom.Pixels16Origin, dicom.width, dicom.height, 5);

            GenImage(file, dicom);

            var filepath = Path.GetDirectoryName(file);
            var filename = Path.GetFileNameWithoutExtension(file);
            Bitmap bitmap = new Bitmap(filepath + "\\png\\" + filename + ".png");

            //Создание слоя
            return new Layer
            {
                Dicom = dicom, //Данные, полученные из Dicom файла
                ImageFromJpg = bitmap
            };

        }

        private void GenImage(string path, DicomDecoder dicom)
        {

            _formShow = new FormShow();
            var drawArea = new Bitmap(_formShow.pictureBox1.Size.Width, _formShow.pictureBox1.Size.Height,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            //_formShow.pictureBox1.Image = drawArea;

            int index = 0;
            string pathtext = path;
            _currentIndexlayer = index;
            Graphics g;
            g = Graphics.FromImage(drawArea);

            Brush gBrush = Brushes.White;
            Brush wBrush = Brushes.Black;

            for (int w = 0; w < dicom.width; ++w)
            {
                for (int h = 0; h < dicom.height; ++h)
                {
                    g.FillRectangle(
                        dicom.Pixels16[w * dicom.height + h] >= 32900 ? gBrush : wBrush,
                        h, w, 1, 1);
                }
            }

            int a = 8;
            _formShow.Text = dicom.DicomFileName;
            //_formShow.pictureBox1.Image = drawArea;
            var filepath = Path.GetDirectoryName(pathtext);
            var filename = Path.GetFileNameWithoutExtension(pathtext);
            //Bitmap bitmap = new Bitmap(filepath + "\\jpg\\" + filename + ".jpg");
            var resized = ResizeBitmap(drawArea, drawArea.Width / a, drawArea.Height / a);
            Directory.CreateDirectory(filepath + "\\png\\");
            resized.Save(filepath + "\\png\\" + filename + ".png", ImageFormat.Png);
            g.Dispose();

        }

        private static Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
                g.DrawImage(sourceBMP, 0, 0, width, height);
            return result;
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
                foreach (string file in Directory.GetFiles(folderBrowserDialog1.SelectedPath))
                {
                    toolStripStatusLabel1.Text = @"Загрузка " + file;
                    toolStripProgressBar1.Value = (int)((currentPosition / (decimal)length) * 100);
                    currentPosition++;
                    _layers.Add(GetLayer(file));
                    Refresh();
                }
                UpdateListBox();
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
                string pathtext = listBox1.GetItemText(listBox1.SelectedItem);
                _currentIndexlayer = index;
                Graphics g;
                g = Graphics.FromImage(drawArea);

                Brush gBrush = Brushes.White;
                Brush wBrush = Brushes.Black;

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
                drawArea.Save(pathtext + ".jpg", ImageFormat.Jpeg);
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

        private void button3_Click(object sender, EventArgs e)
        {
            int i = 0;
            foreach (var a in listBox1.Items)
            {
                _formShow = new FormShow();
                var drawArea = new Bitmap(_formShow.pictureBox1.Size.Width, _formShow.pictureBox1.Size.Height,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                //_formShow.pictureBox1.Image = drawArea;

                int index = i;
                string pathtext = listBox1.GetItemText(a);
                _currentIndexlayer = index;
                Graphics g;
                g = Graphics.FromImage(drawArea);

                Brush gBrush = Brushes.White;
                Brush wBrush = Brushes.Black;

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
                //_formShow.pictureBox1.Image = drawArea;
                var filepath = Path.GetDirectoryName(pathtext);
                var filename = Path.GetFileNameWithoutExtension(pathtext);
                Bitmap bitmap = new Bitmap(filepath + "\\jpg\\" + filename + ".jpg");
                drawArea.Save(filepath + "\\jpg\\" + filename + ".jpg", ImageFormat.Jpeg);
                g.Dispose();
                i++;
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

        private void DrawTimer_Tick(object sender, EventArgs e)
        {
            orb.UpdateOrbiter(MousePosition.X, MousePosition.Y);
            GL_Monitor1.Invalidate();
            if (moveForm)
            {
                this.SetDesktopLocation(MousePosition.X - moveOffsetX, MousePosition.Y - moveOffsetY);
            }
        }

        private void GL_Monitor_Load(object sender, EventArgs e)
        {
            GL_Monitor1.AllowDrop = true;
            monitorLoaded = true;
            GL.ClearColor(Color.Black);
        }

        private void ConfigureBasicLighting(Color modelColor)
        {
            float[] light_1 = new float[] {
            0.2f * modelColor.R / 255.0f,
            0.2f * modelColor.G / 255.0f,
            0.2f * modelColor.B / 255.0f,
            1.0f };
            float[] light_2 = new float[] {
            3.0f * modelColor.R / 255.0f,
            3.0f * modelColor.G / 255.0f,
            3.0f * modelColor.B / 255.0f,
            1.0f };
            float[] specref = new float[] {
                0.01f * modelColor.R / 255.0f,
                0.01f * modelColor.G / 255.0f,
                0.01f * modelColor.B / 255.0f,
                1.0f };
            float[] specular_0 = new float[] { -1.0f, -1.0f, 1.0f, 1.0f };
            float[] specular_1 = new float[] { 1.0f, -1.0f, 1.0f, 1.0f };
            float[] lightPos_0 = new float[] { 1000f, 1000f, -200.0f, 1.0f };
            float[] lightPos_1 = new float[] { -1000f, 1000f, -200.0f, 1.0f };

            GL.Enable(EnableCap.Lighting);
            /* light 0 */
            GL.Light(LightName.Light0, LightParameter.Ambient, light_1);
            GL.Light(LightName.Light0, LightParameter.Diffuse, light_2);
            GL.Light(LightName.Light0, LightParameter.Specular, specular_0);
            GL.Light(LightName.Light0, LightParameter.Position, lightPos_0);
            GL.Enable(EnableCap.Light0);
            /* light 1 */
            GL.Light(LightName.Light1, LightParameter.Ambient, light_1);
            GL.Light(LightName.Light1, LightParameter.Diffuse, light_2);
            GL.Light(LightName.Light1, LightParameter.Specular, specular_1);
            GL.Light(LightName.Light1, LightParameter.Position, lightPos_1);
            GL.Enable(EnableCap.Light1);
            /*material settings  */
            GL.Enable(EnableCap.ColorMaterial);
            GL.ColorMaterial(MaterialFace.Front, ColorMaterialParameter.AmbientAndDiffuse);
            GL.Material(MaterialFace.Front, MaterialParameter.Specular, specref);
            GL.Material(MaterialFace.Front, MaterialParameter.Shininess, 10);
            GL.Enable(EnableCap.Normalize);
        }

        private void GL_Monitor_Paint(object sender, PaintEventArgs e)
        {
            if (!monitorLoaded)
                return;

            Batu_GL.Configure(GL_Monitor1, Batu_GL.Ortho_Mode.CENTER);
            if (modelVAO != null) ConfigureBasicLighting(modelVAO.color);
            GL.Translate(orb.PanX, orb.PanY, 0);
            GL.Rotate(orb.orbitStr.angle, orb.orbitStr.ox, orb.orbitStr.oy, orb.orbitStr.oz);
            GL.Scale(orb.scaleVal, orb.scaleVal, orb.scaleVal);
            GL.Translate(-minPos.x, -minPos.y, -minPos.z);
            GL.Translate(-(maxPos.x - minPos.x) / 2.0f, -(maxPos.y - minPos.y) / 2.0f, -(maxPos.z - minPos.z) / 2.0f);
            if (modelVAO != null) modelVAO.Draw();

            GL_Monitor1.SwapBuffers();
        }

        private void ReadSelectedFile(string fileName)
        {
            STLReader stlReader = new STLReader(fileName);
            TriangleMesh[] meshArray = stlReader.ReadFile();
            modelVAO = new Batu_GL.VAO_TRIANGLES();
            modelVAO.parameterArray = STLExport.Get_Mesh_Vertices(meshArray);
            modelVAO.normalArray = STLExport.Get_Mesh_Normals(meshArray);
            modelVAO.color = Color.Beige;
            minPos = stlReader.GetMinMeshPosition(meshArray);
            maxPos = stlReader.GetMaxMeshPosition(meshArray);
            orb.Reset_Orientation();
            orb.Reset_Pan();
            orb.Reset_Scale();
            if (stlReader.Get_Process_Error())
            {
                modelVAO = null;
                /* if there is an error, deinitialize the gl monitor to clear the screen */
                Batu_GL.Configure(GL_Monitor1, Batu_GL.Ortho_Mode.CENTER);
                GL_Monitor1.SwapBuffers();
            }
        }

        private void GL_Monitor_DragDrop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);
            if (data != null)
            {
                string[] fileNames = data as string[];
                string ext = System.IO.Path.GetExtension(fileNames[0]);
                if (fileNames.Length > 0 && (ext == ".stl" || ext == ".STL" || ext == ".txt" || ext == ".TXT"))
                {
                    ReadSelectedFile(fileNames[0]);
                }
            }
        }

        private void GL_Monitor_DragEnter(object sender, DragEventArgs e)
        {
            // if the extension is not *.txt or *.stl change drag drop effect symbol
            var data = e.Data.GetData(DataFormats.FileDrop);
            if (data != null)
            {
                string[] fileNames = data as string[];
                string ext = System.IO.Path.GetExtension(fileNames[0]);
                if (ext == ".stl" || ext == ".STL" || ext == ".txt" || ext == ".TXT")
                {
                    e.Effect = DragDropEffects.Copy;
                }
                else
                {
                    e.Effect = DragDropEffects.None;
                }
            }
        }



        /// <summary>
        /// Запуск рендеринга
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_render_start_Click(object sender, EventArgs e)
        {
            ReadSelectedFile(Environment.CurrentDirectory + @"\Stl\Vertebras\st1\L1.stl");
        }

        /// <summary>
        /// Остановка рендеринга
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_render_stop_Click(object sender, EventArgs e)
        {
            modelVAO = null;
            /* if there is an error, deinitialize the gl monitor to clear the screen */
            Batu_GL.Configure(GL_Monitor1, Batu_GL.Ortho_Mode.CENTER);
            GL_Monitor1.SwapBuffers();
        }
        #endregion

        private void Button2_Click(object sender, EventArgs e)
        {
            //trnglRepository = new StlTriangularRepository<string>();
            tetrahedralRepository = new StlTetrahedralRepository<string>();
            trinagleRepository = new StlTriangularRepository2<string>();

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
            tetrahedralRepository.Create(model.Id + "in", model.Tetrahedrons);

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
            List<Node> nodlist = ApplyResultsToGenList(results);
            tetrahedralRepository.Create(model.Id + "out", outList);
            tetrahedralRepository.Create2(model.Id + "out2", outList);
            trinagleRepository.Create2(model.Id + "out3", myvertebra, nodlist);

            lastname = model.Id + "out3.stl";
            lastname_fullpath = Environment.CurrentDirectory + "\\" + lastname;

            MessageBox.Show($"Total time solving SLAE: {endSolve.TotalSeconds} sec.");

            //Process.Start("notepad.exe", "results.txt");
        }
        private List<Node> ApplyResultsToGenList(double[] results)
        {
            string workpath = Environment.CurrentDirectory;
            List<Node> nodlist = new List<Node>();

            nodlist = model.Nodes;
            nodlist.ForEach(n =>
            {
                double z = Math.Abs(solution.Results[n.GlobalIndex * 3 + 2]);
                n.DefColor = z;
            });

            return nodlist;
        }


        #region FileWriters  

        /// <summary>
        /// Сохранение файлов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void СохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            renderTimer.Stop();
            saveFileDialog1 = new SaveFileDialog
            {
                Filter = @"Stl Files (*.stl)|*.stl|Mesh Files (*.mesh)|*.mesh|GMesh Files (*.msh)|*.msh",
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

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string aaa = @"e:\123\AppMag\App\bin\Debug\0a72a763-9f46-4db0-954f-20e2a9cd4c87out3.stl";
            // string aaa = lastname_fullpath;
            AppMainForm aa = new AppMainForm();
            aa.ShowDialog();
            //ResultView2 resultView2 = new ResultView2(aaa);
            //resultView2.Show();
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