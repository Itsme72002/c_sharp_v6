/*
Copyright (c) 2014 Vantiv, Inc. - All Rights Reserved.

Sample Code is for reference only and is solely intended to be used for educational purposes and is provided “AS IS” and “AS AVAILABLE” and without 
warranty. It is the responsibility of the developer to  develop and write its own code before successfully certifying their solution.  

This sample may not, in whole or in part, be copied, photocopied, reproduced, translated, or reduced to any electronic medium or machine-readable 
form without prior consent, in writing, from Vantiv, Inc.

Use, duplication or disclosure by the U.S. Government is subject to restrictions set forth in an executed license agreement and in subparagraph (c)(1) 
of the Commercial Computer Software-Restricted Rights Clause at FAR 52.227-19; subparagraph (c)(1)(ii) of the Rights in Technical Data and Computer 
Software clause at DFARS 252.227-7013, subparagraph (d) of the Commercial Computer Software--Licensing clause at NASA FAR supplement 16-52.227-86; 
or their equivalent.

Information in this sample code is subject to change without notice and does not represent a commitment on the part of Vantiv, Inc.  In addition to 
the foregoing, the Sample Code is subject to the terms and conditions set forth in the Vantiv Terms and Conditions of Use (http://www.apideveloper.vantiv.com) 
and the Vantiv Privacy Notice (http://www.vantiv.com/Privacy-Notice).  
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Configuration;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.IO;
using Common;
using System.Web.Services.Protocols;
using System.Web.Services;
using System.Reflection;
using CertSuiteTool_VDP;
using CertSuiteTool.Helper_Class;
using System.Net;
using System.Timers;

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
        public PaymentPortTypeClient PWSClient = new PaymentPortTypeClient();
        TransactionRequestType CurrentTransactionRequestTypeObject;
        
        ////Endpoint addresses
        //http://msdn.microsoft.com/en-us/library/77hkfhh8(VS.71).aspx //SOAP Headers
        //http://msdn.microsoft.com/en-us/library/ms819938.aspx
        //http://msdn.microsoft.com/en-us/library/vstudio/9z52by6a(v=vs.100).aspx
        //http://stackoverflow.com/questions/11263640/net-client-authentication-and-soap-credential-headers-for-a-cxf-web-service

        /*Apigee Endpoint URL By transaciton type*/

        private string _VDPEndpointAddress = ConfigurationSettings.AppSettings["VDP_EndpointAddress"];//drUdR9fR
        private string _VDPLicenseId = ConfigurationSettings.AppSettings["VDP_LicenseId"];//drUdR9fR
        private string _PWSEndpointAddress = ConfigurationSettings.AppSettings["PWS_EndpointAddress"]; //Options "https://ws-cert.vantiv.com/merchant/payments-cert/v6", "https://ws-stage.infoftps.com:4443/merchant/payments-stage/v6"
        private string _PWSUserName = ConfigurationSettings.AppSettings["PWS_VDP_UserName"]; //s.MID2.PAY.WS.NP
        private string _PWSPassword = ConfigurationSettings.AppSettings["PWS_VDP_Password"];//drUdR9fR

        private bool _fromLoading;

        private static System.Timers.Timer aTimer;

        //The following are used to switch the URI for posting data
        private static object svcInfoChannelLock = new object();
        
        public Send_Transactions()
        {
            InitializeComponent();
            _fromLoading = true;
            try
            {
                new VDP_Helper(this);//Used for clients integrating to Vantiv Developer Portal (VDP)
                new PWS_Helper(this);//Used for clients integrating to Payments Web Services (PWS)
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //PicVantiLogo
            PicVantiLogo.Image = ImageFromBase64String(@"iVBORw0KGgoAAAANSUhEUgAAA5IAAAB/CAYAAACdQ/3TAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAADKGlUWHRYTUw6Y29tLmFkb2JlLnhtcAAAAAAAPD94cGFja2V0IGJlZ2luPSLvu78iIGlkPSJXNU0wTXBDZWhpSHpyZVN6TlRjemtjOWQiPz4gPHg6eG1wbWV0YSB4bWxuczp4PSJhZG9iZTpuczptZXRhLyIgeDp4bXB0az0iQWRvYmUgWE1QIENvcmUgNS41LWMwMjEgNzkuMTU1NzcyLCAyMDE0LzAxLzEzLTE5OjQ0OjAwICAgICAgICAiPiA8cmRmOlJERiB4bWxuczpyZGY9Imh0dHA6Ly93d3cudzMub3JnLzE5OTkvMDIvMjItcmRmLXN5bnRheC1ucyMiPiA8cmRmOkRlc2NyaXB0aW9uIHJkZjphYm91dD0iIiB4bWxuczp4bXA9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC8iIHhtbG5zOnhtcE1NPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvbW0vIiB4bWxuczpzdFJlZj0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL3NUeXBlL1Jlc291cmNlUmVmIyIgeG1wOkNyZWF0b3JUb29sPSJBZG9iZSBQaG90b3Nob3AgQ0MgMjAxNCAoTWFjaW50b3NoKSIgeG1wTU06SW5zdGFuY2VJRD0ieG1wLmlpZDoyQ0Q0REY4RTE1Q0IxMUU0OENFNUZBODlGN0UyQjYyRCIgeG1wTU06RG9jdW1lbnRJRD0ieG1wLmRpZDoyQ0Q0REY4RjE1Q0IxMUU0OENFNUZBODlGN0UyQjYyRCI+IDx4bXBNTTpEZXJpdmVkRnJvbSBzdFJlZjppbnN0YW5jZUlEPSJ4bXAuaWlkOjJDRDRERjhDMTVDQjExRTQ4Q0U1RkE4OUY3RTJCNjJEIiBzdFJlZjpkb2N1bWVudElEPSJ4bXAuZGlkOjJDRDRERjhEMTVDQjExRTQ4Q0U1RkE4OUY3RTJCNjJEIi8+IDwvcmRmOkRlc2NyaXB0aW9uPiA8L3JkZjpS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");
            txtDisclaimer.Text = "Copyright (c) 2014 Vantiv, Inc. - All Rights Reserved."
                                + "Sample Code is for reference only and is solely intended to be used for educational purposes and is provided “AS IS” and “AS AVAILABLE” and without "
                                + "warranty. It is the responsibility of the developer to  develop and write its own code before successfully certifying their solution. "
                                + "\r\n\r\n"
                                + "This sample may not, in whole or in part, be copied, photocopied, reproduced, translated, or reduced to any electronic medium or machine-readable "
                                + "form without prior consent, in writing, from Vantiv, Inc."
                                + "\r\n\r\n"
                                + "Use, duplication or disclosure by the U.S. Government is subject to restrictions set forth in an executed license agreement and in subparagraph (c)(1) "
                                + "of the Commercial Computer Software-Restricted Rights Clause at FAR 52.227-19; subparagraph (c)(1)(ii) of the Rights in Technical Data and Computer "
                                + "Software clause at DFARS 252.227-7013, subparagraph (d) of the Commercial Computer Software--Licensing clause at NASA FAR supplement 16-52.227-86; "
                                + "or their equivalent."
                                 + "\r\n\r\n"
                                + "Information in this sample code is subject to change without notice and does not represent a commitment on the part of Vantiv, Inc.  In addition to "
                                + "the foregoing, the Sample Code is subject to the terms and conditions set forth in the Vantiv Terms and Conditions of Use (http://www.apideveloper.vantiv.com) "
                                + "and the Vantiv Privacy Notice (http://www.vantiv.com/Privacy-Notice).";

            //Set Defaults
            CboPaymentInstrument.Text = "";
            CboTerminalDetail.Text = "";//set defult

            CboCreditType.Items.Add("CardKeyed");
            CboCreditType.Items.Add("CardSwiped");
            CboCreditType.Text = "";

            CboTerminalDetail.Items.Add("Terminal");
            CboTerminalDetail.Items.Add("Software");
            CboTerminalDetail.Items.Add("Mobile");
            CboTerminalDetail.Text = "";

            GrpKeyedData.Enabled = true;
            GrpTrackData.Enabled = false;

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
            
            CboTerminalEnvironmentalCode.Sorted = true;
            CboTerminalEnvironmentalCode.DataSource = Enum.GetValues(typeof(TerminalClassificationType));
            try { CboTerminalEnvironmentalCode.SelectedIndex = -1; }
            catch { }

            CboCardInputCode.Sorted = true;
            CboCardInputCode.DataSource = Enum.GetValues(typeof(CardInputCode));
            try { CboCardInputCode.SelectedIndex = -1; }
            catch { }

            CboEntryMode.Sorted = true;
            CboEntryMode.DataSource = Enum.GetValues(typeof(EntryModeType));
            try { CboEntryMode.SelectedIndex = -1; }
            catch { }

            CboBalanceInquiry.Sorted = true;
            CboBalanceInquiry.Items.Add(true);
            CboBalanceInquiry.Items.Add(false);

            CboHostAdjustment.Sorted = true;
            CboHostAdjustment.Items.Add(true);
            CboHostAdjustment.Items.Add(false);

            CboPinEntry.Sorted = true;
            CboPinEntry.DataSource = Enum.GetValues(typeof(PinEntryType));
            try { CboPinEntry.SelectedIndex = -1; }
            catch { }
            
            //Only for PWS

            CboCardReaderType.Sorted = true;
            CboCardReaderType.DataSource = Enum.GetValues(typeof(CardReaderType));
            try { CboCardReaderType.SelectedIndex = -1; }
            catch { }

            #region 
            //Format [MID] : [TID] : [Description]
            CboTestMerchantAccounts.Items.Add("4445012916098 : 001 : Tandem with tokenization");
            CboTestMerchantAccounts.Items.Add("4445012916106 : 001 : Tandem without tokenization");
            #endregion Test Merchant Accounts

            CboSendTransaction.Items.Add(new item("Authorize", "Authorize"));// (Authorization) (AVSOnly 0.00)
            CboSendTransaction.Items.Add(new item("Capture", "Capture"));// (AuthorizationCompletion)
            CboSendTransaction.Items.Add(new item("Purchase aka Sale", "Purchase"));// (Sale) (Voice Auth)
            CboSendTransaction.Items.Add(new item("Adjust", "Adjust"));
            CboSendTransaction.Items.Add(new item("Refund", "Refund"));//(Return)
            CboSendTransaction.Items.Add(new item("Cancel", "Cancel"));
            CboSendTransaction.Items.Add(new item("Void (VDP only)", "Void"));
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

            //Setup Endpoint addresses and login for VDP
            TxtLicenseKeyAPIKey.Text = _VDPLicenseId;
            txtVDPBaseEndpointURL.Text = _VDPEndpointAddress;

            //Setup Endpoint addresses and login for PWS
            txtPWSBaseEndpointURL.Text = _PWSEndpointAddress;
            TxtUserName.Text = _PWSUserName;
            TxtPassword.Text = _PWSPassword;
            lock (svcInfoChannelLock)
            {
                PWSClient.Endpoint.Address = new EndpointAddress(_PWSEndpointAddress);
                PWSClient.ClientCredentials.UserName.UserName = _PWSUserName;
                PWSClient.ClientCredentials.UserName.Password = _PWSPassword;
                PWSClient.Open();
            }

            //Bindings
            //Info about Custom bindings for app.config : http://zianet.dk/blog/2010/12/20/getting-wcf-to-talk-to-a-java-axis-1-x-and-wss4j-web-service-part-3-of-3/
                        
            #endregion setup endpoints

            DisableFields();

            _fromLoading = false;
        }

        #region Form Events

        private void CmdSendTransaction_Click(object sender, EventArgs e)
        {
            try
            {
                if (CboSendTransaction.Text.Length < 1)
                {
                    MessageBox.Show("Please select a message type to send");
                    return;
                }
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
                        if (((ResponseDetails)(itemChecked)).TxnRequestType != "Authorize")
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
                        if (((ResponseDetails)(itemChecked)).TxnRequestType != "Purchase")
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
                    //if (ChkLstTransactionsProcessed.CheckedItems.Count < 1 && CboPaymentInstrument.Text == "Credit") { MessageBox.Show("Please select a transaction(s) to process"); return; }
                    ////First verify if all transactions selected are "Authorize" transactions
                    //List<ResponseDetails> txnsToProcess = new List<ResponseDetails>();
                    //foreach (object itemChecked in ChkLstTransactionsProcessed.CheckedItems)
                    //{
                    //    //((TransactionResponseType)(_response))
                    //    if (((ResponseDetails)(itemChecked)).TxnRequestType != "Authorize")
                    //    {
                    //        MessageBox.Show("All selected messages must be of type Authorize");
                    //        Cursor = Cursors.Default;
                    //        return;
                    //    }
                    //    txnsToProcess.Add(((ResponseDetails)(itemChecked)));
                    //}
                    ////Now process each Authorize message selected
                    //foreach (ResponseDetails _RD in txnsToProcess)
                    //{
                        Return();
                    //}
                }
                if (test == "Cancel")
                {
                    //First verify if all transactions selected can be Canceled
                        //Credit transaction(s) : adjust, authorize, capture, purchase, refund
                        //Debit transaction(s) : purchase, refund, purchase_cashback
                        //Gift transaction(s) : activate, authorize, capture, close, purchase, refund, reload, unload
                    List<ResponseDetails> txnsToProcess = new List<ResponseDetails>();
                    foreach (object itemChecked in ChkLstTransactionsProcessed.CheckedItems)
                    {
                        if (ChkLstTransactionsProcessed.CheckedItems.Count < 1) { MessageBox.Show("Please select transactions to cancel"); return; }
                     
                        //Credit
                        if (((ResponseDetails)(itemChecked)).PaymentInstrumentType == "Credit" &&
                            (((ResponseDetails)(itemChecked)).TxnRequestType != "Adjust"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Authorize"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Capture"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Purchase"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Sale"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Refund"))
                        {
                            MessageBox.Show("Invalid message type for Credit - cancel");
                            Cursor = Cursors.Default;
                            return;
                        }
                        //Debit
                        if (((ResponseDetails)(itemChecked)).PaymentInstrumentType == "Debit" &&
                            (((ResponseDetails)(itemChecked)).TxnRequestType != "Purchase"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Refund"))
                        {
                            MessageBox.Show("Invalid message type for Debit - cancel");
                            Cursor = Cursors.Default;
                            return;
                        }
                        //Gift
                        if (((ResponseDetails)(itemChecked)).PaymentInstrumentType == "Gift" &&
                            (((ResponseDetails)(itemChecked)).TxnRequestType != "Activate"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Authorize"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Capture"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Close"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Purchase"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Sale"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Refund"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Reload"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Unload"))
                        {
                            MessageBox.Show("Invalid message type for Gift - cancel");
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
                if (test == "Void")
                {
                    List<ResponseDetails> txnsToProcess = new List<ResponseDetails>();
                    foreach (object itemChecked in ChkLstTransactionsProcessed.CheckedItems)
                    {
                        if (ChkLstTransactionsProcessed.CheckedItems.Count < 1) { MessageBox.Show("Please select transactions to void"); return; }

                        //Credit
                        if (((ResponseDetails)(itemChecked)).PaymentInstrumentType == "Credit" &&
                            (((ResponseDetails)(itemChecked)).TxnRequestType != "Adjust"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Authorize"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Capture"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Purchase"
                            && ((ResponseDetails)(itemChecked)).TxnRequestType != "Refund"))
                        {
                            MessageBox.Show("Invalid message type for Credit - void");
                            Cursor = Cursors.Default;
                            return;
                        }
                        //Debit
                        if (((ResponseDetails)(itemChecked)).PaymentInstrumentType == "Debit" &&
                            (((ResponseDetails)(itemChecked)).TxnRequestType != "Purchase"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Refund"))
                        {
                            MessageBox.Show("Invalid message type for Debit - void");
                            Cursor = Cursors.Default;
                            return;
                        }
                        //Gift
                        if (((ResponseDetails)(itemChecked)).PaymentInstrumentType == "Gift" &&
                            (((ResponseDetails)(itemChecked)).TxnRequestType != "Activate"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Authorize"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Capture"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Close"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Purchase"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Refund"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Reload"
                            | ((ResponseDetails)(itemChecked)).TxnRequestType != "Unload"))
                        {
                            MessageBox.Show("Invalid message type for Gift - void");
                            Cursor = Cursors.Default;
                            return;
                        }
                        txnsToProcess.Add(((ResponseDetails)(itemChecked)));
                    }
                    //Now process each Authorize message selected
                    foreach (ResponseDetails _RD in txnsToProcess)
                    {
                        Void(_RD);
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

                DialogResult Result;
                Result = MessageBox.Show("Unable to process the transaction. Add Request to list of transactions?", "Add to Transaction List", MessageBoxButtons.YesNo);
                if (Result == DialogResult.Yes)
                {
                    ResponseDetails rd = new ResponseDetails(CboPWSorVDP.Text, null, null, CboPaymentInstrument.Text, CboSendTransaction.Text, null, null);
                    //ResponseDetails rd = new ResponseDetails(CboPWSorVDP.Text, null, null, null, null, CurrentTransactionRequestTypeObject, null, CboPaymentInstrument.Text, CboSendTransaction.Text);
                    ChkLstTransactionsProcessed.Items.Add(rd);
                }
            }
        }

        private void CmdClearTransactions_Click(object sender, EventArgs e)
        {
            ChkLstTransactionsProcessed.Items.Clear();
        }

        private void cmdGo_Click(object sender, EventArgs e)
        {
            if (CboPWSorVDP.Text.Length > 0)
                TbControl.SelectedTab = TbMerchantData;
            else
            {
                MessageBox.Show("Please select a integration method");
                CboPWSorVDP.Focus();
            }
        }

        private void CmdSendTransactions_Click(object sender, EventArgs e)
        {
            if (TxtMID.Text.Length < 1 | TxtTID.Text.Length < 1)
            { 
                MessageBox.Show("Please either enter a Merchant Id and Terminal Id or select an account from the drop down");
                return;
            }
            TbControl.SelectedTab = TbTransactionData;
        }

        private void CmdExampleTestVal_Click(object sender, EventArgs e)
        {
            SetTestTrackData();
        }

        private void ChkEncryptedData_CheckedChanged(object sender, EventArgs e)
        {
            if (ChkEncryptedData.Checked)
                TxtKeySerialNumber.Enabled = true;
            else
                TxtKeySerialNumber.Enabled = false;
        }

        private void ChkLstTransactionsProcessed_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ChkOnClickDisplayTxnMessage.Checked)
                return;

            ResponseDetails rd = ((ResponseDetails)(ChkLstTransactionsProcessed.SelectedItem));
            if (rd.PWS_TxnSummary != null && rd.PWS_TxnSummary.Response != null)
                MessageBox.Show(PWS_Helper.ExtractDetailsFromResponse(rd));
            if (rd.VDP_TxnSummary != null && rd.VDP_TxnSummary.XMLResponse != null)
            {
                MessageBox.Show("REQUEST\r\n" + rd.VDP_TxnSummary.JsonRequest + "\r\n\r\nRESPONSE\r\n" + rd.VDP_TxnSummary.JsonResponse);
                ExtractAdditionalMetaDataFromTxn(rd);
            }

        }

        private void ExtractAdditionalMetaDataFromTxn(ResponseDetails _rd)
        {
            //Clear out boxes we hope to extract meta data
            TxtRequestId.Text = "";
            TxtTokenId.Text = "";
            TxtTokenValue.Text = "";

            //Attempt to extract a RequestId
            try
            {
                TxtRequestId.Text = VDP_Helper.SELECT(_rd.VDP_TxnSummary.XMLResponse, "//RequestId");
            }
            catch{}
            //Attempt to extract Token Information
            try
            {
                if (ChkUseToken.Checked)
                {
                    TxtTokenId.Text = VDP_Helper.SELECT(_rd.VDP_TxnSummary.XMLResponse, "//tokenId");
                    TxtTokenValue.Text = VDP_Helper.SELECT(_rd.VDP_TxnSummary.XMLResponse, "//tokenValue");
                }
            }
            catch { }
        }

        private void ChkUseToken_CheckedChanged(object sender, EventArgs e)
        {
            if (ChkUseToken.Checked)
            {
                DialogResult Result;
                Result = MessageBox.Show("When using a token 'Token Requested' should be false, the PAN should not be set and the transaction should be a keyed transaction. Adjust settings to match?", "Adjust Settings", MessageBoxButtons.OKCancel);
                if (Result == DialogResult.OK)
                {
                    TxtPrimaryAccountNumber.Text = "";
                    ChkTokenRequested.Checked = false;
                    CboCreditType.SelectedItem = "CardKeyed";
                }
            }
        }

        private void CboTransactionType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_fromLoading)
                return;

            try
            {
                DisableFields();
                if ((MarketCode)CboTransactionType.SelectedItem == MarketCode.ecommerce | (MarketCode)CboTransactionType.SelectedItem == MarketCode.moto)
                {
                    CboPaymentInstrument.Enabled = true;
                    CboTerminalDetail.Enabled = true;
                    GrpKeyedData.Enabled = true;
                    CboCreditType.Enabled = true;

                    CboCreditType.Text = "CardKeyed";
                    CboCreditType.BackColor = Color.FromArgb(252, 209, 5);

                    CboPartialApprovalCode.SelectedItem = PartialIndicatorType.not_supported;
                    CboPartialApprovalCode.BackColor = Color.FromArgb(252, 209, 5);
                }
                else if ((MarketCode)CboTransactionType.SelectedItem == MarketCode.present)
                {
                    CboPaymentInstrument.Enabled = true; 
                    CboTerminalDetail.Enabled = true;
                    GrpTrackData.Enabled = true;
                    CboCreditType.Enabled = true;
                    
                    CboCreditType.BackColor = Color.FromArgb(252, 209, 5);
                    CboCreditType.Text = "CardSwiped";

                    CboPartialApprovalCode.SelectedItem = PartialIndicatorType.supported;
                    CboPartialApprovalCode.BackColor = Color.FromArgb(252, 209, 5);
                }
                else
                {
                    MessageBox.Show("Sample of Common values have not been defined");
                }

                StarteTimer();
            }
            catch { }
        }

        private void StarteTimer()
        {
            TmrHighlightChangedFields.Interval = 5000;
            TmrHighlightChangedFields.Start();
            TmrHighlightChangedFields.Enabled = true;
            TmrHighlightChangedFields.Tick += new EventHandler(OnTimedEvent);
        }

        private void OnTimedEvent(Object myObject, EventArgs myEventArgs)
        {
            TmrHighlightChangedFields.Stop();
            try
            {
                //SystemColors.WindowText;
                CboCreditType.BackColor = Color.FromArgb(255, 255, 255);
                CboPartialApprovalCode.BackColor = Color.FromArgb(255, 255, 255);
                CboTerminalEnvironmentalCode.BackColor = Color.FromArgb(255, 255, 255);
                CboCardInputCode.BackColor = Color.FromArgb(255, 255, 255);
                CboEntryMode.BackColor = Color.FromArgb(255, 255, 255);
                CboBalanceInquiry.BackColor = Color.FromArgb(255, 255, 255);
                CboHostAdjustment.BackColor = Color.FromArgb(255, 255, 255);
                CboPinEntry.BackColor = Color.FromArgb(255, 255, 255);
                CboCardReaderType.BackColor = Color.FromArgb(255, 255, 255);
                TxtTrackData.BackColor = Color.FromArgb(255, 255, 255);   
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
         
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

        private void CboCreditType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_fromLoading)
                return;
            DialogResult Result;
            Result = MessageBox.Show("Adjust Terminal Settings to Match?", "Adjust Terminal", MessageBoxButtons.OKCancel);
            if (Result == DialogResult.OK)
            {
                try 
                {
                    if (CboCreditType.Text == "CardKeyed")
                    {//Set Terminal settings to common values for Keyed
                        CboTerminalEnvironmentalCode.SelectedItem = TerminalClassificationType.electronic_cash_register;
                        CboTerminalEnvironmentalCode.BackColor = Color.FromArgb(252, 209, 5);

                        CboCardInputCode.SelectedItem = CardInputCode.ManualKeyed;
                        CboCardInputCode.BackColor = Color.FromArgb(252, 209, 5);

                        CboEntryMode.SelectedItem = EntryModeType.manual;
                        CboEntryMode.BackColor = Color.FromArgb(252, 209, 5);

                        CboBalanceInquiry.SelectedItem = false;
                        CboBalanceInquiry.BackColor = Color.FromArgb(252, 209, 5);

                        CboHostAdjustment.SelectedItem = false;
                        CboHostAdjustment.BackColor = Color.FromArgb(252, 209, 5);

                        CboPinEntry.SelectedItem = PinEntryType.none;
                        CboPinEntry.BackColor = Color.FromArgb(252, 209, 5);

                        CboCardReaderType.SelectedItem = CardReaderType.not_specified;
                        CboCardReaderType.BackColor = Color.FromArgb(252, 209, 5);

                        if(TxtPrimaryAccountNumber.Text.Length > 9)
                            CardTypeLookup(TxtPrimaryAccountNumber.Text);

                        //Set up common values based on keyed versus swiped
                        TxtCardSecurityCode.Text = "111";
                        ChkSetAddressInformation.Checked = true;
                    }
                    else if (CboCreditType.Text == "CardSwiped")
                    {//Set Terminal settings to common values for Swiped
                        CboTerminalEnvironmentalCode.SelectedItem = TerminalClassificationType.electronic_cash_register;
                        CboTerminalEnvironmentalCode.BackColor = Color.FromArgb(252, 209, 5);

                        CboCardInputCode.SelectedItem = CardInputCode.MagstripeRead;
                        CboCardInputCode.BackColor = Color.FromArgb(252, 209, 5);

                        CboEntryMode.SelectedItem = EntryModeType.track2;
                        CboEntryMode.BackColor = Color.FromArgb(252, 209, 5);

                        CboBalanceInquiry.SelectedItem = false;
                        CboBalanceInquiry.BackColor = Color.FromArgb(252, 209, 5);

                        CboHostAdjustment.SelectedItem = false;
                        CboHostAdjustment.BackColor = Color.FromArgb(252, 209, 5);

                        CboPinEntry.SelectedItem = PinEntryType.none;
                        CboPinEntry.BackColor = Color.FromArgb(252, 209, 5);

                        CboCardReaderType.SelectedItem = CardReaderType.magstripe;
                        CboCardReaderType.BackColor = Color.FromArgb(252, 209, 5);

                        //Verify that the cardtype matches the track data.
                        string pan = ExtractPANFromTrack(TxtTrackData.Text);
                        if (pan.Length > 9)
                            CardTypeLookup(pan);

                        //Set up common values based on keyed versus swiped
                        TxtCardSecurityCode.Text = "";
                        ChkSetAddressInformation.Checked = false;

                        TxtTrackData.BackColor = Color.FromArgb(252, 209, 5);
                        SetTestTrackData();
                    }
                    StarteTimer();
                }
                catch { }
            }
            
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

        private void CboTerminalDetail_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (CboTerminalDetail.Text == "Mobile")
                {
                    TxtLongitude.Enabled = true;
                    TxtLatitude.Enabled = true;
                }
                else
                {
                    TxtLongitude.Enabled = false;
                    TxtLatitude.Enabled = false;
                }
            }
            catch { }
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
            if (CboSendTransaction.Text == "Cancel (Reversal)" | CboSendTransaction.Text == "Cancel")
                GrpCancel.Visible = true;
            else
                GrpCancel.Visible = false;
        }

        private void CboTrackChoice_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CboTrackChoice.Text == "Track2")
            {
                CboEntryMode.SelectedItem = EntryModeType.track2;
                CboEntryMode.BackColor = Color.FromArgb(252, 209, 5);
            }
            if (CboTrackChoice.Text == "Track1")
            {
                CboEntryMode.SelectedItem = EntryModeType.track1;
                CboEntryMode.BackColor = Color.FromArgb(252, 209, 5);
            }
            StarteTimer();
        }

        private void CboPWSorVDP_SelectedIndexChanged(object sender, EventArgs e)
        {
            //TxtAPIKey.Enabled = false;
            //TxtUserName.Enabled = false;
            //TxtPassword.Enabled = false;
            grp_VDP.Enabled = false;
            Grp_PWS.Enabled = false;
            CboSendTransaction.SelectedIndex = -1;

            if (CboPWSorVDP.Text == "VDP")
                grp_VDP.Enabled = true;
            else if (CboPWSorVDP.Text == "PWS")
            {
                Grp_PWS.Enabled = true;
            }
            SetSendTransactionValues();//Set the appropriate values based on API
        }

        private void SetSendTransactionValues()
        {
            CboSendTransaction.Items.Clear();

            //Although most transaction type names are the same, some are slightly different. The following changes the name to make things easier.
            if (CboPWSorVDP.Text == "VDP")
            {
                CboSendTransaction.Items.Add(new item("Authorization", "Authorize"));// (Authorization) (AVSOnly 0.00)
                CboSendTransaction.Items.Add(new item("AuthorizationCompletion", "Capture"));// (AuthorizationCompletion)
                CboSendTransaction.Items.Add(new item("Sale", "Purchase"));// (Sale) (Voice Auth)
                CboSendTransaction.Items.Add(new item("Adjust", "Adjust"));
                CboSendTransaction.Items.Add(new item("Return", "Refund"));//(Return)
                CboSendTransaction.Items.Add(new item("Cancel (Reversal)", "Cancel"));
                CboSendTransaction.Items.Add(new item("Void", "Void"));
                CboSendTransaction.Items.Add(new item("Close Batch", "CloseBatch"));
                //CboSendTransaction.Items.Add(new item("Tokenize", "Tokenize"));
                CboSendTransaction.Items.Add(new item("Activate (Gift)", "Activate"));
                CboSendTransaction.Items.Add(new item("Balance Inquiry (Gift)", "BalanceInquiry"));
                CboSendTransaction.Items.Add(new item("Batch Balance", "BatchBalance"));
                CboSendTransaction.Items.Add(new item("Reload (Gift)", "Reload"));
                CboSendTransaction.Items.Add(new item("Unload (Gift)", "Unload"));
                CboSendTransaction.Items.Add(new item("Close (Gift)", "Close"));
                //CboSendTransaction.Items.Add(new item("Update Card", "UpdateCard"));
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                CboSendTransaction.Items.Add(new item("Authorize", "Authorize"));// (Authorization) (AVSOnly 0.00)
                CboSendTransaction.Items.Add(new item("Capture", "Capture"));// (AuthorizationCompletion)
                CboSendTransaction.Items.Add(new item("Purchase", "Purchase"));// (Sale) (Voice Auth)
                CboSendTransaction.Items.Add(new item("Adjust", "Adjust"));
                CboSendTransaction.Items.Add(new item("Refund", "Refund"));//(Return)
                CboSendTransaction.Items.Add(new item("Cancel", "Cancel"));
                //CboSendTransaction.Items.Add(new item("Void (VDP only)", "Void"));
                CboSendTransaction.Items.Add(new item("Close Batch", "CloseBatch"));
                CboSendTransaction.Items.Add(new item("Tokenize", "Tokenize"));
                CboSendTransaction.Items.Add(new item("Activate (Gift)", "Activate"));
                CboSendTransaction.Items.Add(new item("Balance Inquiry (Gift)", "BalanceInquiry"));
                CboSendTransaction.Items.Add(new item("Batch Balance (Gift)", "BatchBalance"));
                CboSendTransaction.Items.Add(new item("Reload (Gift)", "Reload"));
                CboSendTransaction.Items.Add(new item("Unload (Gift)", "Unload"));
                CboSendTransaction.Items.Add(new item("Close (Gift)", "Close"));
                //CboSendTransaction.Items.Add(new item("Update Card", "UpdateCard"));
            }
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

        public CancelTransactionType CancelTransactionTypeFromRequest(string _TxnRequestType)
        {
            CancelTransactionType ctt = new CancelTransactionType();
            if (_TxnRequestType == "AuthorizeRequest" | _TxnRequestType == "Authorize")
                return CancelTransactionType.authorize;
            if (_TxnRequestType == "CaptureRequest" | _TxnRequestType == "Capture" | _TxnRequestType == "AuthorizationCompletion")
                return CancelTransactionType.capture;
            if (_TxnRequestType == "PurchaseRequest" | _TxnRequestType == "Purchase" | _TxnRequestType == "Sale")
                return CancelTransactionType.purchase;
            //if (_TxnRequestType == "PurchaseRequest")
            //    return CancelTransactionType.purchase_cashback;
            if (_TxnRequestType == "AdjustRequest" | _TxnRequestType == "Adjust")
                return CancelTransactionType.adjust;
            if (_TxnRequestType == "RefundRequest" | _TxnRequestType == "Refund" | _TxnRequestType == "Return")
                return CancelTransactionType.refund;
            if (_TxnRequestType == "ActivateRequest" | _TxnRequestType == "Activate")
                return CancelTransactionType.activate;
            if (_TxnRequestType == "UnloadRequest" | _TxnRequestType == "Unload")
                return CancelTransactionType.unload;
            if (_TxnRequestType == "ReloadRequest" | _TxnRequestType == "Reload")
                return CancelTransactionType.reload;
            if (_TxnRequestType == "CloseRequest" | _TxnRequestType == "Close")
                return CancelTransactionType.close;

            MessageBox.Show("Unable to lookup CancelTransactionType. Setting to default of Authorize");
            return CancelTransactionType.authorize;
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

        public string ExtractPANFromTrack(string strTrack)
        {//Sentinels should be removed as well as seperators
            string pan = "";
            try 
            {
                if(strTrack.Substring(0,1) == "B")
                {//Track 1 Data
                    pan = strTrack.Substring(1, strTrack.IndexOf("^") - 1);
                }
                else
                {//Track 2 Data
                    pan = strTrack.Substring(0, strTrack.IndexOf("="));
                }
            }
            catch { }
            return pan;
        }

        public Image ImageFromBase64String(string base64)
        {
            MemoryStream memory = new MemoryStream(Convert.FromBase64String(base64));
            Image result = Image.FromStream(memory);
            memory.Close();
            return result;
        }

        public void SetTestTrackData()
        {
            //In general the industry preference for processing transactions is Track2 followed by Track1 followed by Keyed
            if (CboCardType.Text == "visa")
            {
                if (CboTrackChoice.Text == "Track2")
                {
                    TxtTrackData.Text = "4445222299990007=17121010000023700000";
                    CboEntryMode.SelectedItem = EntryModeType.track2;
                }
                else if (CboTrackChoice.Text == "Track1")
                {
                    TxtTrackData.Text = "B4445222299990007^TESTCARD/TEST^171210100000000000237000000";
                    CboEntryMode.SelectedItem = EntryModeType.track1;
                }
            }
            else if (CboCardType.Text == "masterCard")
            {
                if (CboTrackChoice.Text == "Track2")
                {
                    TxtTrackData.Text = "5444009999222205=14121010000071700";
                    CboEntryMode.SelectedItem = EntryModeType.track2;
                }
                else if (CboTrackChoice.Text == "Track1")
                {
                    TxtTrackData.Text = "B5444009999222205^TEST-VOID/TEST^141210100000000000000071700";
                    CboEntryMode.SelectedItem = EntryModeType.track1;
                }
            }
            else if (CboCardType.Text == "amex")
            {
                if (CboTrackChoice.Text == "Track2")
                {
                    TxtTrackData.Text = "341111597242000=17121010000000000000";
                    CboEntryMode.SelectedItem = EntryModeType.track2;
                }
                else if (CboTrackChoice.Text == "Track1")
                {
                    TxtTrackData.Text = "B341111597242000^ISO/AMEX TEST             ^1712101000000000000000000000000";
                    CboEntryMode.SelectedItem = EntryModeType.track1;
                }
            }
            else if (CboCardType.Text == "discover")
            {
                if (CboTrackChoice.Text == "Track2")
                {
                    TxtTrackData.Text = "6011000990911111=19121010000000000000";
                    CboEntryMode.SelectedItem = EntryModeType.track2;
                }
                else if (CboTrackChoice.Text == "Track1")
                {
                    TxtTrackData.Text = "B6011000990911111^TESTCARD/DISCOVER^1912101000000000000000000000000";
                    CboEntryMode.SelectedItem = EntryModeType.track1;
                }
            }
        }

        #endregion Page Methods

        #region API Operations

        private ResponseDetails Authorize()
        {
            /*Authorize is used to reserve funding for the transaction amount, but does not request settlement.
              Note: If the merchant does not receive a response for an Auth, then the merchant should perform a 
              reversal on the Auth transaction. Also there will not be any reversals in cancel and reversals on a reversal transaction.
            */

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.authorizationRequest(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.authorizeRequest();
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails Capture(ResponseDetails _rd)
        {
            /* Capture is used to schedule a prior authorization for settlement. 
             */

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.authorizationCompletionRequest(_rd, ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.captureRequest(_rd);
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;

        }

        private ResponseDetails Purchase()
        {
            /* Purchase is used to reserve funding for the transaction amount and requesting settlement on the merchant’s behalf.
             */

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.saleRequest(ref vdp);                 
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.purchaseRequest();
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;

        }

        private ResponseDetails Adjust(ResponseDetails _rd)
        {
            //Adjust is used to modify a previous transaction, prior to settlement. Credit card only. Litle Does not support this transaction.

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.adjustRequest(_rd, ref vdp);                 
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.adjustRequest(_rd); 
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails Return()
        {
            /* Return aka Refund is used to transfer funds from the merchant back to the cardholder. 
             */

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.returnRequest(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.refundRequest(); 
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
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

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.reversalRequest(_rd, ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.cancelRequest(_rd); 
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;

        }

        private ResponseDetails Void(ResponseDetails _rd)
        {
            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.voidRequest(_rd, ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                MessageBox.Show("PWS uses the Cancel operation to void transactions. Please change your selection to Cancel to perform a full reversal type.");
                return null;
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;

        }

        private ResponseDetails CloseBatch()
        {
            /*Close Batch is used to close out the transaction batch for the day. The close batch operation is 
              used primarily by businesses that need to manually start end of day processing, typically restaurants.
            */

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.batchClose(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                MessageBox.Show("Sample code is currently not available. Please contact your solution consultant for help.");
                return null;
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails Tokenize()
        {
            /*Tokenize is used to return a token for a card so that it may be used in future transactions. 
             */

            if (CboPWSorVDP.Text == "VDP")
            {
                MessageBox.Show("Sample code is currently not available. Please contact your solution consultant for help.");
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.tokenizeRequest(); 
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;

        }

        private ResponseDetails Activate()
        {
            /*Activate is used to load an initial balance on a gift card or to create a Virtual Gift Card.
            */
            /*Tokenize is used to return a token for a card so that it may be used in future transactions. 
            */

            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.activateRequest(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.activateRequest();
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails Reload()
        {
            /*Reload is used to add an additional amount to a gift card. Gift card only.
            */
            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.reloadRequest(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.reloadRequest();
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails Unload()
        {
            /*Unload is used to remove the remaining balance on a gift card. Gift card only
            */
            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.unloadRequest(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.unloadRequest();
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails Close()
        {
            /*Close is used to finalize a gift card with no further transactions allowed.
            */
            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.closeRequest(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.closeRequest();
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails BalanceInquiry()
        {
            /*Balance Inquiry is used to retrieve the balance on a gift card or a prepaid credit card.
            */
            if (CboPWSorVDP.Text == "VDP")
            {
                VantivDeveloperPortal vdp = new VantivDeveloperPortal();
                return VDP_Helper.balanceInquiryRequest(ref vdp);
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                return PWS_Helper.balanceInquiryRequest(); 
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails BatchBalance()
        {
            /*Batch Balance is used to retrieve a summary for a batch. There are two types of Batch Balance requests, 
              GIFT and CREDIT which are designated by setting the PaymentInstrumentType.
            */

            if (CboPWSorVDP.Text == "VDP")
            {
                MessageBox.Show("Sample code is currently not available. Please contact your solution consultant for help.");
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                MessageBox.Show("Sample code is currently not available. Please contact your solution consultant for help.");
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        private ResponseDetails UpdateCard()
        {
            /*Retrieve a summary of Returns and Sales for all the transactions in the current batch for prepaid credit card 
            or summary of Returns and Sales and, details grouped by following transaction types for gift cards:
            */

            if (CboPWSorVDP.Text == "VDP")
            {
                MessageBox.Show("Sample code is currently not available. Please contact your solution consultant for help.");
            }
            else if (CboPWSorVDP.Text == "PWS")
            {
                MessageBox.Show("Sample code is currently not available. Please contact your solution consultant for help.");
            }
            else
            {
                MessageBox.Show("Please select an integration path before proceeding");
                TbControl.SelectedTab = TbAbout;
            }
            return null;
        }

        #endregion API Operations

        #region Links to Online Resources

        private void lnkVDPOnlineDocumentation_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://apideveloper.vantiv.com/documentation");
        }
        private void LnkVDPForums_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://apideveloper.vantiv.com/forum");
        }
        private void LnkVDPGetHelp_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://apideveloper.vantiv.com/help");
        }
        private void LnkContactUs_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://apideveloper.vantiv.com/content/contact");
        }
        private void LnkNotSure_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("");
        }
        private void LnkLicenseKey_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("");
        }
        private void LnkVDPFeatures_Click_1(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://apideveloper.vantiv.com/docs/payment-web-services/getting-started/platform-feature-overview");
        }
        private void LnkVDPEndpointURL_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://apideveloper.vantiv.com/docs/payment-web-services/endpoint-reference");
        }
        private void LnkPWSUser_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("");
        }
        private void LnkPWSPassword_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("");
        }
        private void LnkPWSEndpointURL_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("");
        }
        private void LnkSampleCode_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(@"https://github.com/vantiv");
        }

        #endregion Links to Online Resources

    }

    public class ResponseDetails
    {
        /*The following class is used by both PWS and VDP as a way to demonstrate the data that may be saved in the database.
         * The developer should be familiar with data needed to perform follow-on transaction and ensure at a minimum they have that 
         * data in their database. They may also wish to record other meta-data that meets their application needs. 
         */ 
         /* *** PCI Considerations ***
         * The developer also  needs to follow PCI data standards in terms of the data they save in their database. For example PCI 
         * does not permit Track data nor CV data to be saved in any format in a database. It's the software companys responsiblity to 
         * build a solution that follows PCI standards for more information please reference https://www.pcisecuritystandards.org/ 
         * or an assesor for guidance.
         */
        //Common Meta Data
        public string VantivInterface;
        public AmountType Amount;
        public string AuthorizationCode;
        public string PaymentInstrumentType;
        public string TxnRequestType;
        //VDP Transaction Summary
        public VDP_TransactionSummary VDP_TxnSummary;
        //PWS Transaction Summary
        public PWS_TransactionSummary PWS_TxnSummary;

        public ResponseDetails(string vantivInterface, AmountType amount, string authorizationCode, string paymentInstrumentType,
                                string txnRequestType, VDP_TransactionSummary vDP_TxnSummary, PWS_TransactionSummary pWS_TxnSummary)
        {
            VantivInterface = vantivInterface;
            Amount = amount;
            AuthorizationCode = authorizationCode;
            PaymentInstrumentType = paymentInstrumentType;
            TxnRequestType = txnRequestType;
            VDP_TxnSummary = vDP_TxnSummary;
            PWS_TxnSummary = pWS_TxnSummary;
        }
        public override string ToString()
        {// Generates the text shown in the List Checkbox
            try
            {
                string info = "[" + VantivInterface + "] " + PaymentInstrumentType + " ";

                if (VDP_TxnSummary != null)
                {//VDP transaction 
                    if (VDP_TxnSummary.XMLResponse != null)
                    {
                        info += Amount.Value.ToString() + " " + TxnRequestType + " (AuthorizationCode: " + AuthorizationCode + ")";
                        info += " [" + DateTime.Now + "]";
                        info += " RequestId: " + VDP_Helper.SELECT(VDP_TxnSummary.XMLResponse, "//RequestId");
                    }
                    else if (VDP_TxnSummary.JsonRequest != null)
                        info += "TRANSACTION FAILURE " + TxnRequestType;
                }
                else if (PWS_TxnSummary != null)
                {//PWS transaction 
                    if (PWS_TxnSummary.Response != null)
                    {
                        info += Amount.Value.ToString() + " " + (((TransactionResponseType)(PWS_TxnSummary.Response)).GetType()).ToString().Replace("Response", "");

                        if (((TransactionResponseType)(PWS_TxnSummary.Response)).Items != null && ((TransactionResponseType)(PWS_TxnSummary.Response)).Items.Count() > 0)
                        {
                            info += " (";
                            int idx = 0;
                            while (idx < ((TransactionResponseType)(PWS_TxnSummary.Response)).Items.Count())
                            {
                                info += ((TransactionResponseType)(PWS_TxnSummary.Response)).ItemsElementName[idx] + ": " + ((TransactionResponseType)(PWS_TxnSummary.Response)).Items[idx] + " ";
                                if (((TransactionResponseType)(PWS_TxnSummary.Response)).TokenizationResult != null && ((TransactionResponseType)(PWS_TxnSummary.Response)).TokenizationResult.successful)
                                    info += "TOKEN";
                                idx++;
                            }
                            info += ") ";
                            info += " [" + DateTime.Now + "]";
                            info += " RequestId: " + ((TransactionResponseType)(PWS_TxnSummary.Response)).RequestId;
                        }
                    }
                    else if (PWS_TxnSummary.PWSRequest != null)
                        info += "TRANSACTION FAILURE " + PWS_TxnSummary.PWSRequest.GetType().ToString().Replace("Request", "");
                }
                else
                { 
                    info += "TRANSACTION FAILURE";
                }

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

    //public class TestScenario
    //{
    //    public string TestNumber;
    //    public Transaction.Response TestResponse;
    //    public Services.Response SvcResponse;

    //    public TestScenario(string testNumber, Transaction.Response testResponse, Services.Response svcResponse)
    //    {
    //        TestNumber = testNumber;
    //        TestResponse = testResponse;
    //        SvcResponse = svcResponse;
    //    }
    //    public override string ToString()
    //    {
    //        // Generates the text shown in the combo box
    //        return TestNumber;
    //    }
    //}

}
