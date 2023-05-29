
namespace ClientApp2
{
    partial class Form3
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
            this.buttonGetSecret = new System.Windows.Forms.Button();
            this.secretIdtextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonGetSecret
            // 
            this.buttonGetSecret.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.buttonGetSecret.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.buttonGetSecret.Location = new System.Drawing.Point(141, 180);
            this.buttonGetSecret.Name = "buttonGetSecret";
            this.buttonGetSecret.Size = new System.Drawing.Size(231, 48);
            this.buttonGetSecret.TabIndex = 10;
            this.buttonGetSecret.Text = "Получить секрет";
            this.buttonGetSecret.UseVisualStyleBackColor = true;
            this.buttonGetSecret.Click += new System.EventHandler(this.buttonGetSecret_Click);
            // 
            // secretIdtextBox
            // 
            this.secretIdtextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.secretIdtextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.secretIdtextBox.Location = new System.Drawing.Point(32, 116);
            this.secretIdtextBox.Multiline = true;
            this.secretIdtextBox.Name = "secretIdtextBox";
            this.secretIdtextBox.Size = new System.Drawing.Size(449, 35);
            this.secretIdtextBox.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label2.Location = new System.Drawing.Point(37, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(201, 25);
            this.label2.TabIndex = 8;
            this.label2.Text = "Введите ID секрета:";
            // 
            // Form3
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(513, 302);
            this.Controls.Add(this.buttonGetSecret);
            this.Controls.Add(this.secretIdtextBox);
            this.Controls.Add(this.label2);
            this.Name = "Form3";
            this.Text = "Форма получения секрета (Клиент 2)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonGetSecret;
        private System.Windows.Forms.TextBox secretIdtextBox;
        private System.Windows.Forms.Label label2;
    }
}