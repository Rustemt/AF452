using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Web.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {

            routes.MapLocalizedRoute("CacheViewer",
                "cache",
                new { controller = "Common", action = "CacheViewer" },
                new[] { "Nop.Web.Controllers" });


            routes.MapLocalizedRoute("HomePage",
                          "",
                          new { controller = "Home", action = "Index0310" },
                          new[] { "Nop.Web.Controllers" });


            routes.MapLocalizedRoute("HomePageNew",
              "anasayfa",
              new { controller = "Home", action = "Index0310" },
              new[] { "Nop.Web.Controllers" });


            routes.MapLocalizedRoute("HomePageOld",
              "index",
              new { controller = "Home", action = "Index" },
              new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("RtbHouseDataFeed",
                           "datafeed_1",
                           new { controller = "Home", action = "ExportRetargetingProductFeed" },
                           new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("Rss20DataFeed",
                             "rss20DataFeed",
                             new { controller = "Catalog", action = "Rss20DataFeed" },
                             new[] { "Nop.Web.Controllers" });

            
            routes.MapLocalizedRoute("CampaignRegister",
                           "CampaignRegister/",
                            new { controller = "Home", action = "CampaignRegister" },
                            new[] { "Nop.Web.Controllers" });
            
            routes.MapLocalizedRoute("CampaingRegisterConvoke",
                           "CampaingRegisterConvoke/{id}",
                            new { controller = "Home", action = "CampaignFriendsConvoke" },
                            new[] { "Nop.Web.Controllers" });
           
            routes.MapLocalizedRoute("CampaignRecordSuccessFul",
                           "CampaignRecordSuccessFul",
                            new { controller = "Home", action = "CampaignRecordSuccessFul" },
                            new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("CampaignRegisterActive",
                           "CampaignRegisterActive/{id}",
                            new { controller = "Home", action = "CampaignRegisterActive" },
                            new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("CampaignRecordCompleted",
                           "CampaignRecordCompleted",
                            new { controller = "Home", action = "RegistedCompleted" },
                            new[] { "Nop.Web.Controllers" });
            
            //products

            routes.MapLocalizedRoute("Product",
                            "p/{productId}/{SeName}/{variantId}",
                            new { controller = "Catalog", action = "Product", SeName = UrlParameter.Optional, variantId = UrlParameter.Optional },
                            new { productId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("ProductExt",
                           "p/{productId}/{SeName}/{variantId}",
                           new { controller = "Catalog", action = "Product", SeName = UrlParameter.Optional },
                           new { productId = @"\d+" },
                           new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("RecentlyViewedProducts",
                            "recentlyviewedproducts/",
                            new { controller = "Catalog", action = "RecentlyViewedProducts" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("RecentlyAddedProducts",
                            "newproducts/",
                            new { controller = "Catalog", action = "RecentlyAddedProducts280" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("RecentlyAddedProductsRSS",
                            "newproducts/rss",
                            new { controller = "Catalog", action = "RecentlyAddedProductsRss" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("ProductsWithImages",
                            "product_images/",
                            new { controller = "Catalog", action = "ProductsWithImages" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CreateProductImages",
                            "cpi/{pictureSize}",
                            new { controller = "Catalog", action = "CreateProductImages" },
                            new[] { "Nop.Web.Controllers" });
            
            //comparing products
            routes.MapLocalizedRoute("AddProductToCompare",
                            "compareproducts/add/{productId}",
                            new { controller = "Catalog", action = "AddProductToCompareList" },
                            new { productId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CompareProducts",
                            "compareproducts/",
                            new { controller = "Catalog", action = "CompareProducts" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("RemoveProductFromCompareList",
                            "compareproducts/remove/{productId}",
                            new { controller = "Catalog", action = "RemoveProductFromCompareList"},
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("ClearCompareList",
                            "clearcomparelist/",
                            new { controller = "Catalog", action = "ClearCompareList" },
                            new[] { "Nop.Web.Controllers" });
            
            //product email a friend
            routes.MapLocalizedRoute("ProductEmailAFriend",
                            "productemailafriend/{variantId}",
                            new { controller = "Catalog", action = "ProductEmailAFriend" },
                            new { variantId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });

            //catalog

            //AF
            routes.MapLocalizedRoute("CategoryMain",
                           "catalog/{categoryId}/{SeName}",
                           new { controller = "Catalog", action = "CategoryMain", SeName = UrlParameter.Optional },
                           new { categoryId = @"\d+" },
                           new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("Category",
                            "c/{categoryId}/{SeName}",
                            new { controller = "Catalog", action = "CategoryProducts280", SeName = UrlParameter.Optional },
                            new { categoryId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("ManufacturerList",
                            "catalog/ManufacturerAll/",
                            new { controller = "Catalog", action = "ManufacturerAll" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("ManufacturersAll",
                          "catalog/CategoryManufacturersAll",
                          new { controller = "Catalog", action = "CategoryManufacturersAll" },
                          new[] { "Nop.Web.Controllers" });

           //AF
            routes.MapLocalizedRoute("CategoryManufacturerList",
                           "manufacturer/{categoryId}/{SeName}",
                           new { controller = "Catalog", action = "CategoryManufacturers", SeName = UrlParameter.Optional },
                           new { categoryId = @"\d+" },
                           new[] { "Nop.Web.Controllers" });

			routes.MapLocalizedRoute("ManufacturerByLetterList",
						   "manufacturer/{mLetter}",
						   new { controller = "Catalog", action = "ManufacturersByStartingLetter" },
						   new[] { "Nop.Web.Controllers" });

            //AF
            routes.MapLocalizedRoute("FilteredManufacturerList",
                           "manufacturer/{filter}",
                           new { controller = "Catalog", action = "FilteredManufacturerAll" },
                           new[] { "Nop.Web.Controllers" });
            //AF
            routes.MapLocalizedRoute("ManufacturerCategory",
                           "m/{manufacturerId}/{mSeName}/{categoryId}/{cSeName}",
                           new { controller = "Catalog", action = "Manufacturer", mSeName = UrlParameter.Optional, cSeName = UrlParameter.Optional },
                           new { categoryId = @"\d+", manufacturerId = @"\d+" },
                           new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("Manufacturer",
                            "m/{manufacturerId}/{SeName}",
                            new { controller = "Catalog", action = "Manufacturer", SeName = UrlParameter.Optional },
                            new { manufacturerId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });

            //reviews
            //routes.MapLocalizedRoute("ProductReviews",
            //                "productreviews/{productId}",
            //                new { controller = "Catalog", action = "ProductReviews" },
            //                new[] { "Nop.Web.Controllers" });

            //login, register
            routes.MapLocalizedRoute("Login",
                            "login/",
                            new { controller = "Customer", action = "Login" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("LoginCheckoutAsGuest",
                            "login/checkoutAsGuest",
                            new { controller = "Customer", action = "Login", checkoutAsGuest = true },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("Register",
                            "register/",
                            new { controller = "Customer", action = "Register" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("Logout",
                            "logout/",
                            new { controller = "Customer", action = "Logout" },
                            new[] { "Nop.Web.Controllers" });

            //shopping cart

            routes.MapLocalizedRoute("AddProductToCart",
                            "cart/addproduct/{productId}",
                            new { controller = "ShoppingCart", action = "AddProductToCart" },
                            new { productId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("ShoppingCart",
                            "cart/",
                            new { controller = "ShoppingCart", action = "Cart" },
                            new[] { "Nop.Web.Controllers" });
            //wishlist
            routes.MapLocalizedRoute("Wishlist",
                            "wishlist/{customerGuid}",
                            new { controller = "ShoppingCart", action = "Wishlist", customerGuid = UrlParameter.Optional },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("EmailWishlist",
                            "emailwishlist",
                            new { controller = "ShoppingCart", action = "EmailWishlist" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("UpdateProduct",
                            "update/wishitem/{id}/{comment}",
                            new { controller = "ShoppingCart", action = "UpdateWishlist", id = UrlParameter.Optional, comment = UrlParameter.Optional },
                            new[] { "Nop.Web.Controllers" });
                            


            //checkout
            routes.MapLocalizedRoute("Checkout",
                            "checkout/",
                            new { controller = "Checkout", action = "Index" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutOnePage",
                            "onepagecheckout/",
                            new { controller = "Checkout", action = "OnePageCheckout" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutShippingAddress",
                            "checkout/shippingaddress",
                            new { controller = "Checkout", action = "ShippingAddress" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutBillingAddress",
                            "checkout/billingaddress",
                            new { controller = "Checkout", action = "BillingAddress" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutShippingMethod",
                            "checkout/shippingmethod",
                            new { controller = "Checkout", action = "ShippingMethod" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutPaymentMethod",
                            "checkout/paymentmethod",
                            new { controller = "Checkout", action = "PaymentMethod" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutPaymentInfo",
                            "checkout/index",
                            new { controller = "Checkout", action = "PaymentInfo" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutConfirm",
                            "checkout/confirm",
                            new { controller = "Checkout", action = "Confirm" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("CheckoutCompleted",
                            "checkout/completed",
                            new { controller = "Checkout", action = "Completed" },
                            new[] { "Nop.Web.Controllers" });

            //orders
            routes.MapLocalizedRoute("OrderDetails",
                            "orderdetails/{orderId}",
                            new { controller = "Order", action = "Details" },
                            new { orderId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("OrderDetail",
                        "orderdetail/{orderId}",
                        new { controller = "Order", action = "Detail" },
                        new { orderId = @"\d+" },
                        new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("ReturnRequest",
                            "returnrequest/{orderId}",
                            new { controller = "Order", action = "ReturnRequest" },
                            new { orderId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("GetOrderPdfInvoice",
                            "orderdetails/pdf/{orderId}",
                            new { controller = "Order", action = "GetPdfInvoice" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("PrintOrderDetails",
                            "orderdetails/print/{orderId}",
                            new { controller = "Order", action = "PrintOrderDetails" },
                            new[] { "Nop.Web.Controllers" });


            //contact us
            routes.MapLocalizedRoute("ContactUs",
                            "contactus",
                            new { controller = "Common", action = "ContactUs" },
                            new[] { "Nop.Web.Controllers" });



            //customer support
            routes.MapLocalizedRoute("CustomerSupport",
                            "customersupport",
                            new { controller = "Common", action = "CustomerSupport"},
                            new[] { "Nop.Web.Controllers" });


            //passwordrecovery
            routes.MapLocalizedRoute("PasswordRecovery",
                            "passwordrecovery",
                            new { controller = "Customer", action = "PasswordRecovery" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("PasswordRecoveryConfirm",
                            "passwordrecovery/confirm",
                            new { controller = "Customer", action = "PasswordRecoveryConfirm" },
                            new[] { "Nop.Web.Controllers" });

            //newsletters
            routes.MapLocalizedRoute("NewsletterActivation",
                            "newsletter/subscriptionactivation/{token}/{active}",
                            new { controller = "Newsletter", action = "SubscriptionActivation" },
                            new[] { "Nop.Web.Controllers" });
            


            //customer
            routes.MapLocalizedRoute("AccountActivation",
                            "customer/activation/{token}/{email}",
                            new { controller = "Customer", action = "AccountActivation" },
                            new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("CustomerProfile",
            //                "profile/{id}",
            //                new { controller = "Profile", action = "Index" },
            //                new { id = @"\d+"},
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("CustomerProfilePaged",
            //                "profile/{id}/page/{page}",
            //                new { controller = "Profile", action = "Index"},
            //                new {  id = @"\d+", page = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("CustomerForumSubscriptions",
            //                "customer/forumsubscriptions",
            //                new { controller = "Customer", action = "ForumSubscriptions"},
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("CustomerForumSubscriptionsPaged",
            //                "customer/forumsubscriptions/{page}",
            //                new { controller = "Customer", action = "ForumSubscriptions", page = UrlParameter.Optional },
            //                new { page = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });

            //blog
            //routes.MapLocalizedRoute("Blog",
            //                "blog",
            //                new { controller = "Blog", action = "List" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("BlogRSS",
            //                "blog/rss/{languageId}",
            //                new { controller = "Blog", action = "ListRss" },
            //                new { languageId = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("BlogPost",
            //                "blog/{blogPostId}/{SeName}",
            //                new { controller = "Blog", action = "BlogPost", SeName = UrlParameter.Optional },
            //                new { blogPostId = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("BlogByTag",
            //                "blog/tag/{tag}",
            //                new { controller = "Blog", action = "List" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("BlogByMonth",
            //                "blog/month/{month}",
            //                new { controller = "Blog", action = "List" },
            //                new[] { "Nop.Web.Controllers" });

            ////forum
            //routes.MapLocalizedRoute("Boards",
            //                "boards",
            //                new { controller = "Boards", action = "Index" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("ActiveDiscussions",
            //                "boards/activediscussions",
            //                new { controller = "Boards", action = "ActiveDiscussions" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("ActiveDiscussionsRSS",
            //                "boards/activediscussionsrss",
            //                new { controller = "Boards", action = "ActiveDiscussionsRSS" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("PostEdit",
            //                "boards/postedit/{id}",
            //                new { controller = "Boards", action = "PostEdit" },
            //                new { id = @"\d+"},
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("PostDelete",
            //                "boards/postdelete/{id}",
            //                new { controller = "Boards", action = "PostDelete" },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("PostCreate",
            //                "boards/postcreate/{id}",
            //                new { controller = "Boards", action = "PostCreate"},
            //                new { id = @"\d+"},
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("PostCreateQuote",
            //                "boards/postcreate/{id}/{quote}",
            //                new { controller = "Boards", action = "PostCreate"},
            //                new { id = @"\d+", quote = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("TopicEdit",
            //                "boards/topicedit/{id}",
            //                new { controller = "Boards", action = "TopicEdit"},
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("TopicDelete",
            //                "boards/topicdelete/{id}",
            //                new { controller = "Boards", action = "TopicDelete"},
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("TopicCreate",
            //                "boards/topiccreate/{id}",
            //                new { controller = "Boards", action = "TopicCreate" },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("TopicMove",
            //                "boards/topicmove/{id}",
            //                new { controller = "Boards", action = "TopicMove" },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("TopicWatch",
            //                "boards/topicwatch/{id}",
            //                new { controller = "Boards", action = "TopicWatch" },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("TopicSlug",
            //                "boards/topic/{id}/{slug}",
            //                new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional },
            //                new { id = @"\d+"},
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("TopicSlugPaged",
            //                "boards/topic/{id}/{slug}/page/{page}",
            //                new { controller = "Boards", action = "Topic", slug = UrlParameter.Optional, page = UrlParameter.Optional },
            //                new { id = @"\d+", page = @"\d+"},
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("ForumWatch",
            //                "boards/forumwatch/{id}",
            //                new { controller = "Boards", action = "ForumWatch" },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("ForumRSS",
            //                "boards/forumrss/{id}",
            //                new { controller = "Boards", action = "ForumRSS" },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("ForumSlug",
            //                "boards/forum/{id}/{slug}",
            //                new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("ForumSlugPaged",
            //                "boards/forum/{id}/{slug}/page/{page}",
            //                new { controller = "Boards", action = "Forum", slug = UrlParameter.Optional, page = UrlParameter.Optional },
            //                new { id = @"\d+", page = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("ForumGroupSlug",
            //                "boards/forumgroup/{id}/{slug}",
            //                new { controller = "Boards", action = "ForumGroup", slug = UrlParameter.Optional },
            //                new { id = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });
            //routes.MapLocalizedRoute("Search",
            //                "boards/search",
            //                new { controller = "Boards", action = "Search" },
            //                new[] { "Nop.Web.Controllers" });

            ////private messages
            //routes.MapLocalizedRoute("PrivateMessages",
            //                "privatemessages/{tab}",
            //                new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
            //                new[] { "Nop.Web.Controllers" });

            //routes.MapLocalizedRoute("PrivateMessagesPaged",
            //                "privatemessages/{tab}/page/{page}",
            //                new { controller = "PrivateMessages", action = "Index", tab = UrlParameter.Optional },
            //                new { page = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });

            //routes.MapLocalizedRoute("PrivateMessagesInbox",
            //                "inboxupdate",
            //                new { controller = "PrivateMessages", action = "InboxUpdate" },
            //                new[] { "Nop.Web.Controllers" });

            //routes.MapLocalizedRoute("PrivateMessagesSent",
            //                "sentupdate",
            //                new { controller = "PrivateMessages", action = "SentUpdate" },
            //                new[] { "Nop.Web.Controllers" });

            //routes.MapLocalizedRoute("SendPM",
            //                "sendpm/{toCustomerId}",
            //                new { controller = "PrivateMessages", action = "SendPM" },
            //                new { toCustomerId = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });

            //routes.MapLocalizedRoute("SendPMReply",
            //                "sendpm/{toCustomerId}/{replyToMessageId}",
            //                new { controller = "PrivateMessages", action = "SendPM" },
            //                new { toCustomerId = @"\d+", replyToMessageId = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });

            //routes.MapLocalizedRoute("ViewPM",
            //                "viewpm/{privateMessageId}",
            //                new { controller = "PrivateMessages", action = "ViewPM" },
            //                new { privateMessageId = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });

            //routes.MapLocalizedRoute("DeletePM",
            //                "deletepm/{privateMessageId}",
            //                new { controller = "PrivateMessages", action = "DeletePM" },
            //                new { privateMessageId = @"\d+" },
            //                new[] { "Nop.Web.Controllers" });

            //news
            routes.MapLocalizedRoute("NewsArchive",
                            "news",
                            new { controller = "News", action = "List" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("NewsRSS",
                            "news/rss/{languageId}",
                            new { controller = "News", action = "ListRss" },
                            new { languageId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("NewsItem",
                            "news/{newsItemId}/{SeName}",
                            new { controller = "News", action = "NewsItem", SeName = UrlParameter.Optional },
                            new { newsItemId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("InterviewItem",
                        "interview/{newsItemId}/{SeName}",
                        new { controller = "News", action = "InterviewItem", SeName = UrlParameter.Optional },
                        new { newsItemId = @"\d+" },
                        new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("NewsItemProducts",
                            "storyproducts/{newsItemId}/{SeName}",
                            new { controller = "Catalog", action = "NewsItemProducts", SeName = UrlParameter.Optional },
                            new { newsItemId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });


            

            //topics
            routes.MapLocalizedRoute("Topic",
                            "t/{SystemName}",
                            new { controller = "Topic", action = "TopicDetails" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("TopicPopup",
                            "t-popup/{SystemName}",
                            new { controller = "Topic", action = "TopicDetailsPopup" },
                            new[] { "Nop.Web.Controllers" });   
            //sitemaps
            routes.MapLocalizedRoute("Sitemap",
                            "sitemap",
                            new { controller = "Common", action = "Sitemap" },
                            new[] { "Nop.Web.Controllers" });
            routes.MapLocalizedRoute("SitemapSEO",
                            "sitemapseo",
                            new { controller = "Common", action = "SitemapSeo" },
                            new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("SitemapImage",
                            "sitemapimage",
                            new { controller = "Common", action = "SitemapImage" },
                            new[] { "Nop.Web.Controllers" });

            //product tags
            routes.MapLocalizedRoute("ProductsByTag",
                            "productag/{productTagId}/{SeName}",
                            new { controller = "Catalog", action = "ProductsByTag", SeName = UrlParameter.Optional },
                            new { productTagId = @"\d+" },
                            new[] { "Nop.Web.Controllers" });
            
            //product search
            routes.MapLocalizedRoute("ProductSearch",
                            "search/",
                            new { controller = "Catalog", action = "Search280" },
                            new[] { "Nop.Web.Controllers" });



            routes.MapLocalizedRoute("ManufacturerCategorySe",
                        "{SeName}/{categoryId}/{cSeName}",
                        new { controller = "Catalog", action = "ManufacturerSe280", cSeName = UrlParameter.Optional },
                        new { categoryId = @"\d+"},
                        new[] { "Nop.Web.Controllers" });

            routes.MapLocalizedRoute("ManufacturerSe",
                           "{SeName}",
                           new { controller = "Catalog", action = "ManufacturerSe280"},
                           new[] { "Nop.Web.Controllers" });



            

        }

        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}
