# Forbidden (403)

- **HTTP status:** `403 Forbidden`
- **`type`:** `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/forbidden.md`
- **`ResultStatus`:** `Forbidden`

The caller is authenticated but lacks permission for the requested operation. Unlike earlier
versions (which returned an empty 403), the response now carries the standard problem body, populated
from any logged `ForbiddenMessage`s. Only reasons the handler explicitly logged are surfaced.

## Example body

```json
{
  "type": "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/forbidden.md",
  "title": "Forbidden",
  "status": 403,
  "detail": "You may not cancel another customer's order.",
  "messages": [
    { "message": "You may not cancel another customer's order.", "code": "order_cancel_forbidden" }
  ]
}
```

When no message was logged, `detail` defaults to `"Access denied."` and `messages` is empty.
