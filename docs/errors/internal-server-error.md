# Internal Server Error (500)

- **HTTP status:** `500 Internal Server Error`
- **`type`:** `https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/internal-server-error.md`
- **`ResultStatus`:** `Failure` (and any unmapped / `Undefined` status)

An unexpected error occurred while processing the request. This is the fallback for `Failure` as well
as for any `ResultStatus` value not explicitly mapped (including `Undefined`). Any logged
`FailureMessage`s appear in the `messages` array; avoid leaking sensitive internal detail into them.

## Example body

```json
{
  "type": "https://raw.githubusercontent.com/lotea-be/MiddleMan.Zero/main/docs/errors/internal-server-error.md",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred.",
  "messages": []
}
```

When no message was logged, `detail` defaults to `"An unexpected error occurred."`.
