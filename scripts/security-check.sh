#!/usr/bin/env bash
#
# security-check.sh — re-runs every exploit found in the 2026-08-11 security review and checks
# that it is still blocked.
#
# WHY THIS EXISTS
# The xUnit tests cover logic that can be tested without a web server (image validation, catalogue
# search, service behaviour). They cannot check things that only exist once the whole application
# is running: authentication redirects, antiforgery enforcement, and HTTP status codes. That is
# what this script covers.
#
# HOW TO USE
#   1. Start the app:   dotnet run --project ElektriKalkulaator
#   2. In another shell: bash scripts/security-check.sh
#
# Every check prints PASS or FAIL. The script exits 1 if anything failed, so it can also be used
# in a build pipeline later.
#
# Git Bash note: MSYS_NO_PATHCONV stops Git Bash rewriting arguments that begin with "/" into
# Windows paths, which would corrupt URLs like returnUrl=/Products.
export MSYS_NO_PATHCONV=1

BASE="${1:-http://localhost:8080}"

# The cookie jar must be a RELATIVE filename, not an absolute /tmp path.
# curl on Windows cannot resolve a Unix-style path, and MSYS_NO_PATHCONV above deliberately stops
# Git Bash translating it. With an unwritable jar every request goes out with no session and no
# antiforgery cookie, so real checks fail for the wrong reason.
JAR="./.security-check-cookies.tmp"
trap 'rm -f "$JAR"' EXIT
FAILURES=0

# Two seeded IDs used throughout.
PRODUCT_ID="22222222-0000-0000-0000-000000000002"   # ABB S201-B16
MISSING_ID="99999999-9999-9999-9999-999999999999"   # deliberately does not exist

pass() { printf '  \033[32mPASS\033[0m  %s\n' "$1"; }
fail() { printf '  \033[31mFAIL\033[0m  %s\n' "$1"; FAILURES=$((FAILURES + 1)); }

check() { # check <description> <expected> <actual>
  if [ "$2" = "$3" ]; then pass "$1"; else fail "$1 (expected $2, got $3)"; fi
}

status()   { curl -s -o /dev/null -w '%{http_code}' "$BASE$1"; }
location() { curl -s -o /dev/null -D- "$BASE$1" | grep -i '^location:' | head -1 | tr -d '\r' | sed 's/^[Ll]ocation: *//'; }

# Fetches a valid antiforgery token and stores the matching session cookie in $JAR.
get_token() {
  curl -s -c "$JAR" -b "$JAR" "$BASE${1:-/Products}" \
    | grep -o 'name="__RequestVerificationToken"[^>]*value="[^"]*"' \
    | head -1 | sed -E 's/.*value="([^"]*)".*/\1/'
}

echo
echo "Security check against $BASE"
echo "============================================================"

# ── 1. Is the app even up? ───────────────────────────────────────────────────
if [ "$(status /)" != "200" ]; then
  echo "  The application is not responding at $BASE — start it with 'dotnet run' first."
  exit 1
fi

# ── 2. Admin pages must require a login ──────────────────────────────────────
echo
echo "Authentication — admin pages must not be reachable anonymously"
for path in "/Products/Create" "/Products/Edit/$PRODUCT_ID" "/Products/Delete/$PRODUCT_ID" "/Products/Categories"; do
  code=$(status "$path")
  loc=$(location "$path")
  case "$loc" in
    */Account/Login*) pass "$path redirects to login" ;;
    *)                fail "$path returned $code, location '$loc' (expected redirect to /Account/Login)" ;;
  esac
done

# ── 3. Public pages must stay public ─────────────────────────────────────────
echo
echo "Public pages must remain reachable without an account"
for path in "/" "/Products" "/Calculator" "/Cart" "/Products/Details/$PRODUCT_ID"; do
  check "$path is public" "200" "$(status "$path")"
done

# ── 4. Antiforgery must be enforced ──────────────────────────────────────────
echo
echo "Cross-site request forgery — POSTs without a token must be refused"
check "POST /Cart/Add no token"    "400" "$(curl -s -o /dev/null -w '%{http_code}' -X POST "$BASE/Cart/Add" -d "productId=$PRODUCT_ID&quantity=1")"
check "POST /Cart/Remove no token" "400" "$(curl -s -o /dev/null -w '%{http_code}' -X POST "$BASE/Cart/Remove" -d "productId=$PRODUCT_ID")"
check "POST /Cart/Clear no token"  "400" "$(curl -s -o /dev/null -w '%{http_code}' -X POST "$BASE/Cart/Clear")"
check "POST /Cart/Checkout no token" "400" "$(curl -s -o /dev/null -w '%{http_code}' -X POST "$BASE/Cart/Checkout")"
check "POST /Calculator no token"  "400" "$(curl -s -o /dev/null -w '%{http_code}' -X POST "$BASE/Calculator" -d 'BuildingType=eramu&RoomCount=3&SocketCount=6&LightCount=8')"

# ── 5. Open redirect ─────────────────────────────────────────────────────────
# Tested WITH a valid token, so a refusal here proves the redirect guard works rather than
# the antiforgery check masking it.
echo
echo "Open redirect — returnUrl must only ever point back at this site"
rm -f "$JAR"; TOKEN=$(get_token /Products)
evil=$(curl -s -o /dev/null -D- -b "$JAR" -c "$JAR" -X POST "$BASE/Cart/Add" \
        -d "__RequestVerificationToken=$TOKEN&productId=$PRODUCT_ID&quantity=1&returnUrl=https://evil.example.com/phish" \
        | grep -i '^location:' | tr -d '\r' | sed 's/^[Ll]ocation: *//')
# Note the check is "went to /Cart", not merely "did not go to evil.example.com". An empty or
# missing Location would satisfy the weaker test even when the request failed for an unrelated
# reason — which would be a check that can never fail, and therefore worthless.
case "$evil" in
  *evil.example.com*) fail "external returnUrl was followed → $evil" ;;
  "/Cart")            pass "external returnUrl refused, fell back to /Cart" ;;
  *)                  fail "unexpected response to external returnUrl: '$evil' (expected /Cart)" ;;
esac

local_redirect=$(curl -s -o /dev/null -D- -b "$JAR" -c "$JAR" -X POST "$BASE/Cart/Add" \
        -d "__RequestVerificationToken=$TOKEN&productId=$PRODUCT_ID&quantity=1&returnUrl=/Products" \
        | grep -i '^location:' | tr -d '\r' | sed 's/^[Ll]ocation: *//')
check "legitimate local returnUrl still works" "/Products" "$local_redirect"

# ── 6. Cart input validation ─────────────────────────────────────────────────
echo
echo "Cart input — quantities and product IDs must be validated"
rm -f "$JAR"; TOKEN=$(get_token /Products)
curl -s -o /dev/null -b "$JAR" -c "$JAR" -X POST "$BASE/Cart/Add" \
  -d "__RequestVerificationToken=$TOKEN&productId=$PRODUCT_ID&quantity=-5"
cart=$(curl -s -b "$JAR" -c "$JAR" "$BASE/Cart" | sed -e 's/<[^>]*>/ /g' | tr -s ' \n' ' \n')
if echo "$cart" | grep -qE '\-[0-9]+([.,][0-9]{2})? *€|\-[0-9]+ tk'; then
  fail "negative quantity produced a negative cart total"
else
  pass "negative quantity rejected"
fi

missing=$(curl -s -o /dev/null -w '%{http_code}' -b "$JAR" -c "$JAR" -X POST "$BASE/Cart/Add" \
  -d "__RequestVerificationToken=$TOKEN&productId=$MISSING_ID&quantity=1")
check "nonexistent product refused" "404" "$missing"

# ── Summary ──────────────────────────────────────────────────────────────────
rm -f "$JAR"
echo
echo "============================================================"
if [ "$FAILURES" -eq 0 ]; then
  echo "  All security checks passed."
  exit 0
else
  echo "  $FAILURES check(s) FAILED — see above."
  exit 1
fi
