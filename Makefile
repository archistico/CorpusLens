# CorpusLens common commands
# Usage examples:
#   make check
#   make corpus-create CORPUS="English Literature" LANG=en
#   make analyze-book BOOK=./books/alice.epub OUT=./artifacts/alice CORPUS="English Literature"
#   make analyze-books BOOKS=./books OUT=./artifacts/books CORPUS="English Literature"
#   make stats-words RUN=1 LIMIT=25
#   make stats-content RUN=1 LIMIT=25
#   make stats-function RUN=1 LIMIT=25
#   make stats-word RUN=1 WORD=alice LIMIT=25
#   make stats-kwic RUN=1 WORD=alice LIMIT=10 CONTEXT=8

DOTNET ?= dotnet
PROJECT ?= src/CorpusLens.Cli
DB ?= ./data/corpuslens.db
CORPUS ?= English Literature
LANG ?= en
BOOK ?= ./books/alice.epub
BOOKS ?= ./books
OUT ?= ./artifacts/analysis
RUN ?= 1
LIMIT ?= 25
WORD ?= alice
N ?= 3
CONTEXT ?= 8

.PHONY: restore build test check demo clean clean-data clean-artifacts corpus-create corpus-list analyze-text analyze-book analyze-books analyze-books-recursive stats-runs stats-summary stats-words stats-content stats-function stats-word stats-kwic stats-ngrams stats-trigrams stats-next stats-categories

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

corpus-create:
	$(DOTNET) run --project $(PROJECT) -- corpus create "$(CORPUS)" --language $(LANG) --db $(DB)

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


stats-runs:
	$(DOTNET) run --project $(PROJECT) -- stats runs --limit $(LIMIT) --db $(DB)

stats-summary:
	$(DOTNET) run --project $(PROJECT) -- stats summary $(RUN) --db $(DB)

stats-words:
	$(DOTNET) run --project $(PROJECT) -- stats words $(RUN) --limit $(LIMIT) --db $(DB)

stats-content:
	$(DOTNET) run --project $(PROJECT) -- stats words $(RUN) --content-only --limit $(LIMIT) --db $(DB)

stats-function:
	$(DOTNET) run --project $(PROJECT) -- stats words $(RUN) --function-only --limit $(LIMIT) --db $(DB)

stats-word:
	$(DOTNET) run --project $(PROJECT) -- stats word $(RUN) $(WORD) --limit $(LIMIT) --db $(DB)

stats-kwic:
	$(DOTNET) run --project $(PROJECT) -- stats kwic $(RUN) $(WORD) --limit $(LIMIT) --context $(CONTEXT) --db $(DB)

stats-ngrams:
	$(DOTNET) run --project $(PROJECT) -- stats ngrams $(RUN) --limit $(LIMIT) --db $(DB)

stats-trigrams:
	$(DOTNET) run --project $(PROJECT) -- stats ngrams $(RUN) --n $(N) --limit $(LIMIT) --db $(DB)

stats-next:
	$(DOTNET) run --project $(PROJECT) -- stats next $(RUN) --word $(WORD) --limit $(LIMIT) --db $(DB)

stats-categories:
	$(DOTNET) run --project $(PROJECT) -- stats categories $(RUN) --db $(DB)
