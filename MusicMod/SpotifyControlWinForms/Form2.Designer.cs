namespace SpotifyControlWinForms
{
	partial class Form2
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
			this.tabControl = new System.Windows.Forms.TabControl();
			this.unitsPage = new System.Windows.Forms.TabPage();
			this.unitsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.connectionsPage = new System.Windows.Forms.TabPage();
			this.connectionsLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
			this.testPage = new System.Windows.Forms.TabPage();
			this.testPage1 = new SpotifyControlWinForms.TestPage();
			this.tabControl.SuspendLayout();
			this.unitsPage.SuspendLayout();
			this.connectionsPage.SuspendLayout();
			this.testPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.unitsPage);
			this.tabControl.Controls.Add(this.connectionsPage);
			this.tabControl.Controls.Add(this.testPage);
			this.tabControl.Location = new System.Drawing.Point(12, 12);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(440, 417);
			this.tabControl.TabIndex = 0;
			// 
			// unitsPage
			// 
			this.unitsPage.Controls.Add(this.unitsLayoutPanel);
			this.unitsPage.Location = new System.Drawing.Point(4, 24);
			this.unitsPage.Name = "unitsPage";
			this.unitsPage.Padding = new System.Windows.Forms.Padding(3);
			this.unitsPage.Size = new System.Drawing.Size(432, 389);
			this.unitsPage.TabIndex = 1;
			this.unitsPage.Text = "Units";
			this.unitsPage.UseVisualStyleBackColor = true;
			// 
			// unitsLayoutPanel
			// 
			this.unitsLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.unitsLayoutPanel.Location = new System.Drawing.Point(6, 6);
			this.unitsLayoutPanel.Name = "unitsLayoutPanel";
			this.unitsLayoutPanel.Size = new System.Drawing.Size(420, 377);
			this.unitsLayoutPanel.TabIndex = 0;
			// 
			// connectionsPage
			// 
			this.connectionsPage.Controls.Add(this.connectionsLayoutPanel);
			this.connectionsPage.Location = new System.Drawing.Point(4, 24);
			this.connectionsPage.Name = "connectionsPage";
			this.connectionsPage.Padding = new System.Windows.Forms.Padding(3);
			this.connectionsPage.Size = new System.Drawing.Size(432, 389);
			this.connectionsPage.TabIndex = 0;
			this.connectionsPage.Text = "Connections";
			this.connectionsPage.UseVisualStyleBackColor = true;
			// 
			// connectionsLayoutPanel
			// 
			this.connectionsLayoutPanel.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
			this.connectionsLayoutPanel.Location = new System.Drawing.Point(6, 6);
			this.connectionsLayoutPanel.Name = "connectionsLayoutPanel";
			this.connectionsLayoutPanel.Size = new System.Drawing.Size(420, 377);
			this.connectionsLayoutPanel.TabIndex = 0;
			// 
			// testPage
			// 
			this.testPage.Controls.Add(this.testPage1);
			this.testPage.Location = new System.Drawing.Point(4, 24);
			this.testPage.Name = "testPage";
			this.testPage.Padding = new System.Windows.Forms.Padding(3);
			this.testPage.Size = new System.Drawing.Size(432, 389);
			this.testPage.TabIndex = 2;
			this.testPage.Text = "Test";
			this.testPage.UseVisualStyleBackColor = true;
			// 
			// testPage1
			// 
			this.testPage1.Location = new System.Drawing.Point(6, 6);
			this.testPage1.Name = "testPage1";
			this.testPage1.Size = new System.Drawing.Size(420, 377);
			this.testPage1.TabIndex = 0;
			// 
			// Form2
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(464, 441);
			this.Controls.Add(this.tabControl);
			this.KeyPreview = true;
			this.Name = "Form2";
			this.Text = "Controller";
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form2_KeyPress);
			this.tabControl.ResumeLayout(false);
			this.unitsPage.ResumeLayout(false);
			this.connectionsPage.ResumeLayout(false);
			this.testPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private TabControl tabControl;
		private TabPage connectionsPage;
		private TabPage unitsPage;
		private FlowLayoutPanel connectionsLayoutPanel;
		private FlowLayoutPanel unitsLayoutPanel;
		private TabPage testPage;
		private TestPage testPage1;
	}
}