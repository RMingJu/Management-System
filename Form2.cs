using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 進銷存
{
    public partial class Form2 : Form
    {
        String proc = null;
        String TableName_Show = null;   //主視窗中顯示的Table
        String InsertTableName = null;  //要異動的Table
        DataTable Source;
        bool ReturnTable = false;

        public Form2()
        {
            InitializeComponent();
            label_QuantityShow.Text = "";
            
        }

        public void GetBindingSource(String TabelName,DataTable Source)
        {
            this.TableName_Show = TabelName;
            this.Source = Source;

        }






        private void SelectTable(object sender, EventArgs e)
        {
            RadioButton btn = sender as RadioButton;
            proc = btn.Text + "INSERT";
            InsertTableName = btn.Text;
            textBox_BarCode.Focus();

            switch (btn.Text)
            {
                case "產品資料":
                    if (btn.Checked)
                    {
                        textBox_Number.Enabled = true;
                        textBox_Item.Enabled = true;
                        textBox_Name.Enabled = true;
                    }
                    else
                    {
                        textBox_Number.Enabled = false;
                        textBox_Item.Enabled = false;
                        textBox_Name.Enabled = false;
                    }
                    break;
                case "庫存表":
                    if (btn.Checked)
                    {
                        textBox_Number.Enabled = true;
                        textBox_Quantity.Enabled = true;
                    }
                    else
                    {
                        textBox_Number.Enabled = false;
                        textBox_Quantity.Enabled = false;
                    }
                    break;
                case "進貨表":
                case "出貨表":
                    if (btn.Checked)
                    {
                        textBox_Number.Enabled = true;
                        textBox_Price.Enabled = true;
                        textBox_Quantity.Enabled = true;
                    }
                    else
                    {
                        textBox_Number.Enabled = false;
                        textBox_Price.Enabled = false;
                        textBox_Quantity.Enabled = false;
                    }
                    break;
            }

            button1.Enabled = true;


            /*
            檢查Form1的DataGridView是不是跟現在要Insert的資料是同一張表
            ，如果是那麼我們要自己一併Insert到BindingSource不要用刷新的很操資料庫
            */
            if (btn.Text == TableName_Show)
            {
                ReturnTable = true;
            }
            else
            {
                ReturnTable = false;
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            GetConnection gc = new GetConnection();
            using (SqlConnection cnn = gc.ConnectionString("新新"))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = proc;

                    try
                    {
                        switch (InsertTableName)
                        {
                            case "產品資料":
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = int.Parse(textBox_Number.Text);
                                cmd.Parameters.Add("@item", SqlDbType.NVarChar).Value = textBox_Item.Text;
                                cmd.Parameters.Add("@name", SqlDbType.NVarChar).Value = textBox_Name.Text;
                                if (String.IsNullOrEmpty(textBox_BarCode.Text))
                                {
                                    cmd.Parameters.Add("@barcode", SqlDbType.NVarChar).Value = DBNull.Value;
                                }
                                else
                                {
                                    cmd.Parameters.Add("@barcode", SqlDbType.NVarChar).Value = textBox_BarCode.Text;
                                }
                                break;
                            case "庫存表":
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = int.Parse(textBox_Number.Text);
                                cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = int.Parse(textBox_Quantity.Text);
                                break;
                            case "進貨表":
                            case "出貨表":
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = int.Parse(textBox_Number.Text);
                                cmd.Parameters.Add("@price", SqlDbType.Int).Value = int.Parse(textBox_Price.Text);
                                cmd.Parameters.Add("@quantity", SqlDbType.Int).Value = int.Parse(textBox_Quantity.Text);
                                break;

                        }
                        cmd.Parameters.Add("@count", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                        cnn.Open();
                        int cc = cmd.ExecuteNonQuery();
                        cmd.Dispose();
                        cnn.Close();

                        //看是要不要直接回寫還是USER要換表刷新才能看到資料新增
                        if (ReturnTable)
                        {
                            DataRow rr = Source.NewRow();

                            switch (InsertTableName)
                            {
                                case "產品資料":
                                    rr["編號"] = int.Parse(textBox_Number.Text);
                                    rr["項目"] = textBox_Item.Text;
                                    rr["品名"] = textBox_Name.Text;
                                    rr["條碼"] = textBox_BarCode.Text;
                                    break;
                                case "庫存表":
                                    rr["編號"] = int.Parse(textBox_Number.Text);
                                    rr["數量"] = int.Parse(textBox_Quantity.Text);
                                    break;
                                case "進貨表":
                                    rr["編號"] = int.Parse(textBox_Number.Text);
                                    rr["數量"] = int.Parse(textBox_Quantity.Text);
                                    rr["進貨價格"] = int.Parse(textBox_Price.Text);
                                    break;
                                case "出貨表":
                                    rr["編號"] = int.Parse(textBox_Number.Text);
                                    rr["數量"] = int.Parse(textBox_Quantity.Text);
                                    rr["出貨價格"] = int.Parse(textBox_Price.Text);
                                    break;
                            }

                            Source.Rows.Add(rr);

                        }
                        if (cc > 1)
                        {
                            MessageBox.Show("新增成功!");

                        }
                        else
                        {
                            MessageBox.Show("新增失敗!");
                        }

                        //清除所有格子的字
                        foreach (var x in this.Controls)
                        {
                            if (x is TextBox)
                            {
                                TextBox tb = x as TextBox;
                                tb.Clear();
                            }
                        }
                        label_QuantityShow.Text = "";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }

                    textBox_BarCode.Focus();


                }
                
            }
        }

        private void textBox_BarCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                foreach (Control x in this.Controls)
                {
                    if (x is TextBox)
                    {
                        TextBox tb = x as TextBox;
                        if (tb.Name == "textBox_BarCode")
                            continue;
                        tb.Text = "";
                    }
                }


                textBox_BarCode.SelectAll();

                GetConnection gc = new GetConnection();
                using (SqlConnection cnn = gc.ConnectionString("新新"))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cnn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "產品資料BarCode查詢";

                        cmd.Parameters.Add("@barcode", SqlDbType.NVarChar).Value = textBox_BarCode.Text;
                        cmd.Parameters.Add("@number", SqlDbType.Int);
                        cmd.Parameters.Add("@name", SqlDbType.NVarChar,30);
                        cmd.Parameters.Add("@item", SqlDbType.NVarChar,20);
                        cmd.Parameters.Add("@quantity", SqlDbType.Int);
                        cmd.Parameters["@number"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@name"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@item"].Direction = ParameterDirection.Output;
                        cmd.Parameters["@quantity"].Direction = ParameterDirection.Output;

                        cnn.Open();
                        SqlDataReader dr = cmd.ExecuteReader();

                        try
                        {
                            //用條碼機取得產品編號
                            int ans = int.Parse(cmd.Parameters["@number"].Value.ToString());
                            textBox_Number.Text = ans.ToString();
                            textBox_Name.Text = cmd.Parameters["@name"].Value.ToString();
                            textBox_Item.Text = cmd.Parameters["@item"].Value.ToString();

                            //如果是出貨表或進貨表的話，SHOW出庫存數量
                            if(InsertTableName == "進貨表" || InsertTableName == "出貨表")
                                label_QuantityShow.Text = "目前庫存量: " + cmd.Parameters["@quantity"].Value.ToString();
                        }
                        catch (Exception ex)
                        {
                            if(InsertTableName != "產品資料")
                                MessageBox.Show("找不到符合的產品!");
                        }



                        cmd.Dispose();
                        dr.Close();
                        cnn.Close();


                    }
                }
            }
        }

        private void textBox_tabstop(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }
    }
}
