using System;
using System.IO;
using System.Text;
using System.Xml;
using Nop.Core;
using System.Collections.Generic;

namespace Nop.Services.Seo
{
    /// <summary>
    /// Represents a base sitemap generator
    /// </summary>
    public abstract partial class BaseSitemapGenerator
    {
        #region Fields

        private const string DateFormat = @"yyyy-MM-dd";
        private XmlTextWriter _writer;

        #endregion

        #region Utilities

        /// <summary>
        /// Method that is overridden, that handles creation of child urls.
        /// Use the method WriteUrlLocation() within this method.
        /// </summary>
        protected abstract void GenerateUrlNodes(bool htmlSeo);

        /// <summary>
        /// Method that is overridden, that handles creation of child urls.
        /// Use the method WriteUrlLocation() within this method.
        /// </summary>
        protected abstract void GenerateUrlNodesForImageSitemap(bool htmlSeo);

        /// <summary>
        /// Writes the url location to the writer.
        /// </summary>
        /// <param name="url">Url of indexed location (don't put root url information in).</param>
        /// <param name="updateFrequency">Update frequency - always, hourly, daily, weekly, yearly, never.</param>
        /// <param name="lastUpdated">Date last updated.</param>
        protected void WriteUrlLocation(string url, UpdateFrequency updateFrequency, DateTime lastUpdated)
        {
            _writer.WriteStartElement("url");
            string loc = XmlHelper.XmlEncode(url);
            _writer.WriteElementString("loc", loc);
            _writer.WriteElementString("changefreq", updateFrequency.ToString().ToLowerInvariant());
            _writer.WriteElementString("lastmod", lastUpdated.ToString(DateFormat));
            _writer.WriteEndElement();
        }

        /// <summary>
        /// Writes the url location to the writer.
        /// </summary>
        /// <param name="url">Url of indexed location (don't put root url information in).</param>
        /// <param name="updateFrequency">Update frequency - always, hourly, daily, weekly, yearly, never.</param>
        /// <param name="lastUpdated">Date last updated.</param>
        protected void WriteUrlLocationForImageSitemap(string url, List<string> imageUrlList)
        {
            _writer.WriteStartElement("url");
            string loc = XmlHelper.XmlEncode(url);
            _writer.WriteElementString("loc", loc);
            //_writer.WriteElementString("changefreq", updateFrequency.ToString().ToLowerInvariant());
            //_writer.WriteElementString("lastmod", lastUpdated.ToString(DateFormat));
            foreach (string imgUrl in imageUrlList)
            {
                _writer.WriteStartElement("image");
                string imgUrlloc = XmlHelper.XmlEncode(imgUrl);
                _writer.WriteElementString("loc", imgUrlloc);
                _writer.WriteEndElement();
            }
            _writer.WriteEndElement();
        }

        #endregion

        #region Methods

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <returns>Sitemap.xml as string</returns>
        public string Generate(bool htmlSeo)
        {
            using (var stream = new MemoryStream())
            {
                if (!htmlSeo)
                {
                    Generate(stream,false);
                }
                else
                {
                    Generate(stream,true);
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// This will build an xml image sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <returns>Sitemap.xml as string</returns>
        public string GenerateImageSitemap(bool htmlSeo)
        {
            using (var stream = new MemoryStream())
            {
                if (!htmlSeo)
                {
                    GenerateImageSitemap(stream, false);
                }
                else
                {
                    GenerateImageSitemap(stream, true);
                }
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="stream">Stream of sitemap.</param>
        public void Generate(Stream stream, bool htmlSeo)
        {
            _writer = new XmlTextWriter(stream, Encoding.UTF8);
            _writer.Formatting = Formatting.Indented;
            _writer.WriteStartDocument();
            _writer.WriteStartElement("urlset");
            _writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
            _writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            _writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

            if (!htmlSeo)
            {
                GenerateUrlNodes(false);
            }
            else
            {
                GenerateUrlNodes(true);
            }
            _writer.WriteEndElement();
            _writer.Close();
        }


        /// <summary>
        /// This will build an xml sitemap for better index with search engines.
        /// See http://en.wikipedia.org/wiki/Sitemaps for more information.
        /// </summary>
        /// <param name="stream">Stream of sitemap.</param>
        public void GenerateImageSitemap(Stream stream, bool htmlSeo)
        {
            _writer = new XmlTextWriter(stream, Encoding.UTF8);
            _writer.Formatting = Formatting.Indented;
            _writer.WriteStartDocument();
            _writer.WriteStartElement("urlset");
            _writer.WriteAttributeString("xmlns", "http://www.sitemaps.org/schemas/sitemap/0.9");
            _writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            _writer.WriteAttributeString("xsi:schemaLocation", "http://www.sitemaps.org/schemas/sitemap/0.9 http://www.sitemaps.org/schemas/sitemap/0.9/sitemap.xsd");

            if (!htmlSeo)
            {
                GenerateUrlNodesForImageSitemap(false);
            }
            else
            {
                GenerateUrlNodesForImageSitemap(true);
            }
            _writer.WriteEndElement();
            _writer.Close();
        }

        #endregion

    }
}
