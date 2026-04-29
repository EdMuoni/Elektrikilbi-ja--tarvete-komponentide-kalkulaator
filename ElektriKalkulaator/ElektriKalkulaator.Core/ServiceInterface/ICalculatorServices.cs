using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;

namespace ElektriKalkulaator.Core.ServiceInterface
{
    public interface ICalculatorServices
    {
        Task<List<BOMItemDto>> Calculate(CalculatorInputDto input);
        Task<PowerboxCalculation> SaveCalculation(CalculatorInputDto input, List<BOMItemDto> bom);
        Task<IEnumerable<PowerboxCalculation>> GetHistory();
    }
}
