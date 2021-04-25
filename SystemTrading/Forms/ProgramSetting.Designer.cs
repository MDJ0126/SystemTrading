namespace SystemTrading.Forms
{
    partial class ProgramSetting
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
            this.lineNotifyTokenTitle = new System.Windows.Forms.Label();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lineTokeyTextBox = new System.Windows.Forms.TextBox();
            this.ReferenceStockListTitle = new System.Windows.Forms.Label();
            this.ReferenceStockListText = new System.Windows.Forms.TextBox();
            this.saveButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // lineNotifyTokenTitle
            // 
            this.lineNotifyTokenTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lineNotifyTokenTitle.AutoSize = true;
            this.lineNotifyTokenTitle.Location = new System.Drawing.Point(3, 10);
            this.lineNotifyTokenTitle.Name = "lineNotifyTokenTitle";
            this.lineNotifyTokenTitle.Size = new System.Drawing.Size(144, 12);
            this.lineNotifyTokenTitle.TabIndex = 0;
            this.lineNotifyTokenTitle.Text = "Line Notify Token";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.ColumnCount = 2;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 32.69639F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 67.30361F));
            this.tableLayoutPanel.Controls.Add(this.lineNotifyTokenTitle, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.lineTokeyTextBox, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.ReferenceStockListTitle, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.ReferenceStockListText, 1, 1);
            this.tableLayoutPanel.Location = new System.Drawing.Point(12, 1);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 10;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(460, 323);
            this.tableLayoutPanel.TabIndex = 1;
            // 
            // lineTokeyTextBox
            // 
            this.lineTokeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lineTokeyTextBox.Location = new System.Drawing.Point(153, 5);
            this.lineTokeyTextBox.Name = "lineTokeyTextBox";
            this.lineTokeyTextBox.Size = new System.Drawing.Size(304, 21);
            this.lineTokeyTextBox.TabIndex = 1;
            // 
            // ReferenceStockListTitle
            // 
            this.ReferenceStockListTitle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ReferenceStockListTitle.AutoSize = true;
            this.ReferenceStockListTitle.Location = new System.Drawing.Point(3, 42);
            this.ReferenceStockListTitle.Name = "ReferenceStockListTitle";
            this.ReferenceStockListTitle.Size = new System.Drawing.Size(144, 12);
            this.ReferenceStockListTitle.TabIndex = 0;
            this.ReferenceStockListTitle.Text = "참고 종목 리스트";
            // 
            // ReferenceStockListText
            // 
            this.ReferenceStockListText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.ReferenceStockListText.Location = new System.Drawing.Point(153, 37);
            this.ReferenceStockListText.Name = "ReferenceStockListText";
            this.ReferenceStockListText.ReadOnly = true;
            this.ReferenceStockListText.Size = new System.Drawing.Size(304, 21);
            this.ReferenceStockListText.TabIndex = 1;
            // 
            // saveButton
            // 
            this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.saveButton.Location = new System.Drawing.Point(161, 330);
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(150, 25);
            this.saveButton.TabIndex = 2;
            this.saveButton.Text = "저장";
            this.saveButton.UseVisualStyleBackColor = true;
            this.saveButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // ProgramSetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.saveButton);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgramSetting";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "프로그램 설정";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lineNotifyTokenTitle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.TextBox lineTokeyTextBox;
        private System.Windows.Forms.Button saveButton;
        private System.Windows.Forms.Label ReferenceStockListTitle;
        private System.Windows.Forms.TextBox ReferenceStockListText;
    }
}