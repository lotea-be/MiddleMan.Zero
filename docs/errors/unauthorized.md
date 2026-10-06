# Unauthorized (401)

- **HTTP status:** `401 Unauthorized`
- **`type`:** `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/unauthorized.md`
- **`ResultStatus`:** `Unauthorized`

The caller must be authenticated to perform the operation. Use this when the handler itself decides
that the caller is anonymous or their credentials are not acceptable; use `Forbidden` (403) when the
caller is authenticated but lacks permission. `Unauthorized` takes precedence over every other status.

The response carries the standard problem body, populated from any logged `UnauthorizedMessage`s.
It does **not** add a `WWW-Authenticate` header — that is the job of your authentication middleware.

## Example body

```json
{
  "type": "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/unauthorized.md",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Sign in to place an order.",
  "messages": [
    { "message": "Sign in to place an order.", "code": "sign_in_required" }
  ]
}
```

When no message was logged, `detail` defaults to `"Authentication is required."` and `messages` is empty.
