namespace _6_ReadDWG
{
    partial class Form_InsertShowLayers
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
            this.label3 = new System.Windows.Forms.Label();
            this.cmbLayerCAD = new System.Windows.Forms.ComboBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 16F);
            this.label3.Location = new System.Drawing.Point(34, 51);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(160, 22);
            this.label3.TabIndex = 10;
            this.label3.Text = "CAD標註圖層 : ";
            // 
            // cmbLayerCAD
            // 
            this.cmbLayerCAD.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLayerCAD.Font = new System.Drawing.Font("新細明體", 14F);
            this.cmbLayerCAD.FormattingEnabled = true;
            this.cmbLayerCAD.Location = new System.Drawing.Point(196, 51);
            this.cmbLayerCAD.Name = "cmbLayerCAD";
            this.cmbLayerCAD.Size = new System.Drawing.Size(230, 27);
            this.cmbLayerCAD.TabIndex = 9;
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("新細明體", 16F);
            this.btnStart.Location = new System.Drawing.Point(305, 168);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(121, 59);
            this.btnStart.TabIndex = 8;
            this.btnStart.Text = "執行";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // Form_InsertShowLayers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 450);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cmbLayerCAD);
            this.Controls.Add(this.btnStart);
            this.Name = "Form_InsertShowLayers";
            this.Text = "Form_InsertShowLayers";
            this.Load += new System.EventHandler(this.Form_InsertShowLayers_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        public System.Windows.Forms.ComboBox cmbLayerCAD;
        private System.Windows.Forms.Button btnStart;
    }
}