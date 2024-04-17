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
            if(table == "Category")
            {
                sqlquery += " where Category_name != 'Ai Failure';";
            }
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

        public static DataTable SelectAll(string selectQuery)
        {

            // query table for values to populate list using column and table parameters
            SqlConnection sqlconn = new(GetConnectionString());
            SqlCommand sqlcomm = new(selectQuery, sqlconn);
            sqlconn.Open();
            SqlDataAdapter adapter = new(sqlcomm);
            DataTable dt = new();
            adapter.Fill(dt);
            sqlconn.Close();
            return dt;

        }

        public static void AddImageToDB(int fk, int position, byte[] image)
        {
            SqlConnection sqlconn = new(GetConnectionString());
            const string sql_insert_string =
                "Update Item_Image " +
                "\r\nSet Item_Image = @image_byte_array" +
                "\r\nWhere Item_Id = @Item_Id" +
                "\r\nAnd Position = @Position;";

            var byteParam = new SqlParameter("@image_byte_array", SqlDbType.VarBinary)
            {
                Direction = ParameterDirection.Input,
                Size = image.Length,
                Value = image
            };

            var imageIdParam = new SqlParameter("@Item_id", SqlDbType.Int, 4)
            {
                Direction = ParameterDirection.Input,
                Value = fk
            };

            var positionParam = new SqlParameter("@Position", SqlDbType.Int, 4)
            {
                Direction = ParameterDirection.Input,
                Value = position
            };

            SqlTransaction transaction = null;
            SqlCommand sqlcomm = new(sql_insert_string, sqlconn, transaction);
            sqlcomm.Parameters.Add(byteParam);
            sqlcomm.Parameters.Add(imageIdParam);
            sqlcomm.Parameters.Add(positionParam);
            sqlconn.Open();
            sqlcomm.ExecuteNonQuery();
            sqlconn.Close();
        }

        public static List<Image?>? GetImages(int? itemId)
        {
            List<Image?>? itemImages = new();
            for(int i = 0; i < 4; i++)
            { 
                Image img = new Image();
                
                itemImages.Add(img);
            }

            string query = $"select Item_Image, Position, flag, verified from Item_Image " +
                $"\r\n where Item_Id = {itemId}" +
                $"\r\nAnd Item_Image is not null;";

            DataTable dt = SelectAll(query);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if(dt.Rows[i][0] != null)
                {
                    itemImages[Convert.ToInt32(dt.Rows[i][1])-1].Img = (byte[])dt.Rows[i][0];
                    itemImages[Convert.ToInt32(dt.Rows[i][1]) - 1].Position = Convert.ToInt32(dt.Rows[i][1]);
                    itemImages[Convert.ToInt32(dt.Rows[i][1]) - 1].Flag = (bool?)dt.Rows[i][2];
					itemImages[Convert.ToInt32(dt.Rows[i][1]) - 1].Verified = (bool?)dt.Rows[i][3];

				}
                
            }

            return itemImages;
        }

        public static void FlagImage(int? id, int? position)
        {
            string flag = "Update Item_image" +
                "\r\nSet flag = 1" +
                $"\r\nWhere Item_Id = {id}" +
                $"\r\nWhere Postition = {position}";
            Insert(flag);

        }

        public static byte[]? GetImage(int? itemId)
        {
            byte[]? itemImage = null;
           
            string query = $"select top 1 Item_Image, Position from Item_Image " +
                $"\r\n where Item_Id = {itemId}" +
                $"\r\nAnd Item_Image is not null;";
                

            DataTable dt = SelectAll(query);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i][0] != null)
                {
                    itemImage =  (byte[])dt.Rows[i][0];
                }

            }

            return itemImage;
        }

        public static void UpdateItemInfo(Item item)
        {
            
            string query = "Update Item" +
                $"\r\nSet Brand = {item.Brand.Value}, Model = '{item.Model}', YYYY = '{item.Year}', Color = '{item.Color}', " +
                $"\r\nCondition = {item.Condition.Value}, Category = {item.Category.Value}, Description = '{item.Description}'," +
                $"\r\nDetails = '{item.Details}', Cost = {item.Cost}, Retail = {item.Retail}" +
                $"\r\nWhere Id = {item.Id};";

            Insert(query);

        }




    }
}
