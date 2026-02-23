# Execution (Layer 3)

Deterministic **Python scripts** that do the work.

- **Purpose:** API calls, data processing, file operations, database interactions.
- **Requirements:** Reliable, testable, well-commented. Use `.env` for secrets (API keys, tokens).
- **Used by:** The orchestrator runs these per the directives; it does not perform the work manually.

Check here for existing scripts before writing new ones. When something breaks: fix the script, test, then update the relevant directive with what you learned.
