
namespace ClientApp1
{
    partial class ErrorForm
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
            this.errorMessage = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // errorMessage
            // 
            this.errorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.errorMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.errorMessage.ForeColor = System.Drawing.Color.Red;
            this.errorMessage.Location = new System.Drawing.Point(0, 0);
            this.errorMessage.Name = "errorMessage";
            this.errorMessage.Size = new System.Drawing.Size(509, 108);
            this.errorMessage.TabIndex = 0;
            this.errorMessage.Text = "text";
            this.errorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ErrorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(509, 108);
            this.Controls.Add(this.errorMessage);
            this.Name = "ErrorForm";
            this.Text = "ErrorForm";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label errorMessage;
    }
}