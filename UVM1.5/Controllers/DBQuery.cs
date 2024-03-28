using System.Data;
using System.Data.SqlClient;
using UVM1._5.Models;

namespace UVM1._5.Controllers
{
    public static class DBQuery
    {

        public static string GetConnectionString()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json", optional: false);
            IConfiguration configuration = builder.Build();

            var connString = configuration.GetConnectionString("UVM_DB");
            return connString;
        }

        public static List<Pair> GetOptions(string table, string order = "")
        {
            // new list that will be retunred for drop down menu
            List<Pair> list = new();

            // query table for values to populate list using column and table parameters
            SqlConnection sqlconn = new(GetConnectionString());
            string sqlquery = $"select * from {table}";
            if(order != "")
            {
                sqlquery += $"\nOrder by {order};";
            }
            SqlCommand sqlcomm = new(sqlquery, sqlconn);
            sqlconn.Open();
            SqlDataAdapter adapter = new(sqlcomm);
            DataTable dt = new();
            adapter.Fill(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // add value to list
                Pair option = new()
                {
                    Value = Convert.ToInt32(dt.Rows[i][0]),
                    Name = dt.Rows[i][1].ToString(),
                };

                list.Add(option);

            }
            sqlconn.Close();
            return list;
        }

        //insert into any table, and return id of new row
        public static int Insert(string insertQuery)
        {
            //var to store new id upon inserted row
            int newId;

            //open db connection and run passed in query
            SqlConnection sqlconn = new(GetConnectionString());
            SqlCommand sqlquery = new(insertQuery, sqlconn);
            sqlconn.Open();

            //execute query statement, return new id
            newId = Convert.ToInt32(sqlquery.ExecuteScalar());
            System.Diagnostics.Debug.WriteLine("New ID : " + newId);
            sqlconn.Close();
            return newId;
        }




    }
}
