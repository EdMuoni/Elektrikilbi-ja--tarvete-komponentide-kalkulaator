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
        public IActionResult Add(Guid productId, int quantity = 1, string? returnUrl = null)
        {
            var cart = GetCartFromSession();

            if (cart.ContainsKey(productId))
                cart[productId] += quantity;
            else
                cart[productId] = quantity;

            SaveCartToSession(cart);

            TempData["Success"] = "Item added to cart!";

            return string.IsNullOrEmpty(returnUrl)
                ? RedirectToAction(nameof(Index))
                : Redirect(returnUrl);
        }

        // POST /Cart/Remove
        [HttpPost]
        public IActionResult Remove(Guid productId)
        {
            var cart = GetCartFromSession();
            cart.Remove(productId);
            SaveCartToSession(cart);

            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Clear
        [HttpPost]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["Success"] = "Cart cleared.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Checkout — demo only, clears cart and shows confirmation
        [HttpPost]
        public IActionResult Checkout()
        {
            HttpContext.Session.Remove(CartSessionKey);
            return View("OrderConfirmed");
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
