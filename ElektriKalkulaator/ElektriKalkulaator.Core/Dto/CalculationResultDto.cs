namespace ElektriKalkulaator.Core.Dto
{
    // The result of one calculation: the Bill of Materials plus plain-language notes about
    // anything the calculator could NOT do.
    //
    // Why the notes exist: when no suitable product was in stock, the calculator used to leave that
    // row out without telling anyone, so the total looked complete when it was not. A calculator
    // whose whole point is showing its reasoning must also show what it left out. The reviewer of
    // the thesis asked exactly this: "Kuidas käitub lahendus puuduva toote või ebapiisava laoseisu
    // korral?"
    public class CalculationResultDto
    {
        public List<BOMItemDto> Items { get; set; } = new();

        // Estonian sentences shown above the result table, e.g.
        // "Valgustus: sobivat 10 A kaitselülitit ei ole laos, seetõttu puudub see rida loendist."
        public List<string> Notes { get; set; } = new();
    }
}
