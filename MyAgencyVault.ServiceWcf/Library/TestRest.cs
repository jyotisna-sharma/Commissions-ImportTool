using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyAgencyVault.BusinessLibrary;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Dispatcher;
using System.Runtime.Serialization;
using System.Data;
using System.Collections.ObjectModel;

namespace MyAgencyVault.WcfService
{
    [ServiceContract]
    interface ITestRest
    {
        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json,  BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        string GetDataRest(string s);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json,  BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        JSONResponse ImportPolicyService(string PolicyTable, Guid LicenseeID);

        [OperationContract]
        [WebInvoke(ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        ListResponse GetPayorCarrierList(string StartDate, string EndDate);
    }

    public partial class MavService : ITestRest, IErrorHandler
    {
        public string GetDataRest(string s)
        {
            return string.Format("You entered: {0}", s);
        }

        public ListResponse GetPayorCarrierList(string StartDate, string EndDate)
        {
            ListResponse res = null;
            try
            {
                if (WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"] != null)
                {
                    string val = Convert.ToString(WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"]);
                    ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - header key:  " + val, true);
                    if (val != "CommDept1973")
                    {
                        res = new ListResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                        ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - Authentication failure ", true);
                        return res;
                    }
                }
                else
                {
                    res = new ListResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                    ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - Authentication failure ", true);
                    return res;
                }
            }
            catch (Exception ex)
            {
                res = new ListResponse("List cannot be returned as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - Authentication failure , no key in header", true);
                return res;
            }

            try
            {
                //if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
                //{
                DateTime dtStart = DateTime.MinValue;
                DateTime dtEnd = DateTime.MinValue;
                DateTime.TryParse(StartDate, out dtStart);
                ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - Start date: " + dtStart, true);
                DateTime.TryParse(EndDate, out dtEnd);
                ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - EndDate:  " + dtEnd, true);

                if ((dtStart != DateTime.MinValue && dtEnd != DateTime.MinValue) ||
                    (dtStart == DateTime.MinValue && dtEnd == DateTime.MinValue))
                {
                    List<CarrierObject> objCarr = BusinessLibrary.Carrier.GetCarriersOnDate(dtStart, dtEnd);
                    List<PayorObject> objPayr = BusinessLibrary.Payor.GetPayorsOnDate(dtStart, dtEnd);
                    res = new ListResponse("Request processed successfully!", Convert.ToInt16(200), "");
                    res.CarrierList = objCarr;
                    res.PayorList = objPayr;
                    return res;
                }
                else
                {
                    res = new ListResponse("Exception in returning list: Please check incoming request", Convert.ToInt16(210), "Invalid request!");
                    ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - invalid request: ", true);
                    return res;
                }
                //}
            }
            catch (Exception ex)
            {
                res = new ListResponse("Exception in returning list: " + ex.Message, Convert.ToInt16(210), "Exception getting data!");
                ActionLogger.Logger.WriteImportLog("GetPayorCarrierList - exception: " + ex.Message, true);
                return res;
            }
        }

        public JSONResponse ImportPolicyService(string strExcel, Guid LicenseeID/*, string uniqueKey*/)
        {
           
            JSONResponse jres = null;
            //Read header and return if not present
            try
            {
                if (WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"] != null)
                {
                    string val = Convert.ToString(WebOperationContext.Current.IncomingRequest.Headers["UniqueKey"]);
                    ActionLogger.Logger.WriteImportLog("Import policy - header key:  " + val, true);
                    if (val != "CommDept1973")
                    {
                        jres = new JSONResponse("Import process cannot be started as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                        ActionLogger.Logger.WriteImportLog("Import policy - Authentication failure ", true);
                        return jres;
                    }
                }
                else
                {
                    jres = new JSONResponse("Import process cannot be started as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                    ActionLogger.Logger.WriteImportLog("Import policy - Authentication failure ", true);
                    return jres;
                }
            }
            catch (Exception ex)
            {
                jres = new JSONResponse("Import process cannot be started as the incoming request is not valid", Convert.ToInt16(404), "Unauthorized request");
                ActionLogger.Logger.WriteImportLog("Import policy - Authentication failure , no key in header", true);
                return jres;
            }

            try
            {
                string inRequest = "Incoming table: " + strExcel + ", LicenseeID: " + LicenseeID;
                ActionLogger.Logger.WriteImportLog(inRequest, true);

                //if (!string.IsNullOrEmpty(uniqueKey) && uniqueKey.Trim() == "CommDept1973")
                {
                    List<CompType> lstComp = (new CompType()).GetAllComptype();
                    ObservableCollection<CompType> compList = new ObservableCollection<CompType>(lstComp);

                    DataTable tbExcel = (DataTable)Newtonsoft.Json.JsonConvert.DeserializeObject(strExcel, (typeof(DataTable)));
                    MailServerDetail.sendMailtodev("jyotisna@acmeminds.com", "Import process started by benefits at " + DateTime.Now.ToString(), inRequest);

                    Benefits_PolicyImportStatus status = Policy.ImportPolicy_Benefits(tbExcel, LicenseeID, compList);
                    
                    jres = new JSONResponse(string.Format("Import process execution completed"), Convert.ToInt16(200), "");
                    jres.ImportStatus = status;
                }
                //else
                //{
                //    jres = new JSONResponse(string.Format("Import process cannot be started, as unique identifier is missing or incorrect in the request"), Convert.ToInt16(210), "");
                //}
             
            }
            catch (Exception ex)
            {
                jres = new JSONResponse("Import process execution failed", Convert.ToInt16(210), ex.Message);
                ActionLogger.Logger.WriteImportLogDetail("AddUpdateClientService failure ", true);
            }
            return jres;
         
        }

    }
    [DataContract]
    public class JSONResponse
    {
        private string _message;
        private int _errorCode;
        private string _exceptionMessage;
        Benefits_PolicyImportStatus _importStatus;

        public JSONResponse()
        {
            //Empty parameter constructor;
        }
        public JSONResponse(string message, int errorCode, string exceptionMessage)
        {
            this.Message = message;
            this.ResponseCode = errorCode;
            this.ExceptionMessage = exceptionMessage;
            // this.SecondResult = secondResult;
        }
        [DataMember]
        public Benefits_PolicyImportStatus ImportStatus
        {
            get { return _importStatus; }
            set { _importStatus = value; }
        }
        [DataMember]
        public int ResponseCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        [DataMember]
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
            set { _exceptionMessage = value; }
        }
    }

    [DataContract]
    public class ListResponse
    {
        private string _message;
        private int _errorCode;
        private string _exceptionMessage;
        List<PayorObject> _listPayors;
        List<CarrierObject> _listCarriers;

        [DataMember]
        public int ResponseCode
        {
            get { return _errorCode; }
            set { _errorCode = value; }
        }

        [DataMember]
        public string Message
        {
            get { return _message; }
            set { _message = value; }
        }
        [DataMember]
        public string ExceptionMessage
        {
            get { return _exceptionMessage; }
            set { _exceptionMessage = value; }
        }

        [DataMember]
        public List<PayorObject> PayorList
        {
            get { return _listPayors; }
            set { _listPayors = value; }
        }

        [DataMember]
        public List<CarrierObject> CarrierList
        {
            get { return _listCarriers; }
            set { _listCarriers = value; }
        }


        public ListResponse()
        {
            //Empty parameter constructor;
        }
        public ListResponse(string message, int errorCode, string exceptionMessage)
        {
            this.Message = message;
            this.ResponseCode = errorCode;
            this.ExceptionMessage = exceptionMessage;
            // this.SecondResult = secondResult;
        }
    }


}