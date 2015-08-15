namespace LiveSplit.Quake
{
    partial class Settings
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.lstUsedEvents = new System.Windows.Forms.ListBox();
            this.lstAvailEvents = new System.Windows.Forms.ListBox();
            this.lblUsedEvents = new System.Windows.Forms.Label();
            this.btnAllEvents = new System.Windows.Forms.Button();
            this.btnNoEvents = new System.Windows.Forms.Button();
            this.btnAddEvent = new System.Windows.Forms.Button();
            this.btnRemoveEvent = new System.Windows.Forms.Button();
            this.lblAvailEvents = new System.Windows.Forms.Label();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.chkUpdateGameTime = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // lstUsedEvents
            // 
            this.lstUsedEvents.FormattingEnabled = true;
            this.lstUsedEvents.Location = new System.Drawing.Point(13, 23);
            this.lstUsedEvents.Name = "lstUsedEvents";
            this.lstUsedEvents.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstUsedEvents.Size = new System.Drawing.Size(196, 186);
            this.lstUsedEvents.TabIndex = 3;
            // 
            // lstAvailEvents
            // 
            this.lstAvailEvents.FormattingEnabled = true;
            this.lstAvailEvents.Location = new System.Drawing.Point(254, 23);
            this.lstAvailEvents.Name = "lstAvailEvents";
            this.lstAvailEvents.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstAvailEvents.Size = new System.Drawing.Size(195, 186);
            this.lstAvailEvents.TabIndex = 4;
            // 
            // lblUsedEvents
            // 
            this.lblUsedEvents.AutoSize = true;
            this.lblUsedEvents.Location = new System.Drawing.Point(10, 7);
            this.lblUsedEvents.Name = "lblUsedEvents";
            this.lblUsedEvents.Size = new System.Drawing.Size(172, 13);
            this.lblUsedEvents.TabIndex = 5;
            this.lblUsedEvents.Text = "Split on these events (in this order):";
            // 
            // btnAllEvents
            // 
            this.btnAllEvents.Location = new System.Drawing.Point(215, 157);
            this.btnAllEvents.Name = "btnAllEvents";
            this.btnAllEvents.Size = new System.Drawing.Size(33, 23);
            this.btnAllEvents.TabIndex = 9;
            this.btnAllEvents.Text = "<<";
            this.btnAllEvents.UseVisualStyleBackColor = true;
            this.btnAllEvents.Click += new System.EventHandler(this.btnAllEvents_Click);
            // 
            // btnNoEvents
            // 
            this.btnNoEvents.Location = new System.Drawing.Point(215, 186);
            this.btnNoEvents.Name = "btnNoEvents";
            this.btnNoEvents.Size = new System.Drawing.Size(33, 23);
            this.btnNoEvents.TabIndex = 10;
            this.btnNoEvents.Text = ">>";
            this.btnNoEvents.UseVisualStyleBackColor = true;
            this.btnNoEvents.Click += new System.EventHandler(this.btnNoEvents_Click);
            // 
            // btnAddEvent
            // 
            this.btnAddEvent.Location = new System.Drawing.Point(215, 23);
            this.btnAddEvent.Name = "btnAddEvent";
            this.btnAddEvent.Size = new System.Drawing.Size(33, 23);
            this.btnAddEvent.TabIndex = 11;
            this.btnAddEvent.Text = "<";
            this.btnAddEvent.UseVisualStyleBackColor = true;
            this.btnAddEvent.Click += new System.EventHandler(this.btnAddEvent_Click);
            // 
            // btnRemoveEvent
            // 
            this.btnRemoveEvent.Location = new System.Drawing.Point(215, 52);
            this.btnRemoveEvent.Name = "btnRemoveEvent";
            this.btnRemoveEvent.Size = new System.Drawing.Size(33, 23);
            this.btnRemoveEvent.TabIndex = 12;
            this.btnRemoveEvent.Text = ">";
            this.btnRemoveEvent.UseVisualStyleBackColor = true;
            this.btnRemoveEvent.Click += new System.EventHandler(this.btnRemoveEvent_Click);
            // 
            // lblAvailEvents
            // 
            this.lblAvailEvents.AutoSize = true;
            this.lblAvailEvents.Location = new System.Drawing.Point(251, 7);
            this.lblAvailEvents.Name = "lblAvailEvents";
            this.lblAvailEvents.Size = new System.Drawing.Size(88, 13);
            this.lblAvailEvents.TabIndex = 13;
            this.lblAvailEvents.Text = "Available events:";
            // 
            // btnUp
            // 
            this.btnUp.Location = new System.Drawing.Point(126, 215);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(33, 23);
            this.btnUp.TabIndex = 15;
            this.btnUp.Text = "Up";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Location = new System.Drawing.Point(165, 215);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(44, 23);
            this.btnDown.TabIndex = 16;
            this.btnDown.Text = "Down";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // chkUpdateGameTime
            // 
            this.chkUpdateGameTime.AutoSize = true;
            this.chkUpdateGameTime.Location = new System.Drawing.Point(13, 250);
            this.chkUpdateGameTime.Name = "chkUpdateGameTime";
            this.chkUpdateGameTime.Size = new System.Drawing.Size(218, 17);
            this.chkUpdateGameTime.TabIndex = 17;
            this.chkUpdateGameTime.Text = "Update game time between intermissions";
            this.chkUpdateGameTime.UseVisualStyleBackColor = true;
            this.chkUpdateGameTime.CheckedChanged += new System.EventHandler(this.chkUpdateGameTime_CheckedChanged);
            // 
            // Settings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkUpdateGameTime);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.lblAvailEvents);
            this.Controls.Add(this.btnRemoveEvent);
            this.Controls.Add(this.btnAddEvent);
            this.Controls.Add(this.btnNoEvents);
            this.Controls.Add(this.btnAllEvents);
            this.Controls.Add(this.lblUsedEvents);
            this.Controls.Add(this.lstAvailEvents);
            this.Controls.Add(this.lstUsedEvents);
            this.Name = "Settings";
            this.Padding = new System.Windows.Forms.Padding(7);
            this.Size = new System.Drawing.Size(459, 275);
            this.HandleDestroyed += new System.EventHandler(this.settings_HandleDestroyed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lstUsedEvents;
        private System.Windows.Forms.ListBox lstAvailEvents;
        private System.Windows.Forms.Label lblUsedEvents;
        private System.Windows.Forms.Button btnAllEvents;
        private System.Windows.Forms.Button btnNoEvents;
        private System.Windows.Forms.Button btnAddEvent;
        private System.Windows.Forms.Button btnRemoveEvent;
        private System.Windows.Forms.Label lblAvailEvents;
        private System.Windows.Forms.Button btnUp;
        private System.Windows.Forms.Button btnDown;
        private System.Windows.Forms.CheckBox chkUpdateGameTime;
    }
}
