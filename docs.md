# Minion.CryptoLab

A free WPF teaching tool for cryptography — same philosophy as
Minion.InjectionLab, applied to a different domain. Unlike SQL injection,
there's nothing unsafe about encrypting or hashing your own practice
text, so unlike InjectionLab, **every demo here runs real, working
cryptography** (via .NET's own `System.Security.Cryptography`) on
whatever you type — nothing is simulated or narrated.

## Why this exists

The user explicitly wanted "a full discussion on all aspects along with
the extreme basics... starting out with simple alpha replacement, then
moving up slowly adding new concepts... the simple decoder-ring type of
explanation with examples... lots of examples are necessary... and
definitely why you'd use one over the other." This is that curriculum,
built as a working app.

## Architecture

- **Minion.CryptoLab.Core** — pure data/logic, no WPF references.
  - `Crypto/ClassicalCiphers.cs` — Caesar, ROT13, Atbash, simple
    substitution, Vigenère, repeating-key XOR. Hand-implemented (these
    predate computers), verified against known textbook vectors before
    use — e.g. Vigenère against the standard `ATTACKATDAWN` + `LEMON` →
    `LXFOPVEFRNHR` example, confirmed via a separate PowerShell
    reimplementation before trusting the C#.
  - `Crypto/ModernCrypto.cs` — real AES (ECB/CBC), MD5, SHA-256, PBKDF2,
    HMAC-SHA256, via `System.Security.Cryptography`. No hand-rolled modern
    crypto anywhere.
  - `Crypto/FrequencyAnalyzer.cs` — letter-frequency counter for the
    frequency-analysis demo.
  - `Models/TutorialStep.cs` + three demo-kind models (`CipherDemo` —
    plain text in/out; `FrequencyDemo` — letter-frequency bar chart;
    `BlockPatternDemo` — ECB vs. CBC block comparison). A step carries at
    most one demo kind.
  - `Tutorial/TutorialContent.cs` — 20 steps, decoder-ring basics through
    Diffie-Hellman key exchange. See "The curriculum" below.
- **Minion.CryptoLab.Wpf** — `MainWindow` (a simple welcome screen + menu
  bar), `TutorialWindow`/`TutorialViewModel` (the wizard). Theme files
  (`Colors.*.xaml`/`Brushes.xaml`/`Styles.xaml`), `ThemeService`, and the
  `BoolToVisibilityConverter`/`NotEmptyToVisibilityConverter` converters
  were copied verbatim from Minion.InjectionLab (same look/feel, proven
  patterns) — only namespaces changed. `FractionToWidthConverter` is new,
  for the frequency bar chart.

## The curriculum (TutorialContent.cs, 20 steps)

1. What is a cipher, really? (intro, no demo)
2. The Caesar cipher *(demo: adjustable shift)*
3. ROT13 & Atbash — ciphers with no key at all *(demo)*
4. Simple substitution — 26! keys, (seemingly) unbreakable *(demo)*
5. Frequency analysis — the attack that doesn't care about keyspace size
   *(demo: live letter-frequency bar chart)*
6. The Vigenère cipher — "le chiffre indéchiffrable" *(demo)*
7. Kasiski examination — the key still repeats *(demo: finds and reports
   actual repeated ciphertext trigrams and their distances)*
8. XOR and the one-time pad — provably unbreakable, under three brutal
   conditions *(demo)*
9. The two-time pad attack — break rule #3 once and it's over *(demo:
   proves `C1 ⊕ C2 == P1 ⊕ P2` live, with two real plaintexts)*
10. Block vs. stream ciphers, meet AES (concept, no demo)
11. ECB vs. CBC *(demo: real AES, shows the actual 16-byte ciphertext
    blocks for both modes side by side — repeated plaintext blocks
    visibly repeat under ECB, never under CBC — PLUS a real, computed,
    block-by-block CBC trace in the prose itself: IV XOR P1 -> AES -> C1,
    C1 XOR P2 -> AES -> C2, with a callout on why a zero/reused IV is a
    real vulnerability)*
12. Authenticated encryption (GCM), and why CBC alone isn't enough
    *(demo: real AES-GCM run twice with the same key+nonce — proves
    C1 XOR C2 == P1 XOR P2 live, the identical two-time-pad failure from
    Lesson 9 wearing modern clothes)*
13. Hashing vs. encryption — a completely different tool *(demo: real
    SHA-256/MD5)*
14. Salting *(demo: the exact "two users, same password" worked example
    from the planning discussion — real salts generated per click,
    identical password hashed three ways)*
15. Slow KDFs *(demo: REAL measured timing — raw SHA-256 loop vs. one
    PBKDF2 call, via `Stopwatch`, on the user's own machine, not an
    invented number)*
16. HMAC — proving a message wasn't tampered with *(demo: byte-level
    avalanche effect — flips exactly one bit of the message and shows
    ~50% of the 256 output bits change, with the real count/percentage)*
17. The padding oracle — two earlier half-fixes combining into a real
    exploit *(demo: a REAL working padding-oracle attack — a genuine
    AES-decrypt-and-check-PKCS7-padding "oracle" function, queried by a
    real byte-by-byte recovery loop that recovers an entire 16-byte
    plaintext block from nothing but pass/fail answers, key never used
    by the attack code)*
18. Key exchange — Diffie-Hellman *(demo: real toy modular-exponentiation
    math, small numbers, both sides' computed shared secret shown to
    match)*
19. Randomness — PRNG vs. CSPRNG *(demo: a seeded `Random` reliably
    repeats; `RandomNumberGenerator` never does)*
20. Wrap-up — pointer to the (not yet built) Lab catalog and Quiz Mode

## Verification approach

Every piece of non-trivial math was checked against a second, independent
implementation in PowerShell (which runs on the same .NET runtime, so it's
a genuine independent check, not just "re-read the same code") BEFORE
being written into the C# tutorial content:

- Caesar/ROT13 round-trip verified.
- Vigenère checked against the canonical textbook vector.
- Atbash checked for self-inverse behavior.
- The two-time-pad property (`C1⊕C2 == P1⊕P2`) verified with real XOR
  arithmetic on two different plaintexts.
- AES-ECB confirmed to produce byte-identical ciphertext blocks for
  byte-identical plaintext blocks; AES-CBC confirmed NOT to, on the same
  input.
- The CBC block-by-block trace (IV XOR P1 -> AES -> C1 -> XOR P2 -> AES
  -> C2) was computed independently in PowerShell and confirmed to match
  .NET's own `AesCbcEncrypt` output byte-for-byte before being written
  into the lesson text.
- AES-GCM nonce reuse: confirmed `C1 XOR C2 == P1 XOR P2` for real
  AES-GCM ciphertexts produced with a reused key+nonce, independently in
  PowerShell, before writing the lesson around it.
- The padding-oracle byte-recovery algorithm (including the pos==15
  disambiguation step, needed because a longer real padding can
  coincidentally look like a valid 1-byte padding) was verified against
  a ground-truth value computed via raw single-block AES decryption, and
  against the actual known plaintext, before being shipped as a demo.
- PBKDF2-vs-SHA256 timing ratio measured for real (not estimated) before
  writing the lesson text describing it.
- The toy Diffie-Hellman example's shared-secret match confirmed.
- Seeded-`Random` predictability and CSPRNG unpredictability both
  confirmed live.

A full run of every tutorial step (via a throwaway console harness
referencing Core, deleted after use) caught two real bugs before this was
ever committed:

1. **Frequency-analysis step**: a missing `$` on one half of an
   interpolated string literally printed `{top.Letter}` instead of the
   actual letter.
2. **Kasiski step**: the original default example sentence never actually
   produced a repeated ciphertext trigram, because the gap between the
   two repeated phrases wasn't a multiple of the key length — the demo
   would have silently shown "no repeats found" to every single person
   who opened it with the default values. Replaced with a verified
   example that reliably produces multiple repeated trigrams at a shared
   distance.

## What's NOT built yet (next round)

- **The Lab catalog** — real-world cryptographic mistakes as a graded
  Partial-vs-Best catalog, the same shape as Minion.InjectionLab's attack
  catalog: hardcoded/derived keys, reused nonces with AES-GCM, ECB in
  production, unsalted fast-hashed passwords, predictable tokens from a
  seeded PRNG, an actual padding-oracle exploit chain.
- **Quiz Mode** — once the Lab catalog exists, same reusable-classification
  approach as InjectionLab's quiz rewrite.
- **Help content** (`help/` folder, mkdocs-style nav) — not started.
- **Branding** — no mascot icon/wordmark for this app yet (InjectionLab's
  assets are Minionware/InjectionLab-specific, not reused here). Currently
  just a plain 🔐 emoji on the welcome screen.
- **Git remote** — not yet created; this is still a local-only repo.
