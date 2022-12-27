using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace PracticStore
{
    public partial class Form1 : Form
    {
        Npgsql.NpgsqlConnection conn = new NpgsqlConnection("Server=195.133.145.141;Port=5432;User Id=eq_admin;Password=BeMeLeTe24;Database=device_accounting;");
        public Form1()
        {
            InitializeComponent();
            conn.Open();
            UpdateDataGridView1();
            UpdateDataGridView2();
            UpdateDataGridView3();

            Npgsql.NpgsqlDataReader dr = executeQuery(@"SELECT 'ID:' || dt.type_id || ' ' || dt.device_title FROM stock_info.device_type as dt;").ExecuteReader();
            while (dr.Read())
            {
                comboBox3.Items.Add(dr[0]);
            }
            dr.Close();
        }


        //Обновление и считывание таблицы - список устройств
        private void UpdateDataGridView3()
        {
            Npgsql.NpgsqlDataReader dr = executeQuery(@"
                    select dos.id, dt.device_title, dos.device_invent_number, dos.service_date, dos.status
                        from stock_info.device_on_stock as dos
                        join stock_info.device_type as dt on dos.device_type = dt.type_id;").ExecuteReader();
            if (dr.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(dr);
                dataGridView3.DataSource = dt;
            }
            dr.Close();
        }

        //Обновление и считывание таблицы stock_info.v_employes_device без фильтра
        private void UpdateDataGridView1()
        {
            Npgsql.NpgsqlDataReader dr = executeQuery("select * from stock_info.v_employes_device;").ExecuteReader();
            if (dr.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(dr);
                dataGridView1.DataSource = dt;
            }
            dr.Close();
        }

        //Обновление и считывание таблицы stock_info.v_employes_device с фильтром
        private void UpdateDataGridView1(string filter)
        {
            Npgsql.NpgsqlDataReader dr = executeQuery($"select * from stock_info.v_employes_device {filter};").ExecuteReader();
            if (dr.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(dr);
                dataGridView1.DataSource = dt;
            }
            dr.Close();
        }

        //Обновление и считывание таблицы stock_info.v_emp_list без фильтра
        private void UpdateDataGridView2()
        {
            Npgsql.NpgsqlDataReader dr = executeQuery("select * from stock_info.employes_list as el;").ExecuteReader();
            if (dr.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(dr);
                dataGridView2.DataSource = dt;
            }
            dr.Close();
        }

        //Обновление и считывание таблицы stock_info.v_emp_list с фильтром
        private void UpdateDataGridView2(string filter)
        {
            Npgsql.NpgsqlDataReader dr = executeQuery($"SELECT x.* FROM stock_info.v_emp_list as x {filter};").ExecuteReader();
            if (dr.HasRows)
            {
                DataTable dt = new DataTable();
                dt.Load(dr);
                dataGridView2.DataSource = dt;
            }
            dr.Close();
        }

        //Исполнитель запросов
        private NpgsqlCommand executeQuery(string query)
        {
            NpgsqlCommand comm = new NpgsqlCommand();
            comm.Connection = conn;
            comm.CommandType = CommandType.Text;
            comm.CommandText = query;
            return comm;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string res = comboBox1.SelectedItem?.ToString();

            if (res is null)
            {
                UpdateDataGridView1();
            }
            else if (res.Equals("Требуется обслуживание"))
            {
                UpdateDataGridView1("where service_date = now()::date");
            }
            else if (res.Equals("Показать только закрепленное"))
            {
                UpdateDataGridView1("where row_id is not null");
            }
            else if (res.Equals("Показать только не закрепленное"))
            {
                UpdateDataGridView1("where row_id is null");
            }
            else
            {
                UpdateDataGridView1();
            }

            label2.Text = "Последнее обновление: " + DateTime.Now;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string res = comboBox2.SelectedItem?.ToString();

            if (res is null)
            {
                UpdateDataGridView2();
            }
            else if (res.Equals("Лист сотрудников"))
            {
                UpdateDataGridView2();
            }
            else if (res.Equals("Cотрудники с оборудованием"))
            {
                UpdateDataGridView2("where device_invent_number is not null");
            }
            else if (res.Equals("Без фильтра"))
            {
                UpdateDataGridView2("order by 1");
            }    
            else
            {
                UpdateDataGridView2();
            }

            label3.Text = "Последнее обновление: " + DateTime.Now;
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        //Добавить сотрудника
        private void button3_Click(object sender, EventArgs e)
        {
            string lastName, firstName, perStr;
            int perNum;

            lastName = textBox1.Text.ToString();
            firstName = textBox2.Text.ToString();

            try
            {
                perStr = textBox3.Text?.ToString();
                perNum = int.Parse(perStr);
                //Обход ошибки-дубля - игнорирование вставки. Может потом поправлю
                Npgsql.NpgsqlDataReader dr = executeQuery(@$"
                                INSERT INTO stock_info.employes_list
                                (last_name, first_name, personnel_number)
                                VALUES('{lastName}', '{firstName}', {perNum})
                                on conflict (personnel_number) do nothing;").ExecuteReader();
                dr.Close();

                textBox1.Clear();
                textBox2.Clear();
                textBox3.Clear();

                label10.Text = "Сотрудник добавлен!";
            }
            catch
            {
                label10.Text = "Поля не заполнены!";
            }

        }

        //Удалить сотрудника
        private void button4_Click(object sender, EventArgs e)
        {
            int emID = int.Parse(textBox4.Text.ToString());

            //Обход ошибки-дубля - игнорирование вставки. Может потом поправлю
            try
            {
                Npgsql.NpgsqlDataReader dr = executeQuery(@$"
                                delete from stock_info.employes_list as el
                                    where el.employes_id = {emID};").ExecuteReader();
                dr.Close();
                textBox4.Clear();

                label12.Text = "Сотрудник удален!";
            }
            catch (Npgsql.PostgresException)
            {
                label12.Text = "Ошибка! Есть девайс!";
            }
        }

        //Добавить устройство
        private void button5_Click(object sender, EventArgs e)
        {
            string devNumst = textBox6.Text?.ToString();
            string res = comboBox3.SelectedItem?.ToString();
            int resInt;

            if (res is null || devNumst is null || devNumst.Equals(""))
            {
                label18.Text = "Не указано устройство/номер!";
                label18.BackColor = Color.Red;
            }
            else
            {
                int devNum = int.Parse(devNumst);
                res = res.Split(' ')[0];
                res = res.Substring(3);
                resInt = int.Parse(res);

                //Выполнить операцию по добавлению
                Npgsql.NpgsqlDataReader dr = executeQuery(@$"
                                INSERT INTO stock_info.device_on_stock
                                (device_type, device_invent_number, service_date, status)
                                VALUES
                                ({resInt}, {devNum}, ('2022-12-20'::date + interval '30 day')::date, default);").ExecuteReader();
                dr.Close();

                comboBox3.SelectedIndex = -1;
                textBox6.Clear();

                label18.Text = "Устройство добавлено!";
            }
        }

        //Обновить таблицу с устройствами
        private void button6_Click(object sender, EventArgs e)
        {
            UpdateDataGridView3();
            label17.Text = "Последнее обновление: " + DateTime.Now;
        }

        //Удалить устройство
        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}