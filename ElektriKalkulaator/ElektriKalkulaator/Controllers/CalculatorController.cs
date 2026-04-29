using ElektriKalkulaator.Core.Dto;
using ElektriKalkulaator.Core.ServiceInterface;
using Microsoft.AspNetCore.Mvc;

namespace ElektriKalkulaator.Controllers
{
    // Handles the calculator form (GET shows the form, POST runs the calculation).
    public class CalculatorController : Controller
    {
        private readonly ICalculatorServices _calculatorServices;

        public CalculatorController(ICalculatorServices calculatorServices)
        {
            _calculatorServices = calculatorServices;
        }

        // GET /Calculator — empty form
        [HttpGet]
        public IActionResult Index()
        {
            return View(new CalculatorInputDto());
        }

        // POST /Calculator — run EVS-HD 60364 calculation, save results, show BOM table
        [HttpPost]
        public async Task<IActionResult> Index(CalculatorInputDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var bom = await _calculatorServices.Calculate(dto);
            await _calculatorServices.SaveCalculation(dto, bom);

            ViewBag.BOM       = bom;
            ViewBag.TotalCost = bom.Sum(b => b.TotalPrice);

            return View(dto);
        }

        // GET /Calculator/History — last 50 calculations
        [HttpGet]
        public async Task<IActionResult> History()
        {
            var history = await _calculatorServices.GetHistory();
            return View(history);
        }
    }
}
