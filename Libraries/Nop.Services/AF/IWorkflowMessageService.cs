using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.Orders;

namespace Nop.Services.Messages
{
    public partial interface IWorkflowMessageService
    {
        int SendMailReceivedNotification(Customer customer, EmailAccount emailAccount, int languageId, string toName, string toEmail);
        int SendProductEmailAFriendMessage(string yourName, string friendName, int languageId, ProductVariant productVariant, string customerEmail, string friendsEmail, string personalMessage);
        int SendWishlistEmailAFriendMessage(Customer customer, int languageId, string customerEmail, string friendsEmail, string personalMessage, bool includingItems);
        int SendCustomerCareStoreOwnerNotification(int languageId, string eMail, string Message, string FullName, string Subject);
        int SendCustomerCareCustomerNotification(int languageId, string customerEmail, string FullName);
        int SendPriceOfferStoreOwnerNotification(string fullName, string email, string subject, string phoneNumber, int productVariantId, string productName, string explanation, int languageId,string priceOfferId,string discountPercentage,string sku,string productId,string priceWithDiscount);
        int SendPriceOfferCustomerNotification(string fullName, string email, string phoneNumber, int productId, string productName, string explanation, int languageId, string discountUrl, ProductVariant productVariant, string priceWithDiscount);
        int SendNewReturnRequestCustomerNotification(string fullName, string email, string orderId, int languageId);
        int SendCustomerCareStoreOwnerNotification(int languageId, string eMail, string Message, string FullName, string Subject,string ItemNumber);
        int SendCustomerCareStoreOwnerNotification(int languageId, string eMail, string Message, string FullName, string Subject, string ItemNumber, string phone);
        int SendNewsLetterSubscriptionActivationMessage(NewsLetterSubscription subscription, string registrationType, int languageId);
        //int SendNewsLetterSubscriptionFromFriendActivationMessage(NewsLetterSubscription subscription, string registrationType, int languageId);
        
    }
}
