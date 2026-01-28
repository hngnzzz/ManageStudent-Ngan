namespace WinClient
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1284, 711);
            this.Name = "MainForm";
            //this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
        }
    }
}
