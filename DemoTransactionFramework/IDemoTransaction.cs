using System;
using System.Collections.Generic;
using System.Text;

namespace DemoTransactionFramework
{
    //NOTETHEPOINT: Binding all services together.
    public interface IDemoTransactionFramework
    {
        ITransactionObject LocalTransactionReference { get; set; }
        void CreateTransactionObject();
        void AddServicePoint(string key, IHTTPServiceEndpoint NewService);
        void ExecuteTransaction();
    }

    public interface HttpReslt {
        int ID { get; set; }
        string Description { get; set; }
    }

    public interface IHTTPServiceEndpoint
    {
        string FullURL { get; set; }
        string Method { get; set; }
        TimeSpan TimeOut { get; set; }
        int OrderOfExecution { get; set; }
        void ExecuteService();
        void ExecuteReversal(); //NOTETHEPOINT: Provide interfaces for APIs to handle rollbacks
        bool CustomErrorOccured { get; set; }
        //NOTETHEPOINT:Keep track of transactions
    }

    public interface ITransactionObject
    {
        Dictionary<string, IHTTPServiceEndpoint> ServiceEndPoints { get; set; }
        bool EnableFrameworkTransaction { get; set; }
        int RetryCounts { get; set; }
        //NOTETHEPOINT Provide interfaces for logging and other diagnostic mechanisms.
        //NOTETHEPOINT Consider latency, waiting and re-tries
        int TransactionObjectId { get; set; }
        
    }

}
