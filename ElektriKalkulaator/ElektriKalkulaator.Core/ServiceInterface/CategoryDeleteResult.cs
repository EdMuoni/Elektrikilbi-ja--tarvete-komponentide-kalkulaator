namespace ElektriKalkulaator.Core.ServiceInterface
{
    // Describes what happened when someone tried to delete a product category.
    //
    // An enum is used instead of throwing exceptions because none of these outcomes is a program
    // error — they are all normal things a user can cause by clicking "delete". The controller
    // reads this value and shows the matching message.
    public enum CategoryDeleteResult
    {
        // The category existed, had no products, and was removed.
        Deleted,

        // No category with that ID exists — most likely already deleted by someone else.
        NotFound,

        // The category still contains products. Deleting it would orphan them, so nothing
        // was removed; the products must be moved or deleted first.
        StillHasProducts
    }
}
