using Microsoft.Web.WebView2.WinForms;

namespace DSRemapper
{
    partial class DSMain
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
            webView = new WebView2();
            SuspendLayout();
            //
            // webView
            //
            webView.Dock=DockStyle.Fill;
            // 
            // DSMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(webView);
            Name = "DSMain";
            Text = "DSMain";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private WebView2 webView;
    }
}