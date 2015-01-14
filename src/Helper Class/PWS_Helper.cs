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
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using CertSuiteTool_VDP;
using System.Net;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace CertSuiteTool.Helper_Class
{
    class PWS_Helper
    {
        #region Variable Declarations
        private static Send_Transactions target;
        #endregion Variable Declarations

        public PWS_Helper(Send_Transactions _myForm)
        {
            target = _myForm;//Setup access to the form fields
        }

        #region Objects

        private static MerchantType merchantType()
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

                //Merchant Detail Section
                mt.CashierNumber = Convert.ToInt32(target.TxtCashierNumber.Text); //8 digit numeric value to represent the cashier entering the transaction	Optional
                if (target.TxtCashierNumber.Text.Length > 0)
                    mt.CashierNumberSpecified = true;
                mt.ChainCode = target.TxtChainCode.Text; //NOTE: *DB* I believe documentation is incorrect (ChainNumber) : 5 character alphanumeric value to represent the company’s chain where the transaction was entered. If provided on the VAR sheet the value provided should be used	Conditional, use if provided
                mt.ClerkNumber = Convert.ToInt32(target.TxtClerkNumber.Text);//3 digit value to represent the clerk entering the transaction	Mandatory
                if (target.TxtClerkNumber.Text.Length > 0)
                    mt.ClerkNumberSpecified = true;
                mt.DivisionNumber = target.TxtDivisionNumber.Text; //3 character alphanumeric value to represent the company’s division where the transaction was entered. The default value is “001”	Optional,use if Division level reporting is required 
                mt.LaneNumber = target.TxtLaneNumber.Text; //3 character alpha numeric value to represent which lane that the transaction was entered.  Used for multi threading of transactions.	Conditional, should be 2 digit number, 0-99.  Used for multi threading transactions in host capture environment
                mt.MerchantId = target.TxtMID.Text;//"4445000865113"; //Identifying ID set up during the boarding process. It can be up to 36 digits.  Also known as MID or merchant account.	Mandatory
                mt.MerchantName = target.TxtMerchantName.Text; //15 character string value used to send the name of the bill payment acquiring merchant in order for customer to see the merchant name in debit statement.	Conditional-used for PIN-less Debit transactions
                mt.NetworkRouting = target.TxtNetworkRouting.Text; //2 character value used to send the transaction to the proper credit network	Mandatory
                mt.StoreNumber = target.TxtStoreNumber.Text; //8 character alphanumeric value to represent the company’s store number where the transaction was entered. The default value is “00000001”	Optional

                //Terminal Detail Section (3 options)
                if (target.CboTerminalDetail.Text == "Terminal")//Options "Terminal", "Software", "Mobile"
                    mt.Terminal = paymentDeviceType();
                else if (target.CboTerminalDetail.Text == "Software")
                    mt.Software = paymentDeviceType();
                else if (target.CboTerminalDetail.Text == "Mobile")
                    mt.Mobile = mobileDeviceType();

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
        private static PaymentDeviceType paymentDeviceType()
        {
            PaymentDeviceType pdt = new PaymentDeviceType();
            Random r = new Random();

            pdt.BalanceInquiry = (bool)target.CboBalanceInquiry.SelectedItem; //Boolean value (true, false) to determine if balance inquiry fields will be returned. The default value is false.	Optional – Must be set to true for card present transactions so that prepaid cards can be supported.  We recommend leaving as false for card not present transactions.
            pdt.CardReader = (CardReaderType)target.CboCardReaderType.SelectedItem; //An optional value to identify the type of card reader used in the transaction. Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated may cause interchange qualification impact.
            pdt.Classification = (TerminalClassificationType)target.CboTerminalEnvironmentalCode.SelectedItem; //Identifies the type of device used in the transaction. Should be changed from the default values to reflect true entry method and point of entry environment.  Not updating from the default may have interchange qualification impact.  Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
            pdt.EntryMode = (EntryModeType)target.CboEntryMode.SelectedItem; //Required value to identify the type of terminal entry used in the transaction. Your choices are: Mandatory – Value must reflect entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
            pdt.HostAdjustment = (bool)target.CboHostAdjustment.SelectedItem; //Boolean value (true, false) to determine if adjust transaction will be allowed for a batch. The default value is false.	Optional – if need to adjust a transaction amount (ex. Add tip, add level 2 data) set to true.  Flag setting should be consistent across all transaction types for a given application. Note – Setting HostAdjustment to true will disable batch auto close functionality.  The host will not auto close a batch if HostAdjust is set to true.
            pdt.IPv4Address = "192.0.2.235"; //The IPv4Address of the calling transaction. The format is 4 sets of up to 3 digits separated by “.”. An example would be “127.0.0.0”. Either IPv4Address or IPv6Address may be entered.	Optional
            //pdt.IPv6Address = ""; //The IPv6Adresss of t he calling transaction. It consists of eight groups of four hexadecimal digits separated by colons, for example 2001:0db8:85a3:0042:0000:8a2e:0370:7334. Either IPv4Address or IPv6Address may be entered.	Optional
            pdt.PinEntry = (PinEntryType)target.CboPinEntry.SelectedItem; //An optional value to identify the type of pin entry used by this terminal. Optional – Defaults must be changed to reflect actual entry method and point of entry environment. Mandatory for PIN Debit Transactions, should always be set to “supported” 
            pdt.SequenceNumber = r.Next(100000, 999999).ToString(); //6 digit value to identify this transaction. The default is 0, but if used, it should be unique for each transaction within a 24 hour period. We recommend using a counter or hour, minute, second (hhmmss), using military time.	Mandatory, also called STAN (System Audit Trace Number)
            pdt.TerminalID = Convert.ToInt16(target.TxtTID.Text); //3 digit value that identifies the terminal used. The terminal ID will be provided on the VAR sheet.  Also known as the TID	Mandatory

            return pdt;
        }
        //or
        private static MobileDeviceType mobileDeviceType()
        {
            MobileDeviceType mdt = new MobileDeviceType();

            //Similar to Terminal
            mdt.BalanceInquiry = (bool)target.CboBalanceInquiry.SelectedItem; //Boolean value (true, false) to determine if balance inquiry fields will be returned. The default value is false.	Optional – Must be set to true for card present transactions so that prepaid cards can be supported.  We recommend leaving as false for card not present transactions.
            mdt.CardReader = (CardReaderType)target.CboCardReaderType.SelectedItem; //An optional value to identify the type of card reader used in the transaction. Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated may cause interchange qualification impact.
            mdt.Classification = (TerminalClassificationType)target.CboTerminalEnvironmentalCode.SelectedItem; //Identifies the type of device used in the transaction. Should be changed from the default values to reflect true entry method and point of entry environment.  Not updating from the default may have interchange qualification impact.  Optional – Defaults must be changed to reflect actual entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
            mdt.EntryMode = (EntryModeType)target.CboEntryMode.SelectedItem; //Required value to identify the type of terminal entry used in the transaction. Your choices are: Mandatory – Value must reflect entry method and point of entry environment.  If not updated, may cause interchange qualification impact.
            mdt.HostAdjustment = (bool)target.CboHostAdjustment.SelectedItem; //Boolean value (true, false) to determine if adjust transaction will be allowed for a batch. The default value is false.	Optional – if need to adjust a transaction amount (ex. Add tip, add level 2 data) set to true.  Flag setting should be consistent across all transaction types for a given application. Note – Setting HostAdjustment to true will disable batch auto close functionality.  The host will not auto close a batch if HostAdjust is set to true.
            mdt.IPv4Address = "192.0.2.235"; //The IPv4Address of the calling transaction. The format is 4 sets of up to 3 digits separated by “.”. An example would be “127.0.0.0”. Either IPv4Address or IPv6Address may be entered.	Optional
            //mdt.IPv6Address = ""; //The IPv6Adresss of t he calling transaction. It consists of eight groups of four hexadecimal digits separated by colons, for example 2001:0db8:85a3:0042:0000:8a2e:0370:7334. Either IPv4Address or IPv6Address may be entered.	Optional
            mdt.Location = new GeolocationType();
            mdt.Location.Longitude = Convert.ToDecimal(target.TxtLongitude.Text);
            mdt.Location.Latitude = Convert.ToDecimal(target.TxtLatitude.Text);
            mdt.PinEntry = (PinEntryType)target.CboPinEntry.SelectedItem; //An optional value to identify the type of pin entry used by this terminal. Optional – Defaults must be changed to reflect actual entry method and point of entry environment. Mandatory for PIN Debit Transactions, should always be set to “supported” 
            mdt.SequenceNumber = "123"; //6 digit value to identify this transaction. The default is 0, but if used, it should be unique for each transaction within a 24 hour period. We recommend using a counter or hour, minute, second (hhmmss), using military time.	Mandatory, also called STAN (System Audit Trace Number)
            mdt.TerminalID = Convert.ToInt16(target.TxtTID.Text); //3 digit value that identifies the terminal used. The terminal ID will be provided on the VAR sheet.  Also known as the TID	Mandatory

            return mdt;
        }
        #endregion Terminal Detail

        //Payment Instruments Credit, Debit, Gift
        #region Payment Instruments

        private static CreditInstrumentType creditInstrumentType()
        {
            CreditInstrumentType cit = new CreditInstrumentType();
            cit.CardholderAddress = addressType();

            if (target.CboCreditType.Text == "CardKeyed")
            {
                cit.CardKeyed = creditOrDebitCardKeyedType();
            }
            else if (target.CboCreditType.Text == "CardSwiped")
            {
                cit.CardSwiped = cardSwipedType();
            }

            cit.CardType = (CreditCardNetworkType)target.CboCardType.SelectedItem;
            if (target.CboPartialApprovalCode.Text.Length > 0)
            {
                cit.PartialApprovalCode = (PartialIndicatorType)target.CboPartialApprovalCode.SelectedItem;//Definition: Type of partial approval to return. If not_supported is selected, partial approvals will fail. Partial approvals must be supported by any application conducting card present transactions, per Visa and MasterCard regulations. We recommend not supporting partial approvals in applications conducted in Card Not Present environments, as split tender scenarios become complicated.
                cit.PartialApprovalCodeSpecified = true;
            }

            return cit;
        }

        private static DebitInstrumentType debitInstrumentType()
        {
            DebitInstrumentType dit = new DebitInstrumentType();

            dit.AccountType = (AccountType)target.CboAccountType.SelectedItem;//Optional for PIN debit balance inquiry action. If left out, the financial institutions default will be populated.
            dit.AccountTypeSpecified = true;
            dit.BenefitTransactionNumber = "1234";
            dit.CardholderAddress = addressType();

            if (target.CboCreditType.Text == "CardKeyed")
            {
                dit.CardKeyed = creditOrDebitCardKeyedType();
            }
            else if (target.CboCreditType.Text == "CardSwiped")
            {
                dit.CardSwiped = cardSwipedType();
            }

            if (target.CboPartialApprovalCode.Text.Length > 0)
            {
                dit.PartialApprovalCode = (PartialIndicatorType)target.CboPartialApprovalCode.SelectedItem;//Definition: Type of partial approval to return. If not_supported is selected, partial approvals will fail. Partial approvals must be supported by any application conducting card present transactions, per Visa and MasterCard regulations. We recommend not supporting partial approvals in applications conducted in Card Not Present environments, as split tender scenarios become complicated.
                dit.PartialApprovalCodeSpecified = true;
            }
            dit.PinData = new EncryptedData();//Mandatory for PIN Debit Transactions
            dit.PinData.encryptiontype = EncryptionType.DUKPT;//DUKPT (default), Voltage
            dit.PinData.key = "";//The key used to encrypt the PIN data. This field is not sent as part of the request if the encryption-type is Voltage. Voltage Encryption Transfer data (Encrypted Symmetric Key). This field is not sent as part of the request if the encryption-type is DUKPT.
            dit.PinData.Value = "";
            dit.VoucherNumber = "";

            return dit;
        }

        private static GiftInstrumentType giftInstrumentType()
        {
            GiftInstrumentType git = new GiftInstrumentType();

            if (target.CboCreditType.Text == "CardKeyed")
            {
                git.CardKeyed = new GiftCardKeyedType();
                git.CardKeyed.EncryptedPrimaryAccountNumber = new EncryptedData();
                //git.CardKeyed.EncryptedPrimaryAccountNumber.encryptiontype = EncryptionType.DUKPT;
                //git.CardKeyed.EncryptedPrimaryAccountNumber.key = "";
                //git.CardKeyed.EncryptedPrimaryAccountNumber.Value = "";
                git.CardKeyed.ExpirationDate = target.TxtExpirationDate.Text;
                if (target.TxtGiftCardPin.Text.Length > 0)
                    git.CardKeyed.GiftCardPin = target.TxtGiftCardPin.Text;
                git.CardKeyed.PrimaryAccountNumber = target.TxtPrimaryAccountNumber.Text;
                if (target.TxtCardSecurityCode.Text.Length > 0)
                    git.CardKeyed.CardSecurityCode = target.TxtCardSecurityCode.Text;

                if (target.ChkUseToken.Checked)
                {
                    git.CardKeyed.Token = new TokenType();
                    git.CardKeyed.Token.tokenId = "";
                    git.CardKeyed.Token.tokenValue = "";
                }

            }
            else if (target.CboCreditType.Text == "CardSwiped")
            {
                git.CardSwiped = new GiftCardSwipedType();
                if (target.TxtGiftCardPin.Text.Length > 0)
                    git.CardSwiped.GiftCardPin = target.TxtGiftCardPin.Text;
                //git.CardSwiped.Item
                //git.CardSwiped.ItemElementName = "";
                if (target.TxtCardSecurityCode.Text.Length > 0)
                    git.CardSwiped.CardSecurityCode = target.TxtCardSecurityCode.Text;
            }

            return git;
        }

        private static VirtualGiftInstrumentType virtualGiftInstrumentType()
        {
            VirtualGiftInstrumentType vgt = new VirtualGiftInstrumentType();
            vgt.PrimaryAccountNumberLength = 2;
            vgt.VirtualGiftCardBIN = "";
            return vgt;
        }

        #region objects used by instrument types

        private static AddressType addressType()
        {
            AddressType at = new AddressType();//Used for Address Verification service to verify the owner of the card to reduce the risk of fraudulent transactions (for Credit and Debit Cards)
            if (target.ChkSetAddressInformation.Checked)
            {
                at.AddressLine = target.TxtAddressLine.Text;
                at.City = target.TxtCity.Text;
                at.CountryCode = (ISO3166CountryCodeType)target.CboCountryCode.SelectedItem;
                at.CountryCodeSpecified = true;
                at.PostalCode = target.TxtPostalCode.Text;
                at.State = (StateCodeType)target.CboStateType.SelectedItem;
                at.StateSpecified = true;
            }
            else
            {
                at = null;
            }
            return at;
        }

        private static CreditOrDebitCardKeyedType creditOrDebitCardKeyedType()
        {
            CreditOrDebitCardKeyedType ckt = new CreditOrDebitCardKeyedType();

            ckt.CardholderName = target.TxtCardholderName.Text;
            if (target.TxtCardSecurityCode.Text.Length > 0) 
                ckt.CardSecurityCode = target.TxtCardSecurityCode.Text;
            //ckt.EncryptedPrimaryAccountNumber = "";
            ckt.ExpirationDate = target.TxtExpirationDate.Text;
            ckt.PrimaryAccountNumber = target.TxtPrimaryAccountNumber.Text;
            //ckt.ThreeDSecure = new ThreeDSecureType();
            //ckt.ThreeDSecure.AuthenticationValue = "";
            //ckt.ThreeDSecure.eCommerceIndicator = "";
            //ckt.ThreeDSecure.TransactionID = "";
            if (target.ChkUseToken.Checked)
            {
                ckt.Token = new TokenType();
                ckt.Token.tokenId = target.TxtTokenId.Text;
                ckt.Token.tokenValue = target.TxtTokenValue.Text;
                ckt.PrimaryAccountNumber = null; //Per the schema if Token is set PAN should not be sent.
                ckt.EncryptedPrimaryAccountNumber = null; //Per the schema if Token is set EncryptedPrimaryAccountNumber should not be sent.
            }

            return ckt;
        }

        private static CardSwipedType cardSwipedType()
        {
            CardSwipedType st = new CardSwipedType();

            st.Item = new TrackDataType();

            if (target.ChkEncryptedData.Checked)
            {
                EncryptedData ed = new EncryptedData();
                ed.encryptiontype = EncryptionType.DUKPT;
                ed.key = target.TxtKeySerialNumber.Text;
                ed.Value = target.TxtTrackData.Text;
                st.Item.Item = ed;
            }
            else
            {
                st.Item.Item = target.TxtTrackData.Text;
            }
            st.ItemElementName = (ItemChoiceType)target.CboTrackChoice.SelectedItem;

            return st;
        }

        #endregion objects used by instrument types

        #endregion Payment Instruments

        private static BillPaymentPayeeType billPaymentPayeeType()
        {
            BillPaymentPayeeType bppt = new BillPaymentPayeeType();

            bppt.PayeeAccountNumber = "1234567890123456789012345"; //25 character string value to pass Account number Payee uses to identify the payer.	Conditional – PIN-less Debit Purchase Only
            bppt.PayeeName = "John Denver"; //25 character string value used to pass Payee name related to an online bill payment using debit card	Conditional – PIN-less Debit Purchase Only
            bppt.PayeePhoneNumber = "513-555-5555"; //25 character string value to pass Payee phone number related to an online bill payment using debit card. Example: 513-555-5555	Conditional – PIN-less Debit Purchase Only

            return bppt;
        }

        #endregion Objects

        #region Transaction Processing

        public static ResponseDetails authorizeRequest()
        {
            AuthorizeRequest a = new AuthorizeRequest();

            Random r = new Random();
            //a.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
            //a.BillPaymentPayee.PayeeAccountNumber = "";
            //a.BillPaymentPayee.PayeeName = "";
            //a.BillPaymentPayee.PayeePhoneNumber = "";
            a.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            a.Merchant = merchantType();
            //a.NetworkResponseCode = "";
            a.PaymentType = (PaymentType)target.CboPaymentType.SelectedItem;//Mandatory
            a.PaymentTypeSpecified = true;
            a.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
            //a.reportgroup = ""; //An optional (required for Litle) attribute used by the merchant to map each transaction to a reporting category.  This can be no longer than 25 characters. 
            a.TokenRequested = target.ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
            a.TokenRequestedSpecified = target.ChkTokenRequested.Checked;
            a.TransactionAmount = new AmountType();
            a.TransactionAmount.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
            a.TransactionAmount.currencySpecified = true;
            a.TransactionAmount.Value = Convert.ToDecimal(target.TxtTransactionAmount.Text);
            a.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
            a.TransactionType = (TransactionTypeType)target.CboTransactionType.SelectedItem;//Mandatory
            int rInt = r.Next(1, 99999); //for ints
            a.systemtraceid = rInt; //A conditional ID used to track each transaction. This must be an integer. Required for Raft and Tandem, optional for Litle? Required for Litle on CancelRequest.
            a.systemtraceidSpecified = true;
            rInt = r.Next(1, 99999999);
            a.merchantrefid = "PWS" + rInt.ToString(); //An optional attribute used by the merchant to identify each transaction. This can be no longer than 16 characters. If the merchant chooses not to use this field it is recommended that you populate this ID with the system-trace-id value.

            //Set the object for payment
            a.Items = new object[1];
            if (target.CboPaymentInstrument.Text == "Credit")
                a.Items[0] = creditInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Debit")
                a.Items[0] = debitInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Gift")
                a.Items[0] = giftInstrumentType();

            PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try 
            {
                ts = new PWS_TransactionSummary(a, target.PWSClient.Authorize(a));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            
            return ProcessResponse(ts);
        }

        public static ResponseDetails purchaseRequest()
        {
            PurchaseRequest p = new PurchaseRequest();

            Random r = new Random();
            p.Merchant = merchantType();
            p.TransactionType = (TransactionTypeType)target.CboTransactionType.SelectedItem;//Mandatory
            p.PaymentType = (PaymentType)target.CboPaymentType.SelectedItem;//Mandatory
            p.PaymentTypeSpecified = true;
            p.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            p.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
            p.TransactionAmount = new AmountType();
            p.TransactionAmount.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
            p.TransactionAmount.currencySpecified = true;
            p.TransactionAmount.Value = Convert.ToDecimal(target.TxtTransactionAmount.Text);
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
            if (target.CboPaymentInstrument.Text == "Credit")
                p.Items[0] = creditInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Debit")
                p.Items[0] = debitInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Gift")
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

            PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try
            {
                ts = new PWS_TransactionSummary(p, target.PWSClient.Purchase(p));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            return ProcessResponse(ts);
        }

        public static ResponseDetails captureRequest(ResponseDetails _rd)
        {
            CaptureRequest c = new CaptureRequest();

            Random ran = new Random();
            c.Merchant = merchantType();
            c.TransactionType = (TransactionTypeType)target.CboTransactionType.SelectedItem;//Mandatory
            c.PaymentType = (PaymentType)target.CboPaymentType.SelectedItem;//Mandatory
            c.PaymentTypeSpecified = true;
            c.DraftLocatorId = "D" + ran.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            c.ReferenceNumber = "R" + ran.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
            c.CaptureAmount = new AmountType();
            c.CaptureAmount.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
            c.CaptureAmount.currencySpecified = true;
            c.CaptureAmount.Value = Convert.ToDecimal(target.TxtTransactionAmount.Text);
            c.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
            c.TokenRequested = target.ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
            c.TokenRequestedSpecified = target.ChkTokenRequested.Checked;
            //a.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
            int rInt = ran.Next(1, 99999); //for ints
            c.systemtraceid = rInt;
            c.systemtraceidSpecified = true;
            rInt = ran.Next(1, 99999999);
            c.merchantrefid = "PWS" + rInt.ToString();

            //Set the object for payment
            c.Items = new object[1];
            if (target.CboPaymentInstrument.Text == "Credit")
                c.Items[0] = creditInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Debit")
                c.Items[0] = debitInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Gift")
                c.Items[0] = giftInstrumentType();

            //Set the Capture specific values
            AuthorizeResponse r = new AuthorizeResponse();
            r = (AuthorizeResponse)_rd.PWS_TxnSummary.Response;

            c.AuthorizationCode = _rd.AuthorizationCode;
            c.OriginalAmount = _rd.Amount;
            AmountType a = new AmountType();
            a.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
            a.currencySpecified = true;
            a.Value = Convert.ToDecimal(target.TxtTransactionAmount.Text);
            c.CaptureAmount = a;
            c.OriginalReferenceNumber = r.ReferenceNumber;

            ItemsChoiceType4[] ict = new ItemsChoiceType4[1];
            ict[0] = ItemsChoiceType4.Credit;
            c.ItemsElementName = ict;

            PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try
            {
                ts = new PWS_TransactionSummary(c, target.PWSClient.Capture(c));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            return ProcessResponse(ts);
        }

        public static ResponseDetails adjustRequest(ResponseDetails _rd)
        {
            AdjustRequest adj = new AdjustRequest();

            Random ran = new Random();
            adj.AdjustedTotalAmount = new AmountType();
            adj.AdjustedTotalAmount.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
            adj.AdjustedTotalAmount.currencySpecified = true;
            adj.AdjustedTotalAmount.Value = Convert.ToDecimal(target.TxtTransactionAmount.Text); //The new amount, which is the original amount and the adjustment
            //adj.AuthorizationCode = "";
            //adj.BillPaymentPayee = new BillPaymentPayeeType();
            //adj.ConvenienceFee = new AmountType();
            adj.Credit = new CreditInstrumentType();
            adj.Credit = creditInstrumentType();
            adj.DraftLocatorId = "D" + ran.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            adj.Merchant = merchantType();
            adj.NetworkResponseCode = "";
            adj.PaymentType = (PaymentType)target.CboPaymentType.SelectedItem;//Mandatory
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
            adj.TokenRequested = target.ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
            adj.TokenRequestedSpecified = target.ChkTokenRequested.Checked;
            adj.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
            adj.TransactionType = (TransactionTypeType)target.CboTransactionType.SelectedItem;//Mandatory       

            PurchaseResponse r = new PurchaseResponse();
            r = (PurchaseResponse)_rd.PWS_TxnSummary.Response;

            //adj = adjustRequest(null);
            adj.AuthorizationCode = _rd.AuthorizationCode;
            adj.OriginalAmount = new AmountType();
            adj.OriginalAmount.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
            adj.OriginalAmount.currencySpecified = true;
            adj.OriginalAmount.Value = _rd.Amount.Value;
            adj.OriginalReferenceNumber = r.ReferenceNumber;

           PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try
            {
                ts = new PWS_TransactionSummary(adj, target.PWSClient.Adjust(adj));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            return ProcessResponse(ts);
        }

        public static ResponseDetails refundRequest()
        {
            RefundRequest rfnd = new RefundRequest();

            rfnd.RefundAmount = new AmountType();
            rfnd.RefundAmount.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
            rfnd.RefundAmount.currencySpecified = true;
            rfnd.RefundAmount.Value = 1.00M;

            Random r = new Random();
            rfnd.Merchant = merchantType();
            rfnd.Merchant.Terminal = null;
            rfnd.TransactionType = (TransactionTypeType)target.CboTransactionType.SelectedItem;//Mandatory
            rfnd.PaymentType = (PaymentType)target.CboPaymentType.SelectedItem;//Mandatory
            rfnd.PaymentTypeSpecified = true;
            rfnd.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            rfnd.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
            rfnd.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
            rfnd.TokenRequested = target.ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
            rfnd.TokenRequestedSpecified = target.ChkTokenRequested.Checked;
            //a.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
            int rInt = r.Next(1, 99999); //for ints
            rfnd.systemtraceid = rInt;
            rfnd.systemtraceidSpecified = true;
            rInt = r.Next(1, 99999999);
            rfnd.merchantrefid = "PWS" + rInt.ToString();

            //Set the object for payment
            rfnd.Items = new object[1];
            if (target.CboPaymentInstrument.Text == "Credit")
                rfnd.Items[0] = creditInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Debit")
                rfnd.Items[0] = debitInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Gift")
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

            PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try
            {
                ts = new PWS_TransactionSummary(rfnd, target.PWSClient.Refund(rfnd));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            return ProcessResponse(ts);
        }

        public static ResponseDetails cancelRequest(ResponseDetails _rd)
        {
            CancelRequest can = new CancelRequest();
            //if ((TransactionTypeType)CboTransactionType.SelectedItem == TransactionTypeType.ecommerce)
            //{
            Random ran = new Random();
            //can.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
            //can.BillPaymentPayee.PayeeAccountNumber = "";
            //can.BillPaymentPayee.PayeeName = "";
            //can.BillPaymentPayee.PayeePhoneNumber = "";
            can.DraftLocatorId = "D" + ran.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            can.Merchant = merchantType();
            can.Merchant.Terminal = null;
            can.PaymentType = ((TransactionRequestType)(_rd.PWS_TxnSummary.PWSRequest)).PaymentType;//Mandatory
            can.PaymentTypeSpecified = true;
            can.ReferenceNumber = "R" + ran.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
            can.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
            can.TransactionType = ((TransactionRequestType)(_rd.PWS_TxnSummary.PWSRequest)).TransactionType;//Mandatory
            can.TokenRequested = target.ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
            can.TokenRequestedSpecified = target.ChkTokenRequested.Checked;
            int rInt = ran.Next(1, 99999); //for ints
            can.systemtraceid = rInt;
            can.systemtraceidSpecified = true;
            rInt = ran.Next(1, 99999999);
            can.merchantrefid = "PWS" + rInt.ToString();
            
            //Set the object for payment
            can.Items = new object[2];
            if (target.CboPaymentInstrument.Text == "Credit")
                can.Items[0] = creditInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Debit")
                can.Items[0] = debitInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Gift")
                can.Items[0] = giftInstrumentType();

            if (target.ChkCancelReplacementAmount.Checked)
            {
                //Remaining amount of the transaction after partial cancel. If the cancel amount is less than original amount, use the replacement amount
                ReplacementAmountType rat = new ReplacementAmountType();
                rat.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
                rat.currencySpecified = true;
                rat.Value = Convert.ToDecimal(target.TxtCancelReplacementAmount.Text);

                can.Items[1] = rat;
            }

            //Check to see if this is a system or merchant cancel
            if ((ReversalReasonType)target.CboReversalReason.SelectedItem == ReversalReasonType.TIME_OUT)
            {//System
                can.CancelType = target.CancelTransactionTypeFromRequest(_rd.TxnRequestType);
                can.OriginalAmount = _rd.Amount;
                can.OriginalTransactionTimestamp = _rd.PWS_TxnSummary.PWSRequest.TransactionTimestamp;
                can.OriginalTransactionTimestampSpecified = true;
                can.OriginalSystemTraceId = _rd.PWS_TxnSummary.PWSRequest.systemtraceid;
                can.OriginalSystemTraceIdSpecified = true;
                //can.OriginalReferenceNumber = ((TransactionResponseType)(_rd.Response)).ReferenceNumber;
                can.OriginalSequenceNumber = _rd.PWS_TxnSummary.PWSRequest.Merchant.Software.SequenceNumber;
                //can.OriginalAuthCode = _rd.AuthorizationCode;
                //can.NetworkResponseCode = ((TransactionResponseType)(_rd.Response)).NetworkResponseCode;
                can.ReversalReason = (ReversalReasonType)target.CboReversalReason.SelectedItem;
                can.ReversalReasonSpecified = true;
            }
            else
            {//Merchant
                can.CancelType = target.CancelTransactionTypeFromRequest(_rd.TxnRequestType);
                can.OriginalAmount = _rd.Amount;
                can.OriginalTransactionTimestamp = _rd.PWS_TxnSummary.PWSRequest.TransactionTimestamp;
                can.OriginalTransactionTimestampSpecified = true;
                can.OriginalSystemTraceId = _rd.PWS_TxnSummary.PWSRequest.systemtraceid;
                can.OriginalSystemTraceIdSpecified = true;
                can.OriginalReferenceNumber = ((TransactionResponseType)(_rd.PWS_TxnSummary.Response)).ReferenceNumber;
                can.OriginalSequenceNumber = _rd.PWS_TxnSummary.PWSRequest.Merchant.Software.SequenceNumber;
                // NEED TO ADD can.OriginalSequenceNumber = p.Merchant.Software.SequenceNumber;
                can.OriginalAuthCode = _rd.AuthorizationCode;
                can.NetworkResponseCode = ((TransactionResponseType)(_rd.PWS_TxnSummary.Response)).NetworkResponseCode;
                can.ReversalReason = (ReversalReasonType)target.CboReversalReason.SelectedItem;
                can.ReversalReasonSpecified = true;
            }

            PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try
            {
                ts = new PWS_TransactionSummary(can, target.PWSClient.Cancel(can));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            return ProcessResponse(ts);
        }

        public static ResponseDetails tokenizeRequest()
        {
            TokenizeRequest t = new TokenizeRequest();

            Random r = new Random();
            //t.BillPaymentPayee = billPaymentPayeeType(); //For PIN-less Debit : Bill payment payee details contain values about the payee including payee name, phone and account number payee uses to identify the payer.
            //t.BillPaymentPayee.PayeeAccountNumber = "";
            //t.BillPaymentPayee.PayeeName = "";
            //t.BillPaymentPayee.PayeePhoneNumber = "";
            t.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            t.Merchant = merchantType();
            t.Merchant.Terminal = null;
            t.PaymentType = (PaymentType)target.CboPaymentType.SelectedItem;//Mandatory
            t.PaymentTypeSpecified = true;
            t.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
            t.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
            t.TransactionType = (TransactionTypeType)target.CboTransactionType.SelectedItem;//Mandatory
            t.TokenRequested = true;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
            t.TokenRequestedSpecified = true;
            int rInt = r.Next(1, 99999); //for ints
            t.systemtraceid = rInt;
            t.systemtraceidSpecified = true;
            rInt = r.Next(1, 99999999);
            t.merchantrefid = "PWS" + rInt.ToString();

            //Set the object for payment
            t.Item = new object[1];
            if (target.CboPaymentInstrument.Text == "Credit")
                t.Item = creditInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Debit")
                t.Item = debitInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Gift")
                t.Item = giftInstrumentType();

            PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try
            {
                ts = new PWS_TransactionSummary(t, target.PWSClient.Tokenize(t));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            return ProcessResponse(ts);
        }

        public static ResponseDetails balanceInquiryRequest()
        {
            BalanceInquiryRequest b = new BalanceInquiryRequest();

            Random r = new Random();
            b.DraftLocatorId = "D" + r.Next(1, 99999999).ToString(); //11 character value.  This field can be used to pass whatever discretionary data the merchant wants to pass.  Examples include employee ID number, invoice numbers, any internal value they use to track transactions.	Optional – only passes thru to reporting on Visa and MasterCard transactions.
            b.Merchant = merchantType();
            //b.NetworkResponseCode = "";
            b.PaymentType = (PaymentType)target.CboPaymentType.SelectedItem;//Mandatory
            b.PaymentTypeSpecified = true;
            b.ReferenceNumber = "R" + r.Next(1, 99999).ToString(); //6 digit value which uniquely identifies the transaction.	Optional
            //b.reportgroup = ""; //An optional (required for Litle) attribute used by the merchant to map each transaction to a reporting category.  This can be no longer than 25 characters. 
            b.TokenRequested = target.ChkTokenRequested.Checked;//Boolean value (true, false) to determine if token is returned for the card. The default value is false.	Optional
            b.TokenRequestedSpecified = target.ChkTokenRequested.Checked;
            b.TransactionTimestamp = DateTime.Now; //The time of this transaction. Use yyyy-MM- ddThh:mm:ss-SS:SS – Should be in merchants local time zone. Mandatory – should be in merchant’s local time zone.
            b.TransactionType = (TransactionTypeType)target.CboTransactionType.SelectedItem;//Mandatory
            int rInt = r.Next(1, 99999); //for ints
            b.systemtraceid = rInt; //A conditional ID used to track each transaction. This must be an integer. Required for Raft and Tandem, optional for Litle? Required for Litle on CancelRequest.
            b.systemtraceidSpecified = true;
            rInt = r.Next(1, 99999999);
            b.merchantrefid = "PWS" + rInt.ToString(); //An optional attribute used by the merchant to identify each transaction. This can be no longer than 16 characters. If the merchant chooses not to use this field it is recommended that you populate this ID with the system-trace-id value.

            //Set the object for payment
            b.Item = new object[1];
            if (target.CboPaymentInstrument.Text == "Credit")
                b.Item = creditInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Debit")
                b.Item = debitInstrumentType();
            else if (target.CboPaymentInstrument.Text == "Gift")
                b.Item = giftInstrumentType();

            PWS_TransactionSummary ts = new PWS_TransactionSummary(null, null);
            try
            {
                ts = new PWS_TransactionSummary(b, target.PWSClient.BalanceInquiry(b));
            }
            catch (Exception ex)
            {
                if (!CheckForFault(ex))
                    MessageBox.Show(ex.Message);
            }
            return ProcessResponse(ts);
        }

        #endregion Transaction Processing

        #region Process Response

        public static ResponseDetails ProcessResponse(PWS_TransactionSummary _ts)
        {
            string AuthorizationCode = "";

            try
            {
                if (((TransactionResponseType)(_ts.Response)).ItemsElementName.Count() > 0)
                {
                    int idx = 0;
                    while (idx < ((TransactionResponseType)(_ts.Response)).Items.Count())
                    {
                        if (((TransactionResponseType)(_ts.Response)).ItemsElementName[idx] == ItemsChoiceType1.AuthorizationCode)
                            AuthorizationCode = ((TransactionResponseType)(_ts.Response)).Items[idx];
                        idx++;
                    }
                }
                //Add to CheckListBox
                AmountType a = new AmountType();
                a.currency = (ISO4217CurrencyCodeType)target.CboCurrencyCodeType.SelectedItem;
                a.currencySpecified = true;
                a.Value = Convert.ToDecimal(target.TxtTransactionAmount.Text);

                ResponseDetails rd = new ResponseDetails(target.CboPWSorVDP.Text, a, AuthorizationCode, target.CboPaymentInstrument.Text, target.CboSendTransaction.Text, null, _ts);
                
                target.ChkLstTransactionsProcessed.Items.Add(rd);

                return rd;
            }
            catch
            {
                return null;
            }
        }

        public static string ExtractDetailsFromResponse(ResponseDetails _rd)
        {
            string transactionInformation = "";

            try
            {
                if (_rd.PWS_TxnSummary.PWSRequest != null)
                {
                    TransactionRequestType trt = (TransactionRequestType)_rd.PWS_TxnSummary.PWSRequest;
                    transactionInformation += "REQUEST\r\n";
                    transactionInformation += "PaymentInstrumentType" + _rd.PaymentInstrumentType + "\r\n";
                    if (trt.BillPaymentPayee != null)
                        transactionInformation += "BillPaymentPayee\r\n "
                            + " - PayeeAccountNumber: " + trt.BillPaymentPayee.PayeeAccountNumber + "\r\n"
                            + " - PayeeName: " + trt.BillPaymentPayee.PayeeName + "\r\n"
                            + " - PayeePhoneNumber: " + trt.BillPaymentPayee.PayeePhoneNumber + "\r\n"
                            ;
                    if (trt.DraftLocatorId != null)
                        transactionInformation += "DraftLocatorId: " + trt.DraftLocatorId + "\r\n";
                    if (trt.Merchant != null)
                        transactionInformation += "Merchant\r\n "
                            + " - CashierNumber: " + trt.Merchant.CashierNumber + "\r\n"
                            + " - ChainCode: " + trt.Merchant.ChainCode + "\r\n"
                            + " - ClerkNumber: " + trt.Merchant.ClerkNumber + "\r\n"
                            + " - DivisionNumber: " + trt.Merchant.DivisionNumber + "\r\n"
                            + " - LaneNumber: " + trt.Merchant.LaneNumber + "\r\n"
                            + " - MerchantId: " + trt.Merchant.MerchantId + "\r\n"
                            + " - MerchantName: " + trt.Merchant.MerchantName + "\r\n"
                            //+ " - Mobile: " + trt.Merchant.Mobile + "\r\n"
                            + " - NetworkRouting: " + trt.Merchant.NetworkRouting + "\r\n"
                            //+ " - Software: " + trt.Merchant.Software + "\r\n"
                            + " - StoreNumber: " + trt.Merchant.StoreNumber + "\r\n"
                            //+ " - Terminal: " + trt.Merchant.Terminal + "\r\n"
                            ;
                    if (trt.merchantrefid != null)
                        transactionInformation += "merchantrefid: " + trt.merchantrefid + "\r\n";
                    if (trt.NetworkResponseCode != null)
                        transactionInformation += "NetworkResponseCode: " + trt.NetworkResponseCode + "\r\n";
                    if (trt.PaymentType != null)
                        transactionInformation += "PaymentType: " + trt.PaymentType + "\r\n";
                    if (trt.ReferenceNumber != null)
                        transactionInformation += "ReferenceNumber: " + trt.ReferenceNumber + "\r\n";
                    if (trt.reportgroup != null)
                        transactionInformation += "reportgroup: " + trt.reportgroup + "\r\n";
                    if (trt.systemtraceid != null)
                        transactionInformation += "systemtraceid: " + trt.systemtraceid + "\r\n";
                    if (trt.TokenRequested != null)
                        transactionInformation += "TokenRequested: " + trt.TokenRequested + "\r\n";
                    if (trt.TransactionTimestamp != null)
                        transactionInformation += "TransactionTimestamp: " + trt.TransactionTimestamp + "\r\n";
                    if (trt.TransactionType != null)
                        transactionInformation += "TransactionType: " + trt.TransactionType + "\r\n";
                    transactionInformation += "\r\n";
                }
                if (_rd.PWS_TxnSummary.Response != null)
                {
                    TransactionResponseType trt = (TransactionResponseType)_rd.PWS_TxnSummary.Response;
                    transactionInformation += "RESPONSE\r\n";
                    if (trt.AddressVerificationResult != null)
                        transactionInformation += "AVS Verification Result\r\n -Code: " + trt.AddressVerificationResult.Code + " Type: " + trt.AddressVerificationResult.Type + "\r\n";

                    if (trt.Balance != null)
                    {
                        transactionInformation += "Balance\r\n ";
                        if (trt.Balance.Authorized != null)
                        {
                            transactionInformation += " - Authorized: "
                            + trt.Balance.Authorized.Value + " "
                            + trt.Balance.Authorized.currency + "\r\n";
                        }
                        if (trt.Balance.AvailableBalance != null)
                        {
                            transactionInformation += " - Available Balance: "
                            + trt.Balance.AvailableBalance.Value + ""
                            + trt.Balance.AvailableBalance.currency + "\r\n";
                        }
                        if (trt.Balance.BeginningBalance != null)
                        {
                            transactionInformation += " - Beginning Balance: "
                            + trt.Balance.BeginningBalance.Value + ""
                            + trt.Balance.BeginningBalance.currency + "\r\n";
                        }
                        if (trt.Balance.Cash != null)
                        {
                            transactionInformation += " - Cash: "
                            + trt.Balance.Cash.Value + ""
                            + trt.Balance.Cash.currency + "\r\n";
                        }
                        if (trt.Balance.Cash != null)
                        {
                            transactionInformation += " - Ending Balance: "
                            + trt.Balance.EndingBalance.Value + ""
                            + trt.Balance.EndingBalance.currency + "\r\n";
                        }
                        if (trt.Balance.PreAuthorized != null)
                        {
                            transactionInformation += " - Pre Authorized: "
                            + trt.Balance.PreAuthorized.Value + ""
                            + trt.Balance.PreAuthorized.currency + "\r\n";
                        }
                    }
                    if (trt.BatchNumber != null)
                        transactionInformation += "BatchNumber: " + trt.BatchNumber + "\r\n";
                    if (trt.CardCategory != null)
                        transactionInformation += "CardCategory: " + trt.CardCategory + "\r\n";
                    if (trt.CardSecurityCodeResult != null)
                        transactionInformation += "Card Security Code Result\r\n -Code: " + trt.CardSecurityCodeResult.Code + " Type: " + trt.CardSecurityCodeResult.Type + "\r\n";
                    if (trt.DemoMode != null)
                        transactionInformation += "Demo Mode: " + trt.DemoMode + "\r\n";
                    if (trt.Items != null && trt.Items.Count() > 0)
                    {
                        transactionInformation += "Items\r\n";
                        if (((TransactionResponseType)(_rd.PWS_TxnSummary.Response)).Items.Count() > 0)
                        {
                            int idx = 0;
                            while (idx < ((TransactionResponseType)(_rd.PWS_TxnSummary.Response)).Items.Count())
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

                    transactionInformation += "\r\n";
                }
            }
            catch
            { }

            return transactionInformation;

        }

        private static bool CheckForFault(Exception _ex)
        {
            if (((System.ServiceModel.FaultException<com.vantiv.types.payment.transactions.v6.RequestValidationFault>)(_ex)) != null)
            {
                string info = "";
                foreach (XmlNode f in ((System.ServiceModel.FaultException<com.vantiv.types.payment.transactions.v6.RequestValidationFault>)(_ex)).Detail.Nodes)
                {
                    info += f.InnerXml + "\r\n";
                }
                MessageBox.Show(info);
                return true;
            }
            else
                return false;
        }

        #endregion Process Response

    }

    #region Extra Classes
    public class PWS_TransactionSummary
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
        public TransactionRequestType PWSRequest;
        public Object Response;

        public PWS_TransactionSummary(TransactionRequestType pWSRequest, Object response)
        {
            PWSRequest = pWSRequest;
            Response = response;
        }
    }
    #endregion Extra Classes
}
