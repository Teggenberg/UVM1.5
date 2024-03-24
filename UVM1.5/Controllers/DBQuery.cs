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

        public static List<MenuOption> GetOptions(string table)
        {
            // new list that will be retunred for drop down menu
            List<MenuOption> list = new();

            // query table for values to populate list using column and table parameters
            SqlConnection sqlconn = new(GetConnectionString());
            string sqlquery = $"select * from {table}";
            SqlCommand sqlcomm = new(sqlquery, sqlconn);
            sqlconn.Open();
            SqlDataAdapter adapter = new(sqlcomm);
            DataTable dt = new();
            adapter.Fill(dt);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                // add value to list
                MenuOption option = new()
                {
                    Value = Convert.ToInt32(dt.Rows[i][0]),
                    Name = dt.Rows[i][1].ToString(),
                };

                list.Add(option);

            }
            sqlconn.Close();
            return list;
        }




    }
}
