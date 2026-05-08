# ExpenseTracker API

The ExpenseTracker API provides the backend surface for identity, session management, and user management. Other product domains have been moved to legacy code while the layered backend is rebuilt.

**API base path:** `/api`

---

## Overview

ExpenseTracker currently exposes endpoints for:

- Authentication and session lifecycle
- User profiles and admin actions

A lightweight info endpoint is also available at `GET /api`.

---

## Authentication and Sessions

ExpenseTracker uses a **session-based authentication** scheme backed by an HTTP-only cookie.

- Cookie name: `expense_tracker_session_id`
- Set by: `POST /api/auth/login` and `POST /api/auth/refresh`
- Cleared by: `POST /api/auth/logout`
- Cookie flags: `HttpOnly`, `Secure`, `SameSite=None`

When calling protected endpoints from a browser client, make sure credentials are included so cookies are sent.

---

## Standard Response Envelope

All endpoints respond with a consistent envelope:

```json
{
  "success": true,
  "messageId": "string",
  "message": "Human readable summary",
  "details": [
    {
      "message": "string", "severity": "Info"
    }
  ],
  "data": { ... }
}
```

Notes:

- `details` can be `null` or empty for successful responses.
- `messageId` is a stable code useful for client-side handling.
- When an unexpected server error occurs, `messageId` is `COMMON_UNKNOWN_ERROR` and `success` is `false`.

---

## Errors and Traceability

Every response includes an `X-Trace-Id` header. Provide this value when reporting issues so logs can be correlated.

---

## Pagination (Cursor)

Some list endpoints use cursor pagination. The shape is:

```json
{
  "items": [ { ... } ],
  "nextCursor": "uuid-or-null",
  "hasMore": true
}
```

Use `nextCursor` as the `cursor` query parameter in subsequent requests.

---

## OpenAPI

In Development environments:

- OpenAPI JSON: `/openapi/v1.json`
- Interactive docs (Scalar): `/scalar`

---

## Security and Data Handling

Sensitive identifiers are hashed or encrypted at rest. Passwords are stored using strong one-way hashing, while personal identifiers are encrypted using modern cryptography (AES-GCM).
