namespace SystemTrading.Forms
{
    partial class UserInfoForm
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
            this.idTitle = new System.Windows.Forms.Label();
            this.idLabel = new System.Windows.Forms.Label();
            this.nameTitle = new System.Windows.Forms.Label();
            this.nameLabel = new System.Windows.Forms.Label();
            this.keyboardTitle = new System.Windows.Forms.Label();
            this.keyboardLabel = new System.Windows.Forms.Label();
            this.firewallLabel = new System.Windows.Forms.Label();
            this.firewallTitle = new System.Windows.Forms.Label();
            this.accountTitle = new System.Windows.Forms.Label();
            this.accountComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // idTitle
            // 
            this.idTitle.AutoSize = true;
            this.idTitle.Location = new System.Drawing.Point(20, 20);
            this.idTitle.Name = "idTitle";
            this.idTitle.Size = new System.Drawing.Size(44, 12);
            this.idTitle.TabIndex = 0;
            this.idTitle.Text = "접속 ID";
            // 
            // idLabel
            // 
            this.idLabel.AutoSize = true;
            this.idLabel.Location = new System.Drawing.Point(132, 20);
            this.idLabel.Name = "idLabel";
            this.idLabel.Size = new System.Drawing.Size(79, 12);
            this.idLabel.TabIndex = 1;
            this.idLabel.Text = "(접속 아이디)";
            // 
            // nameTitle
            // 
            this.nameTitle.AutoSize = true;
            this.nameTitle.Location = new System.Drawing.Point(20, 50);
            this.nameTitle.Name = "nameTitle";
            this.nameTitle.Size = new System.Drawing.Size(29, 12);
            this.nameTitle.TabIndex = 2;
            this.nameTitle.Text = "이름";
            // 
            // nameLabel
            // 
            this.nameLabel.AutoSize = true;
            this.nameLabel.Location = new System.Drawing.Point(132, 50);
            this.nameLabel.Name = "nameLabel";
            this.nameLabel.Size = new System.Drawing.Size(39, 12);
            this.nameLabel.TabIndex = 3;
            this.nameLabel.Text = "(이름)";
            // 
            // keyboardTitle
            // 
            this.keyboardTitle.AutoSize = true;
            this.keyboardTitle.Location = new System.Drawing.Point(20, 110);
            this.keyboardTitle.Name = "keyboardTitle";
            this.keyboardTitle.Size = new System.Drawing.Size(69, 12);
            this.keyboardTitle.TabIndex = 4;
            this.keyboardTitle.Text = "키보드 보안";
            // 
            // keyboardLabel
            // 
            this.keyboardLabel.AutoSize = true;
            this.keyboardLabel.Location = new System.Drawing.Point(132, 110);
            this.keyboardLabel.Name = "keyboardLabel";
            this.keyboardLabel.Size = new System.Drawing.Size(107, 12);
            this.keyboardLabel.TabIndex = 5;
            this.keyboardLabel.Text = "(키보드 보안 상태)";
            // 
            // firewallLabel
            // 
            this.firewallLabel.AutoSize = true;
            this.firewallLabel.Location = new System.Drawing.Point(132, 140);
            this.firewallLabel.Name = "firewallLabel";
            this.firewallLabel.Size = new System.Drawing.Size(107, 12);
            this.firewallLabel.TabIndex = 7;
            this.firewallLabel.Text = "(방화벽 설정 여부)";
            // 
            // firewallTitle
            // 
            this.firewallTitle.AutoSize = true;
            this.firewallTitle.Location = new System.Drawing.Point(20, 140);
            this.firewallTitle.Name = "firewallTitle";
            this.firewallTitle.Size = new System.Drawing.Size(41, 12);
            this.firewallTitle.TabIndex = 6;
            this.firewallTitle.Text = "방화벽";
            // 
            // accountTitle
            // 
            this.accountTitle.AutoSize = true;
            this.accountTitle.Location = new System.Drawing.Point(20, 80);
            this.accountTitle.Name = "accountTitle";
            this.accountTitle.Size = new System.Drawing.Size(69, 12);
            this.accountTitle.TabIndex = 8;
            this.accountTitle.Text = "계좌 리스트";
            // 
            // accountComboBox
            // 
            this.accountComboBox.BackColor = System.Drawing.SystemColors.Window;
            this.accountComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.accountComboBox.FormattingEnabled = true;
            this.accountComboBox.Location = new System.Drawing.Point(134, 75);
            this.accountComboBox.Name = "accountComboBox";
            this.accountComboBox.Size = new System.Drawing.Size(92, 20);
            this.accountComboBox.TabIndex = 9;
            // 
            // UserInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(257, 172);
            this.Controls.Add(this.accountComboBox);
            this.Controls.Add(this.accountTitle);
            this.Controls.Add(this.firewallLabel);
            this.Controls.Add(this.firewallTitle);
            this.Controls.Add(this.keyboardLabel);
            this.Controls.Add(this.keyboardTitle);
            this.Controls.Add(this.nameLabel);
            this.Controls.Add(this.nameTitle);
            this.Controls.Add(this.idLabel);
            this.Controls.Add(this.idTitle);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserInfoForm";
            this.ShowIcon = false;
            this.Text = "사용자 정보";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label idTitle;
        private System.Windows.Forms.Label idLabel;
        private System.Windows.Forms.Label nameTitle;
        private System.Windows.Forms.Label nameLabel;
        private System.Windows.Forms.Label keyboardTitle;
        private System.Windows.Forms.Label keyboardLabel;
        private System.Windows.Forms.Label firewallLabel;
        private System.Windows.Forms.Label firewallTitle;
        private System.Windows.Forms.Label accountTitle;
        private System.Windows.Forms.ComboBox accountComboBox;
    }
}