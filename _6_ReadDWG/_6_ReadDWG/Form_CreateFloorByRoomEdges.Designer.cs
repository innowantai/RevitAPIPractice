namespace _6_ReadDWG
{
    partial class Form_CreateFloorByRoomEdges
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
            this.btnStart = new System.Windows.Forms.Button();
            this.txtShift = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbFloorTypes = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cmbBaseLevels = new System.Windows.Forms.ComboBox();
            this.chIsStructural = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("新細明體", 14F);
            this.btnStart.Location = new System.Drawing.Point(398, 253);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(103, 41);
            this.btnStart.TabIndex = 38;
            this.btnStart.Text = "執行";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // txtShift
            // 
            this.txtShift.Font = new System.Drawing.Font("新細明體", 12F);
            this.txtShift.Location = new System.Drawing.Point(188, 194);
            this.txtShift.Name = "txtShift";
            this.txtShift.Size = new System.Drawing.Size(313, 27);
            this.txtShift.TabIndex = 37;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label3.Location = new System.Drawing.Point(28, 194);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 24);
            this.label3.TabIndex = 36;
            this.label3.Text = "基準偏移(mm) : ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label2.Location = new System.Drawing.Point(76, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 24);
            this.label2.TabIndex = 33;
            this.label2.Text = "樓板類型 :";
            // 
            // cmbFloorTypes
            // 
            this.cmbFloorTypes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFloorTypes.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbFloorTypes.FormattingEnabled = true;
            this.cmbFloorTypes.Location = new System.Drawing.Point(188, 72);
            this.cmbFloorTypes.Name = "cmbFloorTypes";
            this.cmbFloorTypes.Size = new System.Drawing.Size(313, 24);
            this.cmbFloorTypes.TabIndex = 32;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微軟正黑體", 14F);
            this.label4.Location = new System.Drawing.Point(76, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 24);
            this.label4.TabIndex = 31;
            this.label4.Text = "基準樓層 : ";
            // 
            // cmbBaseLevels
            // 
            this.cmbBaseLevels.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBaseLevels.Font = new System.Drawing.Font("新細明體", 12F);
            this.cmbBaseLevels.FormattingEnabled = true;
            this.cmbBaseLevels.Location = new System.Drawing.Point(188, 129);
            this.cmbBaseLevels.Name = "cmbBaseLevels";
            this.cmbBaseLevels.Size = new System.Drawing.Size(313, 24);
            this.cmbBaseLevels.TabIndex = 30;
            // 
            // chIsStructural
            // 
            this.chIsStructural.AutoSize = true;
            this.chIsStructural.Font = new System.Drawing.Font("新細明體", 12F);
            this.chIsStructural.Location = new System.Drawing.Point(515, 72);
            this.chIsStructural.Name = "chIsStructural";
            this.chIsStructural.Size = new System.Drawing.Size(75, 20);
            this.chIsStructural.TabIndex = 39;
            this.chIsStructural.Text = "結構板";
            this.chIsStructural.UseVisualStyleBackColor = true;
            // 
            // Form_CreateFloorByRoomEdges
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(602, 363);
            this.Controls.Add(this.chIsStructural);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.txtShift);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cmbFloorTypes);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cmbBaseLevels);
            this.Name = "Form_CreateFloorByRoomEdges";
            this.Text = "Form_CreateFloorByRoomEdges";
            this.Load += new System.EventHandler(this.Form_CreateFloorByRoomEdges_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnStart;
        public System.Windows.Forms.TextBox txtShift;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.ComboBox cmbFloorTypes;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.ComboBox cmbBaseLevels;
        public System.Windows.Forms.CheckBox chIsStructural;
    }
}