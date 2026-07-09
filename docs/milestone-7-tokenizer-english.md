# Milestone 7 — Tokenizer inglese migliorato

## Obiettivo

Migliorare la gestione di contrazioni e possessivi inglesi senza introdurre ancora una tokenizzazione grammaticale complessa.

## Decisione progettuale

In questa fase le contrazioni e i possessivi restano token singoli:

```text
don't
I'm
I'll
won't
couldn't
Alice's
Queen's
```

Non vengono ancora divisi in componenti grammaticali come:

```text
do + n't
I + 'm
Alice + 's
```

Questa scelta mantiene frequenze, n-grammi, next words e KWIC leggibili, evitando una sovraingegnerizzazione prematura.

## Modifiche principali

- Il tokenizer riconosce apostrofi tipografici dentro le parole.
- Il tokenizer normalizza `’` e `‘` in `'`.
- I possessivi inglesi vengono mantenuti come token singoli.
- Le contrazioni inglesi vengono mantenute come token singoli.
- La ricerca KWIC usa la stessa logica base per parole con apostrofi e trattini.
- Il Makefile quota il parametro `WORD`, così parole come `don't` e `alice's` sono più sicure da passare alla CLI.

## Esempi

```text
I’m       → i'm
Alice’s   → alice's
won’t     → won't
well-known → well-known
```

## Comandi utili

```powershell
make stats-word RUN=1 WORD="don't" LIMIT=25
make stats-kwic RUN=1 WORD="don't" LIMIT=10 CONTEXT=8
make stats-word RUN=1 WORD="alice's" LIMIT=25
make stats-kwic RUN=1 WORD="alice's" LIMIT=10 CONTEXT=8
```

## Criteri di accettazione

- `don't` resta una parola.
- `i'm` resta una parola.
- `won't` resta una parola.
- `alice's` resta una parola.
- `Alice’s` e `Alice's` vengono normalizzati nello stesso modo.
- I bigrammi come `alice's cat` e `won't go` sono leggibili.
- KWIC trova parole con apostrofo tipografico usando una query normalizzata.
- I test esistenti continuano a passare.
