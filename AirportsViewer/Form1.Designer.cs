namespace AirportsViewer
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TextBox textBoxCode;
        private System.Windows.Forms.TextBox textBoxName;
        private System.Windows.Forms.Label labelCode;
        private System.Windows.Forms.Label labelName;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.textBoxCode = new System.Windows.Forms.TextBox();
            this.textBoxName = new System.Windows.Forms.TextBox();
            this.labelCode = new System.Windows.Forms.Label();
            this.labelName = new System.Windows.Forms.Label();
            this.buttonUpdateCsv = new System.Windows.Forms.Button();
            this.textBoxCountry = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Location = new System.Drawing.Point(12, 48);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowHeadersWidth = 62;
            this.dataGridView1.Size = new System.Drawing.Size(854, 401);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.ColumnWidthChanged += new System.Windows.Forms.DataGridViewColumnEventHandler(this.dataGridView1_ColumnWidthChanged);
            // 
            // textBoxCode
            // 
            this.textBoxCode.Enabled = false;
            this.textBoxCode.Location = new System.Drawing.Point(69, 11);
            this.textBoxCode.Name = "textBoxCode";
            this.textBoxCode.Size = new System.Drawing.Size(80, 26);
            this.textBoxCode.TabIndex = 2;
            // 
            // textBoxName
            // 
            this.textBoxName.Enabled = false;
            this.textBoxName.Location = new System.Drawing.Point(216, 12);
            this.textBoxName.Name = "textBoxName";
            this.textBoxName.Size = new System.Drawing.Size(217, 26);
            this.textBoxName.TabIndex = 4;
            // 
            // labelCode
            // 
            this.labelCode.AutoSize = true;
            this.labelCode.Location = new System.Drawing.Point(12, 15);
            this.labelCode.Name = "labelCode";
            this.labelCode.Size = new System.Drawing.Size(51, 20);
            this.labelCode.TabIndex = 1;
            this.labelCode.Text = "Code:";
            // 
            // labelName
            // 
            this.labelName.AutoSize = true;
            this.labelName.Location = new System.Drawing.Point(155, 15);
            this.labelName.Name = "labelName";
            this.labelName.Size = new System.Drawing.Size(55, 20);
            this.labelName.TabIndex = 3;
            this.labelName.Text = "Name:";
            // 
            // buttonUpdateCsv
            // 
            this.buttonUpdateCsv.Location = new System.Drawing.Point(707, 12);
            this.buttonUpdateCsv.Name = "buttonUpdateCsv";
            this.buttonUpdateCsv.Size = new System.Drawing.Size(159, 26);
            this.buttonUpdateCsv.TabIndex = 5;
            this.buttonUpdateCsv.Text = "Update Database";
            this.buttonUpdateCsv.UseVisualStyleBackColor = true;
            this.buttonUpdateCsv.Click += new System.EventHandler(this.buttonUpdateCsv_Click);
            // 
            // textBoxCountry
            // 
            this.textBoxCountry.Enabled = false;
            this.textBoxCountry.Location = new System.Drawing.Point(552, 12);
            this.textBoxCountry.Name = "textBoxCountry";
            this.textBoxCountry.Size = new System.Drawing.Size(100, 26);
            this.textBoxCountry.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(439, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 20);
            this.label1.TabIndex = 7;
            this.label1.Text = "Country code:";
            // 
            // Form1
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(878, 461);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxCountry);
            this.Controls.Add(this.buttonUpdateCsv);
            this.Controls.Add(this.labelCode);
            this.Controls.Add(this.textBoxCode);
            this.Controls.Add(this.labelName);
            this.Controls.Add(this.textBoxName);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Airports Viewer";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.Form1_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button buttonUpdateCsv;
        private System.Windows.Forms.TextBox textBoxCountry;
        private System.Windows.Forms.Label label1;
    }
}
