# Architecture

## Overview

NewSchool Family is implemented as a **modular monolith** on ASP.NET Core MVC. The product is optimized for fast iteration in a SaaS context while keeping clear internal boundaries between business rules, persistence, web delivery, and UI orchestration.

## Internal layers

### `Domain`

Core entities and enums that represent the business model:

- children
- plans
- guided lessons
- evidence records
- storage plans
- library materials

### `Data`

Persistence composition:

- `ApplicationDbContext`
- seed and initialization routines
- curriculum and content catalogs

### `Services`

Business orchestration and application workflows:

- curriculum sequencing
- daily plan generation
- lesson guidance
- evidence storage rules
- reading progression
- billing orchestration
- outbound email automation

### `Controllers`

HTTP entry points for:

- public pages
- account flow
- parent dashboard
- library
- billing webhooks and checkout

### `Views` and `wwwroot`

The delivery layer for the mobile-first parent experience, including:

- guided study flow
- curriculum views
- evidence center
- public marketing pages

## Why a modular monolith

The current product benefits from a single deployment unit because the curriculum engine, billing, library, reporting, and parent experience share the same runtime context and evolve together.

This keeps:

- operational overhead low
- local development simple
- domain changes fast to ship

At the same time, service and catalog boundaries are explicit enough to support future extraction if needed.

## Configuration strategy

Public configuration files are sanitized.

Real secrets are expected from:

- `appsettings.Local.json`
- `appsettings.{Environment}.Local.json`
- User Secrets
- deployment environment variables

## Deployment model

The application is designed to run behind a standard ASP.NET Core host with:

- SQL Server
- optional Stripe integration
- optional Resend integration
- optional library synchronization
