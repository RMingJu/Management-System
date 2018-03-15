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
    public partial class Form1 : Form
    {
        private BindingSource bs;
        private List<int> position;

        public Form1()
        {

            InitializeComponent();
            bs = new BindingSource();
            bs.CurrentChanged += Bs_CurrentChanged;
            label_Number.Text = "";
            label_Count.Text = "";
            label_SerialNumber.Text = "";
            label_TableName.Text = "";

        }
        //顯示DataGridView目前位置
        private void Bs_CurrentChanged(object sender, EventArgs e)
        {
            BindingCount();
        }
        private void BindingCount()
        {
            label_Count.Text = string.Format("目前第 {0} 筆，共 {1} 筆資料", bs.Position + 1, bs.Count);
        }

        //取的連線字串
        private SqlConnection GetConnection(String DB_Name, String UserID = "sqladmin", String PassWord = "1234")
        {
            SqlConnectionStringBuilder scsb = new SqlConnectionStringBuilder();
            scsb.DataSource = "localhost";
            scsb.InitialCatalog = DB_Name;
            scsb.IntegratedSecurity = false;
            scsb.UserID = UserID;
            scsb.Password = PassWord;
            SqlConnection cnn = new SqlConnection(scsb.ConnectionString);
            return cnn;
        }

        //顯示資料
        private void button1_Click(object sender, EventArgs e)
        {
            button8.Enabled = true;
            textBox_SearchBarCode.Enabled = true;
            textBox_SearchBarCode.Focus();
            label_TableName.Text = comboBox1.Text;
            //刪除不符合的查詢
            if (label_TableName.Text == "產品資料")
            {
                textBox_SearchName.Enabled = false;
                textBox_SearchItem.Enabled = false;
            }
            else
            {
                textBox_SearchName.Enabled = true;
                textBox_SearchItem.Enabled = true;
            }

            if (label_TableName.Text == "進貨表" || label_TableName.Text == "出貨表")
            {
                textBox_SearchDate.Enabled = true;
            }
            else
            {
                textBox_SearchDate.Enabled = false;
            }

            bs = new BindingSource();
            bs.CurrentChanged += Bs_CurrentChanged;
            //comboBox1.Enabled = false;


            using (SqlConnection cnn = GetConnection("新新"))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    //選擇查詢哪張表
                    switch (comboBox1.Text)
                    {
                        case "產品資料":
                            cmd.CommandText = "產品資料查詢";
                            break;
                        case "進貨表":
                            cmd.CommandText = "進貨表查詢";
                            break;
                        case "庫存表":
                            cmd.CommandText = "庫存表查詢";
                            break;
                        case "出貨表":
                            cmd.CommandText = "出貨表查詢";
                            break;
                    }

                    cnn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();
                    DataTable tt = new DataTable();
                    tt.Load(dr);


                    cmd.Dispose();
                    dr.Close();
                    cnn.Close();

                    bs.DataSource = tt;
                    dataGridView1.DataSource = bs;
                    BindingCheck();
                }
            }
        }
        //檢查BindingSource
        private void BindingCheck()
        {
            label_SerialNumber.DataBindings.Clear();
            label_Number.DataBindings.Clear();
            textBox_Item.DataBindings.Clear();
            textBox_Name.DataBindings.Clear();
            textBox_Price.DataBindings.Clear();
            textBox_Quantity.DataBindings.Clear();
            textBox_BarCode.DataBindings.Clear();
            textBox_Price.Enabled = true;
            textBox_Quantity.Enabled = true;
            
            switch (comboBox1.Text)
            {
                case "庫存表":
                    label_Number.DataBindings.Add("Text", bs, "編號");
                    textBox_Item.DataBindings.Add("Text", bs, "項目");
                    textBox_Name.DataBindings.Add("Text", bs, "品名");
                    textBox_Price.Enabled = false;
                    textBox_Quantity.DataBindings.Add("Text", bs, "數量");
                    break;
                case "產品資料":
                    label_Number.DataBindings.Add("Text", bs, "編號");
                    textBox_Item.DataBindings.Add("Text", bs, "項目");
                    textBox_Name.DataBindings.Add("Text", bs, "品名");
                    textBox_BarCode.DataBindings.Add("Text", bs, "條碼");
                    textBox_Price.Enabled = false;
                    textBox_Quantity.Enabled = false;


                    break;
                case "進貨表":
                case "出貨表":
                    label_SerialNumber.DataBindings.Add("Text", bs, "流水號");
                    label_Number.DataBindings.Add("Text", bs, "編號");
                    textBox_Item.DataBindings.Add("Text", bs, "項目");
                    textBox_Name.DataBindings.Add("Text", bs, "品名");
                    if (comboBox1.Text == "進貨表")
                    {
                        textBox_Price.DataBindings.Add("Text", bs, "進貨價格");
                    }
                    else
                    {
                        textBox_Price.DataBindings.Add("Text", bs, "出貨價格");
                    }
                    textBox_Quantity.DataBindings.Add("Text", bs, "數量");
                    break;
            }
        }


        //新增資料
        private void button3_Click(object sender, EventArgs e)
        {
            Form2 f2 = null;

            foreach (Form f in Application.OpenForms)
            {

                if (f.Text == "新增資料")
                {
                    f2 = f as Form2;
                    f2.GetBindingSource(label_TableName.Text, bs.DataSource as DataTable);
                    f2.Activate();
                    return; //找到開過的視窗，結束整個方法
                }

            }
            f2 = new Form2();
            f2.GetBindingSource(label_TableName.Text, bs.DataSource as DataTable);
            f2.Show();
        }
        //移動按鈕
        private void BindingBtn(object sender, EventArgs e)
        {
            Button bt = sender as Button;

            switch (bt.Text)
            {
                case "第一筆":
                    bs.MoveFirst();
                    break;
                case "上一筆":
                    bs.MovePrevious();
                    break;
                case "下一筆":
                    bs.MoveNext();
                    break;
                case "最後一筆":
                    bs.MoveLast();
                    break;
            }

        }


        //條碼搜尋
        private void textBox_SearchBarCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox_SearchBarCode.SelectAll();

                //BindingSource的位置
                position = new List<int>();

                using (SqlConnection cnn = GetConnection("新新"))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cnn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "產品資料BarCode查詢";

                        cmd.Parameters.Add("@barcode", SqlDbType.NVarChar).Value = textBox_SearchBarCode.Text;
                        cmd.Parameters.Add("@number", SqlDbType.Int);
                        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 30);
                        cmd.Parameters.Add("@item", SqlDbType.NVarChar, 20);
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
                            DataTable tt = bs.DataSource as DataTable;

                            //比對編號
                            foreach (DataRow x in tt.Rows)
                            {
                                if (x["編號"].Equals(ans))
                                {
                                    //將位置存取
                                    position.Add(tt.Rows.IndexOf(x));

                                    
                                }
                            }
                            //將BindingSource的位置移動到第一個的位置
                            bs.Position = position[0];
                            button_BarCodeUp.Enabled = true;
                            button_BarCodeDown.Enabled = true;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("找不到符合的產品! \r\n" + ex.Message);
                            button_BarCodeUp.Enabled = false;
                            button_BarCodeDown.Enabled = false;
                        }



                        cmd.Dispose();
                        dr.Close();
                        cnn.Close();


                    }
                }
            }
        }

        //刪除
        private void button8_Click(object sender, EventArgs e)
        {
            
            using (SqlConnection cnn = GetConnection("新新"))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    cmd.Connection = cnn;
                    DialogResult result;

                    DataTable tt = bs.DataSource as DataTable;
                    DataRow target = null; //該筆資料列


                    switch (label_TableName.Text)
                    {
                        case "庫存表":
                            MessageBox.Show("庫存表無法做此異動!");
                            break;
                        case "產品資料":
                            result = MessageBox.Show("是否刪除此筆資料?\r\n" + "(此動作會異動庫存表、進貨表、出貨表)", "警告!", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                            if (result == DialogResult.Yes)
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = "產品資料DELETE";
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = int.Parse(label_Number.Text);
                            }
                            else
                            {
                                return;
                            }
                            //找到該筆資料列
                            foreach (DataRow x in tt.Rows)
                            {
                                if (x["編號"].Equals(int.Parse(label_Number.Text)))
                                    target = x;
                            }
                            break;
                        case "出貨表":
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@serial_number", SqlDbType.VarChar).Value = label_SerialNumber.Text;
                            result = MessageBox.Show("刪除此筆資料並異動庫存表\r\n" + "是:庫存表的庫存會異動\r\n" + "否:僅刪除這張表的資料不異動庫存", "警告!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                                cmd.CommandText = "出貨表DELETE";
                            else if (result == DialogResult.No)
                                cmd.CommandText = "ONLY_出貨表DELETE";
                            else
                                return;

                            //找到該筆資料列
                            foreach (DataRow x in tt.Rows)
                            {
                                if (x["流水號"].Equals(label_SerialNumber.Text))
                                    target = x;
                            }

                            break;
                        case "進貨表":
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add("@serial_number", SqlDbType.VarChar).Value = label_SerialNumber.Text;
                            result = MessageBox.Show("刪除此筆資料並異動庫存表\r\n"+"是:庫存表的庫存會異動\r\n"+"否:僅刪除這張表的資料不異動庫存", "警告!", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                                cmd.CommandText = "進貨表DELETE";
                            else if (result == DialogResult.No)
                                cmd.CommandText = "ONLY_進貨表DELETE";
                            else
                                return;

                            //找到該筆資料列
                            foreach (DataRow x in tt.Rows)
                            {
                                if (x["流水號"].Equals(label_SerialNumber.Text))
                                    target = x;
                            }

                            break;

                    }

                    
                    

                    //更動資料庫
                    cnn.Open();
                    int c = cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    cnn.Close();

                    if (c > 1)
                    {
                        MessageBox.Show("刪除成功!");
                        //從dataGridView上刪除
                        
                        tt.Rows.Remove(target);
                        bs.DataSource = tt;
                        dataGridView1.DataSource = bs;


                    }
                    else
                    {
                        MessageBox.Show("刪除失敗!");
                    }
                }
            }
        }
        //更新
        private void button2_Click(object sender, EventArgs e)
        {

        }

        //品名搜尋
        private void textBox_SearchName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox_SearchName.SelectAll();
                label_TableName.Text = comboBox1.Text;
                /*
                DataTable tt = dataGridView1.DataSource as DataTable;
                try
                {
                    var result = from x in tt.AsEnumerable()
                                     //where x.Field<String>("品名").Contains(textBox_SearchName.Text)
                                 select x;

                    bs.DataSource = result.ToList();
                    dataGridView1.DataSource = bs;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                */
                
                using (SqlConnection cnn = GetConnection("新新"))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cnn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = comboBox1.Text + "品名查詢";
                        cmd.Parameters.Add("@name", SqlDbType.NVarChar, 30);
                        cmd.Parameters["@name"].Direction = ParameterDirection.Input;
                        cmd.Parameters["@name"].Value = textBox_SearchName.Text;

                        cnn.Open();
                        SqlDataReader sr = cmd.ExecuteReader();
                        DataTable tt = new DataTable();
                        tt.Load(sr);
                        bs.DataSource = tt;
                        dataGridView1.DataSource = bs;

                        cmd.Dispose();
                        cnn.Close();

                    }
                }
                BindingCheck();

            }
        }

        //項目查詢
        private void textBox_SearchItem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox_SearchName.SelectAll();
                label_TableName.Text = comboBox1.Text;

                using (SqlConnection cnn = GetConnection("新新"))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cnn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = comboBox1.Text + "項目查詢";
                        cmd.Parameters.Add("@item", SqlDbType.NVarChar, 20);
                        cmd.Parameters["@item"].Direction = ParameterDirection.Input;
                        cmd.Parameters["@item"].Value = textBox_SearchItem.Text;

                        cnn.Open();
                        SqlDataReader sr = cmd.ExecuteReader();
                        DataTable tt = new DataTable();
                        tt.Load(sr);
                        bs.DataSource = tt;
                        dataGridView1.DataSource = bs;

                        cmd.Dispose();
                        cnn.Close();

                    }
                }

                BindingCheck();
            }
        }

        //日期查詢
        private void textBox_SearchDate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox_SearchName.SelectAll();
                label_TableName.Text = comboBox1.Text;

                using (SqlConnection cnn = GetConnection("新新"))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = cnn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = comboBox1.Text + "日期月份查詢";
                        cmd.Parameters.Add("@date", SqlDbType.Date);
                        cmd.Parameters["@date"].Direction = ParameterDirection.Input;
                        cmd.Parameters["@date"].Value = textBox_SearchDate.Text;

                        try
                        {
                            cnn.Open();
                            SqlDataReader sr = cmd.ExecuteReader();
                            DataTable tt = new DataTable();
                            tt.Load(sr);
                            bs.DataSource = tt;
                            dataGridView1.DataSource = bs;
                        }
                        catch (FormatException FormatEx)
                        {
                            MessageBox.Show(FormatEx.Message + "\r\n" + "請輸入年份與月份，例如:2018-1", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        finally
                        {
                            cmd.Dispose();
                            cnn.Close();
                        }
                    }
                }
                BindingCheck();
            }
        }

        //選擇表
        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox cb = sender as ComboBox;

            switch (cb.Text)
            {
                case "庫存表":
                    textBox_SearchName.Enabled = true;
                    textBox_SearchItem.Enabled = true;
                    textBox_SearchDate.Enabled = false;
                    break;

                case "進貨表":
                case "出貨表":
                    textBox_SearchDate.Enabled = true;
                    textBox_SearchName.Enabled = true;
                    textBox_SearchItem.Enabled = true;
                    break;
                default:
                    textBox_SearchDate.Enabled = false;
                    textBox_SearchName.Enabled = false;
                    textBox_SearchItem.Enabled = false;
                    break;
            }
        }

        private void UpDownBtn(object sender, EventArgs e)
        {
            Button bt = sender as Button;

            int now = position.IndexOf(bs.Position);
            int last = position.IndexOf(position.Last());
            switch (bt.Text)
            {
                case "向下":
                    if (now < last)
                        bs.Position = position[now + 1];
                    break;
                case "向上":
                    if (now > 0)
                        bs.Position = position[now - 1];
                    break;

            }
            
        }
    }
}
