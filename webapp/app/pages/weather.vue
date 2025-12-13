<!--
/**
 * @file weather.vue
 * @description Protected weather page - requires authentication
 */
-->
<script setup lang="ts">
definePageMeta({
    middleware: 'auth'
})
</script>

<template>
    <div>
        <h1>Weather Forecast</h1>
        <WeatherForecasts />
    </div>
</template>
