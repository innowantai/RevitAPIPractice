﻿namespace _6_ReadDWG
{
    partial class MainForm
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
            this.btnCreateBeamsColumns = new System.Windows.Forms.Button();
            this.btnCreateFloors = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnCreateBeamsColumns
            // 
            this.btnCreateBeamsColumns.Font = new System.Drawing.Font("新細明體", 24F);
            this.btnCreateBeamsColumns.Location = new System.Drawing.Point(227, 54);
            this.btnCreateBeamsColumns.Name = "btnCreateBeamsColumns";
            this.btnCreateBeamsColumns.Size = new System.Drawing.Size(237, 107);
            this.btnCreateBeamsColumns.TabIndex = 0;
            this.btnCreateBeamsColumns.Text = "建立梁柱";
            this.btnCreateBeamsColumns.UseVisualStyleBackColor = true;
            this.btnCreateBeamsColumns.Click += new System.EventHandler(this.btnCreateBeamsColumns_Click);
            // 
            // btnCreateFloors
            // 
            this.btnCreateFloors.Font = new System.Drawing.Font("新細明體", 24F);
            this.btnCreateFloors.Location = new System.Drawing.Point(227, 233);
            this.btnCreateFloors.Name = "btnCreateFloors";
            this.btnCreateFloors.Size = new System.Drawing.Size(237, 107);
            this.btnCreateFloors.TabIndex = 1;
            this.btnCreateFloors.Text = "建立樓板";
            this.btnCreateFloors.UseVisualStyleBackColor = true;
            this.btnCreateFloors.Click += new System.EventHandler(this.btnCreateFloors_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnCreateFloors);
            this.Controls.Add(this.btnCreateBeamsColumns);
            this.Name = "MainForm";
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnCreateBeamsColumns;
        private System.Windows.Forms.Button btnCreateFloors;
    }
}