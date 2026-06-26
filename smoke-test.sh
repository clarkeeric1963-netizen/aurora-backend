#!/usr/bin/env bash
# Quick check that the API is up and tenant scoping works.
# Usage:
#   ./smoke-test.sh                                  # localhost, tenant "sefl"
#   ./smoke-test.sh https://you.up.railway.app sefl  # specify base + tenant slug
set -euo pipefail

BASE="${1:-http://localhost:8080}"
TENANT="${2:-sefl}"
echo "Testing $BASE  (tenant: $TENANT)"
echo

check () {
  local path="$1" ; local hdr="$2"
  printf "  %-26s " "$path"
  if [[ -n "$hdr" ]]; then
    code=$(curl -s -o /dev/null -w "%{http_code}" -H "X-Tenant: $TENANT" "$BASE$path")
  else
    code=$(curl -s -o /dev/null -w "%{http_code}" "$BASE$path")
  fi
  if [[ "$code" =~ ^2 ]]; then echo "OK ($code)"; else echo "FAIL ($code)"; fi
}

echo "Platform routes (no tenant needed):"
check "/health" ""
check "/api/customers" ""

echo
echo "Tenant routes (scoped to '$TENANT'):"
check "/api/orders" "x"
check "/api/drivers" "x"
check "/api/accounts" "x"
check "/api/terminals" "x"
check "/api/users" "x"

echo
echo "Isolation check — orders as 'sefl' vs 'gulfstates':"
echo "  sefl        -> $(curl -s -H 'X-Tenant: sefl' "$BASE/api/orders" | grep -o '"id"' | wc -l | tr -d ' ') order(s)"
echo "  gulfstates  -> $(curl -s -H 'X-Tenant: gulfstates' "$BASE/api/orders" | grep -o '"id"' | wc -l | tr -d ' ') order(s)"
echo "(Each tenant should see only its own — they should differ.)"
