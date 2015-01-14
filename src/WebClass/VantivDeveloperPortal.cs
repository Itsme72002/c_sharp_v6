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

namespace CertSuiteTool_VDP
{
    public partial class VantivDeveloperPortal
    {
        public Credentials credentials;
        public Merchant merchant;
        public Terminal terminal;
        public Transaction transaction;
        public Address address;
        public Card card;
        private string UserName;//The following will be replaced for VDP once License is working
        private string Password;//The following will be replaced for VDP once License is working

        public VantivDeveloperPortal(string userName, string password) 
        {
		    credentials = new Credentials();
            credentials.AccountID = userName;
            credentials.Password = password;
		    merchant = new Merchant();
		    terminal = new Terminal();
		    transaction = new Transaction();
		    address = new Address();
		    card = new Card();
	    }

    }

    #region Additional Classes


    public class Credentials
    {//Online Documentation: http://dev-vantiv.devportal.apigee.com/docs/payment-web-services/api-element-dictionary/rest-credentials-definition
        
        // Required
        private String accountID;
        private String password;

        public string AccountID
        {
            get
            {
                return this.accountID;
            }
            set
            {
                this.accountID = value;
            }
        }

        public string Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

    }

    public class Merchant
    {//Online Documentation: http://dev-vantiv.devportal.apigee.com/docs/payment-web-services/api-element-dictionary/rest-merchant-definition

        // Required
        private String merchantID;
        private String merchantName;
        private String networkRouting;
        // optional
        private String cashierNumber;
        private String laneNumber;
        private String divisionNumber;
        private String chainCode;
        private String storeNumber;

        public string MerchantID
        {
            get
            {
                return this.merchantID;
            }
            set
            {
                this.merchantID = value;
            }
        }

        public string MerchantName
        {
            get
            {
                return this.merchantName;
            }
            set
            {
                this.merchantName = value;
            }
        }

        public string NetworkRouting
        {
            get
            {
                return this.networkRouting;
            }
            set
            {
                this.networkRouting = value;
            }
        }

        public string CashierNumber
        {
            get
            {
                return this.cashierNumber;
            }
            set
            {
                this.cashierNumber = value;
            }
        }

        public string LaneNumber
        {
            get
            {
                return this.laneNumber;
            }
            set
            {
                this.laneNumber = value;
            }
        }

        public string DivisionNumber
        {
            get
            {
                return this.divisionNumber;
            }
            set
            {
                this.divisionNumber = value;
            }
        }

        public string ChainCode
        {
            get
            {
                return this.chainCode;
            }
            set
            {
                this.chainCode = value;
            }
        }

        public string StoreNumber
        {
            get
            {
                return this.storeNumber;
            }
            set
            {
                this.storeNumber = value;
            }
        }
 
    }

    public class Terminal
    {//Online Documentation: http://dev-vantiv.devportal.apigee.com/docs/payment-web-services/api-element-dictionary/rest-terminal-definition
        
        // Required
        private String terminalID;
        private EntryModeType entryMode;
        // Optional
        private String iPv4Address;
        private String iPv6Address;
        private TerminalClassificationType terminalEnvironmentCode;
        private CardInputCode cardInputCode;
        private String cardReader; // I DON'T SEE THIS IN THE ONLINE
        private PinEntryType pinEntry;
        private bool balanceInquiry;
        private DeviceType deviceType;
        private bool hostAdjustment;
        private Nullable<Decimal> latitude;
        private Nullable<Decimal> longitude;

        public String TerminalID
        {
            get
            {
                return this.terminalID;
            }
            set
            {
                this.terminalID = value;
            }
        }

        public EntryModeType EntryMode
        {
            get
            {
                return this.entryMode;
            }
            set
            {
                this.entryMode = value;
            }
        }

        public String IPv4Address
        {
            get
            {
                return this.iPv4Address;
            }
            set
            {
                this.iPv4Address = value;
            }
        }

        public String IPv6Address
        {
            get
            {
                return this.iPv6Address;
            }
            set
            {
                this.iPv6Address = value;
            }
        }

        public TerminalClassificationType TerminalEnvironmentCode
        {
            get
            {
                return this.terminalEnvironmentCode;
            }
            set
            {
                this.terminalEnvironmentCode = value;
            }
        }

        public CardInputCode CardInputCode
        {
            get
            {
                return this.cardInputCode;
            }
            set
            {
                this.cardInputCode = value;
            }
        }

        public String CardReader
        {
            get
            {
                return this.cardReader;
            }
            set
            {
                this.cardReader = value;
            }
        }

        public PinEntryType PinEntry
        {
            get
            {
                return this.pinEntry;
            }
            set
            {
                this.pinEntry = value;
            }
        }

        public bool BalanceInquiry
        {
            get
            {
                return this.balanceInquiry;
            }
            set
            {
                this.balanceInquiry = value;
            }
        }

        public DeviceType DeviceType
        {
            get
            {
                return this.deviceType;
            }
            set
            {
                this.deviceType = value;
            }
        }

        public bool HostAdjustment
        {
            get
            {
                return this.hostAdjustment;
            }
            set
            {
                this.hostAdjustment = value;
            }
        }

        public Nullable<Decimal> Latitude
        {
            get
            {
                return this.latitude;
            }
            set
            {
                this.latitude = value;
            }
        }

        public Nullable<Decimal> Longitude
        {
            get
            {
                return this.longitude;
            }
            set
            {
                this.longitude = value;
            }
        }
       
    }

    public class Transaction
    {//Online Documentation: http://dev-vantiv.devportal.apigee.com/docs/payment-web-services/api-element-dictionary/rest-transaction-definition

        // Required
        private String transactionID;
        private String transactionAmount;
        private Nullable<MarketCode> marketCode;
        private String transactionTimestamp;
        private String clerkNumber;
        private String adjustedTotalAmount;
        // Optional-Conditional
        private Nullable<CancelTransactionType> cancelType;
        private Nullable<PaymentType> paymentType;
        private String referenceNumber;
        private String draftLocatorId;
        private String authorizationCode;
        private String originalAuthorizedAmount;
        private String captureAmount;
        private String cashBackAmount;
        private String originalTransactionTimestamp;
        private String originalSystemTraceId;
        private String originalSequenceNumber;
        private String originalAuthCode;
        private String networkResponseCode;
        private Nullable<ReversalReasonType> reversalReason;
        private String replacementAmount;
        private String originalReferenceNumber;
        private String tipAmount;
        private String convenienceFee;
        private Nullable<bool> taxExempt;
        private String taxable;
        private String taxAmount;
        private String purchaseOrder;
        private Nullable<bool> tokenRequested;
        private Nullable<PartialIndicatorType> partialApprovalCode;
        private String systemTraceID; // NOT IN THE ONLINE DOC

        public string TransactionID
        {
            get
            {
                return this.transactionID;
            }
            set
            {
                this.transactionID = value;
            }
        }

        public string TransactionAmount
        {
            get
            {
                return this.transactionAmount;
            }
            set
            {
                this.transactionAmount = value;
            }
        }

        public Nullable<MarketCode> MarketCode
        {
            get
            {
                return this.marketCode;
            }
            set
            {
                this.marketCode = value;
            }
        }

        public String TransactionTimestamp
        {
            get
            {
                return this.transactionTimestamp;
            }
            set
            {
                this.transactionTimestamp = value;
            }
        }

        public String ClerkNumber
        {
            get
            {
                return this.clerkNumber;
            }
            set
            {
                this.clerkNumber = value;
            }
        }

        public String AdjustedTotalAmount
        {
            get
            {
                return this.adjustedTotalAmount;
            }
            set
            {
                this.adjustedTotalAmount = value;
            }
        }

        public Nullable<CancelTransactionType> CancelType
        {
            get
            {
                return this.cancelType;
            }
            set
            {
                this.cancelType = value;
            }
        }

        public Nullable<PaymentType> PaymentType
        {
            get
            {
                return this.paymentType;
            }
            set
            {
                this.paymentType = value;
            }
        }

        public string ReferenceNumber
        {
            get
            {
                return this.referenceNumber;
            }
            set
            {
                this.referenceNumber = value;
            }
        }

        public string DraftLocatorId
        {
            get
            {
                return this.draftLocatorId;
            }
            set
            {
                this.draftLocatorId = value;
            }
        }

        public string AuthorizationCode
        {
            get
            {
                return this.authorizationCode;
            }
            set
            {
                this.authorizationCode = value;
            }
        }

        public string OriginalAuthorizedAmount
        {
            get
            {
                return this.originalAuthorizedAmount;
            }
            set
            {
                this.originalAuthorizedAmount = value;
            }
        }

        public string CaptureAmount
        {
            get
            {
                return this.captureAmount;
            }
            set
            {
                this.captureAmount = value;
            }
        }

        public string CashBackAmount
        {
            get
            {
                return this.cashBackAmount;
            }
            set
            {
                this.cashBackAmount = value;
            }
        }

        public string OriginalTransactionTimestamp
        {
            get
            {
                return this.originalTransactionTimestamp;
            }
            set
            {
                this.originalTransactionTimestamp = value;
            }
        }

        public string OriginalSystemTraceId
        {
            get
            {
                return this.originalSystemTraceId;
            }
            set
            {
                this.originalSystemTraceId = value;
            }
        }

        public string OriginalSequenceNumber
        {
            get
            {
                return this.originalSequenceNumber;
            }
            set
            {
                this.originalSequenceNumber = value;
            }
        }

        public string OriginalAuthCode
        {
            get
            {
                return this.originalAuthCode;
            }
            set
            {
                this.originalAuthCode = value;
            }
        }

        public string NetworkResponseCode
        {
            get
            {
                return this.networkResponseCode;
            }
            set
            {
                this.networkResponseCode = value;
            }
        }

        public Nullable<ReversalReasonType> ReversalReason
        {
            get
            {
                return this.reversalReason;
            }
            set
            {
                this.reversalReason = value;
            }
        }

        public string ReplacementAmount
        {
            get
            {
                return this.replacementAmount;
            }
            set
            {
                this.replacementAmount = value;
            }
        }

        public string OriginalReferenceNumber
        {
            get
            {
                return this.originalReferenceNumber;
            }
            set
            {
                this.originalReferenceNumber = value;
            }
        }

        public string TipAmount
        {
            get
            {
                return this.tipAmount;
            }
            set
            {
                this.tipAmount = value;
            }
        }

        public string ConvenienceFee
        {
            get
            {
                return this.convenienceFee;
            }
            set
            {
                this.convenienceFee = value;
            }
        }

        public Nullable<bool> TaxExempt
        {
            get
            {
                return this.taxExempt;
            }
            set
            {
                this.taxExempt = value;
            }
        }

        public string Taxable
        {
            get
            {
                return this.taxable;
            }
            set
            {
                this.taxable = value;
            }
        }

        public string TaxAmount
        {
            get
            {
                return this.taxAmount;
            }
            set
            {
                this.taxAmount = value;
            }
        }

        public string PurchaseOrder
        {
            get
            {
                return this.purchaseOrder;
            }
            set
            {
                this.purchaseOrder = value;
            }
        }

        public Nullable<bool> TokenRequested
        {
            get
            {
                return this.tokenRequested;
            }
            set
            {
                this.tokenRequested = value;
            }
        }

        public Nullable<PartialIndicatorType> PartialApprovalCode
        {
            get
            {
                return this.partialApprovalCode;
            }
            set
            {
                this.partialApprovalCode = value;
            }
        }

        public string SystemTraceID
        {
            get
            {
                return this.systemTraceID;
            }
            set
            {
                this.systemTraceID = value;
            }
        }
        
    }

    public class Address
    {//Online Documentation: http://dev-vantiv.devportal.apigee.com/docs/payment-web-services/api-element-dictionary/rest-address-definition

        private String billingAddress1;
        private String billingCity;
        private String billingState;
        private String billingZipcode;
        private Nullable<ISO3166CountryCodeType> countryCode;

        public string BillingAddress1
        {
            get
            {
                return this.billingAddress1;
            }
            set
            {
                this.billingAddress1 = value;
            }
        }

        public string BillingCity
        {
            get
            {
                return this.billingCity;
            }
            set
            {
                this.billingCity = value;
            }
        }

        public string BillingState
        {
            get
            {
                return this.billingState;
            }
            set
            {
                this.billingState = value;
            }
        }

        public string BillingZipcode
        {
            get
            {
                return this.billingZipcode;
            }
            set
            {
                this.billingZipcode = value;
            }
        }

        public Nullable<ISO3166CountryCodeType> CountryCode
        {
            get
            {
                return this.countryCode;
            }
            set
            {
                this.countryCode = value;
            }
        }

    }

    public class Card
    {//Online Documentation: http://dev-vantiv.devportal.apigee.com/docs/payment-web-services/api-element-dictionary/rest-card-definition

        private CreditCardNetworkType cardType;
        private String cardNumber;
        private String expirationMonth;
        private String expirationYear;
        private String track1Data;
        private String encryptedTrack1Data;
        private String track2Data;
        private String encryptedTrack2Data;
        private String cardDataKeySerialNumber;
        private Nullable<EncryptionType> encryptedFormat;
        private String pINBlock;
        private Nullable<EncryptionType> pINBlockEncryptedFormat;
        private String tokenID;
        private String tokenValue;
        private String cVV;
        private String cardholderName;
        private Nullable<AccountType> accountType;

        public CreditCardNetworkType CardType
        {
            get
            {
                return this.cardType;
            }
            set
            {
                this.cardType = value;
            }
        }

        public String CardNumber
        {
            get
            {
                return this.cardNumber;
            }
            set
            {
                this.cardNumber = value;
            }
        }

        public String ExpirationMonth
        {
            get
            {
                return this.expirationMonth;
            }
            set
            {
                this.expirationMonth = value;
            }
        }

        public String ExpirationYear
        {
            get
            {
                return this.expirationYear;
            }
            set
            {
                this.expirationYear = value;
            }
        }

        public String Track1Data
        {
            get
            {
                return this.track1Data;
            }
            set
            {
                this.track1Data = value;
            }
        }

        public String EncryptedTrack1Data
        {
            get
            {
                return this.encryptedTrack1Data;
            }
            set
            {
                this.encryptedTrack1Data = value;
            }
        }

        public String Track2Data
        {
            get
            {
                return this.track2Data;
            }
            set
            {
                this.track2Data = value;
            }
        }

        public String EncryptedTrack2Data
        {
            get
            {
                return this.encryptedTrack2Data;
            }
            set
            {
                this.encryptedTrack2Data = value;
            }
        }

        public String CardDataKeySerialNumber
        {
            get
            {
                return this.cardDataKeySerialNumber;
            }
            set
            {
                this.cardDataKeySerialNumber = value;
            }
        }

        public Nullable<EncryptionType> EncryptedFormat
        {
            get
            {
                return this.encryptedFormat;
            }
            set
            {
                this.encryptedFormat = value;
            }
        }

        public String PINBlock
        {
            get
            {
                return this.pINBlock;
            }
            set
            {
                this.pINBlock = value;
            }
        }

        public Nullable<EncryptionType> PINBlockEncryptedFormat
        {
            get
            {
                return this.pINBlockEncryptedFormat;
            }
            set
            {
                this.pINBlockEncryptedFormat = value;
            }
        }

        public String TokenID
        {
            get
            {
                return this.tokenID;
            }
            set
            {
                this.tokenID = value;
            }
        }

        public String TokenValue
        {
            get
            {
                return this.tokenValue;
            }
            set
            {
                this.tokenValue = value;
            }
        }

        public String CVV
        {
            get
            {
                return this.cVV;
            }
            set
            {
                this.cVV = value;
            }
        }

        public String CardholderName
        {
            get
            {
                return this.cardholderName;
            }
            set
            {
                this.cardholderName = value;
            }
        }

        public Nullable<AccountType> AccountType
        {
            get
            {
                return this.accountType;
            }
            set
            {
                this.accountType = value;
            }
        }

    }

    #region Enumerations

    public enum CreditCardNetworkType
    {
        visa,
        masterCard,
        discover,
        amex,
    }

    public enum EncryptionType
    {
        DUKPT,
        VOLTAGE,
    }

    public enum AccountType
    {
        CHECKING,
        SAVINGS,
    }

    public enum TerminalClassificationType
    {
        unspecified,
        limited_amount_terminal,
        telephone_device,
        unattended_atm,
        unattended_self_service,
        electronic_cash_register,
        mobile_contactless_transaction,
    }

    public enum CardInputCode
    {
        ManualKeyed,
        MagstripeRead,
        ManualKeyedMagstripeFailure,
        ContactlessMagstripeRead,
        Barcode,
        OCR,
        ChipRead,
    }

    public enum PinEntryType
    {
        supported,
        none,
        inoperative,
        terminal_verified,
        unknown,
    }

    public enum DeviceType
    {
        Terminal,
        Software,
        Mobile,
    }

    public enum MarketCode
    {
        present,
        moto,
        ecommerce

        //Default,
        //AutoRental,
        //DirectMarketing,
        //ECommerce,
        //FoodRestaurant,
        //HotelLodging,
        //Petroleum,
        //Retail,
        //QSR,
    }

    public enum CancelTransactionType
    {
        authorize,
        purchase,
        purchase_cashback,
        refund,
        adjust,
        capture,
        activate,
        reload,
        unload,
        close,
    }

    public enum PaymentType
    {
        single,
        recurring,
        installment,
        billpay,
        resubmission,
    }

    public enum ReversalReasonType
    {
        INCOMPLETE_TRANSACTION,
        TIME_OUT,
        INVALID_RESPONSE,
        DESTINATION_NOT_AVAILABLE,
        CLERK_CANCELED_TRANSACTION,
        CUSTOMER_CANCELED_TRANSACTION,
        MISDISPENSE,
        HARDWARE_FAILURE,
        SUSPECTED_FRAUDE,
    }

    public enum PartialIndicatorType
    {
        not_supported,
        supported,
        return_balance,
        partial_cash,
        full_cash,
        partial_merch,
    }

    public enum EntryModeType
    {
        unknown,
        manual,
        track1,
        track2,
        barcode,
        ocr,
        integrated_circuit,
        proximity_vsdc,
        proximity_contactless,
    }

    #endregion Enumerations

    #endregion Additional Classes

}
