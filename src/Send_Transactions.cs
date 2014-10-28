using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Net;
using Common;
using System.Web.Services.Protocols;
using System.Web.Services;

namespace CertSuiteTool
{
    /* Generating the proxy using svcutil.exe
     * Location in Vista : C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin
     * Note:  the use of lists,  and merge switch below. 
     * Note: To contain all of the wsdl and xsd files I created a child folder "PWS".
     * svcutil.exe PWS_5.2.2\payments.wsdl /config:app.config /mergeConfig
     * 
     * Note : for VB.NET customers you should add the switch /language:VB to command lines above
     * 
    */
    public partial class Send_Transactions : Form
    {
        //Web Service Clients
        //PaymentWebServices.PaymentPortTypeClient PWSClient = new PaymentWebServices.PaymentPortTypeClient();
        PaymentPortTypeClient PWSClient = new PaymentPortTypeClient();

        ////Endpoint addresses
        //http://msdn.microsoft.com/en-us/library/77hkfhh8(VS.71).aspx //SOAP Headers
        //http://msdn.microsoft.com/en-us/library/ms819938.aspx
        //http://msdn.microsoft.com/en-us/library/vstudio/9z52by6a(v=vs.100).aspx
        //http://stackoverflow.com/questions/11263640/net-client-authentication-and-soap-credential-headers-for-a-cxf-web-service

        private string _PWSEndpointAddress = "https://ws-cert.vantiv.com/merchant/payments-cert/v6"; //Options "https://ws-cert.vantiv.com/merchant/payments-cert/v6", "https://ws-stage.infoftps.com:4443/merchant/payments-test/v6"
        private string _UserName = "s.MID5.PAY.WS.NP";
        private string _Password = "Tu2u2AHU";
        private string _ApiKey = "";//API key provided by Apigee when creating a new application.

        //The following are used to switch the URI for posting data
        private static object svcInfoChannelLock = new object();
        
        public Send_Transactions()
        {
            InitializeComponent();

            //Set Defaults
            CboPaymentInstrument.Text = "";
            CboTerminalDetail.Text = "";//set defult
            CboCreditType.Text = "";
            GrpKeyedData.Enabled = true;
            GrpTrackData.Enabled = false;

            //Setup Card Types CboCardTypes
            CboTransactionType.Sorted = true;
            CboTransactionType.DataSource = Enum.GetValues(typeof(TransactionTypeType));
            try { CboTransactionType.SelectedIndex = -1; }
            catch { }
            
            CboCardType.Sorted = true;
            CboCardType.DataSource = Enum.GetValues(typeof(CreditCardNetworkType));
            try { CboCardType.SelectedItem = CreditCardNetworkType.visa; }
            catch { }

            CboAccountType.Sorted = true;
            CboAccountType.DataSource = Enum.GetValues(typeof(AccountType));
            try { CboAccountType.SelectedItem = AccountType.CHECKING; }
            catch { }

            CboTrackChoice.Sorted = true;
            CboTrackChoice.DataSource = Enum.GetValues(typeof(ItemChoiceType));
            try { CboTrackChoice.SelectedItem = ItemChoiceType.Track2; }
            catch { }

            CboCancelType.Sorted = true;
            CboCancelType.DataSource = Enum.GetValues(typeof(CancelTransactionType));
            try { CboCancelType.SelectedIndex = -1; }
            catch { }

            CboReversalReason.Sorted = true;
            CboReversalReason.DataSource = Enum.GetValues(typeof(ReversalReasonType));
            try { CboReversalReason.SelectedIndex = -1; }
            catch { }

            CboPaymentType.Sorted = true;
            CboPaymentType.DataSource = Enum.GetValues(typeof(PaymentType));
            try { CboPaymentType.SelectedItem = PaymentType.single; }
            catch { }

            CboCurrencyCodeType.Sorted = true;
            CboCurrencyCodeType.DataSource = Enum.GetValues(typeof(ISO4217CurrencyCodeType));
            try { CboCurrencyCodeType.SelectedItem = ISO4217CurrencyCodeType.USD; }
            catch { }
            
            CboPartialApprovalCode.Sorted = true;
            CboPartialApprovalCode.DataSource = Enum.GetValues(typeof(PartialIndicatorType));
            try { CboPartialApprovalCode.SelectedIndex = -1; }
            catch { }

            CboCountryCode.Sorted = true;
            CboCountryCode.DataSource = Enum.GetValues(typeof(ISO3166CountryCodeType));
            try { CboCountryCode.SelectedItem = ISO3166CountryCodeType.US; }
            catch { }

            CboStateType.Sorted = true;
            CboStateType.DataSource = Enum.GetValues(typeof(StateCodeType));
            try { CboStateType.SelectedItem = StateCodeType.OH; }
            catch { }
            

            #region 
            //Format [MID] : [TID] : [Description]
            CboTestMerchantAccounts.Items.Add("4445000865113 : 002 : Tandem");
            CboTestMerchantAccounts.Items.Add("4445000868901 : 001 : RAFT (aka IBM)");
            CboTestMerchantAccounts.Items.Add("4445012495101 : 001 : RAFT (MID / TID returns a token)");
            #endregion Test Merchant Accounts

            CboSendTransaction.Items.Add(new item("Authorize", "Authorize"));
            CboSendTransaction.Items.Add(new item("Capture", "Capture"));
            CboSendTransaction.Items.Add(new item("Purchase", "Purchase"));
            CboSendTransaction.Items.Add(new item("Adjust", "Adjust"));
            CboSendTransaction.Items.Add(new item("Refund", "Refund"));
            CboSendTransaction.Items.Add(new item("Cancel", "Cancel"));
            CboSendTransaction.Items.Add(new item("Close Batch", "CloseBatch"));
            CboSendTransaction.Items.Add(new item("Tokenize", "Tokenize"));
            CboSendTransaction.Items.Add(new item("Activate", "Activate"));
            CboSendTransaction.Items.Add(new item("Unload", "Unload"));
            CboSendTransaction.Items.Add(new item("Reload", "Reload"));
            CboSendTransaction.Items.Add(new item("Close", "Close"));
            CboSendTransaction.Items.Add(new item("Balance Inquiry", "BalanceInquiry"));
            CboSendTransaction.Items.Add(new item("Batch Balance", "BatchBalance"));
            CboSendTransaction.Items.Add(new item("Update Card", "UpdateCard"));

            #region setup endpoints

            /* Generating the proxy using svcutil.exe
             * Location in Vista : C:\Program Files\Microsoft SDKs\Windows\v6.0A\bin
             * Note:  the use of lists,  and merge switch below. 
             * Note: To contain all of the wsdl and xsd files I created a child folder "CWSSOAP".
             * svcutil.exe PWS\payments.wsdl /config:app.config /mergeConfig
            */

            //Setup Endpoint addresses 
            lock (svcInfoChannelLock)
            {
                PWSClient.Endpoint.Address = new EndpointAddress(_PWSEndpointAddress);
                PWSClient.ClientCredentials.UserName.UserName = _UserName;
                PWSClient.ClientCredentials.UserName.Password = _Password;
                PWSClient.Open();
            }

            //Bindings
            //Info about Custom bindings for app.config : http://zianet.dk/blog/2010/12/20/getting-wcf-to-talk-to-a-java-axis-1-x-and-wss4j-web-service-part-3-of-3/
                        
            #endregion setup endpoints

            DisableFields();
        }

        #region Form Events

        private void CmdSendTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                string test = ((item)(CboSendTransaction.SelectedItem)).Value;

                if (test == "Authorize")
                    Authorize();
                if (test == "Capture")
                {
                    if (ChkLstTransactionsProcessed.CheckedItems.Count < 1) { MessageBox.Show("Please select 'Authorize' transaction(s) to process"); return; }
                    //First verify if all transactions selected are "Authorize" transactions
                    List<ResponseDetails> txnsToProcess = new List<ResponseDetails>();
                    foreach (object itemChecked in ChkLstTransactionsProcessed.CheckedItems)
                    {
                        //((TransactionResponseType)(_response))
                        if (((ResponseDetails)(itemChecked)).Response.GetType().ToString() != "AuthorizeResponse")
                        {
                            MessageBox.Show("All selected messages must be of type Authorize");
                            Cursor = Cursors.Default;
                            return;
                        }
                        txnsToProcess.Add(((ResponseDetails)(itemChecked)));
                    }
                    //Now process each Authorize message selected
                    foreach (ResponseDetails _RD in txnsToProcess)
                    {
                        Capture(_RD);
                    }
                }
                if (test == "Purchase")
                    Purchase();
                if (test == "Adjust")
                {
                    if (ChkLstTransactionsProcessed.CheckedItems.Count < 1) { MessageBox.Show("Please select 'Purchase' transaction(s) to process"); return; }
                    //First verify if all transactions selected are "Authorize" transactions
                    List<ResponseDetails> txnsToProcess = new List<ResponseDetails>();
                    foreach (object itemChecked in ChkLstTransactionsProcessed.CheckedItems)
                    {
                        //((TransactionResponseType)(_response))
                        if (((ResponseDetails)(itemChecked)).Response.GetType().ToString() != "PurchaseResponse")
                        {
                            MessageBox.Show("All selected messages must be of type Purchase");
                            Cursor = Cursors.Default;
                            return;
                        }
                        txnsToProcess.Add(((ResponseDetails)(itemChecked)));
                    }
                    //Now process each Authorize message selected
                    foreach (ResponseDetails _RD in txnsToProcess)
                    {
                        Adjust(_RD);
                    }
                }
                if (test == "Refund")
                {
                    if (ChkLstTransactionsProcessed.CheckedItems.Count < 1) { MessageBox.Show("Please select 'Authorize' transaction(s) to process"); return; }
                    //First verify if all transactions selected are "Authorize" transactions
                    List<ResponseDetails> txnsToProcess = new List<ResponseDetails>();
                    foreach (object itemChecked in ChkLstTransactionsProcessed.CheckedItems)
                    {
                        //((TransactionResponseType)(_response))
                        if (((ResponseDetails)(itemChecked)).Response.GetType().ToString() != "AuthorizeResponse")
                        {
                            MessageBox.Show("All selected messages must be of type Authorize");
                            Cursor = Cursors.Default;
                            return;
                        }
                        txnsToProcess.Add(((ResponseDetails)(itemChecked)));
                    }
                    //Now process each Authorize message selected
                    foreach (ResponseDetails _RD in txnsToProcess)
                    {
                        Refund(_RD);
                    }
                }
                if (test == "Cancel")
                {
                    //First verify if all transactions selected are "Authorize" transactions
                    List<ResponseDetails> txnsToProcess = new List<ResponseDetails>();
                    foreach (object itemChecked in ChkLstTransactionsProcessed.CheckedItems)
                    {
                        if (ChkLstTransactionsProcessed.CheckedItems.Count < 1) { MessageBox.Show("Please select 'Authorize' transaction(s) to process"); return; }
                        //((TransactionResponseType)(_response))
                        if (((ResponseDetails)(itemChecked)).Response.GetType().ToString() != "AuthorizeResponse")
                        {
                            MessageBox.Show("All selected messages must be of type Authorize");
                            Cursor = Cursors.Default;
                            return;
                        }
                        txnsToProcess.Add(((ResponseDetails)(itemChecked)));
                    }
                    //Now process each Authorize message selected
                    foreach (ResponseDetails _RD in txnsToProcess)
                    {
                        Cancel(_RD);
                    }
                }
                if (test == "CloseBatch")
                    CloseBatch();
                if (test == "Tokenize")
                    Tokenize();
                if (test == "Activate")
                    Activate();
                if (test == "Unload")
                    Unload();
                if (test == "Reload")
                    Reload();
                if (test == "Close")
                    Close();
                if (test == "BalanceInquiry")
                    BalanceInquiry();
                if (test == "BatchBalance")
                    BatchBalance();
                if (test == "UpdateCard")
                    UpdateCard();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void CmdClearTransactions_Click(object sender, EventArgs e)
        {
            ChkLstTransactionsProcessed.Items.Clear();
        }

        private void CboTransactionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try 
            {
                DisableFields();
                if (CboTransactionType.Text == "ecommerce" | CboTransactionType.Text == "moto")
                {
                    CboPaymentInstrument.Enabled = true;
                    CboTerminalDetail.Enabled = true;
                    CboCreditType.Enabled = true;
                    GrpKeyedData.Enabled = true;
                    CboCreditType.Text = "CardKeyed";
                    CboPartialApprovalCode.SelectedItem = PartialIndicatorType.not_supported;
                }
                else if (CboTransactionType.Text == "present")
                {
                    CboPaymentInstrument.Enabled = true;
                    CboTerminalDetail.Enabled = true;
                    CboCreditType.Enabled = true;
                    GrpTrackData.Enabled = true;
                    CboCreditType.Text = "CardSwiped";
                    CboPartialApprovalCode.SelectedItem = PartialIndicatorType.supported;
                }
            }
            catch { }
        }

        private void CboPaymentInstrument_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                GrpCredit.Enabled = false;
                GrpDebit.Enabled = false;
                GrpGift.Enabled = false;

                if (CboPaymentInstrument.Text == "Credit")
                {
                    GrpCredit.Enabled = true;
                }
                else if (CboPaymentInstrument.Text == "Debit")
                {
                    GrpDebit.Enabled = true;
                }
                else if (CboPaymentInstrument.Text == "Gift")
                {
                    GrpGift.Enabled = true;
                }
            }
            catch { }
        }

        private void ChkEncryptedData_CheckedChanged(object sender, EventArgs e)
        {
            if (ChkEncryptedData.Checked)
                TxtKeySerialNumber.Enabled = true;
            else
                TxtKeySerialNumber.Enabled = false;
        }

        private void CboCreditType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CboCreditType.Text == "CardKeyed")
            {
                GrpKeyedData.Enabled = true;
                GrpTrackData.Enabled = false;
            }
            if (CboCreditType.Text == "CardSwiped")
            {
                GrpKeyedData.Enabled = false;
                GrpTrackData.Enabled = true;
            }


        }

        private void CboTestMerchantAccounts_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {//[MID] : [TID] : [Description]
                string[] merchantAccounts;
                string[] delimiterChars = { ":" };
                merchantAccounts = CboTestMerchantAccounts.Text.Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
                if (merchantAccounts.Count() == 3)
                {
                    TxtMID.Text = merchantAccounts[0].Trim();
                    TxtTID.Text = merchantAccounts[1].Trim();
                }
            }
            catch { }
        }

        private void CboSendTransaction_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CboSendTransaction.Text == "Cancel")
                GrpCancel.Visible = true;
            else
                GrpCancel.Visible = false;
        }

        private void ChkLstTransactionsProcessed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ChkOnClickDisplayTxnMessage.Checked)
                return;

            ResponseDetails rd = ((ResponseDetails)(ChkLstTransactionsProcessed.SelectedItem));
            MessageBox.Show(ExtractDetailsFromResponse(rd.Response));
        }

        private void TxtPrimaryAccountNumber_TextChanged(object sender, EventArgs e)
        {
            try
            {
                CardTypeLookup(TxtPrimaryAccountNumber.Text);
            }
            catch { }
        }

        #endregion Form Events

        #region Page Methods

        private void DisableFields()
        {
            //Dropdowns
            //CboTransactionType.Enabled = false;//This drives logic for all other fields
            CboPaymentInstrument.Enabled = false;
            CboTerminalDetail.Enabled = false;
            CboCreditType.Enabled = false;
            //CboTrackChoice.Enabled = false;
            //CboCardType.Enabled = false;
            //CboAccountType.Enabled = false;            
            //Text boxes
            //TxtPrimaryAccountNumber.Enabled = false;
            //TxtExpirationDate.Enabled = false;
            //TxtCardholderName.Enabled = false;
            //TxtCardSecurityCode.Enabled = false;
            //TxtTrackData.Enabled = false;
            //TxtKeySerialNumber.Enabled = false;
            //TxtGiftCardPin.Enabled = false;
            //Check boxes
            //ChkEncryptedData.Enabled = false;
            //Group boxes
            GrpKeyedData.Enabled = false;
            GrpTrackData.Enabled = false;
            GrpCredit.Enabled = false;
            GrpDebit.Enabled = false;
            GrpGift.Enabled = false;
        }

        private string ExtractDetailsFromResponse(Object _response)
        {
            string transactionInformation = "";
            TransactionResponseType trt = (TransactionResponseType)_response;
            try
            {
                if (trt.AddressVerificationResult != null)
                    transactionInformation += "AVS Verification Result\r\n -Code: " + trt.AddressVerificationResult.Code + " Type: " + trt.AddressVerificationResult.Type + "\r\n";

                if (trt.Balance != null)
                    transactionInformation += "Balance\r\n " 
                        + " - Authorized: " 
                        + trt.Balance.Authorized.Value + " " 
                        + trt.Balance.Authorized.currency + "\r\n"
                        + " - Available Balance: " 
                        + trt.Balance.AvailableBalance.Value + ""
                        + trt.Balance.AvailableBalance.currency + "\r\n"
                        + " - Beginning Balance: "
                        + trt.Balance.BeginningBalance.Value + ""
                        + trt.Balance.BeginningBalance.currency + "\r\n"
                        + " - Cash: "
                        + trt.Balance.Cash.Value + ""
                        + trt.Balance.Cash.currency + "\r\n"
                        + " - Ending Balance: "
                        + trt.Balance.EndingBalance.Value + ""
                        + trt.Balance.EndingBalance.currency + "\r\n"
                        + " - Pre Authorized: "
                        + trt.Balance.PreAuthorized.Value + ""
                        + trt.Balance.PreAuthorized.currency + "\r\n"
                        ;
                if (trt.BatchNumber != null)
                    transactionInformation += "BatchNumber: " + trt.BatchNumber + "\r\n";
                if(trt.CardCategory != null)
                    transactionInformation += "CardCategory: " + trt.CardCategory + "\r\n";
                if (trt.CardSecurityCodeResult != null)
                    transactionInformation += "Card Security Code Result\r\n -Code: " + trt.CardSecurityCodeResult.Code + " Type: " + trt.CardSecurityCodeResult.Type + "\r\n";
                if (trt.DemoMode != null)
                    transactionInformation += "Demo Mode: " + trt.DemoMode + "\r\n";
                if (trt.Items != null && trt.Items.Count() > 0)
                {
                    transactionInformation += "Items\r\n";
                    if (((TransactionResponseType)(_response)).Items.Count() > 0)
                    {
                        int idx = 0;
                        while (idx < ((TransactionResponseType)(_response)).Items.Count())
                        {
                            transactionInformation += " -" + trt.ItemsElementName[idx] + ": " + trt.Items[idx] + "\r\n";
                            idx++;
                        }
                    }
                }
                if (trt.JulianDay != null)
                    transactionInformation += "Julian Day: " + trt.JulianDay + "\r\n";
                if (trt.keyValuePair != null && trt.keyValuePair.Count() > 0)
                {
                    transactionInformation += "Key Value Pair\r\n";
                    foreach (KeyValuePair kvp in trt.keyValuePair)
                    {
                        transactionInformation += " -Key: " + kvp.key + " Value: " + kvp.Value; 
                    }
                }
                if (trt.merchantrefid != null)
                    transactionInformation += "merchantrefid: " + trt.merchantrefid + "\r\n";
                if (trt.NetworkResponseCode != null)
                    transactionInformation += "Network Response Code: " + trt.NetworkResponseCode + "\r\n";
                if (trt.PaymentServiceResults.Item != null)
                {
                    if (trt.PaymentServiceResults.Item.ToString() == "VisaResultsType")
                    { 
                        VisaResultsType vrt = (VisaResultsType)(trt.PaymentServiceResults.Item);
                        transactionInformation += "VisaResultsType\r\n";
                        transactionInformation += " -AuthorizationCharacteristicsIndicator: " + vrt.AuthorizationCharacteristicsIndicator + "\r\n";
                        transactionInformation += " -CardLevelResultsCode: " + vrt.CardLevelResultsCode + "\r\n";
                        transactionInformation += " -CAVVCode: " + vrt.CAVVCode + "\r\n";
                        transactionInformation += " -Ps2000Qualification: " + vrt.Ps2000Qualification + "\r\n";
                        transactionInformation += " -RequestedPaymentServices: " + vrt.RequestedPaymentServices + "\r\n";
                        transactionInformation += " -TransactionId: " + vrt.TransactionId + "\r\n";
                        transactionInformation += " -ValidationCode: " + vrt.ValidationCode + "\r\n";
                        transactionInformation += " -VisaMultipleClearingSequenceCount: " + vrt.VisaMultipleClearingSequenceCount + "\r\n";
                        transactionInformation += " -VisaMultipleClearingSequenceNumber: " + vrt.VisaMultipleClearingSequenceNumber + "\r\n";
                    }
                    if (trt.PaymentServiceResults.Item.ToString() == "MasterCardResultsType")
                    {
                        MasterCardResultsType mcrt = (MasterCardResultsType)(trt.PaymentServiceResults.Item);
                        transactionInformation += "MasterCardResultsType\r\n";
                        transactionInformation += " -AuthorizationCharacteristicsIndicator: " + mcrt.AuthorizationCharacteristicsIndicator + "\r\n";
                        transactionInformation += " -BanknetDate: " + mcrt.BanknetDate + "\r\n";
                        transactionInformation += " -BanknetErrorCode: " + mcrt.BanknetErrorCode + "\r\n";
                        transactionInformation += " -BanknetReference: " + mcrt.BanknetReference + "\r\n";
                        transactionInformation += " -Category: " + mcrt.Category + "\r\n";
                        transactionInformation += " -CvcValidity: " + mcrt.CvcValidity + "\r\n";
                        transactionInformation += " -TransactionId: " + mcrt.TransactionId + "\r\n";
                        transactionInformation += " -ValidationCode: " + mcrt.ValidationCode + "\r\n";
                    }
                    if (trt.PaymentServiceResults.Item.ToString() == "AmericanExpressResultsType")
                    {
                        AmericanExpressResultsType aert = (AmericanExpressResultsType)(trt.PaymentServiceResults.Item);
                        transactionInformation += "AmericanExpressResultsType\r\n";
                        transactionInformation += " -AmexTransactionId: " + aert.AmexTransactionId + "\r\n";
                        transactionInformation += " -AuthorizationCharacteristicsIndicator: " + aert.AuthorizationCharacteristicsIndicator + "\r\n";
                        if (aert.POSDataCodes.Count() > 0)
                        {
                            transactionInformation += " -POSDataCodes: ";
                            foreach (char c in aert.POSDataCodes)
                            {
                                transactionInformation += c.ToString() + " | ";
                            }
                            transactionInformation += "\r\n";
                        }
                        transactionInformation += " -TransactionId: " + aert.TransactionId + "\r\n";
                        transactionInformation += " -ValidationCode: " + aert.ValidationCode + "\r\n";
                    }
                    if (trt.PaymentServiceResults.Item.ToString() == "DiscoverCardResultsType")
                    {
                        DiscoverCardResultsType dcrt = (DiscoverCardResultsType)(trt.PaymentServiceResults.Item);
                        transactionInformation += "DiscoverCardResultsType\r\n";
                        transactionInformation += " -AuthorizationCharacteristicsIndicator: " + dcrt.AuthorizationCharacteristicsIndicator + "\r\n";
                        transactionInformation += " -NetworkReferenceId: " + dcrt.NetworkReferenceId + "\r\n";
                        transactionInformation += " -PINCapability: " + dcrt.PINCapability + "\r\n";
                        transactionInformation += " -POSData: " + dcrt.POSData + "\r\n";
                        transactionInformation += " -POSEntryMode: " + dcrt.POSEntryMode + "\r\n";
                        transactionInformation += " -ProcessingCode: " + dcrt.ProcessingCode + "\r\n";
                        transactionInformation += " -ResponseCode: " + dcrt.ResponseCode + "\r\n";
                        transactionInformation += " -SystemTraceAuditNumber: " + dcrt.SystemTraceAuditNumber + "\r\n";
                        transactionInformation += " -TrackIIStatus: " + dcrt.TrackIIStatus + "\r\n";
                        transactionInformation += " -TransactionId: " + dcrt.TransactionId + "\r\n";
                        transactionInformation += " -ValidationCode: " + dcrt.ValidationCode + "\r\n";
                    }
                }
                if (trt.ReferenceNumber != null)
                    transactionInformation += "Reference Number: " + trt.ReferenceNumber + "\r\n";
                if (trt.reportgroup != null)
                    transactionInformation += "reportgroup: " + trt.reportgroup + "\r\n";
                if (trt.RequestId != null)
                    transactionInformation += "RequestId: " + trt.RequestId + "\r\n";
                if (trt.systemtraceid != null)
                    transactionInformation += "systemtraceid: " + trt.systemtraceid + "\r\n";
                if (trt.TokenizationResult != null)
                {
                    transactionInformation += "Tokenization Result\r\n";
                    transactionInformation += " -successful: " + trt.TokenizationResult.successful 
                                                + " tokenId: " + trt.TokenizationResult.tokenType.tokenId
                                                + " tokenValue: " + trt.TokenizationResult.tokenType.tokenValue;
                }
                if (trt.TransactionStatus != null)
                    transactionInformation += "Transaction Status: " + trt.TransactionStatus + "\r\n";
                if (trt.TransactionTimestamp != null)
                    transactionInformation += "Transaction Timestamp: " + trt.TransactionTimestamp + "\r\n";
                if (trt.TransmissionTimestamp != null)
                    transactionInformation += "Transmission Timestamp (UTC): " + trt.TransmissionTimestamp + "\r\n";
                if (trt.WorkingKey != null)
                    transactionInformation += "Working Key: " + trt.WorkingKey + "\r\n";
                
            }
            catch
            {}

            return transactionInformation;
            
        }

        public void CardTypeLookup(string strPAN)
        {
            if (Convert.ToInt16(strPAN.Substring(0, 1)) == 4)
            {
                CboCardType.SelectedItem = CreditCardNetworkType.visa;
            }
            else if (Convert.ToInt16(strPAN.Substring(0, 1)) == 5)
            {
                CboCardType.SelectedItem = CreditCardNetworkType.masterCard;
            }
            else if (Convert.ToInt16(strPAN.Substring(0, 2)) == 34 | Convert.ToInt16(strPAN.Substring(0, 2)) == 37)
            {
                CboCardType.SelectedItem = CreditCardNetworkType.amex;
            }
            else if (Convert.ToInt16(strPAN.Substring(0, 1)) == 36)
            {
                CboCardType.SelectedItem = CreditCardNetworkType.masterCard;//MC reissued Diners
            }
            else if (Convert.ToInt16(strPAN.Substring(0, 2)) == 30 | Convert.ToInt16(strPAN.Substring(0, 2)) == 38)
            {
                CboCardType.SelectedItem = CreditCardNetworkType.masterCard;//MC/Diners co-branded
            }
            else if (Convert.ToInt16(strPAN.Substring(0, 4)) == 6011)
            {
                CboCardType.SelectedItem = CreditCardNetworkType.discover;
            }
            else if (Convert.ToInt16(strPAN.Substring(0, 3)) > 644 & Convert.ToInt16(strPAN.Substring(0, 3)) < 659)
            {
                CboCardType.SelectedItem = CreditCardNetworkType.discover;
            }
            else
            {
                CboCardType.SelectedIndex = -1; //No match was found so clear out the card type
            }
        }

        #endregion Page Methods

        #region PWS Objects

        private MerchantType merchantType()
        {
            /*
             * As mentioned above, there are 3 sections to a request. This section describes the Merchant details section, its optional 
             * and mandatory values, and selectable values. Merchant Detail has 2 sections, the merchant detail and terminal detail. Terminal 
             * detail has 3 options: Software, Mobile, and Terminal. The differences and details will be discussed later in this section. 
             * See example 3 for a sample merchant detail.  The merchant detail information will also be delivered as part of the merchant 
             * boarding process.  The merchant or developer will receive a VAR sheet, which will contain all the necessary merchant credentials 
             * (chain, merchant ID, terminal ID, etc) necessary to configure the merchant details section of the header.
             */
            MerchantType mt = new MerchantType();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                //Merchant Detail Section
                mt.CashierNumber = Convert.ToInt32(TxtCashierNumber.Text); //8 digit numeric value to represent the cashier entering the transaction	Optional
                if(TxtCashierNumber.Text.Length > 0)
                    mt.CashierNumberSpecified = true;
                mt.ChainCode = TxtChainCode.Text; //NOTE: *DB* I believe documentation is incorrect (ChainNumber) : 5 character alphanumeric value to represent the company’s chain where the transaction was entered. If provided on the VAR sheet the value provided should be used	Conditional, use if provided
                mt.ClerkNumber = Convert.ToInt32(TxtClerkNumber.Text);//3 digit value to represent the clerk entering the transaction	Mandatory
                if (TxtClerkNumber.Text.Length > 0) 
                    mt.ClerkNumberSpecified = true;
                mt.DivisionNumber = TxtDivisionNumber.Text; //3 character alphanumeric value to represent the company’s division where the transaction was entered. The default value is “001”	Optional,use if Division level reporting is required 
                mt.LaneNumber = TxtLaneNumber.Text; //3 character alpha numeric value to represent which lane that the transaction was entered.  Used for multi threading of transactions.	Conditional, should be 2 digit number, 0-99.  Used for multi threading transactions in host capture environment
                mt.MerchantId = TxtMID.Text;//"4445000865113"; //Identifying ID set up during the boarding process. It can be up to 36 digits.  Also known as MID or merchant account.	Mandatory
                mt.MerchantName = TxtMerchantName.Text; //15 character string value used to send the name of the bill payment acquiring merchant in order for customer to see the merchant name in debit statement.	Conditional-used for PIN-less Debit transactions
                mt.NetworkRouting = TxtNetworkRouting.Text; //2 character value used to send the transaction to the proper credit network	Mandatory
                mt.StoreNumber = TxtStoreNumber.Text; //8 character alphanumeric value to represent the company’s store number where the transaction was entered. The default value is “00000001”	Optional
                
                //Terminal Detail Section (3 options)
                if (CboTerminalDetail.Text == "Terminal")//Options "Terminal", "Software", "Mobile"
                    mt.Terminal = terminalField();
                else if (CboTerminalDetail.Text == "Software")
                    mt.Software = softwareField();
                else if (CboTerminalDetail.Text == "Mobile")
                    mt.Mobile = mobileDeviceType();
               
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }
            
            return mt;
        }

        //Set one of the following
        #region Terminal Detail
        //Terminal Section
        /*
         * The terminal section sends the device details to Vantiv, and has a choice of three options: Software, Mobile, or 
         * Terminal. All 3 options have the same basic values, but some have additional optional values. 
         * Choose Software if the request is coming from an application, either installed payment application or thin client. 
         * Choose Mobile if the request is coming from a smart phone or tablet device. 
         * Choose Terminal if the request is coming from a terminal device. All terminal details use the following values:
         */
        private PaymentDeviceType terminalField()
        {
            PaymentDeviceType pdt = new PaymentDeviceType();

            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                pdt.BalanceInquiry = false; //Boolean value (true, false) to determine if balance inquiry fields will be returned. The default value is false.	Optional – Must be set to true for card present transactions so that prepaid cards can be supported.  We recommend leaving as false for card not present transactions.
                pdt.CardReader = CardReaderType.magstripe; //An optional value to identify the type of card reader used in the transaction. Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated may cause interchange qualification impact.
                pdt.Classification = TerminalClassificationType.unspecified; //Identifies the type of device used in the transaction. Should be changed from the default values to reflect true entry method and point of entry environment.  Not updating from the default may have interchange qualification impact.  Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
                pdt.EntryMode = EntryModeType.manual; //Required value to identify the type of terminal entry used in the transaction. Your choices are: Mandatory – Value must reflect entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
                pdt.HostAdjustment = true; //Boolean value (true, false) to determine if adjust transaction will be allowed for a batch. The default value is false.	Optional – if need to adjust a transaction amount (ex. Add tip, add level 2 data) set to true.  Flag setting should be consistent across all transaction types for a given application. Note – Setting HostAdjustment to true will disable batch auto close functionality.  The host will not auto close a batch if HostAdjust is set to true.
                pdt.IPv4Address = "192.0.2.235"; //The IPv4Address of the calling transaction. The format is 4 sets of up to 3 digits separated by “.”. An example would be “127.0.0.0”. Either IPv4Address or IPv6Address may be entered.	Optional
                //pdt.IPv6Address = ""; //The IPv6Adresss of t he calling transaction. It consists of eight groups of four hexadecimal digits separated by colons, for example 2001:0db8:85a3:0042:0000:8a2e:0370:7334. Either IPv4Address or IPv6Address may be entered.	Optional
                pdt.PinEntry = PinEntryType.none; //An optional value to identify the type of pin entry used by this terminal. Optional – Defaults must be changed to reflect actual entry method and point of entry environment. Mandatory for PIN Debit Transactions, should always be set to “supported” 
                pdt.SequenceNumber = "123"; //6 digit value to identify this transaction. The default is 0, but if used, it should be unique for each transaction within a 24 hour period. We recommend using a counter or hour, minute, second (hhmmss), using military time.	Mandatory, also called STAN (System Audit Trace Number)
                pdt.TerminalID = Convert.ToInt16(TxtTID.Text); //3 digit value that identifies the terminal used. The terminal ID will be provided on the VAR sheet.  Also known as the TID	Mandatory
            
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            } 
            
            return pdt;
        }
        //or
        private PaymentDeviceType softwareField()
        {
            PaymentDeviceType pdt = new PaymentDeviceType();

            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                pdt.BalanceInquiry = false; //Boolean value (true, false) to determine if balance inquiry fields will be returned. The default value is false.	Optional – Must be set to true for card present transactions so that prepaid cards can be supported.  We recommend leaving as false for card not present transactions.
                pdt.CardReader = CardReaderType.magstripe; //An optional value to identify the type of card reader used in the transaction. Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated may cause interchange qualification impact.
                pdt.Classification = TerminalClassificationType.electronic_cash_register; //Identifies the type of device used in the transaction. Should be changed from the default values to reflect true entry method and point of entry environment.  Not updating from the default may have interchange qualification impact.  Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
                pdt.EntryMode = EntryModeType.manual; //Required value to identify the type of terminal entry used in the transaction. Your choices are: Mandatory – Value must reflect entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
                //pdt.HostAdjustment = true; //Boolean value (true, false) to determine if adjust transaction will be allowed for a batch. The default value is false.	Optional – if need to adjust a transaction amount (ex. Add tip, add level 2 data) set to true.  Flag setting should be consistent across all transaction types for a given application. Note – Setting HostAdjustment to true will disable batch auto close functionality.  The host will not auto close a batch if HostAdjust is set to true.
                pdt.IPv4Address = "192.0.2.235";
                //pdt.IPv6Address = ""; //The IPv6Adresss of t he calling transaction. It consists of eight groups of four hexadecimal digits separated by colons, for example 2001:0db8:85a3:0042:0000:8a2e:0370:7334. Either IPv4Address or IPv6Address may be entered.	Optional
                pdt.PinEntry = PinEntryType.unknown;
                pdt.SequenceNumber = "123456";
                pdt.TerminalID = Convert.ToInt16(TxtTID.Text);
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return pdt;
        }
        //or
        private MobileDeviceType mobileDeviceType()
        {
            MobileDeviceType mdt = new MobileDeviceType();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                //Similar to Terminal
                mdt.BalanceInquiry = false; //Boolean value (true, false) to determine if balance inquiry fields will be returned. The default value is false.	Optional – Must be set to true for card present transactions so that prepaid cards can be supported.  We recommend leaving as false for card not present transactions.
                mdt.CardReader = CardReaderType.unknown; //An optional value to identify the type of card reader used in the transaction. Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated may cause interchange qualification impact.
                mdt.Classification = TerminalClassificationType.unspecified; //Identifies the type of device used in the transaction. Should be changed from the default values to reflect true entry method and point of entry environment.  Not updating from the default may have interchange qualification impact.  Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
                mdt.EntryMode = EntryModeType.unknown; //Required value to identify the type of terminal entry used in the transaction. Your choices are: Mandatory – Value must reflect entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
                mdt.HostAdjustment = true; //Boolean value (true, false) to determine if adjust transaction will be allowed for a batch. The default value is false.	Optional – if need to adjust a transaction amount (ex. Add tip, add level 2 data) set to true.  Flag setting should be consistent across all transaction types for a given application. Note – Setting HostAdjustment to true will disable batch auto close functionality.  The host will not auto close a batch if HostAdjust is set to true.
                mdt.IPv4Address = "192.0.2.235"; //The IPv4Address of the calling transaction. The format is 4 sets of up to 3 digits separated by “.”. An example would be “127.0.0.0”. Either IPv4Address or IPv6Address may be entered.	Optional
                //mdt.IPv6Address = ""; //The IPv6Adresss of t he calling transaction. It consists of eight groups of four hexadecimal digits separated by colons, for example 2001:0db8:85a3:0042:0000:8a2e:0370:7334. Either IPv4Address or IPv6Address may be entered.	Optional
                mdt.Location = new GeolocationType();
                mdt.Location.Longitude = 39.5425970M;
                mdt.Location.Latitude = -104.8592690M;
                mdt.PinEntry = PinEntryType.unknown; //An optional value to identify the type of pin entry used by this terminal. Optional – Defaults must be changed to reflect actual entry method and point of entry environment. Mandatory for PIN Debit Transactions, should always be set to “supported” 
                mdt.SequenceNumber = "123"; //6 digit value to identify this transaction. The default is 0, but if used, it should be unique for each transaction within a 24 hour period. We recommend using a counter or hour, minute, second (hhmmss), using military time.	Mandatory, also called STAN (System Audit Trace Number)
                mdt.TerminalID = Convert.ToInt16(TxtTID.Text); //3 digit value that identifies the terminal used. The terminal ID will be provided on the VAR sheet.  Also known as the TID	Mandatory
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }
            
            return mdt;
        }
        #endregion Terminal Detail

        //Payment Instruments Credit, Debit, Gift
        #region Payment Instruments

        private CreditInstrumentType creditInstrumentType()
        {
            CreditInstrumentType cit = new CreditInstrumentType();
            cit.CardholderAddress = addressType();
            
            if(CboCreditType.Text == "CardKeyed")
            {
                cit.CardKeyed = creditOrDebitCardKeyedType();
            }
            else if (CboCreditType.Text == "CardSwiped")
            {
                cit.CardSwiped = cardSwipedType();                
            }

            cit.CardType = (CreditCardNetworkType)CboCardType.SelectedItem;
            cit.PartialApprovalCode = (PartialIndicatorType)CboPartialApprovalCode.SelectedItem;//Definition: Type of partial approval to return. If not_supported is selected, partial approvals will fail. Partial approvals must be supported by any application conducting card present transactions, per Visa and MasterCard regulations. We recommend not supporting partial approvals in applications conducted in Card Not Present environments, as split tender scenarios become complicated.
            cit.PartialApprovalCodeSpecified = true;

            return cit;
        }

        private DebitInstrumentType debitInstrumentType()
        {
            DebitInstrumentType dit = new DebitInstrumentType();

            dit.AccountType = (AccountType)CboAccountType.SelectedItem;//Optional for PIN debit balance inquiry action. If left out, the financial institutions default will be populated.
            dit.AccountTypeSpecified = true;
            dit.BenefitTransactionNumber = "1234";
            dit.CardholderAddress = addressType();

            if (CboCreditType.Text == "CardKeyed")
            {
                dit.CardKeyed = creditOrDebitCardKeyedType();
            }
            else if (CboCreditType.Text == "CardSwiped")
            {
                dit.CardSwiped = cardSwipedType();
            }

            dit.PartialApprovalCode = (PartialIndicatorType)CboPartialApprovalCode.SelectedItem;//Definition: Type of partial approval to return. If not_supported is selected, partial approvals will fail. Partial approvals must be supported by any application conducting card present transactions, per Visa and MasterCard regulations. We recommend not supporting partial approvals in applications conducted in Card Not Present environments, as split tender scenarios become complicated.
            dit.PartialApprovalCodeSpecified = true;
            dit.PinData = new EncryptedData();//Mandatory for PIN Debit Transactions
            dit.PinData.encryptiontype = EncryptionType.DUKPT;//DUKPT (default), Voltage
            dit.PinData.key = "";//The key used to encrypt the PIN data. This field is not sent as part of the request if the encryption-type is Voltage. Voltage Encryption Transfer data (Encrypted Symmetric Key). This field is not sent as part of the request if the encryption-type is DUKPT.
            dit.PinData.Value = "";
            dit.VoucherNumber = "";

            return dit;
        }

        private GiftInstrumentType giftInstrumentType()
        {
            GiftInstrumentType git = new GiftInstrumentType();

            if (CboCreditType.Text == "CardKeyed")
            {
                git.CardKeyed = new GiftCardKeyedType();
                git.CardKeyed.EncryptedPrimaryAccountNumber = new EncryptedData();
                //git.CardKeyed.EncryptedPrimaryAccountNumber.encryptiontype = EncryptionType.DUKPT;
                //git.CardKeyed.EncryptedPrimaryAccountNumber.key = "";
                //git.CardKeyed.EncryptedPrimaryAccountNumber.Value = "";
                git.CardKeyed.ExpirationDate = TxtExpirationDate.Text;
                git.CardKeyed.GiftCardPin = TxtGiftCardPin.Text;
                git.CardKeyed.PrimaryAccountNumber = TxtPrimaryAccountNumber.Text;
                git.CardKeyed.CardSecurityCode = TxtCardSecurityCode.Text;
                //git.CardKeyed.Token = new TokenType();
                //git.CardKeyed.Token.tokenId = "";
                //git.CardKeyed.Token.tokenValue = "";
                
            }
            else if (CboCreditType.Text == "CardSwiped")
            {
                git.CardSwiped = new GiftCardSwipedType();
                git.CardSwiped.GiftCardPin = TxtGiftCardPin.Text;
                //git.CardSwiped.Item
                //git.CardSwiped.ItemElementName = "";
                git.CardSwiped.CardSecurityCode = TxtCardSecurityCode.Text;
            }
            
            return git;
        }

        private VirtualGiftInstrumentType virtualGiftInstrumentType()
        {
            VirtualGiftInstrumentType vgt = new VirtualGiftInstrumentType();
            vgt.PrimaryAccountNumberLength = 2;
            vgt.VirtualGiftCardBIN = "";
            return vgt;
        }
 
        #region objects used by instrument types
        
        private AddressType addressType()
        {
            AddressType at = new AddressType();//Used for Address Verification service to verify the owner of the card to reduce the risk of fraudulent transactions (for Credit and Debit Cards)
            if (ChkSetAddressInformation.Checked)
            {
                at.AddressLine = TxtAddressLine.Text;
                at.City = TxtCity.Text;
                at.CountryCode = (ISO3166CountryCodeType)CboCountryCode.SelectedItem;
                at.CountryCodeSpecified = true;
                at.PostalCode = TxtPostalCode.Text;
                at.State = (StateCodeType)CboStateType.SelectedItem;
                at.StateSpecified = true;
            }
            else 
            {
                at = null;
            }
            return at;
        }

        private CreditOrDebitCardKeyedType creditOrDebitCardKeyedType()
        {
            CreditOrDebitCardKeyedType ckt = new CreditOrDebitCardKeyedType();

            ckt.CardholderName = TxtCardholderName.Text;
            ckt.CardSecurityCode = TxtCardSecurityCode.Text;
            //ckt.EncryptedPrimaryAccountNumber = "";
            ckt.ExpirationDate = TxtExpirationDate.Text;
            ckt.PrimaryAccountNumber = TxtPrimaryAccountNumber.Text;
            //ckt.ThreeDSecure = new ThreeDSecureType();
            //ckt.ThreeDSecure.AuthenticationValue = "";
            //ckt.ThreeDSecure.eCommerceIndicator = "";
            //ckt.ThreeDSecure.TransactionID = "";
            //ckt.Token = new TokenType();
            //ckt.Token.tokenId = "";
            //ckt.Token.tokenValue = "";

            return ckt;
        }

        private CardSwipedType cardSwipedType()
        {
            CardSwipedType st = new CardSwipedType();

            st.Item = new TrackDataType();

            if (ChkEncryptedData.Checked)
            {
                EncryptedData ed = new EncryptedData();
                ed.encryptiontype = EncryptionType.DUKPT;
                ed.key = TxtKeySerialNumber.Text;
                ed.Value = TxtTrackData.Text;
                st.Item.Item = ed;
            }
            else
            {
                st.Item.Item = TxtTrackData.Text;
            }
            st.ItemElementName = (ItemChoiceType)CboTrackChoice.SelectedItem;

            return st;
        }

        #endregion objects used by instrument types

        #endregion Payment Instruments

        private BillPaymentPayeeType billPaymentPayeeType()
        {
            BillPaymentPayeeType bppt = new BillPaymentPayeeType();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                bppt.PayeeAccountNumber = "1234567890123456789012345"; //25 character string value to pass Account number Payee uses to identify the payer.	Conditional – PIN-less Debit Purchase Only
                bppt.PayeeName = "John Denver"; //25 character string value used to pass Payee name related to an online bill payment using debit card	Conditional – PIN-less Debit Purchase Only
                bppt.PayeePhoneNumber = "513-555-5555"; //25 character string value to pass Payee phone number related to an online bill payment using debit card. Example: 513-555-5555	Conditional – PIN-less Debit Purchase Only
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return bppt;
        }

        #region Request Objects
        
        private AuthorizeRequest authorizeRequest()
        {
            AuthorizeRequest a = new AuthorizeRequest();

            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                Random r = new Random();
                //a.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
                //a.BillPaymentPayee.PayeeAccountNumber = "";
                //a.BillPaymentPayee.PayeeName = "";
                //a.BillPaymentPayee.PayeePhoneNumber = "";
                a.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
                a.Merchant = merchantType();
                //a.NetworkResponseCode = "";
                a.PaymentType = (PaymentType)CboPaymentType.SelectedItem;//Mandatory
                a.PaymentTypeSpecified = true;
                a.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
                //a.reportgroup = ""; //An optional (required for Litle) attribute used by the merchant to map each transaction to a reporting category.  This can be no longer than 25 characters. 
                a.TokenRequested = ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
                a.TokenRequestedSpecified = ChkTokenRequested.Checked;
                a.TransactionAmount = new AmountType();
                a.TransactionAmount.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
                a.TransactionAmount.currencySpecified = true;
                a.TransactionAmount.Value = Convert.ToDecimal(TxtTransactionAmount.Text);
                a.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
                a.TransactionType = (TransactionTypeType)CboTransactionType.SelectedItem;//Mandatory
                int rInt = r.Next(1, 99999); //for ints
                a.systemtraceid = rInt; //A conditional ID used to track each transaction. This must be an integer. Required for Raft and Tandem, optional for Litle? Required for Litle on CancelRequest.
                a.systemtraceidSpecified = true;
                rInt = r.Next(1, 99999999);
                a.merchantrefid = "PWS" + rInt.ToString(); //An optional attribute used by the merchant to identify each transaction. This can be no longer than 16 characters. If the merchant chooses not to use this field it is recommended that you populate this ID with the system-trace-id value.
                
                //Set the object for payment
                a.Items = new object[1];
                if(CboPaymentInstrument.Text == "Credit")
                    a.Items[0] = creditInstrumentType();
                else if (CboPaymentInstrument.Text == "Debit")
                    a.Items[0] = debitInstrumentType();
                else if (CboPaymentInstrument.Text == "Gift")
                    a.Items[0] = giftInstrumentType();
              
            }
            else 
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return a;
        }

        private PurchaseRequest purchaseRequest()
        {
            PurchaseRequest p = new PurchaseRequest();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                Random r = new Random();
                p.Merchant = merchantType();
                p.TransactionType = (TransactionTypeType)CboTransactionType.SelectedItem;//Mandatory
                p.PaymentType = (PaymentType)CboPaymentType.SelectedItem;//Mandatory
                p.PaymentTypeSpecified = true;
                p.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
                p.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
                p.TransactionAmount = new AmountType();
                p.TransactionAmount.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
                p.TransactionAmount.currencySpecified = true;
                p.TransactionAmount.Value = Convert.ToDecimal(TxtTransactionAmount.Text);
                p.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
                //p.TokenRequested = ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
                //p.TokenRequestedSpecified = ChkTokenRequested.Checked;
                //a.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
                int rInt = r.Next(1, 99999); //for ints
                p.systemtraceid = rInt;
                p.systemtraceidSpecified = true;
                rInt = r.Next(1, 99999999);
                p.merchantrefid = "PWS" + rInt.ToString();

                //Set the object for payment
                p.Items = new object[1];
                if (CboPaymentInstrument.Text == "Credit")
                    p.Items[0] = creditInstrumentType();
                else if (CboPaymentInstrument.Text == "Debit")
                    p.Items[0] = debitInstrumentType();
                else if (CboPaymentInstrument.Text == "Gift")
                    p.Items[0] = giftInstrumentType();

                //CreditInstrumentType cit = new CreditInstrumentType();
                //cit.CardKeyed = new CreditOrDebitCardKeyedType();
                //cit.CardKeyed.PrimaryAccountNumber = TxtPrimaryAccountNumber.Text;
                //cit.CardKeyed.ExpirationDate = TxtExpirationDate.Text;
                //cit.CardType = CreditCardNetworkType.visa;
                //cit.PartialApprovalCode = PartialIndicatorType.not_supported;
                //cit.CardholderAddress = new AddressType();
                //cit.CardholderAddress.AddressLine = "1234 Main Street";
                //cit.CardholderAddress.City = "Mason";
                //cit.CardholderAddress.State = StateCodeType.OH;
                //cit.CardholderAddress.PostalCode = "45040";
                //cit.CardholderAddress.CountryCode = ISO3166CountryCodeType.US;

                //p.Items = new object[1];
                //p.Items[0] = cit;

                ItemsChoiceType3[] ict = new ItemsChoiceType3[1];
                ict[0] = ItemsChoiceType3.Credit;
                p.ItemsElementName = ict;
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return p;
        }

        private CaptureRequest captureRequest(ResponseDetails _rd)
        {
            CaptureRequest c = new CaptureRequest();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                Random ran = new Random();
                c.Merchant = merchantType();
                c.TransactionType = (TransactionTypeType)CboTransactionType.SelectedItem;//Mandatory
                c.PaymentType = (PaymentType)CboPaymentType.SelectedItem;//Mandatory
                c.PaymentTypeSpecified = true;
                c.DraftLocatorId = "D" + ran.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
                c.ReferenceNumber = "R" + ran.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
                c.CaptureAmount = new AmountType();
                c.CaptureAmount.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
                c.CaptureAmount.currencySpecified = true;
                c.CaptureAmount.Value = Convert.ToDecimal(TxtTransactionAmount.Text);
                c.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
                c.TokenRequested = ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
                c.TokenRequestedSpecified = ChkTokenRequested.Checked;
                //a.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
                int rInt = ran.Next(1, 99999); //for ints
                c.systemtraceid = rInt;
                c.systemtraceidSpecified = true;
                rInt = ran.Next(1, 99999999);
                c.merchantrefid = "PWS" + rInt.ToString();

                //Set the object for payment
                c.Items = new object[1];
                if (CboPaymentInstrument.Text == "Credit")
                    c.Items[0] = creditInstrumentType();
                else if (CboPaymentInstrument.Text == "Debit")
                    c.Items[0] = debitInstrumentType();
                else if (CboPaymentInstrument.Text == "Gift")
                    c.Items[0] = giftInstrumentType();

                //Set the Capture specific values
                AuthorizeResponse r = new AuthorizeResponse();
                r = (AuthorizeResponse)_rd.Response;

                c.AuthorizationCode = _rd.AuthorizationCode;
                c.OriginalAmount = _rd.Amount;
                AmountType a = new AmountType();
                a.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
                a.currencySpecified = true;
                a.Value = Convert.ToDecimal(TxtTransactionAmount.Text);
                c.CaptureAmount = a;
                c.OriginalReferenceNumber = r.ReferenceNumber;

                ItemsChoiceType4[] ict = new ItemsChoiceType4[1];
                ict[0] = ItemsChoiceType4.Credit;
                c.ItemsElementName = ict;
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return c;
        }

        private AdjustRequest adjustRequest(ResponseDetails _rd)
        {
            AdjustRequest adj = new AdjustRequest();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                Random ran = new Random();
                adj.AdjustedTotalAmount = new AmountType();
                adj.AdjustedTotalAmount.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
                adj.AdjustedTotalAmount.currencySpecified = true;
                adj.AdjustedTotalAmount.Value = Convert.ToDecimal(TxtTransactionAmount.Text); //The new amount, which is the original amount and the adjustment
                //adj.AuthorizationCode = "";
                //adj.BillPaymentPayee = new BillPaymentPayeeType();
                //adj.ConvenienceFee = new AmountType();
                adj.Credit = new CreditInstrumentType();
                adj.Credit = creditInstrumentType();
                adj.DraftLocatorId = "D" + ran.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
                adj.Merchant = merchantType();
                adj.NetworkResponseCode = "";
                adj.PaymentType = (PaymentType)CboPaymentType.SelectedItem;//Mandatory
                adj.PaymentTypeSpecified = true;
                //adj.PurchaseOrder = "";
                adj.ReferenceNumber = "R" + ran.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
                adj.reportgroup = "";
                int rInt = ran.Next(1, 99999); //for ints
                adj.systemtraceid = rInt;
                adj.systemtraceidSpecified = true;
                rInt = ran.Next(1, 99999999);
                adj.merchantrefid = "PWS" + rInt.ToString();
                //adj.Tax = new TaxAmountType();
                //adj.TipAmount = new AmountType();
                adj.TokenRequested = ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
                adj.TokenRequestedSpecified = ChkTokenRequested.Checked;
                adj.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
                adj.TransactionType = (TransactionTypeType)CboTransactionType.SelectedItem;//Mandatory       

                PurchaseResponse r = new PurchaseResponse();
                r = (PurchaseResponse)_rd.Response;

                adj = adjustRequest(null);
                adj.AuthorizationCode = _rd.AuthorizationCode;
                adj.OriginalAmount = new AmountType();
                adj.OriginalAmount.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
                adj.OriginalAmount.currencySpecified = true;
                adj.OriginalAmount.Value = _rd.Amount.Value;
                adj.OriginalReferenceNumber = r.ReferenceNumber;
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return adj;
        }

        private RefundRequest refundRequest(Object _response)
        {
            RefundRequest rfnd = new RefundRequest();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                Random r = new Random();
                rfnd.Merchant = merchantType();
                rfnd.Merchant.Terminal = null;
                rfnd.TransactionType = (TransactionTypeType)CboTransactionType.SelectedItem;//Mandatory
                rfnd.PaymentType = (PaymentType)CboPaymentType.SelectedItem;//Mandatory
                rfnd.PaymentTypeSpecified = true;
                rfnd.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
                rfnd.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
                rfnd.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
                rfnd.TokenRequested = ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
                rfnd.TokenRequestedSpecified = ChkTokenRequested.Checked;
                //a.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
                int rInt = r.Next(1, 99999); //for ints
                rfnd.systemtraceid = rInt;
                rfnd.systemtraceidSpecified = true;
                rInt = r.Next(1, 99999999);
                rfnd.merchantrefid = "PWS" + rInt.ToString();

                //Set the object for payment
                rfnd.Items = new object[1];
                if (CboPaymentInstrument.Text == "Credit")
                    rfnd.Items[0] = creditInstrumentType();
                else if (CboPaymentInstrument.Text == "Debit")
                    rfnd.Items[0] = debitInstrumentType();
                else if (CboPaymentInstrument.Text == "Gift")
                    rfnd.Items[0] = giftInstrumentType();

                //CreditInstrumentType cit = new CreditInstrumentType();
                //cit.CardKeyed = new CreditOrDebitCardKeyedType();
                //cit.CardKeyed.PrimaryAccountNumber = TxtPrimaryAccountNumber.Text;
                //cit.CardKeyed.ExpirationDate = TxtExpirationDate.Text;
                //cit.CardType = CreditCardNetworkType.visa;
                //cit.PartialApprovalCode = PartialIndicatorType.not_supported;
                //cit.CardholderAddress = new AddressType();
                //cit.CardholderAddress.AddressLine = "1234 Main Street";
                //cit.CardholderAddress.City = "Mason";
                //cit.CardholderAddress.State = StateCodeType.OH;
                //cit.CardholderAddress.PostalCode = "45040";
                //cit.CardholderAddress.CountryCode = ISO3166CountryCodeType.US;

                //rfnd.Items = new object[1];
                //rfnd.Items[0] = cit;
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return rfnd;
        }

        private CancelRequest cancelRequest(ResponseDetails _rd)
        {
            CancelRequest can = new CancelRequest();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                Random ran = new Random();
                //can.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
                //can.BillPaymentPayee.PayeeAccountNumber = "";
                //can.BillPaymentPayee.PayeeName = "";
                //can.BillPaymentPayee.PayeePhoneNumber = "";
                can.DraftLocatorId = "D" + ran.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
                can.Merchant = merchantType();
                can.Merchant.Terminal = null;
                can.PaymentType = (PaymentType)CboPaymentType.SelectedItem;//Mandatory
                can.PaymentTypeSpecified = true;
                can.ReferenceNumber = "R" + ran.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
                can.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
                can.TransactionType = (TransactionTypeType)CboTransactionType.SelectedItem;//Mandatory
                can.TokenRequested = ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
                can.TokenRequestedSpecified = ChkTokenRequested.Checked;
                int rInt = ran.Next(1, 99999); //for ints
                can.systemtraceid = rInt;
                can.systemtraceidSpecified = true;
                rInt = ran.Next(1, 99999999);
                can.merchantrefid = "PWS" + rInt.ToString();

                //Set the object for payment
                can.Items = new object[1];
                if (CboPaymentInstrument.Text == "Credit")
                    can.Items[0] = creditInstrumentType();
                else if (CboPaymentInstrument.Text == "Debit")
                    can.Items[0] = debitInstrumentType();
                else if (CboPaymentInstrument.Text == "Gift")
                    can.Items[0] = giftInstrumentType();
                
                //CreditInstrumentType cit = new CreditInstrumentType();
                //cit.CardKeyed = new CreditOrDebitCardKeyedType();
                //cit.CardKeyed.PrimaryAccountNumber = TxtPrimaryAccountNumber.Text;
                //cit.CardKeyed.ExpirationDate = TxtExpirationDate.Text;
                //cit.CardType = CreditCardNetworkType.visa;
                //cit.PartialApprovalCode = PartialIndicatorType.not_supported;
                //cit.CardholderAddress = new AddressType();
                //cit.CardholderAddress.AddressLine = "1234 Main Street";
                //cit.CardholderAddress.City = "Mason";
                //cit.CardholderAddress.State = StateCodeType.OH;
                //cit.CardholderAddress.PostalCode = "45040";
                //cit.CardholderAddress.CountryCode = ISO3166CountryCodeType.US;

                //can.Items = new object[1];
                //can.Items[0] = cit;

                PurchaseResponse r = new PurchaseResponse();
                r = (PurchaseResponse)_rd.Response;

                can.CancelType = (CancelTransactionType)CboTransactionType.SelectedItem;
                can.OriginalAmount = _rd.Amount;
                can.OriginalTransactionTimestamp = r.TransactionTimestamp;
                can.OriginalTransactionTimestampSpecified = true;
                can.OriginalSystemTraceId = r.systemtraceid;
                can.OriginalSystemTraceIdSpecified = true;
                can.OriginalReferenceNumber = r.ReferenceNumber;
                // NEED TO ADD can.OriginalSequenceNumber = p.Merchant.Software.SequenceNumber;
                can.OriginalAuthCode = _rd.AuthorizationCode;
                can.NetworkResponseCode = r.NetworkResponseCode;
                can.ReversalReason = (ReversalReasonType)CboReversalReason.SelectedItem;
                can.ReversalReasonSpecified = true;
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return can;
        }

        private TokenizeRequest tokenizeRequest()
        {
            TokenizeRequest t = new TokenizeRequest();
            if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            {
                Random r = new Random();
                //t.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
                //t.BillPaymentPayee.PayeeAccountNumber = "";
                //t.BillPaymentPayee.PayeeName = "";
                //t.BillPaymentPayee.PayeePhoneNumber = "";
                t.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
                t.Merchant = merchantType();
                t.Merchant.Terminal = null;
                t.PaymentType = (PaymentType)CboPaymentType.SelectedItem;//Mandatory
                t.PaymentTypeSpecified = true;
                t.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
                t.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
                t.TransactionType = (TransactionTypeType)CboTransactionType.SelectedItem;//Mandatory
                t.TokenRequested = true;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
                t.TokenRequestedSpecified = true;
                int rInt = r.Next(1, 99999); //for ints
                t.systemtraceid = rInt;
                t.systemtraceidSpecified = true;
                rInt = r.Next(1, 99999999);
                t.merchantrefid = "PWS" + rInt.ToString();

                //Set the object for payment
                t.Item = new object[1];
                if (CboPaymentInstrument.Text == "Credit")
                    t.Item = creditInstrumentType();
                else if (CboPaymentInstrument.Text == "Debit")
                    t.Item = debitInstrumentType();
                else if (CboPaymentInstrument.Text == "Gift")
                    t.Item = giftInstrumentType();
            }
            else
            {
                MessageBox.Show("Industry type is not defined.");
            }

            return t; 
        }

        #endregion Request Objects

        #endregion PWS Objects

        #region API Operations

        private ResponseDetails Authorize()
        {
            /*Authorize is used to reserve funding for the transaction amount, but does not request settlement.
              Note: If the merchant does not receive a response for an Auth, then the merchant should perform a 
              reversal on the Auth transaction. Also there will not be any reversals in cancel and reversals on a reversal transaction.
            */
            AuthorizeRequest a = authorizeRequest();
            return ProcessResponse(PWSClient.Authorize(a));
        }

        private ResponseDetails Capture(ResponseDetails _rd)
        {
            /* Capture is used to schedule a prior authorization for settlement. 
             */

            CaptureRequest c = captureRequest(_rd);
            return ProcessResponse(PWSClient.Capture(c));
        }

        private ResponseDetails Purchase()
        {
            /* Purchase is used to reserve funding for the transaction amount and requesting settlement on the merchant’s behalf.
             */
            PurchaseRequest p = purchaseRequest();
            return ProcessResponse(PWSClient.Purchase(p));
        }

        private ResponseDetails Adjust(ResponseDetails _rd)
        {
            //Adjust is used to modify a previous transaction, prior to settlement. Credit card only. Litle Does not support this transaction.

            AdjustRequest adj = adjustRequest(_rd);
            return ProcessResponse(PWSClient.Adjust(adj)); 
        }

        private ResponseDetails Refund(ResponseDetails _rd)
        {
            /* Refund is used to transfer funds from the merchant back to the cardholder. 
             */

            RefundRequest rfnd = refundRequest(null);
            rfnd.RefundAmount = new AmountType();
            rfnd.RefundAmount.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
            rfnd.RefundAmount.currencySpecified = true;
            rfnd.RefundAmount.Value = 1.00M;

            return ProcessResponse(PWSClient.Refund(rfnd));
        }

        private ResponseDetails Cancel(ResponseDetails _rd)
        {
            /* Cancel is used to reverse a previous transaction. Reversing a ‘Close Batch’ transaction is not available. There are 
               two types of cancel operations, Merchant Initiated and System Initiated (also known as timeout reversal). Both of 
               these are sent by the client but have different meanings. 
               
               NOTE: A $0 Authorization and a Tokenize cannot be cancelled.
               
               Note:   In case a response is not received (for a timeout), it is recommended that the merchant does a reversal 
               before resending the same request.
             */
            
            CancelRequest can = cancelRequest(_rd);
            return ProcessResponse(PWSClient.Cancel(can));
        }

        private ResponseDetails CloseBatch()
        {
            /*Close Batch is used to close out the transaction batch for the day. The close batch operation is 
              used primarily by businesses that need to manually start end of day processing, typically restaurants.
            */
            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        private ResponseDetails Tokenize()
        {
            /*Tokenize is used to return a token for a card so that it may be used in future transactions. 
             */
            
            TokenizeRequest tr = tokenizeRequest();
            return ProcessResponse(PWSClient.Tokenize(tr));
        }

        private ResponseDetails Activate()
        {
            /*Activate is used to load an initial balance on a gift card or to create a Virtual Gift Card.
            */
            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        private ResponseDetails Unload()
        {
            /*Unload is used to remove the remaining balance on a gift card. Gift card only
            */
            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        private ResponseDetails Reload()
        {
            /*Reload is used to add an additional amount to a gift card. Gift card only.
            */
            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        private ResponseDetails Close()
        {
            /*Close is used to finalize a gift card with no further transactions allowed.
            */
            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        private ResponseDetails BalanceInquiry()
        {
            /*Balance Inquiry is used to retrieve the balance on a gift card or a prepaid credit card.
            */
            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        private ResponseDetails BatchBalance()
        {
            /*Batch Balance is used to retrieve a summary for a batch. There are two types of Batch Balance requests, 
              GIFT and CREDIT which are designated by setting the PaymentInstrumentType.
            */

            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        private ResponseDetails UpdateCard()
        {
            /*Retrieve a summary of Returns and Sales for all the transactions in the current batch for prepaid credit card 
            or summary of Returns and Sales and, details grouped by following transaction types for gift cards:
            */

            MessageBox.Show("Can't seem to find the code thingy for this one");
            return null;
        }

        #endregion API Operations

        #region Process Response

        private ResponseDetails ProcessResponse(Object _response)
        {
            string AuthorizationCode = "";

            try
            {
                if (((TransactionResponseType)(_response)).ItemsElementName.Count() > 0)
                {
                    int idx = 0;
                    while (idx < ((TransactionResponseType)(_response)).Items.Count())
                    {
                        if (((TransactionResponseType)(_response)).ItemsElementName[idx] == ItemsChoiceType1.AuthorizationCode)
                            AuthorizationCode = ((TransactionResponseType)(_response)).Items[idx];
                        idx++;
                    }
                }
                //Add to CheckListBox
                AmountType a = new AmountType();
                a.currency = (ISO4217CurrencyCodeType)CboCurrencyCodeType.SelectedItem;
                a.currencySpecified = true;
                a.Value = Convert.ToDecimal(TxtTransactionAmount.Text);

                ResponseDetails rd = new ResponseDetails(_response, a, AuthorizationCode);
                ChkLstTransactionsProcessed.Items.Add(rd);

                return rd;
            }
            catch
            {
                return null;
            }
        }

        public static string SerializeToString(object obj)
        {
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            using (StringWriter writer = new StringWriter())
            {
                serializer.Serialize(writer, obj);
                return writer.ToString();
            }
        }

        #endregion Process Response

    }

    public class ResponseDetails
    {
        public Object Response;
        public AmountType Amount;
        public string AuthorizationCode;

        public ResponseDetails(Object response, AmountType amount, string authorizationCode)
        {
            Response = response;
            Amount = amount;
            AuthorizationCode = authorizationCode;
        }
        public override string ToString()
        {// Generates the text shown in the List Checkbox
            try
            {
                string info = "";
                info = Amount.Value.ToString() + " " + (((TransactionResponseType)(Response)).GetType()).ToString().Replace("Response", "");
                //((TransactionResponseType)(Response)).

                if (((TransactionResponseType)(Response)).Items != null && ((TransactionResponseType)(Response)).Items.Count() > 0)
                {
                    info += " (";
                    int idx = 0;
                    while (idx < ((TransactionResponseType)(Response)).Items.Count())
                    {
                        info += ((TransactionResponseType)(Response)).ItemsElementName[idx] + ": " + ((TransactionResponseType)(Response)).Items[idx] + " ";
                        idx++;
                    }
                    info += ") ";
                }  

                info += " [" + DateTime.Now + "]" + " RequestId: " + ((TransactionResponseType)(Response)).RequestId;

                return info;
            }
            catch (Exception ex) {
                return "[Error: " + ex.Message +"]";
            }

        }
    }
    public class item
    {
        public string Name;
        public string Value;

        public item(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            // Generates the text shown in the combo box
            return Name;
        }
    }

    public class TestScenario
    {
        public string TestNumber;
        public Transaction.Response TestResponse;
        public Services.Response SvcResponse;

        public TestScenario(string testNumber, Transaction.Response testResponse, Services.Response svcResponse)
        {
            TestNumber = testNumber;
            TestResponse = testResponse;
            SvcResponse = svcResponse;
        }
        public override string ToString()
        {
            // Generates the text shown in the combo box
            return TestNumber;
        }
    }

}
