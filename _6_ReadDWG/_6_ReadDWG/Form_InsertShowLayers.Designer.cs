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
            this.btnStart = new System.Windows.Forms.Button();
            this.listOri = new System.Windows.Forms.ListBox();
            this.listSelected = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            this.btnStart.Font = new System.Drawing.Font("新細明體", 16F);
            this.btnStart.Location = new System.Drawing.Point(378, 363);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(121, 59);
            this.btnStart.TabIndex = 8;
            this.btnStart.Text = "執行";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // listOri
            // 
            this.listOri.Font = new System.Drawing.Font("新細明體", 14F);
            this.listOri.FormattingEnabled = true;
            this.listOri.ItemHeight = 19;
            this.listOri.Location = new System.Drawing.Point(38, 92);
            this.listOri.Name = "listOri";
            this.listOri.Size = new System.Drawing.Size(225, 251);
            this.listOri.TabIndex = 11;
            this.listOri.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listOriLayers_MouseDoubleClick);
            // 
            // listSelected
            // 
            this.listSelected.Font = new System.Drawing.Font("新細明體", 14F);
            this.listSelected.FormattingEnabled = true;
            this.listSelected.ItemHeight = 19;
            this.listSelected.Location = new System.Drawing.Point(285, 92);
            this.listSelected.Name = "listSelected";
            this.listSelected.Size = new System.Drawing.Size(214, 251);
            this.listSelected.TabIndex = 12;
            this.listSelected.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.listSelectedLayers_MouseDoubleClick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 16F);
            this.label3.Location = new System.Drawing.Point(70, 54);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(142, 22);
            this.label3.TabIndex = 10;
            this.label3.Text = "CAD所有圖層";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 16F);
            this.label1.Location = new System.Drawing.Point(309, 54);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(142, 22);
            this.label1.TabIndex = 13;
            this.label1.Text = "欲處理之圖層";
            // 
            // Form_InsertShowLayers
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(542, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listSelected);
            this.Controls.Add(this.listOri);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnStart);
            this.Name = "Form_InsertShowLayers";
            this.Text = "Form_InsertShowLayers";
            this.Load += new System.EventHandler(this.Form_InsertShowLayers_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ListBox listOri;
        private System.Windows.Forms.ListBox listSelected;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
    }
}