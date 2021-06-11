// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System.Collections.Generic;

namespace Microsoft.BotBuilderSamples
{
    //This class is for the report parameter filters
    public class ConversationData
    {
        public string ChannelId { get; set; }
        public string Token { get; set; }
        public bool forFAQ = false;

        public string reportName { get; set; }

        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string AsOfDate { get; set; }

        public string PledgeeName { get; set; }
        public string CompanyName { get; set; }
        public string BrokerageFirm { get; set; }
        public string StatusCode { get; set; }

        public int TransactionType { get; set; }
        public int ReportType { get; set; }
        public int DividendType { get; set; }

        public int TransferType { get; set; }
        public int TransferDirection { get; set; }
    }

    public class BalanceSheetParams
    {
        //to serialize as json
        public string AsOfDate;
        public List<string> Brokers = new List<string>();
        public List<string> Symbols = new List<string>();
        public int Format = 0;
    }

    public class StatementofAccountParams
    {
        //to serialize as json for both "Ivestor Satetemt of Account" and "Investor Trade Report" APIs
        public string FromDate;
        public string ToDate;
        public List<string> Brokers= new List<string>();
        public List<string> Symbols = new List<string>();
        public int Format=0; 
    }

    public class StockMvtParams
    {
        //to serialize as json
        public string FromDate;
        public string ToDate;
        public int ReportType = 0;
        public int Format = 0;
        public List<string> Brokers = new List<string>();
        public List<string> Symbols = new List<string>();
    }

    public class PldgeStatementParams
    {
        //to serialize as json
        public string FromDate;
        public string ToDate;
        public int TransactionType = 0;
        public int Format = 0;
        public List<string> Pledges = new List<string>();
        public List<string> Symbols = new List<string>();
    }

    public class OwnershipTransferParams
    {
        //to serialize as json
        public string FromDate;
        public string ToDate;
        public int TransferType = 0;
        public int TransferDirection = 0;
        public int Format = 0;
        public List<string> SymbolCodes = new List<string>(); // will be sent empty always
    }

    public class DividendInquiryParams
    {
        //to serialize as json
        public string FromDate;
        public string ToDate;
        public int DividendType = 0;
        public int Format = 0;
        public List<string> SymbolCodes = new List<string>();
        public List<string> StatusCodes = new List<string>();
    }

    public class IPOParams
    {
        //to serialize as json
        public string FromDate;
        public string ToDate;
        public int Format = 0;
        public List<string> SymbolCodes = new List<string>();
    }

    //service5 Params
    public class GetCoporateParams
    {
        public string Symbol;
        public string Year;
        public string SenderInfo = "CRM";
    }

    public class CalculateMyDividendParams
    {
        public string SenderInfo="CRM";
        public string EQSYMBOL;
        public int QTY;
        public string Year;
    }

    public class GetInvestorDividendParams
    {
        public string SenderInfo = "CRM";
        public string EQSYMBOL;
        public string Year;
    }
}
