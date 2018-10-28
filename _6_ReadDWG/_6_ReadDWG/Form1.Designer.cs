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
            this.label1 = new System.Windows.Forms.Label();
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
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label1.Location = new System.Drawing.Point(200, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "建立柱";
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
            this.cmbColFamilyType.Location = new System.Drawing.Point(179, 150);
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
            this.cmbColType.Location = new System.Drawing.Point(179, 191);
            this.cmbColType.Name = "cmbColType";
            this.cmbColType.Size = new System.Drawing.Size(162, 24);
            this.cmbColType.TabIndex = 6;
            // 
            // cmbBeamType
            // 
            this.cmbBeamType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeamType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBeamType.FormattingEnabled = true;
            this.cmbBeamType.Location = new System.Drawing.Point(379, 191);
            this.cmbBeamType.Name = "cmbBeamType";
            this.cmbBeamType.Size = new System.Drawing.Size(162, 24);
            this.cmbBeamType.TabIndex = 8;
            // 
            // cmbBeamFamilyType
            // 
            this.cmbBeamFamilyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeamFamilyType.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBeamFamilyType.FormattingEnabled = true;
            this.cmbBeamFamilyType.Location = new System.Drawing.Point(379, 150);
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
            this.cmbColBaseLevel.Location = new System.Drawing.Point(179, 249);
            this.cmbColBaseLevel.Name = "cmbColBaseLevel";
            this.cmbColBaseLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbColBaseLevel.TabIndex = 9;
            // 
            // cmbColTopLevel
            // 
            this.cmbColTopLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColTopLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColTopLevel.FormattingEnabled = true;
            this.cmbColTopLevel.Location = new System.Drawing.Point(179, 311);
            this.cmbColTopLevel.Name = "cmbColTopLevel";
            this.cmbColTopLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbColTopLevel.TabIndex = 10;
            // 
            // cmbColCADLayers
            // 
            this.cmbColCADLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColCADLayers.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColCADLayers.FormattingEnabled = true;
            this.cmbColCADLayers.Location = new System.Drawing.Point(179, 90);
            this.cmbColCADLayers.Name = "cmbColCADLayers";
            this.cmbColCADLayers.Size = new System.Drawing.Size(162, 24);
            this.cmbColCADLayers.TabIndex = 11;
            // 
            // cmbBeamCADLayers
            // 
            this.cmbBeamCADLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBeamCADLayers.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBeamCADLayers.FormattingEnabled = true;
            this.cmbBeamCADLayers.Location = new System.Drawing.Point(379, 90);
            this.cmbBeamCADLayers.Name = "cmbBeamCADLayers";
            this.cmbBeamCADLayers.Size = new System.Drawing.Size(162, 24);
            this.cmbBeamCADLayers.TabIndex = 12;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("新細明體", 14F);
            this.button1.Location = new System.Drawing.Point(438, 377);
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
            this.cmbBeamBaseLevel.Location = new System.Drawing.Point(379, 249);
            this.cmbBeamBaseLevel.Name = "cmbBeamBaseLevel";
            this.cmbBeamBaseLevel.Size = new System.Drawing.Size(162, 24);
            this.cmbBeamBaseLevel.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label6.Location = new System.Drawing.Point(416, 30);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(67, 24);
            this.label6.TabIndex = 15;
            this.label6.Text = "建立梁";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.cmbBeamBaseLevel);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cmbBeamCADLayers);
            this.Controls.Add(this.cmbColCADLayers);
            this.Controls.Add(this.cmbColTopLevel);
            this.Controls.Add(this.cmbColBaseLevel);
            this.Controls.Add(this.cmbBeamType);
            this.Controls.Add(this.cmbBeamFamilyType);
            this.Controls.Add(this.cmbColType);
            this.Controls.Add(this.cmbColFamilyType);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
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
        private System.Windows.Forms.Label label6;
    }
}