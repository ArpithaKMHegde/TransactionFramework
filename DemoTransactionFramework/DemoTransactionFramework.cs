using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace DemoTransactionFramework
{
    public class DemoTransaction : IDemoTransactionFramework
    {
        public ITransactionObject LocalTransactionReference { get; set; }
        public void AddServicePoint(string key, IHTTPServiceEndpoint NewService)
        {
            LocalTransactionReference.ServiceEndPoints.Add(key, NewService);
            //NOTETHEPOINT: Binding all services together.
        }

        public void CreateTransactionObject()
        {
            LocalTransactionReference = new DemoHTTPTransactionObject();
            LocalTransactionReference.ServiceEndPoints = new Dictionary<string, IHTTPServiceEndpoint>();
        }

        public void ExecuteTransaction()
        {
            bool bError = false;
            try
            {
                foreach(var item in LocalTransactionReference.ServiceEndPoints)//NOTETHEPOINT: Binding all services together.
                {
                    item.Value.ExecuteService();
                    if (item.Value.CustomErrorOccured) //NOTETHEPOINT:Keep track of transactions
                    {
                        bError = true;
                        break;
                    }
                }
                

                if (bError) //NOTETHEPOINT: Provide interfaces for APIs to handle rollbacks
                {
                    if (LocalTransactionReference.EnableFrameworkTransaction)
                    {
                        foreach (var item in LocalTransactionReference.ServiceEndPoints)//NOTETHEPOINT: Binding all services together.
                        {
                            item.Value.ExecuteReversal();
                        }
                    }
                    

                }

            }
            catch (Exception ex)
            {
                if (LocalTransactionReference.EnableFrameworkTransaction)
                {
                    foreach (var item in LocalTransactionReference.ServiceEndPoints)
                    {
                        item.Value.ExecuteReversal();
                    }
                }
            }
            finally
            { 
            }
            
        }

    }
    public class DemoHTTPServiceEndpoint<U, V, W> : IHTTPServiceEndpoint where U : class where V : class
    {
        public Func<U, V, W, DemoHTTPServiceEndpoint<U, V, W>, bool> OnComplete { get; set; } //NOTETHEPOINT: Provide interfaces for APIs to handle rollbacks
        public DemoHTTPServiceEndpoint(Func<U, V, W, DemoHTTPServiceEndpoint<U, V, W>, bool> onComplete = null)
        {
            if (onComplete != null)
            {
                OnComplete = onComplete;
            }
        }
        public string FullURL { get; set; }
        public string Method { get; set; }
        public TimeSpan TimeOut { get; set; }
        public int OrderOfExecution { get; set; }
        public U Input { get; set; }
        public U ReverseInput { get; set; }
        public U Output { get; set; }
        public V mainServiceObject { get; set; }
        public W Index { get; set; }
        public bool CustomErrorOccured { get; set; }
        public void ExecuteService()
        {
            using (var client = new HttpClient())
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(Input));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = client.PostAsync(FullURL, byteContent); // http://192.168.0.1:915/api/Controller/Object

                response.Wait();

                if (response.IsCompletedSuccessfully == true)
                {
                    string res = response.Result.Content.ReadAsStringAsync().Result;
                    if (res == string.Empty)
                    {
                        CustomErrorOccured = true; //NOTETHEPOINT:Keep track of transactions
                    }
                    else
                    {
                        Output = JsonSerializer.Deserialize<U>(res);
                    
                        if (OnComplete != null) //NOTETHEPOINT:Keep track of transactions
                        {

                            CustomErrorOccured = !OnComplete(Output, mainServiceObject, Index, this);
                        }
                    }
                }
            }
        }

        public void ExecuteReversal()
        {
            using (var client = new HttpClient())
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(ReverseInput));
                var byteContent = new ByteArrayContent(buffer);
                byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = client.PostAsync(FullURL, byteContent); // http://192.168.0.1:915/api/Controller/Object

                response.Wait();

                if (response.IsCompletedSuccessfully == true)
                {
                    string res = response.Result.Content.ReadAsStringAsync().Result;
                    Output = JsonSerializer.Deserialize<U>(res);
                }
            }
        }

    }
    public class DemoHTTPTransactionObject : ITransactionObject
    {
        public Dictionary<string, IHTTPServiceEndpoint> ServiceEndPoints { get; set; }
        public int RetryCounts { get; set; }
        public int TransactionObjectId { get; set; }
        public bool EnableFrameworkTransaction { get; set; }
    }

   
}
