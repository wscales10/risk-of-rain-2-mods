namespace SpotifyAuthenticationWinForms
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.label1 = new System.Windows.Forms.Label();
			this.FlowStateLabel = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.ErrorStateLabel = new System.Windows.Forms.Label();
			this.RestartButton = new System.Windows.Forms.Button();
			this.ConfigureButton = new System.Windows.Forms.Button();
			this.ResetAccessButton = new System.Windows.Forms.Button();
			this.ResetRefreshButton = new System.Windows.Forms.Button();
			this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(67, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Flow State: ";
			// 
			// FlowStateLabel
			// 
			this.FlowStateLabel.AutoSize = true;
			this.FlowStateLabel.Location = new System.Drawing.Point(85, 9);
			this.FlowStateLabel.Name = "FlowStateLabel";
			this.FlowStateLabel.Size = new System.Drawing.Size(12, 15);
			this.FlowStateLabel.TabIndex = 1;
			this.FlowStateLabel.Text = "-";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(12, 31);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(67, 15);
			this.label3.TabIndex = 2;
			this.label3.Text = "Error State: ";
			// 
			// ErrorStateLabel
			// 
			this.ErrorStateLabel.AutoSize = true;
			this.ErrorStateLabel.Location = new System.Drawing.Point(85, 31);
			this.ErrorStateLabel.Name = "ErrorStateLabel";
			this.ErrorStateLabel.Size = new System.Drawing.Size(12, 15);
			this.ErrorStateLabel.TabIndex = 3;
			this.ErrorStateLabel.Text = "-";
			// 
			// RestartButton
			// 
			this.RestartButton.Location = new System.Drawing.Point(12, 49);
			this.RestartButton.Name = "RestartButton";
			this.RestartButton.Size = new System.Drawing.Size(117, 23);
			this.RestartButton.TabIndex = 4;
			this.RestartButton.Text = "Restart";
			this.RestartButton.UseVisualStyleBackColor = true;
			this.RestartButton.Click += new System.EventHandler(this.RestartButton_Click);
			// 
			// ConfigureButton
			// 
			this.ConfigureButton.Location = new System.Drawing.Point(135, 49);
			this.ConfigureButton.Name = "ConfigureButton";
			this.ConfigureButton.Size = new System.Drawing.Size(122, 23);
			this.ConfigureButton.TabIndex = 5;
			this.ConfigureButton.Text = "Configure";
			this.ConfigureButton.UseVisualStyleBackColor = true;
			this.ConfigureButton.Click += new System.EventHandler(this.ConfigureButton_Click);
			// 
			// ResetAccessButton
			// 
			this.ResetAccessButton.Location = new System.Drawing.Point(12, 78);
			this.ResetAccessButton.Name = "ResetAccessButton";
			this.ResetAccessButton.Size = new System.Drawing.Size(117, 23);
			this.ResetAccessButton.TabIndex = 6;
			this.ResetAccessButton.Text = "Reset Access Token";
			this.ResetAccessButton.UseVisualStyleBackColor = true;
			this.ResetAccessButton.Click += new System.EventHandler(this.ResetAccessButton_Click);
			// 
			// ResetRefreshButton
			// 
			this.ResetRefreshButton.Location = new System.Drawing.Point(135, 78);
			this.ResetRefreshButton.Name = "ResetRefreshButton";
			this.ResetRefreshButton.Size = new System.Drawing.Size(122, 23);
			this.ResetRefreshButton.TabIndex = 7;
			this.ResetRefreshButton.Text = "Reset Refresh Token";
			this.ResetRefreshButton.UseVisualStyleBackColor = true;
			this.ResetRefreshButton.Click += new System.EventHandler(this.ResetRefreshButton_Click);
			// 
			// notifyIcon
			// 
			this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
			this.notifyIcon.Text = "Spotify Authorisation Helper";
			this.notifyIcon.Visible = true;
			this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(269, 129);
			this.Controls.Add(this.ResetRefreshButton);
			this.Controls.Add(this.ResetAccessButton);
			this.Controls.Add(this.ConfigureButton);
			this.Controls.Add(this.RestartButton);
			this.Controls.Add(this.ErrorStateLabel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.FlowStateLabel);
			this.Controls.Add(this.label1);
			this.Name = "Form1";
			this.Text = "Spotify Authorisation";
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Label label1;
		private Label FlowStateLabel;
		private Label label3;
		private Label ErrorStateLabel;
		private Button RestartButton;
		private Button ConfigureButton;
		private Button ResetAccessButton;
		private Button ResetRefreshButton;
		private NotifyIcon notifyIcon;
	}
}