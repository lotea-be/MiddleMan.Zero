# Conflict (409)

- **HTTP status:** `409 Conflict`
- **`type`:** `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/conflict.md`
- **`ResultStatus`:** `Conflict`

The request could not be completed because it conflicts with the current state of the resource
(for example, a duplicate or a concurrent modification). Any logged `ConflictMessage`s appear in the
`messages` array.

## Example body

```json
{
  "type": "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/conflict.md",
  "title": "Conflict",
  "status": 409,
  "detail": "An order with this reference already exists.",
  "messages": [
    { "message": "An order with this reference already exists.", "code": "order_already_exists" }
  ]
}
```

When no message was logged, `detail` defaults to `"The request conflicts with the current state."`.
