using System;
using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Catalog;
using Nop.Services.Topics;
using Nop.Services.News;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Services.Media;

namespace Nop.Services.Seo
{
    /// <summary>
    /// Represents a sitemap generator
    /// </summary>
    public partial class SitemapGenerator : BaseSitemapGenerator, ISitemapGenerator
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ITopicService _topicService;
        private readonly INewsService _newsService;
        private readonly CommonSettings _commonSettings;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IPictureService _pictureService;

        public SitemapGenerator(ICategoryService categoryService,
            IProductService productService, IManufacturerService manufacturerService,
            ITopicService topicService, CommonSettings commonSettings, IWebHelper webHelper,
            INewsService newsService, IWorkContext workContext, IPictureService pictureService)
        {
            this._categoryService = categoryService;
            this._productService = productService;
            this._manufacturerService = manufacturerService;
            this._topicService = topicService;
            this._commonSettings = commonSettings;
            this._webHelper = webHelper;
            this._newsService = newsService;
            this._workContext = workContext;
            this._pictureService = pictureService;
        }

        /// <summary>
        /// Method that is overridden, that handles creation of child urls.
        /// Use the method WriteUrlLocation() within this method.
        /// </summary>
        protected override void GenerateUrlNodes(bool htmlSeo)
        {
            if (_commonSettings.SitemapIncludeCategories)
            {
                if (!htmlSeo)
                    WriteCategories(0);
                else
                    WriteCategoriesForHtml(0);
            }

            if (_commonSettings.SitemapIncludeManufacturers)
            {
                if (!htmlSeo)
                    WriteManufacturers();
                else
                    WriteManufacturersForHtml();
            }

            if (_commonSettings.SitemapIncludeProducts)
            {
                if (!htmlSeo)
                    WriteProducts();
                else
                    WriteProductsForHtml();
            }

            if (_commonSettings.SitemapIncludeTopics)
            {
                if (!htmlSeo)
                    WriteTopics();
                else
                    WriteTopicsForHtml();
                
            }
            //TODO: add setting

            if (!htmlSeo)
                WriteNews();
            else
                WriteNewsForHtml();


        }

        /// <summary>
        /// Method that is overridden, that handles creation of child urls.
        /// Use the method WriteUrlLocation() within this method.
        /// </summary>
        protected override void GenerateUrlNodesForImageSitemap(bool htmlSeo)
        {
            //if (_commonSettings.SitemapIncludeCategories)
            //{
            //    if (!htmlSeo)
            //        WriteCategories(0);
            //    else
            //        WriteCategoriesForHtml(0);
            //}

            //if (_commonSettings.SitemapIncludeManufacturers)
            //{
            //    if (!htmlSeo)
            //        WriteManufacturers();
            //    else
            //        WriteManufacturersForHtml();
            //}

            if (_commonSettings.SitemapIncludeProducts)
            {
                if (!htmlSeo)
                    WriteProducts();
                else
                    WriteProductsForHtmlImageSitemap();
            }

            //if (_commonSettings.SitemapIncludeTopics)
            //{
            //    if (!htmlSeo)
            //        WriteTopics();
            //    else
            //        WriteTopicsForHtml();

            //}
            ////TODO: add setting

            //if (!htmlSeo)
            //    WriteNews();
            //else
            //    WriteNewsForHtml();


        }

        private void WriteCategories(int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, false);
            foreach (var category in categories)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}c/{1}/{2}", _webHelper.GetStoreLocation(false), category.Id, category.GetSeName());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = category.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);

                WriteCategories(category.Id);
            }
        }

        private void WriteManufacturers()
        {
            var manufacturers = _manufacturerService.GetAllManufacturers(false);
            foreach (var manufacturer in manufacturers)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}{1}", _webHelper.GetStoreLocation(false),  manufacturer.GetSeName());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = manufacturer.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }

        private void WriteProducts()
        {
            var products = _productService.GetAllProducts(false);
            foreach (var product in products)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}p/{1}/{2}", _webHelper.GetStoreLocation(false),product.Id, product.GetSeName());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = product.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }

        private void WriteTopics()
        {
            var topics = _topicService.GetAllTopics().ToList().FindAll(t => t.IncludeInSitemap);
            foreach (var topic in topics)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}t/{1}", _webHelper.GetStoreLocation(false), topic.SystemName.ToLowerInvariant());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = DateTime.UtcNow;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }

        private void WriteNews()
        {
            var news = _newsService.GetAllNews(0, null, null, NewsType.News | NewsType.Interview, 0, int.MaxValue);
            foreach (var item in news)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}news/{1}/{2}", _webHelper.GetStoreLocation(false),item.Id, item.GetSeName().ToLowerInvariant());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = DateTime.UtcNow;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }



        #region this region for HTML

        private void WriteCategoriesForHtml(int parentCategoryId)
        {
            var categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId, false);
            string url = null;
            foreach (var category in categories)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                if (category.SeName == "styles")
                {
                    url = string.Format("{0}{1}/{2}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, "Styles");
                }
                else if (category.Id == 15)
                {
                    url = string.Format("{0}{1}/{2}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, "Styles");
                }
                else if (category.ParentCategoryId == 15)
                {
                    //url = string.Format("{0}{1}/Styles?CategoryId={2}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, category.Id);
                    url = string.Format("{0}{1}/Styles/{2}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, category.Id);
                                
                }
                else
                {
                    url = string.Format("{0}{1}/{3}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, category.Id, category.GetSeName());
                }

                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = category.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);

                WriteCategoriesForHtml(category.Id);
            }
        }
        private void WriteManufacturersForHtml()
        {
            var manufacturers = _manufacturerService.GetAllManufacturers(false);
            foreach (var manufacturer in manufacturers)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}{1}/{2}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, manufacturer.GetSeName());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = manufacturer.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }
        
        private void WriteProductsForHtml()
        {
            var products = _productService.GetAllProducts(false);
            foreach (var product in products)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}{1}/p/{2}/{3}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, product.Id, product.GetSeName());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = product.UpdatedOnUtc;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }

        private void WriteProductsForHtmlImageSitemap()
        {
            var products = _productService.GetAllProducts(false);
            foreach (var product in products)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}{1}/p/{2}/{3}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, product.Id, product.GetSeName());
                //var updateFrequency = UpdateFrequency.Weekly;
                //var updateTime = product.UpdatedOnUtc;
                //WriteUrlLocation(url, updateFrequency, updateTime);
                List<string> imageUrlList = new List<string>();



                //foreach (ProductVariant pv in product.ProductVariants)
                //{ 
                //    IList<Core.Domain.Media.Picture> pictures = new List<Core.Domain.Media.Picture>();
                //    //pictures = product.ProductVariants.FirstOrDefault().GetProductVariantPictures(_pictureService);
                //    pictures = pv.GetProductVariantPictures(_pictureService);
                //    if (pictures != null)
                //    {
                //        //imageUrlList.Add(string.Format("{0}/content/images/thumbs/{1}", _webHelper.GetStoreLocation(false), _pictureService.GetPictureUrl(pictures.FirstOrDefault())));
                //        imageUrlList.Add(_pictureService.GetPictureUrl(pictures.FirstOrDefault()));                        
                //        //pp.Picture.SeoFilename.ToString()
                    
                //        //xmlWriter.WriteElementString("urun_url", null,
                //            //_pictureService.GetPictureUrl(pictures.FirstOrDefault()));
                //    }
                //    else
                //    {
                //        //xmlWriter.WriteElementString("urun_url", "");
                //    }
                //}




                
                    IList<Core.Domain.Media.Picture> pictures = new List<Core.Domain.Media.Picture>();
                    if (product.ProductVariants == null)
                        continue; //throw new Exception(product.Id + " id li ürünün varyantý null");

                    if (product.ProductVariants.FirstOrDefault() == null)
                        continue; //throw new Exception(product.Id + " id li ürünün varyantý null");

                    pictures = product.ProductVariants.FirstOrDefault().GetProductVariantPictures(_pictureService);
                    //pictures = pv.GetProductVariantPictures(_pictureService);
                    if (pictures != null)
                    {
                        //imageUrlList.Add(string.Format("{0}/content/images/thumbs/{1}", _webHelper.GetStoreLocation(false), _pictureService.GetPictureUrl(pictures.FirstOrDefault())));
                        imageUrlList.Add(_pictureService.GetPictureUrl(pictures.FirstOrDefault()));
                        //pp.Picture.SeoFilename.ToString()

                        //xmlWriter.WriteElementString("urun_url", null,
                        //_pictureService.GetPictureUrl(pictures.FirstOrDefault()));
                    }
                    else
                    {
                        //xmlWriter.WriteElementString("urun_url", "");
                    }

                WriteUrlLocationForImageSitemap(url, imageUrlList);

                }
        }
        
        private void WriteTopicsForHtml()
        {
            var topics = _topicService.GetAllTopics().ToList().FindAll(t => t.IncludeInSitemap);
            foreach (var topic in topics)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}{1}/t/{2}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, topic.SystemName.ToLowerInvariant());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = DateTime.UtcNow;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }
        private void WriteNewsForHtml()
        {
            var news = _newsService.GetAllNews(0, null, null, NewsType.News | NewsType.Interview, 0, int.MaxValue);
            foreach (var item in news)
            {
                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                var url = string.Format("{0}{1}/Styles/{2}/{3}", _webHelper.GetStoreLocation(false), _workContext.WorkingLanguage.UniqueSeoCode, item.Id, item.GetSeName().ToLowerInvariant());
                var updateFrequency = UpdateFrequency.Weekly;
                var updateTime = DateTime.UtcNow;
                WriteUrlLocation(url, updateFrequency, updateTime);
            }
        }
       
        #endregion
    }
}
