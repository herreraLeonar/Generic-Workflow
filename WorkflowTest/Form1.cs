using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WorkflowNet;

namespace WorkflowTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            var wws = new WorkflowControl().GetWorkflows().Select(x => new {
                nombre = x.nombre,
                id = x.id
            }).ToList();
            this.comboBox1.DataSource = wws;
            this.comboBox1.ValueMember = "id";
            this.comboBox1.DisplayMember = "nombre";
            
            this.labelSiguienteEstadoTitulo.Visible = false;
        }

        private void btnCrear_Click(object sender, EventArgs e)
        {
            var wf = new WorkflowControl();
            var idpw = wf.IniciarWorkflow(int.Parse(Convert.ToString(this.comboBox1.SelectedValue)));
            if (idpw>0)
            {
                MessageBox.Show("El Proceso workflow con id: " + idpw + " fue creado exitosamente");
            }
            else
            {
                MessageBox.Show("Existen errores al crear el proceso de workflow");
            }
        }
        private void btnActualizar_Click(object sender, EventArgs e)
        {
            var listapw = new WorkflowControl().GetProcesosWorkflow().Select(x => new {
                Id = x.id,
                Workflow = x.workflow.nombre,
                Estado_actual = x.id_estado_actual,
                nombre = x.estados.nombre
            }).ToList();
            this.listaProcesosWorkflow.DataSource = listapw;
            this.listaProcesosWorkflow_CellClick("", null);
        }

        private void btnSigEstado_Click(object sender, EventArgs e)
        {
            var index = this.listaProcesosWorkflow.SelectedCells[0].RowIndex;
            var parametro = "";
            int idProcesoWorkflow = int.Parse(this.listaProcesosWorkflow.Rows[index].Cells[0].Value.ToString());
            if (!(ComboParametro.SelectedItem==null))
                parametro = this.ComboParametro.Text;

            string siguienteEstado = new WorkflowControl().SiguienteEstado(idProcesoWorkflow, parametro);

            this.btnActualizar_Click("",null);
        }

        private void listaProcesosWorkflow_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            this.ComboParametro.Items.Clear();
            this.ComboParametro.Text = "";
            this.labelSiguienteEstado.Text = "";

            var index = this.listaProcesosWorkflow.SelectedCells[0].RowIndex;
            var parametro = "";
            int idProcesoWorkflow = int.Parse(this.listaProcesosWorkflow.Rows[index].Cells[0].Value.ToString());
            if (!(ComboParametro.SelectedItem == null))
                parametro = this.ComboParametro.SelectedValue.ToString();

            this.labelNombreParametro.Text = new WorkflowControl().getDescripcionParametrosSiguienteEstado(idProcesoWorkflow);

            var parametros = new WorkflowControl().getParametrosToSiguienteEstado(idProcesoWorkflow);
            if ( parametros != null )
            {
                foreach (var par in parametros)
                {
                    this.ComboParametro.Items.Add(par.valor_parametro);
                }
            }

            this.labelSiguienteEstado.Text = new WorkflowControl().getNombreEstadoToParametro(idProcesoWorkflow, parametro);
        }
        
        private void ComboParametro_Change(object sender, EventArgs e)
        {
            if (this.ComboParametro.SelectedItem != null)
            {
                var index = this.listaProcesosWorkflow.SelectedCells[0].RowIndex;
                string parametro = this.ComboParametro.SelectedItem.ToString() ;
                int idProcesoWorkflow = int.Parse(this.listaProcesosWorkflow.Rows[index].Cells[0].Value.ToString());
                this.labelSiguienteEstado.Text = new WorkflowControl().getNombreEstadoToParametro(idProcesoWorkflow, parametro);
                this.labelSiguienteEstado.Visible = true;
            }
        }
    }
}