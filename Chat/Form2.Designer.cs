namespace Test
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.NameTextBox = new System.Windows.Forms.TextBox();
            this.ID = new System.Windows.Forms.TextBox();
            this.Division = new System.Windows.Forms.TextBox();
            this.Password = new System.Windows.Forms.TextBox();
            this.Confirmation = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.isAdmin = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.AdminPass = new System.Windows.Forms.TextBox();
            this.AdminID = new System.Windows.Forms.TextBox();
            this.AdminPassLabel = new System.Windows.Forms.Label();
            this.AdminIDLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 23);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 54);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "UserID";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 84);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(44, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Division";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 110);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 3;
            this.label4.Text = "Password";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 136);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.MaximumSize = new System.Drawing.Size(75, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 26);
            this.label5.TabIndex = 4;
            this.label5.Text = "Password Confirmation";
            this.label5.Click += new System.EventHandler(this.label5_Click);
            // 
            // NameTextBox
            // 
            this.NameTextBox.Location = new System.Drawing.Point(84, 20);
            this.NameTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.NameTextBox.Name = "NameTextBox";
            this.NameTextBox.Size = new System.Drawing.Size(141, 20);
            this.NameTextBox.TabIndex = 5;
            // 
            // ID
            // 
            this.ID.Location = new System.Drawing.Point(84, 50);
            this.ID.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.ID.Name = "ID";
            this.ID.Size = new System.Drawing.Size(141, 20);
            this.ID.TabIndex = 6;
            // 
            // Division
            // 
            this.Division.Location = new System.Drawing.Point(84, 80);
            this.Division.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Division.Name = "Division";
            this.Division.Size = new System.Drawing.Size(141, 20);
            this.Division.TabIndex = 7;
            // 
            // Password
            // 
            this.Password.Location = new System.Drawing.Point(84, 110);
            this.Password.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Password.Name = "Password";
            this.Password.PasswordChar = '*';
            this.Password.Size = new System.Drawing.Size(141, 20);
            this.Password.TabIndex = 8;
            this.Password.TextChanged += new System.EventHandler(this.Password_TextChanged);
            // 
            // Confirmation
            // 
            this.Confirmation.Location = new System.Drawing.Point(84, 136);
            this.Confirmation.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Confirmation.Name = "Confirmation";
            this.Confirmation.PasswordChar = '*';
            this.Confirmation.Size = new System.Drawing.Size(141, 20);
            this.Confirmation.TabIndex = 9;
            this.Confirmation.TextChanged += new System.EventHandler(this.Confirmation_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(92, 224);
            this.button1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(56, 24);
            this.button1.TabIndex = 10;
            this.button1.Text = "Register";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.Register_Click);
            // 
            // checkBox1
            // 
            this.isAdmin.AutoSize = true;
            this.isAdmin.Location = new System.Drawing.Point(84, 175);
            this.isAdmin.Name = "checkBox1";
            this.isAdmin.Size = new System.Drawing.Size(15, 14);
            this.isAdmin.TabIndex = 11;
            this.isAdmin.UseVisualStyleBackColor = true;
            this.isAdmin.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 175);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.MaximumSize = new System.Drawing.Size(75, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(36, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Admin";
            // 
            // AdminPass
            // 
            this.AdminPass.Location = new System.Drawing.Point(84, 222);
            this.AdminPass.Margin = new System.Windows.Forms.Padding(2);
            this.AdminPass.Name = "AdminPass";
            this.AdminPass.PasswordChar = '*';
            this.AdminPass.Size = new System.Drawing.Size(141, 20);
            this.AdminPass.TabIndex = 16;
            this.AdminPass.Visible = false;
            this.AdminPass.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // AdminID
            // 
            this.AdminID.Location = new System.Drawing.Point(84, 196);
            this.AdminID.Margin = new System.Windows.Forms.Padding(2);
            this.AdminID.Name = "AdminID";
            this.AdminID.PasswordChar = '*';
            this.AdminID.Size = new System.Drawing.Size(141, 20);
            this.AdminID.TabIndex = 15;
            this.AdminID.Visible = false;
            this.AdminID.TextChanged += new System.EventHandler(this.textBox2_TextChanged);
            // 
            // AdminPassLabel
            // 
            this.AdminPassLabel.AutoSize = true;
            this.AdminPassLabel.Location = new System.Drawing.Point(9, 222);
            this.AdminPassLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AdminPassLabel.MaximumSize = new System.Drawing.Size(75, 0);
            this.AdminPassLabel.Name = "AdminPassLabel";
            this.AdminPassLabel.Size = new System.Drawing.Size(53, 26);
            this.AdminPassLabel.TabIndex = 14;
            this.AdminPassLabel.Text = "Admin Password";
            this.AdminPassLabel.Visible = false;
            this.AdminPassLabel.Click += new System.EventHandler(this.label7_Click);
            // 
            // AdminIDLabel
            // 
            this.AdminIDLabel.AutoSize = true;
            this.AdminIDLabel.Location = new System.Drawing.Point(9, 196);
            this.AdminIDLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.AdminIDLabel.Name = "AdminIDLabel";
            this.AdminIDLabel.Size = new System.Drawing.Size(47, 13);
            this.AdminIDLabel.TabIndex = 13;
            this.AdminIDLabel.Text = "AdminID";
            this.AdminIDLabel.Visible = false;
            this.AdminIDLabel.Click += new System.EventHandler(this.label8_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(244, 275);
            this.Controls.Add(this.AdminPass);
            this.Controls.Add(this.AdminID);
            this.Controls.Add(this.AdminPassLabel);
            this.Controls.Add(this.AdminIDLabel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.isAdmin);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Confirmation);
            this.Controls.Add(this.Password);
            this.Controls.Add(this.Division);
            this.Controls.Add(this.ID);
            this.Controls.Add(this.NameTextBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "Form2";
            this.Text = "Register";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox NameTextBox;
        private System.Windows.Forms.TextBox ID;
        private System.Windows.Forms.TextBox Division;
        private System.Windows.Forms.TextBox Password;
        private System.Windows.Forms.TextBox Confirmation;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox isAdmin;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox AdminPass;
        private System.Windows.Forms.TextBox AdminID;
        private System.Windows.Forms.Label AdminPassLabel;
        private System.Windows.Forms.Label AdminIDLabel;
    }
}