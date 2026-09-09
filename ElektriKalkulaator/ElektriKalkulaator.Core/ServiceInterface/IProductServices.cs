using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;

namespace ElektriKalkulaator.Core.ServiceInterface
{
    public interface IProductServices
    {
        Task<IEnumerable<Product>> GetAll();
        Task<IEnumerable<Product>> GetByCategory(Guid categoryId);

        // Catalogue search. All arguments are optional:
        //   categoryId = null  -> all categories
        //   searchTerm = null  -> no name/brand text filter
        //   brand      = null  -> all brands
        //   sort               -> defaults to category then name
        // Filtering AND sorting happen in the database rather than in C#, so only the rows that
        // are wanted, already in the right order, travel across the network.
        //
        // searchTerm and brand look similar but are not the same thing. searchTerm is free text
        // and matches a PART of a name or brand, so "ABB" also finds "ABB S201-B32". brand is an
        // EXACT match chosen from a known list, which is what a "show me only Schneider" filter
        // needs -- a partial match there would make one brand's filter include another whose name
        // happens to contain it.
        Task<IEnumerable<Product>> Search(
            Guid? categoryId,
            string? searchTerm,
            string? brand = null,
            ProductSortOrder sort = ProductSortOrder.CategoryThenName);

        // Every brand that actually has a product, alphabetically. Used to build the brand
        // filter, so the list can never offer a brand that would return nothing.
        Task<IEnumerable<string>> GetBrands();
        Task<Product?> GetById(Guid id);
        Task<Product> Create(ProductDto dto);
        Task<Product?> Update(ProductDto dto);
        Task<Product?> Delete(Guid id);
    }
}
