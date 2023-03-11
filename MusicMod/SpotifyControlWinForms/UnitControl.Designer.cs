namespace SpotifyControlWinForms
{
	partial class UnitControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.locationLabel = new System.Windows.Forms.Label();
			this.browseButton = new System.Windows.Forms.Button();
			this.checkBox = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// locationLabel
			// 
			this.locationLabel.AutoEllipsis = true;
			this.locationLabel.Location = new System.Drawing.Point(175, 6);
			this.locationLabel.Margin = new System.Windows.Forms.Padding(3);
			this.locationLabel.Name = "locationLabel";
			this.locationLabel.Size = new System.Drawing.Size(130, 38);
			this.locationLabel.TabIndex = 7;
			this.locationLabel.Text = "[No file selected]";
			this.locationLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// browseButton
			// 
			this.browseButton.Location = new System.Drawing.Point(334, 6);
			this.browseButton.Name = "browseButton";
			this.browseButton.Size = new System.Drawing.Size(75, 38);
			this.browseButton.TabIndex = 6;
			this.browseButton.Text = "Browse...";
			this.browseButton.UseVisualStyleBackColor = true;
			this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
			// 
			// checkBox
			// 
			this.checkBox.AutoSize = true;
			this.checkBox.Location = new System.Drawing.Point(3, 18);
			this.checkBox.Name = "checkBox";
			this.checkBox.Size = new System.Drawing.Size(15, 14);
			this.checkBox.TabIndex = 8;
			this.checkBox.UseVisualStyleBackColor = true;
			this.checkBox.CheckedChanged += new System.EventHandler(this.checkBox_CheckedChanged);
			// 
			// UnitControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.checkBox);
			this.Controls.Add(this.locationLabel);
			this.Controls.Add(this.browseButton);
			this.Name = "UnitControl";
			this.Size = new System.Drawing.Size(412, 50);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Label locationLabel;
		private Button browseButton;
		private CheckBox checkBox;
	}
}
