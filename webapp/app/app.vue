<!--
/**
 * @file app.vue
 * @module WebApp.UI.App
 * @description
 * Root application layout with SSR-compatible authentication state.
 * Uses top-level await to fetch user before render, eliminating auth flash.
 */
-->
<script setup lang="ts">
const { user, fetchUser, login, logout, isAuthenticated } = useAuth()

// Fetch user during SSR - no flash of unauthenticated UI
await fetchUser()
</script>

<template>
  <div>
    <header style="margin-bottom: 1rem; padding: 1rem; border-bottom: 1px solid #eee;">
      <template v-if="isAuthenticated">
        <span>Welcome, {{ user?.name }}</span>
        <button @click="logout()" style="margin-left: 1rem;">Logout</button>
      </template>
      <template v-else>
        <button @click="login()">Login</button>
      </template>
    </header>

    <main style="padding: 1rem;">
      <NuxtPage />
    </main>
  </div>
</template>
