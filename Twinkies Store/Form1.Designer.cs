namespace Twinkies_Store
{
    partial class frmMainMenu
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.settingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.websitesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.reviewsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnManageCustomers = new System.Windows.Forms.Button();
            this.btnManageTransactions = new System.Windows.Forms.Button();
            this.btnManageOrders = new System.Windows.Forms.Button();
            this.btnManageShippings = new System.Windows.Forms.Button();
            this.btnManageShipingComp = new System.Windows.Forms.Button();
            this.btnManageProducts = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.AutoSize = false;
            this.menuStrip1.BackColor = System.Drawing.Color.White;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.settingToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.websitesToolStripMenuItem,
            this.reviewsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1063, 129);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuStrip1_ItemClicked);
            // 
            // settingToolStripMenuItem
            // 
            this.settingToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.settingToolStripMenuItem.Image = global::Twinkies_Store.Properties.Resources.phone_book;
            this.settingToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.settingToolStripMenuItem.Name = "settingToolStripMenuItem";
            this.settingToolStripMenuItem.Size = new System.Drawing.Size(146, 125);
            this.settingToolStripMenuItem.Text = "Phones";
            this.settingToolStripMenuItem.Click += new System.EventHandler(this.settingToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.saveToolStripMenuItem.Image = global::Twinkies_Store.Properties.Resources.user_female1;
            this.saveToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(260, 125);
            this.saveToolStripMenuItem.Text = "Shipping Carriers";
            // 
            // websitesToolStripMenuItem
            // 
            this.websitesToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.websitesToolStripMenuItem.Image = global::Twinkies_Store.Properties.Resources.world;
            this.websitesToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.websitesToolStripMenuItem.Name = "websitesToolStripMenuItem";
            this.websitesToolStripMenuItem.Size = new System.Drawing.Size(178, 125);
            this.websitesToolStripMenuItem.Text = "Websites";
            // 
            // reviewsToolStripMenuItem
            // 
            this.reviewsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI Semibold", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reviewsToolStripMenuItem.Image = global::Twinkies_Store.Properties.Resources.flash_red_eye;
            this.reviewsToolStripMenuItem.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.reviewsToolStripMenuItem.Name = "reviewsToolStripMenuItem";
            this.reviewsToolStripMenuItem.Size = new System.Drawing.Size(169, 125);
            this.reviewsToolStripMenuItem.Text = "Reviews";
            // 
            // btnManageCustomers
            // 
            this.btnManageCustomers.BackColor = System.Drawing.Color.Lavender;
            this.btnManageCustomers.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnManageCustomers.Font = new System.Drawing.Font("Yu Gothic UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageCustomers.Image = global::Twinkies_Store.Properties.Resources.user;
            this.btnManageCustomers.Location = new System.Drawing.Point(12, 145);
            this.btnManageCustomers.Name = "btnManageCustomers";
            this.btnManageCustomers.Size = new System.Drawing.Size(184, 112);
            this.btnManageCustomers.TabIndex = 1;
            this.btnManageCustomers.Text = "Manage Customers";
            this.btnManageCustomers.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnManageCustomers.UseVisualStyleBackColor = false;
            this.btnManageCustomers.Click += new System.EventHandler(this.btnManageCustomers_Click);
            // 
            // btnManageTransactions
            // 
            this.btnManageTransactions.BackColor = System.Drawing.Color.Lavender;
            this.btnManageTransactions.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnManageTransactions.Font = new System.Drawing.Font("Yu Gothic UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageTransactions.Image = global::Twinkies_Store.Properties.Resources.wire_transfer;
            this.btnManageTransactions.Location = new System.Drawing.Point(12, 673);
            this.btnManageTransactions.Name = "btnManageTransactions";
            this.btnManageTransactions.Size = new System.Drawing.Size(184, 112);
            this.btnManageTransactions.TabIndex = 2;
            this.btnManageTransactions.Text = "Manage Transactions";
            this.btnManageTransactions.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnManageTransactions.UseVisualStyleBackColor = false;
            // 
            // btnManageOrders
            // 
            this.btnManageOrders.BackColor = System.Drawing.Color.Lavender;
            this.btnManageOrders.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnManageOrders.Font = new System.Drawing.Font("Yu Gothic UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageOrders.Image = global::Twinkies_Store.Properties.Resources.purchase_order;
            this.btnManageOrders.Location = new System.Drawing.Point(12, 277);
            this.btnManageOrders.Name = "btnManageOrders";
            this.btnManageOrders.Size = new System.Drawing.Size(184, 112);
            this.btnManageOrders.TabIndex = 3;
            this.btnManageOrders.Text = "Manage Orders";
            this.btnManageOrders.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnManageOrders.UseVisualStyleBackColor = false;
            // 
            // btnManageShippings
            // 
            this.btnManageShippings.BackColor = System.Drawing.Color.Lavender;
            this.btnManageShippings.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnManageShippings.Font = new System.Drawing.Font("Yu Gothic UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageShippings.Image = global::Twinkies_Store.Properties.Resources.truck;
            this.btnManageShippings.Location = new System.Drawing.Point(12, 409);
            this.btnManageShippings.Name = "btnManageShippings";
            this.btnManageShippings.Size = new System.Drawing.Size(184, 112);
            this.btnManageShippings.TabIndex = 4;
            this.btnManageShippings.Text = "Manage Shippings";
            this.btnManageShippings.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnManageShippings.UseVisualStyleBackColor = false;
            // 
            // btnManageShipingComp
            // 
            this.btnManageShipingComp.BackColor = System.Drawing.Color.Lavender;
            this.btnManageShipingComp.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnManageShipingComp.Font = new System.Drawing.Font("Yu Gothic UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageShipingComp.Image = global::Twinkies_Store.Properties.Resources.company;
            this.btnManageShipingComp.Location = new System.Drawing.Point(227, 145);
            this.btnManageShipingComp.Name = "btnManageShipingComp";
            this.btnManageShipingComp.Size = new System.Drawing.Size(196, 112);
            this.btnManageShipingComp.TabIndex = 3;
            this.btnManageShipingComp.Text = "Manage Shipping Comp.";
            this.btnManageShipingComp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnManageShipingComp.UseVisualStyleBackColor = false;
            // 
            // btnManageProducts
            // 
            this.btnManageProducts.BackColor = System.Drawing.Color.Lavender;
            this.btnManageProducts.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnManageProducts.Font = new System.Drawing.Font("Yu Gothic UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnManageProducts.Image = global::Twinkies_Store.Properties.Resources.product;
            this.btnManageProducts.Location = new System.Drawing.Point(12, 541);
            this.btnManageProducts.Name = "btnManageProducts";
            this.btnManageProducts.Size = new System.Drawing.Size(184, 112);
            this.btnManageProducts.TabIndex = 5;
            this.btnManageProducts.Text = "Manage Products";
            this.btnManageProducts.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnManageProducts.UseVisualStyleBackColor = false;
            // 
            // frmMainMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.BackgroundImage = global::Twinkies_Store.Properties.Resources.backGround;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1063, 778);
            this.Controls.Add(this.btnManageProducts);
            this.Controls.Add(this.btnManageShippings);
            this.Controls.Add(this.btnManageShipingComp);
            this.Controls.Add(this.btnManageOrders);
            this.Controls.Add(this.btnManageTransactions);
            this.Controls.Add(this.btnManageCustomers);
            this.Controls.Add(this.menuStrip1);
            this.DoubleBuffered = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmMainMenu";
            this.Text = "Twinkies Sore";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem settingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.Button btnManageCustomers;
        private System.Windows.Forms.Button btnManageTransactions;
        private System.Windows.Forms.Button btnManageOrders;
        private System.Windows.Forms.Button btnManageShippings;
        private System.Windows.Forms.Button btnManageShipingComp;
        private System.Windows.Forms.Button btnManageProducts;
        private System.Windows.Forms.ToolStripMenuItem reviewsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem websitesToolStripMenuItem;
    }
}

