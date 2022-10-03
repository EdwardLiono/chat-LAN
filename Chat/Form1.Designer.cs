namespace Test
{
    partial class Form1
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
            this.GroupsBox = new System.Windows.Forms.Panel();
            this.ChatBox = new System.Windows.Forms.Panel();
            this.Find = new System.Windows.Forms.Button();
            this.Make_Group = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.msgTextBox = new System.Windows.Forms.TextBox();
            this.FileButton = new System.Windows.Forms.Button();
            this.Imagebutton = new System.Windows.Forms.Button();
            this.Admin = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // GroupsBox
            // 
            this.GroupsBox.BackColor = System.Drawing.SystemColors.ControlDark;
            this.GroupsBox.Location = new System.Drawing.Point(0, 44);
            this.GroupsBox.Margin = new System.Windows.Forms.Padding(2);
            this.GroupsBox.Name = "GroupsBox";
            this.GroupsBox.Size = new System.Drawing.Size(219, 323);
            this.GroupsBox.TabIndex = 0;
            this.GroupsBox.Paint += new System.Windows.Forms.PaintEventHandler(this.GroupsBox_Paint);
            // 
            // ChatBox
            // 
            this.ChatBox.AutoScroll = true;
            this.ChatBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ChatBox.Location = new System.Drawing.Point(219, 44);
            this.ChatBox.Margin = new System.Windows.Forms.Padding(2);
            this.ChatBox.Name = "ChatBox";
            this.ChatBox.Size = new System.Drawing.Size(381, 211);
            this.ChatBox.TabIndex = 1;
            this.ChatBox.ControlAdded += new System.Windows.Forms.ControlEventHandler(this.ChatBox_ControlAdded);
            this.ChatBox.Paint += new System.Windows.Forms.PaintEventHandler(this.ChatBox_Paint);
            // 
            // Find
            // 
            this.Find.Location = new System.Drawing.Point(16, 10);
            this.Find.Margin = new System.Windows.Forms.Padding(2);
            this.Find.Name = "Find";
            this.Find.Size = new System.Drawing.Size(56, 20);
            this.Find.TabIndex = 2;
            this.Find.Text = "Find ID";
            this.Find.UseVisualStyleBackColor = true;
            this.Find.Click += new System.EventHandler(this.Find_Click);
            // 
            // Make_Group
            // 
            this.Make_Group.Location = new System.Drawing.Point(143, 10);
            this.Make_Group.Margin = new System.Windows.Forms.Padding(2);
            this.Make_Group.Name = "Make_Group";
            this.Make_Group.Size = new System.Drawing.Size(56, 20);
            this.Make_Group.TabIndex = 3;
            this.Make_Group.Text = "Group";
            this.Make_Group.UseVisualStyleBackColor = true;
            this.Make_Group.Click += new System.EventHandler(this.Make_Group_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(219, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 45);
            this.panel1.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Friend Division";
            // 
            // msgTextBox
            // 
            this.msgTextBox.Location = new System.Drawing.Point(219, 253);
            this.msgTextBox.MaximumSize = new System.Drawing.Size(381, 4);
            this.msgTextBox.MinimumSize = new System.Drawing.Size(4, 88);
            this.msgTextBox.Multiline = true;
            this.msgTextBox.Name = "msgTextBox";
            this.msgTextBox.Size = new System.Drawing.Size(381, 88);
            this.msgTextBox.TabIndex = 0;
            this.msgTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // FileButton
            // 
            this.FileButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.FileButton.Location = new System.Drawing.Point(219, 340);
            this.FileButton.Name = "FileButton";
            this.FileButton.Size = new System.Drawing.Size(125, 26);
            this.FileButton.TabIndex = 4;
            this.FileButton.Text = "Send File";
            this.FileButton.UseVisualStyleBackColor = true;
            this.FileButton.Click += new System.EventHandler(this.FileButton_Click);
            // 
            // Imagebutton
            // 
            this.Imagebutton.BackColor = System.Drawing.Color.White;
            this.Imagebutton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Imagebutton.Location = new System.Drawing.Point(343, 340);
            this.Imagebutton.Name = "Imagebutton";
            this.Imagebutton.Size = new System.Drawing.Size(125, 26);
            this.Imagebutton.TabIndex = 5;
            this.Imagebutton.Text = "Send Image";
            this.Imagebutton.UseVisualStyleBackColor = false;
            this.Imagebutton.Click += new System.EventHandler(this.Imagebutton_Click);
            // 
            // Admin
            // 
            this.Admin.Location = new System.Drawing.Point(78, 10);
            this.Admin.Margin = new System.Windows.Forms.Padding(2);
            this.Admin.Name = "Admin";
            this.Admin.Size = new System.Drawing.Size(56, 20);
            this.Admin.TabIndex = 6;
            this.Admin.Text = "Admin";
            this.Admin.UseVisualStyleBackColor = true;
            this.Admin.Visible = false;
            this.Admin.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(600, 366);
            this.Controls.Add(this.Admin);
            this.Controls.Add(this.Imagebutton);
            this.Controls.Add(this.FileButton);
            this.Controls.Add(this.msgTextBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Make_Group);
            this.Controls.Add(this.Find);
            this.Controls.Add(this.ChatBox);
            this.Controls.Add(this.GroupsBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Form1";
            this.Text = "Chat";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel ChatBox;
        private System.Windows.Forms.Button Find;
        private System.Windows.Forms.Button Make_Group;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Panel GroupsBox;
        private System.Windows.Forms.TextBox msgTextBox;
        private System.Windows.Forms.Button FileButton;
        private System.Windows.Forms.Button Imagebutton;
        private System.Windows.Forms.Button Admin;
    }
}

