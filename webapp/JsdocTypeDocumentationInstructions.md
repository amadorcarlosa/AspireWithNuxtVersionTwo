# JSDoc + TypeScript Documentation Guide

This guide defines a clean, consistent standard for documenting TypeScript projects using JSDoc. The goal is to make your documentation:

• Precise for machines
• Clear for humans
• Stable during refactors
• Useful inside editors and generated docs

---

## 1. File-Level Documentation (Required)

Every exported file should begin with a file header. This defines identity, responsibility, and boundary.

```ts
/**
 * @file UserRepository.ts
 * @module Domain.Users.Persistence
 * @description
 * Persistence boundary for the User aggregate.
 * Converts between domain models and storage representations.
 */
```

### Rules

* `@file` must match the filename exactly
* `@module` must express **conceptual location**, not directory path
* `@description` must describe **responsibility, not implementation**

---

## 2. Module Documentation (Recommended)

Use when the file represents a public or architectural boundary.

```ts
/**
 * @module Domain.Users.Persistence
 * Handles persistence rules for User aggregates.
 * No business logic is permitted in this module.
 */
```

---

## 3. Function Documentation (Required for Public APIs)

```ts
/**
 * Persists a user record to storage.
 * @param user The domain user model
 * @returns A Result indicating success or failure
 */
export function saveUser(user: User): Promise<Result<void, SaveError>>
```

### Rules

* The summary line must be a **verb phrase**
* Do not restate types already expressed in TypeScript
* Use JSDoc to explain **intent and semantic meaning**

---

## 4. Parameter Documentation

```ts
/**
 * @param user The validated domain user
 * @param options Optional persistence configuration
 */
```

Only document parameters when:

* The meaning is non-obvious
* The parameter carries domain semantics

---

## 5. Return Value Documentation

```ts
/**
 * @returns A Result indicating success or failure
 */
```

Do not restate raw types. Describe **semantic meaning**, not the shape.

---

## 6. Error & Failure Documentation

```ts
/**
 * @throws ValidationError If the user fails domain validation
 * @throws StorageError If persistence fails
 */
```

Use when working with exceptions or explicit error channels.

---

## 7. Generic Type Documentation

```ts
/**
 * @template T The input data type
 */
function identity<T>(value: T): T
```

Always document generics when they encode rules or constraints.

---

## 8. Type & Interface Documentation

```ts
/**
 * Represents a persisted user record.
 */
export interface UserRecord {
  id: string
  email: string
}
```

Document **purpose**, not field types.

---

## 9. Class Documentation

```ts
/**
 * Repository responsible for user persistence.
 */
export class UserRepository {}
```

---

## 10. What NOT to Document

Do NOT document:

* Obvious types
* Trivial getters/setters
* Redundant descriptions
* Internal helper functions unless they encode domain logic

---

## 11. Documentation Quality Rules

Good documentation:

* Explains *why*, not just *what*
* Matches the system’s domain language
* Survives refactors
* Improves IDE hover intelligence
* Can be safely compiled into public docs

Bad documentation:

* Repeats types
* Describes syntax
* Becomes outdated after refactors
* Explains implementation instead of intent

---

## 12. Tooling Compatibility

This standard is compatible with:

* TypeDoc
* tsdoc
* Docusaurus
* VS Code IntelliSense
* AI code indexing systems

---

## 13. Documentation Is a Contract

JSDoc is not decoration. Combined with TypeScript, it forms a two-part contract:

• Types enforce behavior
• JSDoc communicates intent

When they disagree, the system is lying.

---

End of document.
