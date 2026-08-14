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
        //   searchTerm = null  -> no name/brand filter
        //   sort               -> defaults to category then name
        // Filtering AND sorting happen in the database rather than in C#, so only the rows that
        // are wanted, already in the right order, travel across the network.
        Task<IEnumerable<Product>> Search(
            Guid? categoryId,
            string? searchTerm,
            ProductSortOrder sort = ProductSortOrder.CategoryThenName);
        Task<Product?> GetById(Guid id);
        Task<Product> Create(ProductDto dto);
        Task<Product?> Update(ProductDto dto);
        Task<Product?> Delete(Guid id);
    }
}
