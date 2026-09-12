using Microsoft.Extensions.Configuration;

namespace ElektriKalkulaator
{
    // Produces the short sentence shown next to any total, saying whether prices include VAT.
    //
    // WHY THIS EXISTS
    // The application's whole purpose is producing a cost estimate. An estimate that does not say
    // whether tax is included is ambiguous by more than 20% — the difference between a quote a
    // customer can rely on and one they cannot. Every Estonian retailer states this explicitly;
    // this application did not.
    //
    // The answer lives in configuration rather than being hard-coded, because it is a business
    // decision (are the seeded prices retail prices, or trade prices before tax?) that may change
    // without any code changing. See "Pricing" in appsettings.json.
    public static class VatNotice
    {
        // Configuration keys, named as constants so a typo cannot silently fall back to the
        // default and display the wrong tax statement.
        private const string IncludesVatKey = "Pricing:PricesIncludeVat";
        private const string VatRateKey     = "Pricing:VatRatePercent";

        // Reads the setting and returns the sentence to display.
        // Defaults to "prices include VAT" because that is the normal convention for a
        // consumer-facing Estonian shop, and because quoting a price lower than the customer
        // actually pays is the more damaging way to be wrong.
        public static string Text(IConfiguration configuration)
        {
            var includesVat = configuration.GetValue<bool?>(IncludesVatKey) ?? true;
            var vatRate     = configuration.GetValue<decimal?>(VatRateKey);

            var rateSuffix = vatRate.HasValue ? $" ({vatRate.Value:0.#}%)" : "";

            return includesVat
                ? $"Hinnad sisaldavad käibemaksu{rateSuffix}."
                : $"Hinnad ei sisalda käibemaksu{rateSuffix}.";
        }
    }
}
