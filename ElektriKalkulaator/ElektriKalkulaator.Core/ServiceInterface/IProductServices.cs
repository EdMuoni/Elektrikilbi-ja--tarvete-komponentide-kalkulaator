using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;

namespace ElektriKalkulaator.Core.ServiceInterface
{
    public interface IProductServices
    {
        Task<IEnumerable<Product>> GetAll();
        Task<IEnumerable<Product>> GetByCategory(Guid categoryId);

        // Catalogue search. Both arguments are optional:
        //   categoryId = null  -> all categories
        //   searchTerm = null  -> no name/brand filter
        // Filtering happens in the database rather than in C#, so only matching rows travel
        // across the network.
        Task<IEnumerable<Product>> Search(Guid? categoryId, string? searchTerm);
        Task<Product?> GetById(Guid id);
        Task<Product> Create(ProductDto dto);
        Task<Product?> Update(ProductDto dto);
        Task<Product?> Delete(Guid id);
    }
}
