namespace _6_ReadDWG
{
    partial class Form_CreateFloorByCADHash
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
            this.cmbFloorTypes = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbBaseLevels = new System.Windows.Forms.ComboBox();
            this.cmbCADLayers = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtShift = new System.Windows.Forms.TextBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.chIsIndicatedLayers = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label2.Location = new System.Drawing.Point(54, 127);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 24);
            this.label2.TabIndex = 21;
            this.label2.Text = "樓板類型 :";
            // 
            // cmbFloorTypes
            // 
            this.cmbFloorTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFloorTypes.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbFloorTypes.FormattingEnabled = true;
            this.cmbFloorTypes.Location = new System.Drawing.Point(166, 127);
            this.cmbFloorTypes.Name = "cmbFloorTypes";
            this.cmbFloorTypes.Size = new System.Drawing.Size(313, 24);
            this.cmbFloorTypes.TabIndex = 20;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label4.Location = new System.Drawing.Point(54, 184);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 24);
            this.label4.TabIndex = 19;
            this.label4.Text = "基準樓層 : ";
            // 
            // cmbBaseLevels
            // 
            this.cmbBaseLevels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBaseLevels.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBaseLevels.FormattingEnabled = true;
            this.cmbBaseLevels.Location = new System.Drawing.Point(166, 184);
            this.cmbBaseLevels.Name = "cmbBaseLevels";
            this.cmbBaseLevels.Size = new System.Drawing.Size(313, 24);
            this.cmbBaseLevels.TabIndex = 18;
            // 
            // cmbCADLayers
            // 
            this.cmbCADLayers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCADLayers.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbCADLayers.FormattingEnabled = true;
            this.cmbCADLayers.Location = new System.Drawing.Point(166, 66);
            this.cmbCADLayers.Name = "cmbCADLayers";
            this.cmbCADLayers.Size = new System.Drawing.Size(313, 24);
            this.cmbCADLayers.TabIndex = 23;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label1.Location = new System.Drawing.Point(52, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 24);
            this.label1.TabIndex = 22;
            this.label1.Text = "CAD圖層 :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label3.Location = new System.Drawing.Point(6, 249);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 24);
            this.label3.TabIndex = 24;
            this.label3.Text = "基準偏移(mm) : ";
            // 
            // txtShift
            // 
            this.txtShift.Font = new System.Drawing.Font("新細明體", 12F);
            this.txtShift.Location = new System.Drawing.Point(166, 249);
            this.txtShift.Name = "txtShift";
            this.txtShift.Size = new System.Drawing.Size(313, 27);
            this.txtShift.TabIndex = 25;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("新細明體", 14F);
            this.btnStart.Location = new System.Drawing.Point(376, 308);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(103, 41);
            this.btnStart.TabIndex = 26;
            this.btnStart.Text = "執行";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // chIsIndicatedLayers
            // 
            this.chIsIndicatedLayers.AutoSize = true;
            this.chIsIndicatedLayers.Font = new System.Drawing.Font("新細明體", 12F);
            this.chIsIndicatedLayers.Location = new System.Drawing.Point(166, 34);
            this.chIsIndicatedLayers.Name = "chIsIndicatedLayers";
            this.chIsIndicatedLayers.Size = new System.Drawing.Size(155, 20);
            this.chIsIndicatedLayers.TabIndex = 28;
            this.chIsIndicatedLayers.Text = "是否指定CAD圖層";
            this.chIsIndicatedLayers.UseVisualStyleBackColor = true;
            this.chIsIndicatedLayers.CheckedChanged += new System.EventHandler(this.chIsIndicatedLayers_CheckedChanged);
            // 
            // Form_CreateFloorByCADHash
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(601, 361);
            this.Controls.Add(this.chIsIndicatedLayers);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtShift);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbCADLayers);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbFloorTypes);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbBaseLevels);
            this.Name = "Form_CreateFloorByCADHash";
            this.Text = "Form_CreateFloorByCADHash";
            this.Load += new System.EventHandler(this.Form_CreateFloorByCADHash_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox cmbFloorTypes;
        public System.Windows.Forms.ComboBox cmbBaseLevels;
        public System.Windows.Forms.ComboBox cmbCADLayers;
        public System.Windows.Forms.CheckBox chIsIndicatedLayers;
        private System.Windows.Forms.TextBox txtShift;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnStart;
    }
}