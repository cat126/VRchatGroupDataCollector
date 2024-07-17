namespace VRchatDataCollector
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
            button1 = new Button();
            UserNameBox = new TextBox();
            PasswordBox = new TextBox();
            button2 = new Button();
            label1 = new Label();
            label2 = new Label();
            Output = new Label();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(161, 184);
            button1.Name = "button1";
            button1.Size = new Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "Preview";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // UserNameBox
            // 
            UserNameBox.Location = new Point(161, 56);
            UserNameBox.Name = "UserNameBox";
            UserNameBox.Size = new Size(157, 23);
            UserNameBox.TabIndex = 1;
            UserNameBox.TextChanged += UserNameBox_TextChanged;
            // 
            // PasswordBox
            // 
            PasswordBox.Location = new Point(161, 130);
            PasswordBox.Name = "PasswordBox";
            PasswordBox.PasswordChar = '●';
            PasswordBox.Size = new Size(157, 23);
            PasswordBox.TabIndex = 2;
            // 
            // button2
            // 
            button2.Location = new Point(243, 184);
            button2.Name = "button2";
            button2.Size = new Size(75, 23);
            button2.TabIndex = 3;
            button2.Text = "Send";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(47, 64);
            label1.Name = "label1";
            label1.Size = new Size(62, 15);
            label1.TabIndex = 4;
            label1.Text = "UserName";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(47, 138);
            label2.Name = "label2";
            label2.Size = new Size(57, 15);
            label2.TabIndex = 5;
            label2.Text = "Password";
            // 
            // Output
            // 
            Output.AutoSize = true;
            Output.Location = new Point(352, 56);
            Output.Name = "Output";
            Output.Size = new Size(237, 45);
            Output.TabIndex = 6;
            Output.Text = "Use the same username and password \r\nyou used for the website,not your vrchat or \r\ndiscord  password. Close vrchat first.";
            Output.Click += Output_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(597, 337);
            Controls.Add(Output);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(button2);
            Controls.Add(PasswordBox);
            Controls.Add(UserNameBox);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button button1;
        private TextBox UserNameBox;
        private TextBox PasswordBox;
        private Button button2;
        private Label label1;
        private Label label2;
        private Label Output;
    }
}
