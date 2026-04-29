using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;

namespace ElektriKalkulaator.Core.ServiceInterface
{
    public interface IProductServices
    {
        Task<IEnumerable<Product>> GetAll();
        Task<IEnumerable<Product>> GetByCategory(Guid categoryId);
        Task<Product> GetById(Guid id);
        Task<Product> Create(ProductDto dto);
        Task<Product> Update(ProductDto dto);
        Task<Product> Delete(Guid id);
    }
}
