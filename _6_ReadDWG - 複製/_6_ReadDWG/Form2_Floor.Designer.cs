namespace _6_ReadDWG
{
    partial class Form2_Floor
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
            this.cmbfloorLevel = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbFloorTypes = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbColLevel = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // cmbfloorLevel
            // 
            this.cmbfloorLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbfloorLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbfloorLevel.FormattingEnabled = true;
            this.cmbfloorLevel.Location = new System.Drawing.Point(175, 161);
            this.cmbfloorLevel.Name = "cmbfloorLevel";
            this.cmbfloorLevel.Size = new System.Drawing.Size(149, 24);
            this.cmbfloorLevel.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label4.Location = new System.Drawing.Point(56, 161);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(105, 24);
            this.label4.TabIndex = 11;
            this.label4.Text = "梁基準樓層";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("新細明體", 14F);
            this.button1.Location = new System.Drawing.Point(175, 261);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 41);
            this.button1.TabIndex = 14;
            this.button1.Text = "執行";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 18F);
            this.label1.Location = new System.Drawing.Point(171, 28);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(110, 31);
            this.label1.TabIndex = 15;
            this.label1.Text = "建立樓板";
            // 
            // cmbFloorTypes
            // 
            this.cmbFloorTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFloorTypes.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbFloorTypes.FormattingEnabled = true;
            this.cmbFloorTypes.Location = new System.Drawing.Point(175, 104);
            this.cmbFloorTypes.Name = "cmbFloorTypes";
            this.cmbFloorTypes.Size = new System.Drawing.Size(149, 24);
            this.cmbFloorTypes.TabIndex = 16;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label2.Location = new System.Drawing.Point(72, 104);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 24);
            this.label2.TabIndex = 17;
            this.label2.Text = "樓板類型";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label3.Location = new System.Drawing.Point(56, 212);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 24);
            this.label3.TabIndex = 18;
            this.label3.Text = "柱基準樓層";
            // 
            // cmbColLevel
            // 
            this.cmbColLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbColLevel.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbColLevel.FormattingEnabled = true;
            this.cmbColLevel.Location = new System.Drawing.Point(175, 214);
            this.cmbColLevel.Name = "cmbColLevel";
            this.cmbColLevel.Size = new System.Drawing.Size(149, 24);
            this.cmbColLevel.TabIndex = 19;
            // 
            // Form2_Floor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(443, 332);
            this.Controls.Add(this.cmbColLevel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbFloorTypes);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbfloorLevel);
            this.Name = "Form2_Floor";
            this.Text = "Form2_Floor";
            this.Load += new System.EventHandler(this.Form2_Floor_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.ComboBox cmbfloorLevel;
        public System.Windows.Forms.ComboBox cmbFloorTypes;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox cmbColLevel;
    }
}