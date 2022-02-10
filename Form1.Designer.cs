namespace Talej_Image_Processor_CMSC_630
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Original = new System.Windows.Forms.PictureBox();
            this.Modified = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.impulsenoiseintensityslider = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.gaussian_noise_radio_disable = new System.Windows.Forms.RadioButton();
            this.gaussian_noise_radio_enable = new System.Windows.Forms.RadioButton();
            this.label5 = new System.Windows.Forms.Label();
            this.gaussian_gray_level_label = new System.Windows.Forms.Label();
            this.gaussian_variance_label = new System.Windows.Forms.Label();
            this.gaussian_mean_label = new System.Windows.Forms.Label();
            this.gaussian_mean = new System.Windows.Forms.TextBox();
            this.gaussian_variance = new System.Windows.Forms.TextBox();
            this.gaussian_grey_level = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.eqHistogram = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.histogramImage = new System.Windows.Forms.PictureBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.histogrameqtimebox = new System.Windows.Forms.TextBox();
            this.histogramtimebox = new System.Windows.Forms.TextBox();
            this.gaussiantimebox = new System.Windows.Forms.TextBox();
            this.impulsetimebox = new System.Windows.Forms.TextBox();
            this.grayscaletimebox = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.Original)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Modified)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.impulsenoiseintensityslider)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eqHistogram)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramImage)).BeginInit();
            this.groupBox3.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // Original
            // 
            this.Original.Location = new System.Drawing.Point(149, 18);
            this.Original.Name = "Original";
            this.Original.Size = new System.Drawing.Size(288, 219);
            this.Original.TabIndex = 0;
            this.Original.TabStop = false;
            // 
            // Modified
            // 
            this.Modified.Location = new System.Drawing.Point(443, 18);
            this.Modified.Name = "Modified";
            this.Modified.Size = new System.Drawing.Size(288, 219);
            this.Modified.TabIndex = 2;
            this.Modified.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(6, 31);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Select";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(6, 60);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 4;
            this.button2.Text = "Process";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(6, 22);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(154, 23);
            this.button3.TabIndex = 5;
            this.button3.Text = "Select Input Directory";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(6, 51);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(154, 23);
            this.button4.TabIndex = 6;
            this.button4.Text = "Set Output Path";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(6, 81);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(154, 23);
            this.button5.TabIndex = 7;
            this.button5.Text = "Batch Process";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(166, 81);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(218, 23);
            this.progressBar1.TabIndex = 8;
            this.progressBar1.Click += new System.EventHandler(this.progressBar1_Click);
            // 
            // impulsenoiseintensityslider
            // 
            this.impulsenoiseintensityslider.LargeChange = 25;
            this.impulsenoiseintensityslider.Location = new System.Drawing.Point(14, 66);
            this.impulsenoiseintensityslider.Maximum = 100;
            this.impulsenoiseintensityslider.Name = "impulsenoiseintensityslider";
            this.impulsenoiseintensityslider.Size = new System.Drawing.Size(170, 45);
            this.impulsenoiseintensityslider.SmallChange = 10;
            this.impulsenoiseintensityslider.TabIndex = 9;
            this.impulsenoiseintensityslider.Scroll += new System.EventHandler(this.trackBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 15);
            this.label1.TabIndex = 11;
            this.label1.Text = "Salt and Pepper Noise Intensity";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(166, 23);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(218, 23);
            this.textBox1.TabIndex = 13;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(166, 51);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(218, 23);
            this.textBox2.TabIndex = 14;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.gaussian_noise_radio_disable);
            this.groupBox1.Controls.Add(this.gaussian_noise_radio_enable);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.gaussian_gray_level_label);
            this.groupBox1.Controls.Add(this.gaussian_variance_label);
            this.groupBox1.Controls.Add(this.gaussian_mean_label);
            this.groupBox1.Controls.Add(this.gaussian_mean);
            this.groupBox1.Controls.Add(this.gaussian_variance);
            this.groupBox1.Controls.Add(this.gaussian_grey_level);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.impulsenoiseintensityslider);
            this.groupBox1.Location = new System.Drawing.Point(26, 25);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(437, 159);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Image Corruption Options";
            // 
            // gaussian_noise_radio_disable
            // 
            this.gaussian_noise_radio_disable.AutoSize = true;
            this.gaussian_noise_radio_disable.Location = new System.Drawing.Point(208, 81);
            this.gaussian_noise_radio_disable.Name = "gaussian_noise_radio_disable";
            this.gaussian_noise_radio_disable.Size = new System.Drawing.Size(70, 19);
            this.gaussian_noise_radio_disable.TabIndex = 20;
            this.gaussian_noise_radio_disable.TabStop = true;
            this.gaussian_noise_radio_disable.Text = "Disabled";
            this.gaussian_noise_radio_disable.UseVisualStyleBackColor = true;
            // 
            // gaussian_noise_radio_enable
            // 
            this.gaussian_noise_radio_enable.AutoSize = true;
            this.gaussian_noise_radio_enable.Location = new System.Drawing.Point(208, 63);
            this.gaussian_noise_radio_enable.Name = "gaussian_noise_radio_enable";
            this.gaussian_noise_radio_enable.Size = new System.Drawing.Size(67, 19);
            this.gaussian_noise_radio_enable.TabIndex = 19;
            this.gaussian_noise_radio_enable.TabStop = true;
            this.gaussian_noise_radio_enable.Text = "Enabled";
            this.gaussian_noise_radio_enable.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(208, 39);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(132, 15);
            this.label5.TabIndex = 18;
            this.label5.Text = "Gaussian Noise Options";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // gaussian_gray_level_label
            // 
            this.gaussian_gray_level_label.AutoSize = true;
            this.gaussian_gray_level_label.Location = new System.Drawing.Point(297, 127);
            this.gaussian_gray_level_label.Name = "gaussian_gray_level_label";
            this.gaussian_gray_level_label.Size = new System.Drawing.Size(18, 15);
            this.gaussian_gray_level_label.TabIndex = 17;
            this.gaussian_gray_level_label.Text = "z :";
            // 
            // gaussian_variance_label
            // 
            this.gaussian_variance_label.AutoSize = true;
            this.gaussian_variance_label.Location = new System.Drawing.Point(295, 95);
            this.gaussian_variance_label.Name = "gaussian_variance_label";
            this.gaussian_variance_label.Size = new System.Drawing.Size(20, 15);
            this.gaussian_variance_label.TabIndex = 16;
            this.gaussian_variance_label.Text = "σ :";
            this.gaussian_variance_label.Click += new System.EventHandler(this.label3_Click);
            // 
            // gaussian_mean_label
            // 
            this.gaussian_mean_label.AutoSize = true;
            this.gaussian_mean_label.Location = new System.Drawing.Point(295, 65);
            this.gaussian_mean_label.Name = "gaussian_mean_label";
            this.gaussian_mean_label.Size = new System.Drawing.Size(20, 15);
            this.gaussian_mean_label.TabIndex = 15;
            this.gaussian_mean_label.Text = "µ :";
            this.gaussian_mean_label.Click += new System.EventHandler(this.label2_Click_1);
            // 
            // gaussian_mean
            // 
            this.gaussian_mean.Location = new System.Drawing.Point(321, 66);
            this.gaussian_mean.Name = "gaussian_mean";
            this.gaussian_mean.Size = new System.Drawing.Size(100, 23);
            this.gaussian_mean.TabIndex = 14;
            this.gaussian_mean.TextChanged += new System.EventHandler(this.gaussian_mean_TextChanged);
            // 
            // gaussian_variance
            // 
            this.gaussian_variance.Location = new System.Drawing.Point(321, 95);
            this.gaussian_variance.Name = "gaussian_variance";
            this.gaussian_variance.Size = new System.Drawing.Size(100, 23);
            this.gaussian_variance.TabIndex = 13;
            // 
            // gaussian_grey_level
            // 
            this.gaussian_grey_level.Location = new System.Drawing.Point(321, 124);
            this.gaussian_grey_level.Name = "gaussian_grey_level";
            this.gaussian_grey_level.Size = new System.Drawing.Size(100, 23);
            this.gaussian_grey_level.TabIndex = 12;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.eqHistogram);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.histogramImage);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.button2);
            this.groupBox2.Controls.Add(this.Original);
            this.groupBox2.Controls.Add(this.Modified);
            this.groupBox2.Location = new System.Drawing.Point(26, 211);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(1330, 251);
            this.groupBox2.TabIndex = 16;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Single Image Processing";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1119, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 15);
            this.label6.TabIndex = 17;
            this.label6.Text = "Equalized Histogram";
            // 
            // eqHistogram
            // 
            this.eqHistogram.Location = new System.Drawing.Point(1036, 18);
            this.eqHistogram.Name = "eqHistogram";
            this.eqHistogram.Size = new System.Drawing.Size(288, 219);
            this.eqHistogram.TabIndex = 21;
            this.eqHistogram.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(842, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 15);
            this.label4.TabIndex = 20;
            this.label4.Text = "Histogram";
            // 
            // histogramImage
            // 
            this.histogramImage.Location = new System.Drawing.Point(737, 18);
            this.histogramImage.Name = "histogramImage";
            this.histogramImage.Size = new System.Drawing.Size(288, 219);
            this.histogramImage.TabIndex = 17;
            this.histogramImage.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(255, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 15);
            this.label2.TabIndex = 18;
            this.label2.Text = "Original Image";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(544, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 15);
            this.label3.TabIndex = 19;
            this.label3.Text = "Processed Image";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.button3);
            this.groupBox3.Controls.Add(this.button4);
            this.groupBox3.Controls.Add(this.button5);
            this.groupBox3.Controls.Add(this.progressBar1);
            this.groupBox3.Controls.Add(this.textBox2);
            this.groupBox3.Controls.Add(this.textBox1);
            this.groupBox3.Location = new System.Drawing.Point(953, 25);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(403, 159);
            this.groupBox3.TabIndex = 17;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Batch Processing";
            // 
            // groupBox4
            // 
            this.groupBox4.Location = new System.Drawing.Point(489, 36);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(442, 148);
            this.groupBox4.TabIndex = 18;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Filtering Options";
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.histogrameqtimebox);
            this.groupBox5.Controls.Add(this.histogramtimebox);
            this.groupBox5.Controls.Add(this.gaussiantimebox);
            this.groupBox5.Controls.Add(this.impulsetimebox);
            this.groupBox5.Controls.Add(this.grayscaletimebox);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.label9);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Location = new System.Drawing.Point(26, 488);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1330, 132);
            this.groupBox5.TabIndex = 19;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Batch Performance Metrics";
            // 
            // histogrameqtimebox
            // 
            this.histogrameqtimebox.Location = new System.Drawing.Point(509, 16);
            this.histogrameqtimebox.Name = "histogrameqtimebox";
            this.histogrameqtimebox.Size = new System.Drawing.Size(168, 23);
            this.histogrameqtimebox.TabIndex = 25;
            // 
            // histogramtimebox
            // 
            this.histogramtimebox.Location = new System.Drawing.Point(172, 103);
            this.histogramtimebox.Name = "histogramtimebox";
            this.histogramtimebox.Size = new System.Drawing.Size(168, 23);
            this.histogramtimebox.TabIndex = 24;
            // 
            // gaussiantimebox
            // 
            this.gaussiantimebox.Location = new System.Drawing.Point(172, 74);
            this.gaussiantimebox.Name = "gaussiantimebox";
            this.gaussiantimebox.Size = new System.Drawing.Size(168, 23);
            this.gaussiantimebox.TabIndex = 23;
            // 
            // impulsetimebox
            // 
            this.impulsetimebox.Location = new System.Drawing.Point(172, 45);
            this.impulsetimebox.Name = "impulsetimebox";
            this.impulsetimebox.Size = new System.Drawing.Size(168, 23);
            this.impulsetimebox.TabIndex = 22;
            // 
            // grayscaletimebox
            // 
            this.grayscaletimebox.Location = new System.Drawing.Point(172, 16);
            this.grayscaletimebox.Name = "grayscaletimebox";
            this.grayscaletimebox.Size = new System.Drawing.Size(168, 23);
            this.grayscaletimebox.TabIndex = 21;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(373, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(130, 15);
            this.label11.TabIndex = 20;
            this.label11.Text = "Histogram Equalization";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 106);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(126, 15);
            this.label10.TabIndex = 3;
            this.label10.Text = "Histogram Calculation";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 77);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(148, 15);
            this.label9.TabIndex = 2;
            this.label9.Text = "Gaussian Noise Corruption";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 48);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(150, 15);
            this.label8.TabIndex = 1;
            this.label8.Text = "Salt and Pepper Corruption";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(160, 15);
            this.label7.TabIndex = 0;
            this.label7.Text = "RGB To Grayscale Conversion";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1442, 632);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Image Processor";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Original)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Modified)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.impulsenoiseintensityslider)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.eqHistogram)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramImage)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private PictureBox Original;
        private PictureBox Modified;
        private Button button1;
        private Button button2;
        private Button button3;
        private Button button4;
        private Button button5;
        private ProgressBar progressBar1;
        private TrackBar impulsenoiseintensityslider;
        private Label label1;
        private TextBox textBox1;
        private TextBox textBox2;
        private GroupBox groupBox1;
        private Label gaussian_gray_level_label;
        private Label gaussian_variance_label;
        private Label gaussian_mean_label;
        private TextBox gaussian_mean;
        private TextBox gaussian_variance;
        private TextBox gaussian_grey_level;
        private Label label5;
        private RadioButton gaussian_noise_radio_disable;
        private RadioButton gaussian_noise_radio_enable;
        private GroupBox groupBox2;
        private Label label4;
        private PictureBox histogramImage;
        private Label label2;
        private Label label3;
        private Label label6;
        private PictureBox eqHistogram;
        private GroupBox groupBox3;
        private GroupBox groupBox4;
        private GroupBox groupBox5;
        private TextBox histogrameqtimebox;
        private TextBox histogramtimebox;
        private TextBox gaussiantimebox;
        private TextBox impulsetimebox;
        private TextBox grayscaletimebox;
        private Label label11;
        private Label label10;
        private Label label9;
        private Label label8;
        private Label label7;
    }
}