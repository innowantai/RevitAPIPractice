namespace _6_ReadDWG
{
    partial class Form_InsertCommentToBeam
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.cmbLevels = new System.Windows.Forms.ComboBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 16F);
            this.label1.Location = new System.Drawing.Point(114, 103);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(116, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "基準樓層 : ";
            // 
            // cmbLevels
            // 
            this.cmbLevels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevels.Font = new System.Drawing.Font("新細明體", 14F);
            this.cmbLevels.FormattingEnabled = true;
            this.cmbLevels.Location = new System.Drawing.Point(232, 102);
            this.cmbLevels.Name = "cmbLevels";
            this.cmbLevels.Size = new System.Drawing.Size(121, 27);
            this.cmbLevels.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("新細明體", 16F);
            this.btnStart.Location = new System.Drawing.Point(583, 164);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(121, 59);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "執行";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.button1_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 16F);
            this.label2.Location = new System.Drawing.Point(26, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(204, 22);
            this.label2.TabIndex = 3;
            this.label2.Text = "選擇CAD輸出檔案 : ";
            // 
            // txtFilePath
            // 
            this.txtFilePath.Font = new System.Drawing.Font("新細明體", 14F);
            this.txtFilePath.Location = new System.Drawing.Point(231, 62);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(473, 30);
            this.txtFilePath.TabIndex = 4;
            this.txtFilePath.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtFilePath_MouseClick);
            // 
            // btnSelect
            // 
            this.btnSelect.Font = new System.Drawing.Font("新細明體", 12F);
            this.btnSelect.Location = new System.Drawing.Point(710, 61);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(64, 31);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Text = "選擇";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // Form_InsertCommentToBeam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.cmbLevels);
            this.Controls.Add(this.label1);
            this.Name = "Form_InsertCommentToBeam";
            this.Text = "Form_InsertCommentToBeam";
            this.Load += new System.EventHandler(this.Form_InsertCommentToBeam_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.ComboBox cmbLevels;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Button btnSelect;
    }
}