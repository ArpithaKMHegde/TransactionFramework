using DatabaseConnect;
using Microsoft.Extensions.Configuration;
using Modals;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ProductsAPI.Data
{
    public class ProductDataHandler
    {
        public string ConnectionString = (new ConfigurationBuilder()).AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["ProductBase"];
        public List<Product> Products { get; set; }
        public List<Product> GetProducts()
        {
            try
            {
                string StoreProcedureName = "GetProduct";
                if (Products == null) Products = new List<Product>();
                DatabaseConnectAndExecute db = new DatabaseConnectAndExecute(ConnectionString);
                using (DbDataReader returnReader = db.GetDataReader(StoreProcedureName))
                {
                    while (returnReader.Read())
                    {
                        Products.Add(new Product()
                        {
                            ProductId = Convert.ToInt32(returnReader["ProductId"]),
                            ProductName = returnReader["ProductName"].ToString(),
                            ProductNumber = returnReader["ProductNumber"].ToString(),
                            ProductDescription = returnReader["ProductDescription"].ToString(),
                            Brand = returnReader["Brand"].ToString(),
                            Quantity = Convert.ToInt32(returnReader["Quantity"]),
                            PricePerUnit = Convert.ToDouble(returnReader["PricePerUnit"]),
                        });
                    }

                }
                return Products;
            }
            catch (Exception ex)
            {
                return new List<Product>();
            }
        }

        internal Product SaveProduct(Product pProduct)
        {
            string StoreProcedureName = "SaveProduct";
            DatabaseConnectAndExecute db = new DatabaseConnectAndExecute(ConnectionString);
            List<SqlParameter> parameters = new List<SqlParameter>();
            List<Tuple<string,Type, object>> param = new List<Tuple<string, Type, object>>();
            Tuple<string, Type, object> p = new Tuple<string, Type, object>("ProductId", pProduct.ProductId.GetType(), pProduct.ProductId); param.Add(p);
            p = new Tuple<string, Type, object>("ProductNumber", pProduct.ProductNumber.GetType(), pProduct.ProductNumber); param.Add(p);
            p = new Tuple<string, Type, object>("ProductName", pProduct.ProductName.GetType(), pProduct.ProductName); param.Add(p);
            p = new Tuple<string, Type, object>("ProductDescription", pProduct.ProductDescription.GetType(), pProduct.ProductDescription); param.Add(p);
            p = new Tuple<string, Type, object>("Brand", pProduct.Brand.GetType(), pProduct.Brand); param.Add(p);
            p = new Tuple<string, Type, object>("MemberId", pProduct.MemberId.GetType(), pProduct.MemberId); param.Add(p);
            p = new Tuple<string, Type, object>("Quantity", pProduct.Quantity.GetType(), pProduct.Quantity); param.Add(p);
            p = new Tuple<string, Type, object>("PricePerUnit", (0.0d).GetType(), pProduct.PricePerUnit); param.Add(p);
            p = new Tuple<string, Type, object>("IsDeleted", pProduct.IsDeleted.GetType(), pProduct.IsDeleted); param.Add(p);

            parameters.Add(db.GetTableParameter("@ProductItems", "ProductType", param));
            using (DbDataReader returnReader = db.GetDataReader(StoreProcedureName, parameters))
            {
                //db.ExecuteNonQuery(StoreProcedureName, parameters);
                while (returnReader.Read())
                {
                    return new Product()
                    {
                        ProductId = Convert.ToInt32(returnReader["ProductId"]),
                        ProductName = returnReader["ProductName"].ToString(),
                        ProductNumber = returnReader["ProductNumber"].ToString(),
                        ProductDescription = returnReader["ProductDescription"].ToString(),
                        Brand = returnReader["Brand"].ToString(),
                        Quantity = Convert.ToInt32(returnReader["Quantity"]),
                        PricePerUnit = Convert.ToDouble(returnReader["PricePerUnit"]),
                        MemberId = Convert.ToInt32(returnReader["MemberId"])
                    };
                }
            }
            return new Product()
            {
                Brand = "",
                PricePerUnit = 0.0,
                ProductDescription = "",
                ProductId = 0,
                ProductName = "",
                ProductNumber = "",
                Quantity = 0,
                MemberId = 0
            };
        }

        public Product GetProduct(int Id)
        {
            string StoreProcedureName = "GetProduct";

            DatabaseConnectAndExecute db = new DatabaseConnectAndExecute(ConnectionString);
            List<DbParameter> parameters = new List<DbParameter>();
            parameters.Add(db.GetParameter("Id", Id));
            using (DbDataReader returnReader = db.GetDataReader(StoreProcedureName, parameters))
            {
                while (returnReader.Read())
                {
                    return new Product()
                    {
                        ProductId = Convert.ToInt32(returnReader["ProductId"]),
                        ProductName = returnReader["ProductName"].ToString(),
                        ProductNumber = returnReader["ProductNumber"].ToString(),
                        ProductDescription = returnReader["ProductDescription"].ToString(),
                        Brand = returnReader["Brand"].ToString(),
                        Quantity = Convert.ToInt32(returnReader["Quantity"]),
                        PricePerUnit = Convert.ToDouble(returnReader["PricePerUnit"]),
                        MemberId = Convert.ToInt32(returnReader["MemberId"])
                    };
                }
            }
            return new Product()
            {
                Brand = "",
                PricePerUnit = 0.0,
                ProductDescription = "",
                ProductId = 0,
                ProductName = "",
                ProductNumber = "",
                Quantity = 0,
                MemberId = 0
            };
        }

        public void DeleteProduct(Product pProduct)
        {
            try
            {
                string StoreProcedureName = "SaveProduct";
                DatabaseConnectAndExecute db = new DatabaseConnectAndExecute(ConnectionString);
                List<SqlParameter> parameters = new List<SqlParameter>();
                List<Tuple<string, Type, object>> param = new List<Tuple<string, Type, object>>();
                Tuple<string, Type, object> p = new Tuple<string, Type, object>("ProductId", pProduct.ProductId.GetType(), pProduct.ProductId); param.Add(p);
                p = new Tuple<string, Type, object>("ProductNumber", pProduct.ProductNumber.GetType(), pProduct.ProductNumber); param.Add(p);
                p = new Tuple<string, Type, object>("ProductName", pProduct.ProductName.GetType(), pProduct.ProductName); param.Add(p);
                p = new Tuple<string, Type, object>("ProductDescription", pProduct.ProductDescription.GetType(), pProduct.ProductDescription); param.Add(p);
                p = new Tuple<string, Type, object>("Brand", pProduct.Brand.GetType(), pProduct.Brand); param.Add(p);
                p = new Tuple<string, Type, object>("MemberId", pProduct.MemberId.GetType(), pProduct.MemberId); param.Add(p);
                p = new Tuple<string, Type, object>("Quantity", pProduct.Quantity.GetType(), pProduct.Quantity); param.Add(p);
                p = new Tuple<string, Type, object>("PricePerUnit", (0.0d).GetType(), pProduct.PricePerUnit); param.Add(p);
                p = new Tuple<string, Type, object>("IsDeleted", 0.GetType(), 0); param.Add(p);
                parameters.Add(db.GetTableParameter("@ProductItems", "ProductType", param));
                using (DbDataReader returnReader = db.GetDataReader(StoreProcedureName, parameters))
                {
                    db.ExecuteNonQuery(StoreProcedureName, parameters);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
