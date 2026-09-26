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

        // POST /Calculator — run EVS-HD 60364 calculation, save results, show BOM table.
        //
        // [ValidateAntiForgeryToken] means the request must carry the hidden token that our own
        // form puts on the page. Without it, another website could silently submit this form on a
        // visitor's behalf and fill our calculation tables with junk rows.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CalculatorInputDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // CalculateWithNotes returns the rows AND a note for everything the calculator could
            // not do: no suitable product in stock, less stock than needed, or a stove circuit for
            // a building type without a stove rule. A calculator whose whole point is showing its
            // reasoning must also show what it did NOT do. (The stove message used to be built
            // here; it moved into the service so that all such notes come from one place, and so
            // that a missing 32 A breaker is no longer reported as "no stove rule".)
            var result = await _calculatorServices.CalculateWithNotes(dto);
            var bom = result.Items;
            await _calculatorServices.SaveCalculation(dto, bom);

            ViewBag.BOM       = bom;
            ViewBag.TotalCost = bom.Sum(b => b.TotalPrice);
            ViewBag.Notes     = result.Notes;

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
