using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailReceiverService
{
    public class DB
    {
        public string Connection = "Data Source=192.168.0.201; Initial Catalog=MENAA3; User ID=sa; Password=8888";
        public string Message = "";

        public DataTable ExecSQL(string sql, out string result)
        {
            try
            {
                DataTable t = new DataTable();
                SqlDataAdapter sda = new SqlDataAdapter(sql, this.Connection);
                sda.Fill(t);
                result = "";
                return t.Copy();
            }
            catch (Exception e)
            {
                //Common.Log("Receiver.ExecSQL > sql : " + sql + " > " + e.Message);
                result = "DB.ExecQuery > Exception > " + e.Message;
                return null;
            }

        }

        public string Update(DataTable t)
        {
            try
            {
                string sql = "SELECT * FROM " + t.TableName + " WHERE 1 = 0";
                SqlDataAdapter sda = new SqlDataAdapter(sql, this.Connection);
                SqlCommandBuilder scb = new SqlCommandBuilder(sda);
                sda.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                sda.Update(t);
                return "";
            }
            catch (Exception e)
            {
                //Common.Log("Receiver.Update > tablename :" + t + "  > " + e.Message);
                return "DB.Update > Exception > " + e.Message;
            }
        }

        public string GetValue(string sql, int ColIndex, string DefaultVal = "")
        {
            string result = "";
            DataTable t = this.ExecSQL(sql, out result);
            if (t.Rows.Count > 0)
                return t.Rows[0][ColIndex].ToString();

            return DefaultVal;
        }
    }
}
