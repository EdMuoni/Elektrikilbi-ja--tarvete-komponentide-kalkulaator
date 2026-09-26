using ElektriKalkulaator.Core.Domain;
using ElektriKalkulaator.Core.Dto;

namespace ElektriKalkulaator.Core.ServiceInterface
{
    public interface ICalculatorServices
    {
        Task<List<BOMItemDto>> Calculate(CalculatorInputDto input);

        // Same calculation, plus notes about missing products, insufficient stock and a stove
        // that could not be added. The result page uses this one; Calculate() returns only the rows.
        Task<CalculationResultDto> CalculateWithNotes(CalculatorInputDto input);
        Task<PowerboxCalculation> SaveCalculation(CalculatorInputDto input, List<BOMItemDto> bom);
        Task<IEnumerable<PowerboxCalculation>> GetHistory();
    }
}
