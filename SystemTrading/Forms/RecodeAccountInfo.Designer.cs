
namespace SystemTrading.Forms
{
    partial class RecodeAccountInfo
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RecodeAccountInfo));
            this.label10 = new System.Windows.Forms.Label();
            this.accountListComboBox = new System.Windows.Forms.ComboBox();
            this.accountDataGridView = new System.Windows.Forms.DataGridView();
            this.날짜 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.예수금 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.D2예수금 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.추정자산 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.당일손익률 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.당일손익금 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.accountDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label10.Location = new System.Drawing.Point(526, 5);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(80, 20);
            this.label10.TabIndex = 24;
            this.label10.Text = "조회 계좌";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // accountListComboBox
            // 
            this.accountListComboBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.accountListComboBox.BackColor = System.Drawing.Color.White;
            this.accountListComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.accountListComboBox.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.accountListComboBox.FormattingEnabled = true;
            this.accountListComboBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.accountListComboBox.Location = new System.Drawing.Point(612, 7);
            this.accountListComboBox.Name = "accountListComboBox";
            this.accountListComboBox.Size = new System.Drawing.Size(183, 20);
            this.accountListComboBox.TabIndex = 23;
            this.accountListComboBox.SelectedIndexChanged += new System.EventHandler(this.accountListComboBox_SelectedIndexChanged);
            // 
            // accountDataGridView
            // 
            this.accountDataGridView.AllowUserToAddRows = false;
            this.accountDataGridView.AllowUserToDeleteRows = false;
            this.accountDataGridView.AllowUserToResizeColumns = false;
            this.accountDataGridView.AllowUserToResizeRows = false;
            this.accountDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.accountDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.accountDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.날짜,
            this.예수금,
            this.D2예수금,
            this.추정자산,
            this.당일손익률,
            this.당일손익금});
            this.accountDataGridView.Location = new System.Drawing.Point(0, 35);
            this.accountDataGridView.Name = "accountDataGridView";
            this.accountDataGridView.ReadOnly = true;
            this.accountDataGridView.RowHeadersVisible = false;
            this.accountDataGridView.RowTemplate.Height = 23;
            this.accountDataGridView.Size = new System.Drawing.Size(801, 416);
            this.accountDataGridView.TabIndex = 0;
            // 
            // 날짜
            // 
            this.날짜.HeaderText = "날짜";
            this.날짜.Name = "날짜";
            this.날짜.ReadOnly = true;
            // 
            // 예수금
            // 
            this.예수금.HeaderText = "예수금";
            this.예수금.Name = "예수금";
            this.예수금.ReadOnly = true;
            // 
            // D2예수금
            // 
            this.D2예수금.HeaderText = "D+2예수금";
            this.D2예수금.Name = "D2예수금";
            this.D2예수금.ReadOnly = true;
            // 
            // 추정자산
            // 
            this.추정자산.HeaderText = "추정자산";
            this.추정자산.Name = "추정자산";
            this.추정자산.ReadOnly = true;
            // 
            // 당일손익률
            // 
            this.당일손익률.HeaderText = "당일손익률";
            this.당일손익률.Name = "당일손익률";
            this.당일손익률.ReadOnly = true;
            // 
            // 당일손익금
            // 
            this.당일손익금.HeaderText = "당일손익금";
            this.당일손익금.Name = "당일손익금";
            this.당일손익금.ReadOnly = true;
            // 
            // RecodeAccountInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.accountListComboBox);
            this.Controls.Add(this.accountDataGridView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RecodeAccountInfo";
            this.Text = "계좌 기록";
            ((System.ComponentModel.ISupportInitialize)(this.accountDataGridView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView accountDataGridView;
        private System.Windows.Forms.ComboBox accountListComboBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn 날짜;
        private System.Windows.Forms.DataGridViewTextBoxColumn 예수금;
        private System.Windows.Forms.DataGridViewTextBoxColumn D2예수금;
        private System.Windows.Forms.DataGridViewTextBoxColumn 추정자산;
        private System.Windows.Forms.DataGridViewTextBoxColumn 당일손익률;
        private System.Windows.Forms.DataGridViewTextBoxColumn 당일손익금;
        private System.Windows.Forms.Label label10;
    }
}