namespace SpotifyControlWinForms
{
	partial class Form1
	{
		/// <summary>
		///  Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		///  Clean up any resources being used.
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
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.gameToStringTab = new System.Windows.Forms.TabPage();
			this.stringToMusicTab = new System.Windows.Forms.TabPage();
			this.ConnectionButton = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.tabControl1.SuspendLayout();
			this.stringToMusicTab.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.gameToStringTab);
			this.tabControl1.Controls.Add(this.stringToMusicTab);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(440, 120);
			this.tabControl1.TabIndex = 0;
			// 
			// gameToStringTab
			// 
			this.gameToStringTab.Location = new System.Drawing.Point(4, 24);
			this.gameToStringTab.Name = "gameToStringTab";
			this.gameToStringTab.Padding = new System.Windows.Forms.Padding(3);
			this.gameToStringTab.Size = new System.Drawing.Size(432, 92);
			this.gameToStringTab.TabIndex = 0;
			this.gameToStringTab.Text = "Game > Category";
			this.gameToStringTab.UseVisualStyleBackColor = true;
			// 
			// stringToMusicTab
			// 
			this.stringToMusicTab.Controls.Add(this.ConnectionButton);
			this.stringToMusicTab.Location = new System.Drawing.Point(4, 24);
			this.stringToMusicTab.Name = "stringToMusicTab";
			this.stringToMusicTab.Padding = new System.Windows.Forms.Padding(3);
			this.stringToMusicTab.Size = new System.Drawing.Size(432, 92);
			this.stringToMusicTab.TabIndex = 1;
			this.stringToMusicTab.Text = "Category > Music";
			this.stringToMusicTab.UseVisualStyleBackColor = true;
			// 
			// ConnectionButton
			// 
			this.ConnectionButton.AutoSize = true;
			this.ConnectionButton.Location = new System.Drawing.Point(6, 55);
			this.ConnectionButton.Name = "ConnectionButton";
			this.ConnectionButton.Size = new System.Drawing.Size(75, 25);
			this.ConnectionButton.TabIndex = 0;
			this.ConnectionButton.Text = "button1";
			this.ConnectionButton.UseVisualStyleBackColor = true;
			this.ConnectionButton.Click += new System.EventHandler(this.ConnectionButton_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 144);
			this.Controls.Add(this.tabControl1);
			this.Name = "Form1";
			this.Text = "Controller";
			this.tabControl1.ResumeLayout(false);
			this.stringToMusicTab.ResumeLayout(false);
			this.stringToMusicTab.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private TabControl tabControl1;
		private TabPage gameToStringTab;
		private TabPage stringToMusicTab;
		private OpenFileDialog openFileDialog1;
		private Button ConnectionButton;
	}
}