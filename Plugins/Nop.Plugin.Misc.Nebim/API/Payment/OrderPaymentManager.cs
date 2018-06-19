using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NebimV3.Library;
using NebimV3.v3Entities;
using NebimV3.ItemTransactions;
using NebimV3.DataConnector;
using Nop.Services.ExportImport;

namespace Nop.Plugin.Misc.NebimIntegration.API
{
    public enum myPaymentTypes
    {
        cc=1,
        bank=2, 
        cash=3
    }
    public class OrderPaymentManager
    {
        NebimIntegrationSettings _nebimIntegrationSettings;

        public OrderPaymentManager(NebimIntegrationSettings nebimIntegrationSettings)
        {
            _nebimIntegrationSettings = nebimIntegrationSettings;
            ConnectionManager.ConnectDB(nebimIntegrationSettings);
        }
        // Elimizde yalnızca sipariş numarası var ise
        public void SavePayment(string orderNumber, myPaymentTypes paymentType, string creditCardTypeCode, byte installment,string maskedCCNo, string provisionNo)
        {
          
            NebimV3.Orders.RetailSale order = new NebimV3.Orders.RetailSale();
            order.OrderNumber = orderNumber;
            order.Load();

            this.SavePayment(order, paymentType, creditCardTypeCode, installment, maskedCCNo, provisionNo);
           
        }
        // Elimizde order nesnesi var ise
        public void SavePayment(NebimV3.Orders.RetailSale order, myPaymentTypes paymentType, string creditCardTypeCode, byte installment, string maskedCCNo, string provisionNo)
        {
            try
            {
            if (paymentType == myPaymentTypes.bank)
                this.SavePaymentByBank(order);
            else if (paymentType == myPaymentTypes.cash)
                this.SavePaymentByCash(order, _nebimIntegrationSettings.API_CashAccountCode);//cashAccountCode="100.01.001"
            else
                this.SavePaymentByCreditCard(order, creditCardTypeCode, installment, maskedCCNo, provisionNo);
            }
            catch (Exception ex)
            {
                NebimV3.Library.V3Exception v3Ex = ex as NebimV3.Library.V3Exception;
                if (v3Ex != null)
                    throw new Exception(NebimV3.ApplicationCommon.ExceptionHandlerBase.Default.GetExceptionMessage(v3Ex), ex);
                throw;
            }
        }

        private void SavePaymentByBank(NebimV3.Orders.RetailSale order)
        {
            if (!order.Summary.Initialized)
                order.Summary.Calculate();

            NebimV3.Banks.Bank bankTrans = new NebimV3.Banks.Bank();

            bankTrans.BankTransTypeCode = NebimV3.Banks.BankTransTypes.BankRemittance_EFT;
            bankTrans.DocumentDate = order.OrderDate;
            bankTrans.DocumentTime = order.OrderTime;
            bankTrans.OfficeCode = order.OfficeCode;
            bankTrans.LocalCurrencyCode = order.LocalCurrencyCode;
            bankTrans.DocCurrencyCode = order.DocCurrencyCode;
            bankTrans.RefNumber = order.OrderNumber;
            bankTrans.StoreCode = _nebimIntegrationSettings.API_StoreCode;

            bankTrans.ApplicationCode = NebimV3.v3Entities.ApplicationCodes.Order;
            var PaymentPlans = order.GetPaymentPlans();
            PaymentPlans.Load();
            bankTrans.ApplicationID = PaymentPlans[0].OrderPaymentPlanID;

            NebimV3.Banks.BankLine line = new NebimV3.Banks.BankLine(bankTrans);
            line.CurrAccTypeCode = order.CurrAccTypeCode;
            line.CurrAccCode = order.CustomerCode;

            line.BankAccountCode = _nebimIntegrationSettings.API_BankAccountCode; // Havale yapılan banka hesabının Cari Hesap Kodu

            line.DocCurrencyCode = bankTrans.DocCurrencyCode;
            line.Amount = order.Summary.DebitNetAmount;

            line.DueDate = DateTime.Now;
            
            line.Save();

            bankTrans.SaveAsCompleted();
        }

        private void SavePaymentByCash(NebimV3.Orders.RetailSale order, string CashAccountCode)
        {
            if (!order.Summary.Initialized)
                order.Summary.Calculate();

            NebimV3.Cashs.Cash CashTrans = new NebimV3.Cashs.Cash();

            CashTrans.CashTransTypeCode = NebimV3.Cashs.CashTransTypes.PaymentsIn;
            CashTrans.DocumentDate = order.OrderDate;
            CashTrans.DocumentTime = order.OrderTime;
            CashTrans.OfficeCode = order.OfficeCode;
            CashTrans.StoreCode = order.StoreCode;
            CashTrans.CashAccountCode = CashAccountCode;
            CashTrans.LocalCurrencyCode = order.LocalCurrencyCode;
            CashTrans.DocCurrencyCode = order.DocCurrencyCode;
            CashTrans.RefNumber = order.OrderNumber;

            CashTrans.ApplicationCode = NebimV3.v3Entities.ApplicationCodes.Order;

            var PaymentPlans = order.GetPaymentPlans();
            PaymentPlans.Load();
            CashTrans.ApplicationID = PaymentPlans[0].OrderPaymentPlanID;


            NebimV3.Cashs.CashLine line = new NebimV3.Cashs.CashLine(CashTrans);
            line.CurrAccTypeCode = order.CurrAccTypeCode;
            line.CurrAccCode = order.CustomerCode;
                      

            line.DocCurrencyCode = CashTrans.DocCurrencyCode;
            line.Amount = order.Summary.DebitNetAmount;

            line.Save();

            CashTrans.SaveAsCompleted();
        }

        private void SavePaymentByCreditCard(NebimV3.Orders.RetailSale order, string creditCardTypeCode, byte installment,string maskedCCNo, string provisionNo)
        {
            if (!order.Summary.Initialized)
                order.Summary.Calculate();
            
            NebimV3.Payments.CreditCardPayment ccTrans = new NebimV3.Payments.CreditCardPayment();
             

            ccTrans.CreditCardPaymentTypeCode = NebimV3.Payments.CreditCardPaymentTypes.SalePayment;
            ccTrans.PaymentDate = order.OrderDate;
            ccTrans.PaymentTime = order.OrderTime;
            ccTrans.OfficeCode = order.OfficeCode;
            ccTrans.LocalCurrencyCode = order.LocalCurrencyCode;
            ccTrans.CurrAccTypeCode = order.CurrAccTypeCode;
            ccTrans.CurrAccCode = order.CustomerCode;
            ccTrans.DocCurrencyCode = order.DocCurrencyCode;
            ccTrans.RefNumber = order.OrderNumber;
            ccTrans.ApplicationCode = NebimV3.v3Entities.ApplicationCodes.Order;
            ccTrans.StoreCode = _nebimIntegrationSettings.API_StoreCode;

            var PaymentPlans = order.GetPaymentPlans();
            PaymentPlans.Load();
            ccTrans.ApplicationID = PaymentPlans[0].OrderPaymentPlanID;

            NebimV3.Payments.CreditCardPaymentLine line = new NebimV3.Payments.CreditCardPaymentLine(ccTrans);
            
            // Required
            line.CreditCardTypeCode = creditCardTypeCode; // Kart Tipi
            line.CreditCardInstallmentCount = installment; // Taksit sayısı - (PUAN için 0 taksit)
            line.DocCurrencyCode = ccTrans.DocCurrencyCode;
            line.Amount = order.Summary.DebitNetAmount;

            // Optional
            line.CreditCardNum = maskedCCNo; // Kart Numarası
            line.POSProvisionID = provisionNo;

            line.Save();

            ccTrans.SaveAsCompleted();
        }

      
    }
}
