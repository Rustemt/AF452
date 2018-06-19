using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Plugins;
using System.Collections;
using System.Collections.Generic;

namespace Nop.Services.Payments
{
    public partial class PaymentService : IPaymentService
    {
        private readonly IRepository<Bin> _binRepository;


        public PaymentService(PaymentSettings paymentSettings, IPluginFinder pluginFinder,
            ShoppingCartSettings shoppingCartSettings, IRepository<Bin> binRepository)
        {
            this._paymentSettings = paymentSettings;
            this._pluginFinder = pluginFinder;
            this._shoppingCartSettings = shoppingCartSettings;
            this._binRepository = binRepository;
        }

        public Bin GetBinByCCNo(string CreditCartNo)
        {
            if (CreditCartNo == null)
                throw new ArgumentNullException("AF/PaymentService");
            int binNo = 0;
            try
            {
                var castBin = CreditCartNo.Substring(0, 6);
                binNo = Convert.ToInt32(castBin);
            }
            catch
            {
                return null;
            }


            var query = from br in _binRepository.Table
                        orderby br.Id
                        where br.BinCode == binNo
                        select br;
            var bin = query.FirstOrDefault();
            return bin;


           // return _binRepository.Table.Where(x => x.BinCode == bin).FirstOrDefault();
        }

        public bool BinExists(string CreditCartNo)
        {
            if (CreditCartNo == null)
                throw new ArgumentNullException("AF PaymentService");
            int bin = 0;

            try
            {
                var castBin = CreditCartNo.Substring(0,6);
                bin = Convert.ToInt32(castBin);
            }
            catch
            {
                return false;
            }
            return _binRepository.Table.Where(x => x.BinCode == bin).FirstOrDefault() != null ? true : false;
        }
        
            public virtual PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest preProcessPaymentRequest)
        {
            
            var paymentMethod = LoadPaymentMethodBySystemName(preProcessPaymentRequest.PaymentMethodSystemName);
            if (paymentMethod == null)
                throw new NopException("Payment method couldn't be loaded");
            return paymentMethod.PreProcessPayment(preProcessPaymentRequest);
        }

            public virtual IList<KeyValuePair<string, string>> GetPaymentOptions(ProcessPaymentRequest processPaymentRequest)
            {

                var paymentMethod = LoadPaymentMethodBySystemName(processPaymentRequest.PaymentMethodSystemName);
                if (paymentMethod == null)
                    throw new NopException("Payment method couldn't be loaded");
                if(!(paymentMethod is ICampaignedPaymentMethod))
                    throw new NopException("Payment method does not support campaigns");

                return ((ICampaignedPaymentMethod)paymentMethod).GetPaymentOptions(processPaymentRequest);
            }

    }
}
