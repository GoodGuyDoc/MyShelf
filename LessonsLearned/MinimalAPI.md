# Lessons Learned — .NET API (Minimal)

Finished the basic implementation of the minimal API with a focus on security and getting a better feel for how .NET API applications are structured. A few things that stood out:

1. **DTOs are really useful for patches and updates** — Using data classes to represent partial updates keeps things clean and makes it easy to control exactly what gets modified without exposing the whole object.
2. **Async lambdas are great for keeping things concise** — I hadn't leaned on them much before, but they're a natural fit for quick endpoint actions without a lot of boilerplate.
3. **REST structure clicks differently from the server side** — The general pattern came naturally, but seeing it from the API end rather than consuming it as a client was a genuinely interesting shift in perspective.