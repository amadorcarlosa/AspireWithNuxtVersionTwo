# Unauthorized Page

**File:** [webapp/app/pages/unauthorized.vue](webapp/app/pages/unauthorized.vue#L1-L10)

## Overview

This page is a simple, static route used by server-side route middleware to indicate that the current user is authenticated but does not have sufficient permissions to view the requested page. It intentionally avoids auth logic (redirects are handled by middleware).

Key responsibilities:

- Inform the user they are forbidden to view the resource (UI level).
- Offer basic navigational options (back to home / previous page / request access).

## Where It Is Used

- Role-based middleware: [webapp/app/middleware/teacher.ts](webapp/app/middleware/teacher.ts#L1-L40) returns `navigateTo('/unauthorized')` when the current user lacks the expected `Teacher` role.
- Authentication middleware uses `login()` to redirect unauthenticated users back to the login flow; `unauthorized.vue` is only used when the user is authenticated but unauthorized.

## Current Page (Minimal Template)

Source: [webapp/app/pages/unauthorized.vue](webapp/app/pages/unauthorized.vue#L1-L10)

```vue
<template>
  <div>
    <h1>Unauthorized</h1>
    <p>You don't have permission to access this page.</p>
    <NuxtLink to="/">Return Home</NuxtLink>
  </div>
</template>
```

## Why This Exists

- Middleware such as `teacher.ts` needs a cheap UI to show when role checks fail.
- Keeping this page static keeps it simple and avoids re-running auth probes or SSR logic when users are known to be authenticated but simply lack a role.

## Suggested Improvements

1. Make UX friendlier: add a `Back` button that navigates to the previous page instead of always sending users to the home page.

   - Small example (optional):

     ```vue
     <script setup>
     const router = useRouter();
     const goBack = () => router.back();
     </script>

     <template>
       <div>
         <h1>Unauthorized</h1>
         <p>You don't have permission to access this page.</p>
         <v-btn variant="outlined" @click="goBack">Back</v-btn>
         <NuxtLink to="/">Return Home</NuxtLink>
       </div>
     </template>
     ```

2. Offer a contextual action for authenticated users: "Request Access" (calls an API to request elevated role/permission).

   - This reduces friction when a user lacks a role that can be approved.

3. Surface the user's identity or roles when helpful (e.g., "You are signed in as me@example.com, and this action requires the Teacher role."). Use `useAuth()` to pull that state.

4. Consider HTTP semantics (optional): If you need to return 403 on the HTTP response (for automated clients or monitoring), consider throwing a 403 error from middleware instead of redirecting to the `unauthorized` route:
   - For example: `throw createError({ statusCode: 403, statusMessage: 'Forbidden' })` from middleware (server/SSR contexts only). This preserves the HTTP status code for the response, while still giving a UI fallback if necessary.

## Accessibility & Localization

- Add `aria` attributes and a translation strategy if the site supports multiple languages (e.g., `useI18n`).

Example using `useAuth` to show a helpful message when an authenticated user lacks the role:

```vue
<script setup>
const { user, isAuthenticated } = useAuth();
const router = useRouter();
</script>

<template>
  <div>
    <h1>{{ $t('pages.unauthorized.title') }}</h1>
    <p v-if="isAuthenticated">{{ $t('pages.unauthorized.signedInAs', { user: user?.name }) }}</p>
    <p v-else>{{ $t('pages.unauthorized.pleaseLogin') }}</p>
    <v-btn @click="() => router.back()">{{ $t('actions.goBack') }}</v-btn>
    <NuxtLink to="/">{{ $t('actions.home') }}</NuxtLink>
  </div>
</template>
```

## Tests / Checklist

- When protected by role middleware, `navigateTo('/unauthorized')` should show this page and not redirect back into a login flow.
- If you want the response to be HTTP 403 for programmatic clients, update the middleware to return/throw the error code.
- Make sure the page remains accessible and included in routes so meaningful URLs can be bookmarked or reported.

## Related Files

- Page: [webapp/app/pages/unauthorized.vue](webapp/app/pages/unauthorized.vue#L1-L10)
- Role middleware example: [webapp/app/middleware/teacher.ts](webapp/app/middleware/teacher.ts#L1-L40)
- Authentication middleware: [webapp/app/middleware/auth.ts](webapp/app/middleware/auth.ts#L1-L60)
- Auth composable for user state: [webapp/app/composables/useAuth.ts](webapp/app/composables/useAuth.ts#L1-L200)

---

Last updated: 2025-12-13
