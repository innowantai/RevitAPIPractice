namespace _6_ReadDWG
{
    partial class Form_CreateLight
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
            this.cmbColCADLayers = new System.Windows.Forms.ComboBox();
            this.cmbColTopLevel = new System.Windows.Forms.ComboBox();
            this.cmbColBaseLevel = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbColFamilyType = new System.Windows.Forms.ComboBox();
            this.cmbColType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtShift = new System.Windows.Forms.TextBox();
            this.radCircle = new System.Windows.Forms.RadioButton();
            this.radPloyline = new System.Windows.Forms.RadioButton();
            this.SuspendLayout();
            // 
            // cmbColCADLayers
            // 
            this.cmbColCADLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColCADLayers.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColCADLayers.FormattingEnabled = true;
            this.cmbColCADLayers.Location = new System.Drawing.Point(200, 53);
            this.cmbColCADLayers.Name = "cmbColCADLayers";
            this.cmbColCADLayers.Size = new System.Drawing.Size(162, 24);
            this.cmbColCADLayers.TabIndex = 17;
            // 
            // cmbColTopLevel
            // 
            this.cmbColTopLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColTopLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColTopLevel.FormattingEnabled = true;
            this.cmbColTopLevel.Location = new System.Drawing.Point(228, 558);
            this.cmbColTopLevel.Name = "cmbColTopLevel";
            this.cmbColTopLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbColTopLevel.TabIndex = 16;
            this.cmbColTopLevel.Visible = false;
            // 
            // cmbColBaseLevel
            // 
            this.cmbColBaseLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColBaseLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColBaseLevel.FormattingEnabled = true;
            this.cmbColBaseLevel.Location = new System.Drawing.Point(200, 170);
            this.cmbColBaseLevel.Name = "cmbColBaseLevel";
            this.cmbColBaseLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbColBaseLevel.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label5.Location = new System.Drawing.Point(95, 558);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 24);
            this.label5.TabIndex = 14;
            this.label5.Text = "頂部樓層";
            this.label5.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label4.Location = new System.Drawing.Point(63, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 24);
            this.label4.TabIndex = 13;
            this.label4.Text = "基準樓層 : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label2.Location = new System.Drawing.Point(65, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 24);
            this.label2.TabIndex = 12;
            this.label2.Text = "CAD圖層 :";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("新細明體", 14F);
            this.button1.Location = new System.Drawing.Point(261, 275);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 41);
            this.button1.TabIndex = 18;
            this.button1.Text = "執行";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label3.Location = new System.Drawing.Point(68, 95);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 24);
            this.label3.TabIndex = 19;
            this.label3.Text = "族群類型 :";
            // 
            // cmbColFamilyType
            // 
            this.cmbColFamilyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColFamilyType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColFamilyType.FormattingEnabled = true;
            this.cmbColFamilyType.Location = new System.Drawing.Point(201, 95);
            this.cmbColFamilyType.Name = "cmbColFamilyType";
            this.cmbColFamilyType.Size = new System.Drawing.Size(162, 24);
            this.cmbColFamilyType.TabIndex = 20;
            this.cmbColFamilyType.SelectedIndexChanged += new System.EventHandler(this.cmbColFamilyType_SelectedIndexChanged);
            // 
            // cmbColType
            // 
            this.cmbColType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColType.FormattingEnabled = true;
            this.cmbColType.Location = new System.Drawing.Point(201, 125);
            this.cmbColType.Name = "cmbColType";
            this.cmbColType.Size = new System.Drawing.Size(162, 24);
            this.cmbColType.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label1.Location = new System.Drawing.Point(65, 221);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 24);
            this.label1.TabIndex = 22;
            this.label1.Text = "基準偏移: ";
            // 
            // txtShift
            // 
            this.txtShift.Font = new System.Drawing.Font("新細明體", 14F);
            this.txtShift.Location = new System.Drawing.Point(200, 221);
            this.txtShift.Name = "txtShift";
            this.txtShift.Size = new System.Drawing.Size(164, 30);
            this.txtShift.TabIndex = 23;
            // 
            // radCircle
            // 
            this.radCircle.AutoSize = true;
            this.radCircle.Font = new System.Drawing.Font("新細明體", 12F);
            this.radCircle.Location = new System.Drawing.Point(387, 41);
            this.radCircle.Name = "radCircle";
            this.radCircle.Size = new System.Drawing.Size(58, 20);
            this.radCircle.TabIndex = 24;
            this.radCircle.TabStop = true;
            this.radCircle.Text = "圓形";
            this.radCircle.UseVisualStyleBackColor = true;
            // 
            // radPloyline
            // 
            this.radPloyline.AutoSize = true;
            this.radPloyline.Font = new System.Drawing.Font("新細明體", 12F);
            this.radPloyline.Location = new System.Drawing.Point(387, 64);
            this.radPloyline.Name = "radPloyline";
            this.radPloyline.Size = new System.Drawing.Size(74, 20);
            this.radPloyline.TabIndex = 25;
            this.radPloyline.TabStop = true;
            this.radPloyline.Text = "多邊形";
            this.radPloyline.UseVisualStyleBackColor = true;
            // 
            // Form_CreateLight
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(523, 399);
            this.Controls.Add(this.radPloyline);
            this.Controls.Add(this.radCircle);
            this.Controls.Add(this.txtShift);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbColFamilyType);
            this.Controls.Add(this.cmbColType);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmbColCADLayers);
            this.Controls.Add(this.cmbColTopLevel);
            this.Controls.Add(this.cmbColBaseLevel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label2);
            this.Name = "Form_CreateLight";
            this.Text = "Form_CreateLight";
            this.Load += new System.EventHandler(this.Form_CreateLight_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox cmbColCADLayers;
        private System.Windows.Forms.ComboBox cmbColTopLevel;
        private System.Windows.Forms.ComboBox cmbColBaseLevel;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbColFamilyType;
        private System.Windows.Forms.ComboBox cmbColType;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox txtShift;
        public System.Windows.Forms.RadioButton radCircle;
        public System.Windows.Forms.RadioButton radPloyline;
    }
}