# Milestone 18.3.1 — Async UI loading and progress

## Goal

Keep the Avalonia desktop UI responsive while opening a database and loading run dashboards.

## Changes

- Database run loading is executed on a background task.
- Run dashboard loading is executed on background tasks.
- Health and corpus profile are loaded concurrently.
- The status bar shows an indeterminate progress bar while loading.
- Open database, Refresh and run selection are disabled while the UI is busy.
- Existing application query services and CLI behavior are unchanged.

## Notes

This milestone does not add new analysis features. It only changes the desktop orchestration layer so that synchronous database/query work does not block the UI thread.
