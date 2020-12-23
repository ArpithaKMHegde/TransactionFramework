using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DemoTransactionFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modals;

namespace ElectronicStore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionController : ControllerBase
    {

        private readonly ILogger<TransactionController> _logger;
        public bool? bConnectToAPI = null;
        public bool? bEnableFrameworkTransaction = null;

        public TransactionController(ILogger<TransactionController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Transaction> Get()
        {
            try
            {
                if (!bConnectToAPI.HasValue)
                {
                    string s = (new ConfigurationBuilder()).AddJsonFile("appsettings.json").Build().GetSection("CustomVariables")["ConnectToAPI"];


                    bConnectToAPI = string.IsNullOrEmpty(s) ? false : s == "Yes" ? true : false;

                }
                if (bConnectToAPI.Value)
                {
                    return APIHandler<IEnumerable<Transaction>>.GetMethod("https://localhost:44318/Transactions");
                }
                else
                {
                    UIIndependentTest test = new UIIndependentTest();
                    test.LoadTestData();
                    return UIIndependentTest.Transactions.ToArray();
                }
            }
            catch (Exception ex)
            {
                UIIndependentTest test = new UIIndependentTest();
                test.LoadTestData();
                return UIIndependentTest.Transactions.ToArray();
            }
        }
        [HttpPost]
        public Transaction Post([FromBody] Transaction pTransaction)
        {
            try
            {
                if (!bConnectToAPI.HasValue)
                {
                    string s = (new ConfigurationBuilder()).AddJsonFile("appsettings.json").Build().GetSection("CustomVariables")["ConnectToAPI"];
                    bConnectToAPI = string.IsNullOrEmpty(s) ? false : s == "Yes" ? true : false;
                    //NOTETHEPOINT: Provide mechanism to decide between sync and async 
                    s = (new ConfigurationBuilder()).AddJsonFile("appsettings.json").Build().GetSection("CustomVariables")["EnableFrameworkTransaction"];
                    bEnableFrameworkTransaction = string.IsNullOrEmpty(s) ? false : s == "Yes" ? true : false;
                }
                if (bConnectToAPI.Value)
                {
                    var TrHandler = new TransactionHandler<Transaction>(); TrHandler.CreateTransactionObject();
                    
                    IHTTPServiceEndpoint TrServiceEndPoint = new DemoHTTPServiceEndpoint<Transaction, Transaction, int>()
                    {
                        FullURL = "https://localhost:44318/Transactions",
                        Method = "post",
                        Input = pTransaction,
                    };
                    
                    if (pTransaction.Invoice.BuyingMember.MemberId == 0 && pTransaction.Invoice.BuyingMember.FirstName != "")
                    {
                        //NOTETHEPOINT: Provide interfaces for APIs to handle rollbacks
                        Func<Member, Transaction, int, DemoHTTPServiceEndpoint<Member, Transaction, int>, bool> AssignTo = (U, V, i , W) =>
                        {
                            try
                            {
                                V.Invoice.BuyingMember = U;
                                //NOTETHEPOINT: Provide mechanism to decide between sync and async 
                                if (!bEnableFrameworkTransaction.Value)
                                {
                                    return true;
                                }
                                if (U != null)
                                {
                                    U.IsDeleted = 1;
                                    W.ReverseInput = U;
                                    if (U.MemberId == 0)
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                                return true;
                            }
                            catch (Exception ex)
                            {
                                return false;
                            }
                        };
                        IHTTPServiceEndpoint MrServiceEndPoint = new DemoHTTPServiceEndpoint<Member, Transaction, int>()
                        {
                            FullURL = "https://localhost:44367/Members",
                            Method = "post",
                            Input = pTransaction.Invoice.BuyingMember,
                            mainServiceObject = pTransaction,
                            Index = 0
                        };
                        TrHandler.AddServicePoint("Member", MrServiceEndPoint);
                        
                        (MrServiceEndPoint as DemoHTTPServiceEndpoint<Member, Transaction, int>).OnComplete += AssignTo;
                    }
                    int k = pTransaction.Invoice.Items.Count();
                    for (int i = 0; i < k; i++)
                    {
                        var item = pTransaction.Invoice.Items[i];
                        if (item.SoldProduct.ProductId == 0 && item.SoldProduct.ProductNumber != "")
                        {
                            //NOTETHEPOINT: Provide interfaces for APIs to handle rollbacks
                            Func<Product, Transaction, int, DemoHTTPServiceEndpoint<Product, Transaction, int>, bool> AssignTo = (U, V, i, W) =>
                            {

                                try
                                {
                                    V.Invoice.Items[i].SoldProduct = U;
                                    //NOTETHEPOINT: Provide mechanism to decide between sync and async
                                    if (!bEnableFrameworkTransaction.Value)
                                    {
                                        return true;
                                    }
                                    if (U != null)
                                    {
                                        U.IsDeleted = 1;
                                        W.ReverseInput = U;
                                        if (U.ProductId == 0)
                                        {
                                            return false;
                                        }
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                    return true;
                                }
                                catch (Exception ex)
                                {
                                    return false;
                                }
                               
                            };
                            IHTTPServiceEndpoint MrServiceEndPoint = new DemoHTTPServiceEndpoint<Product, Transaction, int>()
                            {
                                FullURL = "https://localhost:44303/Products",
                                Method = "post",
                                Input = item.SoldProduct,
                                mainServiceObject = pTransaction,
                                Index = i
                            };
                            TrHandler.AddServicePoint("Product", MrServiceEndPoint);

                            (MrServiceEndPoint as DemoHTTPServiceEndpoint<Product, Transaction, int>).OnComplete += AssignTo;
                        }
                    }
                     
                    TrHandler.AddServicePoint("Transaction", TrServiceEndPoint);
                    TrHandler.LocalTransactionReference.EnableFrameworkTransaction = bEnableFrameworkTransaction.Value;
                    TrHandler.ExecuteTransaction();
                    var op = (TrServiceEndPoint as DemoHTTPServiceEndpoint<Transaction, Transaction, int>).Output;
                    op = op == null ? (TrServiceEndPoint as DemoHTTPServiceEndpoint<Transaction, Transaction, int>).Input : op;
                    op.TransactionMessage = TrHandler.LocalTransactionReference.ServiceEndPoints.Where(p => p.Value != null).Where(q=>  q.Value.CustomErrorOccured == true).Count() > 0 ? "Save Failed" : "Save Succeed";
                    return op;
                    //return TrHandler.PostMethod("https://localhost:44318/Transactions", pTransaction);
                    //return  APIHandler<Transaction>.PostMethod("https://localhost:44318/Transactions", pTransaction);
                }
                else
                {
                    //Product pProduct = JsonSerializer.Deserialize<Product>(product);
                    //UIIndependentTest test = new UIIndependentTest();
                    //test.LoadTestData();
                    //test.AddT(pProduct);
                    //return pProduct;
                }
            }
            catch (Exception ex)
            {
                //UIIndependentTest test = new UIIndependentTest();
                //test.LoadTestData();
                //return UIIndependentTest.Products.FirstOrDefault();
                pTransaction.TransactionMessage = "Save failed";
            }
            return pTransaction;
        }

        
    }
}
