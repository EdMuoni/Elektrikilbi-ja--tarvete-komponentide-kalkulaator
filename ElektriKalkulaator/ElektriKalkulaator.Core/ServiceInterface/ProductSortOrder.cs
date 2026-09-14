namespace ElektriKalkulaator.Core.ServiceInterface
{
    // The ways the product catalogue can be ordered.
    //
    // An enum rather than a loose string ("price_asc") because the compiler can then check every
    // use. A typo in a string would silently fall through to the default ordering and look like
    // "sorting does not work", which is an irritating bug to track down.
    //
    // The values are also what appear in the URL (?sort=PriceLowToHigh), so they are named for a
    // reader rather than for the database.
    public enum ProductSortOrder
    {
        // Default. Groups the catalogue by category, then alphabetically inside each — the most
        // useful order for browsing when you do not yet know what you want.
        CategoryThenName = 0,

        NameAToZ,
        PriceLowToHigh,
        PriceHighToLow,

        // Most stock first. Useful to a tradesperson who needs the part today.
        StockHighToLow
    }
}
