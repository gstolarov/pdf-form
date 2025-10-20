namespace ProcPdf {
	partial class cvt {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.pdfFile = new System.Windows.Forms.TextBox();
			this.btnMore = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.dlgFile = new System.Windows.Forms.OpenFileDialog();
			this.prog = new System.Windows.Forms.ProgressBar();
			this.label1 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// pdfFile
			// 
			this.pdfFile.Location = new System.Drawing.Point(72, 23);
			this.pdfFile.Name = "pdfFile";
			this.pdfFile.Size = new System.Drawing.Size(265, 20);
			this.pdfFile.TabIndex = 0;
			// 
			// btnMore
			// 
			this.btnMore.Location = new System.Drawing.Point(343, 21);
			this.btnMore.Name = "btnMore";
			this.btnMore.Size = new System.Drawing.Size(31, 23);
			this.btnMore.TabIndex = 1;
			this.btnMore.Text = "...";
			this.btnMore.UseVisualStyleBackColor = true;
			this.btnMore.Click += new System.EventHandler(this.btnMore_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(218, 126);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 9;
			this.button2.Text = "PDF->SVG";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// dlgFile
			// 
			this.dlgFile.DefaultExt = "*.pdf";
			this.dlgFile.FileName = "dlgFile";
			this.dlgFile.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*";
			// 
			// prog
			// 
			this.prog.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.prog.Location = new System.Drawing.Point(0, 170);
			this.prog.Name = "prog";
			this.prog.Size = new System.Drawing.Size(386, 23);
			this.prog.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 30);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(28, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "PDF";
			// 
			// cvt
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(386, 193);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.prog);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.btnMore);
			this.Controls.Add(this.pdfFile);
			this.Name = "cvt";
			this.Text = "XFDL to PDF";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox pdfFile;
		private System.Windows.Forms.Button btnMore;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.OpenFileDialog dlgFile;
		private System.Windows.Forms.ProgressBar prog;
		private System.Windows.Forms.Label label1;
	}
}

