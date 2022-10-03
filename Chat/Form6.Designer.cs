namespace Test
{
    partial class Form6
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.ChatBox = new System.Windows.Forms.Panel();
            this.GroupsBox = new System.Windows.Forms.Panel();
            this.Send_Broadcast = new System.Windows.Forms.Button();
            this.UserIDSearch = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.Back = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.Controls.Add(this.label1);
            this.panel1.Location = new System.Drawing.Point(219, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(381, 45);
            this.panel1.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(13, 14);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name Division";
            // 
            // ChatBox
            // 
            this.ChatBox.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ChatBox.Location = new System.Drawing.Point(219, 44);
            this.ChatBox.Margin = new System.Windows.Forms.Padding(2);
            this.ChatBox.Name = "ChatBox";
            this.ChatBox.Size = new System.Drawing.Size(381, 323);
            this.ChatBox.TabIndex = 10;
            // 
            // GroupsBox
            // 
            this.GroupsBox.BackColor = System.Drawing.SystemColors.ControlDark;
            this.GroupsBox.Location = new System.Drawing.Point(0, 71);
            this.GroupsBox.Margin = new System.Windows.Forms.Padding(2);
            this.GroupsBox.Name = "GroupsBox";
            this.GroupsBox.Size = new System.Drawing.Size(219, 296);
            this.GroupsBox.TabIndex = 8;
            // 
            // Send_Broadcast
            // 
            this.Send_Broadcast.Location = new System.Drawing.Point(95, 10);
            this.Send_Broadcast.Margin = new System.Windows.Forms.Padding(2);
            this.Send_Broadcast.Name = "Send_Broadcast";
            this.Send_Broadcast.Size = new System.Drawing.Size(93, 20);
            this.Send_Broadcast.TabIndex = 12;
            this.Send_Broadcast.Text = "Send Broadcast";
            this.Send_Broadcast.UseVisualStyleBackColor = true;
            this.Send_Broadcast.Click += new System.EventHandler(this.Send_Broadcast_Click);
            // 
            // UserIDSearch
            // 
            this.UserIDSearch.Location = new System.Drawing.Point(59, 43);
            this.UserIDSearch.Name = "UserIDSearch";
            this.UserIDSearch.Size = new System.Drawing.Size(129, 20);
            this.UserIDSearch.TabIndex = 15;
            this.UserIDSearch.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.UserIDSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox1_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "UserID";
            // 
            // Back
            // 
            this.Back.Location = new System.Drawing.Point(16, 10);
            this.Back.Margin = new System.Windows.Forms.Padding(2);
            this.Back.Name = "Back";
            this.Back.Size = new System.Drawing.Size(75, 20);
            this.Back.TabIndex = 17;
            this.Back.Text = "Back";
            this.Back.UseVisualStyleBackColor = true;
            this.Back.Click += new System.EventHandler(this.Back_Click);
            // 
            // Form6
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(600, 366);
            this.Controls.Add(this.Back);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.UserIDSearch);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.Send_Broadcast);
            this.Controls.Add(this.ChatBox);
            this.Controls.Add(this.GroupsBox);
            this.Name = "Form6";
            this.Text = "Admin Menu";
            this.Load += new System.EventHandler(this.Form6_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel ChatBox;
        public System.Windows.Forms.Panel GroupsBox;
        private System.Windows.Forms.Button Send_Broadcast;
        private System.Windows.Forms.TextBox UserIDSearch;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button Back;
    }
}