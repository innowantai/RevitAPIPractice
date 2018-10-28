namespace _6_ReadDWG
{
    partial class Form1
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
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cmbColFamilyType = new System.Windows.Forms.ComboBox();
            this.cmbColType = new System.Windows.Forms.ComboBox();
            this.cmbBeamType = new System.Windows.Forms.ComboBox();
            this.cmbBeamFamilyType = new System.Windows.Forms.ComboBox();
            this.cmbColBaseLevel = new System.Windows.Forms.ComboBox();
            this.cmbColTopLevel = new System.Windows.Forms.ComboBox();
            this.cmbColCADLayers = new System.Windows.Forms.ComboBox();
            this.cmbBeamCADLayers = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.cmbBeamBaseLevel = new System.Windows.Forms.ComboBox();
            this.chCol = new System.Windows.Forms.CheckBox();
            this.chBeam = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label2.Location = new System.Drawing.Point(54, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(88, 24);
            this.label2.TabIndex = 1;
            this.label2.Text = "CAD圖層";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label3.Location = new System.Drawing.Point(73, 150);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 24);
            this.label3.TabIndex = 2;
            this.label3.Text = "柱類型";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label4.Location = new System.Drawing.Point(56, 246);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 24);
            this.label4.TabIndex = 3;
            this.label4.Text = "基準樓層";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label5.Location = new System.Drawing.Point(54, 311);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 24);
            this.label5.TabIndex = 4;
            this.label5.Text = "頂部樓層";
            // 
            // cmbColFamilyType
            // 
            this.cmbColFamilyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColFamilyType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColFamilyType.FormattingEnabled = true;
            this.cmbColFamilyType.Location = new System.Drawing.Point(26, 86);
            this.cmbColFamilyType.Name = "cmbColFamilyType";
            this.cmbColFamilyType.Size = new System.Drawing.Size(162, 24);
            this.cmbColFamilyType.TabIndex = 5;
            this.cmbColFamilyType.TextChanged += new System.EventHandler(this.cmbColFamilyType_TextChanged);
            // 
            // cmbColType
            // 
            this.cmbColType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColType.FormattingEnabled = true;
            this.cmbColType.Location = new System.Drawing.Point(26, 116);
            this.cmbColType.Name = "cmbColType";
            this.cmbColType.Size = new System.Drawing.Size(162, 24);
            this.cmbColType.TabIndex = 6;
            // 
            // cmbBeamType
            // 
            this.cmbBeamType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeamType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBeamType.FormattingEnabled = true;
            this.cmbBeamType.Location = new System.Drawing.Point(18, 121);
            this.cmbBeamType.Name = "cmbBeamType";
            this.cmbBeamType.Size = new System.Drawing.Size(162, 24);
            this.cmbBeamType.TabIndex = 8;
            // 
            // cmbBeamFamilyType
            // 
            this.cmbBeamFamilyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeamFamilyType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBeamFamilyType.FormattingEnabled = true;
            this.cmbBeamFamilyType.Location = new System.Drawing.Point(18, 86);
            this.cmbBeamFamilyType.Name = "cmbBeamFamilyType";
            this.cmbBeamFamilyType.Size = new System.Drawing.Size(162, 24);
            this.cmbBeamFamilyType.TabIndex = 7;
            this.cmbBeamFamilyType.SelectedIndexChanged += new System.EventHandler(this.cmbBeamFamilyType_SelectedIndexChanged);
            // 
            // cmbColBaseLevel
            // 
            this.cmbColBaseLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColBaseLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColBaseLevel.FormattingEnabled = true;
            this.cmbColBaseLevel.Location = new System.Drawing.Point(26, 185);
            this.cmbColBaseLevel.Name = "cmbColBaseLevel";
            this.cmbColBaseLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbColBaseLevel.TabIndex = 9;
            // 
            // cmbColTopLevel
            // 
            this.cmbColTopLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColTopLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColTopLevel.FormattingEnabled = true;
            this.cmbColTopLevel.Location = new System.Drawing.Point(26, 247);
            this.cmbColTopLevel.Name = "cmbColTopLevel";
            this.cmbColTopLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbColTopLevel.TabIndex = 10;
            // 
            // cmbColCADLayers
            // 
            this.cmbColCADLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColCADLayers.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColCADLayers.FormattingEnabled = true;
            this.cmbColCADLayers.Location = new System.Drawing.Point(26, 26);
            this.cmbColCADLayers.Name = "cmbColCADLayers";
            this.cmbColCADLayers.Size = new System.Drawing.Size(162, 24);
            this.cmbColCADLayers.TabIndex = 11;
            // 
            // cmbBeamCADLayers
            // 
            this.cmbBeamCADLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeamCADLayers.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBeamCADLayers.FormattingEnabled = true;
            this.cmbBeamCADLayers.Location = new System.Drawing.Point(18, 26);
            this.cmbBeamCADLayers.Name = "cmbBeamCADLayers";
            this.cmbBeamCADLayers.Size = new System.Drawing.Size(162, 24);
            this.cmbBeamCADLayers.TabIndex = 12;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("新細明體", 14F);
            this.button1.Location = new System.Drawing.Point(497, 385);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 41);
            this.button1.TabIndex = 13;
            this.button1.Text = "執行";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // cmbBeamBaseLevel
            // 
            this.cmbBeamBaseLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeamBaseLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBeamBaseLevel.FormattingEnabled = true;
            this.cmbBeamBaseLevel.Location = new System.Drawing.Point(18, 185);
            this.cmbBeamBaseLevel.Name = "cmbBeamBaseLevel";
            this.cmbBeamBaseLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbBeamBaseLevel.TabIndex = 14;
            // 
            // chCol
            // 
            this.chCol.AutoSize = true;
            this.chCol.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.chCol.Location = new System.Drawing.Point(229, 33);
            this.chCol.Name = "chCol";
            this.chCol.Size = new System.Drawing.Size(86, 28);
            this.chCol.TabIndex = 16;
            this.chCol.Text = "建立柱";
            this.chCol.UseVisualStyleBackColor = true;
            // 
            // chBeam
            // 
            this.chBeam.AutoSize = true;
            this.chBeam.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.chBeam.Location = new System.Drawing.Point(484, 33);
            this.chBeam.Name = "chBeam";
            this.chBeam.Size = new System.Drawing.Size(86, 28);
            this.chBeam.TabIndex = 17;
            this.chBeam.Text = "建立梁";
            this.chBeam.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cmbBeamCADLayers);
            this.groupBox1.Controls.Add(this.cmbBeamFamilyType);
            this.groupBox1.Controls.Add(this.cmbBeamBaseLevel);
            this.groupBox1.Controls.Add(this.cmbBeamType);
            this.groupBox1.Location = new System.Drawing.Point(437, 67);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 316);
            this.groupBox1.TabIndex = 20;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cmbColCADLayers);
            this.groupBox2.Controls.Add(this.cmbColTopLevel);
            this.groupBox2.Controls.Add(this.cmbColFamilyType);
            this.groupBox2.Controls.Add(this.cmbColBaseLevel);
            this.groupBox2.Controls.Add(this.cmbColType);
            this.groupBox2.Location = new System.Drawing.Point(188, 67);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 316);
            this.groupBox2.TabIndex = 21;
            this.groupBox2.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chBeam);
            this.Controls.Add(this.chCol);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cmbColFamilyType;
        private System.Windows.Forms.ComboBox cmbColType;
        private System.Windows.Forms.ComboBox cmbBeamType;
        private System.Windows.Forms.ComboBox cmbBeamFamilyType;
        private System.Windows.Forms.ComboBox cmbColBaseLevel;
        private System.Windows.Forms.ComboBox cmbColTopLevel;
        private System.Windows.Forms.ComboBox cmbColCADLayers;
        private System.Windows.Forms.ComboBox cmbBeamCADLayers;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox cmbBeamBaseLevel;
        private System.Windows.Forms.CheckBox chCol;
        private System.Windows.Forms.CheckBox chBeam;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}