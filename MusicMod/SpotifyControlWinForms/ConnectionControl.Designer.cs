namespace SpotifyControlWinForms
{
	partial class ConnectionControl
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
			this.nameLabel = new System.Windows.Forms.Label();
			this.connectionButton = new System.Windows.Forms.Button();
			this.pingButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// nameLabel
			// 
			this.nameLabel.AutoEllipsis = true;
			this.nameLabel.Location = new System.Drawing.Point(3, 6);
			this.nameLabel.Margin = new System.Windows.Forms.Padding(3);
			this.nameLabel.Name = "nameLabel";
			this.nameLabel.Size = new System.Drawing.Size(312, 38);
			this.nameLabel.TabIndex = 4;
			this.nameLabel.Text = "[Name Label]";
			this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// connectionButton
			// 
			this.connectionButton.Location = new System.Drawing.Point(321, 6);
			this.connectionButton.Name = "connectionButton";
			this.connectionButton.Size = new System.Drawing.Size(88, 38);
			this.connectionButton.TabIndex = 7;
			this.connectionButton.Text = "[Connection]";
			this.connectionButton.UseVisualStyleBackColor = true;
			this.connectionButton.Click += new System.EventHandler(this.connectionButton_Click);
			// 
			// pingButton
			// 
			this.pingButton.Enabled = false;
			this.pingButton.Location = new System.Drawing.Point(240, 6);
			this.pingButton.Name = "pingButton";
			this.pingButton.Size = new System.Drawing.Size(75, 38);
			this.pingButton.TabIndex = 8;
			this.pingButton.Text = "Ping";
			this.pingButton.UseVisualStyleBackColor = true;
			this.pingButton.Click += new System.EventHandler(this.pingButton_Click);
			// 
			// ConnectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.pingButton);
			this.Controls.Add(this.connectionButton);
			this.Controls.Add(this.nameLabel);
			this.Name = "ConnectionControl";
			this.Size = new System.Drawing.Size(410, 48);
			this.ResumeLayout(false);

		}

		#endregion

		private Label nameLabel;
		private Button connectionButton;
		private Button pingButton;
	}
}
