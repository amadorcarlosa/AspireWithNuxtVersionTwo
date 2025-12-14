<script setup lang="ts">
definePageMeta({
  middleware: 'auth' // <--- This protects the route
})
</script>

<template>
  <v-container>
    <div class="mb-6">
      <h1 class="text-h4 font-weight-bold">Secure Data Channel</h1>
      <p class="text-body-1 text-medium-emphasis">
        This page fetches protected data from the .NET API. 
        If you see data below, your <strong>HttpOnly Cookie</strong> was successfully passed by the Nuxt BFF proxy.
      </p>
    </div>

    <WeatherForecasts />
  </v-container>
</template>