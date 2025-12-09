<script setup lang="ts">
import { useAuth } from './composables/useAuth';

const { login, logout, getUser } = useAuth();
const user = ref<any>(null);

onMounted(async () => {
  user.value = await getUser();
});
</script>

<template>
  <div>
    <div style="margin-bottom: 1rem;">
     <template v-if="user?.name">
        <span>Welcome, {{ user.name }}</span>
        <button @click="logout()">Logout</button>
      </template>
      <template v-else>
        <button @click="login()">Login</button>
      </template>
    </div>

    <div>
      Testing Weather Forecasts Component:
      <WeatherForecasts />
    </div>
  </div>
</template>
