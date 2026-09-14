using System.ComponentModel.DataAnnotations;
using ElektriKalkulaator.Core.Dto;

namespace ElektriKalkulaator.Tests
{
    // Tests for the validation attributes on the form DTOs.
    //
    // These attributes are the app's first line of defence — ASP.NET checks them before any of our
    // controller code runs. They are also easy to break by accident: deleting one line silently
    // removes a rule and nothing fails to compile.
    //
    // No database or web server needed; Validator does exactly what ASP.NET does internally.
    public class FormValidationTests
    {
        // Runs the same validation ASP.NET performs during model binding and returns the messages.
        private static List<string> Validate(object model)
        {
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true);
            return results.Select(r => r.ErrorMessage ?? "").ToList();
        }

        private static bool IsValid(object model) => Validate(model).Count == 0;

        // ── Calculator input ────────────────────────────────────────────────────

        private static CalculatorInputDto ValidCalculatorInput() => new()
        {
            BuildingType = "korterelamu",
            RoomCount = 3,
            SocketCount = 6,
            LightCount = 8
        };

        [Fact]
        public void CalculatorInput_IsValid_WhenEverythingIsInRange()
        {
            Assert.True(IsValid(ValidCalculatorInput()));
        }

        [Fact]
        public void CalculatorInput_RequiresABuildingType()
        {
            var dto = ValidCalculatorInput();
            dto.BuildingType = "";

            Assert.Contains(Validate(dto), m => m.Contains("building type"));
        }

        [Theory]
        [InlineData(0)]    // below the minimum — a building with no rooms makes no sense
        [InlineData(-1)]
        [InlineData(201)]  // above the maximum
        public void CalculatorInput_RejectsRoomCountsOutsideOneToTwoHundred(int rooms)
        {
            var dto = ValidCalculatorInput();
            dto.RoomCount = rooms;

            Assert.False(IsValid(dto));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(200)]
        public void CalculatorInput_AcceptsRoomCountsAtTheBoundaries(int rooms)
        {
            var dto = ValidCalculatorInput();
            dto.RoomCount = rooms;

            Assert.True(IsValid(dto));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(501)]
        public void CalculatorInput_RejectsSocketCountsOutsideZeroToFiveHundred(int sockets)
        {
            var dto = ValidCalculatorInput();
            dto.SocketCount = sockets;

            Assert.False(IsValid(dto));
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(501)]
        public void CalculatorInput_RejectsLightCountsOutsideZeroToFiveHundred(int lights)
        {
            var dto = ValidCalculatorInput();
            dto.LightCount = lights;

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void CalculatorInput_AllowsZeroSocketsAndLights()
        {
            // A building with no sockets or lights is odd but not invalid — the calculator still
            // returns an enclosure and an RCD.
            var dto = ValidCalculatorInput();
            dto.SocketCount = 0;
            dto.LightCount = 0;

            Assert.True(IsValid(dto));
        }

        // ── Product form ────────────────────────────────────────────────────────

        private static ProductDto ValidProduct() => new()
        {
            CategoryId = Guid.NewGuid(),
            Name = "Test toode",
            Brand = "TestBrand",
            Price = 9.99m
        };

        [Fact]
        public void Product_IsValid_WithNameBrandCategoryAndPrice()
        {
            Assert.True(IsValid(ValidProduct()));
        }

        [Fact]
        public void Product_RequiresAName()
        {
            var dto = ValidProduct();
            dto.Name = "";

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Product_RequiresABrand()
        {
            var dto = ValidProduct();
            dto.Brand = "";

            Assert.False(IsValid(dto));
        }

        [Theory]
        [InlineData(0)]        // free products would make the BOM total meaningless
        [InlineData(-5)]
        [InlineData(100000)]   // above the allowed maximum
        public void Product_RejectsPricesOutsideTheAllowedRange(decimal price)
        {
            var dto = ValidProduct();
            dto.Price = price;

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Product_AcceptsTheSmallestAllowedPrice()
        {
            var dto = ValidProduct();
            dto.Price = 0.01m;

            Assert.True(IsValid(dto));
        }

        // ── Account forms ───────────────────────────────────────────────────────

        [Fact]
        public void Login_RequiresEmailAndPassword()
        {
            Assert.False(IsValid(new LoginDto()));
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("missing@")]
        [InlineData("@nouser.ee")]
        public void Login_RejectsMalformedEmailAddresses(string email)
        {
            var dto = new LoginDto { Email = email, Password = "Parool123" };

            Assert.False(IsValid(dto));
        }

        [Fact]
        public void Login_AcceptsAWellFormedAddress()
        {
            Assert.True(IsValid(new LoginDto { Email = "klient@naide.ee", Password = "Parool123" }));
        }

        private static RegisterDto ValidRegistration() => new()
        {
            Email = "uus@naide.ee",
            Password = "Parool123",
            ConfirmPassword = "Parool123"
        };

        [Fact]
        public void Register_IsValid_WhenPasswordsMatchAndAreLongEnough()
        {
            Assert.True(IsValid(ValidRegistration()));
        }

        [Fact]
        public void Register_RejectsPasswordsShorterThanEightCharacters()
        {
            var dto = ValidRegistration();
            dto.Password = "Lyhike1";       // 7 characters
            dto.ConfirmPassword = "Lyhike1";

            Assert.Contains(Validate(dto), m => m.Contains("8 tähemärki"));
        }

        [Fact]
        public void Register_RejectsMismatchedPasswordConfirmation()
        {
            // Catches a typo in a field the user cannot read back, which would otherwise lock them
            // out of the account they just created.
            var dto = ValidRegistration();
            dto.ConfirmPassword = "MidagiMuud1";

            Assert.Contains(Validate(dto), m => m.Contains("ei kattu"));
        }

        [Fact]
        public void Register_AllowsAnOptionalName()
        {
            var dto = ValidRegistration();
            dto.FullName = null;

            Assert.True(IsValid(dto));
        }
    }
}
