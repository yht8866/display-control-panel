namespace DCP
{
    partial class CAreaUserControl
    {
        /// <summary> 
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;


        #region コンポーネント デザイナーで生成されたコード

        /// <summary> 
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を 
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.TimerBlink = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // TimerBlink
            // 
            this.TimerBlink.Interval = 500;
            this.TimerBlink.Tick += new System.EventHandler(this.TimerBlink_Tick);
            // 
            // CAreaUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 19F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Font = new System.Drawing.Font("ＭＳ ゴシック", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "CAreaUserControl";
            this.Size = new System.Drawing.Size(550, 95);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer TimerBlink;

    }
}
