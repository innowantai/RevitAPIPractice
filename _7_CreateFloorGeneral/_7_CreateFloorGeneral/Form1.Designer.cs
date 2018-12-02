namespace _7_CreateFloorGeneral
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
            this.radFloor = new System.Windows.Forms.RadioButton();
            this.radWall = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbTypes = new System.Windows.Forms.ComboBox();
            this.cmbLevels = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbCADLayers = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbCurvesType = new System.Windows.Forms.ComboBox();
            this.btnDone = new System.Windows.Forms.Button();
            this.txtHeigth = new System.Windows.Forms.TextBox();
            this.lblWall1 = new System.Windows.Forms.Label();
            this.lblWall2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // radFloor
            // 
            this.radFloor.AutoSize = true;
            this.radFloor.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radFloor.Location = new System.Drawing.Point(169, 26);
            this.radFloor.Name = "radFloor";
            this.radFloor.Size = new System.Drawing.Size(93, 31);
            this.radFloor.TabIndex = 0;
            this.radFloor.TabStop = true;
            this.radFloor.Text = "建立板";
            this.radFloor.UseVisualStyleBackColor = true;
            this.radFloor.CheckedChanged += new System.EventHandler(this.radFloor_CheckedChanged);
            // 
            // radWall
            // 
            this.radWall.AutoSize = true;
            this.radWall.Font = new System.Drawing.Font("微軟正黑體", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.radWall.Location = new System.Drawing.Point(347, 26);
            this.radWall.Name = "radWall";
            this.radWall.Size = new System.Drawing.Size(93, 31);
            this.radWall.TabIndex = 1;
            this.radWall.TabStop = true;
            this.radWall.Text = "建立牆";
            this.radWall.UseVisualStyleBackColor = true;
            this.radWall.CheckedChanged += new System.EventHandler(this.radWall_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 16F);
            this.label1.Location = new System.Drawing.Point(111, 167);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 22);
            this.label1.TabIndex = 2;
            this.label1.Text = "類型 : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 16F);
            this.label2.Location = new System.Drawing.Point(67, 237);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(110, 22);
            this.label2.TabIndex = 3;
            this.label2.Text = "基礎樓層 :";
            // 
            // cmbTypes
            // 
            this.cmbTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTypes.Font = new System.Drawing.Font("新細明體", 16F);
            this.cmbTypes.FormattingEnabled = true;
            this.cmbTypes.Location = new System.Drawing.Point(206, 164);
            this.cmbTypes.Name = "cmbTypes";
            this.cmbTypes.Size = new System.Drawing.Size(228, 29);
            this.cmbTypes.TabIndex = 4;
            // 
            // cmbLevels
            // 
            this.cmbLevels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevels.Font = new System.Drawing.Font("新細明體", 16F);
            this.cmbLevels.FormattingEnabled = true;
            this.cmbLevels.Location = new System.Drawing.Point(206, 234);
            this.cmbLevels.Name = "cmbLevels";
            this.cmbLevels.Size = new System.Drawing.Size(228, 29);
            this.cmbLevels.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 16F);
            this.label3.Location = new System.Drawing.Point(67, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 22);
            this.label3.TabIndex = 6;
            this.label3.Text = "CAD圖層 : ";
            // 
            // cmbCADLayers
            // 
            this.cmbCADLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCADLayers.Font = new System.Drawing.Font("新細明體", 16F);
            this.cmbCADLayers.FormattingEnabled = true;
            this.cmbCADLayers.Items.AddRange(new object[] {
            "全部圖層"});
            this.cmbCADLayers.Location = new System.Drawing.Point(206, 98);
            this.cmbCADLayers.Name = "cmbCADLayers";
            this.cmbCADLayers.Size = new System.Drawing.Size(228, 29);
            this.cmbCADLayers.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 16F);
            this.label4.Location = new System.Drawing.Point(23, 315);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(154, 22);
            this.label4.TabIndex = 8;
            this.label4.Text = "創立曲線種類 :";
            // 
            // cmbCurvesType
            // 
            this.cmbCurvesType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCurvesType.Font = new System.Drawing.Font("新細明體", 16F);
            this.cmbCurvesType.FormattingEnabled = true;
            this.cmbCurvesType.Items.AddRange(new object[] {
            "封閉曲線",
            "非封閉曲線",
            "兩者皆是"});
            this.cmbCurvesType.Location = new System.Drawing.Point(206, 308);
            this.cmbCurvesType.Name = "cmbCurvesType";
            this.cmbCurvesType.Size = new System.Drawing.Size(228, 29);
            this.cmbCurvesType.TabIndex = 9;
            // 
            // btnDone
            // 
            this.btnDone.Font = new System.Drawing.Font("新細明體", 16F);
            this.btnDone.Location = new System.Drawing.Point(316, 382);
            this.btnDone.Name = "btnDone";
            this.btnDone.Size = new System.Drawing.Size(116, 56);
            this.btnDone.TabIndex = 10;
            this.btnDone.Text = "執行";
            this.btnDone.UseVisualStyleBackColor = true;
            this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
            // 
            // txtHeigth
            // 
            this.txtHeigth.Font = new System.Drawing.Font("新細明體", 12F);
            this.txtHeigth.Location = new System.Drawing.Point(348, 64);
            this.txtHeigth.Name = "txtHeigth";
            this.txtHeigth.Size = new System.Drawing.Size(100, 27);
            this.txtHeigth.TabIndex = 11;
            // 
            // lblWall1
            // 
            this.lblWall1.AutoSize = true;
            this.lblWall1.Font = new System.Drawing.Font("新細明體", 16F);
            this.lblWall1.Location = new System.Drawing.Point(254, 65);
            this.lblWall1.Name = "lblWall1";
            this.lblWall1.Size = new System.Drawing.Size(94, 22);
            this.lblWall1.TabIndex = 12;
            this.lblWall1.Text = "牆高度 : ";
            // 
            // lblWall2
            // 
            this.lblWall2.AutoSize = true;
            this.lblWall2.Font = new System.Drawing.Font("新細明體", 16F);
            this.lblWall2.Location = new System.Drawing.Point(460, 65);
            this.lblWall2.Name = "lblWall2";
            this.lblWall2.Size = new System.Drawing.Size(42, 22);
            this.lblWall2.TabIndex = 13;
            this.lblWall2.Text = "mm";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 450);
            this.Controls.Add(this.lblWall2);
            this.Controls.Add(this.lblWall1);
            this.Controls.Add(this.txtHeigth);
            this.Controls.Add(this.btnDone);
            this.Controls.Add(this.cmbCurvesType);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbCADLayers);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbLevels);
            this.Controls.Add(this.cmbTypes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radWall);
            this.Controls.Add(this.radFloor);
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
        private System.Windows.Forms.Button btnDone;
        public System.Windows.Forms.RadioButton radFloor;
        public System.Windows.Forms.RadioButton radWall;
        public System.Windows.Forms.ComboBox cmbTypes;
        public System.Windows.Forms.ComboBox cmbLevels;
        public System.Windows.Forms.ComboBox cmbCADLayers;
        public System.Windows.Forms.ComboBox cmbCurvesType;
        public System.Windows.Forms.TextBox txtHeigth;
        private System.Windows.Forms.Label lblWall1;
        private System.Windows.Forms.Label lblWall2;
    }
}