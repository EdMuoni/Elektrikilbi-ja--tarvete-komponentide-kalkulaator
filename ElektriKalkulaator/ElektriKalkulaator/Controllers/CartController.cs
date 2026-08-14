using ElektriKalkulaator.Core.ServiceInterface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ElektriKalkulaator.Controllers
{
    // Session-based cart — no authentication needed, persists for 30 min.
    // The cart is stored as a JSON-serialised Dictionary<Guid, int> in the session.
    public class CartController : Controller
    {
        private readonly IProductServices _productServices;
        private const string CartSessionKey = "ShoppingCart";

        // Sensible limits for how many of one product may sit in the cart.
        // Without a lower bound a negative quantity produced a negative line total, which would
        // reduce the amount owed once real payment exists. Without an upper bound someone could
        // order billions of units of a product we hold thirty of.
        private const int MinQuantity = 1;
        private const int MaxQuantity = 999;

        public CartController(IProductServices productServices)
        {
            _productServices = productServices;
        }

        // GET /Cart — loads cart from session and fetches current product data for each item
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cartItems = GetCartFromSession();
            var cartDetails = new List<CartItemViewModel>();

            foreach (var item in cartItems)
            {
                var product = await _productServices.GetById(item.Key);
                if (product != null)
                {
                    cartDetails.Add(new CartItemViewModel
                    {
                        ProductId   = product.Id,
                        ProductName = product.Name,
                        Brand       = product.Brand,
                        UnitPrice   = product.Price,
                        Quantity    = item.Value,
                        TotalPrice  = product.Price * item.Value
                    });
                }
            }

            ViewBag.TotalCost = cartDetails.Sum(c => c.TotalPrice);
            return View(cartDetails);
        }

        // POST /Cart/Add — increments quantity if already in cart, otherwise adds new entry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid productId, int quantity = 1, string? returnUrl = null)
        {
            // Check 1 — the quantity has to be a sensible positive number.
            // Anything sent to a web server can be edited by the person sending it, so we never
            // trust that the value came from our own form.
            if (quantity < MinQuantity || quantity > MaxQuantity)
            {
                TempData["Error"] = $"Kogus peab olema vahemikus {MinQuantity}–{MaxQuantity}.";
                return SafeRedirect(returnUrl);
            }

            // Check 2 — the product has to actually exist. Previously any random ID was accepted
            // into the session and then silently skipped when the cart was displayed, which left
            // the user with no idea why the item never appeared.
            var product = await _productServices.GetById(productId);
            if (product == null)
                return NotFound();

            var cart = GetCartFromSession();

            // Adding the same product again increases the existing line rather than replacing it.
            var alreadyInCart = cart.TryGetValue(productId, out var existingQuantity)
                ? existingQuantity
                : 0;

            // Math.Min keeps repeated adds from creeping past the maximum.
            cart[productId] = Math.Min(alreadyInCart + quantity, MaxQuantity);

            SaveCartToSession(cart);

            TempData["Success"] = "Toode lisati ostukorvi!";

            return SafeRedirect(returnUrl);
        }

        // POST /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(Guid productId)
        {
            var cart = GetCartFromSession();
            cart.Remove(productId);
            SaveCartToSession(cart);

            TempData["Success"] = "Toode eemaldati ostukorvist.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["Success"] = "Ostukorv tühjendati.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Checkout — demo only, clears cart and shows confirmation.
        // NOTE: this deliberately does not create an order or reduce stock — see
        // docs/PROJECT_ROADMAP.md, where real order persistence is planned work.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return View("OrderConfirmed");
        }

        // Redirects only to addresses on this site.
        //
        // Redirect(returnUrl) used to be called with whatever the request contained, so a crafted
        // link starting on our domain could bounce a visitor to an attacker's page — an "open
        // redirect", and a common way to make a fake login page look trustworthy.
        // Url.IsLocalUrl is the framework's own check for "does this point back at us?".
        private IActionResult SafeRedirect(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        // Reads the cart JSON from session; returns empty dict if nothing stored yet.
        private Dictionary<Guid, int> GetCartFromSession()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(json))
                return new Dictionary<Guid, int>();

            return JsonConvert.DeserializeObject<Dictionary<Guid, int>>(json)
                   ?? new Dictionary<Guid, int>();
        }

        private void SaveCartToSession(Dictionary<Guid, int> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }
    }

    // View model for one cart row — combines product info with quantity and line total.
    public class CartItemViewModel
    {
        public Guid ProductId    { get; set; }
        public string ProductName { get; set; } = "";
        public string Brand      { get; set; } = "";
        public decimal UnitPrice { get; set; }
        public int Quantity      { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
