using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Core.Domain.Logging;

namespace Nop.Services.Messages
{
    public partial class WorkflowMessageService : IWorkflowMessageService
    {
      
        private IList<Token> GenerateTokensVariant(ProductVariant productVariant)
        {
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens);
            //_messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddVariantTokens(tokens, productVariant);
            return tokens;
        }

        private IList<Token> GenerateTokens(Customer customer, IList<ShoppingCartItem> wishListItems)
        {
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            _messageTokenProvider.AddWishListTokens(tokens, wishListItems);
            return tokens;
        }

        private IList<Token> GenerateTokensWishList(Customer customer)
        {
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens);
            _messageTokenProvider.AddCustomerTokens(tokens, customer);
            return tokens;
        }

        public virtual string ProductSpecificationAttributes(IList<Token> tokens, ProductVariant productVariant, int languageId)
        {
            string result = string.Empty;
            var sb = new StringBuilder();
            var dic = new Dictionary<string, string>();
            foreach (var spec in productVariant.Product.ProductSpecificationAttributes)
            {
                var attr = dic.FirstOrDefault(x => x.Key == spec.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(y => y.Name));
                if (attr.Key == null)
                {
                    dic.Add(spec.SpecificationAttributeOption.SpecificationAttribute.GetLocalized(x => x.Name), spec.SpecificationAttributeOption.GetLocalized(x => x.Name));
                }
                else
                {
                    string key = attr.Key;
                    string value = attr.Value;
                    dic.Remove(attr.Key);
                    value += ", " + spec.SpecificationAttributeOption.GetLocalized(x => x.Name);
                    dic.Add(key, value);
                }
            }
            foreach (var dicItem in dic)
            {
                sb.Append("<table><tr><td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\" width=\"200px;\">" + dicItem.Key + "</td><td style=\"font-size: 11px; color: rgb(102, 102, 102); font-family: 'Lucida Sans Unicode','Lucida Grande',sans-serif; border-bottom: 1px solid rgb(219, 219, 219);\">" + dicItem.Value + "</td></tr></table>");
            }
            result = sb.ToString();
            return result;
        }

        private IList<Token> GenerateTokensPriceOffer(ProductVariant productVariant, string discountUrl, string explanation, string fullName, string productName, string priceWithDiscount)
        {
            var customerTokens = new List<Token>();
            var attr = ProductSpecificationAttributes(customerTokens, productVariant, 1);
            customerTokens.Add(new Token("Customer.FullName", fullName));
            customerTokens.Add(new Token("ManufacturerProductName", productName));
            customerTokens.Add(new Token("ProductName",string.IsNullOrWhiteSpace(productVariant.GetLocalized(x => x.Name)) ? productVariant.Product.GetLocalized(x => x.Name) : productVariant.GetLocalized(x => x.Name)));
            customerTokens.Add(new Token("Sku",productVariant.Sku));
            customerTokens.Add(new Token("ManufacturerName", productVariant.Product.ProductManufacturers.Select(x => x.Manufacturer.Name).FirstOrDefault()));
            customerTokens.Add(new Token("Explanation", explanation));
            customerTokens.Add(new Token("DiscountUrl", discountUrl, true));
            customerTokens.Add(new Token("ProductAttr", attr, true));
            customerTokens.Add(new Token("PriceWithDiscount", priceWithDiscount));
            _messageTokenProvider.AddStoreTokens(customerTokens);
            _messageTokenProvider.AddVariantTokens(customerTokens, productVariant);

            return customerTokens;
        }

        public virtual int SendProductEmailAFriendMessage(string yourName, string friendName, int languageId,
            ProductVariant productVariant, string customerEmail, string friendsEmail, string personalMessage)
        {
            if (productVariant == null)
                throw new ArgumentNullException("product");

            languageId = EnsureLanguageIsActive(languageId);
            var messageTemplate = GetLocalizedActiveMessageTemplate("Product.EmailAFriend", languageId);
            if (messageTemplate == null)
                return 0;
            var customerProductTokens = GenerateTokensVariant(productVariant);
            customerProductTokens.Add(new Token("EmailAFriend.PersonalMessage", personalMessage, true));
            customerProductTokens.Add(new Token("EmailAFriend.Email", customerEmail));
            customerProductTokens.Add(new Token("Customer.FullName", yourName));
            _messageTokenProvider.AddStoreTokens(customerProductTokens);
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = friendsEmail;
            var toName = friendName ?? "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, customerProductTokens,
                toEmail, toName);
        }

        public virtual int SendMailReceivedNotification(Customer customer, EmailAccount emailAccount, int languageId, string toName, string toEmail)
        {

            if (customer == null)
                throw new ArgumentNullException("customer");

            languageId = EnsureLanguageIsActive(languageId);

            var messageTemplate = GetLocalizedActiveMessageTemplate("Notification.MailReceived", languageId);
            if (messageTemplate == null)
                return 0;
            var tokens = new List<Token>();
            _messageTokenProvider.AddStoreTokens(tokens);
            _messageTokenProvider.AddCustomerTokens(tokens, customer, toName);
            return SendNotification(messageTemplate, emailAccount,
                languageId, tokens,
                toEmail, toName);
        }

        public virtual int SendWishlistEmailAFriendMessage(Customer customer, int languageId,
            string customerEmail, string friendsEmail, string personalMessage, bool includingItems)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            languageId = EnsureLanguageIsActive(languageId);

            var messageTemplate = GetLocalizedActiveMessageTemplate("Wishlist.EmailAFriend", languageId);
            if (messageTemplate == null)
                return 0;
            IList<Token> customerTokens;
            if (includingItems)
                customerTokens = GenerateTokens(customer, customer.ShoppingCartItems.Where(x => x.ShoppingCartType == ShoppingCartType.Wishlist).ToList());
            else
                customerTokens = GenerateTokens(customer);
            //TODO add a method for getting URL
            customerTokens.Add(new Token("Wishlist.URLForCustomer", string.Format("{0}wishlist/{1}", _webHelper.GetStoreLocation(false), customer.CustomerGuid)));
            customerTokens.Add(new Token("Wishlist.PersonalMessage", personalMessage, true));
            customerTokens.Add(new Token("Wishlist.Email", customerEmail));

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = friendsEmail;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, customerTokens,
                toEmail, toName);
        }

        public virtual int SendCustomerCareStoreOwnerNotification(int languageId, string customerEmail, string Message, string FullName,string Subject)
        {
            var messageTemplate = GetLocalizedActiveMessageTemplate("CustomerCare.StoreOwnerNotification", languageId);
            if (messageTemplate == null)
                return 0;
            languageId = EnsureLanguageIsActive(languageId);
            var customerTokens = new List<Token>();
            customerTokens.Add(new Token("FullName", FullName));
            customerTokens.Add(new Token("Text", Message));
            customerTokens.Add(new Token("Mail", customerEmail));
            customerTokens.Add(new Token("Subject", Subject));
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            int mailId = SendNotification(messageTemplate, emailAccount,
                languageId, customerTokens,
                toEmail, toName);
            QueuedEmail queuedmail = _queuedEmailService.GetQueuedEmailById(mailId);
            queuedmail.Subject += " - " + mailId.ToString();
            _queuedEmailService.UpdateQueuedEmail(queuedmail);
            return mailId;
        }

        public virtual int SendCustomerCareStoreOwnerNotification(int languageId, string customerEmail, string Message, string FullName, string Subject, string ItemNumber)
        {
            var messageTemplate = GetLocalizedActiveMessageTemplate("CustomerCare.StoreOwnerNotification", languageId);
            if (messageTemplate == null)
                return 0;
            languageId = EnsureLanguageIsActive(languageId);
            var customerTokens = new List<Token>();
            customerTokens.Add(new Token("FullName", FullName));
            customerTokens.Add(new Token("Text", Message));
            customerTokens.Add(new Token("Mail", customerEmail));
            customerTokens.Add(new Token("Subject", Subject));
            customerTokens.Add(new Token("ItemNumber", ItemNumber));
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            int mailId = SendNotification(messageTemplate, emailAccount,
                languageId, customerTokens,
                toEmail, toName);
            QueuedEmail queuedmail = _queuedEmailService.GetQueuedEmailById(mailId);
            queuedmail.Subject += " - " + "[#" + mailId.ToString() + "]";
            _queuedEmailService.UpdateQueuedEmail(queuedmail);
            return mailId;
        }

        public virtual int SendCustomerCareStoreOwnerNotification(int languageId, string customerEmail, string Message, string FullName, string Subject, string ItemNumber, string phone)
        {
            var messageTemplate = GetLocalizedActiveMessageTemplate("CustomerCare.StoreOwnerNotification", languageId);
            if (messageTemplate == null)
                return 0;
            languageId = EnsureLanguageIsActive(languageId);
            var customerTokens = new List<Token>();
            customerTokens.Add(new Token("FullName", FullName));
            customerTokens.Add(new Token("Text", Message));
            customerTokens.Add(new Token("Mail", customerEmail));
            customerTokens.Add(new Token("Subject", Subject));
            customerTokens.Add(new Token("ItemNumber", ItemNumber));
            customerTokens.Add(new Token("Phone", phone));
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = emailAccount.Email;
            var toName = emailAccount.DisplayName;
            int mailId = SendNotification(messageTemplate, emailAccount,
                languageId, customerTokens,
                toEmail, toName);
            QueuedEmail queuedmail = _queuedEmailService.GetQueuedEmailById(mailId);
            queuedmail.Subject += " - " + "[#"+mailId.ToString()+"]";
            _queuedEmailService.UpdateQueuedEmail(queuedmail);
            return mailId;
        }

        public virtual int SendCustomerCareCustomerNotification(int languageId, string customerEmail, string fullName)
        {
            var messageTemplate = GetLocalizedActiveMessageTemplate("CustomerCare.CustomerNotification", languageId);
            if (messageTemplate == null)
                return 0;
            languageId = EnsureLanguageIsActive(languageId);
            var customerTokens = new List<Token>();
            customerTokens.Add(new Token("Customer.FullName", fullName));
            _messageTokenProvider.AddStoreTokens(customerTokens);
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = customerEmail.Trim();
            var toName = fullName;
            return SendNotification(messageTemplate, emailAccount,languageId, customerTokens,toEmail, toName);
        }

        public virtual int SendPriceOfferStoreOwnerNotification(string fullName, string email, string subject, string phoneNumber, int productVariantId, string productName, string explanation, int languageId, string priceOfferId, string discountPercentage, string sku, string productId, string priceWithDiscount)
        {
            var messageTemplate = GetLocalizedActiveMessageTemplate("PriceOffer.StoreOwnerNotification", languageId);
            if (messageTemplate == null)
                return 0;
            languageId = EnsureLanguageIsActive(languageId);
            var customerTokens = new List<Token>();
            customerTokens.Add(new Token("FullName", fullName));
            customerTokens.Add(new Token("Mail", email));
            customerTokens.Add(new Token("Phone", phoneNumber));
            customerTokens.Add(new Token("ProductId", productVariantId.ToString()));
            customerTokens.Add(new Token("ProductName", productName));
            customerTokens.Add(new Token("Explanation", explanation));
            customerTokens.Add(new Token("Subject", subject));
            customerTokens.Add(new Token("priceOfferId",priceOfferId));
            customerTokens.Add(new Token("DiscountPercentage", discountPercentage));
            customerTokens.Add(new Token("Sku", sku));
            customerTokens.Add(new Token("priceWithDiscount", priceWithDiscount));
            string url = string.Format("{0}" + "/p/" + productId + "", _webHelper.GetStoreLocation(false).Substring(0, _webHelper.GetStoreLocation(false).Length - 1));
            customerTokens.Add(new Token("Url", url));
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = GetEmailAccountOfMessageTemplate(messageTemplate, languageId).Email;
            var toName = fullName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, customerTokens,
                toEmail, toName);
        }

        public virtual int SendPriceOfferCustomerNotification(string fullName, string email, string phoneNumber, int productId, string productName, string explanation, int languageId, string discountUrl, ProductVariant productVariant, string priceWithDiscount)
        {
            var messageTemplate = GetLocalizedActiveMessageTemplate(productVariant.MessageTemplate.Name, languageId);
            if(!string.IsNullOrEmpty(messageTemplate.RestrictedMails))
                {
                List<string> mails = messageTemplate.RestrictedMails.Split(';').ToList<string>();
                foreach (var mail in mails)
                {
                    string sPriceOfferEmail = email.Trim().ToLower();
                    string sBlacklistMail = mail.Trim().ToLower();

                    if (sPriceOfferEmail == sBlacklistMail)
                    {

                        var logger = EngineContext.Current.Resolve<ILogger>();
                         logger.Information("PriceOfferCustomerNotification not sent => Blacklist Email: "+email);
                        return -1;
                    }
                }
                }
            if (messageTemplate == null)
                return 0;
            languageId = EnsureLanguageIsActive(languageId);
            discountUrl = string.Format("{0}" + discountUrl + "", _webHelper.GetStoreLocation(false).Substring(0, _webHelper.GetStoreLocation(false).Length - 1));

            //string passwordRecoveryUrl = string.Format("{0}passwordrecovery/confirm?token={1}&email={2}", _webHelper.GetStoreLocation(false), customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), customer.Email);
            var customerTokens = GenerateTokensPriceOffer(productVariant, discountUrl, explanation, fullName, productName, priceWithDiscount);
            _messageTokenProvider.AddStoreTokens(customerTokens);
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = email.Trim();
            var toName = fullName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, customerTokens,
                toEmail, toName);
        }

        public virtual int SendNewReturnRequestCustomerNotification(string fullName, string customerEmail, string orderId, int languageId)
        {
            var messageTemplate = GetLocalizedActiveMessageTemplate("ReturnRequest.NewCustomerNotification", languageId);
            if (messageTemplate == null)
                return 0;
            languageId = EnsureLanguageIsActive(languageId);
            var customerTokens = new List<Token>();
            customerTokens.Add(new Token("Customer.FullName", fullName));
            customerTokens.Add(new Token("OrderId", orderId));
            _messageTokenProvider.AddStoreTokens(customerTokens);
            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = customerEmail.Trim(); ;
            var toName = fullName;
            return SendNotification(messageTemplate, emailAccount,
                languageId, customerTokens,
                toEmail, toName);
        }

        public virtual int SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription, string registrationType, int languageId)
        {
            if (subscription == null)
                throw new ArgumentNullException("subscription");

            languageId = EnsureLanguageIsActive(languageId);
            MessageTemplate messageTemplate = null;
            if (string.IsNullOrEmpty(subscription.RefererEmail))
            {//self registerar
                messageTemplate = GetLocalizedActiveMessageTemplate(string.Format("NewsLetterSubscription.{0}", registrationType), languageId);
            }
            else 
            {//registered by friend
                messageTemplate = GetLocalizedActiveMessageTemplate(string.Format("NewsLetterSubscriptionFromFriend.{0}", registrationType), languageId);
            }
            if (messageTemplate == null)
                return 0;

            var orderTokens = GenerateTokens(subscription);

            var emailAccount = GetEmailAccountOfMessageTemplate(messageTemplate, languageId);
            var toEmail = subscription.Email;
            var toName = "";
            return SendNotification(messageTemplate, emailAccount,
                languageId, orderTokens,
                toEmail, toName);
        }

    

    }
}
