<!--
/**
 * @file WeatherForecasts.vue
 * @module Components.Weather
 * @description
 * Weather forecast display component that fetches and renders forecast data from the backend API.
 * 
 * Responsibilities:
 * - Fetches weather forecast data via server-side proxy to backend API
 * - Handles loading, error, and success states
 * - Renders forecast data in a tabular format with temperature in both Celsius and Fahrenheit
 * 
 * Integration:
 * Uses `useAuthFetch` (wrapping Nuxt's useFetch) for SSR-compatible data fetching and automatic 401 handling
 * - Requests are proxied through /api/weatherforecast to the backend via server middleware
 * - Backend API URL is resolved at runtime via Aspire service discovery
 */
-->
<script setup lang="ts">
/**
 * Weather forecast data structure from the backend API.
 * Represents a single day's weather prediction.
 */
interface WeatherForecast {
  /** ISO 8601 date string for the forecast day */
  date: string;
  /** Temperature in Celsius */
  temperatureC: number;
  /** Temperature in Fahrenheit */
  temperatureF: number;
  /** Weather condition summary (e.g., "Sunny", "Rainy") */
  summary: string;
}

import type { Ref } from 'vue'

/**
 * Fetches weather forecast data from the backend API.
 * Uses Nuxt's useFetch for SSR/CSR compatibility.
 * The request is proxied through server middleware to the backend API.
 */
const { data: weatherForecasts, error, status } = await useAuthFetch<WeatherForecast[]>('/api/weatherforecast') as {
  data: Ref<WeatherForecast[] | null>
  error: Ref<Error | null>
  status: Ref<string>
};

console.log('=== WEATHER COMPONENT DEBUG ===');
console.log('Status:', status.value);
console.log('Data:', weatherForecasts.value);
console.log('Error:', error.value);
console.log('===============================');
</script>

<template>
  <div>
    <div v-if="error" style="color: red;">
      Error: {{ error.message }}
    </div>
    <div v-else-if="status === 'pending'">
      Loading...
    </div>
    <div v-else-if="weatherForecasts">
      <table>
        <thead>
          <tr>
            <th>Date</th>
            <th>Summary</th>
            <th>T (°C)</th>
            <th>T (°F)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in weatherForecasts" :key="item.date">
            <td>{{ item.date }}</td>
            <td>{{ item.summary }}</td>
            <td>{{ item.temperatureC }}</td>
            <td>{{ item.temperatureF }}</td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-else>
      No data
    </div>
  </div>
</template>
