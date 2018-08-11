namespace WorkflowTest
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnCrear = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.listaProcesosWorkflow = new System.Windows.Forms.DataGridView();
            this.labelNombreParametro = new System.Windows.Forms.Label();
            this.btnSigEstado = new System.Windows.Forms.Button();
            this.ComboParametro = new System.Windows.Forms.ComboBox();
            this.labelSiguienteEstadoTitulo = new System.Windows.Forms.Label();
            this.labelSiguienteEstado = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.listaProcesosWorkflow)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 21);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(198, 24);
            this.comboBox1.TabIndex = 0;
            // 
            // btnCrear
            // 
            this.btnCrear.Location = new System.Drawing.Point(216, 21);
            this.btnCrear.Name = "btnCrear";
            this.btnCrear.Size = new System.Drawing.Size(75, 23);
            this.btnCrear.TabIndex = 1;
            this.btnCrear.Text = "Crear";
            this.btnCrear.UseVisualStyleBackColor = true;
            this.btnCrear.Click += new System.EventHandler(this.btnCrear_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(580, 54);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(104, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Actualizar";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnActualizar_Click);
            // 
            // listaProcesosWorkflow
            // 
            this.listaProcesosWorkflow.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.listaProcesosWorkflow.Location = new System.Drawing.Point(9, 83);
            this.listaProcesosWorkflow.Name = "listaProcesosWorkflow";
            this.listaProcesosWorkflow.RowTemplate.Height = 24;
            this.listaProcesosWorkflow.Size = new System.Drawing.Size(675, 245);
            this.listaProcesosWorkflow.TabIndex = 3;
            this.listaProcesosWorkflow.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.listaProcesosWorkflow_CellClick);
            // 
            // labelNombreParametro
            // 
            this.labelNombreParametro.AutoSize = true;
            this.labelNombreParametro.Location = new System.Drawing.Point(360, 351);
            this.labelNombreParametro.Name = "labelNombreParametro";
            this.labelNombreParametro.Size = new System.Drawing.Size(0, 17);
            this.labelNombreParametro.TabIndex = 5;
            // 
            // btnSigEstado
            // 
            this.btnSigEstado.Location = new System.Drawing.Point(534, 367);
            this.btnSigEstado.Name = "btnSigEstado";
            this.btnSigEstado.Size = new System.Drawing.Size(150, 34);
            this.btnSigEstado.TabIndex = 6;
            this.btnSigEstado.Text = "Siguiente Estado";
            this.btnSigEstado.UseVisualStyleBackColor = true;
            this.btnSigEstado.Click += new System.EventHandler(this.btnSigEstado_Click);
            // 
            // ComboParametro
            // 
            this.ComboParametro.FormattingEnabled = true;
            this.ComboParametro.Location = new System.Drawing.Point(363, 377);
            this.ComboParametro.Name = "ComboParametro";
            this.ComboParametro.Size = new System.Drawing.Size(138, 24);
            this.ComboParametro.TabIndex = 7;
            this.ComboParametro.SelectedIndexChanged += new System.EventHandler(this.ComboParametro_Change);
            // 
            // labelSiguienteEstadoTitulo
            // 
            this.labelSiguienteEstadoTitulo.AutoSize = true;
            this.labelSiguienteEstadoTitulo.Location = new System.Drawing.Point(12, 351);
            this.labelSiguienteEstadoTitulo.Name = "labelSiguienteEstadoTitulo";
            this.labelSiguienteEstadoTitulo.Size = new System.Drawing.Size(316, 17);
            this.labelSiguienteEstadoTitulo.TabIndex = 8;
            this.labelSiguienteEstadoTitulo.Text = "Siguiente Estado(No requiere ningun parametro)";
            // 
            // labelSiguienteEstado
            // 
            this.labelSiguienteEstado.AutoSize = true;
            this.labelSiguienteEstado.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSiguienteEstado.Location = new System.Drawing.Point(22, 377);
            this.labelSiguienteEstado.Name = "labelSiguienteEstado";
            this.labelSiguienteEstado.Size = new System.Drawing.Size(0, 17);
            this.labelSiguienteEstado.TabIndex = 9;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(695, 420);
            this.Controls.Add(this.labelSiguienteEstado);
            this.Controls.Add(this.labelSiguienteEstadoTitulo);
            this.Controls.Add(this.ComboParametro);
            this.Controls.Add(this.btnSigEstado);
            this.Controls.Add(this.labelNombreParametro);
            this.Controls.Add(this.listaProcesosWorkflow);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnCrear);
            this.Controls.Add(this.comboBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.listaProcesosWorkflow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnCrear;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.DataGridView listaProcesosWorkflow;
        private System.Windows.Forms.Label labelNombreParametro;
        private System.Windows.Forms.Button btnSigEstado;
        private System.Windows.Forms.ComboBox ComboParametro;
        private System.Windows.Forms.Label labelSiguienteEstadoTitulo;
        private System.Windows.Forms.Label labelSiguienteEstado;
    }
}

