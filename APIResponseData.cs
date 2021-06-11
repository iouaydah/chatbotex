// Developed by Ismail Ouaydah for Ubility Customer ADX
// namespace BotBuilderSamples

using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{

    //Models to serialize result returned from api call
    //===================================================
    //List of elements in Data
    public class APIResponseListstModel<T>
    {
        public List<T> Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    // single element in Data
    public class APIResponseSingleModel<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    //Service2.1 Data
    public class DailySummaryAPIData
    {
        public string Title { get; set; }
        public string URL { get; set; }
    }
    //Service2.2 Data
    public class SymbolDetailsAPIData
    {
        public string ArabicName { get; set; }
        public float CLOSE_PERCENT_CHANGE { get; set; }
        public float CLOSE_PRICE { get; set; }
        public string DSYMBOL { get; set; }
        public string EQSymbol { get; set; }
        public string EnglishName { get; set; }
        public float HIGH_PRICE { get; set; }
        public float LOW_PRICE { get; set; }
        public string LastTraded { get; set; }
        public float OPEN_PRICE { get; set; }
        public float PREV_CLOSE_PRICE { get; set; }
        public float TRADES_COUNT { get; set; }
        public float TRADE_VALUE { get; set; }
        public float TRADE_VOLUME { get; set; }
        public string TradingSymbol { get; set; }
        public string URL { get; set; }
    }
   
    //classes to serialize service 3
    //3-1 Reports Filter
    public class ReportFilterModel
    {
        public JObject Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }
    //3-2 Reports pdf
    public class ReportModelData
    {
        public string Extention { get; set; }
        public string FileMIMEType { get; set; }
        public string Data { get; set; }
    }
    public class ReportResponseModel
    {
        //serialized by the API Call
        public ReportModelData Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    //Service4: FeesCalculator
    //4-1 service list
    public class ServiceListAPIData
    {
        public string ID { get; set; }
        public string NameEng { get; set; }
        public string NameArb { get; set; }
    }
    //4-2 Service Fees
    public class ServiceFeeAPIData
    {
        public string ClosingDate { get; set; }
        public float ClosingPrice { get; set; }
        public float Total { get; set; }
        public float VAT { get; set; }
    }
    
    //Service 5: Calculate Dividend List of all symbols
    public class Service5SymbolsData
    {
        public string DisplaySymbol { get; set; }
        public string EQSymbol { get; set; }
        public string SymbolArabicName { get; set; }
        public string SymbolEnglishName { get; set; }
        public string TradingSymbol { get; set; }
    }

    //Service 5: Interims
    public class Service5CorporateActionsData
    {
        public string AGMDate { get; set; }
        public float BonusPercent { get; set; }
        public float DividendPercent { get; set; }
        public string LED { get; set; }
        public string PaymentDate { get; set; }
        public string RDC { get; set; }
        public string Symbol { get; set; }
        public string SymbolNameArb { get; set; }
        public string SymbolNameEng { get; set; }
    }

    //single element in Data. Service 5 Calculate Qty
    public class Service5CalculateData
    {
        public float Dividend { get; set; }
        public float Cash { get; set; }
    }

    //single element in Data Model
    public class Service5CalculateModel
    {
        public Service5CalculateData Data { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    // Not Used: Service5 CheckMyDividend
    public class Service5InvestorDividendData
    {
        public string Dividend { get; set; }       //float
        public string EﬀectiveDate { get; set; }
        public string Type { get; set; }
        public string Year { get; set; }        //int
    }


}