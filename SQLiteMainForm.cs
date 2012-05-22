using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SQLiteTheManager
{
    public partial class SQLiteMainForm : Form
    {
        private String database;
        private SQLiteConnection connection;
        private TreeNode rootNode;

        public SQLiteMainForm()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog().Equals(DialogResult.OK))
            {
                if (connection != null)
                {
                    connection.Close();
                }

                database = openFileDialog.FileName;
                this.Text += " - " + database;
                try
                {
                    connection = new SQLiteConnection(String.Format("Data Source={0};Version=3;", database));
                    connection.Open();
                    executeMenuItem.Enabled = true;
                    refreshTablesToolStripMenuItem.Enabled = true;
                    refreshTables();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void refreshTables()
        {
            treeView1.Nodes.Clear();
            //
            rootNode = new TreeNode(connection.DataSource, 1,1);
            treeView1.Nodes.Add(rootNode);
            //
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type='table'";
            try
            {
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    TreeNode node = new TreeNode(reader.GetString(0), 0 , 0);
                    node.Nodes.Add("");
                    node.Name = "table";
                    rootNode.Nodes.Add(node);
                }
                rootNode.Expand();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void SQLiteMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (connection != null)
            {
                try
                {
                    connection.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Name.Equals("table"))
            {
                sqlTextBox.Text = "SELECT * FROM " + e.Node.Text;
                executeQuery();
            }
            e.Node.ExpandAll();
        }

        private void executeQuery()
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = sqlTextBox.Text;
            if (command.CommandText.StartsWith("SELECT"))
            {
                try
                {
                    SQLiteDataReader reader = command.ExecuteReader();
                    dataGridView1.Columns.Clear();
                    dataGridView1.Rows.Clear();
                    //
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        dataGridView1.Columns.Add(reader.GetName(i), reader.GetName(i));
                        
                    }
                    //
                    int count = 0;
                    while (reader.Read())
                    {
                        count++;
                        Object[] values = new Object[reader.FieldCount];
                        reader.GetValues(values);
                        dataGridView1.Rows.Add(values);
                    }
                    toolStripStatusLabel.Text = count + " rows returned";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                try
                {
                    int affected = command.ExecuteNonQuery();
                    toolStripStatusLabel.Text = affected + " rows affected";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void executeMenuItem_Click(object sender, EventArgs e)
        {
            executeQuery();
        }

        private void refreshTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (connection != null)
            {
                refreshTables();
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].FormattedValue.Equals("System.Byte[]"))
            {
                if (saveFileDialog.ShowDialog().Equals(DialogResult.OK))
                {
                    File.WriteAllBytes(saveFileDialog.FileName, (byte[])dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
                }
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (connection != null)
            {
                refreshTables();
            }
            sqlTextBox.Text = "SQL";
            executeMenuItem.Enabled = false;
            refreshTablesToolStripMenuItem.Enabled = false;
            treeView1.Nodes.Clear();
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
        }

        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Name.Equals("table"))
            {
                try
                {
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = String.Format("SELECT * FROM {0}", e.Node.Text);
                    SQLiteDataReader reader = command.ExecuteReader();
                    //
                    e.Node.Nodes.Clear();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        e.Node.Nodes.Add("column", reader.GetName(i), 4, 4);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
