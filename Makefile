# CorpusLens common commands
# Usage examples:
#   make check
#   make corpus-create CORPUS="English Literature" LANG=en
#   make analyze-book BOOK=./books/alice.epub OUT=./artifacts/alice CORPUS="English Literature"
#   make analyze-books BOOKS=./books/en OUT=./artifacts/en CORPUS="English Literature" LANG=en
#   make analyze-en
#   make analyze-it
#   make stats-profile RUN=1 LIMIT=10 PHRASE_LIMIT=10
#   make stats-token-index RUN=1
#   make stats-words RUN=1 LIMIT=25
#   make stats-content RUN=1 LIMIT=25
#   make stats-function RUN=1 LIMIT=25
#   make stats-word RUN=1 WORD=alice LIMIT=25
#   make stats-word-books RUN=1 WORD=alice LIMIT=25
#   make stats-compare-word RUN_A=1 RUN_B=2 WORD=love
#   make stats-compare-words RUN_A=1 RUN_B=2 LIMIT=25 MIN_COUNT=5 SHARED_ONLY=--shared-only
#   make stats-difficulty RUN=1
#   make stats-compare-difficulty RUN_A=1 RUN_B=2
#   make stats-language-profiles
#   make stats-language-profile LANG=it
#   make stats-collocations RUN=1 WORD=alice WINDOW=4 LIMIT=25 MIN_COUNT=1 MIN_DICE=0.0
#   make stats-collocations-content RUN=1 WORD=whale WINDOW=4 LIMIT=25 MIN_COUNT=3
#   make stats-collocations-function RUN=1 WORD=love WINDOW=4 LIMIT=25 MIN_COUNT=3
#   make stats-phrases RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 MIN_CHAPTERS=1 LIMIT=25
#   make stats-phrases-content-boundary RUN=1 MIN_N=2 MAX_N=5 MIN_COUNT=3 MIN_CHAPTERS=1 LONGEST_ONLY=--longest-only LIMIT=25
#   make stats-kwic RUN=1 WORD=alice LIMIT=10 CONTEXT=8
#   make inspect-run RUN=1

DOTNET ?= dotnet
PROJECT ?= src/CorpusLens.Cli
DB ?= ./data/corpuslens.db
CORPUS ?= English Literature
LANG ?= en
BOOKS_ROOT ?= ./books
BOOKS_EN ?= $(BOOKS_ROOT)/en
BOOKS_IT ?= $(BOOKS_ROOT)/it
OUT_EN ?= ./artifacts/en
OUT_IT ?= ./artifacts/it
CORPUS_EN ?= English Literature
CORPUS_IT ?= Italian Literature
BOOK ?= $(BOOKS_EN)/alice.epub
BOOKS ?= $(BOOKS_EN)
OUT ?= ./artifacts/analysis
RUN ?= 1
RUN_A ?= 1
RUN_B ?= 2
LIMIT ?= 25
WORD ?= alice
N ?= 3
CONTEXT ?= 8
WINDOW ?= 4
MIN_COUNT ?= 1
MIN_DICE ?= 0.0
LONG_WORD_LENGTH ?=
VERY_LONG_WORD_LENGTH ?=
SHARED_ONLY ?=
EXCLUSIVE_ONLY ?=
MIN_CHAPTERS ?= 1
LONGEST_ONLY ?=
MIN_N ?= 2
MAX_N ?= 5
DIAGNOSTICS_OUT ?= ./artifacts/diagnostics/import_diagnostics.md
PHRASE_LIMIT ?= 10
MIN_PHRASE_COUNT ?= 3
MIN_PHRASE_CHAPTERS ?= 2
DIFFICULTY_LENGTH_ARGS = $(if $(LONG_WORD_LENGTH),--long-word-length $(LONG_WORD_LENGTH),) $(if $(VERY_LONG_WORD_LENGTH),--very-long-word-length $(VERY_LONG_WORD_LENGTH),)

.PHONY: restore build test check demo clean clean-data clean-artifacts setup-books corpus-create corpus-create-en corpus-create-it corpus-list analyze-text analyze-book analyze-books analyze-books-recursive analyze-en analyze-it analyze-en-recursive analyze-it-recursive stats-runs stats-summary stats-profile stats-books stats-token-index stats-words stats-content stats-function stats-word stats-word-books stats-compare-word stats-compare-words stats-compare-words-content stats-compare-words-function stats-compare-words-shared stats-compare-words-exclusive stats-difficulty stats-compare-difficulty stats-language-profiles stats-language-profile stats-collocations stats-collocations-content stats-collocations-function stats-phrases stats-phrases-content-boundary stats-kwic stats-ngrams stats-trigrams stats-next stats-categories inspect-run

restore:
	$(DOTNET) restore

build:
	$(DOTNET) build

test:
	$(DOTNET) test

check: clean-data clean-artifacts restore build test

demo:
	$(DOTNET) run --project $(PROJECT) -- demo --out ./artifacts/demo

clean: clean-data clean-artifacts

clean-data:
	powershell -NoProfile -ExecutionPolicy Bypass -Command "if (Test-Path './data') { Remove-Item -LiteralPath './data' -Recurse -Force }"

clean-artifacts:
	powershell -NoProfile -ExecutionPolicy Bypass -Command "if (Test-Path './artifacts') { Remove-Item -LiteralPath './artifacts' -Recurse -Force }"

setup-books:
	powershell -NoProfile -ExecutionPolicy Bypass -Command "New-Item -ItemType Directory -Force -Path './books/en', './books/it' | Out-Null"

corpus-create:
	$(DOTNET) run --project $(PROJECT) -- corpus create "$(CORPUS)" --language $(LANG) --db $(DB)

corpus-create-en:
	$(DOTNET) run --project $(PROJECT) -- corpus create "$(CORPUS_EN)" --language en --db $(DB)

corpus-create-it:
	$(DOTNET) run --project $(PROJECT) -- corpus create "$(CORPUS_IT)" --language it --db $(DB)

corpus-list:
	$(DOTNET) run --project $(PROJECT) -- corpus list --db $(DB)

analyze-text:
	$(DOTNET) run --project $(PROJECT) -- analyze-text ./samples/texts/sample_english_short.txt --language $(LANG) --title "Sample English" --out ./artifacts/sample-text

analyze-book:
	$(DOTNET) run --project $(PROJECT) -- analyze-epub $(BOOK) --language $(LANG) --corpus "$(CORPUS)" --db $(DB) --out $(OUT)

analyze-books:
	$(DOTNET) run --project $(PROJECT) -- analyze-epub-folder $(BOOKS) --language $(LANG) --corpus "$(CORPUS)" --db $(DB) --out $(OUT)

analyze-books-recursive:
	$(DOTNET) run --project $(PROJECT) -- analyze-epub-folder $(BOOKS) --language $(LANG) --corpus "$(CORPUS)" --db $(DB) --out $(OUT) --recursive

analyze-en:
	$(DOTNET) run --project $(PROJECT) -- analyze-epub-folder $(BOOKS_EN) --language en --corpus "$(CORPUS_EN)" --db $(DB) --out $(OUT_EN)

analyze-it:
	$(DOTNET) run --project $(PROJECT) -- analyze-epub-folder $(BOOKS_IT) --language it --corpus "$(CORPUS_IT)" --db $(DB) --out $(OUT_IT)

analyze-en-recursive:
	$(DOTNET) run --project $(PROJECT) -- analyze-epub-folder $(BOOKS_EN) --language en --corpus "$(CORPUS_EN)" --db $(DB) --out $(OUT_EN) --recursive

analyze-it-recursive:
	$(DOTNET) run --project $(PROJECT) -- analyze-epub-folder $(BOOKS_IT) --language it --corpus "$(CORPUS_IT)" --db $(DB) --out $(OUT_IT) --recursive


stats-runs:
	$(DOTNET) run --project $(PROJECT) -- stats runs --limit $(LIMIT) --db $(DB)

stats-summary:
	$(DOTNET) run --project $(PROJECT) -- stats summary $(RUN) --db $(DB)

stats-profile:
	$(DOTNET) run --project $(PROJECT) -- stats profile $(RUN) --limit $(LIMIT) --phrase-limit $(PHRASE_LIMIT) --min-phrase-count $(MIN_PHRASE_COUNT) --min-phrase-chapters $(MIN_PHRASE_CHAPTERS) --db $(DB)

stats-books:
	$(DOTNET) run --project $(PROJECT) -- stats books $(RUN) --db $(DB)

stats-token-index:
	$(DOTNET) run --project $(PROJECT) -- stats token-index $(RUN) --db $(DB)

stats-words:
	$(DOTNET) run --project $(PROJECT) -- stats words $(RUN) --limit $(LIMIT) --db $(DB)

stats-content:
	$(DOTNET) run --project $(PROJECT) -- stats words $(RUN) --content-only --limit $(LIMIT) --db $(DB)

stats-function:
	$(DOTNET) run --project $(PROJECT) -- stats words $(RUN) --function-only --limit $(LIMIT) --db $(DB)

stats-word:
	$(DOTNET) run --project $(PROJECT) -- stats word $(RUN) "$(WORD)" --limit $(LIMIT) --db $(DB)

stats-word-books:
	$(DOTNET) run --project $(PROJECT) -- stats word-books $(RUN) "$(WORD)" --limit $(LIMIT) --db $(DB)

stats-compare-word:
	$(DOTNET) run --project $(PROJECT) -- stats compare-word $(RUN_A) $(RUN_B) "$(WORD)" --db $(DB)

stats-compare-words:
	$(DOTNET) run --project $(PROJECT) -- stats compare-words $(RUN_A) $(RUN_B) --limit $(LIMIT) --min-count $(MIN_COUNT) $(SHARED_ONLY) $(EXCLUSIVE_ONLY) --db $(DB)

stats-compare-words-content:
	$(DOTNET) run --project $(PROJECT) -- stats compare-words $(RUN_A) $(RUN_B) --content-only --limit $(LIMIT) --min-count $(MIN_COUNT) $(SHARED_ONLY) $(EXCLUSIVE_ONLY) --db $(DB)

stats-compare-words-function:
	$(DOTNET) run --project $(PROJECT) -- stats compare-words $(RUN_A) $(RUN_B) --function-only --limit $(LIMIT) --min-count $(MIN_COUNT) $(SHARED_ONLY) $(EXCLUSIVE_ONLY) --db $(DB)

stats-compare-words-shared:
	$(DOTNET) run --project $(PROJECT) -- stats compare-words $(RUN_A) $(RUN_B) --shared-only --limit $(LIMIT) --min-count $(MIN_COUNT) --db $(DB)

stats-compare-words-exclusive:
	$(DOTNET) run --project $(PROJECT) -- stats compare-words $(RUN_A) $(RUN_B) --exclusive-only --limit $(LIMIT) --min-count $(MIN_COUNT) --db $(DB)


stats-difficulty:
	$(DOTNET) run --project $(PROJECT) -- stats difficulty $(RUN) $(DIFFICULTY_LENGTH_ARGS) --db $(DB)

stats-compare-difficulty:
	$(DOTNET) run --project $(PROJECT) -- stats compare-difficulty $(RUN_A) $(RUN_B) $(DIFFICULTY_LENGTH_ARGS) --db $(DB)

stats-language-profiles:
	$(DOTNET) run --project $(PROJECT) -- stats language-profiles

stats-language-profile:
	$(DOTNET) run --project $(PROJECT) -- stats language-profile $(LANG)

stats-collocations:
	$(DOTNET) run --project $(PROJECT) -- stats collocations $(RUN) "$(WORD)" --window $(WINDOW) --limit $(LIMIT) --min-count $(MIN_COUNT) --min-dice $(MIN_DICE) --db $(DB)

stats-collocations-content:
	$(DOTNET) run --project $(PROJECT) -- stats collocations $(RUN) "$(WORD)" --content-only --window $(WINDOW) --limit $(LIMIT) --min-count $(MIN_COUNT) --min-dice $(MIN_DICE) --db $(DB)

stats-collocations-function:
	$(DOTNET) run --project $(PROJECT) -- stats collocations $(RUN) "$(WORD)" --function-only --window $(WINDOW) --limit $(LIMIT) --min-count $(MIN_COUNT) --min-dice $(MIN_DICE) --db $(DB)

stats-phrases:
	$(DOTNET) run --project $(PROJECT) -- stats phrases $(RUN) --min-n $(MIN_N) --max-n $(MAX_N) --limit $(LIMIT) --min-count $(MIN_COUNT) --min-chapters $(MIN_CHAPTERS) $(LONGEST_ONLY) --db $(DB)

stats-phrases-content-boundary:
	$(DOTNET) run --project $(PROJECT) -- stats phrases $(RUN) --min-n $(MIN_N) --max-n $(MAX_N) --limit $(LIMIT) --min-count $(MIN_COUNT) --min-chapters $(MIN_CHAPTERS) --content-boundary $(LONGEST_ONLY) --db $(DB)

stats-kwic:
	$(DOTNET) run --project $(PROJECT) -- stats kwic $(RUN) "$(WORD)" --limit $(LIMIT) --context $(CONTEXT) --db $(DB)

stats-ngrams:
	$(DOTNET) run --project $(PROJECT) -- stats ngrams $(RUN) --limit $(LIMIT) --db $(DB)

stats-trigrams:
	$(DOTNET) run --project $(PROJECT) -- stats ngrams $(RUN) --n $(N) --limit $(LIMIT) --db $(DB)

stats-next:
	$(DOTNET) run --project $(PROJECT) -- stats next $(RUN) --word "$(WORD)" --limit $(LIMIT) --db $(DB)

stats-categories:
	$(DOTNET) run --project $(PROJECT) -- stats categories $(RUN) --db $(DB)

inspect-run:
	$(DOTNET) run --project $(PROJECT) -- inspect run $(RUN) --out $(DIAGNOSTICS_OUT) --db $(DB)
