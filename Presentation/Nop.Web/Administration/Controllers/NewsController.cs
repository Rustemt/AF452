using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Nop.Admin.Models.News;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.News;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Telerik.Web.Mvc;
using Nop.Services.Media;
using Nop.Services.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Admin.Models.Catalog;

namespace Nop.Admin.Controllers
{
	[AdminAuthorize]
    public class NewsController : BaseNopController
	{
		#region Fields

        private readonly INewsService _newsService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerContentService _customerContentService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IPictureService _pictureService;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IExtraContentService _extraContentService;
       
		#endregion

		#region Constructors

        public NewsController(INewsService newsService, ILanguageService languageService,
            IDateTimeHelper dateTimeHelper, ICustomerContentService customerContentService,
            ILocalizationService localizationService, IPermissionService permissionService,
            AdminAreaSettings adminAreaSettings, IPictureService pictureService, 
             ICategoryService categoryService,IManufacturerService manufacturerService,
            IProductService productService, IWorkContext workContext, IExtraContentService extraContentService)
        {
            this._newsService = newsService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerContentService = customerContentService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._adminAreaSettings = adminAreaSettings;
            this._pictureService = pictureService;
            this._productService = productService;
            this._workContext = workContext;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._extraContentService = extraContentService;
		}

		#endregion 
        
        #region News items

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();
            var model = new NewsItemListModel();
            //system types
            model.AvailableSystemTypeNames = NewsType.HomeMainContent.ToSelectList(false).ToList();
            model.AvailableSystemTypeNames.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            var news = _newsService.GetAllNews(0, null, null, 0, _adminAreaSettings.GridPageSize, true);
            model.NewsItems = new GridModel<NewsItemModel>
            {
                Data = news.Select(x =>
                {
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.LanguageName = x.Language.Name;
                    m.SystemType = (NewsType)x.SystemTypeId;
                    m.Comments = x.NewsComments.Count;
                    return m;
                }),
                Total = news.TotalCount
            };
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command,NewsItemListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();


            var news = _newsService.GetAllNewsSearch(command.Page - 1, command.PageSize, model.SearchTitle, model.SearchSystemTypeId);
            var gridModel = new GridModel<NewsItemModel>
            {
                Data = news.Select(x =>
                {
                    var m = x.ToModel();
                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                    m.LanguageName = x.Language.Name;
                    m.Comments = x.NewsComments.Count;
                    m.SystemType = (NewsType)x.SystemTypeId;
                    return m;
                }),
                Total = news.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new NewsItemModel();
            //default values
            model.Published = true;
            model.AllowComments = true;
            PrepareCategoryMapping(model);
            PrepareManufacturerMapping(model);
            PrepareAddNewsItemPictureModel(model);
            return View(model);
        }

        [HttpPost, FormValueExists("save", "save-continue", "continueEditing")]
        public ActionResult Create(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var newsItem = model.ToEntity();
                newsItem.CreatedOnUtc = DateTime.UtcNow;
                _newsService.InsertNews(newsItem);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            PrepareAddNewsItemPictureModel(model);
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = newsItem.ToModel();
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.CreatedOnUtc, DateTimeKind.Utc);
            PrepareAddNewsItemPictureModel(model);
            ////categories
            //model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            //foreach (var c in _categoryService.GetAllCategories(true))
            //    model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            ////manufacturers
            //model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            //foreach (var m in _manufacturerService.GetAllManufacturers(true))
            //    model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });
            PrepareCategoryMapping(model);
            PrepareManufacturerMapping(model);
            return View(model);
        }

        [HttpPost, FormValueExists("save", "save-continue", "continueEditing")]
        public ActionResult Edit(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(model.Id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsItem = model.ToEntity(newsItem);
                newsItem.CreatedOnUtc = _dateTimeHelper.ConvertToUtcTime(model.CreatedOn, _dateTimeHelper.CurrentTimeZone);
                _newsService.UpdateNews(newsItem);

                SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            PrepareAddNewsItemPictureModel(model);
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(true))
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });
            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                //No news item found with the specified id
                return RedirectToAction("List");

            _newsService.DeleteNews(newsItem);

            SuccessNotification(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Comments

        public ActionResult Comments(int? filterByNewsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.FilterByNewsItemId = filterByNewsItemId;
            var model = new GridModel<NewsCommentModel>();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Comments(int? filterByNewsItemId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            IList<NewsComment> comments;
            if (filterByNewsItemId.HasValue)
            {
                //filter comments by news item
                var newsItem = _newsService.GetNewsById(filterByNewsItemId.Value);
                comments = newsItem.NewsComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all news comments
                comments = _customerContentService.GetAllCustomerContent<NewsComment>(0, null);
            }

            var gridModel = new GridModel<NewsCommentModel>
            {
                Data = comments.PagedForCommand(command).Select(newsComment =>
                {
                    var commentModel = new NewsCommentModel();
                    commentModel.Id = newsComment.Id;
                    commentModel.NewsItemId = newsComment.NewsItemId;
                    commentModel.NewsItemTitle = newsComment.NewsItem.Title;
                    commentModel.CustomerId = newsComment.CustomerId;
                    commentModel.IpAddress = newsComment.IpAddress;
                    commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc);
                    commentModel.CommentTitle = newsComment.CommentTitle;
                    commentModel.CommentText = Core.Html.HtmlHelper.FormatText(newsComment.CommentText, false, true, false, false, false, false);
                    return commentModel;
                }),
                Total = comments.Count,
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CommentDelete(int? filterByNewsItemId, int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var comment = _customerContentService.GetCustomerContentById(id);
            if (comment == null)
                throw new ArgumentException("No comment found with the specified id");
            _customerContentService.DeleteCustomerContent(comment);

            return Comments(filterByNewsItemId, command);
        }


        #endregion

        #region NewsItem extra contents
        [GridAction]
        public ActionResult ExtraContentsSelect(int newsItemId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null)
                throw new ArgumentException("No newsItem found with the specified id", "newsItemId");

            var extraContents = newsItem.ExtraContents.OrderByDescending(a => a.Id).ToList();
            
            var gridModel = new GridModel<ExtraContentModel>
            {
                Data = extraContents.Select(x =>
                {
                    var model = x.ToModel();
                    model.Id = x.Id;
                    model.FullDescription = x.FullDescription;
                    model.DisplayOrder = x.DisplayOrder;
                    model.Title = x.Title;
                    return model;
                }),
                Total = extraContents.Count
            };
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction]
        public ActionResult ExtraContentDelete(int newsItemId, int extraContentId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null)
                throw new ArgumentException("No newsItem found with the specified id", "newsItemId");

            var extraContent = newsItem.ExtraContents.Where(a => a.Id == extraContentId).FirstOrDefault();
            newsItem.RemoveExtraContent(extraContent);
            _newsService.UpdateNews(newsItem);
            //now delete the extracontent record
            _extraContentService.DeleteExtraContent(extraContent);
            return ExtraContentsSelect(newsItemId, command);
        }

        public ActionResult ExtraContentCreate(int newsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null)
                //No customer found with the specified id
                return RedirectToAction("List");

            var model = new NewsItemExtraContentModel();
            model.ExtraContent = new ExtraContentModel();
            model.NewsItemId = newsItemId;
            
            return View(model);
        }

        [HttpPost]
        public ActionResult ExtraContentCreate(NewsItemExtraContentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var newsItem =_newsService.GetNewsById(model.NewsItemId);
            if (newsItem == null)
                //No newsItem found with the specified id
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var extraContent = model.ExtraContent.ToEntity();
                
                newsItem.AddExtraContent(extraContent);
                _newsService.UpdateNews(newsItem);

                SuccessNotification(_localizationService.GetResource("Admin.NewsItems.ExtraContents.Added"));
                return RedirectToAction("ExtraContentEdit", new { extraContentId = extraContent.Id, newsItemId = model.NewsItemId });
            }

            //If we got this far, something failed, redisplay form
            model.NewsItemId = newsItem.Id;
           
            return View(model);
        }

        public ActionResult ExtraContentEdit(int extraContentId, int newsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null)
                //No newsItem found with the specified id
                return RedirectToAction("List");

            var extraContent = _extraContentService.GetExtraContentById(extraContentId);
            if (extraContent == null)
                //No etracontent found with the specified id
                return RedirectToAction("Edit", new { id = newsItem.Id });

            var model = new NewsItemExtraContentModel();
            model.NewsItemId = newsItemId;
            model.ExtraContent = extraContent.ToModel();
            
            return View(model);
        }

        [HttpPost]
        public ActionResult ExtraContentEdit(NewsItemExtraContentModel model)
        {
            var newsItem = _newsService.GetNewsById(model.NewsItemId);
            if (newsItem == null)
                //No newsItem found with the specified id
                return RedirectToAction("List");

            var extraContent = _extraContentService.GetExtraContentById(model.ExtraContent.Id);
            if (extraContent == null)
                //No etracontent found with the specified id
                return RedirectToAction("Edit", new { id = newsItem.Id });
            
            if (ModelState.IsValid)
            {
                extraContent = model.ExtraContent.ToEntity(extraContent);
                _extraContentService.UpdateExtraContent(extraContent);

                SuccessNotification(_localizationService.GetResource("Admin.News.ExtraContents.Updated"));
                return RedirectToAction("ExtraContentEdit", new { newsItemId = model.NewsItemId, extraContentId = model.ExtraContent.Id });
            }

            //If we got this far, something failed, redisplay form
            model.NewsItemId = newsItem.Id;
            model.ExtraContent = extraContent.ToModel();
           
            return View(model);
        }
        #endregion extra contents

        #region NewsItem pictures

        public ActionResult NewsItemPictureAdd(int pictureId, int displayOrder, int newsItemId, int pictureType)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            if (pictureId == 0)
                throw new ArgumentException();

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null)
                throw new ArgumentException("No newsItem found with the specified id");

            _newsService.InsertNewsItemPicture(new NewsItemPicture()
            {
                PictureId = pictureId,
                NewsItemId = newsItemId,
                DisplayOrder = displayOrder,
                NewsItemPictureTypeId = pictureType,
                NewsItemPictureType = (NewsItemPictureType)pictureType
            });

            _pictureService.SetSeoFilename(pictureId, _pictureService.GetPictureSeName(newsItem.Title));

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemPictureList(GridCommand command, int newsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemPictures = _newsService.GetNewsItemPicturesByNewsItemId(newsItemId, null);
            var newsItemPicturesModel = newsItemPictures
                .Select(x =>
                {
                    return new NewsItemModel.NewsItemPictureModel()
                    {
                        Id = x.Id,
                        NewsItemId = x.NewsItemId,
                        PictureId = x.PictureId,
                        PictureUrl = _pictureService.GetPictureUrl(x.PictureId),
                        DisplayOrder = x.DisplayOrder,
                        PictureType = x.NewsItemPictureType,
                        PictureTypeId = x.NewsItemPictureTypeId
                    };
                })
                .ToList();

            var model = new GridModel<NewsItemModel.NewsItemPictureModel>
            {
                Data = newsItemPicturesModel,
                Total = newsItemPicturesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemPictureUpdate(NewsItemModel.NewsItemPictureModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemPicture = _newsService.GetNewsItemPictureById(model.Id);
            if (newsItemPicture == null)
                throw new ArgumentException("No newsItem picture found with the specified id");

            newsItemPicture.DisplayOrder = model.DisplayOrder;
            newsItemPicture.NewsItemPictureType = model.PictureType;
            newsItemPicture.NewsItemPictureTypeId = model.PictureTypeId;
            _newsService.UpdateNewsItemPicture(newsItemPicture);

            return NewsItemPictureList(command, newsItemPicture.NewsItemId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemPictureDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemPicture = _newsService.GetNewsItemPictureById(id);
            if (newsItemPicture == null)
                throw new ArgumentException("No newsItem picture found with the specified id");

            var newsItemId = newsItemPicture.NewsItemId;
            _newsService.DeleteNewsItemPicture(newsItemPicture);

            var picture = _pictureService.GetPictureById(newsItemPicture.PictureId);
            _pictureService.DeletePicture(picture);

            return NewsItemPictureList(command, newsItemId);
        }  
        
        [NonAction]
        private void PrepareAddNewsItemPictureModel(NewsItemModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (model.AddPictureModel == null)
                model.AddPictureModel = new NewsItemModel.NewsItemPictureModel();
        }

        #endregion

        #region Products

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductList(GridCommand command, int newsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItemProducts = _newsService.GetNewsItemProductsByNewsItemId(newsItemId, true);
            var NewsItemProductsModel = newsItemProducts
                .Select(x =>
                {
                    return new NewsItemModel.NewsItemProductModel()
                    {
                        Id = x.Id,
                        NewsItemId = x.NewsItemId,
                        ProductId = x.ProductId,
                        ProductName = _productService.GetProductById(x.ProductId).Name,
                        IsFeaturedProduct = x.IsFeaturedProduct,
                        DisplayOrder1 = x.DisplayOrder
                    };
                })
                .ToList();

            var model = new GridModel<NewsItemModel.NewsItemProductModel>
            {
                Data = NewsItemProductsModel,
                Total = NewsItemProductsModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductUpdate(GridCommand command, NewsItemModel.NewsItemProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItemProduct = _newsService.GetNewsItemProductById(model.Id);
            if (newsItemProduct == null)
                throw new ArgumentException("No news product mapping found with the specified id");

            newsItemProduct.IsFeaturedProduct = model.IsFeaturedProduct;
            newsItemProduct.DisplayOrder = model.DisplayOrder1;
            _newsService.UpdateNewsItemProduct(newsItemProduct);

            return ProductList(command, newsItemProduct.NewsItemId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ProductDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItemProduct = _newsService.GetNewsItemProductById(id);
            if (newsItemProduct == null)
                throw new ArgumentException("No news product mapping found with the specified id");

            var newsItemId = newsItemProduct.NewsItemId;
            _newsService.DeleteNewsItemProduct(newsItemProduct);

            return ProductList(command, newsItemId);
        }

        public ActionResult ProductAddPopup(int newsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var products = new PagedList<Product>(new List<Product>(), 0, 1);

            var model = new NewsItemModel.AddNewsItemProductModel();
            model.Products = new GridModel<ProductModel>
            {
                Data = products.Select(x => x.ToModel()),
                Total = products.TotalCount
            };
            //categories
            model.AvailableCategories.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var c in _categoryService.GetAllCategories(true))
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });

            //manufacturers
            model.AvailableManufacturers.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ProductAddPopupList(GridCommand command, NewsItemModel.AddNewsItemProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var gridModel = new GridModel();
             IPagedList<Product> products = null;
             if (model.SearchCategoryId == 0
                 && model.SearchManufacturerId == 0
                 && string.IsNullOrEmpty(model.SearchProductName))
             {
                 products = new PagedList<Product>(new List<Product>(), 0, 1);
             }
             else
             {

                 products = _productService.SearchProducts(model.SearchCategoryId,
                     model.SearchManufacturerId, null, null, null, 0, model.SearchProductName, false,
                     _workContext.WorkingLanguage.Id, new List<int>(),
                     ProductSortingEnum.Position, command.Page - 1, command.PageSize, true);
                 gridModel.Data = products.Select(x => x.ToModel());
                 gridModel.Total = products.TotalCount;
             }
            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult ProductAddPopup(string btnId, string formId, NewsItemModel.AddNewsItemProductModel model, string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();
            
            var ids = selectedIds
                   .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(x => Convert.ToInt32(x))
                   .ToArray();

            if (ids.Length>0)
            {
                foreach (int id in ids)
                {
                    var product = _productService.GetProductById(id);
                    if (product != null)
                    {
                        var existingNewsItemProducts = _newsService.GetNewsItemProductsByNewsItemId(model.NewsItemId);
                        if (existingNewsItemProducts.FindNewsItemProduct(id, model.NewsItemId) == null)
                        {
                            _newsService.InsertNewsItemProduct(
                                new NewsItemProduct()
                                {
                                    NewsItemId = model.NewsItemId,
                                    ProductId = id,
                                    IsFeaturedProduct = false,
                                    DisplayOrder = 1
                                });
                        }
                    }
                }
            }



            //if (model.SelectedProductIds != null)
            //{
            //    foreach (int id in model.SelectedProductIds)
            //    {
            //        var product = _productService.GetProductById(id);
            //        if (product != null)
            //        {
            //            var existingNewsItemProducts = _newsService.GetNewsItemProductsByNewsItemId(model.NewsItemId);
            //            if (existingNewsItemProducts.FindNewsItemProduct(id, model.NewsItemId) == null)
            //            {
            //                _newsService.InsertNewsItemProduct(
            //                    new NewsItemProduct()
            //                    {
            //                        NewsItemId = model.NewsItemId,
            //                        ProductId = id,
            //                        IsFeaturedProduct = false,
            //                        DisplayOrder = 1
            //                    });
            //            }
            //        }
            //    }
            //}

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;
            model.Products = new GridModel<ProductModel>();
            return View(model);
        }

        #endregion

        #region utilities

        [NonAction]
        private void PrepareManufacturerMapping(NewsItemModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.NumberOfAvailableManufacturers = _manufacturerService.GetAllManufacturers(true).Count;
        }
        [NonAction]
        private void PrepareCategoryMapping(NewsItemModel model)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            model.NumberOfAvailableCategories = _categoryService.GetAllCategories(true).Count;
        }
        #endregion

        #region News categories

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemCategoryList(GridCommand command, int newsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemCategories = _categoryService.GetNewsItemCategoriesByNewsItemId(newsItemId, true);
            var newsItemCategoriesModel = newsItemCategories
                .Select(x =>
                {
                    return new NewsItemModel.NewsItemCategoryModel()
                    {
                        Id = x.Id,
                        Category = _categoryService.GetCategoryById(x.CategoryId).GetCategoryBreadCrumb(_categoryService),
                        NewsItemId = x.NewsItemId,
                        CategoryId = x.CategoryId
                    };
                })
                .ToList();

            var model = new GridModel<NewsItemModel.NewsItemCategoryModel>
            {
                Data =newsItemCategoriesModel,
                Total = newsItemCategoriesModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemCategoryInsert(GridCommand command, NewsItemModel.NewsItemCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemCategory = new NewsItemCategory()
            {
                NewsItemId = model.NewsItemId,
                CategoryId = Int32.Parse(model.Category), //use Category property (not CategoryId) because appropriate property is stored in it
                //DisplayOrder = model.DisplayOrder
                CreatedOnUtc =  DateTime.UtcNow
            };
            _categoryService.InsertNewsItemCategory(newsItemCategory);

            return NewsItemCategoryList(command, model.NewsItemId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemCategoryUpdate(GridCommand command, NewsItemModel.NewsItemCategoryModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemCategory = _categoryService.GetNewsItemCategoryById(model.Id);
            if (newsItemCategory == null)
                throw new ArgumentException("No newsitem category mapping found with the specified id");

            //use Category property (not CategoryId) because appropriate property is stored in it
            newsItemCategory.CategoryId = Int32.Parse(model.Category);
            _categoryService.UpdateNewsItemCategory(newsItemCategory);

            return NewsItemCategoryList(command, newsItemCategory.NewsItemId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemCategoryDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemCategory = _categoryService.GetNewsItemCategoryById(id);
            if (newsItemCategory == null)
                throw new ArgumentException("No newsItem category mapping found with the specified id");

            var newsItemId = newsItemCategory.NewsItemId;
            _categoryService.DeleteNewsItemCategory(newsItemCategory);
            return NewsItemCategoryList(command, newsItemId);
        }
        #endregion

        #region NewsItem manufacturers

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemManufacturerList(GridCommand command, int newsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemManufacturers = _manufacturerService.GetNewsItemManufacturersByNewsItemtId(newsItemId, true);
            var newsItemManufacturersModel = newsItemManufacturers
                .Select(x =>
                {
                    return new NewsItemModel.NewsItemManufacturerModel()
                    {
                        Id = x.Id,
                        Manufacturer = _manufacturerService.GetManufacturerById(x.ManufacturerId).Name,
                        NewsItemId = x.NewsItemId,
                        ManufacturerId = x.ManufacturerId
                    };
                })
                .ToList();

            var model = new GridModel<NewsItemModel.NewsItemManufacturerModel>
            {
                Data = newsItemManufacturersModel,
                Total = newsItemManufacturersModel.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemManufacturerInsert(GridCommand command, NewsItemModel.NewsItemManufacturerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemManufacturer = new NewsItemManufacturer()
            {
                NewsItemId = model.NewsItemId,
                ManufacturerId = Int32.Parse(model.Manufacturer), //use Manufacturer property (not ManufacturerId) because appropriate property is stored in it
                //DisplayOrder = model.DisplayOrder
                 CreatedOnUtc =  DateTime.UtcNow
            };
            _manufacturerService.InsertNewsItemManufacturer(newsItemManufacturer);

            return NewsItemManufacturerList(command, model.NewsItemId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemManufacturerUpdate(GridCommand command, NewsItemModel.NewsItemManufacturerModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemManufacturer = _manufacturerService.GetNewsItemManufacturerById(model.Id);
            if (newsItemManufacturer == null)
                throw new ArgumentException("No newsItem manufacturer mapping found with the specified id");

            //use Manufacturer property (not ManufacturerId) because appropriate property is stored in it
            newsItemManufacturer.ManufacturerId = Int32.Parse(model.Manufacturer);
            _manufacturerService.UpdateNewsItemManufacturer(newsItemManufacturer);

            return NewsItemManufacturerList(command, newsItemManufacturer.NewsItemId);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult NewsItemManufacturerDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
                return AccessDeniedView();

            var newsItemManufacturer = _manufacturerService.GetNewsItemManufacturerById(id);
            if (newsItemManufacturer == null)
                throw new ArgumentException("No newsItem manufacturer mapping found with the specified id");

            var newsItemId = newsItemManufacturer.NewsItemId;
            _manufacturerService.DeleteNewsItemManufacturer(newsItemManufacturer);

            return NewsItemManufacturerList(command, newsItemId);
        }

        #endregion
    }
}
