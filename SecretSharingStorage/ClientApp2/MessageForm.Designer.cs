
namespace ClientApp2
{
    partial class MessageForm
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
            this.messageText = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // messageText
            // 
            this.messageText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.messageText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.messageText.Location = new System.Drawing.Point(0, 0);
            this.messageText.Name = "messageText";
            this.messageText.Size = new System.Drawing.Size(513, 204);
            this.messageText.TabIndex = 1;
            this.messageText.Text = "label1";
            this.messageText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 204);
            this.Controls.Add(this.messageText);
            this.Name = "MessageForm";
            this.Text = "Сообщение (Клиент 2)";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label messageText;
    }
}