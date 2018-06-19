namespace Nop.Core.Domain.AFEntities
{
    /// <summary>
    /// Represents the product sorting
    /// </summary>
    public enum ProductSortingEnumAF
    {
        /// <summary>
        /// Position (display order)
        /// </summary>
        Position = 0,
        /// <summary>
        /// Name
        /// </summary>
        Name = 5,
        /// <summary>
        /// Price
        /// </summary>
        PriceAscending = 10,
        /// <summary>
        /// Product creation date
        /// </summary>
        CreatedOn = 15,

        PriceDescending = 11,

    }

    public enum ProductOuterSortingEnumAF
    {
        None,
        Category,
        Manufacturer
    }
}