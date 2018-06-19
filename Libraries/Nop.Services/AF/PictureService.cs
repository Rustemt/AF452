using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using Nop.Services.Configuration;
using Nop.Core.Domain.News;
using Nop.Core.Infrastructure;
using ImageResizer;
using Nop.Services.Logging;
using AlternativeDataAccess;

namespace Nop.Services.Media
{
    /// <summary>
    /// Picture service
    /// </summary>
    public partial class PictureService : IPictureService
    {
        #region Fields
        private readonly IRepository<ProductVariantPicture> _productVariantPictureRepository;
        private readonly IRepository<NewsItemPicture> _newsItemPictureRepository;
		private readonly ICacheManager _cacheManager;
		private readonly ILogger _logger;
        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pictureRepository">Picture repository</param>
        /// <param name="productPictureRepository">Product picture repository</param>
        /// <param name="settingService">Setting service</param>
        /// <param name="webHelper">Web helper</param>
        /// <param name="mediaSettings">Media settings</param>
        public PictureService(IRepository<Picture> pictureRepository,
            IRepository<ProductPicture> productPictureRepository,
            IRepository<ProductVariantPicture> productVariantPictureRepository,
            IRepository<NewsItemPicture> newsItemPictureRepository,
            ISettingService settingService, IWebHelper webHelper, 
            MediaSettings mediaSettings, ILogger logger)
        {
			this._cacheManager = EngineContext.Current.ContainerManager.Resolve<ICacheManager>("nop_cache_static");
            this._pictureRepository = pictureRepository;
            this._productPictureRepository = productPictureRepository;
            this._productVariantPictureRepository = productVariantPictureRepository;
            this._newsItemPictureRepository = newsItemPictureRepository;
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._mediaSettings = mediaSettings;
			this._logger = logger;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets pictures by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Pictures</returns>
        public IList<Picture> GetPicturesByProductVariantId(int productVariantId)
        {
            return GetPicturesByProductVariantId(productVariantId, 0);
        }

        //public IList<Picture> GetPicturesByNewsItemId(int NewsItemId)
        //{
        //    return GetPicturesByNewsItemId(NewsItemId, 0);
        //}

        /// <summary>
        /// Gets pictures by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        /// <returns>Pictures</returns>
        public IList<Picture> GetPicturesByProductVariantId(int productVariantId, int recordsToReturn)
        {
			return new PictureRepository().GetByProductVariantId(productVariantId, recordsToReturn);

			////if (productVariantId == 0)
			////	return new List<Picture>();


			////var pics = _cacheManager.Get("PicturesByProductVariant", 60, () =>
			////{
			////	var query = from p in _pictureRepository.Table
			////				join pvp in _productVariantPictureRepository.Table on p.Id equals pvp.PictureId
			////				orderby pvp.DisplayOrder
			////				select new {p.Id, p.IsNew, p.MimeType, p.NewsItemPictures, p.PictureBinary, p.ProductPictures, p.ProductVariantPictures, p.SeoFilename, pvp.ProductVariantId};
			////	return query.ToList();
			////});

			//var query = from p in _pictureRepository.Table
			//			join pvp in _productVariantPictureRepository.Table on p.Id equals pvp.PictureId
			//			orderby pvp.DisplayOrder
			//			where pvp.ProductVariantId == productVariantId
			//			select p;

			////pics = pics.Where(pvp => pvp.ProductVariantId == productVariantId).ToList();

			////if (recordsToReturn > 0)
			////	pics = pics.Take(recordsToReturn).ToList();

			////return pics.Select(x => new Picture { Id = x.Id, IsNew = x.IsNew, MimeType = x.MimeType, 
			////	NewsItemPictures = x.NewsItemPictures, PictureBinary = x.PictureBinary, ProductPictures = x.ProductPictures,
			////	ProductVariantPictures = x.ProductVariantPictures, SeoFilename = x.SeoFilename}).ToList();
        }

        /// <summary>
        /// Gets pictures by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Number of records to return. 0 if you want to get all items</param>
        /// <returns>Pictures</returns>
        //public IList<Picture> GetPicturesByNewsItemId(int newsItemId, int recordsToReturn)
        //{
        //    if (newsItemId == 0)
        //        return new List<Picture>();


        //    var query = from p in _pictureRepository.Table
        //                join np in _newsItemPictureRepository.Table on p.Id equals np.PictureId
        //                orderby np.DisplayOrder
        //                where np.NewsItemId == newsItemId
        //                select p;

        //    if (recordsToReturn > 0)
        //        query = query.Take(recordsToReturn);

        //    var pics = query.ToList();
        //    return pics;
        //}


		/// <summary>
		/// Get a picture URL
		/// </summary>
		/// <param name="picture">Picture instance</param>
		/// <param name="targetSize">The target picture size (longest side)</param>
		/// <param name="showDefaultPicture">A value indicating whether the default picture is shown</param>
		/// <param name="storeLocation">Store location URL; null to use determine the current store location automatically</param>
		/// <param name="defaultPictureType">Default picture type</param>
		/// <returns>Picture URL</returns>
		public virtual string GetPictureUrl(Picture picture, int targetSize = 0, bool showDefaultPicture = true)
		{
			string url = string.Empty;
			string lastPart = "";
			string thumbFileName;

			if (picture != null && !picture.IsNew)
			{
				lastPart = GetFileExtensionFromMimeType(picture.MimeType);
				string seoFileName = picture.SeoFilename;
				var thumbFilePath = "";
				if (targetSize == 0)
				{
					thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
						string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), seoFileName, lastPart) :
						string.Format("{0}.{1}", picture.Id.ToString("0000000"), lastPart);
					thumbFilePath = Path.Combine(this.LocalThumbImagePath, thumbFileName);
				}
				else
				{
					thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
						string.Format("{0}_{1}_{2}.{3}", picture.Id.ToString("0000000"), seoFileName, targetSize, lastPart) :
						string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart);
					thumbFilePath = Path.Combine(this.LocalThumbImagePath, thumbFileName);
				}
				if (File.Exists(thumbFilePath))
					return _webHelper.GetStoreLocation() + "content/images/thumbs/" + thumbFileName;
			}


			byte[] pictureBinary = null;
			if (picture != null)
				pictureBinary = LoadPictureBinary(picture);

			if (picture == null || pictureBinary == null || pictureBinary.Length == 0)
			{
				if (showDefaultPicture)
					url = GetDefaultPictureUrl(targetSize);
				return url;
			}

			if (picture.IsNew)
			{
				DeletePictureThumbs(picture);

				//we do not validate picture binary here to ensure that no exception ("Parameter is not valid") will be thrown
				picture = UpdatePicture(picture.Id, pictureBinary, picture.MimeType, picture.SeoFilename, false);
			}
			
			lock (s_lock)
			{
				lastPart = GetFileExtensionFromMimeType(picture.MimeType);
				string seoFileName = picture.SeoFilename; // = GetPictureSeName(picture.SeoFilename); //just for sure
				if (targetSize == 0)
				{
					thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
						string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), seoFileName, lastPart) :
						string.Format("{0}.{1}", picture.Id.ToString("0000000"), lastPart);
					var thumbFilePath = Path.Combine(this.LocalThumbImagePath, thumbFileName);
					if (!File.Exists(thumbFilePath))
					{
						File.WriteAllBytes(thumbFilePath, pictureBinary);
					}
				}
				else
				{
					thumbFileName = !String.IsNullOrEmpty(seoFileName) ?
						string.Format("{0}_{1}_{2}.{3}", picture.Id.ToString("0000000"), seoFileName, targetSize, lastPart) :
						string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart);
					var thumbFilePath = Path.Combine(this.LocalThumbImagePath, thumbFileName);
					if (!File.Exists(thumbFilePath))
					{
						using (var stream = new MemoryStream(pictureBinary))
						{
							Bitmap b = null;
							try
							{
								//try-catch to ensure that picture binary is really OK. Otherwise, we can get "Parameter is not valid" exception if binary is corrupted for some reasons
								b = new Bitmap(stream);
							}
							catch (ArgumentException exc)
							{
								_logger.Error(string.Format("Error generating picture thumb. ID={0}", picture.Id), exc);
							}
							if (b == null)
							{
								//bitmap could not be loaded for some reasons
								return url;
							}

							var newSize = CalculateDimensions(b.Size, targetSize);

							var destStream = new MemoryStream();
							ImageBuilder.Current.Build(b, destStream, new ResizeSettings()
							{
								Width = newSize.Width,
								Height = newSize.Height,
								Scale = ScaleMode.Both,
								//BackgroundColor = Color.FromArgb(226, 226, 226),
								Mode = FitMode.Stretch,
								Quality = 90//_mediaSettings.DefaultImageQuality
							});
							var destBinary = destStream.ToArray();
							File.WriteAllBytes(thumbFilePath, destBinary);

							b.Dispose();
						}
					}
				}
			}
			url = _webHelper.GetStoreLocation() + "content/images/thumbs/" + thumbFileName;
			return url;
		}


		//public virtual string GetPictureUrl(Picture picture, int targetSize = 0, bool showDefaultPicture = true)
		//{
		//	string url = string.Empty;
		//	if (picture == null || LoadPictureBinary(picture).Length == 0)
		//	{
		//		if (showDefaultPicture)
		//		{
		//			url = GetDefaultPictureUrl(targetSize);
		//		}
		//		return url;
		//	}

		//	string lastPart = GetFileExtensionFromMimeType(picture.MimeType);
		//	string localFilename;
		//	if (picture.IsNew)
		//	{
		//		DeletePictureThumbs(picture);

		//		picture = UpdatePicture(picture.Id, LoadPictureBinary(picture), picture.MimeType, picture.SeoFilename, false);
		//	}
		//	lock (s_lock)
		//	{
		//		string seoFileName = picture.SeoFilename; // = GetPictureSeName(picture.SeoFilename); //just for sure
		//		if (targetSize == 0)
		//		{
		//			localFilename = !String.IsNullOrEmpty(seoFileName) ?
		//				string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), seoFileName, lastPart) :
		//				string.Format("{0}.{1}", picture.Id.ToString("0000000"), lastPart);

		//			if (!File.Exists(Path.Combine(this.LocalThumbImagePath, localFilename)))
		//			{
		//				if (!System.IO.Directory.Exists(this.LocalThumbImagePath))
		//				{
		//					System.IO.Directory.CreateDirectory(this.LocalThumbImagePath);
		//				}
		//				File.WriteAllBytes(Path.Combine(this.LocalThumbImagePath, localFilename), LoadPictureBinary(picture));
		//			}
		//		}
		//		else
		//		{
		//			localFilename = !String.IsNullOrEmpty(seoFileName) ?
		//				string.Format("{0}_{1}_{2}.{3}", picture.Id.ToString("0000000"), seoFileName, targetSize, lastPart) :
		//				string.Format("{0}_{1}.{2}", picture.Id.ToString("0000000"), targetSize, lastPart);
		//			if (!File.Exists(Path.Combine(this.LocalThumbImagePath, localFilename)))
		//			{
		//				if (!System.IO.Directory.Exists(this.LocalThumbImagePath))
		//				{
		//					System.IO.Directory.CreateDirectory(this.LocalThumbImagePath);
		//				}
		//				using (var stream = new MemoryStream(LoadPictureBinary(picture)))
		//				{
		//					var b = new Bitmap(stream);

		//					var newSize = CalculateDimensions(b.Size, targetSize);

		//					if (newSize.Width < 1)
		//						newSize.Width = 1;
		//					if (newSize.Height < 1)
		//						newSize.Height = 1;

		//					var newBitMap = new Bitmap(newSize.Width, newSize.Height);
		//					var g = Graphics.FromImage(newBitMap);
		//					g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
		//					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
		//					g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
		//					g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
		//					/**/
		//					g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
		//					/**/
		//					var attr = new ImageAttributes();
		//					/**/
		//					attr.SetWrapMode(System.Drawing.Drawing2D.WrapMode.Clamp, Color.White);
		//					/**/
		//					g.DrawImage(b, new Rectangle(0, 0, newSize.Width, newSize.Height), 0, 0, b.Width, b.Height, GraphicsUnit.Pixel, attr);
		//					var ep = new EncoderParameters();
		//					ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, this.ImageQuality);
		//					ImageCodecInfo ici = GetImageCodecInfoFromExtension(lastPart);
		//					if (ici == null)
		//						ici = GetImageCodecInfoFromMimeType("image/jpeg");
		//					newBitMap.Save(Path.Combine(this.LocalThumbImagePath, localFilename), ici, ep);
		//					newBitMap.Dispose();
		//					b.Dispose();
		//				}
		//			}
		//		}
		//	}
		//	url = _webHelper.GetStoreLocation() + "content/images/thumbs/" + localFilename;
		//	return url;
		//}

          //Crop algorithm
                //using (var bitmap = new Bitmap(targetWidth, targetHeight, p_image.PixelFormat))
                //{
                //    using (var graphics = Graphics.FromImage(bitmap))
                //    {
                //        var rect = new Rectangle((newWidth - targetWidth) / 2, (newHeight - targetHeight) / 2, bitmap.Width, bitmap.Height);
                //        graphics.CompositingQuality = CompositingQuality.HighQuality;
                //        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                //        graphics.SmoothingMode = SmoothingMode.HighQuality;
                //        graphics.DrawImage(resizedImage, 0, 0, rect, GraphicsUnit.Pixel);
                //    }

                //    bitmap.Save(p_destinationPath, JpegCodecInfo, highQualityEncoder);
                //}

        
        #endregion

		public static string GetPicturePath(int pictureId, string pictureSeoFilename, int? size)
		{
			var localFilename = "";
			if (size == null)
				localFilename = string.Format("{0}_{1}.jpeg", pictureId.ToString("0000000"), pictureSeoFilename);
			else
				localFilename = string.Format("{0}_{1}_{2}.jpeg", pictureId.ToString("0000000"), pictureSeoFilename, size);
			return "/content/images/thumbs/" + localFilename;
		}
    }
}
