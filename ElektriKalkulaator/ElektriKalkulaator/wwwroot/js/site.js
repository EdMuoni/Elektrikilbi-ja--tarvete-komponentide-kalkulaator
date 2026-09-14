/* =============================================================================
   site.js — small behaviours shared by every page
   =============================================================================
   Anything here must be OPTIONAL. The site is server-rendered Razor and has to
   keep working with JavaScript disabled, so nothing in this file may be the only
   way to complete a task — it only makes existing things pleasanter.
   ============================================================================= */
(function () {
    'use strict';

    /* ── NUMBER INPUTS THAT START AT ZERO ─────────────────────────────────────
       The calculator's number fields are bound to int properties, so Razor
       renders them with the value "0" rather than empty. Typing "2" into a field
       showing "0" gives "02", because the caret lands after the existing zero —
       which is exactly what a person does NOT expect, and it is easy to submit
       without noticing.

       Clearing the zero on focus fixes that. The value is put back on blur if
       the field was left empty, so the form still posts a valid number and the
       server never has to deal with "".

       Only "0" is cleared, never a real value: someone correcting 12 to 13 must
       still be able to click into the field and edit it normally. */
    function clearLeadingZeroOnFocus(input) {
        input.addEventListener('focus', function () {
            if (input.value === '0') {
                input.value = '';
            }
        });

        input.addEventListener('blur', function () {
            if (input.value.trim() === '') {
                /* Restore the field's own minimum rather than a hard-coded 0 —
                   RoomCount has min="1", so blanking it must not leave a value
                   the form will reject. */
                input.value = input.getAttribute('min') || '0';
            }
        });
    }

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('input[type="number"]').forEach(clearLeadingZeroOnFocus);
    });
})();
