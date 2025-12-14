<script setup lang="ts">
import { ref, computed } from 'vue'

// 1. Get Real Auth State
const { user, isAuthenticated, login } = useAuth()

// 2. Define your Experiments (The Menu)
// As you build new pages, just add them to this array.
const experiments = [
  {
    title: 'Secure Data Channel',
    subtitle: 'BFF Pattern & OIDC',
    icon: 'mdi-shield-lock',
    color: 'blue',
    to: '/lab/secure-data',
    description: 'Test the Backend-for-Frontend proxy. Verifies that HttpOnly cookies are correctly passing to the .NET API without exposing tokens to the browser.',
    status: 'Active'
  },
  {
    title: 'Semantic Kernel Chat',
    subtitle: 'RAG & AI Agents',
    icon: 'mdi-robot-outline',
    color: 'purple',
    to: '/lab/chat',
    description: 'Interactive chat interface using Microsoft Semantic Kernel. Tests streaming responses and context maintenance via the MCP server.',
    status: 'Coming Soon' // Placeholder until you build it
  },
  {
    title: 'Real-Time Event Bus',
    subtitle: 'SignalR & Azure Service Bus',
    icon: 'mdi-flash',
    color: 'amber',
    to: '/lab/realtime',
    description: 'Live visualization of system events. Connects to the Azure Service Bus via SignalR to show infrastructure heartbeat in real-time.',
    status: 'Planned'
  }
]

// 3. Simple System Health Mock (You can hook this to real API later)
const systemStatus = ref([
  { name: 'Identity Provider', status: 'Online', color: 'success' },
  { name: '.NET Aspire Proxy', status: 'Online', color: 'success' },
  { name: 'AI Service', status: 'Standby', color: 'warning' },
])
</script>

<template>
  <v-container class="py-8">
    
    <v-row class="mb-8">
      <v-col cols="12">
        <div class="d-flex align-center mb-2">
          <v-icon icon="mdi-flask" color="primary" size="x-large" class="mr-3" />
          <h1 class="text-h3 font-weight-black">The Lab</h1>
        </div>
        <p class="text-h6 font-weight-light text-medium-emphasis" style="max-width: 800px;">
          Active research protocols and prototypes. This environment interacts directly with the 
          <strong>.NET Aspire</strong> backend. Expect volatility.
        </p>
      </v-col>
    </v-row>

    <v-row class="mb-12">
      <v-col cols="12">
        <v-card border elevation="0" class="bg-grey-lighten-5">
          <v-card-title class="text-caption font-weight-bold text-uppercase text-grey-darken-1">
            System Telemetry
          </v-card-title>
          
          <v-divider />

          <v-row no-gutters>
            <v-col cols="12" md="4" class="pa-4 border-e">
              <div class="d-flex align-center mb-2">
                <v-icon :icon="isAuthenticated ? 'mdi-account-check' : 'mdi-account-off'" 
                        :color="isAuthenticated ? 'success' : 'error'" 
                        class="mr-2" />
                <span class="font-weight-bold">Authentication</span>
              </div>
              <div v-if="isAuthenticated" class="text-body-2">
                User: <span class="font-weight-medium">{{ user?.name }}</span><br>
                <span class="text-caption text-grey">Session Secure (BFF)</span>
              </div>
              <div v-else class="text-body-2 text-grey">
                Guest Access. <a href="#" @click.prevent="login()" class="text-decoration-none font-weight-bold">Log in</a> to run secured experiments.
              </div>
            </v-col>

            <v-col cols="12" md="8" class="pa-4">
              <div class="d-flex flex-wrap gap-4">
                <v-chip 
                  v-for="service in systemStatus" 
                  :key="service.name"
                  :color="service.color"
                  variant="flat"
                  size="small"
                  label
                >
                  <v-icon start icon="mdi-circle-small" />
                  {{ service.name }}: {{ service.status }}
                </v-chip>
              </div>
              <div class="mt-2 text-caption text-grey">
                Connected via YARP Reverse Proxy (localhost:5105)
              </div>
            </v-col>
          </v-row>
        </v-card>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" class="mb-4">
        <h2 class="text-h5 font-weight-bold">Active Protocols</h2>
      </v-col>

      <v-col 
        v-for="exp in experiments" 
        :key="exp.title" 
        cols="12" 
        md="4"
      >
        <v-card 
          height="100%" 
          hover 
          border
          :to="exp.status === 'Active' ? exp.to : undefined"
          :class="{ 'opacity-60': exp.status !== 'Active' }"
        >
          <v-card-item>
            <template v-slot:prepend>
              <v-avatar :color="exp.color" variant="tonal" rounded>
                <v-icon :icon="exp.icon" />
              </v-avatar>
            </template>
            <v-card-title>{{ exp.title }}</v-card-title>
            <v-card-subtitle>{{ exp.subtitle }}</v-card-subtitle>
            <template v-slot:append>
              <v-chip size="x-small" :color="exp.status === 'Active' ? 'success' : 'grey'">
                {{ exp.status }}
              </v-chip>
            </template>
          </v-card-item>

          <v-card-text>
            {{ exp.description }}
          </v-card-text>
          
          <v-card-actions v-if="exp.status === 'Active'">
            <v-btn variant="text" color="primary" :to="exp.to">
              Run Protocol
              <v-icon end icon="mdi-arrow-right" />
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>

  </v-container>
</template>

<style scoped>
.opacity-60 {
  opacity: 0.7;
}
</style>