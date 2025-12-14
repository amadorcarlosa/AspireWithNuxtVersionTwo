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
import type { Ref } from 'vue'

interface WeatherForecast {
  date: string;
  temperatureC: number;
  temperatureF: number;
  summary: string;
}

// Fetch data using your secure composable
const { data: forecasts, error, status } = await useAuthFetch<WeatherForecast[]>('/api/weatherforecast')

// Helper for weather icons
const getWeatherIcon = (summary: string) => {
  const s = summary.toLowerCase()
  if (s.includes('rain') || s.includes('drizzle')) return 'mdi-weather-rainy'
  if (s.includes('cloud')) return 'mdi-weather-cloudy'
  if (s.includes('snow')) return 'mdi-weather-snowy'
  if (s.includes('sun') || s.includes('clear')) return 'mdi-weather-sunny'
  return 'mdi-weather-partly-cloudy'
}

// Helper for temperature color
const getTempColor = (tempC: number) => {
  if (tempC > 30) return 'red-darken-1'
  if (tempC > 20) return 'orange-darken-1'
  if (tempC > 10) return 'green-darken-1'
  return 'blue-darken-1'
}
</script>

<template>
  <v-card border elevation="0">
    <div v-if="status === 'pending'" class="pa-4">
      <div class="text-center mb-4">Establishing secure connection...</div>
      <v-progress-linear indeterminate color="primary" />
    </div>

    <v-alert
      v-else-if="error"
      type="error"
      title="Access Denied / API Error"
      variant="tonal"
      class="ma-4"
    >
      {{ error.message }}
      <div class="mt-2 text-caption">
        Debug: Check if your .NET backend is running on port 5105.
      </div>
    </v-alert>

    <v-table v-else hover>
      <thead>
        <tr>
          <th class="text-left">Date</th>
          <th class="text-left">Condition</th>
          <th class="text-right">Temp (°C)</th>
          <th class="text-right">Temp (°F)</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="item in forecasts" :key="item.date">
          <td>
            {{ new Date(item.date).toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric' }) }}
          </td>
          
          <td>
            <v-chip size="small" variant="tonal" class="text-capitalize">
              <v-icon start :icon="getWeatherIcon(item.summary)" />
              {{ item.summary }}
            </v-chip>
          </td>

          <td class="text-right font-weight-bold" :class="`text-${getTempColor(item.temperatureC)}`">
            {{ item.temperatureC }}°
          </td>

          <td class="text-right text-grey">
            {{ item.temperatureF }}°
          </td>
        </tr>
      </tbody>
    </v-table>
  </v-card>
</template>