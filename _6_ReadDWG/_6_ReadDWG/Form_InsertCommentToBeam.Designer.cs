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
            this.btnStart = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnSelect = new System.Windows.Forms.Button();
            this.cmbFamilyType = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.listOri = new System.Windows.Forms.ListBox();
            this.listSelected = new System.Windows.Forms.ListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 16F);
            this.label1.Location = new System.Drawing.Point(114, 159);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(148, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "全部基準樓層 ";
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("新細明體", 16F);
            this.btnStart.Location = new System.Drawing.Point(583, 196);
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
            this.label2.Location = new System.Drawing.Point(26, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(204, 22);
            this.label2.TabIndex = 3;
            this.label2.Text = "選擇CAD輸出檔案 : ";
            // 
            // txtFilePath
            // 
            this.txtFilePath.Font = new System.Drawing.Font("新細明體", 14F);
            this.txtFilePath.Location = new System.Drawing.Point(231, 43);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(473, 30);
            this.txtFilePath.TabIndex = 4;
            // 
            // btnSelect
            // 
            this.btnSelect.Font = new System.Drawing.Font("新細明體", 12F);
            this.btnSelect.Location = new System.Drawing.Point(710, 42);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(64, 31);
            this.btnSelect.TabIndex = 5;
            this.btnSelect.Text = "選擇";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // cmbFamilyType
            // 
            this.cmbFamilyType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFamilyType.Font = new System.Drawing.Font("新細明體", 14F);
            this.cmbFamilyType.FormattingEnabled = true;
            this.cmbFamilyType.Items.AddRange(new object[] {
            "梁",
            "柱"});
            this.cmbFamilyType.Location = new System.Drawing.Point(232, 91);
            this.cmbFamilyType.Name = "cmbFamilyType";
            this.cmbFamilyType.Size = new System.Drawing.Size(121, 27);
            this.cmbFamilyType.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 16F);
            this.label3.Location = new System.Drawing.Point(114, 93);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 22);
            this.label3.TabIndex = 7;
            this.label3.Text = "處理物件 : ";
            // 
            // listOri
            // 
            this.listOri.FormattingEnabled = true;
            this.listOri.ItemHeight = 12;
            this.listOri.Location = new System.Drawing.Point(62, 193);
            this.listOri.Name = "listOri";
            this.listOri.Size = new System.Drawing.Size(236, 208);
            this.listOri.TabIndex = 8;
            this.listOri.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listOri_MouseDoubleClick);
            // 
            // listSelected
            // 
            this.listSelected.FormattingEnabled = true;
            this.listSelected.ItemHeight = 12;
            this.listSelected.Location = new System.Drawing.Point(328, 193);
            this.listSelected.Name = "listSelected";
            this.listSelected.Size = new System.Drawing.Size(215, 208);
            this.listSelected.TabIndex = 9; 
            this.listSelected.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listSelected_MouseDoubleClick);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 16F);
            this.label4.Location = new System.Drawing.Point(370, 159);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 22);
            this.label4.TabIndex = 10;
            this.label4.Text = "處理樓層 ";
            // 
            // Form_InsertCommentToBeam
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.listSelected);
            this.Controls.Add(this.listOri);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbFamilyType);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.label1);
            this.Name = "Form_InsertCommentToBeam";
            this.Text = "Form_InsertCommentToBeam";
            this.Load += new System.EventHandler(this.Form_InsertCommentToBeam_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        public System.Windows.Forms.ComboBox cmbFamilyType;
        public System.Windows.Forms.TextBox txtFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ListBox listOri;
        private System.Windows.Forms.ListBox listSelected;
        private System.Windows.Forms.Label label4;
    }
}