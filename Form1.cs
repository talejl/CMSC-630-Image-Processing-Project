using System.Diagnostics;
using Talej_Image_Processor_CMSC_630.Properties;

namespace Talej_Image_Processor_CMSC_630
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

            textBox1.Text = Settings.Default.InputPath;
            textBox2.Text = Settings.Default.OutputPath;
            textBox1.ReadOnly = true;
            textBox2.ReadOnly = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openImage  = new OpenFileDialog();
            openImage.Filter = "Image File (*.bmp) | *.bmp";

            if (DialogResult.OK == openImage.ShowDialog())
            {
                this.Original.Image = ImageProcessing.ResizeImage(new Bitmap(openImage.FileName),288,219);

            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var grayscaletimer = new Stopwatch();
            var impulsetimer = new Stopwatch();
            var histogramtimer = new Stopwatch();

            Bitmap copy = new Bitmap(this.Original.Image);

            grayscaletimer.Start();
            Bitmap modifiedImage = ImageProcessing.RGB2GrayscaleImage(copy);
            grayscaletimer.Stop();
            impulsetimer.Start();
            Bitmap ImpulseNoiseImage = ImageProcessing.ImpulseNoise(modifiedImage, impulsenoiseintensityslider.Value);
            impulsetimer.Stop();
            this.Modified.Image = ImageProcessing.ResizeImage(ImpulseNoiseImage, 288, 219);
            histogramtimer.Start();
            Bitmap histogramimage = ImageProcessing.GenerateHistogram(modifiedImage);
            histogramtimer.Stop();  
            this.histogramImage.Image = ImageProcessing.ResizeImage(histogramimage, 288, 219);

            grayscaletimebox.Text = (grayscaletimer.ElapsedMilliseconds).ToString() + "ms";
            impulsetimebox.Text = (impulsetimer.ElapsedMilliseconds).ToString() + "ms";
            histogramtimebox.Text = (histogramtimer.ElapsedMilliseconds).ToString() + "ms";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string[] files = Directory.GetFiles(fbd.SelectedPath);

                    System.Windows.Forms.MessageBox.Show("Input Path Set. Files found: " + files.Length.ToString(), "Message");

                    Settings.Default["InputPath"] = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                    textBox1.Text = fbd.SelectedPath;

                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    Settings.Default["OutputPath"] = fbd.SelectedPath;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var totaltimer = new Stopwatch();
            totaltimer.Start();
            string inputPath = Settings.Default["InputPath"].ToString();
            string outputPath = Settings.Default["OutputPath"].ToString();
            var files = new DirectoryInfo(inputPath)
            .GetFiles()
            .Where(f => f.IsImage());

            if(String.IsNullOrWhiteSpace(inputPath) && String.IsNullOrWhiteSpace(outputPath))
            {
                button5.Enabled = false;
            }
            var Grayscaletimer = new Stopwatch();
            var ImpulseTimer = new Stopwatch();
            var HistogramGenTimer = new Stopwatch();

         
            foreach (var file in files)
            {
                using (Bitmap image = (Bitmap)Bitmap.FromFile(file.FullName))
                {
                    Grayscaletimer.Start();
                    using (var grayscaleImage = ImageProcessing.RGB2GrayscaleImage(image))
                    {
                        Grayscaletimer.Stop();
                        ImpulseTimer.Start();
                        //using (var gaussian_corrupted_image = ImageProcessing.GaussianNoiseCorruption(grayscaleImage,double.Parse(gaussian_variance.Text), double.Parse(gaussian_mean.Text), double.Parse(gaussian_grey_level.Text)))
                        //{
                        using (var ImpulsedImage = ImageProcessing.ImpulseNoise(grayscaleImage, impulsenoiseintensityslider.Value))
                        {

                            ImpulseTimer.Stop();
                            try
                            {
                                var newImageName = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file.Name) + "_processed" + file.Extension);
                                ImpulsedImage.Save(newImageName);
                                try
                                {
                                    HistogramGenTimer.Start();
                                    using (var HistogramImage = ImageProcessing.GenerateHistogram(ImpulsedImage))
                                    {
                                        HistogramGenTimer.Stop();
                                        var histogramImageName = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(file.Name) + "_histogram" + file.Extension);
                                        HistogramImage.Save(histogramImageName);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    continue;
                                }

                            }
                            catch
                            {
                                continue;
                            }
                        }
                        //}
                    }
                }
            }
            totaltimer.Stop();

            histogrameqtimebox.Text = totaltimer.ElapsedMilliseconds.ToString();
            grayscaletimebox.Text = Grayscaletimer.ElapsedMilliseconds.ToString();
            impulsetimebox.Text = ImpulseTimer.ElapsedMilliseconds.ToString();
            histogrameqtimebox.Text = HistogramGenTimer.ElapsedMilliseconds.ToString();
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.ReadOnly = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.ReadOnly = true;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click_1(object sender, EventArgs e)
        {

        }

        private void gaussian_mean_TextChanged(object sender, EventArgs e)
        {

        }
    }
    }
