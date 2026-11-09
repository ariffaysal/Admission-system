# Assumptions

- Admin authentication is omitted because the supplied requirements do not define it. The dashboard exposes class creation directly for the assignment demo.
- A valid student identity is a 5–50 digit birth registration number sent in `X-Student-Id`.
- Admission fee payment is simulated as successful when the user selects “Pay admission fee”.
- A student may have one active (HELD or CONFIRMED) application per class/shift and may apply to other classes.
- Holds default to 600 seconds and are configurable with `HOLD_DURATION_SECONDS`.
- Expired holds release seats through a periodic worker and when the class capacity is next checked.
- PostgreSQL persists records in a named Docker volume. `docker compose down -v` removes that stored data.
